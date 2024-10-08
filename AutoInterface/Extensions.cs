﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace AutoInterface;

internal static class Extensions {
    /// <summary>
    /// Appends <see cref="Indent.Level"/> of copies of <see cref="Indent.CHAR"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="indent"></param>
    public static StringBuilder AppendIndent(this StringBuilder builder, AutoInterfaceGenerator.Indent indent) => builder.Append(AutoInterfaceGenerator.Indent.CHAR, indent.Level);


    /// <summary>
    /// Finds the first node of type T by traversing the parent nodes.
    /// </summary>
    /// <typeparam name="T">the type of </typeparam>
    /// <param name="syntaxNode"></param>
    /// <returns>The first node of type T, otherwise null.</returns>
    public static T? GetParent<T>(this SyntaxNode syntaxNode) where T : SyntaxNode {
        SyntaxNode? currentNode = syntaxNode.Parent;
        while (currentNode != null) {
            if (currentNode is T t)
                return t;

            currentNode = currentNode.Parent;
        }

        return null;
    }

    /// <summary>
    /// Finds the first attribute that matches the given name.
    /// </summary>
    /// <param name="member"></param>
    /// <param name="attributeName"></param>
    /// <param name="attributeNameAttribute"></param>
    /// <returns></returns>
    public static AttributeSyntax? GetAttribute(this MemberDeclarationSyntax member, string attributeName) {
        foreach (AttributeListSyntax attributeList in member.AttributeLists)
            foreach (AttributeSyntax attribute in attributeList.Attributes) {
                string identifier = attribute.Name switch {
                    SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
                    QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
                    _ => string.Empty
                };
                if (identifier == attributeName)
                    return attribute;

                const string ATTRIBUTE = "Attribute";
                if (identifier.Length == attributeName.Length + ATTRIBUTE.Length) {
                    ReadOnlySpan<char> identifierSpan = identifier.AsSpan();
                    if (identifierSpan[..^ATTRIBUTE.Length].SequenceEqual(attributeName.AsSpan()) && identifierSpan[attributeName.Length..].SequenceEqual(ATTRIBUTE.AsSpan()))
                        return attribute;
                }
            }

        return null;
    }

    /// <summary>
    /// Basically linq Contains method on a SyntaxTokenList
    /// </summary>
    /// <param name="modifiers"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static bool Contains(this SyntaxTokenList modifiers, string token) {
        foreach (SyntaxToken modifier in modifiers)
            if (modifier.ValueText == token)
                return true;

