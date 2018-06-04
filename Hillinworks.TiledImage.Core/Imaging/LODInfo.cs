namespace Hillinworks.TiledImage.Imaging
{
	public struct LODInfo
	{
		public int MinLODLevel { get; }
		public int MaxLODLevel { get; }
		public double InitialZoomLevel { get; }
		public double MaxZoomLevel { get; }

		public LODInfo(int minLODLevel, int maxLODLevel, double initialZoomLevel, double maxZoomLevel)
		{
			this.MinLODLevel = minLODLevel;
			this.MaxLODLevel = maxLODLevel;
			this.InitialZoomLevel = initialZoomLevel;
			this.MaxZoomLevel = maxZoomLevel;
		}
	}
}