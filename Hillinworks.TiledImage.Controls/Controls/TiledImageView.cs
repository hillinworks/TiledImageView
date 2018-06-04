using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hillinworks.TiledImage.Imaging;
using Hillinworks.TiledImage.Properties;

namespace Hillinworks.TiledImage.Controls
{
	public partial class TiledImageView : Control
	{
		public static readonly DependencyProperty ImageProperty =
			DependencyProperty.Register(
				"TiledImage",
				typeof(Imaging.TiledImage),
				typeof(TiledImageView),
				new PropertyMetadata(null, OnImageChanged));

		static TiledImageView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TiledImageView),
				new FrameworkPropertyMetadata(typeof(TiledImageView)));
		}

		public Imaging.TiledImage TiledImage
		{
			get => (Imaging.TiledImage)this.GetValue(ImageProperty);
			set => this.SetValue(ImageProperty, value);
		}

		internal ImageViewState ViewState { get; set; }
		internal ImageTilesManager TilesManager { get; set; }

		private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((TiledImageView)d).OnImageChanged((Imaging.TiledImage)e.NewValue);
		}

		private void OnImageChanged(Imaging.TiledImage image)
		{
			if (image == null)
			{
				this.ViewState = null;
				this.TilesManager = null;
			}
			else
			{
				this.ViewState = new ImageViewState(this);
				this.TilesManager = new ImageTilesManager(this);
				this.ViewState.Initialize();
			}

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

			if (this.TiledImage == null || this.TilesManager.Tiles == null)
			{
				return;
			}


			var worldScale = this.ViewState.ViewToWorldScale;

			context.PushTransform(new MatrixTransform(this.ViewState.WorldToViewMatrix));

			//this.RenderGhostImage(context);

			var tiles = this.TilesManager.Tiles;

#if DEBUG
			if (Features.DrawTileStatus)
			{
				var consolasTypeface = new Typeface("Consolas");
				foreach (var tile in tiles)
				{
					if (tile.LoadTask.Status == LoadTileStatus.Succeed)
					{
						continue;
					}

					foreach (var tileRect in tile.Regions)
					{
						var renderRect = tileRect;
						renderRect.Inflate(-5 * worldScale, -5 * worldScale);
						var debugBrush = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
						context.DrawRectangle(Brushes.Transparent, new Pen(debugBrush, 1 * worldScale), renderRect);

						var text = "";
						switch (tile.LoadTask.Status)
						{
							case LoadTileStatus.Loading:
								text = $"{(int)(tile.LoadTask.LoadProgress * 100)}%";
								break;
							case LoadTileStatus.Failed:
								text = $"Failed: {tile.LoadTask.ErrorMessage}";
								break;
							case LoadTileStatus.Canceled:
								text = "Canceled";
								break;
						}

						// WPF does not allow font size larger than 32768em, so we have to scale the text
						// using a transform
						context.PushTransform(new ScaleTransform(worldScale, worldScale));

						var textPosition = new Point(renderRect.X / worldScale + 5, renderRect.Y / worldScale + 5);

						context.DrawText(
							new FormattedText(
								text,
								CultureInfo.CurrentCulture,
								FlowDirection.LeftToRight,
								consolasTypeface,
								18,
								debugBrush),
							textPosition);
						context.Pop();
					}
				}
			}
#endif

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
				else
				{
					foreach (var tileRect in tile.Regions)
					{
						context.DrawRectangle(new SolidColorBrush(Color.FromArgb(0x20, 0, 0, 0)), null, tileRect);
					}
				}
			}

#if DEBUG
			if (Features.DrawImageViewBoundaries)
			{
				context.DrawRectangle(null, new Pen(Brushes.Red, 2 * worldScale), new Rect(this.ViewState.EnvelopSize));
				context.DrawRectangle(null, new Pen(Brushes.Orange, 2 * worldScale), new Rect(this.ViewState.ContentSize));
			}
#endif
			context.Pop();

#if DEBUG
			if (Features.DrawImageViewBoundaries)
			{
				context.DrawRectangle(null, new Pen(Brushes.Black, 2), new Rect(0, 0, this.ActualWidth, this.ActualHeight));
			}
#endif
		}
	}
}