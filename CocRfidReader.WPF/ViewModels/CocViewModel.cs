using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CocRfidReader.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CocRfidReader.WPF.ViewModels
{
    public class CocViewModel : ObservableObject, IEqualityComparer<CocViewModel>
    {
        private int? pRODUKTIONSNR;
        private string? itemNumber;
        private string? name;
        private string? itemText;
        private string accountNumber;
        private bool isAccountCorrect;
        private int number;

        public int Number
        {
            get => number; 
            set
            {
                number = value;
                OnPropertyChanged(nameof(number));
            }
        }

        public string AccountNumber
        {
            get => accountNumber;
            set
            {
                accountNumber = value;
                OnPropertyChanged();
            }
        }
        public int? PRODUKTIONSNR
        {
            get => pRODUKTIONSNR;
            set
            {
                pRODUKTIONSNR = value;
                OnPropertyChanged();
            }
        }
        public string? ItemNumber
        {
            get => itemNumber; set
            {
                itemNumber = value;
                OnPropertyChanged();
            }
        }
        public string? Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public string? ItemText
        {
            get => itemText;
            set
            {
                itemText = value;
                OnPropertyChanged();
            }
        }
        public string? EPC { get; set; }
        public bool IsAccountCorrect
        {
            get => isAccountCorrect;
            set
            {
                isAccountCorrect = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }

        public Brush BackgroundBrush
        {
            get
            {
                return IsAccountCorrect ? new SolidColorBrush(Color.FromRgb(51, 255, 51)) : new SolidColorBrush(Color.FromRgb(255, 51, 51));
            }
        }

        public CocViewModel(Coc coc)
        {
            PRODUKTIONSNR = coc.PRODUKTIONSNR;
            ItemNumber = coc.ItemNumber;
            Name = coc.Name;
            ItemText = coc.ItemText;
            EPC = coc.EPC;
            AccountNumber = coc.AccountNumber;
        }


        public override bool Equals(object? obj)
        {
            if (obj != null && obj is CocViewModel coc)
                return Equals(this, coc);
            else return false;
        }

        public bool Equals(CocViewModel? x, CocViewModel? y)
        {
            if (x == null || y == null) return false;
            if (x.PRODUKTIONSNR == y.PRODUKTIONSNR)
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] CocViewModel obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            var hash = 17;
            unchecked
            {
                hash *= 23 + (PRODUKTIONSNR?.GetHashCode() ?? 0);
            }
            return hash;
        }

        public override string ToString()
        {
            return $"{PRODUKTIONSNR} | {Name} | {ItemNumber} {ItemText}";
        }
    }
}
