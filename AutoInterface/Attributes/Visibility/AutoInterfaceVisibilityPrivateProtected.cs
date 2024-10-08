﻿namespace AutoInterface;

public static partial class Attributes {
    public const string AutoInterfaceVisibilityPrivateProtectedAttribute = $$"""
        // <auto-generated/>
        #pragma warning disable
        #nullable enable annotations


        #if !AUTOINTERFACE_EXCLUDE_ATTRIBUTES

        using System;

        namespace AutoInterfaceAttributes;

        /// <summary>
        /// Adds a "private protected" access modifier to the interface member.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("{{NAME}}", "{{VERSION}}")]
        internal sealed class AutoInterfaceVisibilityPrivateProtected : Attribute { }

        #endif

        """;
}
