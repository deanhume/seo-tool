using System.Net;
using System.Xml;

namespace SeoTool
{
    public static class Utils
    {
        /// <summary>
        /// Fetches HTML content from a specified URL
        /// </summary>
        /// <param name="url">The URL to fetch HTML from</param>
        /// <returns>The HTML content as a string</returns>
        public static async Task<string> FetchHtmlAsync(string url)
        {
            // Create an HttpClient instance
            using HttpClient client = new HttpClient();

            // Add a user agent to avoid being blocked by some websites
            client.DefaultRequestHeaders.Add("User-Agent", "SEO-Tool/1.0");

            // Send the GET request and get the response
            HttpResponseMessage response = await client.GetAsync(url);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Read the content as a string
            string html = await response.Content.ReadAsStringAsync();

            return html;
        }

        /// <summary>
        /// Fetches and parses a sitemap XML file to extract URLs
        /// </summary>
        /// <param name="sitemapUrl">The URL of the sitemap XML file</param>
        /// <returns>A list of URLs found in the sitemap</returns>
        public static async Task<List<string>> FetchSitemapUrlsAsync(string sitemapUrl)
        {
            var urls = new List<string>();

            try
            {
                // Fetch the sitemap XML
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "SEO-Tool/1.0");
                string sitemapXml = await client.GetStringAsync(sitemapUrl);

                // Parse the XML
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sitemapXml);

                // Define the XML namespace manager
                var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                nsManager.AddNamespace("sm", "http://www.sitemaps.org/schemas/sitemap/0.9");

                // Extract all URLs from the sitemap
                var urlNodes = xmlDoc.SelectNodes("//sm:url/sm:loc", nsManager);

                if (urlNodes != null)
                {
                    foreach (XmlNode node in urlNodes)
                    {
                        if (node != null && !string.IsNullOrEmpty(node.InnerText))
                        {
                            urls.Add(node.InnerText.Trim());
                        }
                    }
                }

                // If no URLs were found with namespace, try without namespace
                if (urls.Count == 0)
                {
                    var urlNodesNoNs = xmlDoc.SelectNodes("//url/loc");

                    if (urlNodesNoNs != null)
                    {
                        foreach (XmlNode node in urlNodesNoNs)
                        {
                            if (node != null && !string.IsNullOrEmpty(node.InnerText))
                            {
                                urls.Add(node.InnerText.Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing sitemap: {ex.Message}");
                throw;
            }

            return urls;
        }

        /// <summary>
        /// Represents a URL resource with its HTTP status
        /// </summary>
        public class UrlResource
        {
            public string Url { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public bool IsValid => StatusCode == HttpStatusCode.OK;

            public UrlResource(string url, HttpStatusCode statusCode)
            {
                Url = url;
                StatusCode = statusCode;
            }
        }
    }
}