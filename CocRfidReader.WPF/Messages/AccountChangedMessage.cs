using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.WPF.ViewModels;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CocRfidReader.WPF.Messages
{
    public class AccountChangedMessage : ValueChangedMessage<AccountViewModel>
    {
        public AccountChangedMessage(AccountViewModel value) : base(value)
        {
        }
    }
}
