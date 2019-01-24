using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Hillinworks.TiledImage.Imaging.Sources
{
    public abstract partial class WebImageSource : IImageSource
    {
        /// <summary>
        ///     Stores all the active and completed download tasks, also acts as a download cache.
        /// </summary>
        private Dictionary<TileIndex.Full, DownloadImageTask> DownloadTasks { get; }
            = new Dictionary<TileIndex.Full, DownloadImageTask>();

        protected virtual bool AllowLocalCache { get; } = true;
        protected virtual string LocalCachePath { get; } = "WebImageCache";

        public abstract Dimensions Dimensions { get; }
        public abstract LODInfo LOD { get; }


        public Task<BitmapSource> LoadTileAsync(TileIndex.Full index, IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (!this.DownloadTasks.TryGetValue(index, out var downloadTask)
                || !downloadTask.Status.IsAlive())
            {
                var url = this.GetTileAddress(index);

                downloadTask = new DownloadImageTask(this, index, url);
                lock (this.DownloadTasks)
                {
                    this.DownloadTasks.Add(index, downloadTask);
                }
            }

            downloadTask.HandleObserver(progress, cancellationToken);

            return downloadTask.CompletionSource.Task;
        }

        public Task<BitmapSource> LoadNamedImageAsync(
            string name,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult((BitmapSource)null);
        }

        public Task<BitmapSource> CreateThumbnailAsync(double width, double height)
        {
            throw new NotImplementedException();
        }


        void IDisposable.Dispose()
        {
        }


        protected abstract string GetTileAddress(TileIndex.Full tileIndex);

        private void RemoveDownloadTask(DownloadImageTask downloadTask)
        {
            lock (this.DownloadTasks)
            {
                this.DownloadTasks.Remove(downloadTask.TileIndex);
            }
        }
    }
}