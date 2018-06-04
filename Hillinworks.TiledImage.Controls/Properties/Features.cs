namespace Hillinworks.TiledImage.Properties
{
	internal static class Features
	{
		#region Debug-only features

#if DEBUG

		/// <summary>
		///     Save the ghost tiledImage to a PNG file for debug purpose.
		/// </summary>
		public static bool SaveGhostImage { get; } = true;

		/// <summary>
		///     The path where the ghost tiledImage should be saved.
		/// </summary>
		public static string GhostImageSavePath { get; } = @"d:\\1.png";

		/// <summary>
		///     Draw the bounding boxes of the actual size (black), core size (red), viewport (orange) and layout rect (yellow)
		///     of the ImageView control.
		/// </summary>
		public static bool DrawImageViewBoundaries { get; } = true;

		/// <summary>
		///     Print the current stats on the top-left of the ImageView control.
		/// </summary>
		public static bool PrintImageViewStats { get; } = true;

		/// <summary>
		///     Draw status (failed, canceled, load progress etc.) and bounding boxes of tiles that haven't been loaded.
		/// </summary>
		public static bool DrawTileStatus { get; } = false;

#endif

		#endregion
	}
}