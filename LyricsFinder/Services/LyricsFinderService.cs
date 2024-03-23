using LyricsFinder.CommandParameters;
using LyricsFinder.Contracts;
using LyricsFinder.Providers;
using LyricsFinder.Utils.ExtensionMethods;
using Microsoft.Extensions.Logging;

namespace LyricsFinder.Services;

public class LyricsFinderService(ILogger<LyricsFinderService> logger, IEnumerable<ILyricsProvider> providers, ITrackFileService trackFileService) : ILyricsFinderService
{
    private readonly List<ILyricsProvider> _providers = providers.ToList();

    public async Task FindLyricsByDirectory(CommonParameters commonParameters, DirectorySearchParameters parameters, CancellationToken cancellationToken = default)
    {
        LogCommonParameters(commonParameters);

        foreach (string trackPath in trackFileService.GetTracksInDirectory(parameters.Path, parameters.Recursive))
        {
            LyricsContent? result = await GetLyricsByPath(trackPath);

            if (commonParameters.DryRun || result is null) continue;

            SaveLyricsResult saveLyricsResult = await trackFileService.SaveLyricsForTrackAsync(trackPath, result.Lyrics, commonParameters.OverwriteExisting, commonParameters.BackupExisting,
                cancellationToken: cancellationToken);
            LogSaveLyricsResult(saveLyricsResult);
        }
    }

    public async Task FindLyricsByFileAsync(CommonParameters commonParameters, FileSearchParameters parameters, CancellationToken cancellationToken = default)
    {
        LogCommonParameters(commonParameters);

        LyricsContent? result = await GetLyricsByPath(parameters.Path);

        if (commonParameters.DryRun || result is null) return;

        SaveLyricsResult saveLyricsResult = await trackFileService.SaveLyricsForTrackAsync(parameters.Path, result.Lyrics, commonParameters.OverwriteExisting, commonParameters.BackupExisting,
            cancellationToken: cancellationToken);
        LogSaveLyricsResult(saveLyricsResult);
    }

    public async Task FindLyricsByQuery(CommonParameters commonParameters, QuerySearchParameters parameters, CancellationToken cancellationToken = default)
    {
        LogCommonParameters(commonParameters);

        logger.LogInformation("DryRun is set to {DryRun}", commonParameters.DryRun);
        LyricsSearchResult[] results = await Task.WhenAll(QueryAllProviders(parameters.Artist, parameters.Title));

        if (results.IsNullOrEmpty())
        {
            logger.LogWarning("No match found for artist {Artist} and title {Title}", parameters.Artist, parameters.Title);
            return;
        }

        LyricsContent? result = GetBestResult(results);

        if (result is null)
        {
            logger.LogWarning("No match found for artist {Artist} and title {Title}", parameters.Artist, parameters.Title);
            return;
        }

        if (commonParameters.DryRun) return;

        SaveLyricsResult saveLyricsResult = await trackFileService.SaveLyricsAsync(parameters.Output, result.Lyrics, commonParameters.OverwriteExisting, commonParameters.BackupExisting,
            cancellationToken: cancellationToken);
        LogSaveLyricsResult(saveLyricsResult);
    }

    private void LogCommonParameters(CommonParameters commonParameters)
    {
        logger.LogDebug("Will limit concurrent requests per provider to {ConcurrentRequestLimit}", commonParameters.ConcurrentRequestLimit);

        if (commonParameters.DryRun)
        {
            logger.LogInformation("Dry run is toggled on. Will not save lyrics files");
            return;
        }

        logger.LogInformation("Overwriting existing is toggled {OverwriteExistingToggleStatus}. Will {OverwriteExistingAction} existing files", commonParameters.OverwriteExisting ? "on" : "off",
            commonParameters.OverwriteExisting ? "overwrite" : "not overwrite");

        if (!commonParameters.OverwriteExisting) return;

        logger.LogInformation("Backup existing tracks is toggled {BackupExistingToggleStatus}. Will {BackupExistingAction} existing files", commonParameters.BackupExisting ? "on" : "off",
            commonParameters.BackupExisting ? "backup" : "not backup");
    }

    private async Task<LyricsContent?> GetLyricsByPath(string path)
    {
        logger.LogInformation("Getting lyrics for file in {FilePath}", path);
        (string artist, string album, string title) = trackFileService.GetTrackTagsAsync(path);
        logger.LogInformation("Searching lyrics by track tags artist - {Artist}, album - {Album}, title - {Title} in {ProvidersCount}", artist, album, title, _providers.Count);
        LyricsSearchResult[] results = await Task.WhenAll(QueryAllProviders(artist, title));

        if (results.IsNullOrEmpty())
        {
            logger.LogWarning("No match found for artist {Artist} and album {Album} and title {Title}", artist, album, title);
            return null;
        }

        LyricsContent? lyrics = GetBestResult(results);

        if (lyrics is null) logger.LogWarning("No match found for artist {Artist} and album {Album} and title {Title}", artist, album, title);

        return lyrics;
    }

    private IEnumerable<Task<LyricsSearchResult>> QueryAllProviders(string artist, string title, string? album = null)
    {
        return _providers.Select(provider => provider.FindLyricsAsync(artist, title, album));
    }

    private static LyricsContent? GetBestResult(IEnumerable<LyricsSearchResult> results)
    {
        return results.Where(result => result.Successful).Select(result => result.Lyrics).Where(content => content is not null).Cast<LyricsContent>().FirstOrDefault();
    }

    private void LogSaveLyricsResult(SaveLyricsResult saveLyricsResult)
    {
        if (saveLyricsResult.Saved)
        {
            if (saveLyricsResult.Overwrite)
                logger.LogInformation("Successfully saved lyrics in {SavePath}. Backed up older lyrics in {BackupPath}", saveLyricsResult.SavePath, saveLyricsResult.BackupPath);
            else
                logger.LogInformation("Successfully saved lyrics in {SavePath}", saveLyricsResult.SavePath);
        }
        else
        {
            logger.LogError(saveLyricsResult.Exception, "Failed to save lyrics. Status is {SaveResultStatus}", saveLyricsResult.Status);
        }
    }
}