using System.Windows;
using CocRfidReader.WPF.ViewModels;

namespace CocRfidReader.WPF.Views
{
    /// <summary>
    /// Interaction logic for AddAccountView.xaml
    /// </summary>
    public partial class AddAccountView : Window
    {
        public AddAccountView(AccountViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
