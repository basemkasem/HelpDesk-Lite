using FluentValidation;
using HelpDeskLite.Application.DTOs.Tickets;

namespace HelpDeskLite.Application.Validators;

public class CreateTicketCommentRequestValidator : AbstractValidator<CreateTicketCommentRequestDto>
{
    public CreateTicketCommentRequestValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
