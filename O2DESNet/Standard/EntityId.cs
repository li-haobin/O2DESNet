using System;

namespace O2DESNet.Standard;

/// <summary>
/// Strongly typed unique identifier for an entity, normalized to a GUID string in the compact "n" format (32 digits, no hyphens).
/// </summary>
/// <remarks>
/// The underlying value is stored as a <c>Guid</c> rendered with format specifier <c>"n"</c>, e.g., <c>3f2504e04f8911d39a0c0305e82c3301</c>.
/// </remarks>
/// <example>
/// <code>
/// // Create a new unique id
/// var id = EntityId.New();
/// 
/// // Recommended: Create from factory methods
/// var fromGuid = EntityId.Create(Guid.NewGuid());
/// var fromString = EntityId.Create("3f2504e04f8911d39a0c0305e82c3301");
/// 
/// // Use as string
/// string s = (string)id;            // explicit cast to string
/// string s2 = id.ToString();        // normalized "n" format
/// 
/// // Retrieve Guid value
/// Guid g = (Guid)fromString;        // explicit cast to Guid
/// 
/// // Parse helpers
/// var parsed = EntityId.Parse(s);
/// if (EntityId.TryParse(s, out var tryId))
/// {
///     // use tryId
/// }
/// </code>
/// </example>
public readonly record struct EntityId
{
    // Private Fields
    private static readonly string _emptyNormalized = Guid.Empty.ToString("n");
    private readonly string _value = _emptyNormalized;

    // Public Properties
    /// <summary>
    /// A sentinel empty identifier equal to <see cref="Guid.Empty"/> in the normalized "n" format.
    /// </summary>
    public static EntityId Empty { get; } = new(Guid.Empty);

    /// <summary>
    /// Gets a value indicating whether this identifier is the empty value.
    /// </summary>
    public bool IsEmpty => Value == Empty.Value;

    /// <summary>
    /// Normalized GUID string (format <c>"n"</c>). Defaults to <see cref="Guid.Empty"/> when using <c>default(EntityId)</c>.
    /// </summary>
    public string Value
    {
        get => _value;
    }

    // Constructors
    /// <summary>
    /// Create a new <see cref="EntityId"/> from a <see cref="Guid"/>.
    /// </summary>
    public EntityId(Guid guid)
    {
        _value = guid.ToString("n");
    }

    /// <summary>
    /// Create a new <see cref="EntityId"/> from a GUID string. The value is validated and normalized to format <c>"n"</c>.
    /// </summary>
    /// <param name="value">Any valid GUID string in supported formats (e.g., "D", "N", "B", "P", "X").</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid GUID.</exception>
    public EntityId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("EntityId value cannot be null or whitespace.", nameof(value));
        }

        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException("EntityId value must be a valid GUID string.", nameof(value));
        }

        _value = guid.ToString("n");
    }

    // Public Methods
    /// <summary>
    /// Generate a new unique <see cref="EntityId"/>.
    /// </summary>
    public static EntityId New() => new(Guid.NewGuid());

    /// <summary>
    /// Factory method to create an <see cref="EntityId"/> from a <see cref="Guid"/>.
    /// </summary>
    public static EntityId Create(Guid guid) => new(guid);

    /// <summary>
    /// Factory method to create an <see cref="EntityId"/> from a GUID string. The input is validated and normalized.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid GUID.</exception>
    public static EntityId Create(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Try to parse a string into an <see cref="EntityId"/>.
    /// </summary>
    /// <param name="value">The input string. Any valid GUID format is accepted.</param>
    /// <param name="id">The resulting identifier when parsing succeeds; otherwise <see cref="Empty"/>.</param>
    /// <returns><see langword="true"/> if parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string? value, out EntityId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new EntityId(guid);
            return true;
        }

        id = default;
        return false;
    }

    /// <summary>
    /// Parse a string into an <see cref="EntityId"/>, throwing if invalid.
    /// </summary>
    /// <param name="value">The input string. Any valid GUID format is accepted.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid GUID.</exception>
    public static EntityId Parse(string value) => new(value);

    /// <summary>
    /// Implicit conversion from <see cref="Guid"/> to <see cref="EntityId"/>.
    /// </summary>
    public static implicit operator EntityId(Guid value) => new(value);

    /// <summary>
    /// Implicit conversion from <see cref="string"/> to <see cref="EntityId"/>.
    /// </summary>
    public static implicit operator EntityId(string value) => new(value);

    /// <summary>
    /// Explicit conversion from <see cref="EntityId"/> to <see cref="Guid"/>.
    /// </summary>
    public static explicit operator Guid(EntityId id) => Guid.Parse(id.Value);

    /// <summary>
    /// Explicit conversion from <see cref="EntityId"/> to <see cref="string"/>.
    /// </summary>
    public static explicit operator string(EntityId id) => id.Value;
}

