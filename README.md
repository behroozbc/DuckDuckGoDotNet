# DuckDuckGo

چت هوش مصنوعی و جستجو برای متن، اخبار، تصاویر و ویدیوها با استفاده از موتور جستجوی DuckDuckGo.com، پیاده‌سازی شده در سی شارپ.

طراحی این کتاب خانه از کتاب خانه [duckduckgo_search](https://github.com/deedy5/duckduckgo_search) الگو برداری شده.

## فهرست مطالب

* [نصب](#نصب)
* [پروکسی](#پروکسی)
* [چت](#چت)
* [جستوجو](#جستوجو)
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
***نمونه***
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
        /// <returns>List of dictionaries with search results. [title,href,body]</returns>
        public async Task<List<Dictionary<string, string>>> TextAsync(
            string keywords,
            string region = "wt-wt",
            string safesearch = "moderate",
            string timelimit = null,
            string backend = "auto",
            int? maxResults = null)

```
***نمونه***
```C#
var search=await (new DuckDuckGoSearch()).Text("Iran");
foreach (var item in search)
{
    var a=item["title"];
    Console.WriteLine(a);
}
```


## سلب مسئولیت

این کتابخانه هیچ وابستگی به DuckDuckGo ندارد و صرفاً برای اهداف آموزشی طراحی شده است. این کتابخانه برای استفاده تجاری یا هر هدفی که نقض شرایط خدمات DuckDuckGo باشد در نظر گرفته نشده است. با استفاده از این کتابخانه، شما تأیید می‌کنید که از آن به شکلی که شرایط DuckDuckGo را نقض کند استفاده نخواهید کرد. وب‌سایت رسمی DuckDuckGo در آدرس https://duckduckgo.com قابل دسترسی است.