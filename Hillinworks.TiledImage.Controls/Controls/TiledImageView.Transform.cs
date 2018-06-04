using System.Windows;
using System.Windows.Media;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		public static readonly DependencyProperty OffsetProperty =
			DependencyProperty.Register(
				"Offset",
				typeof(Vector),
				typeof(TiledImageView),
				new PropertyMetadata(default(Vector), OnOffsetChanged));

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