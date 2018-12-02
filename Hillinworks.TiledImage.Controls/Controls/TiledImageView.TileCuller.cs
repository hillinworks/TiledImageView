using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Hillinworks.TiledImage.Imaging;

namespace Hillinworks.TiledImage.Controls
{
    partial class TiledImageView
    {
        /// <summary>
        ///     Binary (quad-tree) tile culler to detect which tiles are visible in the specified CullRect
        /// </summary>
        private class TileCuller : CullerBase
        {
            public TileCuller(TiledImageView owner, Rect cullRect)
            {
                this.Owner = owner;
                this.Context = new CullContext(cullRect, owner.ViewState);
            }

            private CullContext Context { get; }

            private TiledImageView Owner { get; }
            private ImageViewState ViewState => this.Owner.ViewState;

            public IEnumerable<TileIndex> GetVisibleTiles()
            {
                var tiles = new List<TileIndex>();
                this.CullRecursive(
                    new Int32Rect(
                        0, 
                        0, 
                        this.ViewState.LODDimensions.HorizontalTiles,
                        this.ViewState.LODDimensions.VerticalTiles),
                    tiles);

                return tiles;
            }

            [SuppressMessage("ReSharper", "TailRecursiveCall")]
            private void CullRecursive(Int32Rect indexRegion, ICollection<TileIndex> tiles)
            {
                // tile size in world space
                var tileWidth = this.ViewState.LODDimensions.TileWidth * this.ViewState.LODToWorldScale;
                var tileHeight = this.ViewState.LODDimensions.TileHeight * this.ViewState.LODToWorldScale;

                // indexRegion in world space
                var region = new Rect(
                    indexRegion.X * tileWidth,
                    indexRegion.Y * tileHeight,
                    indexRegion.Width * tileWidth,
                    indexRegion.Height * tileHeight
                );

                switch (CheckIntersection(this.Context, region))
                {
                    case Intersection.Contain:

                        // the region is fully contained by current view
                        for (var row = indexRegion.Y; row < indexRegion.Y + indexRegion.Height; ++row)
                        {
                            for (var column = indexRegion.X; column < indexRegion.X + indexRegion.Width; ++column)
                            {
                                tiles.Add(new TileIndex(column, row));
                            }
                        }

                        return;

                    case Intersection.Intersect:

                        // either the region intersects with current view, or it contains current view

                        var halfWidth1 = indexRegion.Width / 2;
                        var halfWidth2 = indexRegion.Width - halfWidth1;
                        var halfHeight1 = indexRegion.Height / 2;
                        var halfHeight2 = indexRegion.Height - halfHeight1;

                        if (indexRegion.Width == 1)
                        {
                            if (indexRegion.Height == 1)
                            {
                                // the indexRegion cannot be subdivided any more
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
                                        indexRegion.Y, 
                                        halfWidth2,
                                        halfHeight1),
                                    tiles);
                                this.CullRecursive(
                                    new Int32Rect(
                                        indexRegion.X,
                                        indexRegion.Y + halfHeight1,
                                        halfWidth1, 
                                        halfHeight2),
                                    tiles);
                                this.CullRecursive(
                                    new Int32Rect(
                                        indexRegion.X + halfWidth1,
                                        indexRegion.Y + halfHeight1,
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
        }
    }
}