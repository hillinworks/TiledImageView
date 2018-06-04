using System.Threading;
using System.Windows.Media.Imaging;

namespace Hillinworks.TiledImage.Imaging
{
	public interface ILoadTileTask
	{
		TileIndex.Full Index { get; }
		CancellationToken CancellationToken { get; }
		void ReportProgress(double progress);
		void OnError(string errorMessage);
		void OnCompleted(BitmapSource bitmap);
		void OnCanceled();
	}
}