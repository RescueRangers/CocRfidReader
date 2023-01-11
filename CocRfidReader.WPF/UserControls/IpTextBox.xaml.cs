using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
    public partial class IpTextBox : UserControl, INotifyPropertyChanged
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

            var control = (IpTextBox)d;
            control.UpdateOctets(ipOctets);
        }

        private void UpdateOctets(string[] octets)
        {
            Octet1.Text = octets[0];
            Octet2.Text = octets[1];
            Octet3.Text = octets[2];
            Octet4.Text = octets[3];
        }

        public static readonly DependencyProperty Octet1Property =
            DependencyProperty.Register
            (
                nameof(Octet1Str),
                typeof(string),
                typeof(IpTextBox),
                new UIPropertyMetadata("", OnOctetChanged)
            );

        public string Octet1Str
        {
            get { return (string)GetValue(Octet1Property); }
            set
            {
                SetValue(Octet1Property, value);
            }
        }
        public static readonly DependencyProperty Octet2Property =
            DependencyProperty.Register
            (
                nameof(Octet2Str),
                typeof(string),
                typeof(IpTextBox),
                new UIPropertyMetadata("", OnOctetChanged)
            );
        public string Octet2Str
        {
            get { return (string)GetValue(Octet2Property); }
            set
            {
                SetValue(Octet2Property, value);
            }
        }
        public static readonly DependencyProperty Octet3Property =
            DependencyProperty.Register
            (
                nameof(Octet3Str),
                typeof(string),
                typeof(IpTextBox),
                new UIPropertyMetadata("", OnOctetChanged)
            );
        public string Octet3Str
        {
            get { return (string)GetValue(Octet3Property); }
            set
            {
                SetValue(Octet3Property, value);
            }
        }
        public static readonly DependencyProperty Octet4Property =
            DependencyProperty.Register
            (
                nameof(Octet4Str),
                typeof(string),
                typeof(IpTextBox),
                new UIPropertyMetadata("", OnOctetChanged)
            );
        public string Octet4Str
        {
            get { return (string)GetValue(Octet4Property); }
            set
            {
                SetValue(Octet4Property, value);
            }
        }

        private static void OnOctetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IpTextBox)d;
            control.UpdateIpAddress();
        }


        public void UpdateIpAddress()
        {
            IpAddress = $"{Octet1Str}.{Octet2Str}.{Octet3Str}.{Octet4Str}";
        }

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set
            {
                SetValue(IpAddressProperty, value);
            }
        }

        private bool dontSelectAll = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IpTextBox()
        {
            InitializeComponent();
        }

        #region Events
        private void Octet_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod || e.Key == Key.Enter)
            {
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                textBox.MoveFocus(request);
                e.Handled = true;
            }
            if (e.Key == Key.Back)
            {
                if (textBox.SelectionStart == 0)
                {
                    var request = new TraversalRequest(FocusNavigationDirection.Previous);
                    request.Wrapped = true;
                    dontSelectAll = true;
                    textBox.MoveFocus(request);
                    e.Handled = true;
                }
                else
                    e.Handled = false;
            }
            else if (textBox.Text.Length >= 3 
                && textBox.SelectedText.Length == 0 
                && textBox.Name != "Octet4")
            {
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                textBox.MoveFocus(request);
                e.Handled = false;
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
            var box = sender as TextBox;
            if (dontSelectAll)
            {
                dontSelectAll = false;
                box.SelectionStart = box.Text.Length;
                e.Handled = true;
                return;
            }
            box.SelectAll();
            e.Handled = true;
        }
        #endregion
    }
}
