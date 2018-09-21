using System;

namespace Hillinworks.TiledImage.Controls.Overlays
{
    partial class PointQuadTree<T>
    {
        [Flags]
        public enum Direction
        {
            North = 0 << 1,
            South = 1 << 1,
            West = 0,
            East = 1,

            Northwest = North | West,
            Northeast = North | East,
            Southwest = South | West,
            Southeast = South | East
        }
    }
}