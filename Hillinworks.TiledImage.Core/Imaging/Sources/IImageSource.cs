using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hillinworks.TiledImage.Imaging.Sources
{
    public interface IImageSource : IDisposable
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
        ///     Load a tile asynchronously.
        /// </summary>
        Task<BitmapSource> LoadTileAsync(
            TileIndex.Full index, 
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default);

        Task<BitmapSource> CreateThumbnailAsync(double width, double height);
    }
}