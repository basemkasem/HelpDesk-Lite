namespace HelpDeskLite.Domain.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid email or password.")
        : base(message)
    {
    }
}
