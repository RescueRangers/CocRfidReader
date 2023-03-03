using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace CocRfidReader.WPF.Services
{
    public class WpfMessagingService : IMessagingService
    {
        public void DisplayMessage(string message, MessageType type)
        {
            MessageBoxImage mboxImage;
            string caption;

            switch (type)
            {
                case MessageType.Info:
                    mboxImage = MessageBoxImage.Information;
                    caption = "Information";
                    break;
                case MessageType.Error:
                    mboxImage = MessageBoxImage.Error;
                    caption = "Error";
                    break;
                default:
                    caption = "Coś poszło nie tak";
                    mboxImage = MessageBoxImage.Warning;
                    break;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, mboxImage);
            }));
        }
    }
}
