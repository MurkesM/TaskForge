using FluentValidation;
using TaskForge.Dtos.Auth;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MinimumLength(20); // base64 tokens are long; this prevents garbage input
    }
}
