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

namespace CocRfidReader.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for IpTextBox.xaml
    /// </summary>
    public partial class IpTextBox : UserControl
    {
        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register
            (
                nameof(IpAddress),
                typeof(string),
                typeof(IpTextBox),
                new UIPropertyMetadata(null, OnIpAddressChanged)
            );

        private static void OnIpAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace((string)e.NewValue)) return;
            var ip = (string)e.NewValue;
            var ipOctets = ip.Split('.');
            if(ipOctets.Length != 4) return;
            if(ipOctets.Any(s => s.Length > 3)) return;

            for (int i = 0; i < 4; i++)
            {
                octets[i].Text = ipOctets[i];
            }

        }

        private static List<TextBox> octets = new List<TextBox>();

        public string? IpAddress 
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        public IpTextBox()
        {
            InitializeComponent();
            octets.Add(Octet1);
            octets.Add(Octet2);
            octets.Add(Octet3);
            octets.Add(Octet4);
        }

        private void Octet_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Decimal || e.Key == Key.OemPeriod || e.Key == Key.Enter)
            {
                var textBox = (TextBox)sender;
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                textBox.MoveFocus(request);
                e.Handled = true;
            }
        }

        private void Octet_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);

        }

        private void Octet_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
    }
}
