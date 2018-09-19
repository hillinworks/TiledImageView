using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Tronmedi.Collections;

namespace Hillinworks.TiledImage.Controls.Overlays
{
    public partial class OverlayCuller<T>
        : IReadOnlyCollection<T>
            , INotifyCollectionChanged
            , INotifyPropertyChanged
    {
        private PointQuadTree<QuadTreeItem> QuadTree { get; }
            = new PointQuadTree<QuadTreeItem>(i => i.Point);

        private HashSet<T> VisibleItems { get; set; }
            = new HashSet<T>();

        private Dictionary<T, QuadTreeItem[]> QuadTreeItemMap { get; }
            = new Dictionary<T, QuadTreeItem[]>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.VisibleItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public int Count => this.VisibleItems.Count;

        public void Cull(ImageViewState viewState)
        {
            var cullResult = new HashSet<T>(this.InternalCull(viewState));
            var addedItems = new List<T>();
            var removedItems = new List<T>();

            foreach (var item in cullResult)
            {
                if (!this.VisibleItems.Contains(item))
                {
                    addedItems.Add(item);
                }
            }

            foreach (var item in this.VisibleItems)
            {
                if (!cullResult.Contains(item))
                {
                    removedItems.Add(item);
                }
            }

            var countChanged = this.VisibleItems.Count != cullResult.Count;

            this.VisibleItems = cullResult;

            if (countChanged)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
            }

            if (removedItems.Count > 0)
            {
                this.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
            }

            if (addedItems.Count > 0)
            {
                this.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems));
            }
        }


        protected virtual IEnumerable<T> InternalCull(ImageViewState viewState)
        {
            var culler = new QuadTreeCuller(this.QuadTree);

            // because every registered item has been inserted for 4 times (see RegisterItem),
            // we use Distinct() to remove duplicated items.
            return culler.Cull(viewState).Distinct();
        }

        public void RegisterItem(T item, RectangleF bounds)
        {
            if (this.QuadTreeItemMap.ContainsKey(item))
            {
                throw new ArgumentException("item already registered", nameof(item));
            }

            var quadTreeItems = new QuadTreeItem[4];
            quadTreeItems[0] = new QuadTreeItem(item, new PointF(bounds.Left, bounds.Top));
            quadTreeItems[1] = new QuadTreeItem(item, new PointF(bounds.Left, bounds.Bottom));
            quadTreeItems[2] = new QuadTreeItem(item, new PointF(bounds.Right, bounds.Top));
            quadTreeItems[3] = new QuadTreeItem(item, new PointF(bounds.Right, bounds.Bottom));

            foreach (var quadTreeItem in quadTreeItems)
            {
                this.QuadTree.Insert(quadTreeItem);
            }

            this.QuadTreeItemMap[item] = quadTreeItems;
        }

        public void UnregisterItem(T item)
        {
            if (!this.QuadTreeItemMap.TryGetValue(item, out var quadTreeItems))
            {
                throw new ArgumentException("item is not registered", nameof(item));
            }

            this.QuadTreeItemMap.Remove(item);

            foreach (var quadTreeItem in quadTreeItems)
            {
                this.QuadTree.Remove(quadTreeItem);
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            this.CollectionChanged?.Invoke(this, args);
        }
    }
}