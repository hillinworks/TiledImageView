using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Hillinworks.TiledImage.Imaging;
using Hillinworks.TiledImage.Utilities;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		/// <summary>
		///     Binary (quad-tree) tile culler to detect which tiles are visible in the specified CullRect
		/// </summary>
		private class TileCuller
		{
			private enum Intersection
			{
				Contain,
				Intersect,
				NotIntersect
			}

			public TileCuller(TiledImageView owner, Rect cullRect)
			{
				this.Owner = owner;
				this.CullRect = cullRect;
				this.WorldCullRectVertices = this.ViewState.ViewToWorldMatrix.TransformVertices(this.CullRect);
			}

			private TiledImageView Owner { get; }
			private Rect CullRect { get; }
			private Point[] WorldCullRectVertices { get; }
			private ImageViewState ViewState => this.Owner.ViewState;

			public IEnumerable<TileIndex> GetVisibleTiles()
			{
				var tiles = new List<TileIndex>();
				this.CullRecursive(
					new Int32Rect(0, 0, this.ViewState.LODDimensions.HorizontalTiles, this.ViewState.LODDimensions.VerticalTiles),
					tiles);

				return tiles;
			}

			[SuppressMessage("ReSharper", "TailRecursiveCall")]
			private void CullRecursive(Int32Rect indexRegion, List<TileIndex> tiles)
			{
				var tileWidth = this.ViewState.LODDimensions.TileWidth * this.ViewState.LODToWorldScale;
				var tileHeight = this.ViewState.LODDimensions.TileHeight * this.ViewState.LODToWorldScale;
				var region = new Rect(
					indexRegion.X * tileWidth,
					indexRegion.Y * tileHeight,
					indexRegion.Width * tileWidth,
					indexRegion.Height * tileHeight
				);

				switch (this.CheckIntersection(region))
				{
					case Intersection.Contain:
						for (var row = indexRegion.Y; row < indexRegion.Y + indexRegion.Height; ++row)
						{
							for (var column = indexRegion.X; column < indexRegion.X + indexRegion.Width; ++column)
							{
								tiles.Add(new TileIndex(column, row));
							}
						}
						return;
					case Intersection.Intersect:
						var halfWidth1 = indexRegion.Width / 2;
						var halfWidth2 = indexRegion.Width - halfWidth1;
						var halfHeight1 = indexRegion.Height / 2;
						var halfHeight2 = indexRegion.Height - halfHeight1;

						if (indexRegion.Width == 1)
						{
							if (indexRegion.Height == 1)
							{
								tiles.Add(new TileIndex(indexRegion.X, indexRegion.Y));
							}
							else
							{
								this.CullRecursive(
									new Int32Rect(
										indexRegion.X,
										indexRegion.Y,
										indexRegion.Width,
										halfHeight1),
									tiles);

								this.CullRecursive(
									new Int32Rect(
										indexRegion.X,
										indexRegion.Y + halfHeight1,
										indexRegion.Width,
										halfHeight2),
									tiles);
							}
						}
						else
						{
							if (indexRegion.Height == 1)
							{
								this.CullRecursive(
									new Int32Rect(
										indexRegion.X,
										indexRegion.Y,
										halfWidth1,
										indexRegion.Height),
									tiles);
								this.CullRecursive(
									new Int32Rect(
										indexRegion.X + halfWidth1,
										indexRegion.Y + halfHeight1,
										halfWidth2,
										indexRegion.Height),
									tiles);
							}
							else
							{
								this.CullRecursive(
									new Int32Rect(
										indexRegion.X,
										indexRegion.Y,
										halfWidth1,
										halfHeight1),
									tiles);
								this.CullRecursive(
									new Int32Rect(
										indexRegion.X + halfWidth1,
										indexRegion.Y, halfWidth2,
										halfHeight1),
									tiles);
								this.CullRecursive(
									new Int32Rect(
										indexRegion.X,
										indexRegion.Y + halfHeight1,
										halfWidth1, halfHeight2),
									tiles);
								this.CullRecursive(
									new Int32Rect(
										indexRegion.X + halfWidth1,
										indexRegion.Y + halfHeight2,
										halfWidth2,
										halfHeight2),
									tiles);
							}
						}
						break;
					case Intersection.NotIntersect:
						return;
				}
			}

			/// <summary>
			///     Check whether target rect, in world space, intersects with the cull rect
			/// </summary>
			private Intersection CheckIntersection(Rect targetRect)
			{
				// This method is a simplified implementation with the knowledge of CullRect being unrotated

				var targetPointsInViewSpace = this.ViewState.WorldToViewMatrix.TransformVertices(targetRect);

				// first check whether any or all vertices of targetRect, transformed to the view space,
				// is contained by the cull rect
				var contained = false;
				var fullyContained = true;
				foreach (var point in targetPointsInViewSpace)
				{
					if (this.CullRect.Contains(point))
					{
						contained = true;
						if (!fullyContained)
						{
							return Intersection.Intersect;
						}
					}
					else
					{
						fullyContained = false;
					}
				}

				if (fullyContained)
				{
					return Intersection.Contain;
				}

				if (contained)
				{
					return Intersection.Intersect;
				}

				// then check if any of the vertices in the cull rect, transformed to world space,
				// is contained by the target rect
				if (this.WorldCullRectVertices.Any(targetRect.Contains))
				{
					return Intersection.Intersect;
				}

				// however we still have the third case, where the two rects intersects with each
				// other but do not contain each other's vertices. By checking if the center of the
				// target rect, transformed to view space, is contained by the cull rect, we can 
				// rule this case out.
				var targetCenterInViewSpace = new Point(
					(targetPointsInViewSpace[0].X + targetPointsInViewSpace[2].X) / 2,
					(targetPointsInViewSpace[0].Y + targetPointsInViewSpace[2].Y) / 2);

				if (this.CullRect.Contains(targetCenterInViewSpace))
				{
					return Intersection.Intersect;
				}

				return Intersection.NotIntersect;
			}
		}
	}
}