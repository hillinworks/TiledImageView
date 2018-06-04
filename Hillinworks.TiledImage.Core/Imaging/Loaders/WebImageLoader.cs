using System;
using System.Collections.Generic;

namespace Hillinworks.TiledImage.Imaging.Loaders
{
	public abstract partial class WebImageLoader : IImageLoader
	{
		/// <summary>
		///     Stores all the active and completed download tasks, also acts as a download cache.
		/// </summary>
		private Dictionary<TileIndex.Full, DownloadImageTask> DownloadTasks { get; }
			= new Dictionary<TileIndex.Full, DownloadImageTask>();

		protected internal virtual bool AllowLocalCache { get; } = true;
		protected internal virtual string LocalCachePath { get; } = "WebImageCache";

		public abstract Dimensions Dimensions { get; }
		public abstract LODInfo LOD { get; }

		public virtual void BeginLoadTileAsync(ILoadTileTask task)
		{
			if (!this.DownloadTasks.TryGetValue(task.Index, out var downloadTask)
			    || !downloadTask.Status.IsAlive())
			{
				var url = this.GetTileAddress(task.Index);

				downloadTask = new DownloadImageTask(this, task.Index, url);
				lock (this.DownloadTasks)
				{
					this.DownloadTasks.Add(task.Index, downloadTask);
				}
			}

			downloadTask.HandleObserver(task);
		}

		void IDisposable.Dispose()
		{
		}

		void IImageLoader.Initialize()
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