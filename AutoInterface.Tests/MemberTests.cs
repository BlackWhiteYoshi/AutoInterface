namespace AutoInterface.Tests;

public sealed class MemberTests {
    [Test]
    public async ValueTask NoMembers() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test { }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask NoInterfacing() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                internal void SomeMethod() { }

                private int Prop => 1;

                internal int AccessorProp { get; private set; }

                public int OneProp { private get; set; }

                public static void StaticMethod() { }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int OneProp { set; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    #region Method

    [Test]
    public async ValueTask Method() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int MTest() => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int MTest();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_Generic() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public T? MTest<T>() where T : INumber<T> => default;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                T? MTest<T>() where T : INumber<T>;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_Parameter() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int MTest(int number, string str) => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int MTest(int number, string str);
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_Summary() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public int MTest() => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                int MTest();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_SummaryWithAttributes() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                public int MTest() => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                int MTest();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_Async() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public async Task TestAsync() => Task.CompletedTask;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                Task TestAsync();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_FullName() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public global::System.Collections.Generic.List<int> NameTest() => new();
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                global::System.Collections.Generic.List<int> NameTest();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_Explicit() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                int ITest.ExplicitTest() => -1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int ExplicitTest();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_IgnoreAutoInterfaceAttribute() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                [IgnoreAutoInterface]
                public int NoTest() => 0;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_Everything() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public int MTest() => 1;

                public async Task TestAsync() => Task.CompletedTask;

                public global::System.Collections.Generic.List<int> NameTest() => new();

                int ITest.ExplicitTest() => -1;

                [IgnoreAutoInterface]
                public int NoTest() => 0;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                int MTest();

                Task TestAsync();

                global::System.Collections.Generic.List<int> NameTest();

                int ExplicitTest();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion


    #region Property

    [Test]
    public async ValueTask Property() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int PTest => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int PTest { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_Summary() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public int PTest => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                int PTest { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_SummaryWithAttributes() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                public int PTest => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                int PTest { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_Get() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int TestGet { get; } = 2;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int TestGet { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_Set() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int TestSet { set; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int TestSet { set; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_GetSet() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int TestGetSet { get; set; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int TestGetSet { get; set; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_Init() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int TestInit { init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int TestInit { init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_GetInit() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public int TestGetInit { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int TestGetInit { get; init; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_FullName() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public global::System.Collections.Generic.List<int> NameTest => new();
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                global::System.Collections.Generic.List<int> NameTest { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_Explicit() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                int ITest.ExplicitTest => -1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                int ExplicitTest { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_IgnoreAutoInterfaceAttribute() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                [IgnoreAutoInterface]
                public int NoTest => 0;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_Everything() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public int PTest => 1;

                public int TestGet { get; } = 2;

                public int TestSet { set; }

                public int TestGetSet { get; set; }

                public int TestInit { init; }

                public int TestGetInit { get; init; }

                public global::System.Collections.Generic.List<int> NameTest => new();

                int ITest.ExplicitTest => -1;

                [IgnoreAutoInterface]
                public int NoTest => 0;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                int PTest { get; }

                int TestGet { get; }

                int TestSet { set; }

                int TestGetSet { get; set; }

                int TestInit { init; }

                int TestGetInit { get; init; }

                global::System.Collections.Generic.List<int> NameTest { get; }

                int ExplicitTest { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion


    #region Indexer

    [Test]
    public async ValueTask Indexer() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestGet {
                public int this[int i] {
                    get => i;
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITestGet {
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);

    }

    [Test]
    public async ValueTask Indexer_Summary() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestGet {
                /// <summary>
                /// my description
                /// </summary>
                public int this[int i] {
                    get => i;
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITestGet {
                /// <summary>
                /// my description
                /// </summary>
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_SummaryWithAttributes() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestGet {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                public int this[int i] {
                    get => i;
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITestGet {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_Get() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestGet {
                public int this[int i] {
                    get { return i; }
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITestGet {
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);

    }

    [Test]
    public async ValueTask Indexer_Set() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestSet {
                public int this[int i] {
                    set { }
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITestSet {
                int this[int i] { set; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_GetSet() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestGetSet {
                public int this[int i] {
                    get => i;
                    set { }
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITestGetSet {
                int this[int i] { get; set; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_Explicit() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestExplicit {
                int ITestExplicit.this[int i] {
                    get => i
                    set { }
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITestExplicit {
                int this[int i] { get; set; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_IgnoreAutoInterfaceAttribute() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class NoTest {
                [IgnoreAutoInterface]
                public int this[int i] {
                    get { return i; }
                    set { }
                }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface INoTest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_Everything() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class TestGet {
                /// <summary>
                /// my description
                /// </summary>
                public int this[int i] {
                    get { return i; }
                }
            }

            [AutoInterface]
            public class TestSet {
                public int this[int i] {
                    set { }
                }
            }

            [AutoInterface]
            public class TestGetSet {
                public int this[int i] {
                    get { return i; }
                    set { }
                }
            }

            [AutoInterface]
            public class TestExplicit {
                int ITestExplicit.this[int i] {
                    get { return i; }
                    set { }
                }
            }

            [AutoInterface]
            public class NoTest {
                [IgnoreAutoInterface]
                public int this[int i] {
                    get { return i; }
                    set { }
                }
            }

            """;
        string[] sourceText = input.GenerateSourceText(out _, out _);
        sourceText = [.. sourceText.Skip(sourceText.Length - 5).Take(5)];

        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITestGet {
                    /// <summary>
                    /// my description
                    /// </summary>
                    int this[int i] { get; }
                }

                """;
            await Assert.That(sourceText[0]).IsEqualTo(expected);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITestSet {
                    int this[int i] { set; }
                }

                """;
            await Assert.That(sourceText[1]).IsEqualTo(expected);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITestGetSet {
                    int this[int i] { get; set; }
                }

                """;
            await Assert.That(sourceText[2]).IsEqualTo(expected);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITestExplicit {
                    int this[int i] { get; set; }
                }

                """;
            await Assert.That(sourceText[3]).IsEqualTo(expected);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface INoTest {}

                """;
            await Assert.That(sourceText[4]).IsEqualTo(expected);
        }
    }

    #endregion


    #region EventField

    [Test]
    public async ValueTask EventField() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public event Action ATest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventField_Summary() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public event Action ATest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventField_SummaryWithAttributes() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                public event Action ATest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventField_IgnoreAutoInterfaceAttribute() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                [IgnoreAutoInterface]
                public event Action NoTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventField_Everything() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public event Action ATest;

                [IgnoreAutoInterface]
                public event Action NoTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion


    #region EventProperty

    [Test]
    public async ValueTask EventProperty() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                public event Action ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventProperty_Summary() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public event Action ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventProperty_SummaryWithAttributes() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                public event Action ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                [Test, Test2]
                [Test3]
                event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventProperty_Explicit() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                event Action ITest.ExplicitTest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                event Action ExplicitTest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventProperty_IgnoreAutoInterfaceAttribute() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                [IgnoreAutoInterface]
                public event Action NoTest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventProperty_Everything() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                public event Action ATest { add { } remove { } }

                event Action ITest.ExplicitTest { add { } remove { } }

                [IgnoreAutoInterface]
                public event Action NoTest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                /// <summary>
                /// my description
                /// </summary>
                event Action ATest;

                event Action ExplicitTest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion
}
