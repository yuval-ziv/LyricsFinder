using System.Net;
using System.Text.RegularExpressions;
using Flurl;
using Flurl.Http;
using LyricsFinder.Configurations;
using LyricsFinder.Contracts;
using LyricsFinder.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static System.StringComparison;

namespace LyricsFinder.Providers;

public partial class ShironetLyricsProvider(ILogger<ShironetLyricsProvider> logger, IOptionsMonitor<ShironetConfiguration> configurationMonitor, IApiInjectorService apiInjectorService)
    : ILyricsProvider
{
    private readonly ShironetConfiguration _configuration = configurationMonitor.CurrentValue;

    [GeneratedRegex(
        """\s*<a href="(?<LyricsApi>\/artist\?type=lyrics.*)" class=".*">\s*(?<Title>.*)<\/a>\s*-\s*<a href="(?<ArtistApi>\/artist\?lang=1.*)" class=".*">\s*(?<Artist>.*)<\/a>""")]
    private static partial Regex SongsPattern();

    [GeneratedRegex("""<span  itemprop="Lyrics" class="artist_lyrics_text">(?<LyricsSection>.*)<\/span>""")]
    private static partial Regex LyricsSectionPattern();

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagPattern();

    public async Task<LyricsSearchResult> FindLyricsAsync(string artist, string title, string? album = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Finding lyrics for {Artist} - {Title}", artist, title);

            ICollection<ShironetSongSearchResult> allMatches = (await FreeTextSearchAsync(artist, title, cancellationToken)).ToList();
            ShironetSongSearchResult? result = GetBestPossibleMatch(allMatches, artist, title);

            if (result is null)
            {
                logger.LogWarning("No match found for {Artist} - {Title}. Possible matches found are: {PossibleMatchesJson}", artist, title,
                    JsonConvert.SerializeObject(allMatches));
                return LyricsSearchResult.Failure($"Couldn't find lyrics with a partial match to the artist {artist} or the title {title}");
            }

            logger.LogInformation("Found result for {Artist} - {Title}. Artist API is {ArtistApi} and lyrics API is {LyricsApi}",
                artist, title, result.ArtistApi, result.LyricsApi);

            string lyrics = await GetLyricsAsync(result.LyricsApi, cancellationToken);

            logger.LogInformation("Successfully found lyrics for {Artist} - {Title}", artist, title);

            return LyricsSearchResult.Success(new LyricsContent { Artist = result.Artist, Title = result.Title, Lyrics = lyrics });
        }
        catch (Exception exception)
        {
            return LyricsSearchResult.Failure(exception);
        }
    }

    private async Task<IEnumerable<ShironetSongSearchResult>> FreeTextSearchAsync(string artist, string title, CancellationToken cancellationToken)
    {
        string urlPattern = Url.Decode(Url.Combine(_configuration.BaseUrl, _configuration.AllSearchApiPattern), true);
        string searchUrl = apiInjectorService.InjectAll(urlPattern, artist, title: title);

        logger.LogDebug("Fetching results for {Artist} - {Title} from {SearchUrl}", artist, title, searchUrl);
        IFlurlResponse searchResponse = await searchUrl.WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:126.0) Gecko/20100101 Firefox/126.0").GetAsync(cancellationToken: cancellationToken);

        if (searchResponse.StatusCode != (int)HttpStatusCode.OK)
            logger.LogWarning("Got status code {StatusCode} {StatusCodeName} from {SearchUrl}", searchResponse.StatusCode, (HttpStatusCode)searchResponse.StatusCode, searchUrl);

        string searchContent = await searchResponse.GetStringAsync();

        if (searchContent.Contains("Please solve this CAPTCHA in helping us understand your behavior to grant access"))
        {
            logger.LogWarning("CAPTCHA needed. Need to throttle provider Shironet provider");
            return Enumerable.Empty<ShironetSongSearchResult>();
        }

        MatchCollection matches = SongsPattern().Matches(searchContent);
        logger.LogDebug("Found {AmountOfResults} results for {Artist} - {Title}", matches.Count, artist, title);

        return matches.Select(match => new ShironetSongSearchResult(match));
    }

    /// <summary>
    ///     Returns the best possible match i.e. <br />
    ///     <ul>
    ///         Order
    ///         <li>Exact match of both artist and title</li>
    ///         <li>Exact match of both artist or title</li>
    ///         <li>Partial match of both artist and title</li>
    ///         <li>Partial match of both artist or title</li>
    ///     </ul>
    /// </summary>
    /// <param name="artist">The artist you want to match</param>
    /// <param name="title">The title you want to match</param>
    /// <param name="allMatches">All <see cref="ShironetSongSearchResult" /> that might match the artist and title</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>Best possible match, null otherwise</returns>
    private ShironetSongSearchResult? GetBestPossibleMatch(ICollection<ShironetSongSearchResult> allMatches, string artist, string title, StringComparison comparisonType = OrdinalIgnoreCase)
    {
        logger.LogDebug("Trying to get best possible match for {Artist} - {Title} out of {AmountOfMatches} matches", artist, title, allMatches.Count);
        List<ShironetSongSearchResult> artistOrTitlePartialMatch = allMatches.Where(result => ArtistOrTitleMatchesPartially(result, artist, title, comparisonType)).ToList();

        if (artistOrTitlePartialMatch.Count == 0) return null;

        logger.LogDebug("Found {AmountOfMatches} partial matches for artist {Artist} or title {Title}", artistOrTitlePartialMatch.Count, artist, title);

        List<ShironetSongSearchResult> artistOrTitleExactMatch = artistOrTitlePartialMatch.Where(result => ArtistOrTitleMatchExactly(result, artist, title, comparisonType)).ToList();

        if (artistOrTitleExactMatch.Count == 0)
        {
            logger.LogDebug("Found no exact matches for artist {Artist} or title {Title}", artist, title);
            return GetBestPartialMatch(artist, title, artistOrTitlePartialMatch, comparisonType);
        }

        logger.LogDebug("Found {AmountOfMatches} exact matches for artist {Artist} or title {Title}", artistOrTitleExactMatch.Count, artist, title);

        List<ShironetSongSearchResult> artistAndTitleExactMatch = artistOrTitleExactMatch.Where(result => ArtistAndTitleMatchExactly(result, artist, title, comparisonType)).ToList();

        if (artistAndTitleExactMatch.Count == 0)
        {
            logger.LogDebug("Found no exact matches for artist {Artist} and title {Title}. Returning the first exact artist or title match", artist, title);
            return artistOrTitleExactMatch.First();
        }

        logger.LogDebug("Found {AmountOfMatches} exact matches for artist {Artist} and title {Title}. Returning the first one", artistAndTitleExactMatch.Count, artist, title);

        return artistAndTitleExactMatch.First();
    }

    private ShironetSongSearchResult GetBestPartialMatch(string artist, string title, IReadOnlyCollection<ShironetSongSearchResult> artistOrTitlePartialMatch, StringComparison comparisonType)
    {
        List<ShironetSongSearchResult> artistAndTitlePartialMatches = artistOrTitlePartialMatch.Where(result => ArtistAndTitleMatchesPartially(result, artist, title, comparisonType)).ToList();

        if (artistAndTitlePartialMatches.Count == 0)
        {
            logger.LogDebug("Found no partial matches for artist {Artist} and title {Title}. Returning the first partial artist or title match", artist, title);
            return artistOrTitlePartialMatch.First();
        }

        logger.LogDebug("Found {AmountOfMatches} partial matches for artist {Artist} and title {Title}. Returning the first one", artistAndTitlePartialMatches.Count, artist, title);
        return artistAndTitlePartialMatches.First();
    }

    private static bool ArtistOrTitleMatchesPartially(ShironetSongSearchResult result, string artist, string title, StringComparison comparisonType)
    {
        return result.Artist.Contains(artist, comparisonType) || result.Title.Contains(title, comparisonType);
    }

    private static bool ArtistOrTitleMatchExactly(ShironetSongSearchResult result, string artist, string title, StringComparison comparisonType)
    {
        return string.Equals(result.Artist, artist, comparisonType) || string.Equals(result.Title, title, comparisonType);
    }

    private static bool ArtistAndTitleMatchesPartially(ShironetSongSearchResult result, string artist, string title, StringComparison comparisonType)
    {
        return result.Artist.Contains(artist, comparisonType) && result.Title.Contains(title, comparisonType);
    }


    private static bool ArtistAndTitleMatchExactly(ShironetSongSearchResult result, string artist, string title, StringComparison comparisonType)
    {
        return string.Equals(result.Artist, artist, comparisonType) && string.Equals(result.Title, title, comparisonType);
    }

    private async Task<string> GetLyricsAsync(string lyricsApi, CancellationToken cancellationToken)
    {
        string lyricsUrl = Url.Combine(_configuration.BaseUrl, lyricsApi);
        string lyricsContent = await lyricsUrl.GetStringAsync(cancellationToken: cancellationToken);

        string lyricsSection = LyricsSectionPattern().Match(lyricsContent).Groups["LyricsSection"].Value;
        return HtmlTagPattern().Replace(lyricsSection, string.Empty);
    }

    private class ShironetSongSearchResult
    {
        public ShironetSongSearchResult(Match match)
        {
            Artist = HtmlTagPattern().Replace(match.Groups["Artist"].Value, string.Empty);
            Title = HtmlTagPattern().Replace(match.Groups["Title"].Value, string.Empty);
            LyricsApi = HtmlTagPattern().Replace(match.Groups["LyricsApi"].Value, string.Empty);
            ArtistApi = HtmlTagPattern().Replace(match.Groups["ArtistApi"].Value, string.Empty);
        }

        public string Artist { get; }
        public string Title { get; }
        public string LyricsApi { get; }
        public string ArtistApi { get; }
    }
}