using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView : IScrollInfo
	{
		private const double LineSize = 16;

		private bool CanVerticallyScroll { get; set; }

		private bool CanHorizontallyScroll { get; set; }

		private ScrollViewer ScrollOwner { get; set; }

		bool IScrollInfo.CanVerticallyScroll
		{
			get => this.CanVerticallyScroll;
			set => this.CanVerticallyScroll = value;
		}

		bool IScrollInfo.CanHorizontallyScroll
		{
			get => this.CanHorizontallyScroll;
			set => this.CanHorizontallyScroll = value;
		}

		double IScrollInfo.ExtentWidth => this.ExtentSize.Width;
		double IScrollInfo.ExtentHeight => this.ExtentSize.Height;
		double IScrollInfo.ViewportWidth => this.RenderSize.Width;
		double IScrollInfo.ViewportHeight => this.RenderSize.Height;

		double IScrollInfo.HorizontalOffset =>
			this.ViewState == null
				? 0.0
				: this.Offset.X.PositiveModulo(this.ViewState.EnvelopRect.Width);

		double IScrollInfo.VerticalOffset =>
			this.ViewState == null
				? 0.0
				: this.Offset.Y.PositiveModulo(this.ViewState.EnvelopRect.Height);

		ScrollViewer IScrollInfo.ScrollOwner
		{
			get => this.ScrollOwner;
			set => this.ScrollOwner = value;
		}

		void IScrollInfo.LineDown()
		{
			this.Offset = new Vector(this.Offset.X, this.Offset.Y + LineSize);
		}

		void IScrollInfo.LineLeft()
		{
			this.Offset = new Vector(this.Offset.X - LineSize, this.Offset.Y);
		}

		void IScrollInfo.LineRight()
		{
			this.Offset = new Vector(this.Offset.X + LineSize, this.Offset.Y);
		}

		void IScrollInfo.LineUp()
		{
			this.Offset = new Vector(this.Offset.X, this.Offset.Y - LineSize);
		}

		Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
		{
			throw new NotImplementedException();
		}

		void IScrollInfo.MouseWheelDown()
		{
			/* wheel scroll disabled */
		}

		void IScrollInfo.MouseWheelLeft()
		{
			/* wheel scroll disabled */
		}

		void IScrollInfo.MouseWheelRight()
		{
			/* wheel scroll disabled */
		}

		void IScrollInfo.MouseWheelUp()
		{
			/* wheel scroll disabled */
		}

		void IScrollInfo.PageDown()
		{
			this.Offset = new Vector(this.Offset.X, this.Offset.Y + this.ActualHeight);
		}

		void IScrollInfo.PageLeft()
		{
			this.Offset = new Vector(this.Offset.X - this.ActualWidth, this.Offset.Y);
		}

		void IScrollInfo.PageRight()
		{
			this.Offset = new Vector(this.Offset.X + this.ActualWidth, this.Offset.Y);
		}

		void IScrollInfo.PageUp()
		{
			this.Offset = new Vector(this.Offset.X, this.Offset.Y - this.ActualHeight);
		}

		void IScrollInfo.SetHorizontalOffset(double offset)
		{
			this.Offset = new Vector(offset, this.Offset.Y);
		}

		void IScrollInfo.SetVerticalOffset(double offset)
		{
			this.Offset = new Vector(this.Offset.X, offset);
		}

		private void UpdateScrollability()
		{
			this.CanHorizontallyScroll = this.ViewState != null 
			                             && this.ActualWidth < this.ViewState.EnvelopRect.Width;
			this.CanVerticallyScroll = this.ViewState != null
			                           && this.ActualHeight < this.ViewState.EnvelopRect.Height;
			this.InvalidateScrollInfo();
		}

		private void InvalidateScrollInfo()
		{
			this.ScrollOwner?.InvalidateScrollInfo();
		}
	}
}