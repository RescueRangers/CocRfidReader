using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CocRfidReader.WPF.Models
{
    public class EmailAddress : ObservableObject, IEqualityComparer<EmailAddress>
    {
        private string address;

        public string Address
        {
            get => address; 
            set
            {
                address = value;
                OnPropertyChanged();
            }
        }

        public EmailAddress(string address)
        {
            Address = address;
        }

        public override bool Equals(object? obj)
        {
            if (obj is EmailAddress address)
            {
                return Equals(this, address);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public bool Equals(EmailAddress? x, EmailAddress? y)
        {
            if (x == null || y == null) return false;
            return string.Equals(x.address, y.address, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] EmailAddress obj)
        {
            return address.GetHashCode();
        }
    }
}
