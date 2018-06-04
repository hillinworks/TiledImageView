using System;
using System.Windows;
using System.Windows.Media;
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
				new PropertyMetadata(1.0, OnZoomLevelChanged));

		public double Rotation
		{
			get => (double) this.GetValue(RotationProperty);
			set => this.SetValue(RotationProperty, value);
		}

		public double ZoomLevel
		{
			get => (double) this.GetValue(ZoomLevelProperty);
			set => this.SetValue(ZoomLevelProperty, value);
		}

		public Vector Offset
		{
			get => (Vector) this.GetValue(OffsetProperty);
			set => this.SetValue(OffsetProperty, value);
		}

		private Point CenterPoint => new Point(this.ActualWidth / 2, this.ActualHeight / 2);

		private static object CoerceOffset(DependencyObject d, object basevalue)
		{
			return ((TiledImageView) d).CoerceOffset((Vector) basevalue);
		}

		private Vector CoerceOffset(Vector baseValue)
		{
			if (this.ViewState == null)
			{
				return baseValue;
			}

			double x;
			if (this.ViewState.EnvelopSize.Width < this.ActualWidth)
			{
				x = -(this.ActualWidth - this.ViewState.EnvelopSize.Width) / 2;
			}
			else
			{
				x = baseValue.X.Clamp(0, this.ViewState.EnvelopSize.Width - this.ActualWidth);
			}

			double y;
			if (this.ViewState.EnvelopSize.Height < this.ActualHeight)
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
			((TiledImageView) d).OnRotationChanged((double) e.NewValue);
		}

		private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((TiledImageView) d).OnZoomLevelChanged((double) e.NewValue);
		}

		private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((TiledImageView) d).OnOffsetChanged((Vector) e.NewValue);
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
			this.ViewState?.Zoom(zoomLevel, this.InputFocalPoint);

			this.UpdateScrollability();
		}

		public void SetTransform(Matrix matrix)
		{
			this.ViewState?.SetTransform(matrix);
		}
	}
}