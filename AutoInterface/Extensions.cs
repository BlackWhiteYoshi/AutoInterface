﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoInterface;

internal static class Extensions {
    /// <summary>
    /// Finds the first node of type T by traversing the parent nodes.
    /// </summary>
    /// <typeparam name="T">the type of </typeparam>
    /// <param name="syntaxNode"></param>
    /// <returns>The first node of type T, otherwise null.</returns>
    internal static T? GetParent<T>(this SyntaxNode syntaxNode) where T : SyntaxNode {
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
    internal static AttributeSyntax? GetAttribute(this MemberDeclarationSyntax member, string attributeName) {
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
    internal static bool Contains(this SyntaxTokenList modifiers, string token) {
        foreach (SyntaxToken modifier in modifiers)
            if (modifier.ValueText == token)
                return true;

        return false;
    }


    /// <summary>
    /// <para>Finds the argument with the given name and returns it's expression.</para>
    /// <para>If not found, it returns null.</para>
    /// </summary>
    /// <param name="argumentList"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    internal static ExpressionSyntax? GetExpression(this AttributeArgumentListSyntax argumentList, string name) {
        foreach (AttributeArgumentSyntax argument in argumentList.Arguments) {
            if (argument.NameEquals?.Name.Identifier.ValueText == name)
                return argument.Expression;
        }

        return null;
    }

    /// <summary>
    /// <para>Finds the argument with the given name and returns it's expression as LiteralExpressionSyntax.</para>
    /// <para>If not found or expression is not a LiteralExpressionSyntax, it returns null.</para>
    /// </summary>
    /// <param name="argumentList"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    internal static LiteralExpressionSyntax? GetLiteral(this AttributeArgumentListSyntax argumentList, string name) => argumentList.GetExpression(name) as LiteralExpressionSyntax;
}
