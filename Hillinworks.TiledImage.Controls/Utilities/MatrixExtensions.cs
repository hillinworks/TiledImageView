using System.Windows;
using System.Windows.Media;

namespace Hillinworks.TiledImage.Utilities
{
	public static class MatrixExtensions
	{
		/// <summary>
		///     Transform the vertices of the specified rect with the specified matrix.
		/// </summary>
		/// <returns>Transformed vertices in the order of top-left, top-right, bottom-right and bottom-left.</returns>
		public static Point[] TransformVertices(this Matrix matrix, Rect rect)
		{
			var points = new[]
			{
				rect.TopLeft,
				rect.TopRight,
				rect.BottomRight,
				rect.BottomLeft
			};

			matrix.Transform(points);

			return points;
		}

		public static Matrix GetInverse(this Matrix matrix)
		{
			var inverse = matrix;
			inverse.Invert();
			return inverse;
		}
	}
}