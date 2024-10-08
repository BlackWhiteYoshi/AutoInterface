﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Immutable;
using System.Text;

namespace AutoInterface;

[Generator(LanguageNames.CSharp)]
public sealed partial class AutoInterfaceGenerator : IIncrementalGenerator {
    /// <summary>
    /// An int wrapper to handle indentation.
    /// </summary>
    public struct Indent {
        public const int AMOUNT = 4;
        public const char CHAR = ' ';


        public int Level { get; private set; }

        public void IncreaseLevel() => Level += AMOUNT;

        public void DecreaseLevel() => Level -= AMOUNT;
    }


    private readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool(initialCapacity: 8192, maximumRetainedCapacity: 1024 * 1024);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // register attribute marker
        context.RegisterPostInitializationOutput(static (IncrementalGeneratorPostInitializationContext context) => {
            context.AddSource("AutoInterfaceAttribute.g.cs", Attributes.AutoInterfaceAttribute);
            context.AddSource("IgnoreAutoInterfaceAttribute.g.cs", Attributes.IgnoreAutoInterfaceAttribute);

            context.AddSource("AutoInterfaceVisibilityPublicAttribute.g.cs", Attributes.AutoInterfaceVisibilityPublicAttribute);
            context.AddSource("AutoInterfaceVisibilityInternalAttribute.g.cs", Attributes.AutoInterfaceVisibilityInternalAttribute);
            context.AddSource("AutoInterfaceVisibilityProtectedAttribute.g.cs", Attributes.AutoInterfaceVisibilityProtectedAttribute);
            context.AddSource("AutoInterfaceVisibilityProtectedInternalAttribute.g.cs", Attributes.AutoInterfaceVisibilityProtectedInternalAttribute);
            context.AddSource("AutoInterfaceVisibilityPrivateProtectedAttribute.g.cs", Attributes.AutoInterfaceVisibilityPrivateProtectedAttribute);
        });

