using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hillinworks.TiledImage.Imaging;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		internal class ImageTilesManager
		{
			private Lazy<TileRenderInfo[]> _lazyTiles = new Lazy<TileRenderInfo[]>(() => null);
			private Dictionary<TileIndex.Full, TileRenderInfo> _tilesMap = new Dictionary<TileIndex.Full, TileRenderInfo>();

			public ImageTilesManager(TiledImageView owner)
			{
				this.Owner = owner;

				this.UpdateTiles();
			}

			public TileRenderInfo[] Tiles => _lazyTiles.Value;

			private Dictionary<TileIndex.Full, TileRenderInfo> TilesMap
			{
				get => _tilesMap;
				set
				{
					_tilesMap = value;
					_lazyTiles = new Lazy<TileRenderInfo[]>(() => _tilesMap.Values.ToArray());
				}
			}

			private TiledImageView Owner { get; }
			private ImageViewState ViewState => this.Owner.ViewState;
			private TiledImageSource TiledImage => this.Owner.Source;


			public void UpdateTiles()
			{
				var updateRequests = new List<UpdateTileRequest>();

				var culler = new TileCuller(this.Owner, new Rect(0, 0, this.Owner.ActualWidth, this.Owner.ActualHeight));

				var worldTileWidth = this.ViewState.LODDimensions.TileWidth * this.ViewState.LODToWorldScale;
				var worldTileHeight = this.ViewState.LODDimensions.TileHeight * this.ViewState.LODToWorldScale;

				foreach (var tile in culler.GetVisibleTiles())
				{
					var tileIndex = tile.ToFull(this.ViewState.Layer, this.ViewState.LODLevel);
					var worldRect = new Rect(
						tile.Column * worldTileWidth,
						tile.Row * worldTileHeight,
						worldTileWidth,
						worldTileHeight);

					updateRequests.Add(new UpdateTileRequest(tileIndex, worldRect));
				}

				var focalComparer = new UpdateTileRequest.FocalComparer(
					this.Owner.ViewState.ViewToWorldMatrix.Transform(Mouse.GetPosition(this.Owner)));
				updateRequests.Sort(focalComparer);

				var newTiles = new Dictionary<TileIndex.Full, TileRenderInfo>();
				foreach (var request in updateRequests)
				{
					if (!newTiles.TryGetValue(request.TileIndex, out var renderInfo))
					{
						LoadTileTask reusedLoadTask = null;
						if (this.TilesMap.TryGetValue(request.TileIndex, out var reusedRenderInfo))
						{
							reusedLoadTask = reusedRenderInfo.LoadTask;
						}

						renderInfo = this.CreateTileRenderInfo(request.TileIndex, reusedLoadTask);
					}

					renderInfo.Regions.Add(request.WorldRect);

					newTiles[request.TileIndex] = renderInfo;
				}

				foreach (var pair in this.TilesMap)
				{
					if (!newTiles.ContainsKey(pair.Key))
					{
						this.DisposeLoadTileTask(pair.Value.LoadTask);
					}
				}

				this.TilesMap = newTiles;
				this.OnContentChanged();
			}

			/// <remarks>
			///     Specify loadTask to reuse a LoadTileTask. The task will be restarted if it's not alive.
			/// </remarks>
			private TileRenderInfo CreateTileRenderInfo(TileIndex.Full tileIndex, LoadTileTask loadTask = null)
			{
				if (loadTask == null)
				{
					loadTask = new LoadTileTask(tileIndex);
					loadTask.LoadStateChanged += this.OnLoadTileTaskStateChanged;
					this.TiledImage.BeginLoadTileAsync(loadTask);
				}
				else if (!loadTask.Status.IsAlive())
				{
					loadTask.Reset();
					this.TiledImage.BeginLoadTileAsync(loadTask);
				}

				return new TileRenderInfo(loadTask);
			}

			private void DisposeLoadTileTask(LoadTileTask task)
			{
				task.LoadStateChanged -= this.OnLoadTileTaskStateChanged;
				task.Cancel();
			}

			private void OnLoadTileTaskStateChanged(object sender, EventArgs e)
			{
				this.OnContentChanged();
			}

			private void OnContentChanged()
			{
				this.Owner.Dispatcher.BeginInvoke((Action)this.Owner.InvalidateVisual);
			}


			public void UpdateLayer(int layer)
			{
				var newTiles = new Dictionary<TileIndex.Full, TileRenderInfo>();

				foreach (var pair in this.TilesMap)
				{
					var tileIndex = new TileIndex.Full(pair.Key.Column, pair.Key.Row, layer, pair.Key.LODLevel);
					var renderInfo = this.CreateTileRenderInfo(tileIndex);
					renderInfo.Regions.AddRange(pair.Value.Regions);
					newTiles.Add(tileIndex, renderInfo);
					this.DisposeLoadTileTask(pair.Value.LoadTask);
				}

				this.TilesMap = newTiles;
				this.OnContentChanged();
			}
		}
	}
}