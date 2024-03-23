namespace LyricsFinder.Contracts;

public record LyricsSearchResult
{
    private LyricsSearchResult(bool successful, LyricsContent? lyrics, string? errorMessage, Exception? exception)
    {
        Successful = successful;
        Lyrics = lyrics;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public bool Successful { get; init; }
    public LyricsContent? Lyrics { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static LyricsSearchResult Success(LyricsContent lyrics)
    {
        return new LyricsSearchResult(true, lyrics, null, null);
    }

    public static LyricsSearchResult Failure(string errorMessage)
    {
        return new LyricsSearchResult(false, null, errorMessage, null);
    }

    public static LyricsSearchResult Failure(Exception exception)
    {
        return new LyricsSearchResult(false, null, null, exception);
    }

    public static LyricsSearchResult Failure(string errorMessage, Exception exception)
    {
        return new LyricsSearchResult(false, null, errorMessage, exception);
    }
}