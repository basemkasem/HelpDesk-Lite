using FluentValidation;
using HelpDeskLite.Application.DTOs.Tickets;
using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.Validators;

public class UpdateTicketStatusRequestValidator : AbstractValidator<UpdateTicketStatusRequestDto>
{
    public UpdateTicketStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<TicketStatus>(s, ignoreCase: true, out _))
            .WithMessage("Status must be a valid ticket status.");
    }
}
