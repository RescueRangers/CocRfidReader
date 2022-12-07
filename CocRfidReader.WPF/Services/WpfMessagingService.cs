using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

            MessageBox.Show(message, caption, MessageBoxButton.OK, mboxImage);
        }
    }
}
