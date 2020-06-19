using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineChat.ViewModels.Validations
{
    public class RegistrationViewModelValidator : AbstractValidator<RegisterationViewModel>
    {
        public RegistrationViewModelValidator()
        {
            RuleFor(vm => vm.Email).NotEmpty().WithMessage("Email cannot be empty");
            RuleFor(vm => vm.Password).NotEmpty().WithMessage("Password cannot be empty");
            RuleFor(vm => vm.FullName).NotEmpty().WithMessage("Full cannot be empty");
            RuleFor(vm => vm.PhoneNo).NotEmpty().WithMessage("Phone cannot be empty");
            RuleFor(vm => vm.City).NotEmpty().WithMessage("City cannot be empty");

        }
       

    }
}
