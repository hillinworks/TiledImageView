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

		/// <summary>
		///     Width of left and right margin (blank space before and after content) in pixels
		/// </summary>
		public int HorizontalMargin { get; }

		/// <summary>
		///     Height of left and right margin (white space before and after content) in pixels
		/// </summary>
		public int VerticalMargin { get; }

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
		public int ContentWidth => this.Width - this.HorizontalMargin * 2;

		/// <summary>
		///     Height of the entire image with the blank space trimmed, in pixels
		/// </summary>
		public int ContentHeight => this.Height - this.VerticalMargin * 2;

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
			int horizontalMargin,
			int verticalMargin)
		{
			this.HorizontalTiles = horizontalTiles;
			this.VerticalTiles = verticalTiles;
			this.TileWidth = tileWidth;
			this.TileHeight = tileHeight;
			this.LayerCount = layerCount;
			this.HorizontalMargin = horizontalMargin;
			this.VerticalMargin = verticalMargin;
			this.Validate();
		}

		public Dimensions AtLODLevel(int lodLevel)
		{
			var lodFactor = (int) Math.Pow(2, lodLevel);
			return new Dimensions(
				this.HorizontalTiles / lodFactor,
				this.VerticalTiles / lodFactor,
				this.TileWidth,
				this.TileHeight,
				this.LayerCount,
				this.HorizontalMargin / lodFactor,
				this.VerticalMargin / lodFactor);
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

			if (this.HorizontalMargin < 0)
			{
				throw new InvalidDimensionsException($"{nameof(this.HorizontalMargin)} must be greater than or equal to zero");
			}

			if (this.VerticalMargin < 0)
			{
				throw new InvalidDimensionsException($"{nameof(this.VerticalMargin)} must be greater than or equal to zero");
			}
		}
	}
}