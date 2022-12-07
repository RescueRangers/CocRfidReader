using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CocRfidReader.Model
{
    public class Coc : IEqualityComparer<Coc>, INotifyPropertyChanged
    {
        private int? pRODUKTIONSNR;
        private string? itemNumber;
        private string? name;
        private string? itemText;

        public int? PRODUKTIONSNR
        {
            get => pRODUKTIONSNR; set
            {
                pRODUKTIONSNR = value;
                RaisePropertyChanged(nameof(PRODUKTIONSNR));
            }
        }
        public string? ItemNumber
        {
            get => itemNumber; set
            {
                itemNumber = value;
                RaisePropertyChanged(nameof(ItemNumber));
            }
        }
        public string? Name
        {
            get => name; set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
        public string? ItemText
        {
            get => itemText; set
            {
                itemText = value;
                RaisePropertyChanged(nameof(ItemText));
            }
        }
        public string? EPC { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Coc coc)
                return Equals(this, coc);
            else return false;
        }

        public bool Equals(Coc? x, Coc? y)
        {
            if (x == null || y == null) return false;
            if (string.Equals(x.EPC, y.EPC, StringComparison.OrdinalIgnoreCase) && x.PRODUKTIONSNR == y.PRODUKTIONSNR)
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] Coc obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            var hash = 17;
            unchecked
            {
                hash *= 23 + (EPC?.GetHashCode() ?? 0);
                hash *= 23 + (PRODUKTIONSNR?.GetHashCode() ?? 0);
            }
            return hash;
        }

        public override string ToString()
        {
            return $"{PRODUKTIONSNR} | {Name} | {ItemNumber} {ItemText}";
        }

        private void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
