namespace LyricsFinder.Configurations;

public class TracksPathParserConfiguration
{
    public const string Section = "TracksPathParser";

    private const string DefaultArtistGroupKey = "artist";
    private const string DefaultAlbumGroupKey = "album";
    private const string DefaultTitleGroupKey = "title";
    private const string DefaultPattern = $@".*\\(?<{DefaultArtistGroupKey}>.*)\\(?<{DefaultAlbumGroupKey}>.*)\\(?<{DefaultTitleGroupKey}>.*)\..*";

    public string ArtistGroupKey { get; init; } = DefaultArtistGroupKey;
    public string AlbumGroupKey { get; init; } = DefaultAlbumGroupKey;
    public string TitleGroupKey { get; init; } = DefaultTitleGroupKey;
    public List<string> FileMatchPatterns { get; init; } = [DefaultPattern];
}