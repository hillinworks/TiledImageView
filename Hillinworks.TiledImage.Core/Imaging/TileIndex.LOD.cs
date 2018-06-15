using System;

namespace Hillinworks.TiledImage.Imaging
{
	partial struct TileIndex
	{
		public struct LOD
		{
			public static explicit operator TileIndex(LOD index)
			{
				return new TileIndex(index.Column, index.Row);
			}

			public int Row { get; }
			public int Column { get; }
			public int LODLevel { get; }

			public LOD(int column, int row, int lodLevel)
			{
				if (column < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(column));
				}

				if (row < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(row));
				}

				if (lodLevel < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(lodLevel));
				}

				this.Column = column;
				this.Row = row;
				this.LODLevel = lodLevel;
			}

			public override int GetHashCode()
			{
				return (this.LODLevel << 24) + (this.Column << 12) + this.Row;
			}

			public override string ToString()
			{
				return $"({this.Row}-{this.Column} @ LOD{this.LODLevel})";
			}
		}
	}
}