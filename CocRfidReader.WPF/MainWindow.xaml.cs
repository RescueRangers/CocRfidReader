using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CocRfidReader.WPF.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace CocRfidReader.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isFullScreen = false;

        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            PackingListTextBox.Focus();
            GoFullScreen = new RelayCommand(ToggleFullScreen);

            InputBindings.Add(new InputBinding(GoFullScreen, new KeyGesture(Key.F11)));
        }

        public ICommand GoFullScreen { get; }

        private bool IsDigit(string text)
        {
            return int.TryParse(text, out var _);
        }

        private void PackingListTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsDigit(e.Text))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        public void ToggleFullScreen()
        {
            if (isFullScreen)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                isFullScreen = false;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                isFullScreen = true;
            }
        }
    }
}
