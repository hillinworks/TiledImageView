using System;
using System.Threading;

namespace Hillinworks.TiledImage.Imaging.Sources
{
    partial class WebImageSource
    {
        private partial class DownloadImageTask
        {
            private class Observer : IDisposable
            {
                public IProgress<double> Progress { get; }
                public CancellationToken CancellationToken { get; }

                public CancellationTokenRegistration CancellationTokenRegistration { get; set; }

                public Observer(IProgress<double> progress, CancellationToken cancellationToken)
                {
                    this.Progress = progress;
                    this.CancellationToken = cancellationToken;
                }

                public void Dispose()
                {
                    this.CancellationTokenRegistration.Dispose();
                }
            }
        }
    }
}