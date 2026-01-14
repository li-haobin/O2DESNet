using System.Collections.Generic;

namespace O2DESNet.Internals;

internal sealed class EventComparer : IComparer<Event>
{
    private static readonly EventComparer _instance = new();

    private EventComparer() { }

    public static EventComparer Instance => _instance;

    public int Compare(Event x, Event y)
    {
        int compare = x.Timestamp.CompareTo(y.Timestamp);

        if (compare == 0)
            return x.Index.CompareTo(y.Index);

        return compare;
    }
}
