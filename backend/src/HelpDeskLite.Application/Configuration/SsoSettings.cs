namespace HelpDeskLite.Application.Configuration;

public class SsoSettings
{
    public const string SectionName = "Sso";

    public bool Enabled { get; set; }
    public string? Authority { get; set; }
    public string? ClientId { get; set; }
}