        // all classes/structs with AutoInterfaceAttribute
        IncrementalValuesProvider<ClassWithAttributeData> interfaceTypeProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "AutoInterfaceAttributes.AutoInterfaceAttribute",
            (SyntaxNode syntaxNode, CancellationToken _) => syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax,
            (GeneratorAttributeSyntaxContext syntaxContext, CancellationToken _) => ((TypeDeclarationSyntax)syntaxContext.TargetNode, (INamedTypeSymbol)syntaxContext.TargetSymbol, syntaxContext.Attributes))
            .SelectMany(((TypeDeclarationSyntax type, INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributes) pair, CancellationToken _) => {
                ImmutableArray<ClassWithAttributeData>.Builder attributeWithClassList = ImmutableArray.CreateBuilder<ClassWithAttributeData>(pair.attributes.Length);
                foreach (AttributeData attributeData in pair.attributes)
                    attributeWithClassList.Add(new ClassWithAttributeData(pair.type, pair.typeSymbol, attributeData));
                return attributeWithClassList;
            });

        context.RegisterSourceOutput(interfaceTypeProvider, Execute);
    }

    private void Execute(SourceProductionContext context, ClassWithAttributeData provider) {
        Indent indent = new();

        TypeDeclarationSyntax targetType = provider.Type;
        INamedTypeSymbol targetSymbol = provider.TypeSymbol;
        AttributeData attributeData = provider.AttributeData;

        (string? name, string modifier, string? namspace, INamedTypeSymbol[] inheritance, string[] nested, bool staticMembers) attribute = (null, "public partial", null, [], [], false);
        if (attributeData.NamedArguments.Length > 0) {
            if (attributeData.NamedArguments.GetArgument<string>("Name") is string name)
                attribute.name = name;
            if (attributeData.NamedArguments.GetArgument<string>("Modifier") is string modifier)
                attribute.modifier = modifier;
            if (attributeData.NamedArguments.GetArgument<string>("Namespace") is string namspace)
                attribute.namspace = namspace;
            attribute.inheritance = attributeData.NamedArguments.GetArgumentArray<INamedTypeSymbol>("Inheritance");
            attribute.nested = attributeData.NamedArguments.GetArgumentArray<string>("Nested");
            attribute.staticMembers = attributeData.NamedArguments.GetArgument<bool>("StaticMembers");
        }

        // tracking record parameter overwrites
        SeparatedSyntaxList<ParameterSyntax> recordParameterList = targetType switch {
            RecordDeclarationSyntax { ParameterList: ParameterListSyntax } record => record.ParameterList.Parameters,
            _ => default
        };
        Span<bool> recordParameterOverwrittenFlags = stackalloc bool[recordParameterList.Count];
        bool recordDeconstructOverwrittenFlag = false;


        StringBuilder builder = stringBuilderPool.Get();

        builder.Append("""
            // <auto-generated/>
            #pragma warning disable
            #nullable enable annotations



            """);

        // usingStatements
        {
            BaseNamespaceDeclarationSyntax? namspace = targetType.GetParent<BaseNamespaceDeclarationSyntax>();
            while (namspace != null) {
                string usings = namspace.Usings.ToString();
                if (usings != string.Empty)
                    builder.AppendInterpolation($"{usings}\n");
                namspace = namspace.GetParent<BaseNamespaceDeclarationSyntax>();
            }

            CompilationUnitSyntax? compilationUnit = targetType.GetParent<CompilationUnitSyntax>();
            if (compilationUnit != null)
                builder.AppendInterpolation($"{compilationUnit.Usings}\n");

            builder.Append('\n');
        }

        // namespace
        switch (attribute.namspace) {
            case "":
                break; // global namespace -> append nothing
            case null:
                INamespaceSymbol namespaceSymbol = targetSymbol.ContainingNamespace;
                if (namespaceSymbol.Name == string.Empty)
                    break; // global namespace -> append nothing

                builder.AppendInterpolation($"namespace {namespaceSymbol.ContainingNamespace.AsNamespace()}{namespaceSymbol.Name};\n\n");
                break;
            default:
                builder.AppendInterpolation($"namespace {attribute.namspace};\n\n");
                break;
        }

        // nesting
        foreach (string containingType in attribute.nested) {
            builder.AppendInterpolation($"{indent}{containingType} {{\n");
            indent.IncreaseLevel();
        }

        // summary
        foreach (SyntaxTrivia trivia in targetType.AttributeLists[0].GetLeadingTrivia())
            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                builder.AppendInterpolation($"{indent}///{documentationCommentTrivia}");
                break;
            }

        // class/struct declaration
        {
            if (attribute.name is null)
                builder.AppendInterpolation($"{indent}{attribute.modifier} interface I{targetSymbol.Name}");
            else
                builder.AppendInterpolation($"{indent}{attribute.modifier} interface {attribute.name}");

            if (targetType.TypeParameterList?.Parameters.Count > 0) {
                builder.Append('<');

                foreach (TypeParameterSyntax parameter in targetType.TypeParameterList.Parameters)
                    builder.AppendInterpolation($"{parameter.Identifier.ValueText}, ");
                builder.Length -= 2;

                builder.Append('>');
            }

            if (attribute.inheritance.Length > 0) {
                builder.AppendInterpolation($" : {attribute.inheritance[0]}");
                for (int i = 1; i < attribute.inheritance.Length; i++)
                    builder.AppendInterpolation($", {attribute.inheritance[i]}");
            }

            builder.Append(" {\n");
            indent.IncreaseLevel();
        }

        foreach (MemberDeclarationSyntax member in targetType.Members) {
            if (member.GetAttribute("IgnoreAutoInterface") != null)
                continue;

            switch (member) {
                case FieldDeclarationSyntax fieldDeclarationSyntax: {
                    // check if identifier overwrites record parameter
                    for (int i = 0; i < recordParameterList.Count; i++)
                        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
                            if (recordParameterList[i].Identifier.ValueText == variable.Identifier.ValueText) {
                                recordParameterOverwrittenFlags[i] = true;
                                break;
                            }

                    break;
                }

                case MethodDeclarationSyntax methodDeclarationSyntax: {
                    if (methodDeclarationSyntax.Modifiers.Contains("public")) {
                        // inherited member check
                        foreach (INamedTypeSymbol typeSymbol in attribute.inheritance) {
                            ImmutableArray<INamedTypeSymbol>.Enumerator allInterfaces = typeSymbol.AllInterfaces.GetEnumerator();
                            INamedTypeSymbol? inheritedSymbol = typeSymbol;
                            do {
                                foreach (ISymbol symbol in inheritedSymbol.GetMembers(methodDeclarationSyntax.Identifier.ValueText)) {
                                    if (symbol is not IMethodSymbol methodSymbol)
                                        continue;

                                    foreach (ISymbol target in targetSymbol.GetMembers(methodDeclarationSyntax.Identifier.ValueText))
                                        if (target is IMethodSymbol targetMethod)
                                            if (SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, targetMethod.ReturnType))
                                                if (methodSymbol.TypeParameters.Length == targetMethod.TypeParameters.Length)
                                                    if (methodSymbol.Parameters.SequenceEqual(targetMethod.Parameters, (IParameterSymbol a, IParameterSymbol b) => SymbolEqualityComparer.Default.Equals(a.Type, b.Type)))
                                                        if (methodSymbol.IsStatic == targetMethod.IsStatic)
                                                            goto _switchBreak;
                                }

                                if (!allInterfaces.MoveNext())
                                    break;
                                inheritedSymbol = allInterfaces.Current;
                            } while (true);
                        }
                    }
                    else {
                        // explicit interface specifier
                        if (methodDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is not IdentifierNameSyntax identifierSyntax)
                            break;

                        if (attribute.name is null) {
                            if (identifierSyntax.Identifier.ValueText.Length != targetSymbol.Name.Length + 1)
                                break;
                            if (identifierSyntax.Identifier.ValueText[0] != 'I')
                                break;
                            if (!identifierSyntax.Identifier.ValueText.AsSpan(1).SequenceEqual(targetSymbol.Name.AsSpan()))
                                break;
                        }
                        else {
                            if (identifierSyntax.Identifier.ValueText != attribute.name)
                                break;
                        }
                    }

                    // check for Deconstruct() overwrite
                    if (recordParameterList.Count > 0)
                        if (methodDeclarationSyntax.Identifier.ValueText == "Deconstruct")
                            if (!methodDeclarationSyntax.Modifiers.Contains("static"))
                                if (methodDeclarationSyntax.ParameterList.Parameters.Count == recordParameterList.Count) {
                                    for (int i = 0; i < recordParameterList.Count; i++)
                                        if (methodDeclarationSyntax.ParameterList.Parameters[i].Modifiers is not [SyntaxToken { ValueText: "out" }])
                                            if (methodDeclarationSyntax.ParameterList.Parameters[i].Type != recordParameterList[i].Type)
                                                goto DeconstructChecked;
                                    recordDeconstructOverwrittenFlag = true;
                                }
                    DeconstructChecked:

                    string modifiers;
                    if (!methodDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    SyntaxTriviaList triviaList = methodDeclarationSyntax.AttributeLists.Count switch {
                        > 0 => methodDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                        _ => methodDeclarationSyntax.Modifiers.Count switch {
                            > 0 => methodDeclarationSyntax.Modifiers[0].LeadingTrivia,
                            _ => methodDeclarationSyntax.ReturnType.GetLeadingTrivia()
                        }
                    };
                    foreach (SyntaxTrivia trivia in triviaList)
                        if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                            builder.AppendInterpolation($"{indent}///{documentationCommentTrivia}");
                            break;
                        }

                    // attributes
                    if (methodDeclarationSyntax.AttributeLists.Count > 0)
                        builder.AppendInterpolation($"{indent}{methodDeclarationSyntax.AttributeLists}\n");

                    // actual declaration
                    builder.AppendInterpolation($"{indent}{member.AsAccessModifier()}{modifiers}{methodDeclarationSyntax.ReturnType} {methodDeclarationSyntax.Identifier.ValueText}{methodDeclarationSyntax.TypeParameterList}{methodDeclarationSyntax.ParameterList}");
                    if (methodDeclarationSyntax.ConstraintClauses.ToString() is string { Length: > 0 } constraintClauses)
                        builder.AppendInterpolation($" {constraintClauses}");
                    builder.Append(";\n\n");

                    _switchBreak:
                    break;
                }

                case PropertyDeclarationSyntax propertyDeclarationSyntax: {
                    // check if identifier overwrites record parameter
                    for (int i = 0; i < recordParameterList.Count; i++)
                        if (recordParameterList[i].Identifier.ValueText == propertyDeclarationSyntax.Identifier.ValueText) {
                            recordParameterOverwrittenFlags[i] = true;
                            break;
                        }

                    // public or explicit interface specifier
                    if (propertyDeclarationSyntax.Modifiers.Contains("public")) {
                        // inherited member check
                        foreach (INamedTypeSymbol typeSymbol in attribute.inheritance) {
                            ImmutableArray<INamedTypeSymbol>.Enumerator allInterfaces = typeSymbol.AllInterfaces.GetEnumerator();
                            INamedTypeSymbol? inheritedSymbol = typeSymbol;
                            do {
                                foreach (ISymbol symbol in inheritedSymbol.GetMembers(propertyDeclarationSyntax.Identifier.ValueText)) {
                                    if (symbol is not IPropertySymbol propertySymbol)
                                        continue;

                                    foreach (ISymbol target in targetSymbol.GetMembers(propertyDeclarationSyntax.Identifier.ValueText))
                                        if (target is IPropertySymbol targetProperty)
                                            if (SymbolEqualityComparer.Default.Equals(propertySymbol.Type, targetProperty.Type))
                                                if (propertySymbol.GetMethod is null == targetProperty.GetMethod is null && propertySymbol.SetMethod is null == targetProperty.SetMethod is null)
                                                    if (propertySymbol.IsStatic == targetProperty.IsStatic)
                                                        goto _switchBreak;
                                }

                                if (!allInterfaces.MoveNext())
                                    break;
                                inheritedSymbol = allInterfaces.Current;
                            } while (true);
                        }
                    }
                    else {
                        if (propertyDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is not IdentifierNameSyntax identifierSyntax)
                            break;

                        if (attribute.name is null) {
                            if (identifierSyntax.Identifier.ValueText.Length != targetSymbol.Name.Length + 1)
                                break;
                            if (identifierSyntax.Identifier.ValueText[0] != 'I')
                                break;
                            if (!identifierSyntax.Identifier.ValueText.AsSpan(1).SequenceEqual(targetSymbol.Name.AsSpan()))
                                break;
                        }
                        else {
                            if (identifierSyntax.Identifier.ValueText != attribute.name)
                                break;
                        }
                    }

                    string modifiers;
                    if (!propertyDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    SyntaxTriviaList triviaList = propertyDeclarationSyntax.AttributeLists.Count switch {
                        > 0 => propertyDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                        _ => propertyDeclarationSyntax.Modifiers.Count switch {
                            > 0 => propertyDeclarationSyntax.Modifiers[0].LeadingTrivia,
                            _ => propertyDeclarationSyntax.Type.GetLeadingTrivia()
                        }
                    };
                    foreach (SyntaxTrivia trivia in triviaList)
                        if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                            builder.AppendInterpolation($"{indent}///{documentationCommentTrivia}");
                            break;
                        }

                    // attributes
                    if (propertyDeclarationSyntax.AttributeLists.Count > 0)
                        builder.AppendInterpolation($"{indent}{propertyDeclarationSyntax.AttributeLists}\n");

                    // actual declaration
                    builder.AppendInterpolation($"{indent}{member.AsAccessModifier()}{modifiers}{propertyDeclarationSyntax.Type} {propertyDeclarationSyntax.Identifier.ValueText} {{ ");
                    if (propertyDeclarationSyntax.AccessorList is not null) {
                        foreach (AccessorDeclarationSyntax accessor in propertyDeclarationSyntax.AccessorList.Accessors)
                            if (accessor.Modifiers.Count == 0) // check if public
                                builder.AppendInterpolation($"{accessor.Keyword.ValueText}; ");
                    }
                    else
                        builder.Append("get; ");  // body-expression syntax: "=> "
                    builder.Append("}\n\n");

                    _switchBreak:
                    break;
                }

                case IndexerDeclarationSyntax indexerDeclarationSyntax: {
                    // public or explicit interface specifier
                    if (indexerDeclarationSyntax.Modifiers.Contains("public")) {
                        // inherited member check
                        foreach (INamedTypeSymbol typeSymbol in attribute.inheritance) {
                            ImmutableArray<INamedTypeSymbol>.Enumerator allInterfaces = typeSymbol.AllInterfaces.GetEnumerator();
                            INamedTypeSymbol? inheritedSymbol = typeSymbol;
                            do {
                                foreach (ISymbol symbol in inheritedSymbol.GetMembers()) {
                                    if (symbol is not IPropertySymbol { IsIndexer: true } propertySymbol)
                                        continue;

                                    foreach (ISymbol target in targetSymbol.GetMembers())
                                        if (target is IPropertySymbol { IsIndexer: true } targetIndexer)
                                            if (SymbolEqualityComparer.Default.Equals(propertySymbol.Type, targetIndexer.Type))
                                                if (propertySymbol.Parameters.SequenceEqual(targetIndexer.Parameters, (IParameterSymbol a, IParameterSymbol b) => SymbolEqualityComparer.Default.Equals(a.Type, b.Type)))
                                                    if (propertySymbol.GetMethod is null == targetIndexer.GetMethod is null && propertySymbol.SetMethod is null == targetIndexer.SetMethod is null)
                                                        goto _switchBreak;
                                }

                                if (!allInterfaces.MoveNext())
                                    break;
                                inheritedSymbol = allInterfaces.Current;
                            } while (true);
                        }
                    }
                    else {
                        if (indexerDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is not IdentifierNameSyntax identifierSyntax)
                            break;

                        if (attribute.name is null) {
                            if (identifierSyntax.Identifier.ValueText.Length != targetSymbol.Name.Length + 1)
                                break;
                            if (identifierSyntax.Identifier.ValueText[0] != 'I')
                                break;
                            if (!identifierSyntax.Identifier.ValueText.AsSpan(1).SequenceEqual(targetSymbol.Name.AsSpan()))
                                break;
                        }
                        else {
                            if (identifierSyntax.Identifier.ValueText != attribute.name)
                                break;
                        }
                    }


                    // summary
                    SyntaxTriviaList triviaList = indexerDeclarationSyntax.AttributeLists.Count switch {
                        > 0 => indexerDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                        _ => indexerDeclarationSyntax.Modifiers.Count switch {
                            > 0 => indexerDeclarationSyntax.Modifiers[0].LeadingTrivia,
                            _ => indexerDeclarationSyntax.Type.GetLeadingTrivia()
                        }
                    };
                    foreach (SyntaxTrivia trivia in triviaList)
                        if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                            builder.AppendInterpolation($"{indent}///{documentationCommentTrivia}");
                            break;
                        }

                    // attributes
                    if (indexerDeclarationSyntax.AttributeLists.Count > 0)
                        builder.AppendInterpolation($"{indent}{indexerDeclarationSyntax.AttributeLists}\n");

                    // actual declaration
                    builder.AppendInterpolation($"{indent}{member.AsAccessModifier()}{indexerDeclarationSyntax.Type} this{indexerDeclarationSyntax.ParameterList} {{ ");
                    if (indexerDeclarationSyntax.AccessorList != null) {
                        foreach (AccessorDeclarationSyntax accessor in indexerDeclarationSyntax.AccessorList.Accessors)
                            if (accessor.Modifiers.Count == 0) // check if public
                                builder.AppendInterpolation($"{accessor.Keyword.ValueText}; ");
                    }
                    else
                        builder.Append("get; ");  // body-expression syntax: "=> "
                    builder.Append("}\n\n");

                    _switchBreak:
                    break;
                }

                case EventFieldDeclarationSyntax eventFieldDeclarationSyntax: {
                    if (eventFieldDeclarationSyntax.Modifiers.Contains("public")) {
                        // inherited member check
                        foreach (INamedTypeSymbol typeSymbol in attribute.inheritance) {
                            ImmutableArray<INamedTypeSymbol>.Enumerator allInterfaces = typeSymbol.AllInterfaces.GetEnumerator();
                            INamedTypeSymbol? inheritedSymbol = typeSymbol;
                            do {
                                foreach (VariableDeclaratorSyntax eventFieldVariable in eventFieldDeclarationSyntax.Declaration.Variables)
                                    foreach (ISymbol symbol in inheritedSymbol.GetMembers(eventFieldVariable.Identifier.ValueText)) {
                                        if (symbol is not IEventSymbol eventSymbol)
                                            continue;

                                        foreach (ISymbol target in targetSymbol.GetMembers(eventFieldVariable.Identifier.ValueText))
                                            if (target is IEventSymbol targetEvent)
                                                if (SymbolEqualityComparer.Default.Equals(eventSymbol.Type, targetEvent.Type))
                                                    if (eventSymbol.IsStatic == targetEvent.IsStatic)
                                                        goto _switchBreak;
                                    }

                                if (!allInterfaces.MoveNext())
                                    break;
                                inheritedSymbol = allInterfaces.Current;
                            } while (true);
                        }
                    }
                    else
                        break;

                    string modifiers;
                    if (!eventFieldDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    SyntaxTriviaList triviaList = eventFieldDeclarationSyntax.AttributeLists.Count switch {
                        > 0 => eventFieldDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                        _ => eventFieldDeclarationSyntax.Modifiers.Count switch {
                            > 0 => eventFieldDeclarationSyntax.Modifiers[0].LeadingTrivia,
                            _ => eventFieldDeclarationSyntax.EventKeyword.LeadingTrivia
                        }
                    };
                    foreach (SyntaxTrivia trivia in triviaList)
                        if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                            builder.AppendInterpolation($"{indent}///{documentationCommentTrivia}");
                            break;
                        }

                    // attributes
                    if (eventFieldDeclarationSyntax.AttributeLists.Count > 0)
                        builder.AppendInterpolation($"{indent}{eventFieldDeclarationSyntax.AttributeLists}\n");

                    // actual declaration
                    builder.AppendInterpolation($"{indent}{member.AsAccessModifier()}{modifiers}event {eventFieldDeclarationSyntax.Declaration.Type} {eventFieldDeclarationSyntax.Declaration.Variables};\n\n");

                    _switchBreak:
                    break;
                }

                case EventDeclarationSyntax eventDeclarationSyntax: {
                    // public or explicit interface specifier
                    if (eventDeclarationSyntax.Modifiers.Contains("public")) {
                        // inherited member check
                        foreach (INamedTypeSymbol typeSymbol in attribute.inheritance) {
                            ImmutableArray<INamedTypeSymbol>.Enumerator allInterfaces = typeSymbol.AllInterfaces.GetEnumerator();
                            INamedTypeSymbol? inheritedSymbol = typeSymbol;
                            do {
                                foreach (ISymbol symbol in inheritedSymbol.GetMembers(eventDeclarationSyntax.Identifier.ValueText)) {
                                    if (symbol is not IEventSymbol eventSymbol)
                                        continue;

                                    foreach (ISymbol target in targetSymbol.GetMembers(eventDeclarationSyntax.Identifier.ValueText))
                                        if (target is IEventSymbol targetEvent)
                                            if (SymbolEqualityComparer.Default.Equals(eventSymbol.Type, targetEvent.Type))
                                                if (eventSymbol.IsStatic == targetEvent.IsStatic)
                                                    goto _switchBreak;
                                }

                                if (!allInterfaces.MoveNext())
                                    break;
                                inheritedSymbol = allInterfaces.Current;
                            } while (true);
                        }
                    }
                    else {
                        if (eventDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is not IdentifierNameSyntax identifierSyntax)
                            break;

                        if (attribute.name is null) {
                            if (identifierSyntax.Identifier.ValueText.Length != targetSymbol.Name.Length + 1)
                                break;
                            if (identifierSyntax.Identifier.ValueText[0] != 'I')
                                break;
                            if (!identifierSyntax.Identifier.ValueText.AsSpan(1).SequenceEqual(targetSymbol.Name.AsSpan()))
                                break;
                        }
                        else {
                            if (identifierSyntax.Identifier.ValueText != attribute.name)
                                break;
                        }
                    }

                    string modifiers;
                    if (!eventDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    SyntaxTriviaList triviaList = eventDeclarationSyntax.AttributeLists.Count switch {
                        > 0 => eventDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                        _ => eventDeclarationSyntax.Modifiers.Count switch {
                            > 0 => eventDeclarationSyntax.Modifiers[0].LeadingTrivia,
                            _ => eventDeclarationSyntax.EventKeyword.LeadingTrivia
                        }
                    };
                    foreach (SyntaxTrivia trivia in triviaList)
                        if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                            builder.AppendInterpolation($"{indent}///{documentationCommentTrivia}");
                            break;
                        }

                    // attributes
                    if (eventDeclarationSyntax.AttributeLists.Count > 0)
                        builder.AppendInterpolation($"{indent}{eventDeclarationSyntax.AttributeLists}\n");

                    // actual declaration
                    builder.AppendInterpolation($"{indent}{member.AsAccessModifier()}{modifiers}event {eventDeclarationSyntax.Type} {eventDeclarationSyntax.Identifier};\n\n");

                    _switchBreak:
                    break;
                }
            }
        }

        // adding non-overwritten record member
        if (recordParameterList.Count > 0) {
            // parameter
            string getterSetter = ((RecordDeclarationSyntax)targetType).ClassOrStructKeyword.ValueText switch {
                "struct" => " { get; set; }\n\n",
                _ /* "class" or "" */ => " { get; init; }\n\n"
            };

            for (int i = 0; i < recordParameterList.Count; i++)
                if (!recordParameterOverwrittenFlags[i] && recordParameterList[i] is ParameterSyntax { Type: not null } parameter)
                    builder.AppendInterpolation($"{indent}{parameter.Type} {parameter.Identifier.ValueText}{getterSetter}");

            // Deconstruct()
            if (!recordDeconstructOverwrittenFlag) {
                builder.AppendInterpolation($"{indent}void Deconstruct(");

                foreach (ParameterSyntax parameter in recordParameterList)
                    builder.AppendInterpolation($"out {parameter.Type} {parameter.Identifier.ValueText}, ");
                builder.Length -= 2;

                builder.Append(");\n\n");
            }
        }


        builder.Length--;
        indent.DecreaseLevel();
        builder.AppendInterpolation($"{indent}}}\n");

        // nesting
        for (int i = 0; i < attribute.nested.Length; i++) {
            indent.DecreaseLevel();
            builder.AppendInterpolation($"{indent}}}\n");
        }

        string source = builder.ToString();


        builder.Clear();

        if (attribute.name is null)
            builder.AppendInterpolation($"I{targetSymbol.Name}");
        else
            builder.Append(attribute.name);
        builder.Append('_')
            .AppendNamespace(targetSymbol.ContainingNamespace)
            .AppendContainingTypes(targetSymbol.ContainingType)
            .Append(targetSymbol.Name)
            .AppendParameterList(targetSymbol)
            .AppendInterpolation($"_{Path.GetFileName(targetType.SyntaxTree.FilePath)}.g.cs");

        string hintName = builder.ToString();


        context.AddSource(hintName, source);

        stringBuilderPool.Return(builder);
    }
}
