namespace O2DESNet.Standard;

/// <summary>
/// Generic abstract entity base that supports strongly typed identifiers while still implementing the non-generic IEntity.
/// </summary>
/// <typeparam name="TId">The identifier value type.</typeparam>
public abstract class Entity<TId> : IEntity<TId>
{
    // Private Fields
    private static int _index = 0;
    private readonly TId _id;

    // Public Properties
    public int Index => _index;

    /// <summary>
    /// Strongly typed identifier.
    /// </summary>
    public TId Id => _id;

    // Explicit implementation for the non-generic abstraction, exposes Id as string
    string IEntity.Id => _id?.ToString() ?? string.Empty;

    // Constructors
    protected Entity(TId id)
    {
        _id = id;
        _index++;
    }

    protected Entity(TId id, int index)
        : this(id)
    {
        _index = index;
    }

    // Public Methods
    public override string ToString() => $"{_id}#{_index}";
}
