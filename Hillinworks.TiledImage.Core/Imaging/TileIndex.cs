namespace Hillinworks.TiledImage.Imaging
{
	public partial struct TileIndex
	{
		internal static string GetAlphabetRow(int row)
		{
			return ((char) ('A' + row)).ToString();
		}

		public string AlphabetRow => GetAlphabetRow(this.Row);
		public int Row { get; }
		public int Column { get; }

		public TileIndex(int column, int row)
		{
			this.Column = column;
			this.Row = row;
		}

		public Layered ToLayered(int layer)
		{
			return new Layered(this.Column, this.Row, layer);
		}

		public Full ToFull(int layer, int lodLevel)
		{
			return new Full(this.Column, this.Row, layer, lodLevel);
		}

		public override int GetHashCode()
		{
			return (this.Column << 12) + this.Row;
		}

		public override string ToString()
		{
			return $"({this.AlphabetRow}{this.Column})";
		}
	}
}