using ATL;
using LyricsFinder.Configurations;
using LyricsFinder.Contracts;
using LyricsFinder.Utils;
using LyricsFinder.Utils.ExtensionMethods;
using Microsoft.Extensions.Options;

namespace LyricsFinder.Services;

public class TrackFileService(IOptionsMonitor<TrackFileConfiguration> configurationMonitor, ITrackPathParserService trackPathParserService, IDirectoryService directoryService) : ITrackFileService
{
    private readonly TrackFileConfiguration _configuration = configurationMonitor.CurrentValue;

    public IEnumerable<string> GetTracksInDirectory(string directoryPath, bool recursive)
    {
        return directoryService.GetFiles(directoryPath, recursive, _configuration.MusicExtensions);
    }

    public TrackFileTags GetTrackTagsAsync(string trackPath)
    {
        var track = new Track(trackPath);
        string artist = track.Artist;
        string album = track.Album;
        string title = track.Title;

        if (!artist.IsNullOrWhitespace() && !album.IsNullOrWhitespace() && !title.IsNullOrWhitespace()) return new TrackFileTags(artist, album, title);

        (string? Artist, string? Album, string? Title) tags = trackPathParserService.GetTagsFromPath(trackPath);

        artist = artist.IsNullOrWhitespace() && tags.Artist is not null ? tags.Artist : artist;
        album = album.IsNullOrWhitespace() && tags.Album is not null ? tags.Album : album;
        title = title.IsNullOrWhitespace() && tags.Title is not null ? tags.Title : title;

        return new TrackFileTags(artist, album, title);
    }

    public async Task<SaveLyricsResult> SaveLyricsForTrackAsync(string trackPath, string? lyrics, bool overwriteExisting = false, bool backupExisting = true, string lyricsFileExtension = "lrc",
        CancellationToken cancellationToken = default)
    {
        string lyricsPath = Path.ChangeExtension(trackPath, lyricsFileExtension);

        return await SaveLyricsAsync(lyricsPath, lyrics, overwriteExisting, backupExisting, lyricsFileExtension, cancellationToken);
    }

    public async Task<SaveLyricsResult> SaveLyricsAsync(string lyricsPath, string? lyrics, bool overwriteExisting = false, bool backupExisting = true, string lyricsFileExtension = "lrc",
        CancellationToken cancellationToken = default)
    {
        if (!FileUtil.IsValidFilePath(lyricsPath)) return SaveLyricsResult.Failure(SaveLyricsResultStatusEnum.InvalidPath);

        bool lyricsFileExists = await FileUtil.ExistsAsync(lyricsPath, cancellationToken);
        if (lyricsFileExists && !overwriteExisting) return SaveLyricsResult.Failure(SaveLyricsResultStatusEnum.Existing);

        try
        {
            string? existingLyricsFilePathBackup = await BackupExistingIfNeededAsync(lyricsPath, backupExisting, lyricsFileExtension, lyricsFileExists, cancellationToken);
            string expandedLyricsPath = await SaveFileAsync(lyricsPath, lyrics, cancellationToken);
            return SaveLyricsResult.Success(expandedLyricsPath, existingLyricsFilePathBackup);
        }
        catch (UnauthorizedAccessException exception)
        {
            return SaveLyricsResult.Failure(SaveLyricsResultStatusEnum.Unauthorized, exception);
        }
    }

    private static async Task<string?> BackupExistingIfNeededAsync(string lyricsPath, bool backupExisting, string lyricsFileExtension, bool lyricsFileExists, CancellationToken cancellationToken)
    {
        if (!backupExisting || !lyricsFileExists) return null;

        string existingLyricsFilePathBackup = Path.ChangeExtension(lyricsPath, lyricsFileExtension + ".bk");
        await FileUtil.MoveAsync(lyricsPath, existingLyricsFilePathBackup, true, cancellationToken);
        return existingLyricsFilePathBackup;
    }

    private static async Task<string> SaveFileAsync(string lyricsPath, string? lyrics, CancellationToken cancellationToken)
    {
        string expandedLyricsPath = PathUtil.ExpandPath(lyricsPath);
        CreateDirectoryIfNotExists(expandedLyricsPath);
        await File.WriteAllTextAsync(expandedLyricsPath, lyrics, cancellationToken);
        return expandedLyricsPath;
    }

    private static void CreateDirectoryIfNotExists(string expandedLyricsPath)
    {
        string directory = Path.GetDirectoryName(expandedLyricsPath)!;

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
    }
}