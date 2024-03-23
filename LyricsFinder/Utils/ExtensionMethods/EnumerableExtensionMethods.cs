namespace LyricsFinder.Utils.ExtensionMethods;

public static class EnumerableExtensionMethods
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
    {
        return enumerable.Where(item => item is not null).Cast<T>();
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable is null || !enumerable.Any();
    }
}