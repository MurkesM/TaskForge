using FluentValidation;
using TaskForge.Dtos;

public class TaskUpdateValidator : AbstractValidator<UpdateTaskDto>
{
    public TaskUpdateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.IsComplete)
            .NotNull();
    }
}