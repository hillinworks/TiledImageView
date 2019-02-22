
 
using System.Windows;

namespace Hillinworks.TiledImage.Extensions
{
	internal static class PointExtensions
	{
		public static Point Scale(this Point p, double scale)
		{
			return new Point(p.X * scale, p.Y * scale);
		}
	}
}