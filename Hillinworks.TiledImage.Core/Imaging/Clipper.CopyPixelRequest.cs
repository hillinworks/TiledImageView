using System.Windows;
using System.Windows.Media.Imaging;

namespace Hillinworks.TiledImage.Imaging
{
    public static partial class Clipper
    {
        private struct CopyPixelRequest
        {
            public CopyPixelRequest(BitmapSource tileImage, Int32Rect sourceRect, Int32Rect destinationRect)
            {
                this.TileImage = tileImage;
                this.SourceRect = sourceRect;
                this.DestinationRect = destinationRect;
            }

            public Int32Rect SourceRect { get; }
            public Int32Rect DestinationRect { get;  }
            public BitmapSource TileImage { get; }
        }
    }
}
