using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hillinworks.TiledImage.Imaging.Sources
{
    partial class WebImageSource
    {
        /// <inheritdoc />
        /// <summary>
        ///     Manages a single web image download task.
        ///     If the task is in progress, ILoadTileTasks can observe this task. The observers
        ///     will be notified about the download progress and result.
        ///     If all the observers have cancelled their load tasks, the download task will be
        ///     cancelled.
        ///     If the download task is already done, an observer will be fed with the downloaded
        ///     image immediately.
        /// </summary>
        private partial class DownloadImageTask : IDisposable
        {
            public DownloadImageTask(WebImageSource owner, TileIndex.Full index, string url)
            {
                this.Owner = owner;
                this.TileIndex = index;
                this.Url = url;

                this.WebClient = new WebClient();

                Task.Run(() =>
                {
                    if (!this.TryLoadCache())
                    {
                        this.StartDownloading();
                    }
                });
            }

            public TaskCompletionSource<BitmapSource> CompletionSource { get; }
                = new TaskCompletionSource<BitmapSource>();

            private HashSet<Observer> Observers { get; }
                = new HashSet<Observer>();

            private WebImageSource Owner { get; }
            private WebClient WebClient { get; }
            public TileIndex.Full TileIndex { get; }
            private string Url { get; }
            public LoadTileStatus Status { get; private set; }
            private double LoadProgress { get; set; }

            public void Dispose()
            {
                this.WebClient.Dispose();
                GC.SuppressFinalize(this);
            }

            private string GetCachePath()
            {
                var crypt = new SHA256Managed();
                var hashBuilder = new StringBuilder();
                var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(this.Url));
                foreach (var @byte in crypto)
                {
                    hashBuilder.Append(@byte.ToString("x2"));
                }

                var hash = hashBuilder.ToString();

                // use a git-like path
                return Path.Combine(this.Owner.LocalCachePath, hash.Substring(0, 2), hash.Substring(2, 2),
                    hashBuilder + ".png");
            }

            private bool TryLoadCache()
            {
                if (!this.Owner.AllowLocalCache)
                {
                    return false;
                }

                var path = this.GetCachePath();

                if (!File.Exists(path))
                {
                    return false;
                }

                try
                {
                    var bitmap = new BitmapImage();

                    var directory = Path.GetDirectoryName(path);

                    Debug.Assert(directory != null);

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (var file = File.OpenRead(path))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = file;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    this.Status = LoadTileStatus.Succeed;

                    this.CompletionSource.SetResult(bitmap);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            private void SaveCache(BitmapImage bitmap)
            {
                if (!this.Owner.AllowLocalCache)
                {
                    return;
                }

                Debug.Assert(this.Status == LoadTileStatus.Succeed);
                Debug.Assert(bitmap != null);

                var path = this.GetCachePath();
                var directory = Path.GetDirectoryName(path);
                Debug.Assert(directory != null);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                bitmap.SaveAsPng(path);
            }

            private void StartDownloading()
            {
                this.WebClient.DownloadProgressChanged += this.WebClient_DownloadProgressChanged;
                this.WebClient.DownloadDataCompleted += this.WebClient_DownloadDataCompleted;
                this.WebClient.DownloadDataAsync(new Uri(this.Url));
            }

            private void ClearObservers()
            {
                foreach (var observer in this.Observers)
                {
                    observer.Dispose();
                }

                this.Observers.Clear();
            }

            private void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
            {
                Debug.Assert(this.Status == LoadTileStatus.Loading);

                lock (this.Observers)
                {
                    this.ClearObservers();
                }

                if (e.Cancelled)
                {
                    this.Owner.RemoveDownloadTask(this);
                    this.Status = LoadTileStatus.Canceled;

                    this.CompletionSource.SetCanceled();
                }
                else if (e.Error != null)
                {
                    this.Owner.RemoveDownloadTask(this);
                    this.Status = LoadTileStatus.Failed;

                    this.CompletionSource.SetException(e.Error);
                }
                else
                {
                    this.Status = LoadTileStatus.Succeed;

                    var buffer = e.Result;

                    var bitmap = new BitmapImage();

                    using (var stream = new MemoryStream(buffer))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    this.SaveCache(bitmap);

                    this.CompletionSource.SetResult(bitmap);
                }
            }


            private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                if (!this.Status.IsAlive())
                {
                    return;
                }

                Observer[] observers;

                lock (this.Observers)
                {
                    observers = this.Observers.ToArray();
                }

                this.LoadProgress = e.ProgressPercentage / 100.0;
                foreach (var observer in observers)
                {
                    observer.Progress.Report(this.LoadProgress);
                }
            }

            public void HandleObserver(IProgress<double> progress, CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                switch (this.Status)
                {
                    case LoadTileStatus.Loading:

                        var observer = new Observer(progress, cancellationToken);
                        observer.CancellationTokenRegistration =
                            cancellationToken.Register(this.OnObserverCancelled, observer);

                        lock (this.Observers)
                        {
                            this.Observers.Add(observer);
                        }

                        progress.Report(this.LoadProgress);
                        break;

                    case LoadTileStatus.Succeed:

                        break;

                    default:
                        throw new InvalidOperationException("Invalid state for DownloadImageTask");
                }
            }

            private void OnObserverCancelled(object userState)
            {
                var cancelledObserver = (Observer) userState;
                lock (this.Observers)
                {
                    cancelledObserver.Dispose();
                    this.Observers.Remove(cancelledObserver);

                    if (this.Observers.All(t => t.CancellationToken.IsCancellationRequested))
                    {
                        // WebClient.CancelAsync can be expensive. Run it asynchronously.
                        Task.Run(this.WebClient.CancelAsync);

                        this.ClearObservers();
                    }
                }
            }
        }
    }
}