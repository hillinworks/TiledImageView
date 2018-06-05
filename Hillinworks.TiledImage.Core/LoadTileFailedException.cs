using System;

namespace Hillinworks.TiledImage
{
	public class LoadTileFailedException : Exception
	{
		public LoadTileFailedException(string message)
			: base(message)
		{
		}
	}
}