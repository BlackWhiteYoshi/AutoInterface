using System.Reflection;
using AutoInterfaceAttributes;
using Xunit;

namespace AutoInterface.Tests;

public static class AssemblyNameAndVersionTest {
    [Fact]
    public static void AssemblyNameAndVersionMatch() {
        string assemblyName = typeof(AutoInterfaceGenerator).Assembly.GetName().Name!;
        string assemblyVersion = typeof(AutoInterfaceGenerator).Assembly.GetName().Version!.ToString()[..^2];

        FieldInfo[] fields = typeof(AttributeInfo).GetFields();
        string name = (string)fields[0].GetValue(null)!;
        string version = (string)fields[1].GetValue(null)!;

        Assert.Equal(assemblyName, name);
        Assert.Equal(assemblyVersion, version);
    }
}
