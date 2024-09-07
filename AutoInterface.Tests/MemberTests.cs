using Xunit;

namespace AutoInterface.Tests;

public static class MemberTests {
    [Fact]
    public static void NoMembers() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void NoInterfacing() {
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
        Assert.Equal(expected, sourceText);
    }


    #region Method

    [Fact]
    public static void Method() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_Generic() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_Parameter() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_Summary() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_SummaryWithAttributes() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_Async() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_FullName() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_Explicit() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_IgnoreAutoInterfaceAttribute() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_Everything() {
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
        Assert.Equal(expected, sourceText);
    }

    #endregion


    #region Property

    [Fact]
    public static void Property() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_Summary() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_SummaryWithAttributes() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_Get() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_Set() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_GetSet() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_Init() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_GetInit() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_FullName() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_Explicit() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_IgnoreAutoInterfaceAttribute() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_Everything() {
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
        Assert.Equal(expected, sourceText);
    }

    #endregion


    #region Indexer

    [Fact]
    public static void Indexer() {
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
        Assert.Equal(expected, sourceText);

    }

    [Fact]
    public static void Indexer_Summary() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_SummaryWithAttributes() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_Get() {
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
        Assert.Equal(expected, sourceText);

    }

    [Fact]
    public static void Indexer_Set() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_GetSet() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_Explicit() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_IgnoreAutoInterfaceAttribute() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_Everything() {
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
        sourceText = sourceText.Skip(sourceText.Length - 5).Take(5).ToArray();

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
            Assert.Equal(expected, sourceText[0]);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITestSet {
                    int this[int i] { set; }
                }

                """;
            Assert.Equal(expected, sourceText[1]);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITestGetSet {
                    int this[int i] { get; set; }
                }

                """;
            Assert.Equal(expected, sourceText[2]);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITestExplicit {
                    int this[int i] { get; set; }
                }

                """;
            Assert.Equal(expected, sourceText[3]);
        }
        {
            const string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface INoTest {}

                """;
            Assert.Equal(expected, sourceText[4]);
        }
    }

    #endregion


    #region EventField

    [Fact]
    public static void EventField() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventField_Summary() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventField_SummaryWithAttributes() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventField_IgnoreAutoInterfaceAttribute() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventField_Everything() {
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
        Assert.Equal(expected, sourceText);
    }

    #endregion


    #region EventProperty

    [Fact]
    public static void EventProperty() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventProperty_Summary() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventProperty_SummaryWithAttributes() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventProperty_Explicit() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventProperty_IgnoreAutoInterfaceAttribute() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventProperty_Everything() {
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
        Assert.Equal(expected, sourceText);
    }

    #endregion
}
