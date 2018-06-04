using System;
using Hillinworks.TiledImage.Imaging.Loaders;

namespace Hillinworks.TiledImage.Imaging
{
	public class TiledImage : IDisposable
	{
		public TiledImage(IImageLoader imageLoader)
		{
			this.ImageLoader = imageLoader;
			this.ImageLoader.Initialize();
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