using LyricsFinder.Contracts;

namespace LyricsFinder.Services;

public interface ITrackFileService
{
    public IEnumerable<string> GetTracksInDirectory(string directoryPath, bool recursive);
    public TrackFileTags GetTrackTagsAsync(string trackPath);

    public Task<SaveLyricsResult> SaveLyricsForTrackAsync(string trackPath, string? lyrics, bool overwriteExisting = false, bool backupExisting = true, string lyricsFileExtension = "lrc",
        CancellationToken cancellationToken = default);

    public Task<SaveLyricsResult> SaveLyricsAsync(string lyricsPath, string? lyrics, bool overwriteExisting = false, bool backupExisting = true, string lyricsFileExtension = "lrc",
        CancellationToken cancellationToken = default);
}