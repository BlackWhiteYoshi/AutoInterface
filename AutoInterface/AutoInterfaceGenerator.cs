﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Immutable;
using System.Text;

namespace AutoInterface;

[Generator(LanguageNames.CSharp)]
public sealed partial class AutoInterfaceGenerator : IIncrementalGenerator {
    private readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool(initialCapacity: 8192, maximumRetainedCapacity: 1024 * 1024);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // register attribute marker
        context.RegisterPostInitializationOutput(static (IncrementalGeneratorPostInitializationContext context) => {
            context.AddSource("AutoInterfaceAttribute.g.cs", Attributes.AutoInterfaceAttribute);
            context.AddSource("IgnoreAutoInterfaceAttribute.g.cs", Attributes.IgnoreAutoInterfaceAttribute);
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
        const char INDENTCHAR = ' ';
        const int INDENTLEVEL = 4;
        int currentIndent = 0;

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
                if (usings != string.Empty) {
                    builder.Append(usings);
                    builder.Append('\n');
                }
                namspace = namspace.GetParent<BaseNamespaceDeclarationSyntax>();
            }
            
            CompilationUnitSyntax? compilationUnit = targetType.GetParent<CompilationUnitSyntax>();
            if (compilationUnit != null) {
                builder.Append(compilationUnit.Usings.ToString());
                builder.Append('\n');
            }

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

                builder.Append("namespace ");
                builder.AppendNamespace(namespaceSymbol.ContainingNamespace);
                builder.Append(namespaceSymbol.Name);
                builder.Append(";\n\n");
                break;
            default:
                builder.Append("namespace ");
                builder.Append(attribute.namspace);
                builder.Append(";\n\n");
                break;
        }

        // nesting
        foreach (string containingType in attribute.nested) {
            builder.Append(INDENTCHAR, currentIndent);
            builder.Append(containingType);
            builder.Append(" {\n");
            currentIndent += INDENTLEVEL;
        }

