using System;

namespace O2DESNet.Internals;

[System.Diagnostics.DebuggerDisplay("{_owner.Id}#{_tag}#{_index}")]
internal class Event
{
    private readonly Sandbox _owner = null!;
    private readonly int _index;
    private readonly string? _tag;
    private readonly Action? _action;
    private readonly TimeSpan _timestamp;

    internal int Index => _index;

    internal string? Tag => _tag;

    internal Sandbox Owner => _owner;

    internal TimeSpan Timestamp => _timestamp;

    internal Action? Action => _action;

    private Event() { }

    internal Event(Sandbox owner, int eventIndex, Action action, TimeSpan timestamp, string? tag)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner), "Owner of this event instance cannot be null");
        _index = eventIndex;
        _action = action;
        _timestamp = timestamp;
        _tag = tag;
    }

    internal void Invoke()
    {
        _action?.Invoke();
    }

    public override string ToString()
    {
        return $"{_owner.Id}#{_tag}#{_index}";
    }
}
