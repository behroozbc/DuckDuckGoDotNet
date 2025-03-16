namespace DuckDuckGoDotNet.Search;
public class BaseSearchItem
{
    public string Title { get; init; }

}
public class TextSearchItem : BaseSearchItem
{
    public Uri Href { get; init; }
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
    public Uri Image { get; init; }
    public Uri URL { get; init; }
    public string Source { get; init; }
}
public class NewsSearchItem : BaseMediaSearchItem
{
    public DateTimeOffset Date { get; init; }
    public string Body { get; init; }
}