using System.Text.RegularExpressions;

namespace SeoTool
{
    public static class HtmlUtils
    {
        /// <summary>
        /// Extracts all links from HTML content
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>A list of all href URLs found in anchor tags</returns>
        public static List<string> ExtractLinks(string html)
        {
            var links = new List<string>();
            var regex = new Regex(@"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string href = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrEmpty(href) && !href.StartsWith("#") && !href.StartsWith("javascript:"))
                    {
                        links.Add(href);
                    }
                }
            }

            return links;
        }

        /// <summary>
        /// Extracts all image sources from HTML content
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>A list of all src URLs found in img tags</returns>
        public static List<string> ExtractImages(string html)
        {
            var images = new List<string>();
            var regex = new Regex(@"<img\s+(?:[^>]*?\s+)?src=""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string src = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrEmpty(src) && !src.StartsWith("data:"))
                    {
                        images.Add(src);
                    }
                }
            }

            return images;
        }

        /// <summary>
        /// Extracts the title from HTML content
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>The title text or "No title found" if not present</returns>
        public static string ExtractTitle(string html)
        {
            // Use regex to extract the content between <title> tags
            var titleMatch = Regex.Match(html, @"<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (titleMatch.Success && titleMatch.Groups.Count > 1)
            {
                return titleMatch.Groups[1].Value.Trim();
            }

            return "No title found";
        }
    }
}