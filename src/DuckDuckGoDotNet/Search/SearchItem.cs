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
public class VideoSearchItem : BaseSearchItem
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("duration")]
    public string Duration { get; set; }
    [JsonPropertyName("embed_html")]
    public string EmbedHtml { get; set; }
    [JsonPropertyName("embed_url")]
    public string EmbedUrl { get; set; }
    [JsonPropertyName("image_token")]
    public string ImageToken { get; set; }
    [JsonPropertyName("provider")]
    public string Provider { get; set; }
    [JsonPropertyName("published")]
    public DateTime Published { get; set; }
    [JsonPropertyName("publisher")]
    public string Publisher { get; set; }
    [JsonPropertyName("statistics")]
    public Statistics Statistics { get; set; }
    [JsonPropertyName("uploader")]
    public string Uploader { get; set; }
    [JsonPropertyName("images")]
    public Images Images { get; set; }
}

public class Images
{
    [JsonPropertyName("large")]
    public Uri Large { get; set; }
    [JsonPropertyName("medium")]
    public Uri Medium { get; set; }
    [JsonPropertyName("motion")]
    public Uri Motion { get; set; }
    [JsonPropertyName("small")]
    public Uri Small { get; set; }
}

public class Statistics
{
    [JsonPropertyName("viewCount")]
    public int? ViewCount { get; set; }
}