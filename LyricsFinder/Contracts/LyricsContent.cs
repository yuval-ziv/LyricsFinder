namespace LyricsFinder.Contracts;

public record LyricsContent
{
    public string? Artist { get; init; }
    public string? Album { get; init; }
    public string? Title { get; init; }
    public string? Lyrics { get; init; }
}