using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Hillinworks.TiledImage.Controls.Overlays
{
    public abstract class OverlayUserControl : UserControl, IOverlay
    {
        protected internal TiledImageView ImageView { get; internal set; }

        protected TiledImageView AssociatedView { get; private set; }

        void IOverlay.OnLayerChanged(int layer)
        {
            this.OnLayerChanged(layer);
        }

        void IOverlay.OnViewStateChanged(ImageViewState viewState)
        {
            this.OnViewStateChanged(viewState);
        }

        void IOverlay.Render(DrawingContext context)
        {
            this.Render(context);
        }

        public virtual Control GetControl()
        {
            return this;
        }

        TiledImageView IOverlay.AssociatedView
        {
            set => this.AssociatedView = value;
        }

        /// <summary>
        /// Propagate a routed event to ImageView.
        /// </summary>
        /// <remarks>
        /// Some controls could stop events from being propagated (i.e. Button for mouse up/down events).
        /// </remarks>
        private void PropagateEventToImageView(RoutedEventArgs e)
        {
            var imageViewEvent = e.Clone();
            imageViewEvent.SetInvokingHandler(false);
            imageViewEvent.Source = this.ImageView;
            this.ImageView.RaiseEvent(imageViewEvent);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            this.PropagateEventToImageView(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            this.PropagateEventToImageView(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.PropagateEventToImageView(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            this.PropagateEventToImageView(e);
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            base.OnManipulationStarting(e);
            this.PropagateEventToImageView(e);
        }

        protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
        {
            base.OnManipulationInertiaStarting(e);
            this.PropagateEventToImageView(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);
            this.PropagateEventToImageView(e);
        }

        protected virtual void OnLayerChanged(int layer)
        {
        }

        protected virtual void OnViewStateChanged(ImageViewState viewState)
        {
        }

        protected virtual void Render(DrawingContext context)
        {
        }
    }
}