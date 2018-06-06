

// ReSharper disable once CheckNamespace
namespace System.Windows.Input
{
	public static class MouseEventArgsExtensions
	{
		public static bool CheckMouseButtonStates(
			this MouseEventArgs e,
			bool leftPressed,
			bool middlePressed,
			bool rightPressed)
		{
			return e.LeftButton == MouseButtonState.Pressed == leftPressed
			       && e.MiddleButton == MouseButtonState.Pressed == middlePressed
			       && e.RightButton == MouseButtonState.Pressed == rightPressed;
		}
	}
}