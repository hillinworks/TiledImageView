using System;

namespace Hillinworks.TiledImage.Imaging.Loaders
{
	public interface IImageLoader : IDisposable
	{
		/// <summary>
		///     Get the dimensions of this image.
		/// </summary>
		Dimensions Dimensions { get; }

		/// <summary>
		///     Get the Level of Detail information of this image.
		/// </summary>
		LODInfo LOD { get; }

		/// <summary>
		///     Initialize this image loader. Will be called after construction.
		/// </summary>
		void Initialize();

		/// <summary>
		///     Start an asynchronized tile loading task
		/// </summary>
		void BeginLoadTileAsync(ILoadTileTask task);
	}
}