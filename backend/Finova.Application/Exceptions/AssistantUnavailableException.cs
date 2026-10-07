namespace Finova.Application.Exceptions;

public class AssistantUnavailableException : Exception
{
    public AssistantUnavailableException(string message) : base(message) { }
    public AssistantUnavailableException(string message, Exception inner) : base(message, inner) { }
}
