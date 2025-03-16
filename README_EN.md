# DuckDuckGo

Chat with AI and search for text, news, images, and videos using the DuckDuckGo.com search engine, implemented in C#.

The design of this library is inspired by the [duckduckgo_search](https://github.com/deedy5/duckduckgo_search) library.

## Table of Contents

- [Installation](#installation)
- [Proxy](#proxy)
- [Chat](#chat)
- [Search](#search)

## Installation

To install this library in your project, download the NuGet package for this library:

```bash
dotnet add DuckDuckGoDotNet
```

## Proxy

The package supports HTTP/HTTPS/SOCKS proxies. Example: http://user:pass@example.com:3128. Use a rotating proxy; otherwise, each time the DuckDuckGoSearch class is instantiated, it will use a new proxy.

## Chat

To chat with the ready DuckDuckGo AI, you can use the following command:

```c#
/// <summary>
/// Initiates a chat session with DuckDuckGo AI.
/// </summary>
/// <param name="keywords">The initial message or question to send to the AI.</param>
/// <param name="model">The model to use: "gpt-4o-mini", "llama-3.3-70b", "claude-3-haiku",
///     "o3-mini", "mistral-small-3". Defaults to "gpt-4o-mini".</param>
/// <param name="timeout">Timeout value for the HTTP client in seconds. Defaults to 30.</param>
/// <returns>The response from the AI as a string.</returns>
public string Chat(string message, Model model = Model.Gpt4oMini, int timeout = 30)
```

**_Example_**

```c#
var results = new DuckDuckGoSearch().Chat("Tell me about Iran's history", Model.Llama3370b);
```

## Search

You can perform a text search using the `TextAsync` method.

```c#
/// <summary>
/// DuckDuckGo text search. Query params: https://duckduckgo.com/params.
/// </summary>
/// <param name="keywords">Keywords for query.</param>
/// <param name="region">wt-wt, us-en, uk-en, ru-ru, etc. Defaults to "wt-wt".</param>
/// <param name="safesearch">on, moderate, off. Defaults to "moderate".</param>
/// <param name="timelimit">d, w, m, y. Defaults to null.</param>
/// <param name="backend">auto, html, lite. Defaults to "auto".
///     auto - try all backends in random order,
///     html - collect data from https://html.duckduckgo.com,
///     lite - collect data from https://lite.duckduckgo.com.
/// </param>
/// <param name="maxResults">Max number of results. If null, returns results only from the first response. Defaults to null.</param>
/// <returns>List of search results.</returns>
public async Task<IEnumerable<TextSearchItem>> TextAsync(
    string keywords,
    string region = "wt-wt",
    string safesearch = "moderate",
    string timelimit = null,
    string backend = "auto",
    int? maxResults = null)
```

**_Example_**

```c#
var search = await (new DuckDuckGoSearch()).Text("Iran");
foreach (var item in search)
{
    var title = item["title"];
    Console.WriteLine(title);
}
```

## Images

You can perform an image search using the `ImagesAsync` method.

```c#
/// <summary>
/// DuckDuckGo images search. Query params: https://duckduckgo.com/params.
/// </summary>
/// <param name="keywords">Keywords for query.</param>
/// <param name="region">wt-wt, us-en, uk-en, ru-ru, etc. Defaults to "wt-wt".</param>
/// <param name="safesearch">on, moderate, off. Defaults to "moderate".</param>
/// <param name="timelimit">Day, Week, Month, Year. Defaults to null.</param>
/// <param name="size">Small, Medium, Large, Wallpaper. Defaults to null.</param>
/// <param name="color">color, Monochrome, Red, Orange, Yellow, Green, Blue,
///     Purple, Pink, Brown, Black, Gray, Teal, White. Defaults to null.</param>
/// <param name="typeImage">photo, clipart, gif, transparent, line.
///     Defaults to null.</param>
/// <param name="layout">Square, Tall, Wide. Defaults to null.</param>
/// <param name="licenseImage">any (All Creative Commons), Public (PublicDomain),
///     Share (Free to Share and Use), ShareCommercially (Free to Share and Use Commercially),
///     Modify (Free to Modify, Share, and Use), ModifyCommercially (Free to Modify, Share, and
///     Use Commercially). Defaults to null.</param>
/// <param name="maxResults">Max number of results. If null, returns results only from the first response. Defaults to null.</param>
/// <returns>List of image search results.</returns>
public async Task<IEnumerable<ImageSearchItem>> ImagesAsync(
    string keywords,
    string region = "wt-wt",
    string safesearch = "moderate",
    string timelimit = null,
    string size = null,
    string color = null,
    string typeImage = null,
    string layout = null,
    string licenseImage = null,
    int? maxResults = null)
```

## Videos

You can perform a video search using the `VideosAsync` method.

```c#
/// <summary>
/// DuckDuckGo videos search. Query params: https://duckduckgo.com/params.
/// </summary>
/// <param name="keywords">Keywords for query.</param>
/// <param name="region">wt-wt, us-en, uk-en, ru-ru, etc. Defaults to "wt-wt".</param>
/// <param name="safesearch">on, moderate, off. Defaults to "moderate".</param>
/// <param name="timelimit">d, w, m. Defaults to null.</param>
/// <param name="resolution">high, standard. Defaults to null.</param>
/// <param name="duration">short, medium, long. Defaults to null.</param>
/// <param name="licenseVideos">creativeCommon, youtube. Defaults to null.</param>
/// <param name="maxResults">Max number of results. If null, returns results only from the first response. Defaults to null.</param>
/// <returns>List of dictionaries with video search results.</returns>
public async Task<IEnumerable<NewsSearchItem>> VideosAsync(
    string keywords,
    string region = "wt-wt",
    string safesearch = "moderate",
    string timelimit = null,
    string resolution = null,
    string duration = null,
    string licenseVideos = null,
    int? maxResults = null)
```

## News

You can perform a news search using the `NewsAsync` method.

```c#
/// <summary>
/// DuckDuckGo news search. Query params: https://duckduckgo.com/params.
/// </summary>
/// <param name="keywords">Keywords for query.</param>
/// <param name="region">wt-wt, us-en, uk-en, ru-ru, etc. Defaults to "wt-wt".</param>
/// <param name="safesearch">on, moderate, off. Defaults to "moderate".</param>
/// <param name="timelimit">d, w, m. Defaults to null.</param>
/// <param name="maxResults">Max number of results. If null, returns results only from the first response. Defaults to null.</param>
/// <returns>List of news search results.</returns>
public async Task<IEnumerable<NewsSearchItem>> NewsAsync(
    string keywords,
    string region = "wt-wt",
    string safesearch = "moderate",
    string timelimit = null,
    int? maxResults = null)
```

## Disclaimer

This library has no affiliation with DuckDuckGo and is designed solely for educational purposes. This library is not intended for commercial use or for any purpose that would violate the DuckDuckGo terms of service. By using this library, you confirm that you will not use it in any manner that violates DuckDuckGo's terms. The official DuckDuckGo website can be accessed at https://duckduckgo.com.
