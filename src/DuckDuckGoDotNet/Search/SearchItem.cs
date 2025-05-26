using System.Text.Json.Serialization;

namespace DuckDuckGoDotNet.Search;

public class BaseSearchItem
{
    [JsonPropertyName("title")]
    public string Title { get; init; }

}
public class TextSearchItem : BaseSearchItem
{
    [JsonPropertyName("href")]
    public Uri Href { get; init; }
    [JsonPropertyName("description")]
    public string Description { get; init; }
}
public class ImageSearchItem : BaseMediaSearchItem
{
    public Uri Thumbnail { get; init; }
    public string Height { get; init; }
    public string Width { get; init; }
}
public class BaseMediaSearchItem : BaseSearchItem
{
    [JsonPropertyName("image")]
    public Uri Image { get; init; }
    [JsonPropertyName("url")]
    public Uri URL { get; init; }
    [JsonPropertyName("source")]
    public string Source { get; init; }
}
public class NewsSearchItem : BaseMediaSearchItem
{
    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; init; }
    [JsonPropertyName("body")]
    public string Body { get; init; }
}