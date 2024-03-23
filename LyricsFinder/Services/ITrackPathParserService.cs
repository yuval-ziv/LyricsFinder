namespace LyricsFinder.Services;

public interface ITrackPathParserService
{
    (string? Artist, string? Album, string? Title) GetTagsFromPath(string path);
}