using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Hillinworks.TiledImage.Imaging
{
	public class LoadTileTask : ILoadTileTask
	{
		public LoadTileTask(TileIndex.Full index)
		{
			this.Index = index;
			this.Reset();
		}

		private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
		public BitmapSource Bitmap { get; private set; }
		public LoadTileStatus Status { get; private set; }
		public double LoadProgress { get; private set; }
		public string ErrorMessage { get; private set; }

		public TileIndex.Full Index { get; }

		CancellationToken ILoadTileTask.CancellationToken => this.CancellationTokenSource.Token;

		void ILoadTileTask.ReportProgress(double progress)
		{
			Debug.Assert(this.Status.IsAlive());
			this.LoadProgress = progress;
			this.OnLoadStateChanged();
		}

		void ILoadTileTask.OnError(string errorMessage)
		{
			Debug.Assert(this.Status == LoadTileStatus.Loading);
			this.Status = LoadTileStatus.Failed;
			this.ErrorMessage = errorMessage;
			this.LoadProgress = 0.0;
			this.OnLoadStateChanged();
		}

		void ILoadTileTask.OnCompleted(BitmapSource bitmap)
		{
			Debug.Assert(this.Status == LoadTileStatus.Loading);
			this.Status = LoadTileStatus.Succeed;
			this.Bitmap = bitmap;
			this.LoadProgress = 1.0;
			this.OnLoadStateChanged();
		}

		void ILoadTileTask.OnCanceled()
		{
			Debug.Assert(this.Status == LoadTileStatus.Loading);
			this.Status = LoadTileStatus.Canceled;
			this.LoadProgress = 0.0;
			this.OnLoadStateChanged();
		}

		public event EventHandler LoadStateChanged;

		public void Cancel()
		{
			this.CancellationTokenSource.Cancel();
		}

		public void Reset()
		{
			this.Status = LoadTileStatus.Loading;
			this.LoadProgress = 0.0;
			this.ErrorMessage = null;
			this.Bitmap = null;
		}

		private void OnLoadStateChanged()
		{
			this.LoadStateChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}