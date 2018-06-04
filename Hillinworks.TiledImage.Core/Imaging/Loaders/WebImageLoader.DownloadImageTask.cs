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
using Hillinworks.TiledImage.Utilities;

namespace Hillinworks.TiledImage.Imaging.Loaders
{
	partial class WebImageLoader
	{
		/// <summary>
		///     Manages a single web image download task.
		///     If the task is in progress, ILoadTileTasks can observe this task. The observers
		///     will be notified about the download progress and result.
		///     If all the observers have cancelled their load tasks, the download task will be
		///     cancelled.
		///     If the download task is already done, an observer will be fed with the downloaded
		///     image immediately.
		/// </summary>
		private class DownloadImageTask : IDisposable
		{
			public DownloadImageTask(WebImageLoader owner, TileIndex.Full index, string url)
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

			private List<ILoadTileTask> Observers { get; }
				= new List<ILoadTileTask>();

			private Dictionary<CancellationToken, CancellationTokenRegistration> CancellationTokenRegistrations { get; }
				= new Dictionary<CancellationToken, CancellationTokenRegistration>();

			private object ObserverSyncRoot { get; } = new object();

			private WebImageLoader Owner { get; }
			private WebClient WebClient { get; }
			public TileIndex.Full TileIndex { get; }
			public string Url { get; }
			public BitmapSource Bitmap { get; private set; }
			public LoadTileStatus Status { get; private set; }

			public double LoadProgress { get; private set; }
			public string ErrorMessage { get; private set; }

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
				return Path.Combine(this.Owner.LocalCachePath, hash.Substring(0, 2), hash.Substring(2, 2), hashBuilder + ".png");
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
					this.Bitmap = bitmap;

					foreach (var task in this.Observers)
					{
						task.OnCompleted(bitmap);
					}

					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}

			private void SaveCache()
			{
				if (!this.Owner.AllowLocalCache)
				{
					return;
				}

				Debug.Assert(this.Status == LoadTileStatus.Succeed);
				Debug.Assert(this.Bitmap != null);

				var path = this.GetCachePath();
				var directory = Path.GetDirectoryName(path);
				Debug.Assert(directory != null);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				this.Bitmap.SaveAsPng(path);
			}

			private void StartDownloading()
			{
				this.WebClient.DownloadProgressChanged += this.WebClient_DownloadProgressChanged;
				this.WebClient.DownloadDataCompleted += this.WebClient_DownloadDataCompleted;
				this.WebClient.DownloadDataAsync(new Uri(this.Url));
			}

			private void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
			{
				Debug.Assert(this.Status == LoadTileStatus.Loading);

				ILoadTileTask[] observers;
				lock (this.ObserverSyncRoot)
				{
					observers = this.Observers.ToArray();
					foreach (var registration in this.CancellationTokenRegistrations.Values)
					{
						registration.Dispose();
					}

					this.CancellationTokenRegistrations.Clear();
					this.Observers.Clear();
				}

				if (e.Cancelled)
				{
					this.Owner.RemoveDownloadTask(this);
					this.Status = LoadTileStatus.Canceled;

					foreach (var task in observers)
					{
						task.OnCanceled();
					}
				}
				else if (e.Error != null)
				{
					this.Owner.RemoveDownloadTask(this);
					this.ErrorMessage = e.Error.Message;
					this.Status = LoadTileStatus.Failed;

					foreach (var task in observers)
					{
						task.OnError(this.ErrorMessage);
					}
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

					this.Bitmap = bitmap;
					this.SaveCache();

					foreach (var task in observers)
					{
						task.OnCompleted(bitmap);
					}
				}
			}


			private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
			{
				if (!this.Status.IsAlive())
				{
					return;
				}

				ILoadTileTask[] observers;
				lock (this.ObserverSyncRoot)
				{
					observers = this.Observers.ToArray();
				}

				this.LoadProgress = e.ProgressPercentage / 100.0;
				foreach (var task in observers)
				{
					task.ReportProgress(this.LoadProgress);
				}
			}

			public void HandleObserver(ILoadTileTask task)
			{
				if (task.CancellationToken.IsCancellationRequested)
				{
					return;
				}

				switch (this.Status)
				{
					case LoadTileStatus.Loading:

						lock (this.ObserverSyncRoot)
						{
							this.Observers.Add(task);

							this.CancellationTokenRegistrations.Add(
								task.CancellationToken,
								task.CancellationToken.Register(this.OnObserverCancelled, task.CancellationToken));
						}

						task.ReportProgress(this.LoadProgress);
						break;

					case LoadTileStatus.Succeed:

						task.OnCompleted(this.Bitmap);
						break;

					default:
						throw new InvalidOperationException("Invalid state for DownloadImageTask");
				}
			}

			private void OnObserverCancelled(object userState)
			{
				var cancellationToken = (CancellationToken) userState;
				lock (this.ObserverSyncRoot)
				{
					this.CancellationTokenRegistrations[cancellationToken].Dispose();
					this.CancellationTokenRegistrations.Remove(cancellationToken);

					if (this.Observers.All(t => t.CancellationToken.IsCancellationRequested))
					{
						// WebClient.CancelAsync can be expensive. Run it asynchronizely.
						Task.Run((Action) this.WebClient.CancelAsync);

						foreach (var registration in this.CancellationTokenRegistrations.Values)
						{
							registration.Dispose();
						}

						this.CancellationTokenRegistrations.Clear();
					}
				}
			}
		}
	}
}