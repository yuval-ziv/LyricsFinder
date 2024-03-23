namespace LyricsFinder.Configurations;

public class ShironetConfiguration
{
    public const string Section = "Shironet";
    public string? BaseUrl { get; init; }
    public string? AllSearchApiPattern { get; init; }
    public string? SongsSearchApiPattern { get; init; }
    public string? ArtistSearchApiPattern { get; init; }
}