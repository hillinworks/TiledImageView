// ReSharper disable once CheckNamespace
namespace System
{
	public static class MathExtensions
	{
		public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
		{
			if (value.CompareTo(min) < 0)
			{
				return min;
			}

			if (value.CompareTo(max) > 0)
			{
				return max;
			}

			return value;
		}

		public static double PositiveModulo(this double value, double quotient)
		{
			return (value % quotient + quotient) % quotient;
		}

		public static int PositiveModulo(this int value, int quotient)
		{
			return (value % quotient + quotient) % quotient;
		}
	}
}