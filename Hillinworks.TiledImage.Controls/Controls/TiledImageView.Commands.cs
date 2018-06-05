using System.Windows.Input;

namespace Hillinworks.TiledImage.Controls
{
	partial class TiledImageView
	{
		public static RoutedUICommand CentralizeCommand { get; }
			= new RoutedUICommand("Centralize", "TiledImageView.Centralize", typeof(TiledImageView));

		private void InitializeCommandBindings()
		{
			this.CommandBindings.Add(new CommandBinding(CentralizeCommand, this.ExecuteCentralizeCommand));
		}

		private void ExecuteCentralizeCommand(object sender, ExecutedRoutedEventArgs e)
		{
			this.Centralize();
		}
	}
}