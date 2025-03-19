using SeoTool;

namespace set_tool_test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ResolveUrl_WithAbsoluteUrl_ReturnsSameUrl()
    {
        // Arrange
        string baseUrl = "https://example.com";
        string absoluteUrl = "https://anotherexample.com/page";

        // Act
        string result = Utils.ResolveUrl(baseUrl, absoluteUrl);

        // Assert
        Assert.That(absoluteUrl, Is.EqualTo(result));
    }

    [Test]
    public void ResolveUrl_WithRelativeUrl_ReturnsResolvedUrl()
    {
        // Arrange
        string baseUrl = "https://example.com";
        string relativeUrl = "page";

        // Act
        string result = Utils.ResolveUrl(baseUrl, relativeUrl);

        // Assert
        Assert.That("https://example.com/page", Is.EqualTo(result));
    }

    [Test]
    public void ResolveUrl_WithRelativeUrlAndBaseUrlWithoutTrailingSlash_ReturnsResolvedUrl()
    {
        // Arrange
        string baseUrl = "https://example.com";
        string relativeUrl = "page";

        // Act
        string result = Utils.ResolveUrl(baseUrl, relativeUrl);

        // Assert
        Assert.That("https://example.com/page", Is.EqualTo(result));
    }

    [Test]
    public void ResolveUrl_WithEmptyRelativeUrl_ReturnsBaseUrl()
    {
        // Arrange
        string baseUrl = "https://example.com";
        string relativeUrl = "";

        // Act
        string result = Utils.ResolveUrl(baseUrl, relativeUrl);

        // Assert
        Assert.That(baseUrl, Is.EqualTo(result));
    }

    [Test]
    public void ResolveUrl_WithNullRelativeUrl_ReturnsBaseUrl()
    {
        // Arrange
        string baseUrl = "https://example.com";
        string relativeUrl = null;

        // Act
        string result = Utils.ResolveUrl(baseUrl, relativeUrl);

        // Assert
        Assert.That(baseUrl, Is.EqualTo(result));
    }
}
