namespace O2DESNet.Standard;

public interface IEntity
{
    int Index { get; }

    // Expose primitive identifier to avoid dependency on EntityId in abstractions
    string Id { get; }
}

// Generic abstraction to opt-in to strongly typed identifiers without coupling the base interface
public interface IEntity<out TId> : IEntity
{
    new TId Id { get; }
}
