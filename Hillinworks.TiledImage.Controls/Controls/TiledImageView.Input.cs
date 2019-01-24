using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Hillinworks.TiledImage.Controls
{
    partial class TiledImageView
    {
        public static readonly DependencyProperty AllowUserPanProperty =
            DependencyProperty.Register("AllowUserPan", typeof(bool), typeof(TiledImageView),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowUserRotateProperty =
            DependencyProperty.Register("AllowUserRotate", typeof(bool), typeof(TiledImageView),
                new PropertyMetadata(true));

        public static readonly DependencyProperty AllowUserZoomProperty =
            DependencyProperty.Register("AllowUserZoom", typeof(bool), typeof(TiledImageView),
                new PropertyMetadata(true));


        public bool AllowUserPan
        {
            get => (bool)this.GetValue(AllowUserPanProperty);
            set => this.SetValue(AllowUserPanProperty, value);
        }


        public bool AllowUserRotate
        {
            get => (bool)this.GetValue(AllowUserRotateProperty);
            set => this.SetValue(AllowUserRotateProperty, value);
        }


        public bool AllowUserZoom
        {
            get => (bool)this.GetValue(AllowUserZoomProperty);
            set => this.SetValue(AllowUserZoomProperty, value);
        }


        /// <summary>
        ///     The origin of next zoom. Will be consumed after zooming.
        /// </summary>
        private Point? ZoomOrigin { get; set; }

        /// <summary>
        ///     The origin of next rotation. Will be consumed after rotation.
        /// </summary>
        private Point? RotateOrigin { get; set; }


        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            this.EndDragging();
            e.Handled = true;
        }
        
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            this.EndDragging();
            e.Handled = true;
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {

            base.OnMouseMove(e);

            var mousePosition = e.GetPosition(this);

            if (e.CheckMouseButtonStates(true, false, false) && this.AllowUserPan)
            {
                if (!this.IsDragging)
                {
                    this.StartDragPanning(mousePosition);
                    e.Handled = true;
                }
                else
                {
                    this.UpdateDragTranslating(mousePosition);
                    e.Handled = true;
                }
            }
            else if (e.CheckMouseButtonStates(false, false, true) && this.AllowUserRotate)
            {
                if (!this.IsDragging)
                {
                    this.StartDragRotating(mousePosition);
                    e.Handled = true;
                }
                else
                {
                    this.UpdateDragRotating(mousePosition);
                    e.Handled = true;
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.LeftButton != MouseButtonState.Pressed
                && e.RightButton != MouseButtonState.Pressed
                && e.MiddleButton != MouseButtonState.Pressed
                && this.AllowUserZoom)
            {
                var delta = e.Delta > 0 ? 1.0 + e.Delta / 1200.0 : 1 / (1.0 - e.Delta / 1200.0);
                this.Zoom(this.ZoomLevel * delta, e.GetPosition(this));
            }
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            base.OnManipulationStarting(e);
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
        {
            base.OnManipulationInertiaStarting(e);

            if (this.AllowUserPan)
            {
                e.TranslationBehavior.DesiredDeceleration = 10.0 * 96 / (1000.0 * 1000.0);
            }

            if (this.AllowUserZoom)
            {
                e.ExpansionBehavior.DesiredDeceleration = 0.1 * 96 / (1000.0 * 1000.0);
            }

            if (this.AllowUserRotate)
            {
                e.RotationBehavior.DesiredDeceleration = 720 / (1000.0 * 1000.0);
            }

            e.Handled = true;
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            if (this.ViewState == null)
            {
                return;
            }

            if (this.AllowUserRotate)
            {
                this.Rotate(this.Rotation + e.DeltaManipulation.Rotation, e.ManipulationOrigin);
            }

            if (this.AllowUserZoom)
            {
                this.Zoom(this.ZoomLevel * e.DeltaManipulation.Scale.X, e.ManipulationOrigin);
            }

            if (this.AllowUserPan)
            {
                this.Translate(this.Offset - e.DeltaManipulation.Translation);
            }

            e.Handled = true;
        }
    }
}