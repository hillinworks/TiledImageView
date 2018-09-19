// ReSharper disable once CheckNamespace
namespace System.Windows
{
	public static class RectExtensions
	{
		public static Point GetCenter(this Rect rect)
		{
			return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
		}

	    public static Point[] GetVertices(this Rect rect)
	    {
	        return new Point[]
	        {
	            rect.TopLeft,
	            rect.TopRight,
	            rect.BottomRight,
	            rect.BottomLeft
	        };
	    }
	}
}