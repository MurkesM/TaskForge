using FluentValidation;
using TaskForge.Dtos;

public class AuditQueryParametersValidator : AbstractValidator<AuditQueryParameters>
{
    public AuditQueryParametersValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.AfterId)
            .GreaterThan(0)
            .When(x => x.AfterId.HasValue);
    }
}