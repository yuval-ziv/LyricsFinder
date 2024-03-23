using LyricsFinder.Utils.ExtensionMethods;

namespace LyricsFinder.Utils;

public static class PathUtil
{
    public static string ExpandPath(string path)
    {
        path = Environment.ExpandEnvironmentVariables(path);

        if (path.StartsWith('~')) path = path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        if (!Path.IsPathRooted(path)) path = Path.Combine(Directory.GetCurrentDirectory(), path);

        // Handle platform specific separators
        return path.ReplaceAll(["\\", "/"], Path.DirectorySeparatorChar.ToString());
    }
}