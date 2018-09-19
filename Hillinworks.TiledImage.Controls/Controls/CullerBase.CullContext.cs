using System.Windows;
using System.Windows.Media;

namespace Hillinworks.TiledImage.Controls
{
    internal abstract partial class CullerBase
    {
        protected struct CullContext
        {
            public Rect CullRect { get; }
            public ImageViewState ViewState { get; }

            public CullContext(Rect cullRect, ImageViewState viewState)
            {
                this.CullRect = cullRect;
                this.ViewState = viewState;
                this.WorldCullRectVertices = viewState.ViewToWorldMatrix.TransformVertices(cullRect);
                var cullRectCenter = new Point(cullRect.X + cullRect.Width / 2,
                    cullRect.Y + cullRect.Height / 2);
                this.WorldCullRectCenter = viewState.ViewToWorldMatrix.Transform(cullRectCenter);
            }

            public Point WorldCullRectCenter { get; }

            public Point[] WorldCullRectVertices { get; }
        }
    }
}