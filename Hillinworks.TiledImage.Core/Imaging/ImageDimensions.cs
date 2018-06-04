using System;

namespace Hillinworks.TiledImage.Imaging
{
	public struct ImageDimensions
	{
		public int HorizontalTiles { get; }
		public int VerticalTiles { get; }
		public int TileWidth { get; }
		public int TileHeight { get; }
		public int LayerCount { get; }
		public int Width => this.HorizontalTiles * this.TileWidth;
		public int Height => this.VerticalTiles * this.TileHeight;
		public int TileCount => this.HorizontalTiles * this.VerticalTiles;
		public int LayeredTileCount => this.HorizontalTiles * this.VerticalTiles * this.LayerCount;

		public ImageDimensions(
			int horizontalTiles,
			int verticalTiles,
			int tileWidth,
			int tileHeight,
			int layerCount)
		{
			this.HorizontalTiles = horizontalTiles;
			this.VerticalTiles = verticalTiles;
			this.TileWidth = tileWidth;
			this.TileHeight = tileHeight;
			this.LayerCount = layerCount;
		}

		public ImageDimensions AtMipLevel(int mipLevel)
		{
			var mipFactor = (int) Math.Pow(2, mipLevel);
			return new ImageDimensions(
				this.HorizontalTiles / mipFactor,
				this.VerticalTiles / mipFactor,
				this.TileWidth,
				this.TileHeight,
				this.LayerCount);
		}
	}
}