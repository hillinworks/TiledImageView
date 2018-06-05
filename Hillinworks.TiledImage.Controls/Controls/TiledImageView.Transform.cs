using System;
using System.Windows;
using System.Windows.Media;
using Hillinworks.TiledImage.Properties;
using Hillinworks.TiledImage.Utilities;

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
				new PropertyMetadata(0.0, OnRotationChanged));

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
			set => this.SetValue(LayerProperty, value);
		}


		public double Rotation
		{
			get => (double)this.GetValue(RotationProperty);
			set => this.SetValue(RotationProperty, value);
		}

		public double ZoomLevel
		{
			get => (double)this.GetValue(ZoomLevelProperty);
			set => this.SetValue(ZoomLevelProperty, value);
		}

		public Vector Offset
		{
			get => (Vector)this.GetValue(OffsetProperty);
			set => this.SetValue(OffsetProperty, value);
		}

		private Point CenterPoint => new Point(this.ActualWidth / 2, this.ActualHeight / 2);

		private static object CoerceLayer(DependencyObject d, object baseValue)
		{
			return ((TiledImageView)d).CoerceLayer((int)baseValue);
		}

		private object CoerceLayer(int baseValue)
		{
			return this.ViewState == null 
				? Math.Max(0, baseValue) 
				: baseValue.Clamp(0, this.Source.Dimensions.LayerCount);
		}

		private static void OnLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((TiledImageView)d).OnLayerChanged((int)e.NewValue);
		}

		private void OnLayerChanged(int layer)
		{
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
			if (this.ViewState.EnvelopSize.Width < this.ActualWidth
				&& Features.CentralizeImageIfSmallerThanViewport)
			{
				x = -(this.ActualWidth - this.ViewState.EnvelopSize.Width) / 2;
			}
			else
			{
				x = baseValue.X.Clamp(0, this.ViewState.EnvelopSize.Width - this.ActualWidth);
			}

			double y;
			if (this.ViewState.EnvelopSize.Height < this.ActualHeight
				&& Features.CentralizeImageIfSmallerThanViewport)
			{
				y = -(this.ActualHeight - this.ViewState.EnvelopSize.Height) / 2;
			}
			else
			{
				y = baseValue.Y.Clamp(0, this.ViewState.EnvelopSize.Height - this.ActualHeight);
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
			}

			this.InvalidateScrollInfo();
		}

		private void OnRotationChanged(double rotation)
		{
			if (this.ViewState != null)
			{
				this.ViewState.Rotation = rotation;
			}

			this.UpdateScrollability();
		}

		private void OnZoomLevelChanged(double zoomLevel)
		{
			var focalPoint = this.InputFocalPoint;
			if (focalPoint.X < 0
				|| focalPoint.Y < 0
				|| focalPoint.X > this.ActualWidth
				|| focalPoint.Y > this.ActualHeight)
			{
				// zoom about our center if the focal point is out of bound, this is good for 
				// host program to implement button/slider based zooming
				focalPoint = this.CenterPoint;
			}

			this.ViewState?.Zoom(zoomLevel, focalPoint);

			this.UpdateScrollability();
		}

		public void SetTransform(Matrix matrix)
		{
			this.ViewState?.SetTransform(matrix);
		}
	}
}