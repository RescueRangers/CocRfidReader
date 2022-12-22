using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CocRfidReader.WPF.ViewModels
{
    public class AccountViewModel : ObservableObject
    {
        private string zipCity;
        private string accountName;
        private string accountNumber;

        public string AccountNumber
        {
            get => accountNumber;
            set
            {
                accountNumber = value;
                OnPropertyChanged();
            }
        }
        public string AccountName
        {
            get => accountName;
            set
            {
                accountName = value;
                OnPropertyChanged();
            }
        }
        public string ZipCity 
        { 
            get => zipCity; 
            set 
            {
                zipCity = value;
                OnPropertyChanged();
            } 
        }
    }
}
