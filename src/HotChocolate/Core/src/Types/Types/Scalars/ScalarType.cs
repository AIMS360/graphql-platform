using HotChocolate.Language;
using HotChocolate.Properties;
using HotChocolate.Types.Descriptors.Configurations;
using HotChocolate.Utilities;
using static HotChocolate.Serialization.SchemaDebugFormatter;

#nullable enable

namespace HotChocolate.Types;

/// <summary>
/// Scalar types represent primitive leaf values in a GraphQL type system.
/// GraphQL responses take the form of a hierarchical tree;
/// the leaves on these trees are GraphQL scalars.
/// </summary>
public abstract partial class ScalarType
    : TypeSystemObject<ScalarTypeConfiguration>
    , IScalarTypeDefinition
    , ILeafType
    , IHasRuntimeType
{
    private Uri? _specifiedBy;

    /// <summary>
    /// Gets the type kind.
    /// </summary>
    public TypeKind Kind => TypeKind.Scalar;

    /// <summary>
    /// Defines if this scalar binds implicitly to its runtime type or
    /// if it has to be explicitly assigned to it.
    /// </summary>
    public BindingBehavior Bind { get; }

    /// <summary>
    /// The .NET type representation of this scalar.
    /// </summary>
    public abstract Type RuntimeType { get; }

    /// <summary>
    /// Gets the schema coordinate of this scalar type.
    /// </summary>
    public SchemaCoordinate Coordinate => new(Name, ofDirective: false);

    /// <summary>
    /// Gets the optional description of this scalar type.
    /// </summary>
    public Uri? SpecifiedBy
    {
        get => _specifiedBy;
        protected set
        {
            if (IsCompleted)
            {
                throw new InvalidOperationException(
                    TypeResources.TypeSystemObject_DescriptionImmutable);
            }
            _specifiedBy = value;
        }
    }

    /// <summary>
    /// Gets the directives of this scalar type.
    /// </summary>
    public DirectiveCollection Directives { get; private set; }

    IReadOnlyDirectiveCollection IDirectivesProvider.Directives
        => Directives.AsReadOnlyDirectiveCollection();

    /// <summary>
    /// Provides access to the schema type converter.
    /// </summary>
    protected ITypeConverter Converter => _converter;

    /// <summary>
    /// Defines if the specified <paramref name="type"/> is assignable from the current <see cref="ScalarType"/>.
    /// </summary>
    /// <param name="type">
    /// The type that shall be checked.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified <paramref name="type"/> is assignable from the current <see cref="ScalarType"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool IsAssignableFrom(ITypeDefinition type)
        => ReferenceEquals(type, this);

    public bool Equals(IType? other) => ReferenceEquals(other, this);

    /// <summary>
    /// Defines if the specified <paramref name="valueSyntax" />
    /// can be parsed by this scalar.
    /// </summary>
    /// <param name="valueSyntax">
    /// The literal that shall be checked.
    /// </param>
    /// <returns>
    /// <c>true</c> if the literal can be parsed by this scalar;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="valueSyntax" /> is <c>null</c>.
    /// </exception>
    public abstract bool IsInstanceOfType(IValueNode valueSyntax);

    /// <summary>
    /// Defines if the specified <paramref name="runtimeValue" />
    /// is an instance of this type.
    /// </summary>
    /// <param name="runtimeValue">
    /// A value representation of this type.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value is a value of this type;
    /// otherwise, <c>false</c>.
    /// </returns>
    public virtual bool IsInstanceOfType(object? runtimeValue)
    {
        if (runtimeValue is null)
        {
            return true;
        }

        return RuntimeType.IsInstanceOfType(runtimeValue);
    }

    /// <summary>
    /// Parses the specified <paramref name="valueSyntax" />
    /// to the .NET representation of this type.
    /// </summary>
    /// <param name="valueSyntax">
    ///     The literal that shall be parsed.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="valueSyntax" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="SerializationException">
    /// The specified <paramref name="valueSyntax" /> cannot be parsed
    /// by this scalar.
    /// </exception>
    public abstract object? ParseLiteral(IValueNode valueSyntax);

    /// <summary>
    /// Parses the .NET value representation to a value literal.
    /// </summary>
    /// <param name="runtimeValue">
    /// The .NET value representation.
    /// </param>
    /// <returns>
    /// Returns a GraphQL literal representing the .NET value.
    /// </returns>
    /// <exception cref="SerializationException">
    /// The specified <paramref name="runtimeValue" /> cannot be parsed
    /// by this scalar.
    /// </exception>
    public abstract IValueNode ParseValue(object? runtimeValue);

    /// <summary>
    /// Parses a result value of this scalar into a GraphQL value syntax representation.
    /// </summary>
    /// <param name="resultValue">
    /// A result value representation of this type.
    /// </param>
    /// <returns>
    /// Returns a GraphQL value syntax representation of the <paramref name="resultValue"/>.
    /// </returns>
    /// <exception cref="SerializationException">
    /// Unable to parse the given <paramref name="resultValue"/>
    /// into a GraphQL value syntax representation of this type.
    /// </exception>
    public abstract IValueNode ParseResult(object? resultValue);

    /// <summary>
    /// Serializes the .NET value representation.
    /// </summary>
    /// <param name="runtimeValue">
    /// The .NET value representation.
    /// </param>
    /// <returns>
    /// Returns the serialized value.
    /// </returns>
    /// <exception cref="SerializationException">
    /// The specified <paramref name="runtimeValue" /> cannot be serialized
    /// by this scalar.
    /// </exception>
    public virtual object? Serialize(object? runtimeValue)
    {
        if (TrySerialize(runtimeValue, out var s))
        {
            return s;
        }

        throw new SerializationException(
            ErrorBuilder.New()
                .SetMessage(TypeResourceHelper.Scalar_Cannot_Serialize(Name))
                .SetExtension("actualValue", runtimeValue?.ToString() ?? "null")
                .SetExtension("actualType", runtimeValue?.GetType().FullName ?? "null")
                .Build(),
            this);
    }

    /// <summary>
    /// Tries to serializes the .NET value representation to the output format.
    /// </summary>
    /// <param name="runtimeValue">
    /// The .NET value representation.
    /// </param>
    /// <param name="resultValue">
    /// The serialized value.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value was correctly serialized; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool TrySerialize(object? runtimeValue, out object? resultValue);

    /// <summary>
    /// Deserializes the serialized value to it`s .NET value representation.
    /// </summary>
    /// <param name="resultValue">
    /// The serialized value representation.
    /// </param>
    /// <returns>
    /// Returns the .NET value representation.
    /// </returns>
    /// <exception cref="SerializationException">
    /// The specified <paramref name="resultValue" /> cannot be deserialized
    /// by this scalar.
    /// </exception>
    public virtual object? Deserialize(object? resultValue)
    {
        if (TryDeserialize(resultValue, out var v))
        {
            return v;
        }

        throw new SerializationException(
            TypeResourceHelper.Scalar_Cannot_Deserialize(Name),
            this);
    }

    /// <summary>
    /// Tries to deserializes the value from the output format to the .NET value representation.
    /// </summary>
    /// <param name="resultValue">
    /// The serialized value.
    /// </param>
    /// <param name="runtimeValue">
    /// The .NET value representation.
    /// </param>
    /// <returns>
    /// <c>true</c> if the serialized value was correctly deserialized; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool TryDeserialize(object? resultValue, out object? runtimeValue);

    protected bool TryConvertSerialized<T>(
        object serialized,
        ValueKind expectedKind,
        out T value)
    {
        if (Scalars.TryGetKind(serialized, out var kind)
            && kind == expectedKind
            && _converter.TryConvert(serialized, out T c))
        {
            value = c;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="ScalarType"/>.
    /// </summary>
    /// <returns>
    /// A string that represents the current <see cref="ScalarType"/>.
    /// </returns>
    public override string ToString() => Format(this).ToString();

    /// <summary>
    /// Creates a <see cref="ScalarTypeDefinitionNode"/> from the current <see cref="ScalarType"/>.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="ScalarTypeDefinitionNode"/>.
    /// </returns>
    public ScalarTypeDefinitionNode ToSyntaxNode() => Format(this);

    ISyntaxNode ISyntaxNodeProvider.ToSyntaxNode() => ToSyntaxNode();
}
