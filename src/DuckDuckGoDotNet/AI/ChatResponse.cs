namespace DuckDuckGoDotNet.AI;

public enum Model
{
    Gpt4oMini,
    Llama3370b,
    Claude3Haiku,
    O3Mini,
    MistralSmall3
}
public class ChatResponse
{
    public string Content { get; set; }
    public ChatRole Role { get; set; }

}
