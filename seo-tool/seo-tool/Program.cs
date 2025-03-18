﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SeoTool
{
    /// <summary>
    /// Main program class for the SEO Tool
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the application
        /// </summary>
        public static async Task Main(string[] args)
        {
            await RunAsync();
        }

        /// <summary>
        /// Main functionality of the SEO Tool
        /// </summary>
        private static async Task RunAsync()
        {
            Console.WriteLine("SEO Tool - HTML Analyzer");
            Console.WriteLine("------------------------");
            
            string sitemapUrl = "https://deanhume.com/sitemap-posts.xml";
            string keyword = "Azure Function"; // The keyword to check for in title and description
            
            Console.WriteLine($"Fetching sitemap from: {sitemapUrl}");
            
            try
            {
                // Fetch and parse the sitemap
                var urls = await FetchSitemapUrlsAsync(sitemapUrl);
                Console.WriteLine($"Found {urls.Count} URLs in sitemap");
                
                int processedCount = 0;
                int successCount = 0;
                int issuesCount = 0;
                
                // Process each URL in the sitemap (limit to 5 for demonstration)
                foreach (var url in urls.Take(5))
                {
                    processedCount++;
                    Console.WriteLine($"\n[{processedCount}/{Math.Min(5, urls.Count)}] Analyzing: {url}");
                    
                    try
                    {
                        // Fetch HTML for the current URL
                        string html = await FetchHtmlAsync(url);
                        Console.WriteLine($"Successfully fetched {html.Length} characters of HTML");
                        
                        bool hasIssues = false;
                        
                        // Extract and analyze the title
                        string title = ExtractTitle(html);
                        Console.WriteLine($"Title: {title}");
                        bool titleContainsKeyword = ContainsSimilarString(title, keyword);
                        Console.WriteLine($"Title contains keyword or similar: {(titleContainsKeyword ? "YES ✓" : "NO ✗")}");
                        if (!titleContainsKeyword) hasIssues = true;
                        
                        // Extract and analyze the og:description
                        string ogDescription = ExtractOgDescription(html);
                        Console.WriteLine($"OG Description: {ogDescription}");
                        bool descriptionContainsKeyword = ContainsSimilarString(ogDescription, keyword);
                        Console.WriteLine($"OG Description contains keyword or similar: {(descriptionContainsKeyword ? "YES ✓" : "NO ✗")}");
                        if (!descriptionContainsKeyword) hasIssues = true;
                        
                        // Check for broken links and missing images
                        var (links, brokenLinks) = await CheckLinksAsync(html, url);
                        var (images, missingImages) = await CheckImagesAsync(html, url);
                        
                        // Report on links
                        if (brokenLinks.Count > 0)
                        {
                            Console.WriteLine($"⚠ Found {brokenLinks.Count} broken links out of {links.Count} total");
                            hasIssues = true;
                        }
                        else
                        {
                            Console.WriteLine($"✓ No broken links found (checked {links.Count} links)");
                        }
                        
                        // Report on images
                        if (missingImages.Count > 0)
                        {
                            Console.WriteLine($"⚠ Found {missingImages.Count} missing images out of {images.Count} total");
                            hasIssues = true;
                        }
                        else
                        {
                            Console.WriteLine($"✓ No missing images found (checked {images.Count} images)");
                        }
                        
                        // Overall assessment for this URL
                        if (hasIssues)
                        {
                            Console.WriteLine("⚠ This page has SEO issues that should be addressed");
                            issuesCount++;
                        }
                        else
                        {
                            Console.WriteLine("✓ This page passes all SEO checks");
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error analyzing {url}: {ex.Message}");
                        issuesCount++;
                    }
                    
                    // Add a separator between URLs
                    Console.WriteLine(new string('-', 50));
                }
                
                // Summary report
                Console.WriteLine("\nSEO Analysis Summary:");
                Console.WriteLine($"Processed {processedCount} URLs from sitemap");
                Console.WriteLine($"✓ {successCount} URLs passed all SEO checks");
                Console.WriteLine($"⚠ {issuesCount} URLs have SEO issues that should be addressed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Fetches and parses a sitemap XML file to extract URLs
        /// </summary>
        /// <param name="sitemapUrl">The URL of the sitemap XML file</param>
        /// <returns>A list of URLs found in the sitemap</returns>
        private static async Task<List<string>> FetchSitemapUrlsAsync(string sitemapUrl)
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
        /// Fetches HTML content from a specified URL
        /// </summary>
        /// <param name="url">The URL to fetch HTML from</param>
        /// <returns>The HTML content as a string</returns>
        private static async Task<string> FetchHtmlAsync(string url)
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
        /// Extracts the title from HTML content
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>The title text or "No title found" if not present</returns>
        private static string ExtractTitle(string html)
        {
            // Use regex to extract the content between <title> tags
            var titleMatch = Regex.Match(html, @"<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            if (titleMatch.Success && titleMatch.Groups.Count > 1)
            {
                return titleMatch.Groups[1].Value.Trim();
            }
            
            return "No title found";
        }

        /// <summary>
        /// Extracts the Open Graph description from HTML content
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>The OG description or "No OG description found" if not present</returns>
        private static string ExtractOgDescription(string html)
        {
            // Use regex to extract the content of meta tag with property="og:description"
            var ogDescMatch = Regex.Match(html, @"<meta\s+property=""og:description""\s+content=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            if (ogDescMatch.Success && ogDescMatch.Groups.Count > 1)
            {
                return ogDescMatch.Groups[1].Value.Trim();
            }
            
            // Try alternate format with property and content in different order
            ogDescMatch = Regex.Match(html, @"<meta\s+content=""(.*?)""\s+property=""og:description""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            if (ogDescMatch.Success && ogDescMatch.Groups.Count > 1)
            {
                return ogDescMatch.Groups[1].Value.Trim();
            }
            
            return "No OG description found";
        }

        /// <summary>
        /// Checks if a text contains a keyword or similar terms
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <param name="keyword">The keyword to look for</param>
        /// <returns>True if the text contains the keyword or similar terms</returns>
        private static bool ContainsSimilarString(string text, string keyword)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(keyword))
            {
                return false;
            }
            
            // Convert both to lowercase for case-insensitive comparison
            text = text.ToLower();
            keyword = keyword.ToLower();
            
            // Direct match check
            if (text.Contains(keyword))
            {
                return true;
            }
            
            // Check for individual words in the keyword
            string[] keywordWords = keyword.Split(new[] { ' ', ',', '.', ';', ':', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            
            // If at least half of the keyword words are found, consider it a match
            int matchCount = 0;
            foreach (string word in keywordWords)
            {
                if (word.Length > 2 && text.Contains(word)) // Only check words longer than 2 characters
                {
                    matchCount++;
                }
            }
            
            // Consider it a match if at least half of the keyword words are found
            return keywordWords.Length > 0 && (double)matchCount / keywordWords.Length >= 0.5;
        }

        /// <summary>
        /// Extracts all links from HTML content
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>A list of all href URLs found in anchor tags</returns>
        private static List<string> ExtractLinks(string html)
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
        private static List<string> ExtractImages(string html)
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
        /// Resolves a relative URL to an absolute URL
        /// </summary>
        /// <param name="baseUrl">The base URL of the page</param>
        /// <param name="relativeUrl">The relative URL to resolve</param>
        /// <returns>The absolute URL</returns>
        private static string ResolveUrl(string baseUrl, string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return baseUrl;
                
            if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out _))
                return relativeUrl;
                
            if (relativeUrl.StartsWith("//"))
            {
                var baseUri = new Uri(baseUrl);
                return $"{baseUri.Scheme}:{relativeUrl}";
            }
            
            if (!baseUrl.EndsWith("/"))
                baseUrl = baseUrl + "/";
                
            var uri = new Uri(new Uri(baseUrl), relativeUrl);
            return uri.ToString();
        }

        /// <summary>
        /// Checks if a URL is valid by making an HTTP request
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <param name="client">The HttpClient to use</param>
        /// <returns>A UrlResource with the URL and its status</returns>
        private static async Task<UrlResource> CheckUrlAsync(string url, HttpClient client)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await client.SendAsync(request);
                return new UrlResource(url, response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new UrlResource(url, HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                return new UrlResource(url, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Checks all links in the HTML content
        /// </summary>
        /// <param name="html">The HTML content to check</param>
        /// <param name="baseUrl">The base URL of the page</param>
        /// <returns>A tuple with all links and broken links</returns>
        private static async Task<(List<UrlResource> AllLinks, List<UrlResource> BrokenLinks)> CheckLinksAsync(string html, string baseUrl)
        {
            var links = ExtractLinks(html);
            var results = new List<UrlResource>();
            var brokenLinks = new List<UrlResource>();
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SEO-Tool/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);
            
            Console.WriteLine($"Checking {links.Count} links...");
            
            foreach (var link in links.Distinct().Take(20)) // Limit to 20 links to avoid too many requests
            {
                var absoluteUrl = ResolveUrl(baseUrl, link);
                Console.Write(".");
                var result = await CheckUrlAsync(absoluteUrl, client);
                results.Add(result);
                
                if (!result.IsValid)
                {
                    brokenLinks.Add(result);
                }
            }
            
            Console.WriteLine();
            return (results, brokenLinks);
        }

        /// <summary>
        /// Checks all images in the HTML content
        /// </summary>
        /// <param name="html">The HTML content to check</param>
        /// <param name="baseUrl">The base URL of the page</param>
        /// <returns>A tuple with all images and missing images</returns>
        private static async Task<(List<UrlResource> AllImages, List<UrlResource> MissingImages)> CheckImagesAsync(string html, string baseUrl)
        {
            var images = ExtractImages(html);
            var results = new List<UrlResource>();
            var missingImages = new List<UrlResource>();
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SEO-Tool/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);
            
            Console.WriteLine($"Checking {images.Count} images...");
            
            foreach (var image in images.Distinct())
            {
                var absoluteUrl = ResolveUrl(baseUrl, image);
                Console.Write(".");
                var result = await CheckUrlAsync(absoluteUrl, client);
                results.Add(result);
                
                if (!result.IsValid)
                {
                    missingImages.Add(result);
                }
            }
            
            Console.WriteLine();
            return (results, missingImages);
        }
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
