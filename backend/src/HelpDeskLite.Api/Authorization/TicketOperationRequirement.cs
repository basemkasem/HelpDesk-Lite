using Microsoft.AspNetCore.Authorization;

namespace HelpDeskLite.Api.Authorization;

public class TicketOperationRequirement : IAuthorizationRequirement
{
    public const string View = "View";
    public string Operation { get; }

    public TicketOperationRequirement(string operation) => Operation = operation;
}
