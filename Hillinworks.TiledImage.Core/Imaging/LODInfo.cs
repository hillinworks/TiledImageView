using System;

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
					$"{nameof(this.MinLODLevel)} must be smaller than or equal to {nameof(this.MaxLODLevel)}");
			}

			if (this.InitialZoomLevel <= 0)
			{
				throw new InvalidLODInfoException(
					$"{nameof(this.InitialZoomLevel)} must be greater than zero");
			}

			if (this.MaxZoomLevel <= 0)
			{
				throw new InvalidLODInfoException(
					$"{nameof(this.MaxZoomLevel)} must be greater than zero");
			}

			if (this.InitialZoomLevel > this.MaxZoomLevel)
			{
				throw new InvalidLODInfoException(
					$"{nameof(this.InitialZoomLevel)} must be smaller than or equal to {nameof(this.MaxZoomLevel)}");
			}
		}

	    public int CalculateLODLevel(double zoomLevel)
	    {
	        return ((int)Math.Floor(Math.Log(this.MaxZoomLevel / zoomLevel, 2)))
	            .Clamp(0, this.MaxLODLevel);
        }
	}
}