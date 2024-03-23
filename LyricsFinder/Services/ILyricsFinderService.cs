using LyricsFinder.CommandParameters;

namespace LyricsFinder.Services;

public interface ILyricsFinderService
{
    public Task FindLyricsByDirectory(CommonParameters commonParameters, DirectorySearchParameters parameters, CancellationToken cancellationToken = default);
    public Task FindLyricsByFileAsync(CommonParameters commonParameters, FileSearchParameters parameters, CancellationToken cancellationToken = default);
    public Task FindLyricsByQuery(CommonParameters commonParameters, QuerySearchParameters parameters, CancellationToken cancellationToken = default);
}