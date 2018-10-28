using System;
using System.Windows;
using System.Windows.Input;
using Hillinworks.TiledImage.Properties;

namespace Hillinworks.TiledImage.Controls
{
    partial class TiledImageView
    {
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(
                "Offset",
                typeof(Vector),
                typeof(TiledImageView),
                new PropertyMetadata(default(Vector), OnOffsetChanged, CoerceOffset));

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(
                "Rotation",
                typeof(double),
                typeof(TiledImageView),
                new PropertyMetadata(0.0, OnRotationChanged, CoerceRotation));

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(
                "ZoomLevel",
                typeof(double),
                typeof(TiledImageView),
                new PropertyMetadata(1.0, OnZoomLevelChanged, CoerceZoomLevel));

        public static readonly DependencyProperty LayerProperty =
            DependencyProperty.Register(
                "Layer",
                typeof(int),
                typeof(TiledImageView),
                new PropertyMetadata(1, OnLayerChanged, CoerceLayer));

        public int Layer
        {
            get => (int)this.GetValue(LayerProperty);
            set => this.SetValue(LayerProperty, this.CoerceLayer(value));
        }

        /// <summary>
        /// Get or set the rotation of the image.
        /// </summary>
        /// <remarks>
        /// To rotate about a point, use the <see cref="TiledImageView.Rotate"/> method. Setting this directly 
        /// will defaultly rotate about the center of the view.
        /// </remarks>
        public double Rotation
        {
            get => (double)this.GetValue(RotationProperty);
            set => this.SetValue(RotationProperty, this.CoerceRotation(value));
        }

        /// <summary>
        /// Get or set the zoom level of the image.
        /// </summary>
        /// <remarks>
        /// To zoom about a point, use the <see cref="TiledImageView.Zoom"/> method. Setting this directly
        /// will defaulty zoom about the mouse position if it's inside of the view, otherwise the center point
        /// of the view.
        /// </remarks>
        public double ZoomLevel
        {
            get => (double)this.GetValue(ZoomLevelProperty);
            set => this.SetValue(ZoomLevelProperty, this.CoerceZoomLevel(value));
        }

        /// <summary>
        /// Get or set the offset (translation) of the image.
        /// </summary>
        public Vector Offset
        {
            get => (Vector)this.GetValue(OffsetProperty);
            set => this.SetValue(OffsetProperty, this.CoerceOffset(value));
        }

        private Point CenterPoint => new Point(this.ActualWidth / 2, this.ActualHeight / 2);

        private static object CoerceRotation(DependencyObject d, object baseValue)
        {
            return ((TiledImageView)d).CoerceRotation((double)baseValue);
        }

        private double CoerceRotation(double baseValue)
        {
            return baseValue.PositiveModulo(360);
        }

        private static object CoerceLayer(DependencyObject d, object baseValue)
        {
            return ((TiledImageView)d).CoerceLayer((int)baseValue);
        }

        private object CoerceLayer(int baseValue)
        {
            return this.ViewState == null
                ? Math.Max(0, baseValue)
                : baseValue.Clamp(this.Source.Dimensions.MinimumLayerIndex, this.Source.Dimensions.MaximumLayerIndex);
        }

        private static void OnLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TiledImageView)d).OnLayerChanged((int)e.NewValue);
        }

        private void OnLayerChanged(int layer)
        {
            if (this.ViewState == null)
            {
                return;
            }

            this.ViewState.Layer = layer;
        }

        private static object CoerceZoomLevel(DependencyObject d, object baseValue)
        {
            return ((TiledImageView)d).CoerceZoomLevel((double)baseValue);
        }

        private double CoerceZoomLevel(double baseValue)
        {
            if (this.Source == null)
            {
                return baseValue;
            }

            // todo: overzoom and underzoom should be configurable somewhere
            return baseValue.Clamp(0.5, this.Source.LOD.MaxZoomLevel * 2);
        }

        private static object CoerceOffset(DependencyObject d, object basevalue)
        {
            return ((TiledImageView)d).CoerceOffset((Vector)basevalue);
        }

        private Vector CoerceOffset(Vector baseValue)
        {
            if (this.ViewState == null)
            {
                return baseValue;
            }

            double x;
            if (this.ViewState.EnvelopRect.Width < this.ActualWidth
                && Features.CentralizeImageIfSmallerThanViewport)
            {
                x = -(this.ActualWidth - this.ViewState.EnvelopRect.Width) / 2;
            }
            else
            {
                x = baseValue.X.Clamp(0, this.ViewState.EnvelopRect.Width - this.ActualWidth);
            }

            double y;
            if (this.ViewState.EnvelopRect.Height < this.ActualHeight
                && Features.CentralizeImageIfSmallerThanViewport)
            {
                y = -(this.ActualHeight - this.ViewState.EnvelopRect.Height) / 2;
            }
            else
            {
                y = baseValue.Y.Clamp(0, this.ViewState.EnvelopRect.Height - this.ActualHeight);
            }

            return new Vector(x, y);
        }

        private static void OnRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TiledImageView)d).OnRotationChanged((double)e.NewValue);
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TiledImageView)d).OnZoomLevelChanged((double)e.NewValue);
        }

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TiledImageView)d).OnOffsetChanged((Vector)e.NewValue);
        }

        private void OnOffsetChanged(Vector offset)
        {
            if (this.ViewState != null)
            {
                this.ViewState.Offset = offset;
                this.OnViewStateChanged();
            }

            this.InvalidateScrollInfo();
        }

        private void OnRotationChanged(double rotation)
        {
            if (this.ViewState != null)
            {
                this.ViewState.Rotate(rotation, this.RotateOrigin ?? this.CenterPoint);
                this.OnViewStateChanged();
            }

            this.RotateOrigin = null;

            this.UpdateScrollability();
        }

        private void OnZoomLevelChanged(double zoomLevel)
        {
            var origin = this.ZoomOrigin ?? Mouse.GetPosition(this);
            if (origin.X < 0
                || origin.Y < 0
                || origin.X > this.ActualWidth
                || origin.Y > this.ActualHeight)
            {
                // zoom about our center if the focal point is out of bound, this is good for 
                // host program to implement button/slider based zooming
                origin = this.CenterPoint;
            }

            this.ZoomOrigin = null;

            if (this.ViewState != null)
            {
                this.ViewState.Zoom(zoomLevel, origin);
                this.OnViewStateChanged();
            }

            this.UpdateScrollability();
        }

        public void Centralize(Point? position = null)
        {
            Vector offset;
            if (position != null)
            {
                var viewPosition = this.ViewState.WorldToEnvelopMatrix.Transform(position.Value);
                offset = new Vector(
                    viewPosition.X - this.ActualWidth / 2, 
                    viewPosition.Y - this.ActualHeight / 2);
            }
            else
            {
                offset = new Vector(
                    (this.ViewState.EnvelopRect.Width - this.ActualWidth) / 2,
                    (this.ViewState.EnvelopRect.Height - this.ActualHeight) / 2);
            }

            this.Translate(offset);
        }

        private void Rotate(double rotation, Point origin)
        {
            this.RotateOrigin = origin;
            this.Rotation = rotation;
        }

        private void Zoom(double zoomLevel, Point origin)
        {
            this.ZoomOrigin = origin;
            this.ZoomLevel = zoomLevel;
        }

        private void Translate(Vector translation)
        {
            this.Offset = translation;
        }
    }
}