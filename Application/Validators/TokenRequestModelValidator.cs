using Application.DTOs;
using FluentValidation;

public class TokenRequestModelValidator : AbstractValidator<TokenRequestModel>
{
    public TokenRequestModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 6 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.");
    }
}