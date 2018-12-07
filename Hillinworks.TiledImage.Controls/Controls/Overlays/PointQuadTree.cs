using System;
using System.Collections.Generic;
using System.Drawing;

namespace Hillinworks.TiledImage.Controls.Overlays
{
    internal partial class PointQuadTree<T>
        where T : class
    {
        public PointQuadTree(Func<T, PointF> pointGetter)
        {
            this.PointGetter = pointGetter;
        }

        private Func<T, PointF> PointGetter { get; }

        public Node RootNode { get; private set; }

        public void Insert(T item)
        {
            if (this.RootNode == null)
            {
                this.RootNode = new Node(this, null, item);
                this.RootNode.SetDebugIdentifier("Root");
            }
            else
            {
                this.RootNode.Insert(item);
            }
        }

        public bool Remove(T item)
        {
            if (this.RootNode == null)
            {
                return false;
            }

            var result = this.RootNode.Remove(item, out var collapse);
            if (collapse)
            {
                this.RootNode = null;
            }

            return result;
        }

        public IEnumerable<T> Collect(RectangleF rect)
        {
            if (this.RootNode == null)
            {
                return new T[0];
            }

            return this.RootNode.Collect(rect);
        }

        public void Clear()
        {
            this.RootNode = null;
        }
    }
}