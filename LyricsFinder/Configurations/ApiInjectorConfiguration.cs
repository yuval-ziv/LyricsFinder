namespace LyricsFinder.Configurations;

public class ApiInjectorConfiguration
{
    private const string DefaultArtistLongPattern = "{artist}";
    private const string DefaultArtistShortPattern = "{ar}";
    private const string DefaultAlbumLongPattern = "{album}";
    private const string DefaultAlbumShortPattern = "{al}";
    private const string DefaultTitleLongPattern = "{title}";
    private const string DefaultTitleShortPattern = "{t}";

    public const string Section = "ApiInjector";
    public string ArtistLongPattern { get; init; } = DefaultArtistLongPattern;
    public string ArtistShortPattern { get; init; } = DefaultArtistShortPattern;
    public string AlbumLongPattern { get; init; } = DefaultAlbumLongPattern;
    public string AlbumShortPattern { get; init; } = DefaultAlbumShortPattern;
    public string TitleLongPattern { get; init; } = DefaultTitleLongPattern;
    public string TitleShortPattern { get; init; } = DefaultTitleShortPattern;
}