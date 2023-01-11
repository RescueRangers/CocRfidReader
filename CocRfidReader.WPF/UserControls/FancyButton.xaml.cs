using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;

namespace CocRfidReader.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for FancyButton.xaml
    /// </summary>
    public partial class FancyButton : UserControl
    {
        public static readonly DependencyProperty ButtonColorProperty = 
            DependencyProperty.Register
            (
                nameof(ButtonColor), 
                typeof(Brush), 
                typeof(FancyButton), 
                new UIPropertyMetadata(Brushes.Red)
            );
        public Brush ButtonColor 
        {
            get { return (Brush)GetValue(ButtonColorProperty); }
            set { SetValue(ButtonColorProperty, value); } 
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register
            (
                nameof(ButtonText),
                typeof(string),
                typeof(FancyButton),
                new UIPropertyMetadata("Connect")
            );
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ConnectCommandProperty =
            DependencyProperty.Register
            (
                nameof(ConnectCommand),
                typeof(IRelayCommand),
                typeof(FancyButton),
                new UIPropertyMetadata(null)
            );
        public IRelayCommand ConnectCommand
        {
            get { return (IRelayCommand)GetValue(ConnectCommandProperty); }
            set { SetValue(ConnectCommandProperty, value); }
        }


        public static readonly DependencyProperty AnimationStartedProperty =
            DependencyProperty.Register
            (
                nameof(AnimationStarted),
                typeof(bool),
                typeof(FancyButton),
                new UIPropertyMetadata(false)
            );
        public bool AnimationStarted
        {
            get { return (bool)GetValue(AnimationStartedProperty); }
            set { SetValue(AnimationStartedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register
            (
                nameof(Connected),
                typeof(bool),
                typeof(FancyButton),
                new UIPropertyMetadata(false)
            );
        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }


        public static readonly DependencyProperty IsImageButtonProperty =
            DependencyProperty.Register
            (
                nameof(IsImageButton),
                typeof(bool),
                typeof(FancyButton),
                new UIPropertyMetadata(false)
            );
        public bool IsImageButton
        {
            get { return (bool)GetValue(IsImageButtonProperty); }
            set { SetValue(IsImageButtonProperty, value); }
        }

        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register
            (
                nameof(ImageSource),
                typeof(string),
                typeof(FancyButton),
                new UIPropertyMetadata("empty.png")
            );

        public object ImageSource
        {
            get
            {
                BitmapImage image = new BitmapImage();

                try
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    image.UriSource = new Uri(ImagePath, UriKind.Relative);
                    image.DecodePixelWidth = 50;
                    image.EndInit();
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }

                return image;
            }
        }
        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        public FancyButton()
        {
            InitializeComponent();
        }
    }
}
