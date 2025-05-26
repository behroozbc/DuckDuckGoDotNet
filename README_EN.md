[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://lbesson.mit-license.org/)

# DuckDuckGo

Search for texts, news, images, and videos using the DuckDuckGo.com search engine, implemented in C#.

The design of this library was inspired by the [duckduckgo_search](https://github.com/deedy5/duckduckgo_search) library.

If you have any suggestions or issues, please be sure to create an issue.
### [English version](https://github.com/behroozbc/DuckDuckGoDotNet/blob/master/README_EN.md)

## Table of Contents

- [Installation](#installation)
- [Proxy](#proxy)
- [Search](#search)

## Installation

To install this library in your project, download the library from [NuGet](https://www.nuget.org/packages/DuckDuckGoDotNet/).

```bash
dotnet add DuckDuckGoDotNet
```

## Proxy

The package supports http/https/socks proxies. Example: http://user:pass@example.com:3128. Use a rotating proxy. Otherwise, a new proxy will be used every time an instance of the DuckDuckGoSearch class is initialized.

## Search

You can perform text search operations using the `TextAsync` command.

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

```C#
var search = await (new DuckDuckGoSearch()).Text("Iran");
foreach (var item in search)
{
    var a = item["title"];
    Console.WriteLine(a);
}
```

## Image

You can perform image search operations using the `ImagesAsync` command.

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

## Video

You can perform video search operations using the `VideosAsync` command.

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

You can perform news search operations using the `NewsAsync` command.

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

This library has no affiliation with DuckDuckGo and is intended solely for educational purposes. It is not intended for commercial use or any purpose that may violate DuckDuckGo's terms of service. By using this library, you confirm that you will not use it in any way that violates DuckDuckGo's terms. The official DuckDuckGo website is available at https://duckduckgo.com.