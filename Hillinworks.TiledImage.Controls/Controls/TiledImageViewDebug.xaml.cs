using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Hillinworks.TiledImage.Controls
{
	public partial class TiledImageViewDebug
	{
		public TiledImageViewDebug()
		{
			this.InitializeComponent();

			new DispatcherTimer(
				TimeSpan.FromMilliseconds(33),
				DispatcherPriority.DataBind,
				this.Tick,
				this.Dispatcher).Start();
		}

		private TiledImageView TiledImageView
			=> this.DataContext as TiledImageView;

		private void Tick(object sender, EventArgs e)
		{
			var tiledImageView = this.TiledImageView;
			if (tiledImageView == null)
			{
				return;
			}

			LODText.Text = tiledImageView.ViewState == null
				? "LOD: 0 @ 0x"
				: $"LOD: {tiledImageView.ViewState.LODLevel} @ {tiledImageView.ViewState.ViewToLODScale:0.#}x";

			var tileCount = tiledImageView.TilesManager?.Tiles.Length ?? 0;
			var loadProgress = tiledImageView.TilesManager?.Tiles.Sum(t => t.LoadTask.LoadProgress) / tileCount * 100 ?? 100;
			TilesText.Text = $"Tiles: {tileCount}, {loadProgress:0.#}% loaded";
		}
	}
}