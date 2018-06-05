using System.Windows;
using System.Windows.Input;
using Hillinworks.TiledImage.Utilities;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		/// <summary>
		/// The origin of next zoom. Will be consumed after zooming.
		/// </summary>
		private Point? ZoomOrigin { get; set; }

		/// <summary>
		/// The origin of next rotation. Will be consumed after rotation.
		/// </summary>
		private Point? RotateOrigin { get; set; }

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

			var mousePosition = e.GetPosition(this);

			if (CheckMouseButtonStates(e, true, false, false))
			{
				if (!this.IsDragging)
				{
					this.StartDragTranslating(mousePosition);
					e.Handled = true;
				}
				else
				{
					this.UpdateDragTranslating(mousePosition);
					e.Handled = true;
				}
			}
			else if (CheckMouseButtonStates(e, false, false, true))
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
				&& e.MiddleButton != MouseButtonState.Pressed)
			{
				this.Zoom(this.ZoomLevel * (1.0 + e.Delta / 1200.0), e.GetPosition(this));
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
			e.TranslationBehavior.DesiredDeceleration = 10.0 * 96 / (1000.0 * 1000.0);
			e.ExpansionBehavior.DesiredDeceleration = 0.1 * 96 / (1000.0 * 1000.0);
			e.RotationBehavior.DesiredDeceleration = 720 / (1000.0 * 1000.0);
			e.Handled = true;
		}

		protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
		{
			base.OnManipulationDelta(e);

			if (this.ViewState == null)
			{
				return;
			}

			this.Rotate(this.Rotation + e.DeltaManipulation.Rotation, e.ManipulationOrigin);
			this.Zoom(this.ZoomLevel * e.DeltaManipulation.Scale.X, e.ManipulationOrigin);
			this.Translate(this.Offset - e.DeltaManipulation.Translation);

			e.Handled = true;
		}
	}
}