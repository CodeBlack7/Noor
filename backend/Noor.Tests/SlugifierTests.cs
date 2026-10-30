using Noor.Infrastructure.Helpers;

namespace Noor.Tests;

public class SlugifierTests
{
    [Theory]
    [InlineData("Layla Everyday Burqa", "layla-everyday-burqa")]
    [InlineData("  The  Noor & Co.  ", "the-noor-and-co")]
    [InlineData("Nour's Abaya", "nours-abaya")]
    [InlineData("Silk — Mocha / Sage", "silk-mocha-sage")]
    public void Slugify_NormalisesReadableSlugs(string input, string expected)
    {
        Assert.Equal(expected, Slugifier.Slugify(input));
    }

    [Fact]
    public void Slugify_TruncatesLongValuesTo160Characters()
    {
        var slug = Slugifier.Slugify(new string('a', 200));

        Assert.Equal(160, slug.Length);
    }

    [Fact]
    public void UniqueSlug_AppendsSuffixesUntilFree()
    {
        var taken = new HashSet<string> { "layla-burqa", "layla-burqa-2" };

        var slug = Slugifier.UniqueSlug("Layla Burqa", taken.Contains);

        Assert.Equal("layla-burqa-3", slug);
    }

    [Fact]
    public void UniqueSlug_UsesFallbackForUnusableNames()
    {
        var slug = Slugifier.UniqueSlug("***", _ => false, "product");

        Assert.Equal("product", slug);
    }
}
