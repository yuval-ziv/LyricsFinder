namespace LyricsFinder.Utils;

public static class FileUtil
{
    public static async Task<bool> ExistsAsync(string? path, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => File.Exists(path), cancellationToken);
    }

    public static async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => File.Delete(path), cancellationToken);
    }

    public static async Task<bool> MoveAsync(string sourceFileName, string destFileName, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        if (await ExistsAsync(destFileName, cancellationToken) && !overwrite)
        {
            return false;
        }

        await using Stream source = File.OpenRead(sourceFileName);
        await using Stream destination = File.Create(destFileName);
        await source.CopyToAsync(destination, cancellationToken);
        await DeleteAsync(sourceFileName, cancellationToken);

        return true;
    }

    public static bool IsValidFilePath(string? path)
    {
        return path is not null && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
    }
}