﻿using System.Reflection;

namespace AutoInterface.Tests;

public sealed class AssemblyNameAndVersionTest {
    [Test]
    public async ValueTask AssemblyNameAndVersionMatch() {
        string assemblyName = typeof(AutoInterfaceGenerator).Assembly.GetName().Name!;
        string assemblyVersion = typeof(AutoInterfaceGenerator).Assembly.GetName().Version!.ToString(3);

        FieldInfo[] fields = typeof(Attributes).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
        string name = (string)fields[0].GetValue(null)!;
        string version = (string)fields[1].GetValue(null)!;

        await Assert.That(name).IsEqualTo(assemblyName);
        await Assert.That(version).IsEqualTo(assemblyVersion);
    }
}
