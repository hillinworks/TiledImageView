using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Hillinworks.TiledImage.Controls.Overlays
{
#if DEBUG
    internal static class PointQuadTree
    {
        internal static class Node
        {
            public static int DebugNodeCount { get; set; }
        }
    }
#endif

    partial class PointQuadTree<T>
    {
        [DebuggerDisplay("QuadTreeNode {DebugDisplayName}: {Element}")]
        public class Node
        {
            internal Node(PointQuadTree<T> tree, Node parent, T element)
            {
                this.Tree = tree;
                this.Parent = parent;
                this.Element = element;
                this.Point = this.Tree.PointGetter(this.Element);
                this.UpdateBounds();

#if DEBUG
                this.DebugId = PointQuadTree.Node.DebugNodeCount++;
#endif
            }

#if DEBUG
            public int DebugId { get; private set; }
            public string DebugQualifiedName { get; private set; }
            public string DebugDisplayName => $"#{this.DebugId} {this.DebugQualifiedName}";
#endif
            public RectangleF Bounds { get; private set; }

            private PointQuadTree<T> Tree { get; }
            public T Element { get; }

            private Node[] SubnodeArray { get; }
                = new Node[4];

            public IEnumerable<Node> Subnodes => this.SubnodeArray.Where(n => n != null);

            public Node Parent { get; }

            public PointF Point { get; }

            [Conditional("DEBUG")]
            internal void SetDebugIdentifier(string relativeIdentifier)
            {
                this.DebugQualifiedName = this.Parent == null
                    ? relativeIdentifier
                    : $"{this.Parent.DebugQualifiedName}.{relativeIdentifier}";
            }

            public Node GetSubnode(Direction direction)
            {
                return this.SubnodeArray[(int)direction];
            }

            private void UpdateBounds()
            {
                var minX = this.Point.X;
                var minY = this.Point.Y;
                var maxX = this.Point.X;
                var maxY = this.Point.Y;
                foreach (var subNode in this.SubnodeArray)
                {
                    if (subNode == null)
                    {
                        continue;
                    }

                    var bounds = subNode.Bounds;
                    minX = Math.Min(bounds.Left, minX);
                    minY = Math.Min(bounds.Top, minY);
                    maxX = Math.Max(bounds.Right, maxX);
                    maxY = Math.Max(bounds.Bottom, maxY);
                }

                this.Bounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }

            public void Insert(T item)
            {
                this.InternalInsert(item);

                this.UpdateBounds();
            }

            private void InternalInsert(T item)
            {
                var direction = this.GetSubnodeDirection(item);
                var index = (int)direction;

                if (this.SubnodeArray[index] == null)
                {
                    var subnode = new Node(this.Tree, this, item);
                    subnode.SetDebugIdentifier(direction.ToString());
                    this.SubnodeArray[index] = subnode;
                }
                else
                {
                    this.SubnodeArray[index].Insert(item);
                }
            }

            public IEnumerable<T> Collect(RectangleF rect)
            {
                var items = new List<T>();
                this.CollectRecursive(items, rect, null);

                return items;
            }

            private void CollectRecursive(
                ICollection<T> items,
                RectangleF rect,
                Direction? anchor)
            {
                Direction? collectedAnchor = null;
                if (rect.Contains(this.Point))
                {
                    items.Add(this.Element);

                    if (anchor != null)
                    {
                        this.SubnodeArray[(int)anchor.Value]?.CollectAll(items);
                        collectedAnchor = anchor;
                    }
                }

                if (rect.Left < this.Point.X)
                {
                    if (rect.Top < this.Point.Y && collectedAnchor != Direction.Northwest)
                    {
                        this.SubnodeArray[(int)Direction.Northwest]?
                            .CollectRecursive(items, rect, Direction.Southeast);
                    }

                    if (rect.Bottom > this.Point.Y && collectedAnchor != Direction.Southwest)
                    {
                        this.SubnodeArray[(int)Direction.Southwest]?
                            .CollectRecursive(items, rect, Direction.Northeast);
                    }
                }

                if (rect.Right > this.Point.X)
                {
                    if (rect.Top < this.Point.Y && collectedAnchor != Direction.Northeast)
                    {
                        this.SubnodeArray[(int)Direction.Northeast]?
                            .CollectRecursive(items, rect, Direction.Southwest);
                    }

                    if (rect.Bottom > this.Point.Y && collectedAnchor != Direction.Southeast)
                    {
                        this.SubnodeArray[(int)Direction.Southeast]?
                            .CollectRecursive(items, rect, Direction.Northwest);
                    }
                }
            }

            private void CollectAll(ICollection<T> items)
            {
                items.Add(this.Element);

                foreach (var subnode in this.SubnodeArray)
                {
                    subnode?.CollectAll(items);
                }
            }

            private Direction GetSubnodeDirection(T item)
            {
                var point = this.Tree.PointGetter(item);
                return (point.X < this.Point.X ? Direction.West : Direction.East)
                       | (point.Y < this.Point.Y ? Direction.North : Direction.South);
            }

            public IEnumerable<T> EnumerateDescendants()
            {
                foreach (var subnode in this.SubnodeArray)
                {
                    if (subnode == null)
                    {
                        continue;
                    }

                    yield return subnode.Element;

                    foreach (var descendant in subnode.EnumerateDescendants())
                    {
                        yield return descendant;
                    }
                }
            }

            public IEnumerable<Node> EnumerateDescendantNodes()
            {
                foreach (var subnode in this.SubnodeArray)
                {
                    if (subnode == null)
                    {
                        continue;
                    }

                    yield return subnode;

                    foreach (var descendant in subnode.EnumerateDescendantNodes())
                    {
                        yield return descendant;
                    }
                }
            }

            public bool Remove(T item)
            {
                Debug.Assert(!Equals(item, this.Element));

                var direction = this.GetSubnodeDirection(item);
                var subnode = this.GetSubnode(direction);
                if (subnode == null)
                {
                    return false;
                }

                if (Equals(item, subnode.Element))
                {
                    this.SubnodeArray[(int)direction] = null;

                    foreach (var descendant in subnode.EnumerateDescendants())
                    {
                        this.InternalInsert(descendant);
                    }

                    this.UpdateBounds();
                    return true;
                }

                var result = subnode.Remove(item);
                if (result)
                {
                    this.UpdateBounds();
                }

                return result;
            }
        }
    }
}