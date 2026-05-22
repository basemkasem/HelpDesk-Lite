using HelpDeskLite.Application.DTOs.Auth;

namespace HelpDeskLite.Application.Features.Auth.Commands;

/// <summary>
/// CQRS preparation: login command and handler contract for future MediatR wiring.
/// </summary>
public record LoginCommand(LoginRequestDto Request);

public interface ILoginCommandHandler
{
    Task<LoginResponseDto> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default);
}
