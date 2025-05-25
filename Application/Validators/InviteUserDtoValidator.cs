using Application.DTOs;
using FluentValidation;

public class InviteUserDtoValidator : AbstractValidator<InviteUserDto>
{
    public InviteUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role == "Guest" || role == "Admin")
            .WithMessage("Role must be either 'Guest' or 'Admin'.");
    }
}
