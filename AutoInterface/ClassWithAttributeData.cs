using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoInterface;

/// <summary>
/// Container for 2 Nodes: The target class and target attribute
/// </summary>
/// <param name="type"></param>
/// <param name="attributeData"></param>
public readonly struct ClassWithAttributeData(TypeDeclarationSyntax type, AttributeData attributeData) : IEquatable<ClassWithAttributeData> {
    public TypeDeclarationSyntax Type { get; } = type;
    public AttributeData AttributeData { get; } = attributeData;


    public static bool operator ==(ClassWithAttributeData left, ClassWithAttributeData right) => left.Equals(right);

    public static bool operator !=(ClassWithAttributeData left, ClassWithAttributeData right) => !(left == right);

    public override bool Equals(object? obj) {
        if (obj is not ClassWithAttributeData classWithAttributeData)
            return false;

        return Equals(classWithAttributeData);
    }

    public bool Equals(ClassWithAttributeData other) => Type == other.Type;

    public override int GetHashCode() => Type.GetHashCode();
}
