namespace DuckDuckGoDotNet;
public class DuckDuckGoSearchException : Exception
{
    public DuckDuckGoSearchException(string message) : base(message) { }
    public DuckDuckGoSearchException(string message, Exception innerException) : base(message, innerException) { }
}
public class RatelimitException : Exception
{
    public RatelimitException(string message) : base(message) { }
    public RatelimitException(string message, Exception innerException) : base(message, innerException) { }
}
public class ConversationLimitException : Exception
{
    public ConversationLimitException(string message) : base(message) { }
    public ConversationLimitException(string message, Exception innerException) : base(message, innerException) { }
}
