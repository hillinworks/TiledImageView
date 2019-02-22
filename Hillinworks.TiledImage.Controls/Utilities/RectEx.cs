
 
using System.Windows;

namespace Hillinworks.TiledImage.Utilities
{
	public static class RectEx
	{
		public static Rect FronCenterAndSize(Point center, Size size)
		{
			return new Rect(center.X - size.Width / 2, center.Y - size.Height / 2, size.Width, size.Height);
		}
	}
}