namespace DuckDuckGoDotNet.AI;
public class Models
{
    private static readonly string Gpt4oMini = "gpt-4o-mini";
    private static readonly string Llama3370b = "meta-llama/Llama-3.3-70B-Instruct-Turbo";
    private static readonly string Claude3Haiku = "claude-3-haiku-20240307";
    private static readonly string O3Mini = "o3-mini";
    private static readonly string MistralSmall3 = "mistralai/Mistral-Small-24B-Instruct-2501";
    public static string GetModel(Model model)
    {
        return model switch
        {
            Model.Gpt4oMini => Gpt4oMini,
            Model.Llama3370b => Llama3370b,
            Model.Claude3Haiku => Claude3Haiku,
            Model.O3Mini => O3Mini,
            Model.MistralSmall3 => MistralSmall3,
            _ => Gpt4oMini // Default fallback
        };
    }
}
public enum Model
{
    Gpt4oMini,
    Llama3370b,
    Claude3Haiku,
    O3Mini,
    MistralSmall3
}