using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hillinworks.TiledImage.Utilities;

namespace Hillinworks.TiledImage.Imaging
{
    public static class Clipper
    {
        private static unsafe void ClearBackBuffer(this WriteableBitmap bitmap)
        {
            var length = bitmap.PixelHeight * bitmap.BackBufferStride;
            var pBuffer = (byte*)bitmap.BackBuffer;
            for (var i = 0; i < length; ++i, ++pBuffer)
            {
                *pBuffer = 0xff;
            }
        }

        public static async Task<BitmapSource> ClipAsync(
			this TiledImageSource imageSource, 
			Int32Rect bounds, 
			int? layer = null, 
			int? lodLevel = null)
        {
	        var finalLayer = layer ?? imageSource.Dimensions.MinimumLayerIndex;
	        var finalLodLevel = lodLevel ?? imageSource.LOD.MaxLODLevel;

			var dimensions = imageSource.Dimensions.AtLODLevel(finalLodLevel);

            var tileWidth = dimensions.TileWidth;
            var tileHeight = dimensions.TileHeight;
            var tileIndexLeft = bounds.X / tileWidth;
            var tileIndexTop = bounds.Y / tileHeight;
            var tileIndexRight = (bounds.X + bounds.Width - 1) / tileWidth;
            var tileIndexBottom = (bounds.Y + bounds.Height - 1) / tileHeight;

            var image = new WriteableBitmap(bounds.Width, bounds.Height, 96, 96, PixelFormats.Bgr24, null);
            image.ClearBackBuffer();

            for (var row = tileIndexTop; row <= tileIndexBottom; ++row)
            {
                var tileTop = row * tileHeight;
                var tileBottom = (row + 1) * tileHeight;

                var copyMetricsY = ImageCopyMetrics.Calculate(tileTop, tileBottom, bounds.Y, bounds.Height);

                for (var column = tileIndexLeft; column <= tileIndexRight; ++column)
                {
                    var tileLeft = column * tileWidth;
                    var tileRight = (column + 1) * tileWidth;

                    var index = new TileIndex.Full(column, row, finalLayer, finalLodLevel);
                    var tileImage = await imageSource.LoadTileAsync(index);

                    var copyMetricsX = ImageCopyMetrics.Calculate(tileLeft, tileRight, bounds.X, bounds.Height);

                    var sourceRect = new Int32Rect(
                        copyMetricsX.Source,
                        copyMetricsY.Source,
                        copyMetricsX.Size,
                        copyMetricsY.Size);

                    var destinationRect = new Int32Rect(
                        copyMetricsX.Destination,
                        copyMetricsY.Destination,
                        copyMetricsX.Size,
                        copyMetricsY.Size);

                    tileImage.CopyPixels(
                        sourceRect,
                        image.BackBuffer + destinationRect.Y * image.BackBufferStride + destinationRect.X * 3,
                        image.PixelHeight * image.BackBufferStride,
                        image.BackBufferStride);
                }
            }

            image.Freeze();
            return image;
        }
    }
}
