using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace AutoInterface;

[Generator(LanguageNames.CSharp)]
public sealed class AutoInterfaceGenerator : IIncrementalGenerator {
    /// <summary>
    /// Container for 2 Nodes: The attribute AutoInterfaceAttribute together with the corresponding class/struct.
    /// </summary>
    /// <param name="Attribute"></param>
    /// <param name="Type"></param>
    private readonly record struct AttributeWithClass(AttributeSyntax Attribute, TypeDeclarationSyntax Type) : IEquatable<AttributeWithClass>;


    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // register attribute marker
        context.RegisterPostInitializationOutput(static (IncrementalGeneratorPostInitializationContext context) => {
            context.AddSource("AutoInterfaceAttribute.g.cs", Attributes.AutoInterfaceAttribute);
            context.AddSource("IgnoreAutoInterfaceAttribute.g.cs", Attributes.IgnoreAutoInterfaceAttribute);
        });

        // all classes/structs with AutoInterfaceAttribute
        IncrementalValuesProvider<AttributeWithClass> interfaceTypeProvider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform);

        context.RegisterSourceOutput(interfaceTypeProvider, Execute);
    }


    private static bool Predicate(SyntaxNode syntaxNode, CancellationToken _) {
        if (syntaxNode is not AttributeSyntax attributeSyntax)
            return false;

        if (attributeSyntax.Parent?.Parent is not (ClassDeclarationSyntax or StructDeclarationSyntax))
            return false;


        string identifier = attributeSyntax.Name switch {
            SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
            _ => string.Empty
        };

        if (identifier != "AutoInterface" && identifier != "AutoInterfaceAttribute")
            return false;


        return true;
    }

    private static AttributeWithClass Transform(GeneratorSyntaxContext syntaxContext, CancellationToken _) {
        AttributeSyntax attribute = (AttributeSyntax)syntaxContext.Node;
        TypeDeclarationSyntax type = (TypeDeclarationSyntax)attribute.Parent!.Parent!;
        return new AttributeWithClass(attribute, type);
    }


    private static void Execute(SourceProductionContext context, AttributeWithClass provider) {
        (string name, string modifier, string namspace, string[] inheritance, bool staticMembers) attribute;
        {
            if (provider.Attribute.ArgumentList != null) {
                // Name
                if (provider.Attribute.ArgumentList.GetLiteral("Name") is LiteralExpressionSyntax nameLiteral)
                    attribute.name = nameLiteral.Token.ValueText;
                else
                    attribute.name = DefaultName(provider);

                // Modifier
                if (provider.Attribute.ArgumentList.GetLiteral("Modifier") is LiteralExpressionSyntax modifierLiteral)
                    attribute.modifier = modifierLiteral.Token.ValueText;
                else
                    attribute.modifier = DefaultModifier();

                // Namespace
                if (provider.Attribute.ArgumentList.GetLiteral("Namespace") is LiteralExpressionSyntax namespaceLiteral)
                    attribute.namspace = namespaceLiteral.Token.ValueText;
                else
                    attribute.namspace = DefaultNamespace(provider);

                // Inheritance
                {
                    attribute.inheritance = provider.Attribute.ArgumentList.GetExpression("Inheritance") switch {
                        CollectionExpressionSyntax collectionExpression => ExpressionElementsToStringArray(collectionExpression),
                        ImplicitArrayCreationExpressionSyntax arrayExpression => ExpressionsToStringArray(arrayExpression.Initializer),
                        ArrayCreationExpressionSyntax { Initializer: InitializerExpressionSyntax initializerExpression } => ExpressionsToStringArray(initializerExpression),
                        _ => DefaultInheritance()
                    };

                    static string[] ExpressionsToStringArray(InitializerExpressionSyntax initializerExpression) {
                        string[] result = new string[initializerExpression.Expressions.Count];

                        for (int i = 0; i < initializerExpression.Expressions.Count; i++)
                            if (initializerExpression.Expressions[i] is TypeOfExpressionSyntax typeOfExpression)
                                result[i] = typeOfExpression.Type.ToString();
                    
                        return result;
                    }

                    static string[] ExpressionElementsToStringArray(CollectionExpressionSyntax collectionExpression) {
                        string[] result = new string[collectionExpression.Elements.Count];

                        for (int i = 0; i < collectionExpression.Elements.Count; i++)
                            if (collectionExpression.Elements[i] is ExpressionElementSyntax expression && expression.Expression is TypeOfExpressionSyntax typeOfExpression)
                                result[i] = typeOfExpression.Type.ToString();

                        return result;
                    }
                }

                // StaticMembers
                if (provider.Attribute.ArgumentList.GetLiteral("StaticMembers") is LiteralExpressionSyntax staticMembersLiteral)
                    attribute.staticMembers = staticMembersLiteral.Token.Value as bool? ?? DefaultStaticMembers();
                else
                    attribute.staticMembers = DefaultStaticMembers();
            }
            else {
                attribute.name = DefaultName(provider);
                attribute.modifier = DefaultModifier();
                attribute.namspace = DefaultNamespace(provider);
                attribute.inheritance = DefaultInheritance();
                attribute.staticMembers = DefaultStaticMembers();
            }


            static string DefaultName(AttributeWithClass provider) => $"I{provider.Type.Identifier.ValueText}";
            
            static string DefaultModifier() => "public";
            
            static string DefaultNamespace(AttributeWithClass provider) {
                BaseNamespaceDeclarationSyntax? namspace = provider.Type.GetParent<BaseNamespaceDeclarationSyntax>();
                if (namspace == null)
                    return string.Empty;
                
                BaseNamespaceDeclarationSyntax? parentNamespace = namspace.GetParent<BaseNamespaceDeclarationSyntax>();
                if (parentNamespace == null)
                    return namspace.Name.ToString();


                StringBuilder namespaceBuilder = new();
                AppendNamespace(parentNamespace, namespaceBuilder);
                namespaceBuilder.Append(namspace.Name.ToString());
                return namespaceBuilder.ToString();

                static void AppendNamespace(BaseNamespaceDeclarationSyntax namspace, StringBuilder namespaceBuilder) {
                    BaseNamespaceDeclarationSyntax? parentNamespace = namspace.GetParent<BaseNamespaceDeclarationSyntax>();
                    if (parentNamespace != null)
                        AppendNamespace(parentNamespace, namespaceBuilder);

                    namespaceBuilder.Append(namspace.Name.ToString());
                    namespaceBuilder.Append('.');
                }
            }

            static string[] DefaultInheritance() => Array.Empty<string>();

            static bool DefaultStaticMembers() => false;
        }


        StringBuilder builder = new(65536);
        builder.Append("""
            // <auto-generated/>
            #pragma warning disable
            #nullable enable annotations



            """);

        // usingStatements
        {
            BaseNamespaceDeclarationSyntax? namspace = provider.Type.GetParent<BaseNamespaceDeclarationSyntax>();
            while (namspace != null) {
                string usings = namspace.Usings.ToString();
                if (usings != string.Empty) {
                    builder.Append(usings);
                    builder.Append('\n');
                }
                namspace = namspace.GetParent<BaseNamespaceDeclarationSyntax>();
            }
            
            CompilationUnitSyntax? compilationUnit = provider.Type.GetParent<CompilationUnitSyntax>();
            if (compilationUnit != null) {
                builder.Append(compilationUnit.Usings.ToString());
                builder.Append('\n');
            }

            builder.Append('\n');
        }

        // namespace
        if (attribute.namspace != string.Empty) {
            builder.Append("namespace ");
            builder.Append(attribute.namspace);
            builder.Append(';');
            builder.Append('\n');
            builder.Append('\n');
        }

        // summary
        {
            SyntaxTriviaList triviaList = provider.Type.AttributeLists[0].GetLeadingTrivia();
            foreach (SyntaxTrivia trivia in triviaList)
                if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                    builder.Append("///");
                    builder.Append(documentationCommentTrivia.ToString());
                    break;
                }
        }
        // class/struct declaration
        builder.Append(attribute.modifier);
        builder.Append(" interface ");
        builder.Append(attribute.name);
        if (provider.Type.TypeParameterList?.Parameters.Count > 0) {
            builder.Append('<');

            foreach (TypeParameterSyntax parameter in provider.Type.TypeParameterList.Parameters) {
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
            builder.Append(attribute.inheritance[0]);
            for (int i = 1; i < attribute.inheritance.Length; i++) {
                builder.Append(',');
                builder.Append(' ');
                builder.Append(attribute.inheritance[i]);
            }
        }
        builder.Append(' ');
        builder.Append('{');
        builder.Append('\n');

        foreach (MemberDeclarationSyntax member in provider.Type.Members) {
            switch (member) {
                case MethodDeclarationSyntax methodDeclarationSyntax: {
                    if (methodDeclarationSyntax.GetAttribute("IgnoreAutoInterface") != null)
                        break;

                    // public or explicit interface specifier
                    if (!methodDeclarationSyntax.Modifiers.Contains("public"))
                        if (!(methodDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is IdentifierNameSyntax identifierSyntax && identifierSyntax.Identifier.ValueText == attribute.name))
                            break;

                    string? modifiers;
                    if (!methodDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    {
                        SyntaxTriviaList triviaList = methodDeclarationSyntax.AttributeLists.Count switch {
                            > 0 => methodDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                            _ => methodDeclarationSyntax.Modifiers.Count switch {
                                > 0 => methodDeclarationSyntax.Modifiers[0].LeadingTrivia,
                                _ => methodDeclarationSyntax.ReturnType.GetLeadingTrivia()
                            }
                        };
                        foreach (SyntaxTrivia trivia in triviaList)
                            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                                builder.Append("    ///");
                                builder.Append(documentationCommentTrivia.ToString());
                                break;
                            }
                    }

                    builder.Append("    ");
                    builder.Append(modifiers);
                    builder.Append(methodDeclarationSyntax.ReturnType.ToString());
                    builder.Append(' ');
                    builder.Append(methodDeclarationSyntax.Identifier.ValueText);

                    builder.Append(methodDeclarationSyntax.ParameterList.ToString());
                    builder.Append(';');

                    builder.Append('\n');
                    builder.Append('\n');

                    break;
                }

                case PropertyDeclarationSyntax propertyDeclarationSyntax: {
                    if (propertyDeclarationSyntax.GetAttribute("IgnoreAutoInterface") != null)
                        break;

                    // public or explicit interface specifier
                    if (!propertyDeclarationSyntax.Modifiers.Contains("public"))
                        if (!(propertyDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is IdentifierNameSyntax identifierSyntax && identifierSyntax.Identifier.ValueText == attribute.name))
                            break;

                    string? modifiers;
                    if (!propertyDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    {
                        SyntaxTriviaList triviaList = propertyDeclarationSyntax.AttributeLists.Count switch {
                            > 0 => propertyDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                            _ => propertyDeclarationSyntax.Modifiers.Count switch {
                                > 0 => propertyDeclarationSyntax.Modifiers[0].LeadingTrivia,
                                _ => propertyDeclarationSyntax.Type.GetLeadingTrivia()
                            }
                        };
                        foreach (SyntaxTrivia trivia in triviaList)
                            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                                builder.Append("    ///");
                                builder.Append(documentationCommentTrivia.ToString());
                                break;
                            }
                    }

                    builder.Append("    ");
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

                    break;
                }

                case IndexerDeclarationSyntax indexerDeclarationSyntax: {
                    if (indexerDeclarationSyntax.GetAttribute("IgnoreAutoInterface") != null)
                        break;

                    // public or explicit interface specifier
                    if (!indexerDeclarationSyntax.Modifiers.Contains("public"))
                        if (!(indexerDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is IdentifierNameSyntax identifierSyntax && identifierSyntax.Identifier.ValueText == attribute.name))
                            break;

                    string? modifiers;
                    if (!indexerDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    {
                        SyntaxTriviaList triviaList = indexerDeclarationSyntax.AttributeLists.Count switch {
                            > 0 => indexerDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                            _ => indexerDeclarationSyntax.Modifiers.Count switch {
                                > 0 => indexerDeclarationSyntax.Modifiers[0].LeadingTrivia,
                                _ => indexerDeclarationSyntax.Type.GetLeadingTrivia()
                            }
                        };
                        foreach (SyntaxTrivia trivia in triviaList)
                            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                                builder.Append("    ///");
                                builder.Append(documentationCommentTrivia.ToString());
                                break;
                            }
                    }

                    builder.Append("    ");
                    builder.Append(modifiers);
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

                    break;
                }

                case EventFieldDeclarationSyntax eventFieldDeclarationSyntax: {
                    if (eventFieldDeclarationSyntax.GetAttribute("IgnoreAutoInterface") != null)
                        break;

                    if (!eventFieldDeclarationSyntax.Modifiers.Contains("public"))
                        break;

                    string? modifiers;
                    if (!eventFieldDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    {
                        SyntaxTriviaList triviaList = eventFieldDeclarationSyntax.AttributeLists.Count switch {
                            > 0 => eventFieldDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                            _ => eventFieldDeclarationSyntax.Modifiers.Count switch {
                                > 0 => eventFieldDeclarationSyntax.Modifiers[0].LeadingTrivia,
                                _ => eventFieldDeclarationSyntax.EventKeyword.LeadingTrivia
                            }
                        };
                        foreach (SyntaxTrivia trivia in triviaList)
                            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                                builder.Append("    ///");
                                builder.Append(documentationCommentTrivia.ToString());
                                break;
                            }
                    }

                    builder.Append("    ");
                    builder.Append(modifiers);
                    builder.Append("event ");
                    builder.Append(eventFieldDeclarationSyntax.Declaration.Type.ToString());
                    builder.Append(' ');
                    builder.Append(eventFieldDeclarationSyntax.Declaration.Variables.ToString());
                    builder.Append(';');

                    builder.Append('\n');
                    builder.Append('\n');

                    break;
                }

                case EventDeclarationSyntax eventDeclarationSyntax: {
                    if (eventDeclarationSyntax.GetAttribute("IgnoreAutoInterface") != null)
                        break;

                    // public or explicit interface specifier
                    if (!eventDeclarationSyntax.Modifiers.Contains("public"))
                        if (!(eventDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is IdentifierNameSyntax identifierSyntax && identifierSyntax.Identifier.ValueText == attribute.name))
                            break;

                    string? modifiers;
                    if (!eventDeclarationSyntax.Modifiers.Contains("static"))
                        modifiers = string.Empty; // object-method (non static)
                    else if (attribute.staticMembers)
                        modifiers = "static abstract "; // static-method and static is enabled
                    else
                        break;  // ignore static member


                    // summary
                    {
                        SyntaxTriviaList triviaList = eventDeclarationSyntax.AttributeLists.Count switch {
                            > 0 => eventDeclarationSyntax.AttributeLists[0].GetLeadingTrivia(),
                            _ => eventDeclarationSyntax.Modifiers.Count switch {
                                > 0 => eventDeclarationSyntax.Modifiers[0].LeadingTrivia,
                                _ => eventDeclarationSyntax.EventKeyword.LeadingTrivia
                            }
                        };
                        foreach (SyntaxTrivia trivia in triviaList)
                            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationCommentTrivia) {
                                builder.Append("    ///");
                                builder.Append(documentationCommentTrivia.ToString());
                                break;
                            }
                    }

                    builder.Append("    ");
                    builder.Append(modifiers);
                    builder.Append("event ");
                    builder.Append(eventDeclarationSyntax.Type.ToString());
                    builder.Append(' ');
                    builder.Append(eventDeclarationSyntax.Identifier.ToString());
                    builder.Append(';');

                    builder.Append('\n');
                    builder.Append('\n');

                    break;
                }
            }
        }

        builder.Length--;
        builder.Append('}');
        builder.Append('\n');

        string interfaceName = attribute.name;
        string className = provider.Type.Identifier.ValueText;
        string fileName = Path.GetFileName(provider.Type.SyntaxTree.FilePath);
        context.AddSource($"{interfaceName}_{className}_{fileName}.g.cs", builder.ToString());
    }
}
