using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.WPF.ViewModels;
using FluentValidation;

namespace CocRfidReader.WPF.Validators
{
    public class AccountValidator : AbstractValidator<AccountViewModel>
    {
        public AccountValidator()
        {
            RuleFor(account => account.AccountNumber).NotEmpty().WithMessage("Numer konta nie może być pusty");
        }
    }
}
