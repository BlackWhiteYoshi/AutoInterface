#pragma warning disable

#if !AUTOINTERFACE_EXCLUDE_ATTRIBUTES

using System;
using System.Diagnostics;

namespace AutoInterfaceAttributes;

/// <summary>
/// Adds a "public" access modifier to the interface member.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
[Conditional("AUTO_INTERFACE_USAGES")]
[System.CodeDom.Compiler.GeneratedCodeAttribute("{{NAME}}", "{{VERSION}}")]
public sealed class AutoInterfaceVisibilityPublic : Attribute;

#endif