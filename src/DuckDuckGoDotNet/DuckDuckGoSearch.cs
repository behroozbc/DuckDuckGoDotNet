using System.Net;
using System.Text;
using System.Text.Json;
using DuckDuckGoDotNet.AI;
using DuckDuckGoDotNet.Search;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace DuckDuckGoDotNet
{
    public class DuckDuckGoSearch
    {
        private static readonly ILogger<DuckDuckGoSearch> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DuckDuckGoSearch>();

        private readonly HttpClient client;
        private readonly string proxy;
        private readonly int timeout;
        private readonly bool verify;

        private List<Dictionary<string, string>> chatMessages = new List<Dictionary<string, string>>();
        private int chatTokensCount = 0;
        private string chatVqd = "";
        private string chatVqdHash = "";
        private static readonly string[] impersonates = new string[]
        {
            "chrome_100", "chrome_101", "chrome_104", "chrome_105", "chrome_106", "chrome_107",
            "chrome_108", "chrome_109", "chrome_114", "chrome_116", "chrome_117", "chrome_118",
            "chrome_119", "chrome_120", "chrome_123", "chrome_124", "chrome_126", "chrome_127",
            "chrome_128", "chrome_129", "chrome_130", "chrome_131", "chrome_133",
            "safari_ios_16.5", "safari_ios_17.2", "safari_ios_17.4.1", "safari_ios_18.1.1",
            "safari_15.3", "safari_15.5", "safari_15.6.1", "safari_16", "safari_16.5",
            "safari_17.0", "safari_17.2.1", "safari_17.4.1", "safari_17.5",
            "safari_18", "safari_18.2",
            "safari_ipad_18",
            "edge_101", "edge_122", "edge_127", "edge_131",
            "firefox_109", "firefox_117", "firefox_128", "firefox_133", "firefox_135"
        };

        private static readonly string[] impersonatesOs = new string[]
        {
            "android", "ios", "linux", "macos", "windows"
        };


        public DuckDuckGoSearch(Dictionary<string, string> headers = null, string proxy = null, string proxies = null, int? timeout = 10, bool verify = true)
        {
            string ddgsProxy = Environment.GetEnvironmentVariable("DDGS_PROXY");
            this.proxy = ddgsProxy ?? Utils.ExpandProxyTbAlias(proxy);
            if (string.IsNullOrEmpty(proxy) && !string.IsNullOrEmpty(proxies))
            {
                Console.WriteLine("'proxies' is deprecated, use 'proxy' instead.");
                this.proxy = proxies;
            }
            if (this.proxy != null && !this.proxy.Contains("://"))
            {
                throw new ArgumentException("proxy must be a string with a protocol (e.g., 'http://', 'socks5://')");
            }
            this.timeout = timeout ?? 10;
            this.verify = verify;

            var handler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(this.proxy))
            {
                handler.Proxy = new WebProxy(this.proxy);
            }
            if (!verify)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(this.timeout) };
            headers ??= new Dictionary<string, string>();
            headers["Referer"] = "https://duckduckgo.com/";
            foreach (var kvp in headers)
            {
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            }

            string impersonate = impersonates[new Random().Next(impersonates.Length)];
            client.DefaultRequestHeaders.UserAgent.ParseAdd(GetUserAgent(impersonate));
        }

        private string GetUserAgent(string impersonate)
        {
            // Placeholder: Map impersonate strings to User-Agent strings
            if (impersonate.StartsWith("chrome_"))
            {
                return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36";
            }
            else if (impersonate.StartsWith("safari_"))
            {
                return "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Safari/605.1.15";
            }
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36";
        }
        private async Task<HttpResponseMessage> GetUrl(
            string method,
            string url,
            Dictionary<string, string> paramsDict = null,
            byte[] content = null,
            Dictionary<string, string> data = null,
            Dictionary<string, string> headers = null,
            Dictionary<string, string> cookies = null,
            object json = null,
            int? timeout = null)
        {
            try
            {
                var request = new HttpRequestMessage(new HttpMethod(method), url);

                if (paramsDict != null)
                {
                    var query = string.Join("&", paramsDict.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
                    request.RequestUri = new Uri($"{url}?{query}");
                }
                if (content != null)
                {
                    request.Content = new ByteArrayContent(content);
                }
                else if (data != null)
                {
                    request.Content = new FormUrlEncodedContent(data);
                }
                else if (json != null)
                {
                    request.Content = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
                }
                if (headers != null)
                {
                    foreach (var kvp in headers)
                    {
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }
                }
                if (timeout is not null)
                    client.Timeout = TimeSpan.FromSeconds(timeout.Value);
                var response = await client.SendAsync(request);
                logger.LogDebug($"_get_url() {response.RequestMessage.RequestUri} {response.StatusCode}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
                else if (new[] { HttpStatusCode.Accepted, HttpStatusCode.MovedPermanently, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.TooManyRequests, (HttpStatusCode)418 }.Contains(response.StatusCode))
                {
                    throw new RatelimitException($"{response.RequestMessage.RequestUri} {(int)response.StatusCode} Ratelimit");
                }
                throw new DuckDuckGoSearchException($"{response.RequestMessage.RequestUri} returned None.");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                {
                    throw new TimeoutException($"{url} {ex.GetType().Name}: {ex.Message}", ex);
                }
                throw new DuckDuckGoSearchException($"{url} {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        private async Task<string> GetVqd(string keywords)
        {
            using var resp = await GetUrl("GET", "https://duckduckgo.com", new Dictionary<string, string> { { "q", keywords } });
            using var stream = resp.Content.ReadAsStream();
            byte[] content = new byte[stream.Length];
            stream.Read(content, 0, content.Length);
            return Utils.ExtractVqd(content, keywords);
        }
        private async Task<(string vqd, string hash)> GetChatVqdAsync(bool force = false)
        {

            if (string.IsNullOrEmpty(chatVqd) || force || string.IsNullOrEmpty(chatVqdHash))
            {
                using var resp = await GetUrl("GET", "https://duckduckgo.com/duckchat/v1/status", headers: new Dictionary<string, string> { { "x-vqd-accept", "1" } }, timeout: timeout);
                return (resp.Headers.GetValues("x-vqd-4").FirstOrDefault(chatVqd), resp.Headers.GetValues("x-vqd-hash-1").FirstOrDefault(chatVqdHash));
            }
            return (chatVqd, chatVqdHash);
        }
        public async IAsyncEnumerable<string> ChatTokensAysnc(string keywords, Model model = Model.Gpt4oMini)
        {
            (this.chatVqd, this.chatVqdHash) = await GetChatVqdAsync();
            chatMessages.Add(new Dictionary<string, string> { { "role", "user" }, { "content", keywords } });
            chatTokensCount += Math.Max(keywords.Length / 4, 1);

            var jsonData = new { model = Models.GetModel(model), messages = chatMessages };
            var request = new HttpRequestMessage(HttpMethod.Post, "https://duckduckgo.com/duckchat/v1/chat")
            {
                Content = new StringContent(JsonSerializer.Serialize(jsonData), Encoding.UTF8, "application/json"),

            };
            request.Headers.Add("x-vqd-4", chatVqd);
            request.Headers.Add("x-vqd-hash-1", chatVqdHash);

            var response = client.Send(request);
            chatVqd = response.Headers.GetValues("x-vqd-4").FirstOrDefault() ?? chatVqd;
            chatVqdHash = response.Headers.GetValues("x-vqd-hash-1").FirstOrDefault() ?? chatVqdHash;
            using (var stream = response.Content.ReadAsStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                List<string> chunks = new List<string>();
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("data:"))
                    {
                        string data = line.Substring(5).Trim();
                        if (data == "[DONE]") break;
                        if (data == "[DONE][LIMIT_CONVERSATION]") throw new ConversationLimitException("ERR_CONVERSATION_LIMIT");
                        JsonElement x;
                        try
                        {
                            x = JsonSerializer.Deserialize<JsonElement>(data);
                        }
                        catch (JsonException ex)
                        {
                            throw new DuckDuckGoSearchException($"chat_yield JsonException: {ex.Message}", ex);
                        }

                        if (x.ValueKind == JsonValueKind.Object)
                        {
                            if (x.TryGetProperty("action", out var action) && action.GetString() == "error")
                            {
                                string errMessage = x.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : "";
                                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                                {
                                    if (errMessage == "ERR_CONVERSATION_LIMIT") throw new ConversationLimitException(errMessage);
                                    throw new RatelimitException(errMessage);
                                }
                                throw new DuckDuckGoSearchException(errMessage);
                            }
                            else if (x.TryGetProperty("message", out var messageProp))
                            {
                                string message = messageProp.GetString();
                                chunks.Add(message);
                                yield return message;
                            }
                        }
                    }
                }
                string result = string.Join("", chunks);
                chatMessages.Add(new Dictionary<string, string> { { "role", "assistant" }, { "content", result } });
                chatTokensCount += result.Length;
            }
            response.Dispose();
        }
        /// <summary>
        /// Initiates a chat session with DuckDuckGo AI.
        /// </summary>
        /// <param name="keywords">The initial message or question to send to the AI.</param>
        /// <param name="model">The model to use: "gpt-4o-mini", "llama-3.3-70b", "claude-3-haiku",
        ///     "o3-mini", "mistral-small-3". Defaults to "gpt-4o-mini".</param>
        public string Chat(string message, Model model = Model.Gpt4oMini)
        {

            return string.Join("", ChatTokensAysnc(message, model).ToEnumerable());
        }
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
        /// <returns>List of dictionaries with search results.</returns>
        public async Task<IEnumerable<TextSearchItem>> TextAsync(
            string keywords,
            string region = "wt-wt",
            string safesearch = "moderate",
            string timelimit = null,
            string backend = "auto",
            int? maxResults = null)
        {
            if (backend == "api" || backend == "ecosia")
            {
                Console.WriteLine($"{backend} is deprecated, using backend='auto'");
                backend = "auto";
            }
            var backends = backend == "auto" ? new List<string> { "html", "lite" } : new List<string> { backend };
            backends = backends.OrderBy(x => Guid.NewGuid()).ToList(); // Shuffle

            foreach (var b in backends)
            {
                try
                {
                    return b == "html" ? await TextHtmlAsync(keywords, region, timelimit, maxResults) : await TextLiteAsync(keywords, region, timelimit, maxResults);
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"Error to search using {b} backend: {ex}");
                    if (b == backends.Last()) throw new DuckDuckGoSearchException(ex.Message, ex);
                }
            }
            return Array.Empty<TextSearchItem>();
        }

        private async Task<IEnumerable<TextSearchItem>> TextHtmlAsync(string keywords, string region = "wt-wt", string timelimit = null, int? maxResults = null)
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            var payload = new Dictionary<string, string> { { "q", keywords }, { "b", "" }, { "kl", region } };
            if (!string.IsNullOrEmpty(timelimit)) payload["df"] = timelimit;

            var cache = new HashSet<string>();
            var results = new List<TextSearchItem>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = await GetUrl("POST", "https://html.duckduckgo.com/html", data: payload);
                string html = await resp.Content.ReadAsStringAsync();
                if (html.Contains("No  results.")) return results;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var elements = doc.DocumentNode.SelectNodes("//div[h2]");
                if (elements == null) return results;

                foreach (var e in elements)
                {
                    var hrefNode = e.SelectSingleNode("./a");
                    string href = hrefNode?.GetAttributeValue("href", null);
                    if (href != null && !cache.Contains(href) && !href.StartsWith("http://www.google.com/search?q=") && !href.StartsWith("https://duckduckgo.com/y.js?ad_domain"))
                    {
                        cache.Add(href);
                        string title = e.SelectSingleNode("./h2/a")?.InnerText ?? "";
                        string body = string.Join("", e.SelectSingleNode("./a")?.Descendants().Where(n => n.NodeType == HtmlNodeType.Text).Select(n => n.InnerText) ?? Array.Empty<string>());
                        results.Add(new()
                        {
                            Title = Utils.Normalize(title),
                            Href = Utils.NormalizeUrl(href),
                            Description = Utils.Normalize(body)
                        });
                        if (maxResults.HasValue && results.Count >= maxResults.Value) return results;
                    }
                }

                var nextPage = doc.DocumentNode.SelectSingleNode("//div[@class='nav-link']");
                if (nextPage == null || !maxResults.HasValue) return results;

                var inputs = nextPage.SelectNodes(".//input[@type='hidden']");
                if (inputs != null)
                {
                    payload = inputs.ToDictionary(n => n.GetAttributeValue("name", ""), n => n.GetAttributeValue("value", ""));
                }
            }
            return results;
        }

        private async Task<IEnumerable<TextSearchItem>> TextLiteAsync(string keywords, string region = "wt-wt", string timelimit = null, int? maxResults = null)
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            var payload = new Dictionary<string, string> { { "q", keywords }, { "kl", region } };
            if (!string.IsNullOrEmpty(timelimit)) payload["df"] = timelimit;

            var cache = new HashSet<string>();
            var results = new List<TextSearchItem>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = await GetUrl("POST", "https://lite.duckduckgo.com/lite/", data: payload);
                string html = await resp.Content.ReadAsStringAsync();
                if (html.Contains("No more results.")) return results;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var elements = doc.DocumentNode.SelectNodes("//table[last()]//tr");
                if (elements == null) return results;

                string href = null, title = null;
                foreach (var e in elements)
                {
                    int rowIndex = int.Parse(e.GetAttributeValue("data-row", "0")) % 4 + 1;
                    if (rowIndex == 1)
                    {
                        href = e.SelectSingleNode(".//a")?.GetAttributeValue("href", null);
                        if (href == null || cache.Contains(href) || href.StartsWith("http://www.google.com/search?q=") || href.StartsWith("https://duckduckgo.com/y.js?ad_domain"))
                        {
                            href = null;
                            continue;
                        }
                        cache.Add(href);
                        title = e.SelectSingleNode(".//a")?.InnerText ?? "";
                    }
                    else if (rowIndex == 2 && href != null)
                    {
                        string body = e.SelectSingleNode(".//td[@class='result-snippet']")?.InnerText.Trim() ?? "";
                        results.Add(new()
                        {
                            Title = Utils.Normalize(title),
                            Href = Utils.NormalizeUrl(href),
                            Description = Utils.Normalize(body)
                        });
                        if (maxResults.HasValue && results.Count >= maxResults.Value) return results;
                    }
                }

                var nextPage = doc.DocumentNode.SelectSingleNode("//form[.//input[contains(@value, 'ext')]]");
                if (nextPage == null || !maxResults.HasValue) return results;

                var inputs = nextPage.SelectNodes(".//input[@type='hidden']");
                if (inputs != null)
                {
                    payload = inputs.ToDictionary(n => n.GetAttributeValue("name", ""), n => n.GetAttributeValue("value", ""));
                }
            }
            return results;
        }

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
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            string vqd = await GetVqd(keywords);
            var safesearchBase = new Dictionary<string, string> { { "on", "1" }, { "moderate", "1" }, { "off", "-1" } };
            string f = string.Join(",", new[] {
                timelimit != null ? $"time:{timelimit}" : "",
                size != null ? $"size:{size}" : "",
                color != null ? $"color:{color}" : "",
                typeImage != null ? $"type:{typeImage}" : "",
                layout != null ? $"layout:{layout}" : "",
                licenseImage != null ? $"license:{licenseImage}" : ""
            }.Where(s => !string.IsNullOrEmpty(s)));

            var payload = new Dictionary<string, string>
            {
                {"l", region}, {"o", "json"}, {"q", keywords}, {"vqd", vqd}, {"f", f}, {"p", safesearchBase[safesearch.ToLower()]}
            };

            var cache = new HashSet<string>();
            var results = new List<ImageSearchItem>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = await GetUrl("GET", "https://duckduckgo.com/i.js", paramsDict: payload, headers: new Dictionary<string, string> { { "Referer", "https://duckduckgo.com/" } });
                string jsonStr = await resp.Content.ReadAsStringAsync();
                var respJson = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr);
                var pageData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(respJson["results"].ToString());

                foreach (var row in pageData)
                {
                    string imageUrl = row["image"]?.ToString();
                    if (imageUrl != null && !cache.Contains(imageUrl))
                    {
                        cache.Add(imageUrl);
                        results.Add(new()
                        {
                            Title = row["title"].ToString(),
                            Image = Utils.NormalizeUrl(imageUrl),
                            Thumbnail = Utils.NormalizeUrl(row["thumbnail"].ToString()),
                            URL = Utils.NormalizeUrl(row["url"].ToString()),
                            Height = row["height"].ToString(),
                            Width = row["width"].ToString(),
                            Source = row["source"].ToString()
                        });
                        if (maxResults.HasValue && results.Count >= maxResults.Value) return results;
                    }
                }

                string next = respJson.ContainsKey("next") ? respJson["next"].ToString() : null;
                if (string.IsNullOrEmpty(next) || !maxResults.HasValue) return results;
                payload["s"] = next.Split("s=")[1].Split("&")[0];
            }
            return results;
        }
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
        /// <returns>List of video search results.</returns>
        public async Task<List<Dictionary<string, string>>> VideosAsync(
            string keywords,
            string region = "wt-wt",
            string safesearch = "moderate",
            string timelimit = null,
            string resolution = null,
            string duration = null,
            string licenseVideos = null,
            int? maxResults = null)
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            string vqd = await GetVqd(keywords);
            var safesearchBase = new Dictionary<string, string> { { "on", "1" }, { "moderate", "-1" }, { "off", "-2" } };
            string f = string.Join(",", new[] {
                timelimit != null ? $"publishedAfter:{timelimit}" : "",
                resolution != null ? $"videoDefinition:{resolution}" : "",
                duration != null ? $"videoDuration:{duration}" : "",
                licenseVideos != null ? $"videoLicense:{licenseVideos}" : ""
            }.Where(s => !string.IsNullOrEmpty(s)));

            var payload = new Dictionary<string, string>
            {
                {"l", region}, {"o", "json"}, {"q", keywords}, {"vqd", vqd}, {"f", f}, {"p", safesearchBase[safesearch.ToLower()]}
            };

            var cache = new HashSet<string>();
            var results = new List<Dictionary<string, string>>();

            for (int i = 0; i < 8; i++)
            {
                using var resp = await GetUrl("GET", "https://duckduckgo.com/v.js", paramsDict: payload);
                var jsonStr = await resp.Content.ReadAsStringAsync();
                var respJson = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr);
                var pageData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(respJson["results"].ToString());

                foreach (var row in pageData)
                {
                    if (!cache.Contains(row["content"]))
                    {
                        cache.Add(row["content"]);
                        results.Add(row);
                        if (maxResults.HasValue && results.Count >= maxResults.Value) return results;
                    }
                }

                string next = respJson.ContainsKey("next") ? respJson["next"].ToString() : null;
                if (string.IsNullOrEmpty(next) || !maxResults.HasValue) return results;
                payload["s"] = next.Split("s=")[1].Split("&")[0];
            }
            return results;
        }
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
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            string vqd = await GetVqd(keywords);
            var safesearchBase = new Dictionary<string, string> { { "on", "1" }, { "moderate", "-1" }, { "off", "-2" } };
            var payload = new Dictionary<string, string>
            {
                {"l", region}, {"o", "json"}, {"noamp", "1"}, {"q", keywords}, {"vqd", vqd}, {"p", safesearchBase[safesearch.ToLower()]}
            };
            if (!string.IsNullOrEmpty(timelimit)) payload["df"] = timelimit;

            var cache = new HashSet<string>();
            var results = new List<NewsSearchItem>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = await GetUrl("GET", "https://duckduckgo.com/news.js", paramsDict: payload);
                string jsonStr = await resp.Content.ReadAsStringAsync();
                var respJson = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr);
                var pageData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(respJson["results"].ToString());

                foreach (var row in pageData)
                {
                    string url = row["url"].ToString();
                    if (!cache.Contains(url))
                    {
                        cache.Add(url);
                        string imageUrl = row.ContainsKey("image") ? row["image"]?.ToString() : null;
                        results.Add(new()
                        {
                            Date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(row["date"].ToString())),
                            Title = row["title"].ToString(),
                            Body = Utils.Normalize(row["excerpt"].ToString()),
                            URL = Utils.NormalizeUrl(url),
                            Image = imageUrl != null ? Utils.NormalizeUrl(imageUrl) : null,
                            Source = row["source"].ToString()
                        });
                        if (maxResults.HasValue && results.Count >= maxResults.Value) return results;
                    }
                }

                string next = respJson.ContainsKey("next") ? respJson["next"].ToString() : null;
                if (string.IsNullOrEmpty(next) || !maxResults.HasValue) return results;
                payload["s"] = next.Split("s=")[1].Split("&")[0];
            }
            return results;
        }
    }
}