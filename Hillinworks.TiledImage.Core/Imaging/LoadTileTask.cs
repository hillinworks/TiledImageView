using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hillinworks.TiledImage.Imaging.Sources;

namespace Hillinworks.TiledImage.Imaging
{
    public class LoadTileTask
    {
        private LoadTileStatus _status;

        public LoadTileTask(IImageSource imageSource, TileIndex.Full index)
        {
            this.ImageSource = imageSource;
            this.Index = index;

            this.Progress.ProgressChanged += this.Progress_ProgressChanged;
        }

        private CancellationTokenSource CancellationTokenSource { get; }
            = new CancellationTokenSource();

        private Progress<double> Progress { get; }
            = new Progress<double>();

        public Task<BitmapSource> LoadTask { get; private set; }

        public BitmapSource Bitmap { get; private set; }

        public LoadTileStatus Status
        {
            get => _status;
            private set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                this.OnStatusChanged();
            }
        }

        public double LoadProgress { get; private set; }
        public string ErrorMessage { get; private set; }

        public IImageSource ImageSource { get; }
        public TileIndex.Full Index { get; }

        private void Progress_ProgressChanged(object sender, double progress)
        {
            this.LoadProgress = progress;
            this.OnStatusChanged();
        }

        public void BeginLoad()
        {
            this.Status = LoadTileStatus.Loading;
            this.LoadProgress = 0.0;
            this.ErrorMessage = null;
            this.Bitmap = null;

            Task.Run(this.BeginLoadAsync, this.CancellationTokenSource.Token);
        }

        private async Task BeginLoadAsync()
        {
            this.LoadTask = this.ImageSource.LoadTileAsync(this.Index, this.Progress, this.CancellationTokenSource.Token);
            try
            {
                this.Bitmap = await this.LoadTask;
                this.LoadProgress = 1.0;
                this.Status = LoadTileStatus.Succeed;
            }
            catch (OperationCanceledException)
            {
                this.Status = LoadTileStatus.Canceled;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
                this.Status = LoadTileStatus.Failed;
            }
        }

        public void Cancel()
        {
            this.CancellationTokenSource.Cancel();
        }

        public event EventHandler StatusChanged;

        private void OnStatusChanged()
        {
            this.StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}