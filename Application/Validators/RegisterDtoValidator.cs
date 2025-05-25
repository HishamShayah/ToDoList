using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
  
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(50).WithMessage("Email must not exceed 50 characters.")
                .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MaximumLength(256).WithMessage("Password must not exceed 256 characters.");
        }
    }

}
