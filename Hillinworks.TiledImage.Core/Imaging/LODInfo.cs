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
			this.Validate();
		}

		internal void Validate()
		{
			if (this.MinLODLevel > this.MaxLODLevel)
			{
				throw new InvalidLODInfoException(
					$"{nameof(MinLODLevel)} must be smaller than or equal to {nameof(MaxLODLevel)}");
			}

			if (this.InitialZoomLevel <= 0)
			{
				throw new InvalidLODInfoException(
					$"{nameof(InitialZoomLevel)} must be greater than zero");
			}

			if (this.MaxZoomLevel <= 0)
			{
				throw new InvalidLODInfoException(
					$"{nameof(MaxZoomLevel)} must be greater than zero");
			}

			if (this.InitialZoomLevel > this.MaxZoomLevel)
			{
				throw new InvalidLODInfoException(
					$"{nameof(InitialZoomLevel)} must be smaller than or equal to {nameof(MaxZoomLevel)}");
			}
		}
	}
}