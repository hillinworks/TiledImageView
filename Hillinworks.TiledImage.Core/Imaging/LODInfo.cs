using System;
using System.Linq;

namespace Hillinworks.TiledImage.Imaging
{
    public struct LODInfo
    {
        public int MinLODLevel { get; }
        public int MaxLODLevel { get; }
        public double InitialZoomLevel { get; }
        public double MaxZoomLevel { get; }
        public double MinZoomLevel { get; }

        /// <summary>
        /// Scale ratios between LOD levels.
        /// </summary>
        public double[] LODGaps { get; }

        public LODInfo(int minLODLevel, int maxLODLevel, double initialZoomLevel, double maxZoomLevel, double[] lodGaps)
        {
            this.MinLODLevel = minLODLevel;
            this.MaxLODLevel = maxLODLevel;
            this.InitialZoomLevel = initialZoomLevel;
            this.MaxZoomLevel = maxZoomLevel;
            this.LODGaps = lodGaps;
            this.MinZoomLevel = lodGaps.Aggregate(maxZoomLevel, (result, gap) => result / gap);
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

            if (this.MinZoomLevel <= 0)
            {
                throw new InvalidLODInfoException(
                    $"{nameof(this.MinZoomLevel)} must be greater than zero");
            }

            if (this.MaxZoomLevel < this.MinZoomLevel)
            {
                throw new InvalidLODInfoException(
                    $"{nameof(this.MaxZoomLevel)} must be greater than {this.MinZoomLevel}");
            }

            if (this.InitialZoomLevel > this.MaxZoomLevel)
            {
                throw new InvalidLODInfoException(
                    $"{nameof(this.InitialZoomLevel)} must be smaller than or equal to {nameof(this.MaxZoomLevel)}");
            }
        }

        public int CalculateLODLevel(double zoomLevel)
        {
            var currentZoomLevel = this.MinZoomLevel;
            for (var lodLevel = this.MaxLODLevel; lodLevel > this.MinLODLevel; --lodLevel)
            {
                if (zoomLevel <= currentZoomLevel)
                {
                    return lodLevel;
                }

                currentZoomLevel *= this.LODGaps[lodLevel - 1];
            }

            return this.MinLODLevel;
        }

        public double GetLODToWorldScale(int lodLevel)
        {
            var scale = 1.0;
            for (var i = 0; i < lodLevel; ++i)
            {
                scale *= this.LODGaps[i];
            }

            return scale;
        }
    }
}