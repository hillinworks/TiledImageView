using System.Windows;

namespace Hillinworks.TiledImage.Utilities
{
	public static class FrameworkElementExtensions
	{
		public static Size GetActualSize(this FrameworkElement element)
		{
			return new Size(element.ActualWidth, element.ActualHeight);
		}
	}
}