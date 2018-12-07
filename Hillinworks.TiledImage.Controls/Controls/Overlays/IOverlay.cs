using System.Windows.Controls;
using System.Windows.Media;

namespace Hillinworks.TiledImage.Controls.Overlays
{
    public interface IOverlay
    {
        void OnLayerChanged(int layer);
        void OnViewStateChanged(ImageViewState viewState);
        void Render(DrawingContext context);
        Control GetControl();
        TiledImageView AssociatedView { set; }
    }
}