        // summary
        {
            SyntaxTriviaList triviaList = targetType.AttributeLists[0].GetLeadingTrivia();
            foreach (SyntaxTrivia trivia in triviaList)
                if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                    builder.Append(INDENTCHAR, currentIndent);
                    builder.Append("///");
                    builder.Append(documentationCommentTrivia.ToString());
                    break;
                }
        }

        // class/struct declaration
        builder.Append(INDENTCHAR, currentIndent);
        builder.Append(attribute.modifier);
        builder.Append(" interface ");
        if (attribute.name is null) {
            builder.Append('I');
            builder.Append(targetSymbol.Name);
        }
        else
            builder.Append(attribute.name);

        if (targetType.TypeParameterList?.Parameters.Count > 0) {
            builder.Append('<');

            foreach (TypeParameterSyntax parameter in targetType.TypeParameterList.Parameters) {
                builder.Append(parameter.Identifier.ValueText);
                builder.Append(',');
                builder.Append(' ');
            }

            builder.Length -= 2;
            builder.Append('>');
        }

        if (attribute.inheritance.Length > 0) {
            builder.Append(' ');
            builder.Append(':');
            builder.Append(' ');
            builder.Append(attribute.inheritance[0].ToDisplayString());
            for (int i = 1; i < attribute.inheritance.Length; i++) {
                builder.Append(',');
                builder.Append(' ');
                builder.Append(attribute.inheritance[i].ToDisplayString());
            }
        }

        builder.Append(' ');
        builder.Append('{');
        builder.Append('\n');


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
                            builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                            builder.Append("///");
                            builder.Append(documentationCommentTrivia.ToString());
                            break;
                        }

                    // attributes
                    if (methodDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                        builder.Append(methodDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
                    }

                    builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                    builder.Append(modifiers);
                    builder.Append(methodDeclarationSyntax.ReturnType.ToString());
                    builder.Append(' ');
                    builder.Append(methodDeclarationSyntax.Identifier.ValueText);

                    if (methodDeclarationSyntax.TypeParameterList != null)
                        builder.Append(methodDeclarationSyntax.TypeParameterList.ToString());

                    builder.Append(methodDeclarationSyntax.ParameterList.ToString());

                    string constraintClauses = methodDeclarationSyntax.ConstraintClauses.ToString();
                    if (constraintClauses.Length > 0) {
                        builder.Append(' ');
                        builder.Append(constraintClauses);
                    }

                    builder.Append(';');
                    builder.Append('\n');
                    builder.Append('\n');

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
                            builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                            builder.Append("///");
                            builder.Append(documentationCommentTrivia.ToString());
                            break;
                        }

                    // attributes
                    if (propertyDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                        builder.Append(propertyDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
                    }

                    builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                    builder.Append(modifiers);
                    builder.Append(propertyDeclarationSyntax.Type.ToString());
                    builder.Append(' ');
                    builder.Append(propertyDeclarationSyntax.Identifier.ValueText);
                    builder.Append(' ');
                    
                    builder.Append('{');
                    builder.Append(' ');
                    if (propertyDeclarationSyntax.AccessorList != null)
                        foreach (AccessorDeclarationSyntax accessor in propertyDeclarationSyntax.AccessorList.Accessors) {
                            if (accessor.Modifiers.Count > 0) // non public
                                continue;

                            builder.Append(accessor.Keyword.ValueText);
                            builder.Append(';');
                            builder.Append(' ');
                        }
                    else
                        builder.Append("get; ");  // body-expression syntax: "=> "
                    builder.Append('}');

                    builder.Append('\n');
                    builder.Append('\n');

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
                            builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                            builder.Append("///");
                            builder.Append(documentationCommentTrivia.ToString());
                            break;
                        }

                    // attributes
                    if (indexerDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                        builder.Append(indexerDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
                    }

                    builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                    builder.Append(indexerDeclarationSyntax.Type.ToString());
                    builder.Append(' ');
                    builder.Append("this");
                    builder.Append(indexerDeclarationSyntax.ParameterList.ToString());
                    builder.Append(' ');

                    builder.Append('{');
                    builder.Append(' ');
                    if (indexerDeclarationSyntax.AccessorList != null)
                        foreach (AccessorDeclarationSyntax accessor in indexerDeclarationSyntax.AccessorList.Accessors) {
                            if (accessor.Modifiers.Count > 0) // non public
                                continue;

                            builder.Append(accessor.Keyword.ValueText);
                            builder.Append(';');
                            builder.Append(' ');
                        }
                    else
                        builder.Append("get; ");  // body-expression syntax: "=> "
                    builder.Append('}');

                    builder.Append('\n');
                    builder.Append('\n');

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
                            builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                            builder.Append("///");
                            builder.Append(documentationCommentTrivia.ToString());
                            break;
                        }

                    // attributes
                    if (eventFieldDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                        builder.Append(eventFieldDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
                    }

                    builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                    builder.Append(modifiers);
                    builder.Append("event ");
                    builder.Append(eventFieldDeclarationSyntax.Declaration.Type.ToString());
                    builder.Append(' ');
                    builder.Append(eventFieldDeclarationSyntax.Declaration.Variables.ToString());
                    builder.Append(';');

                    builder.Append('\n');
                    builder.Append('\n');

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
                            builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                            builder.Append("///");
                            builder.Append(documentationCommentTrivia.ToString());
                            break;
                        }

                    // attributes
                    if (eventDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                        builder.Append(eventDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
                    }

                    builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                    builder.Append(modifiers);
                    builder.Append("event ");
                    builder.Append(eventDeclarationSyntax.Type.ToString());
                    builder.Append(' ');
                    builder.Append(eventDeclarationSyntax.Identifier.ToString());
                    builder.Append(';');

                    builder.Append('\n');
                    builder.Append('\n');

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

            for (int i = 0; i < recordParameterList.Count; i++) {
                if (recordParameterOverwrittenFlags[i])
                    continue;

                ParameterSyntax parameter = recordParameterList[i];
                if (parameter.Type == null)
                    continue;

                builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                builder.Append(parameter.Type.ToString());
                builder.Append(' ');
                builder.Append(parameter.Identifier.ValueText);
                builder.Append(getterSetter);
            }

            // Deconstruct()
            if (!recordDeconstructOverwrittenFlag) {
                builder.Append(INDENTCHAR, currentIndent + INDENTLEVEL);
                builder.Append("void Deconstruct(");
                foreach (ParameterSyntax parameter in recordParameterList) {
                    builder.Append("out ");
                    builder.Append(parameter.Type);
                    builder.Append(' ');
                    builder.Append(parameter.Identifier.ValueText);
                    builder.Append(',');
                    builder.Append(' ');
                }
                builder.Length -= 2;
                builder.Append(");\n\n");
            }
        }


        builder.Length--;
        builder.Append(INDENTCHAR, currentIndent);
        builder.Append("}\n");

        // nesting
        for (int i = 0; i < attribute.nested.Length; i++) {
            currentIndent -= INDENTLEVEL;
            builder.Append(INDENTCHAR, currentIndent);
            builder.Append("}\n");
        }

        string source = builder.ToString();

        
        builder.Clear();
        if (attribute.name is null) {
            builder.Append('I');
            builder.Append(targetSymbol.Name);
        }
        else
            builder.Append(attribute.name);

        builder.Append('_');
        builder.AppendNamespace(targetSymbol.ContainingNamespace);
        builder.AppendContainingTypes(targetSymbol.ContainingType);
        builder.Append(targetSymbol.Name);
        builder.AppendParameterList(targetSymbol);

        builder.Append('_');
        builder.Append(Path.GetFileName(targetType.SyntaxTree.FilePath));

        builder.Append(".g.cs");
        string hintName = builder.ToString();

        context.AddSource(hintName, source);

        stringBuilderPool.Return(builder);
    }
}
