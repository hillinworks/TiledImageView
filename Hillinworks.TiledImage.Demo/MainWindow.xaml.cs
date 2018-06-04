using System.Windows;

namespace Hillinworks.TiledImage.Demo
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();

			TiledImage.TiledImage = new Imaging.TiledImage(new OpenStreetMapLoader());
		}
	}
}