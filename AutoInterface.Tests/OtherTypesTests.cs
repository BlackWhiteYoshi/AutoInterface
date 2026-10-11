namespace AutoInterface.Tests;

public sealed class OtherTypesTests {
    [Test]
    public async ValueTask Generic() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test<T> {
                public T PTest => default;

                public T? MTest() => default;

                public T this[int i] => default;

                public event Action ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest<T> {
                T PTest { get; }

                T? MTest();

                T this[int i] { get; }

                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask GenericExplicit() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test<T> {
                int ITest<T>.PTest => -1;

                T? ITest<T>.MTest() => default;

                T ITest<T>.this[int i] => default;

                event Action ITest<T>.ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest<T> {
                int PTest { get; }

                T? MTest();

                T this[int i] { get; }

                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask GenericWhereClause() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test<T> where T : System.IDisposable {
                public T Test => default;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest<T> where T : System.IDisposable {
                T Test { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask Struct() {
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

            public partial interface ITest {
                int Number { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Record() {
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

            public partial interface ITest {
                int Number { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask RecordClass() {
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

            public partial interface ITest {
                int Number { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordClassParameterList() {
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

            public partial interface ITest {
                int Number { get; init; }

                string Name { get; init; }

                void Deconstruct(out string Name);
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordClassEmptyParameterList() {
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

            public partial interface ITest {
                int Number { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask RecordStruct() {
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

            public partial interface ITest {
                int Number { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordStructParameterList() {
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

            public partial interface ITest {
                int Number { get; init; }

                string Name { get; set; }

                void Deconstruct(out string Name);
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordStructEmptyParameterList() {
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

            public partial interface ITest {
                int Number { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordOverwriteProperty() {
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

            public partial interface ITest {
                int Number { get; }

                void Deconstruct(out int Number);
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordOverwriteField() {
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

            public partial interface ITest {
                void Deconstruct(out int Number);
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordOverwriteMultipleFields() {
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

            public partial interface ITest {
                void Deconstruct(out int Number, out int Number2);
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask RecordOverwriteDeconstrcut() {
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

            public partial interface ITest {
                int Deconstruct(out int a);

                int Number { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
