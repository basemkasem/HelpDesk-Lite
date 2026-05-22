using HelpDeskLite.Application.Interfaces;

namespace HelpDeskLite.Infrastructure.Authentication;

/// <summary>
/// SSO preparation stub. Replace with real OIDC/SAML provider integration later.
/// </summary>
public class ExternalAuthProviderStub : IExternalAuthProvider
{
    public Task<ExternalUserInfo?> ValidateExternalTokenAsync(string token, CancellationToken cancellationToken = default) =>
        Task.FromResult<ExternalUserInfo?>(null);
}
