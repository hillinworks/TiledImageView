using System.Windows;

namespace Hillinworks.TiledImage.Utilities
{
	public static class RectExtensions
	{
		public static Point GetCenter(this Rect rect)
		{
			return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
		}
	}
}