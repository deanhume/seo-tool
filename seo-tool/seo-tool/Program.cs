﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using static SeoTool.Utils;

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
            
            // A list of URLs to ignore these might have weird issues.
            List<string> urlsToIgnore = new List<string>
            {
                "https://deanhume.com/azure-hybrid-and-embedded-text-to-speech/",
                "https://deanhume.com/book-the-people-manager/",
                "https://deanhume.com/accelerate-devops-book-review/",
                "https://deanhume.com/progressive-web-apps-book-giveaway-results/",
                "https://deanhume.com/chrome-internal-urls-the-door-to-narnia/",
                "https://deanhume.com/a-simple-client-side-approach-to-measuring-speed-index/"
            };

            // Initialize variables for tracking issues
            StringBuilder issuesContent = new StringBuilder();
            bool hasAnyIssues = false;
            
            // Add header to issues content
            AppendToResults(issuesContent, "SEO TOOL - ISSUES REPORT");
            AppendToResults(issuesContent, $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AppendToResults(issuesContent, $"Sitemap URL: {sitemapUrl}");
            AppendToResults(issuesContent, new string('=', 80));
            AppendToResults(issuesContent, "");

            Console.WriteLine($"Fetching sitemap from: {sitemapUrl}");

            try
            {
                // Fetch and parse the sitemap
                var urls = await FetchSitemapUrlsAsync(sitemapUrl);
                Console.WriteLine($"Found {urls.Count} URLs in sitemap");
                AppendToResults(issuesContent, $"Found {urls.Count} URLs in sitemap");
                AppendToResults(issuesContent, "");

                int processedCount = 0;
                int successCount = 0;
                int issuesCount = 0;
                int limitUrls = 30; // Limit to 30 URLs for demonstration

                // Process each URL in the sitemap (limit to 30 for demonstration)
                foreach (var url in urls.Take(limitUrls))
                {

                    processedCount++;
                    Console.WriteLine($"\n[{processedCount}/{Math.Min(limitUrls, urls.Count)}] Analyzing: {url}");

                    // Create a StringBuilder for this URL's issues (if any)
                    StringBuilder urlIssuesContent = new StringBuilder();
                    bool hasUrlIssues = false;
                    
                    // Add URL header to the URL issues content
                    AppendToResults(urlIssuesContent, $"[{processedCount}/{Math.Min(limitUrls, urls.Count)}] Analyzing: {url}");

                    try
                    {
                        // Check if the URL is in the ignore list
                        if (urlsToIgnore.Contains(url))
                        {
                            Console.WriteLine($"Skipping ignored URL: {url}");
                            AppendToResults(urlIssuesContent, $"Skipping ignored URL: {url}");
                            continue;
                        }

                        // Fetch HTML for the current URL
                        string html = await Utils.FetchHtmlAsync(url);
                        Console.WriteLine($"Successfully fetched {html.Length} characters of HTML");
                        AppendToResults(urlIssuesContent, $"Successfully fetched {html.Length} characters of HTML");

                        bool hasIssues = false;

                        // Extract and analyze the title
                        string title = HtmlUtils.ExtractTitle(html);
                        Console.WriteLine($"Title: {title}");
                        AppendToResults(urlIssuesContent, $"Title: {title}");

                        // Check for broken links and missing images
                        var (links, brokenLinks) = await CheckLinksAsync(html, url);
                        var (images, missingImages) = await CheckImagesAsync(html, url);

                        // Report on links
                        if (brokenLinks.Count > 0)
                        {
                            Console.WriteLine($"⚠ Found {brokenLinks.Count} broken links out of {links.Count} total");
                            AppendToResults(urlIssuesContent, $"⚠ Found {brokenLinks.Count} broken links out of {links.Count} total");

                            // List broken links in the issues file
                            foreach (var link in brokenLinks)
                            {
                                AppendToResults(urlIssuesContent, $"  - {link.Url} ({link.StatusCode})");
                            }

                            hasIssues = true;
                            hasUrlIssues = true;
                        }
                        else
                        {
                            Console.WriteLine($"✓ No broken links found (checked {links.Count} links)");
                            AppendToResults(urlIssuesContent, $"✓ No broken links found (checked {links.Count} links)");
                        }

                        // Report on images
                        if (missingImages.Count > 0)
                        {
                            Console.WriteLine($"⚠ Found {missingImages.Count} missing images out of {images.Count} total");
                            AppendToResults(urlIssuesContent, $"⚠ Found {missingImages.Count} missing images out of {images.Count} total");

                            // List missing images in the issues file
                            foreach (var image in missingImages)
                            {
                                AppendToResults(urlIssuesContent, $"  - {image.Url} ({image.StatusCode})");
                            }

                            hasIssues = true;
                            hasUrlIssues = true;
                        }
                        else
                        {
                            Console.WriteLine($"✓ No missing images found (checked {images.Count} images)");
                            AppendToResults(urlIssuesContent, $"✓ No missing images found (checked {images.Count} images)");
                        }

                        // Overall assessment for this URL
                        if (hasIssues)
                        {
                            Console.WriteLine("⚠ This page has SEO issues that should be addressed");
                            AppendToResults(urlIssuesContent, "⚠ This page has SEO issues that should be addressed");
                            issuesCount++;
                            
                            // If this URL has issues, append its content to the main issues content
                            if (hasUrlIssues)
                            {
                                hasAnyIssues = true;
                                AppendToResults(issuesContent, urlIssuesContent.ToString());
                                AppendToResults(issuesContent, new string('-', 80));
                                AppendToResults(issuesContent, "");
                            }
                        }
                        else
                        {
                            Console.WriteLine("✓ This page passes all SEO checks");
                            successCount++;
                            // We don't append passing pages to the issues file
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error analyzing {url}: {ex.Message}");
                        // For errors, we still want to record them in the issues file
                        AppendToResults(urlIssuesContent, $"Error analyzing {url}: {ex.Message}");
                        issuesCount++;
                        hasUrlIssues = true;
                        hasAnyIssues = true;
                        
                        // Append error information to the main issues content
                        AppendToResults(issuesContent, urlIssuesContent.ToString());
                        AppendToResults(issuesContent, new string('-', 80));
                        AppendToResults(issuesContent, "");
                    }

                    // Add a separator in the console output
                    Console.WriteLine(new string('-', 50));
                }

                // Summary report
                Console.WriteLine("\nSEO Analysis Summary:");
                Console.WriteLine($"Processed {processedCount} URLs from sitemap");
                Console.WriteLine($"✓ {successCount} URLs passed all SEO checks");
                Console.WriteLine($"⚠ {issuesCount} URLs have SEO issues that should be addressed");

                // Add summary to the issues content
                AppendToResults(issuesContent, "SEO ANALYSIS SUMMARY:");
                AppendToResults(issuesContent, $"Processed {processedCount} URLs from sitemap");
                AppendToResults(issuesContent, $"✓ {successCount} URLs passed all SEO checks");
                AppendToResults(issuesContent, $"⚠ {issuesCount} URLs have SEO issues that should be addressed");

                // Only write to file if there are issues
                if (hasAnyIssues)
                {
                    string resultsFileName = $"issues_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                    File.WriteAllText(resultsFileName, issuesContent.ToString());
                    Console.WriteLine($"\nIssues report saved to: {resultsFileName}");
                }
                else
                {
                    Console.WriteLine("\nNo SEO issues found. No report file was generated.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                AppendToResults(issuesContent, $"Error: {ex.Message}");

                // Write results to file if there was an error
                string resultsFileName = $"issues_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                File.WriteAllText(resultsFileName, issuesContent.ToString());
                Console.WriteLine($"\nError report saved to: {resultsFileName}");
            }
        }

        /// <summary>
        /// Appends a line to the results content
        /// </summary>
        /// <param name="sb">The StringBuilder to append to</param>
        /// <param name="line">The line to append</param>
        private static void AppendToResults(StringBuilder sb, string line)
        {
            sb.AppendLine(line);
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
            var links = HtmlUtils.ExtractLinks(html);
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
            var images = HtmlUtils.ExtractImages(html);
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
}
