using System;
using System.Collections.Generic;

namespace O2DESNet.Internals;

internal class FutureEventList : SortedSet<Event>
{
    private readonly Sandbox _sandbox = null!;

    private FutureEventList() { }

    public FutureEventList(Sandbox sandbox) : base(EventComparer.Instance)
    {
        _sandbox = sandbox ?? throw new ArgumentNullException(nameof(sandbox));
    }

    public Event Add(Action action, TimeSpan scheduledTime, string? tag)
    {
        var e = new Event(_sandbox, EventIndexer.GenerateIndex(), action, scheduledTime, tag);
        base.Add(e);
        return e;
    }

    public void Reset()
    {
        base.Clear();
        EventIndexer.Reset();
    }
}
