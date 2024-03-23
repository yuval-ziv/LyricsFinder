using LyricsFinder.Contracts;

namespace LyricsFinder.Providers;

public interface ILyricsProvider
{
    public Task<LyricsSearchResult> FindLyricsAsync(string artist, string title, string? album = null, CancellationToken cancellationToken = default);
}