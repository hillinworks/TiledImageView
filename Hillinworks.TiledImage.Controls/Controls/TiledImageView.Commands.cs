using System.Windows;
using System.Windows.Input;

namespace Hillinworks.TiledImage.Controls
{
    partial class TiledImageView
    {
        /// <summary>
        /// Centralize a point in world coordinate in view.
        /// If the parameter is omitted, the center point of the entire view will be used.
        /// </summary>
        public static RoutedUICommand CentralizeCommand { get; }
            = new RoutedUICommand("Centralize", "TiledImageView.Centralize", typeof(TiledImageView));

        private void InitializeCommandBindings()
        {
            this.CommandBindings.Add(new CommandBinding(CentralizeCommand, this.ExecuteCentralizeCommand));
        }

        private void ExecuteCentralizeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.Centralize(e.Parameter is Point point ? (Point?)point : null);
        }
    }
}