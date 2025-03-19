using SeoTool;

namespace set_tool_test;

public class HtmlUtilsTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ExtractLinks_ReturnsCorrectLinks()
    {
        // Arrange
        var html = "<html><body><a href=\"http://example.com\">Example</a></body></html>";
        var expectedLinks = new List<string> { "http://example.com" };

        // Act
        var result = HtmlUtils.ExtractLinks(html);

        // Assert
        Assert.That(result, Is.EquivalentTo(expectedLinks));
    }

    [Test]
    public void ExtractImages_ReturnsCorrectImages()
    {
        // Arrange
        var html = "<html><body><img src=\"http://example.com/image.jpg\" /></body></html>";
        var expectedImages = new List<string> { "http://example.com/image.jpg" };

        // Act
        var result = HtmlUtils.ExtractImages(html);

        // Assert
        Assert.That(result, Is.EquivalentTo(expectedImages));
    }


    [Test]
    public void ExtractTitle_ReturnsCorrectTitle()
    {
        // Arrange
        var html = "<html><head><title>Test Title</title></head><body></body></html>";
        var expectedTitle = "Test Title";

        // Act
        var result = HtmlUtils.ExtractTitle(html);

        // Assert
        Assert.That(result, Is.EqualTo(expectedTitle));
    }

    [Test]
    public void ExtractTitle_ReturnsNoTitleFound_WhenTitleTagIsMissing()
    {
        // Arrange
        var html = "<html><head></head><body></body></html>";
        var expectedTitle = "No title found";

        // Act
        var result = HtmlUtils.ExtractTitle(html);

        // Assert
        Assert.That(result, Is.EqualTo(expectedTitle));
    }

    [Test]
    public void ExtractTitle_ReturnsNoTitleFound_WhenHtmlIsEmpty()
    {
        // Arrange
        var html = "";
        var expectedTitle = "No title found";

        // Act
        var result = HtmlUtils.ExtractTitle(html);

        // Assert
        Assert.That(result, Is.EqualTo(expectedTitle));
    }

    [Test]
    public void ExtractTitle_IgnoresWhitespaceAroundTitle()
    {
        // Arrange
        var html = "<html><head><title>   Test Title   </title></head><body></body></html>";
        var expectedTitle = "Test Title";

        // Act
        var result = HtmlUtils.ExtractTitle(html);

        // Assert
        Assert.That(result, Is.EqualTo(expectedTitle));
    }

    [Test]
    public void ExtractTitle_ReturnsFirstTitle_WhenMultipleTitleTagsExist()
    {
        // Arrange
        var html = "<html><head><title>First Title</title><title>Second Title</title></head><body></body></html>";
        var expectedTitle = "First Title";

        // Act
        var result = HtmlUtils.ExtractTitle(html);

        // Assert
        Assert.That(result, Is.EqualTo(expectedTitle));
    }
}