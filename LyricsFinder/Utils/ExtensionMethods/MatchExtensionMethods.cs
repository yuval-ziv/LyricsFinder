using System.Text.RegularExpressions;

namespace LyricsFinder.Utils.ExtensionMethods;

public static class MatchExtensionMethods
{
    public static string? GetValueOrDefault(this Match match, string groupName, string? defaultValue = null)
    {
        return match.Groups[groupName].Success ? match.Groups[groupName].Value : defaultValue;
    }
}