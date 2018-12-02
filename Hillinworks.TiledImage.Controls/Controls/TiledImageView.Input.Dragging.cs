using System.Windows;
using System.Windows.Input;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		private bool IsDragging { get; set; }
		private double DragStartRotationAngle { get; set; }
		private Point DragStartPosition { get; set; }
		private Vector DragStartOffset { get; set; }


		private void StartDragPanning(Point position)
		{
			var canHorizontallyScroll = this.CanHorizontallyScroll 
			                            && this.ActualWidth < this.ViewState.EnvelopRect.Width;

			var canVerticallyScroll = this.CanVerticallyScroll 
			                          && this.ActualHeight < this.ViewState.EnvelopRect.Height;

			if (canHorizontallyScroll)
			{
				this.Cursor = canVerticallyScroll ? Cursors.ScrollAll : Cursors.ScrollWE;
			}
			else
			{
				if (canVerticallyScroll)
				{
					this.Cursor = Cursors.ScrollNS;
				}
				else
				{
					return;
				}
			}

			this.IsDragging = true;
			this.DragStartPosition = position;
			this.DragStartOffset = this.Offset;
			this.CaptureMouse();
		}

		private void UpdateDragTranslating(Point position)
		{
			this.Offset = this.DragStartPosition - position + this.DragStartOffset;

			this.InvalidateScrollInfo();
		}

		private void EndDragging()
		{
			this.Cursor = Cursors.Arrow;
			this.IsDragging = false;
			this.ReleaseMouseCapture();
		}

		private void StartDragRotating(Point position)
		{
			this.IsDragging = true;
			this.DragStartPosition = position;
			this.DragStartRotationAngle = this.Rotation;
			this.CaptureMouse();
		}

		private void UpdateDragRotating(Point position)
		{
			var center = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
			var delta = Vector.AngleBetween(this.DragStartPosition - center, position - center);
			this.Rotate(this.DragStartRotationAngle + delta, this.CenterPoint);
		}
	}
}