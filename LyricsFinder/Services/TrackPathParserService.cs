using System.Text.RegularExpressions;
using LyricsFinder.Configurations;
using LyricsFinder.Utils.ExtensionMethods;
using Microsoft.Extensions.Options;

namespace LyricsFinder.Services;

public class TrackPathParserService(IOptionsMonitor<TracksPathParserConfiguration> configurationMonitor) : ITrackPathParserService
{
    private readonly TracksPathParserConfiguration _configuration = configurationMonitor.CurrentValue;

    public (string? Artist, string? Album, string? Title) GetTagsFromPath(string path)
    {
        List<(string? Artist, string? Album, string? Title)> tagsOptions = _configuration.FileMatchPatterns.Select(pattern => Regex.Match(path, pattern)).Select(GetTagsFromMatch).ToList();

        return GetBestTags(tagsOptions);
    }

    private static (string? Artist, string? Album, string? Title) GetBestTags(IReadOnlyCollection<(string? Artist, string? Album, string? Title)> tagsOptions)
    {
        List<(string? Artist, string? Album, string? Title)> allTagsAreNotNull = tagsOptions.Where(tags => tags.Artist is not null && tags.Album is not null && tags.Title is not null).ToList();

        if (allTagsAreNotNull.Count > 0) return allTagsAreNotNull.First();

        List<(string? Artist, string? Album, string? Title)> titleAndArtistAreNotNull = tagsOptions.Where(tags => tags.Artist is not null && tags.Title is not null).ToList();

        return titleAndArtistAreNotNull.Count > 0 ? titleAndArtistAreNotNull.First() : tagsOptions.First();
    }

    private (string? Artist, string? Album, string? Title) GetTagsFromMatch(Match match)
    {
        string? artist = match.GetValueOrDefault(_configuration.ArtistGroupKey);
        string? album = match.GetValueOrDefault(_configuration.AlbumGroupKey);
        string? title = match.GetValueOrDefault(_configuration.TitleGroupKey);

        return (artist, album, title);
    }
}