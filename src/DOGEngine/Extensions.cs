namespace DOGEngine;

public static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (T obj in collection)
            action(obj);
    }
}