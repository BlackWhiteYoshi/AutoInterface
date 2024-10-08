﻿namespace AutoInterface;

public static partial class Attributes {
    public const string IgnoreAutoInterfaceAttribute = $$"""
        // <auto-generated/>
        #pragma warning disable
        #nullable enable annotations


        #if !AUTOINTERFACE_EXCLUDE_ATTRIBUTES

        using System;

        namespace AutoInterfaceAttributes;

        /// <summary>
        /// The decorated member will be Ignored by the generator.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("{{NAME}}", "{{VERSION}}")]
        internal sealed class IgnoreAutoInterfaceAttribute : Attribute { }

        #endif

        """;
}