        return false;
    }


    /// <summary>
    /// <para>Finds the argument with the given name and returns it's value.</para>
    /// <para>If not found, it returns null.</para>
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static TypedConstant? GetArgument(this ImmutableArray<KeyValuePair<string, TypedConstant>> arguments, string name) {
        for (int i = 0; i < arguments.Length; i++)
            if (arguments[i].Key == name)
                return arguments[i].Value;

        return null;
    }

    /// <summary>
    /// <para>Finds the argument with the given name and returns it's value.</para>
    /// <para>If not found or value is not castable, it returns default.</para>
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T? GetArgument<T>(this ImmutableArray<KeyValuePair<string, TypedConstant>> arguments, string name)
        => GetArgument(arguments, name) switch {
            TypedConstant { Value: T value } => value,
            _ => default
        };

    /// <summary>
    /// <para>Finds the argument with the given name and returns it's value as array.</para>
    /// <para>If not found or any value is not castable, it returns an empty array.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arguments"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T[] GetArgumentArray<T>(this ImmutableArray<KeyValuePair<string, TypedConstant>> arguments, string name) {
        if (arguments.GetArgument(name) is not TypedConstant { Kind: TypedConstantKind.Array } typeArray)
            return [];

        T[] result = new T[typeArray.Values.Length];
        for (int i = 0; i < result.Length; i++) {
            if (typeArray.Values[i].Value is not T value)
                return [];
            result[i] = value;
        }

        return result;
    }


    /// <summary>
    /// <para>
    /// Appends the name of the given symbol prefixed with the names of its containing namespaces with trailing dot:
    /// "namespace1.namespace2.namespace3...namespaceN."
    /// </para>
    /// <para>If the given namespace is string.Empty, nothing is appended.</para>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="namespaceSymbol"></param>
    public static StringBuilder AppendNamespace(this StringBuilder builder, INamespaceSymbol namespaceSymbol) {
        if (namespaceSymbol.Name == string.Empty)
            return builder;

        AppendNamespace(builder, namespaceSymbol.ContainingNamespace)
            .AppendInterpolation($"{namespaceSymbol.Name}.");

        return builder;
    }
    /// <summary>
    /// Creates a type to map method <see cref="StringBuilderInterpolationHandler.AppendFormatted(StringBuilderInterpolationHandler.INamespace)"/> to <see cref="AppendNamespace"/>.
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static StringBuilderInterpolationHandler.INamespace AsNamespace(this INamespaceSymbol namespaceSymbol) => Unsafe.As<StringBuilderInterpolationHandler.INamespace>(namespaceSymbol);

    /// <summary>
    /// <para>
    /// Appends the name of the given type prefixed with the names of its containing types with trailing dot:<br />
    /// "type1.type2.typ3...typeN."
    /// </para>
    /// <para>if the given type is null, nothing is appended.</para>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="containingType"></param>
    public static StringBuilder AppendContainingTypes(this StringBuilder builder, INamedTypeSymbol? containingType) {
        if (containingType == null)
            return builder;

        builder.AppendContainingTypes(containingType.ContainingType)
            .AppendInterpolation($"{containingType.Name}.");

        return builder;
    }
    /// <summary>
    /// Creates a type to map method <see cref="StringBuilderInterpolationHandler.AppendFormatted(StringBuilderInterpolationHandler.IContainingTypes)"/> to <see cref="AppendContainingTypes"/>.
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static StringBuilderInterpolationHandler.IContainingTypes AsContainingTypes(this INamedTypeSymbol? containingType) => Unsafe.As<StringBuilderInterpolationHandler.IContainingTypes>(containingType);

    /// <summary>
    /// <para>
    /// Appends the typeparameters of the given type sourrounded by curly braces:<br />
    /// "{T1, T2, T3, ..., TN}"
    /// </para>
    /// <para>if the given symbol has no typeParameters, nothing is appended.</para>
    /// </summary>
    /// <param name="builder"></param>
    public static StringBuilder AppendParameterList(this StringBuilder builder, INamedTypeSymbol typeSymbol) {
        if (typeSymbol.TypeParameters.Length == 0)
            return builder;

        builder.Append('{');

        builder.Append(typeSymbol.TypeParameters[0].Name);
        for (int i = 1; i < typeSymbol.TypeParameters.Length; i++)
            builder.AppendInterpolation($"{typeSymbol.TypeParameters[i].Name}, ");

        builder.Append('}');

        return builder;
    }
    /// <summary>
    /// Creates a type to map method <see cref="StringBuilderInterpolationHandler.AppendFormatted(StringBuilderInterpolationHandler.IParameterList)"/> to <see cref="AppendParameterList"/>.
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static StringBuilderInterpolationHandler.IParameterList AsParameterList(this INamedTypeSymbol typeSymbol) => Unsafe.As<StringBuilderInterpolationHandler.IParameterList>(typeSymbol);

    /// <summary>
    /// <para>Appends a access modifier when the corresponding attribute is present:</para>
    /// <para>
    /// [AutoInterfaceVisibilityPublic] => "public"<br />
    /// [AutoInterfaceVisibilityInternal] => "internal"<br />
    /// [AutoInterfaceVisibilityProtected] => "protected"<br />
    /// [AutoInterfaceVisibilityProtectedInternal] => "protected internal"<br />
    /// [AutoInterfaceVisibilityPrivateProtected] => "private protected"
    /// </para>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="member"></param>
    public static StringBuilder AppendAccessModifier(this StringBuilder builder, MemberDeclarationSyntax member) {
        if (member.GetAttribute("AutoInterfaceVisibilityPublic") is not null)
            builder.Append("public ");
        if (member.GetAttribute("AutoInterfaceVisibilityInternal") is not null)
            builder.Append("internal ");
        if (member.GetAttribute("AutoInterfaceVisibilityProtected") is not null)
            builder.Append("protected ");
        if (member.GetAttribute("AutoInterfaceVisibilityProtectedInternal") is not null)
            builder.Append("protected internal ");
        if (member.GetAttribute("AutoInterfaceVisibilityPrivateProtected") is not null)
            builder.Append("private protected ");

        return builder;
    }
    /// <summary>
    /// Creates a type to map method <see cref="StringBuilderInterpolationHandler.AppendFormatted(StringBuilderInterpolationHandler.IAccessModifier)"/> to <see cref="AppendAccessModifier"/>.
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static StringBuilderInterpolationHandler.IAccessModifier AsAccessModifier(this MemberDeclarationSyntax member) => Unsafe.As<StringBuilderInterpolationHandler.IAccessModifier>(member);


    /// <summary>
    /// The same as <see cref="StringBuilder.Append(string)"/>, but only for interpolated strings: $"..."<br />
    /// It constructs the string directly in the builder, so no unnecessary string memory allocations.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public static StringBuilder AppendInterpolation(this StringBuilder builder, [InterpolatedStringHandlerArgument("builder")] StringBuilderInterpolationHandler handler) => builder;

    [InterpolatedStringHandler]
    public readonly ref struct StringBuilderInterpolationHandler {
        private readonly StringBuilder builder;

        public StringBuilderInterpolationHandler(int literalLength, int formattedCount, StringBuilder builder) => this.builder = builder;

        public void AppendLiteral(string str) => builder.Append(str);

        public void AppendFormatted<T>(T item) => builder.Append(item);


        public void AppendFormatted(AutoInterfaceGenerator.Indent indent) => builder.AppendIndent(indent);

        public interface INamespace;
        public void AppendFormatted(INamespace @namespace) => builder.AppendNamespace(Unsafe.As<INamespaceSymbol>(@namespace));

        public interface IContainingTypes;
        public void AppendFormatted(IContainingTypes containingType) => builder.AppendContainingTypes(Unsafe.As<INamedTypeSymbol?>(containingType));

        public interface IParameterList;
        public void AppendFormatted(IParameterList parameterList) => builder.AppendParameterList(Unsafe.As<INamedTypeSymbol>(parameterList));

        public interface IAccessModifier;
        public void AppendFormatted(IAccessModifier accessModifier) => builder.AppendAccessModifier(Unsafe.As<MemberDeclarationSyntax>(accessModifier));

    }
}
