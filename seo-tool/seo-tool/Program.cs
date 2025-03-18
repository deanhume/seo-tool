﻿﻿﻿﻿﻿﻿using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Main program
await RunAsync();

// Function to demonstrate the HTML fetching and SEO analysis
async Task RunAsync()
{
    Console.WriteLine("SEO Tool - HTML Analyzer");
    Console.WriteLine("------------------------");
    
    string url = "https://deanhume.com";
    string keyword = "web performance"; // The keyword to check for in title and description
    
    Console.WriteLine($"Fetching HTML from: {url}");
    Console.WriteLine($"Checking for keyword: \"{keyword}\"");
    
    try
    {
        string html = await FetchHtmlAsync(url);
        Console.WriteLine($"Successfully fetched {html.Length} characters of HTML");
        
        // Extract and analyze the title
        string title = ExtractTitle(html);
        Console.WriteLine($"\nTitle: {title}");
        bool titleContainsKeyword = ContainsSimilarString(title, keyword);
        Console.WriteLine($"Title contains keyword or similar: {(titleContainsKeyword ? "YES ✓" : "NO ✗")}");
        
        // Extract and analyze the og:description
        string ogDescription = ExtractOgDescription(html);
        Console.WriteLine($"\nOG Description: {ogDescription}");
        bool descriptionContainsKeyword = ContainsSimilarString(ogDescription, keyword);
        Console.WriteLine($"OG Description contains keyword or similar: {(descriptionContainsKeyword ? "YES ✓" : "NO ✗")}");
        
        // Overall SEO assessment
        Console.WriteLine("\nSEO Assessment:");
        if (titleContainsKeyword && descriptionContainsKeyword)
        {
            Console.WriteLine("✓ Great! Both title and description contain the keyword or similar terms.");
        }
        else if (titleContainsKeyword || descriptionContainsKeyword)
        {
            Console.WriteLine("⚠ Partial match. Only one element contains the keyword or similar terms.");
        }
        else
        {
            Console.WriteLine("✗ Poor match. Neither title nor description contains the keyword or similar terms.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}


/// <summary>
/// Fetches HTML content from a specified URL
/// </summary>
/// <param name="url">The URL to fetch HTML from</param>
/// <returns>The HTML content as a string</returns>
async Task<string> FetchHtmlAsync(string url)
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
string ExtractTitle(string html)
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
string ExtractOgDescription(string html)
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
bool ContainsSimilarString(string text, string keyword)
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
