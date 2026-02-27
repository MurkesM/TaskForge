using FluentValidation;
using TaskForge.Dtos;

public class NotificationQueryParametersValidator : AbstractValidator<NotificationQueryParameters>
{
    public NotificationQueryParametersValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.AfterId)
            .GreaterThan(0)
            .When(x => x.AfterId.HasValue);
    }
}