
 // ReSharper disable once CheckNamespace
namespace System.Windows
{
	public static class FrameworkElementExtensions
	{
		public static Size GetActualSize(this FrameworkElement element)
		{
			return new Size(element.ActualWidth, element.ActualHeight);
		}
	}
}