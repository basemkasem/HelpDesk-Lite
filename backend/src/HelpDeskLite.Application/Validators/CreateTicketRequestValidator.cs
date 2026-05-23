using FluentValidation;
using HelpDeskLite.Application.DTOs.Tickets;

namespace HelpDeskLite.Application.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequestDto>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(8000);

        RuleFor(x => x.Priority)
            .IsInEnum();
    }
}
