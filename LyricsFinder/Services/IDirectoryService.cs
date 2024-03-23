using System.Text.RegularExpressions;
using static System.IO.SearchOption;

namespace LyricsFinder.Services;

public interface IDirectoryService
{
    public IEnumerable<string> GetFiles(string path, bool recursive);
    public IEnumerable<string> GetFiles(string path, bool recursive, IEnumerable<string> extensions);
    public IEnumerable<string> GetFiles(string path, bool recursive, string pattern);
    public IEnumerable<string> GetFiles(string path, bool recursive, Regex pattern);
}

public class DirectoryService : IDirectoryService
{
    private const string MatchAllPattern = ".*";

    public IEnumerable<string> GetFiles(string path, bool recursive)
    {
        return GetFiles(path, recursive, MatchAllPattern);
    }

    public IEnumerable<string> GetFiles(string path, bool recursive, IEnumerable<string> extensions)
    {
        return GetFiles(path, recursive, MatchAnyPattern(extensions));
    }

    public IEnumerable<string> GetFiles(string path, bool recursive, string pattern)
    {
        return GetFiles(path, recursive, new Regex(pattern));
    }

    public IEnumerable<string> GetFiles(string path, bool recursive, Regex regex)
    {
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"The directory path '{path}' does not exist.");

        SearchOption searchOption = recursive ? AllDirectories : TopDirectoryOnly;

        return Directory.EnumerateFiles(path, "*", searchOption).Where(file => regex.IsMatch(Path.GetExtension(file)));
    }

    private static string MatchAnyPattern(IEnumerable<string> extensions)
    {
        return @"\b(" + string.Join("|", extensions.Select(Regex.Escape).ToArray()) + @"\b)";
    }
}