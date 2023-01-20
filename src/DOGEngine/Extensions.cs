using FreeTypeSharp.Native;

namespace DOGEngine;

public static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (T obj in collection)
            action(obj);
    }
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) => source.Select((item, index) => (item, index));

    public static StringSplitEnumerator SplitLine(this string str, char split) => new (str.AsSpan(), split);
}
public ref struct StringSplitEnumerator
{
    private ReadOnlySpan<char> _span;
    private readonly char _split;
    public ReadOnlySpan<char> Current { get; private set; }

    public StringSplitEnumerator(ReadOnlySpan<char> span, char split)
    {
        _span = span;
        _split = split;
        Current = default;
    }
    public StringSplitEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        if (_span.Length == 0) 
            return false;

        var index = _span.IndexOf(_split);
        if (index == -1)
        {
            Current = _span;
            _span = ReadOnlySpan<char>.Empty;
            return true;
        }

        if (index < _span.Length - 1)
        {
            Current = _span.Slice(0, index);
            _span = _span.Slice(index + 1);
            if (Current.Length == 0) return MoveNext();
            return true;
        }

        Current = _span.Slice(0, index);
        _span = _span.Slice(index + 1);
        return Current.Length != 0;
    }

}