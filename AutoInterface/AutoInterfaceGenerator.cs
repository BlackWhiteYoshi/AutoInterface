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
            (GeneratorAttributeSyntaxContext syntaxContext, CancellationToken _) => ((TypeDeclarationSyntax)syntaxContext.TargetNode, syntaxContext.Attributes))
            .SelectMany(((TypeDeclarationSyntax type, ImmutableArray<AttributeData> attributes) pair, CancellationToken _) => {
                ImmutableArray<ClassWithAttributeData>.Builder attributeWithClassList = ImmutableArray.CreateBuilder<ClassWithAttributeData>(pair.attributes.Length);
                foreach (AttributeData attributeData in pair.attributes)
                    attributeWithClassList.Add(new ClassWithAttributeData(pair.type, attributeData));
                return attributeWithClassList;
            });

        context.RegisterSourceOutput(interfaceTypeProvider, Execute);
    }


    private void Execute(SourceProductionContext context, ClassWithAttributeData provider) {
        TypeDeclarationSyntax targetType = provider.Type;
        AttributeData attributeData = provider.AttributeData;

        (string name, string modifier, string namspace, string[] inheritance, bool staticMembers) attribute;
        {
            if (attributeData.NamedArguments.Length > 0) {
                attribute.name = attributeData.NamedArguments.GetArgument<string>("Name") ?? DefaultName(targetType);
                attribute.modifier = attributeData.NamedArguments.GetArgument<string>("Modifier") ?? DefaultModifier();
                attribute.namspace = attributeData.NamedArguments.GetArgument<string>("Namespace") ?? DefaultNamespace(targetType);
                
                // attribute.inheritance
                if (attributeData.NamedArguments.GetArgument("Inheritance") is TypedConstant { Kind: TypedConstantKind.Array } typeArray) {
                    attribute.inheritance = new string[typeArray.Values.Length];
                    for (int i = 0; i < attribute.inheritance.Length; i++)
                        attribute.inheritance[i] = typeArray.Values[i].Value?.ToString() ?? string.Empty;
                }
                else
                    attribute.inheritance = DefaultInheritance();

                attribute.staticMembers = attributeData.NamedArguments.GetArgument<bool>("StaticMembers");

            }
            else {
                attribute.name = DefaultName(targetType);
                attribute.modifier = DefaultModifier();
                attribute.namspace = DefaultNamespace(targetType);
                attribute.inheritance = DefaultInheritance();
                attribute.staticMembers = false;
            }


            static string DefaultName(TypeDeclarationSyntax targetType) => $"I{targetType.Identifier.ValueText}";
            
            static string DefaultModifier() => "public";
            
            static string DefaultNamespace(TypeDeclarationSyntax targetType) {
                BaseNamespaceDeclarationSyntax? namspace = targetType.GetParent<BaseNamespaceDeclarationSyntax>();
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

            static string[] DefaultInheritance() => [];
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
        if (attribute.namspace != string.Empty) {
            builder.Append("namespace ");
            builder.Append(attribute.namspace);
            builder.Append(';');
            builder.Append('\n');
            builder.Append('\n');
        }

        // summary
        {
            SyntaxTriviaList triviaList = targetType.AttributeLists[0].GetLeadingTrivia();
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
                    // public or explicit interface specifier
                    if (!methodDeclarationSyntax.Modifiers.Contains("public"))
                        if (!(methodDeclarationSyntax.ExplicitInterfaceSpecifier?.Name is IdentifierNameSyntax identifierSyntax && identifierSyntax.Identifier.ValueText == attribute.name))
                            break;

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

                    string? modifiers;
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
                            builder.Append("    ///");
                            builder.Append(documentationCommentTrivia.ToString());
                            break;
                        }

                    // attributes
                    if (methodDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append("    ");
                        builder.Append(methodDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
                    }

                    builder.Append("    ");
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

                    // attributes
                    if (propertyDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append("    ");
                        builder.Append(propertyDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
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

                    // attributes
                    if (indexerDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append("    ");
                        builder.Append(indexerDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
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

                    // attributes
                    if (eventFieldDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append("    ");
                        builder.Append(eventFieldDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
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

                    // attributes
                    if (eventDeclarationSyntax.AttributeLists.Count > 0) {
                        builder.Append("    ");
                        builder.Append(eventDeclarationSyntax.AttributeLists.ToString());
                        builder.Append('\n');
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

                builder.Append("    ");
                builder.Append(parameter.Type.ToString());
                builder.Append(' ');
                builder.Append(parameter.Identifier.ValueText);
                builder.Append(getterSetter);
            }

            // Deconstruct()
            if (!recordDeconstructOverwrittenFlag) {
                builder.Append("    void Deconstruct(");
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
        builder.Append('}');
        builder.Append('\n');

        string interfaceName = attribute.name;
        string className = targetType.Identifier.ValueText;
        string fileName = Path.GetFileName(targetType.SyntaxTree.FilePath);
        context.AddSource($"{interfaceName}_{className}_{fileName}.g.cs", builder.ToString());

        stringBuilderPool.Return(builder);
    }
}
