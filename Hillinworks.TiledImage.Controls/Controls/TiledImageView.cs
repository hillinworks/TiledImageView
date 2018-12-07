using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hillinworks.TiledImage.Controls.Overlays;
using Hillinworks.TiledImage.Imaging;
using Hillinworks.TiledImage.Imaging.Sources;
using Hillinworks.TiledImage.Properties;

namespace Hillinworks.TiledImage.Controls
{
    public partial class TiledImageView : Control
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(IImageSource),
                typeof(TiledImageView),
                new PropertyMetadata(null, OnSourceChanged));

        private static readonly DependencyPropertyKey ExtentSizePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExtentSize),
                typeof(Size),
                typeof(TiledImageView),
                new PropertyMetadata(default(Size)));

        public static readonly DependencyProperty ExtentSizeProperty
            = ExtentSizePropertyKey.DependencyProperty;

        public static readonly DependencyProperty OverlaysProperty =
            DependencyProperty.Register(
                nameof(Overlays),
                typeof(IEnumerable<IOverlay>),
                typeof(TiledImageView),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnOverlaysChanged));

        static TiledImageView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TiledImageView),
                new FrameworkPropertyMetadata(typeof(TiledImageView)));
        }

        public TiledImageView()
        {
            this.InitializeCommandBindings();
            this.IsManipulationEnabled = Features.SupportTouchManipulation;
        }

        public IEnumerable<IOverlay> Overlays
        {
            get => (IEnumerable<IOverlay>)this.GetValue(OverlaysProperty);
            set => this.SetValue(OverlaysProperty, value);
        }

        public Size ExtentSize
        {
            get => (Size)this.GetValue(ExtentSizeProperty);
            internal set => this.SetValue(ExtentSizePropertyKey, value);
        }

        public IImageSource Source
        {
            get => (IImageSource)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        // ViewState and TilesManager are tightly coupled all along with this control together
        // They are only not null if Source is not null
        internal ImageViewState ViewState { get; private set; }

        internal ImageTilesManager TilesManager { get; private set; }

        private static void OnOverlaysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TiledImageView)d).OnOverlaysChanged((IEnumerable<IOverlay>)e.OldValue, (IEnumerable<IOverlay>)e.NewValue);
        }

        private void OnOverlaysChanged(IEnumerable<IOverlay> oldValue, IEnumerable<IOverlay> newValue)
        {
            if (oldValue != null && oldValue is INotifyCollectionChanged oldObservableCollection)
            {
                oldObservableCollection.CollectionChanged -= this.OnOverlayCollectionChanged;
            }

            if (newValue == null)
            {
                return;
            }

            var addedOverlays = oldValue == null ? newValue : newValue.Except(oldValue);
            foreach (var overlay in addedOverlays)
            {
                this.InitializeOverlay(overlay);
            }

            if (newValue is INotifyCollectionChanged newObservableCollection)
            {
                newObservableCollection.CollectionChanged += this.OnOverlayCollectionChanged;
            }
        }

        private void InitializeOverlay(IOverlay overlay)
        {
            overlay.AssociatedView = this;
            if (overlay is OverlayUserControl control)
            {
                control.ImageView = this;
            }
            overlay.OnLayerChanged(this.Layer);
            overlay.OnViewStateChanged(this.ViewState);
        }

        private void OnOverlayCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IOverlay overlay in e.NewItems)
                {
                    this.InitializeOverlay(overlay);
                }
            }
        }

        private void OnViewStateChanged()
        {
            if (this.Overlays != null)
            {
                foreach (var overlay in this.Overlays)
                {
                    overlay.OnViewStateChanged(this.ViewState);
                }
            }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TiledImageView)d).OnSourceChanged((IImageSource)e.NewValue);
        }

        private void OnSourceChanged(IImageSource source)
        {
            if (source == null)
            {
                this.ViewState = null;
                this.TilesManager = null;
            }
            else
            {
                this.ViewState = new ImageViewState(this);
                this.TilesManager = new ImageTilesManager(this);
                this.ViewState.Initialize();
                this.Zoom(source.LOD.InitialZoomLevel, this.CenterPoint);
                this.Centralize();
            }

            this.OnViewStateChanged();

            this.UpdateScrollability();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.UpdateScrollability();
            this.TilesManager?.UpdateTiles();
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            context.DrawRectangle(this.Background, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            if (this.Source == null || this.TilesManager.Tiles == null)
            {
                return;
            }

            var worldScale = this.ViewState.ViewToWorldScale;

            context.PushTransform(new MatrixTransform(this.ViewState.WorldToViewMatrix));

            this.RenderGhostImage(context);

            var tiles = this.TilesManager.Tiles;


            foreach (var tile in tiles)
            {
                if (tile.LoadTask.Status == LoadTileStatus.Succeed)
                {
                    foreach (var tileRect in tile.Regions)
                    {
                        if (Features.CompensateForTileGaps)
                        {
                            tileRect.Inflate(worldScale * 0.5, worldScale * 0.5);
                        }

                        context.DrawImage(tile.LoadTask.Bitmap, tileRect);
                    }
                }
                else if (Features.DarkenLoadingTiles)
                {
                    foreach (var tileRect in tile.Regions)
                    {
                        context.DrawRectangle(new SolidColorBrush(Color.FromArgb(0x20, 0, 0, 0)), null, tileRect);
                    }
                }
            }


#if DEBUG
            if (Features.DrawTileInfo)
            {
                var consolasTypeface = new Typeface("Consolas");
                foreach (var tile in tiles)
                {
                    foreach (var tileRect in tile.Regions)
                    {
                        var renderRect = tileRect;
                        renderRect.Inflate(-5 * worldScale, -5 * worldScale);
                        var debugBrush = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
                        context.DrawRectangle(Brushes.Transparent, new Pen(debugBrush, 1 * worldScale), renderRect);

                        var builder = new StringBuilder();

                        builder.AppendLine(tile.LoadTask.Index.ToString());
                        switch (tile.LoadTask.Status)
                        {
                            case LoadTileStatus.Loading:
                                builder.AppendLine($"{(int)(tile.LoadTask.LoadProgress * 100)}%");
                                break;
                            case LoadTileStatus.Failed:
                                builder.AppendLine($"Failed: {tile.LoadTask.ErrorMessage}");
                                break;
                            case LoadTileStatus.Canceled:
                                builder.AppendLine("Canceled");
                                break;
                        }

                        // WPF does not allow font size larger than 32768em, so we have to scale the text
                        // using a transform
                        context.PushTransform(new ScaleTransform(worldScale, worldScale));

                        var textPosition = new Point(renderRect.X / worldScale + 5, renderRect.Y / worldScale + 5);

                        context.DrawText(
                            new FormattedText(
                                builder.ToString(),
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                consolasTypeface,
                                12,
                                debugBrush),
                            textPosition);
                        context.Pop();
                    }
                }
            }
#endif

            if (this.Overlays != null)
            {
                foreach (var overlay in this.Overlays)
                {
                    overlay.Render(context);
                }
            }

            context.Pop();
        }
    }
}