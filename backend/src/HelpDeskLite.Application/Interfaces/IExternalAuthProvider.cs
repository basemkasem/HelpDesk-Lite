namespace HelpDeskLite.Application.Interfaces;

public interface IExternalAuthProvider
{
    Task<ExternalUserInfo?> ValidateExternalTokenAsync(string token, CancellationToken cancellationToken = default);
}

public record ExternalUserInfo(string SubjectId, string Email, string FullName);
