using Flurl;
using LyricsFinder.Configurations;
using LyricsFinder.Utils.ExtensionMethods;
using Microsoft.Extensions.Options;

namespace LyricsFinder.Services;

public class ApiInjectorService(IOptionsMonitor<ApiInjectorConfiguration> configurationMonitor) : IApiInjectorService
{
    private readonly ApiInjectorConfiguration _configuration = configurationMonitor.CurrentValue;

    public string InjectAll(string? url, string artist = "", string album = "", string title = "")
    {
        url = InjectArtist(url, artist);
        url = InjectAlbum(url, album);
        url = InjectTitle(url, title);

        return url;
    }

    public string InjectArtist(string? url, string artist)
    {
        string encodedArtist = Url.Encode(artist);

        return url?.ReplaceAll([_configuration.ArtistShortPattern, _configuration.ArtistLongPattern], encodedArtist) ?? string.Empty;
    }

    public string InjectAlbum(string? url, string album)
    {
        string encodedAlbum = Url.Encode(album);

        return url?.ReplaceAll([_configuration.AlbumShortPattern, _configuration.AlbumLongPattern], encodedAlbum) ?? string.Empty;
    }

    public string InjectTitle(string? url, string title)
    {
        string encodedTitle = Url.Encode(title);

        return url?.ReplaceAll([_configuration.TitleShortPattern, _configuration.TitleLongPattern], encodedTitle) ?? string.Empty;
    }
}