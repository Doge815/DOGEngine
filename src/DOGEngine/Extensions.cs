namespace DOGEngine;

public static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (T obj in collection)
            action(obj);
    }
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) => source.Select((item, index) => (item, index));
}