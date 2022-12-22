using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CocRfidReader.WPF.Views
{
    /// <summary>
    /// Interaction logic for CocView.xaml
    /// </summary>
    public partial class CocView : UserControl
    {
        private Brush originalBackground;
        private Color highlightColour = Color.FromRgb(50, 171, 70);
        public CocView()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                var origColour = ((SolidColorBrush)MainGrid.Background).Color;
                var newColour = Color.Add(origColour, highlightColour);
                grid.Background = new SolidColorBrush(newColour);
            }
        }

        

        private void MainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                grid.Background = originalBackground;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            originalBackground = MainGrid.Background;
        }
    }
}
