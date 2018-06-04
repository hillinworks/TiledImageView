using System.Windows;
using System.Windows.Input;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		private Point InputFocalPoint { get; set; }

		private static bool CheckMouseButtonStates(MouseEventArgs e, bool leftPressed, bool middlePressed, bool rightPressed)
		{
			return e.LeftButton == MouseButtonState.Pressed == leftPressed
			       && e.MiddleButton == MouseButtonState.Pressed == middlePressed
			       && e.RightButton == MouseButtonState.Pressed == rightPressed;
		}

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

			this.InputFocalPoint = e.GetPosition(this);

			if (CheckMouseButtonStates(e, true, false, false))
			{
				if (!this.IsDragging)
				{
					this.StartDragTranslating(this.InputFocalPoint);
					e.Handled = true;
				}
				else
				{
					this.UpdateDragTranslating(this.InputFocalPoint);
					e.Handled = true;
				}
			}
			else if (CheckMouseButtonStates(e, false, false, true))
			{
				if (!this.IsDragging)
				{
					this.StartDragRotating(this.InputFocalPoint);
					e.Handled = true;
				}
				else
				{
					this.UpdateDragRotating(this.InputFocalPoint);
					e.Handled = true;
				}
			}
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			base.OnMouseWheel(e);
			if (e.LeftButton != MouseButtonState.Pressed
			    && e.RightButton != MouseButtonState.Pressed
			    && e.MiddleButton != MouseButtonState.Pressed)
			{
				this.ZoomLevel *= 1.0 + e.Delta / 1200.0;
			}
		}
	}
}