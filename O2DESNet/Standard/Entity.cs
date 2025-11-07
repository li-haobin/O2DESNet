namespace O2DESNet.Standard;

public abstract class Entity : IEntity
{
    private static int _index = 0;
    private readonly EntityId _id;

    public Entity(EntityId id)
    {
        _id = id;
        _index++;
    }

    public Entity(EntityId id, int index)
        : this(id)
    {
        _index = index;
    }

    public int Index => _index;

    // Expose primitive identifier per IEntity while keeping strong type internally
    public string Id => _id.ToString();

    // Give derived classes access to the strong typed id without leaking it into the interface contract
    protected EntityId StrongId => _id;

    public override string ToString() => $"{_id}#{_index}";
}
