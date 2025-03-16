using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using DockDockGoDotNet;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace DuckDuckGoSearch
{
    public class DDGS
    {
        private static readonly ILogger<DDGS> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DDGS>();

        private readonly HttpClient client;
        private readonly string proxy;
        private readonly int timeout;
        private readonly bool verify;

        private List<Dictionary<string, string>> chatMessages = new List<Dictionary<string, string>>();
        private int chatTokensCount = 0;
        private string chatVqd = "";
        private double sleepTimestamp = 0.0;

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

        private static readonly Dictionary<string, string> chatModels = new Dictionary<string, string>
        {
            {"gpt-4o-mini", "gpt-4o-mini"},
            {"llama-3.3-70b", "meta-llama/Llama-3.3-70B-Instruct-Turbo"},
            {"claude-3-haiku", "claude-3-haiku-20240307"},
            {"o3-mini", "o3-mini"},
            {"mistral-small-3", "mistralai/Mistral-Small-24B-Instruct-2501"}
        };

        public DDGS(Dictionary<string, string> headers = null, string proxy = null, string proxies = null, int? timeout = 10, bool verify = true)
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

        private void Sleep(float sleeptime = 0.75f)
        {
            double delay = 0.0;
            if (sleepTimestamp != 0.0)
            {
                double currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (currentTime - sleepTimestamp < 20)
                {
                    delay = sleeptime;
                }
            }
            sleepTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (delay > 0)
            {
                Thread.Sleep((int)(delay * 1000));
            }
        }

        private HttpResponseMessage GetUrl(
            string method,
            string url,
            Dictionary<string, string> paramsDict = null,
            byte[] content = null,
            Dictionary<string, string> data = null,
            Dictionary<string, string> headers = null,
            Dictionary<string, string> cookies = null,
            object json = null,
            float? timeout = null)
        {
            Sleep();
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
                var response = client.Send(request);
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

        private string GetVqd(string keywords)
        {
            using var resp = GetUrl("GET", "https://duckduckgo.com", new Dictionary<string, string> { { "q", keywords } });
            using var stream = resp.Content.ReadAsStream();
            byte[] content = new byte[stream.Length];
            stream.Read(content, 0, content.Length);
            return Utils.ExtractVqd(content, keywords);
        }

        public IEnumerable<string> ChatYield(string keywords, string model = "gpt-4o-mini", int timeout = 30)
        {
            if (string.IsNullOrEmpty(chatVqd))
            {
                using var resp = GetUrl("GET", "https://duckduckgo.com/duckchat/v1/status", headers: new Dictionary<string, string> { { "x-vqd-accept", "1" } });
                chatVqd = resp.Headers.GetValues("x-vqd-4").FirstOrDefault() ?? "";
            }

            chatMessages.Add(new Dictionary<string, string> { { "role", "user" }, { "content", keywords } });
            chatTokensCount += Math.Max(keywords.Length / 4, 1);

            if (!chatModels.ContainsKey(model))
            {
                Console.WriteLine($"Warning: {model} is unavailable. Using 'gpt-4o-mini'");
                model = "gpt-4o-mini";
            }

            var jsonData = new { model = chatModels[model], messages = chatMessages };
            var request = new HttpRequestMessage(HttpMethod.Post, "https://duckduckgo.com/duckchat/v1/chat")
            {
                Content = new StringContent(JsonSerializer.Serialize(jsonData), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-vqd-4", chatVqd);

            var response = client.Send(request);
            chatVqd = response.Headers.GetValues("x-vqd-4").FirstOrDefault() ?? chatVqd;

            using (var stream = response.Content.ReadAsStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                List<string> chunks = new List<string>();
                while ((line = reader.ReadLine()) != null)
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

        public string Chat(string keywords, string model = "gpt-4o-mini", int timeout = 30)
        {
            return string.Join("", ChatYield(keywords, model, timeout));
        }

        public List<Dictionary<string, string>> Text(
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
                    return b == "html" ? TextHtml(keywords, region, timelimit, maxResults) : TextLite(keywords, region, timelimit, maxResults);
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"Error to search using {b} backend: {ex}");
                    if (b == backends.Last()) throw new DuckDuckGoSearchException(ex.Message, ex);
                }
            }
            return new List<Dictionary<string, string>>();
        }

        private List<Dictionary<string, string>> TextHtml(string keywords, string region = "wt-wt", string timelimit = null, int? maxResults = null)
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            var payload = new Dictionary<string, string> { { "q", keywords }, { "b", "" }, { "kl", region } };
            if (!string.IsNullOrEmpty(timelimit)) payload["df"] = timelimit;

            var cache = new HashSet<string>();
            var results = new List<Dictionary<string, string>>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = GetUrl("POST", "https://html.duckduckgo.com/html", data: payload);
                string html = resp.Content.ReadAsStringAsync().Result;
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
                        results.Add(new Dictionary<string, string>
                        {
                            {"title", Utils.Normalize(title)},
                            {"href", Utils.NormalizeUrl(href)},
                            {"body", Utils.Normalize(body)}
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

        private List<Dictionary<string, string>> TextLite(string keywords, string region = "wt-wt", string timelimit = null, int? maxResults = null)
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            var payload = new Dictionary<string, string> { { "q", keywords }, { "kl", region } };
            if (!string.IsNullOrEmpty(timelimit)) payload["df"] = timelimit;

            var cache = new HashSet<string>();
            var results = new List<Dictionary<string, string>>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = GetUrl("POST", "https://lite.duckduckgo.com/lite/", data: payload);
                string html = resp.Content.ReadAsStringAsync().Result;
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
                        results.Add(new Dictionary<string, string>
                        {
                            {"title", Utils.Normalize(title)},
                            {"href", Utils.NormalizeUrl(href)},
                            {"body", Utils.Normalize(body)}
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

        public List<Dictionary<string, string>> Images(
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

            string vqd = GetVqd(keywords);
            var safesearchBase = new Dictionary<string, string> { {"on", "1"}, {"moderate", "1"}, {"off", "-1"} };
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
            var results = new List<Dictionary<string, string>>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = GetUrl("GET", "https://duckduckgo.com/i.js", paramsDict: payload, headers: new Dictionary<string, string> { {"Referer", "https://duckduckgo.com/"} });
                string jsonStr = resp.Content.ReadAsStringAsync().Result;
                var respJson = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr);
                var pageData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(respJson["results"].ToString());

                foreach (var row in pageData)
                {
                    string imageUrl = row["image"]?.ToString();
                    if (imageUrl != null && !cache.Contains(imageUrl))
                    {
                        cache.Add(imageUrl);
                        results.Add(new Dictionary<string, string>
                        {
                            {"title", row["title"].ToString()},
                            {"image", Utils.NormalizeUrl(imageUrl)},
                            {"thumbnail", Utils.NormalizeUrl(row["thumbnail"].ToString())},
                            {"url", Utils.NormalizeUrl(row["url"].ToString())},
                            {"height", row["height"].ToString()},
                            {"width", row["width"].ToString()},
                            {"source", row["source"].ToString()}
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

        public List<Dictionary<string, string>> Videos(
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

            string vqd = GetVqd(keywords);
            var safesearchBase = new Dictionary<string, string> { {"on", "1"}, {"moderate", "-1"}, {"off", "-2"} };
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
                using var resp = GetUrl("GET", "https://duckduckgo.com/v.js", paramsDict: payload);
                string jsonStr = resp.Content.ReadAsStringAsync().Result;
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

        public List<Dictionary<string, string>> News(
            string keywords,
            string region = "wt-wt",
            string safesearch = "moderate",
            string timelimit = null,
            int? maxResults = null)
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentException("keywords is mandatory");

            string vqd = GetVqd(keywords);
            var safesearchBase = new Dictionary<string, string> { {"on", "1"}, {"moderate", "-1"}, {"off", "-2"} };
            var payload = new Dictionary<string, string>
            {
                {"l", region}, {"o", "json"}, {"noamp", "1"}, {"q", keywords}, {"vqd", vqd}, {"p", safesearchBase[safesearch.ToLower()]}
            };
            if (!string.IsNullOrEmpty(timelimit)) payload["df"] = timelimit;

            var cache = new HashSet<string>();
            var results = new List<Dictionary<string, string>>();

            for (int i = 0; i < 5; i++)
            {
                using var resp = GetUrl("GET", "https://duckduckgo.com/news.js", paramsDict: payload);
                string jsonStr = resp.Content.ReadAsStringAsync().Result;
                var respJson = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr);
                var pageData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(respJson["results"].ToString());

                foreach (var row in pageData)
                {
                    string url = row["url"].ToString();
                    if (!cache.Contains(url))
                    {
                        cache.Add(url);
                        string imageUrl = row.ContainsKey("image") ? row["image"]?.ToString() : null;
                        results.Add(new Dictionary<string, string>
                        {
                            {"date", DateTimeOffset.FromUnixTimeSeconds(long.Parse(row["date"].ToString())).ToString("O")},
                            {"title", row["title"].ToString()},
                            {"body", Utils.Normalize(row["excerpt"].ToString())},
                            {"url", Utils.NormalizeUrl(url)},
                            {"image", imageUrl != null ? Utils.NormalizeUrl(imageUrl) : null},
                            {"source", row["source"].ToString()}
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