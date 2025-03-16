namespace DuckDuckGoDotNet;

using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;


// Define a static utility class to hold the methods
public static class Utils
{
    // Compiled regex for stripping HTML tags, equivalent to Python's REGEX_STRIP_TAGS
    private static readonly Regex StripTagsRegex = new Regex("<.*?>", RegexOptions.Compiled);

    // Extract vqd value from HTML bytes, equivalent to _extract_vqd
    public static string ExtractVqd(byte[] htmlBytes, string keywords)
    {
        string html = Encoding.UTF8.GetString(htmlBytes);
        // Define patterns similar to Python's (c1, c1_len, c2) tuple
        var patterns = new (string StartPattern, string EndPattern)[]
        {
            ("vqd=\"", "\""), // vqd="..." pattern
            ("vqd=", "&"),   // vqd=...& pattern
            ("vqd='", "'")   // vqd='...' pattern
        };

        foreach (var (startPattern, endPattern) in patterns)
        {
            int startIndex = html.IndexOf(startPattern);
            if (startIndex != -1)
            {
                startIndex += startPattern.Length;
                int endIndex = html.IndexOf(endPattern, startIndex);
                if (endIndex != -1)
                {
                    return html.Substring(startIndex, endIndex - startIndex);
                }
            }
        }
        throw new DuckDuckGoSearchException($"_extract_vqd() keywords={keywords} Could not extract vqd.");
    }

    // Normalize HTML by stripping tags and decoding entities, equivalent to _normalize
    public static string Normalize(string rawHtml)
    {
        if (string.IsNullOrEmpty(rawHtml)) return "";
        string stripped = StripTagsRegex.Replace(rawHtml, "");
        return WebUtility.HtmlDecode(stripped); // Equivalent to Python's html.unescape
    }

    // Normalize URL by decoding and replacing spaces, equivalent to _normalize_url
    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        string decoded = WebUtility.UrlDecode(url); // Equivalent to Python's urllib.parse.unquote
        return decoded.Replace(" ", "+");
    }

    // Expand proxy "tb" alias, equivalent to _expand_proxy_tb_alias
    public static string ExpandProxyTbAlias(string proxy)
    {
        // In Python, proxy can be str | None; in C#, null is handled naturally
        return proxy == "tb" ? "socks5://127.0.0.1:9150" : proxy;
    }
}
