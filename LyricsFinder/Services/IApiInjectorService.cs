namespace LyricsFinder.Services;

public interface IApiInjectorService
{
    public string InjectAll(string? url, string artist = "", string album = "", string title = "");
    public string InjectArtist(string? url, string artist);
    public string InjectAlbum(string? url, string album);
    public string InjectTitle(string? url, string title);
}