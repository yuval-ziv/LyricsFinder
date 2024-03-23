using System.Text;

namespace LyricsFinder.Utils.ExtensionMethods;

public static class StringExtensionMethods
{
    public static string ReplaceAll(this string s, IEnumerable<string?> oldValues, string newValue)
    {
        var stringBuilder = new StringBuilder(s);

        foreach (string oldValue in oldValues.WhereNotNull()) stringBuilder.Replace(oldValue, newValue);

        return stringBuilder.ToString();
    }

    public static bool IsNullOrWhitespace(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }
}