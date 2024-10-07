#if !AUTOINTERFACE_EXCLUDE_ATTRIBUTES

using System;
using System.Diagnostics;

namespace AutoInterfaceAttributes;

/// <summary>
/// The decorated member will be Ignored by the generator.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
[Conditional("AUTO_INTERFACE_USAGES")]
[System.CodeDom.Compiler.GeneratedCodeAttribute(AttributeInfo.NAME, AttributeInfo.VERSION)]
public sealed class IgnoreAutoInterfaceAttribute : Attribute;

#endif