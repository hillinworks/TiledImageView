using System;

namespace Hillinworks.TiledImage.Imaging
{
	partial struct TileIndex
	{
		public struct Layered
		{
			public static explicit operator TileIndex(Layered layeredTileIndex)
			{
				return new TileIndex(layeredTileIndex.Column, layeredTileIndex.Row);
			}

			public int Row { get; }
			public int Column { get; }
			public int Layer { get; }

			public Layered(int column, int row, int layer)
			{
				if (column < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(column));
				}

				if (row < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(row));
				}

				if (layer < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(layer));
				}

				this.Column = column;
				this.Row = row;
				this.Layer = layer;
			}

			public override int GetHashCode()
			{
				return (this.Layer << 24) + (this.Column << 12) + this.Row;
			}

			public override string ToString()
			{
				return $"(L{this.Layer}:{this.Row}-{this.Column})";
			}
		}
	}
}