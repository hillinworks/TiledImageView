using System.Collections.Generic;
using System.Windows;
using Hillinworks.TiledImage.Imaging;

namespace Hillinworks.TiledImage.Controls
{
	internal struct UpdateTileRequest
	{
		public class FocalComparer : IComparer<UpdateTileRequest>
		{
			public FocalComparer(Point worldFocalPoint)
			{
				this.WorldFocalPoint = worldFocalPoint;
			}

			public Point WorldFocalPoint { get; }

			public int Compare(UpdateTileRequest x, UpdateTileRequest y)
			{
				return this.GetSquaredDistanceToFocalPoint(x).CompareTo(this.GetSquaredDistanceToFocalPoint(y));
			}

			private double GetSquaredDistanceToFocalPoint(UpdateTileRequest request)
			{
				var center = new Point(request.WorldRect.X + request.WorldRect.Width / 2,
					request.WorldRect.Y + request.WorldRect.Height / 2);


				return (center - this.WorldFocalPoint).LengthSquared;
			}
		}

		public TileIndex.Full TileIndex { get; }
		public Rect WorldRect { get; }

		public UpdateTileRequest(TileIndex.Full tileIndex, Rect worldRect)
		{
			this.TileIndex = tileIndex;
			this.WorldRect = worldRect;
		}
	}
}