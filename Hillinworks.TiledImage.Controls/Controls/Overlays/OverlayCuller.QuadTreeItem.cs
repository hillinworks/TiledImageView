using System.Drawing;

namespace Hillinworks.TiledImage.Controls.Overlays
{
    partial class OverlayCuller<T>
    {
        private class QuadTreeItem
        {
            public PointF Point { get; }
            public T Element { get; }

            public QuadTreeItem(T element, PointF point)
            {
                this.Element = element;
                this.Point = point;
            }

            public override string ToString()
            {
                return this.Element.ToString();
            }

            public override int GetHashCode()
            {
                return this.Element.GetHashCode();
            }
        }
    }
}
