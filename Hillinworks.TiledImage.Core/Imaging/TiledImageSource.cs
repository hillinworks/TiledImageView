using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hillinworks.TiledImage.Imaging.Loaders;

namespace Hillinworks.TiledImage.Imaging
{
	public class TiledImageSource : IDisposable
	{
		public TiledImageSource(IImageLoader imageLoader)
		{
			this.ImageLoader = imageLoader;
			this.ImageLoader.Initialize();
			this.ImageLoader.LOD.Validate();
			this.ImageLoader.Dimensions.Validate();
		}

		public Dimensions Dimensions => this.ImageLoader.Dimensions;
		public LODInfo LOD => this.ImageLoader.LOD;
		internal IImageLoader ImageLoader { get; }
		private bool IsDisposed { get; set; }

		public void Dispose()
		{
			this.Dispose(true);
		}

		public void BeginLoadTileAsync(ILoadTileTask task)
		{
			this.ImageLoader.BeginLoadTileAsync(task);
		}

		public Task<BitmapSource> LoadTileAsync(TileIndex.Full index)
		{
			var task = new LoadTileTask(index);
			var completionSource = new TaskCompletionSource<BitmapSource>();
			task.LoadStateChanged += (o, e) =>
			{
				switch (task.Status)
				{
					case LoadTileStatus.Loading:
						break;
					case LoadTileStatus.Succeed:
						completionSource.SetResult(task.Bitmap);
						break;
					case LoadTileStatus.Failed:
						completionSource.SetException(new LoadTileFailedException(task.ErrorMessage));
						break;
					case LoadTileStatus.Canceled:
						completionSource.SetCanceled();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};
			this.BeginLoadTileAsync(task);
			return completionSource.Task;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.IsDisposed)
			{
				if (disposing)
				{
					this.ImageLoader.Dispose();
				}

				this.IsDisposed = true;
			}
		}
	}
}