using Xunit;

namespace AutoInterface.Tests;

public static class OtherTypesTests {
    [Fact]
    public static void Generic() {
        const string input = $$"""
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public class Test<T> {
                public T? MTest() => default;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest<T> {
                T? MTest();
            }

            """;
        Assert.Equal(expected, sourceText);
    }


    [Fact]
    public static void Struct() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public struct Test {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Record() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record Test {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }


    [Fact]
    public static void RecordClass() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record class Test {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordClassParameterList() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record class Test(string Name) {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }

                string Name { get; init; }

                void Deconstruct(out string Name);
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordClassEmptyParameterList() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record class Test() {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }


    [Fact]
    public static void RecordStruct() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record struct Test {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordStructParameterList() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record struct Test(string Name) {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }

                string Name { get; set; }

                void Deconstruct(out string Name);
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordStructEmptyParameterList() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record struct Test() {
                public int Number { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; init; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordOverwriteProperty() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record Test(int Number) {
                public int Number { get; } = Number;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Number { get; }

                void Deconstruct(out int Number);
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordOverwriteField() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record Test(int Number) {
                private int Number = Number;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                void Deconstruct(out int Number);
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordOverwriteMultipleFields() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record Test(int Number, int Number2) {
                private int Number = Number, Number2 = Number2;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                void Deconstruct(out int Number, out int Number2);
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void RecordOverwriteDeconstrcut() {
        const string input = """
            using AutoInterfaceAttributes;
            
            namespace MyCode;
            
            [AutoInterface]
            public record Test(int Number) {
                public int Deconstruct(out int a) {
                    a = 1;
                    return 2;
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public interface ITest {
                int Deconstruct(out int a);

                int Number { get; init; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }
}
