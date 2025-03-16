# DuckDuckGo

چت هوش مصنوعی و جستجو برای متن، اخبار، تصاویر و ویدیوها با استفاده از موتور جستجوی DuckDuckGo.com، پیاده‌سازی شده در سی شارپ.

طراحی این کتاب خانه از کتاب خانه [duckduckgo_search](https://github.com/deedy5/duckduckgo_search) الگو برداری شده.

## فهرست مطالب

- [نصب](#نصب)
- [پروکسی](#پروکسی)
- [چت](#چت)
- [جستوجو](#جستوجو)

## نصب

برای نصب این کتابخانه در پروژه خود باید از نوگت این کتاب خانه را دانلود کنید.

```bash
dotnet add DuckDuckGoDotNet
```

## پروکسی

پکیج از پروکسی‌های http/https/socks پشتیبانی می‌کند. مثال: http://user:pass@example.com:3128. از یک پروکسی چرخشی استفاده کنید. در غیر این صورت، با هر بار مقداردهی اولیه کلاس DuckDuckGoSearch از یک پروکسی جدید استفاده کنید.

## چت

برای چت کردن با هوش مصنوعی آماده duckduckgo میتونید از دستور زیر استفاده کنید.

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

**_نمونه_**

```python
var results= new DuckDuckGoSearch().Chat("Tell me about Iran's history",Model.Llama3370b);
```

## جستوجو

عملیات جست و جو رو با دستور `TextAsync` میتوانید انجام دهید.

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

**_نمونه_**

```C#
var search=await (new DuckDuckGoSearch()).Text("Iran");
foreach (var item in search)
{
    var a=item["title"];
    Console.WriteLine(a);
}
```

## تصویر

عملیات جست و جو تصویر رو با دستور `ImagesAsync` میتوانید انجام دهید.

```C#
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

## فیلم

عملیات جست و جو فیلم رو با دستور `VideosAsync` میتوانید انجام دهید.

```C#
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

## اخبار
عملیات جست و جو اخبار رو با دستور `NewsAsync` میتوانید انجام دهید.
```C#
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

## سلب مسئولیت

این کتابخانه هیچ وابستگی به DuckDuckGo ندارد و صرفاً برای اهداف آموزشی طراحی شده است. این کتابخانه برای استفاده تجاری یا هر هدفی که نقض شرایط خدمات DuckDuckGo باشد در نظر گرفته نشده است. با استفاده از این کتابخانه، شما تأیید می‌کنید که از آن به شکلی که شرایط DuckDuckGo را نقض کند استفاده نخواهید کرد. وب‌سایت رسمی DuckDuckGo در آدرس https://duckduckgo.com قابل دسترسی است.
