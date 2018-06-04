using System.Collections.Generic;
using System.Windows;
using Hillinworks.TiledImage.Imaging;

namespace Hillinworks.TiledImage.Controls
{
    internal class TileRenderInfo
    {
        public List<Rect> Regions { get; } = new List<Rect>();
        public LoadTileTask LoadTask { get; }

        public TileRenderInfo(LoadTileTask loadTask)
	    {
		    this.LoadTask = loadTask;
	    }
		
    }
}
