using System.Buffers;
using System.Reflection;
using System.Text;
using HotChocolate.Utilities;

namespace HotChocolate;

/// <summary>
/// Contains helpers and extensions to reformat the name of a type system member to conform with
/// GraphQL naming standards.
/// </summary>
internal static class NameFormattingHelpers
{
    private const string Get = "Get";
    private const string Async = "Async";
    private const char GenericTypeDelimiter = '`';

    public static string GetGraphQLName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var name = GetFromType(type);

        return NameUtils.MakeValidGraphQLName(name)!;
    }

    public static string GetGraphQLName(this PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var name = property.IsDefined(
            typeof(GraphQLNameAttribute), false)
            ? property.GetCustomAttribute<GraphQLNameAttribute>()!.Name
            : FormatFieldName(property.Name);

        return NameUtils.MakeValidGraphQLName(name)!;
    }

    public static string GetGraphQLName(this MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        var name = method.IsDefined(
            typeof(GraphQLNameAttribute), false)
            ? method.GetCustomAttribute<GraphQLNameAttribute>()!.Name
            : FormatMethodName(method);

        return NameUtils.MakeValidGraphQLName(name)!;
    }

    public static string GetGraphQLName(this ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        var name = parameter.IsDefined(
            typeof(GraphQLNameAttribute), false)
            ? parameter.GetCustomAttribute<GraphQLNameAttribute>()!.Name
            : FormatFieldName(parameter.Name!);

        return NameUtils.MakeValidGraphQLName(name)!;
    }

    public static string GetGraphQLName(this MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (member is MethodInfo m)
        {
            return GetGraphQLName(m);
        }

        if (member is PropertyInfo p)
        {
            return GetGraphQLName(p);
        }

        throw new NotSupportedException(
            "Only properties and methods are accepted as members.");
    }

    private static string FormatMethodName(MethodInfo method)
    {
        var name = method.Name;

        if (name.StartsWith(Get, StringComparison.Ordinal)
            && name.Length > Get.Length)
        {
            name = name[Get.Length..];
        }

        if (IsAsyncMethod(method.ReturnType)
            && name.Length > Async.Length
            && name.EndsWith(Async, StringComparison.Ordinal))
        {
            name = name[..^Async.Length];
        }

        return FormatFieldName(name);
    }

    private static bool IsAsyncMethod(Type returnType)
    {
        if (typeof(Task).IsAssignableFrom(returnType)
            || typeof(ValueTask).IsAssignableFrom(returnType))
        {
            return true;
        }

        if (returnType.IsGenericType)
        {
            var typeDefinition = returnType.GetGenericTypeDefinition();
            return typeof(ValueTask<>) == typeDefinition
                || typeof(IAsyncEnumerable<>) == typeDefinition;
        }

        return false;
    }

    public static string? GetGraphQLDescription(
        this ICustomAttributeProvider attributeProvider)
    {
        if (attributeProvider.IsDefined(
            typeof(GraphQLDescriptionAttribute),
            false))
        {
            var attribute =
                (GraphQLDescriptionAttribute)
                    attributeProvider.GetCustomAttributes(
                        typeof(GraphQLDescriptionAttribute),
                        false)[0];
            return attribute.Description;
        }

        return null;
    }

    public static bool IsDeprecated(
        this ICustomAttributeProvider attributeProvider,
        out string? reason)
    {
        var deprecatedAttribute =
            GetAttributeIfDefined<GraphQLDeprecatedAttribute>(attributeProvider);

        if (deprecatedAttribute is not null)
        {
            reason = deprecatedAttribute.DeprecationReason;
            return true;
        }

        var obsoleteAttribute =
            GetAttributeIfDefined<ObsoleteAttribute>(attributeProvider);

        if (obsoleteAttribute is not null)
        {
            reason = obsoleteAttribute.Message;
            return true;
        }

        reason = null;
        return false;
    }

    private static string GetFromType(Type type)
    {
        var typeName = type.IsDefined(typeof(GraphQLNameAttribute), false)
            ? type.GetCustomAttribute<GraphQLNameAttribute>()!.Name
            : null;

        if (type.IsGenericType)
        {
            if (typeName == null)
            {
                typeName = type.GetGenericTypeDefinition().Name;

                var nameSpan = typeName.AsSpan();
                var index = nameSpan.LastIndexOf(GenericTypeDelimiter);

                if (index >= 0)
                {
                    nameSpan = nameSpan[..index];
                }

                typeName = nameSpan.ToString();
            }

            var arguments = type.GetGenericArguments();
            var stringBuilder = new StringBuilder(typeName).Append("Of");

            for (var i = 0; i < arguments.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append("And");
                }

                stringBuilder.Append(GetFromType(arguments[i]));
            }

            return stringBuilder.ToString();
        }

        return typeName ?? type.Name;
    }

    public static unsafe string FormatFieldName(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        // quick exit
        if (char.IsLower(fieldName[0]))
        {
            return fieldName;
        }

        var size = fieldName.Length;
        char[]? rented = null;
        var buffer = size <= 128
            ? stackalloc char[size]
            : rented = ArrayPool<char>.Shared.Rent(size);

        try
        {
            var p = 0;
            for (; p < fieldName.Length && char.IsLetter(fieldName[p]) && char.IsUpper(fieldName[p]); p++)
            {
                buffer[p] = char.ToLowerInvariant(fieldName[p]);
            }

            // in case more than one character is upper case, we uppercase
            // the current character. We only uppercase the character
            // back if the last character is a letter
            //
            // before    after      result
            // FOOBar    FOOBar   = fooBar
            //    ^        ^
            // FOO1Ar    FOO1Ar   = foo1Ar
            //   ^         ^
            // FOO_Ar    FOO_Ar   = foo_Ar
            //   ^         ^
            if (p < fieldName.Length && p > 1 && char.IsLetter(fieldName[p]))
            {
                buffer[p - 1] = char.ToUpperInvariant(fieldName[p - 1]);
            }

            for (; p < fieldName.Length; p++)
            {
                buffer[p] = fieldName[p];
            }

            fixed (char* charPtr = buffer)
            {
                return new string(charPtr, 0, buffer.Length);
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    private static TAttribute? GetAttributeIfDefined<TAttribute>(
        ICustomAttributeProvider attributeProvider)
        where TAttribute : Attribute
    {
        var attributeType = typeof(TAttribute);

        if (attributeProvider.IsDefined(attributeType, false))
        {
            return (TAttribute)attributeProvider
                .GetCustomAttributes(attributeType, false)[0];
        }

        return null;
    }
}
