namespace DuckDuckGoDotNet.AI;

public static class Extentions
{
    public static string ToRole(this ChatRole role)
    {
        return role switch
        {
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            _ => throw new NotImplementedException()
        };
    }
    public static string ToModelName(this Model model)
    {
        string MistralSmall3 = "mistralai/Mistral-Small-24B-Instruct-2501";
        string O3Mini = "o3-mini";
        string Claude3Haiku = "claude-3-haiku-20240307";
        string Llama3370b = "meta-llama/Llama-3.3-70B-Instruct-Turbo";
        string Gpt4oMini = "gpt-4o-mini";
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