using System;
using Hillinworks.TiledImage.Imaging;
using Hillinworks.TiledImage.Imaging.Loaders;

namespace Hillinworks.TiledImage.Demo
{
	public class OpenStreetMapLoader : WebImageLoader
	{
		private const int SideTileCount = 524288; // 2 ^ 19
		private const int TileSideSize = 256;

		public override LODInfo LOD => new LODInfo(0, 19, Math.Pow(2, 3), Math.Pow(2, 19));

		public override Dimensions Dimensions { get; } =
			new Dimensions(SideTileCount, SideTileCount, TileSideSize, TileSideSize, 1, 0, 0);

		protected override string GetTileAddress(TileIndex.Full tileIndex)
		{
			var level = this.LOD.MaxLODLevel - tileIndex.LODLevel;
			return
				$"https://a.tile.openstreetmap.org/{level}/{tileIndex.Column}/{tileIndex.Row}.png";
		}
	}
}