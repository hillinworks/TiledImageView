using System;

namespace Hillinworks.TiledImage.Imaging
{
    public struct Dimensions
    {
        /// <summary>
        ///     Horizontal tile count
        /// </summary>
        public int HorizontalTiles { get; }

        /// <summary>
        ///     Vertical tile count
        /// </summary>
        public int VerticalTiles { get; }

        /// <summary>
        ///     Width of a single tile in pixels
        /// </summary>
        public int TileWidth { get; }

        /// <summary>
        ///     Height of a single tile in pixels
        /// </summary>
        public int TileHeight { get; }

        /// <summary>
        ///     Layer count, does not include the stacked layer
        /// </summary>
        public int LayerCount { get; }

        public int MinimumLayerIndex => this.LayerCount == 1 ? 1 : 0;
        public int MaximumLayerIndex => this.LayerCount == 1 ? 1 : this.LayerCount - 1;

        /// <summary>
        ///     Width of blank space before (to the left of) content in pixels
        /// </summary>
        public int ContentLeft { get; }

        /// <summary>
        ///     Height of blank space before (on top of) content in pixels
        /// </summary>
        public int ContentTop { get; }

        /// <summary>
        ///     Width of the entire image in pixels
        /// </summary>
        public int Width => this.HorizontalTiles * this.TileWidth;

        /// <summary>
        ///     Height of the entire image in pixels
        /// </summary>
        public int Height => this.VerticalTiles * this.TileHeight;

        /// <summary>
        ///     Width of the entire image with the blank space trimmed, in pixels
        /// </summary>
        public int ContentWidth { get; }

        /// <summary>
        ///     Height of the entire image with the blank space trimmed, in pixels
        /// </summary>
        public int ContentHeight { get; }

        /// <summary>
        ///     Total tile count of a single layer in this image
        /// </summary>
        public int TileCount => this.HorizontalTiles * this.VerticalTiles;

        /// <summary>
        ///     Total tile count of all layers in this image, including the stacked layer
        /// </summary>
        public int LayeredTileCount => this.HorizontalTiles * this.VerticalTiles * (this.LayerCount + 1);

        public Dimensions(
            int horizontalTiles,
            int verticalTiles,
            int tileWidth,
            int tileHeight,
            int layerCount,
            int contentLeft,
            int contentTop,
            int contentWidth,
            int contentHeight)
        {
            this.HorizontalTiles = horizontalTiles;
            this.VerticalTiles = verticalTiles;
            this.TileWidth = tileWidth;
            this.TileHeight = tileHeight;
            this.LayerCount = layerCount;
            this.ContentLeft = contentLeft;
            this.ContentTop = contentTop;
            this.ContentWidth = contentWidth;
            this.ContentHeight = contentHeight;
            this.Validate();
        }

        public Dimensions AtLODLevel(int lodLevel)
        {
            var lodFactor = (int)Math.Pow(2, lodLevel);
            return new Dimensions(
                Math.Max(1, this.HorizontalTiles / lodFactor),
                Math.Max(1, this.VerticalTiles / lodFactor),
                this.TileWidth,
                this.TileHeight,
                this.LayerCount,
                this.ContentLeft / lodFactor,
                this.ContentTop / lodFactor,
                this.ContentWidth / lodFactor,
                this.ContentHeight / lodFactor);
        }

        internal void Validate()
        {
            if (this.HorizontalTiles <= 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.HorizontalTiles)} must be greater than zero");
            }

            if (this.VerticalTiles <= 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.VerticalTiles)} must be greater than zero");
            }

            if (this.TileWidth <= 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.TileWidth)} must be greater than zero");
            }

            if (this.TileHeight <= 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.TileHeight)} must be greater than zero");
            }

            if (this.LayerCount <= 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.LayerCount)} must be greater than zero");
            }

            if (this.ContentLeft < 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.ContentLeft)} must be greater than or equal to zero");
            }

            if (this.ContentTop < 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.ContentTop)} must be greater than or equal to zero");
            }

            if (this.ContentWidth <= 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.ContentWidth)} must be greater than zero");
            }

            if (this.ContentHeight <= 0)
            {
                throw new InvalidDimensionsException($"{nameof(this.ContentHeight)} must be greater than zero");
            }
        }
    }
}