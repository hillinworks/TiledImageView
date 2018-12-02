using System;
using System.IO;

namespace Hillinworks.TiledImage.Imaging
{
    partial struct TileIndex
    {
        public struct Full
        {
            public static explicit operator TileIndex(Full index)
            {
                return new TileIndex(index.Column, index.Row);
            }

            public static explicit operator LOD(Full index)
            {
                return new LOD(index.Column, index.Row, index.LODLevel);
            }

            public static explicit operator Layered(Full index)
            {
                return new Layered(index.Column, index.Row, index.Layer);
            }

            public int Row { get; }
            public int Column { get; }
            public int Layer { get; }
            public int LODLevel { get; }

            public Full(int column, int row, int layer, int lodLevel)
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

                if (lodLevel < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(lodLevel));
                }

                this.Column = column;
                this.Row = row;
                this.Layer = layer;
                this.LODLevel = lodLevel;
            }

            public override int GetHashCode()
            {
                return (this.LODLevel << 26) + (this.Layer << 20) + (this.Column << 10) + this.Row;
            }

            public override string ToString()
            {
                return $"{this.Column}, {this.Row}, L{this.Layer}, LOD{this.LODLevel}";
            }

            public string ToPath()
            {
                return Path.Combine(this.LODLevel.ToString(), this.Layer.ToString(), this.Row.ToString(),
                    this.Column.ToString());
            }
        }
    }
}