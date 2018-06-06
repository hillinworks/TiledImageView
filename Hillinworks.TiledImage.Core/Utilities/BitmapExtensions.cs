using System.IO;

// ReSharper disable once CheckNamespace
namespace System.Windows.Media.Imaging
{
	public static class BitmapExtensions
	{
		public static void SaveAsPng(this BitmapSource bitmap, string path)
		{
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmap));

			using (var fileStream = new FileStream(path, FileMode.Create))
			{
				encoder.Save(fileStream);
			}
		}
	}
}