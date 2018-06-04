using System.Windows;

namespace Hillinworks.TiledImage.Utilities
{
	public static class VectorLikeExtensions
	{
		public static Vector ToVector(this Point point)
		{
			return new Vector(point.X, point.Y);
		}

		public static Point ToPoint(this Vector vector)
		{
			return new Point(vector.X, vector.Y);
		}

		public static Vector ToVector(this Size size)
		{
			return new Vector(size.Width, size.Height);
		}

		public static Size ToSize(this Vector vector)
		{
			return new Size(vector.X, vector.Y);
		}
	}
}