namespace AutoInterface.Tests;

public sealed class AttributeTests {
    [Test]
    [Arguments("Example")]
    [Arguments("asdf")]
    [Arguments("TestInterface")]
    [Arguments("WUWU")]
    public async ValueTask Name(string name) {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Name = "{{name}}")]
            public class Test { }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface {{name}} {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    [Arguments("internal")]
    [Arguments("public partial")]
    [Arguments("internal partial")]
    public async ValueTask Modifier(string modifier) {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Modifier = "{{modifier}}")]
            public class Test { }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            {{modifier}} interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    [Arguments("MySpace")]
    [Arguments("System.Generics")]
    [Arguments("BLAHBLAH.BLAH")]
    public async ValueTask Namespace(string namspace) {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Namespace = "{{namspace}}")]
            public class Test { }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace {{namspace}};

            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Namespace_Empty() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Namespace = "")]
            public class Test { }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    [Arguments("[]", "")]
    [Arguments("[typeof(ICore)]", ": MyCode.ICore ")]
    [Arguments("[typeof(A), typeof(B), typeof(C)]", ": MyCode.A, MyCode.B, MyCode.C ")]
    [Arguments("new[] { }", "")]
    [Arguments("new[] { typeof(ICore) }", ": MyCode.ICore ")]
    [Arguments("new[] { typeof(A), typeof(B), typeof(C) }", ": MyCode.A, MyCode.B, MyCode.C ")]
    //[Arguments("new Type[] { }", "")]
    //[Arguments("new Type[] { typeof(ICore) }", ": MyCode.ICore ")]
    //[Arguments("new Type[] { typeof(A), typeof(B), typeof(C) }", ": MyCode.A, MyCode.B, MyCode.C ")]
    public async ValueTask Inheritance(string inheritance, string result) {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = {{inheritance}})]
            public class Test { }


            public interface ICore;
            public interface A;
            public interface B;
            public interface C;

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {{result}}{}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask Nested() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Nested = ["public partial interface MyWrapper"])]
            public class Test;

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface MyWrapper {
                public partial interface ITest {    }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Nested_Triple() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Nested = ["public partial class MyWrapper", "public readonly partial struct MyWrapper2", "public partial interface OuterInterface"])]
            public class Test;

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial class MyWrapper {
                public readonly partial struct MyWrapper2 {
                    public partial interface OuterInterface {
                        public partial interface ITest {            }
                    }
                }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask NestedWithMembers() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Nested = ["public partial interface OuterInterface"])]
            public class Test {
                public int GetNumber() => 1;

                public int Number => 5;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface OuterInterface {
                public partial interface ITest {
                    int GetNumber();

                    int Number { get; }
                }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }


    [Test]
    public async ValueTask StaticMembers() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(StaticMembers = true)]
            public class Test {
                public static int GetNumber() => 1;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                static abstract int GetNumber();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }



    [Test]
    public async ValueTask AutoInterfaceVisibilityPublic() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test : ITest {
                [AutoInterfaceVisibilityPublic]
                public int MTest() => 1;

                [AutoInterfaceVisibilityPublic]
                public int PTest => 1;

                [AutoInterfaceVisibilityPublic]
                public int this[int i] {
                    get => i;
                }

                [AutoInterfaceVisibilityPublic]
                public event Action aTest;

                [AutoInterfaceVisibilityPublic]
                public event Action ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                [AutoInterfaceVisibilityPublic]
                public int MTest();

                [AutoInterfaceVisibilityPublic]
                public int PTest { get; }

                [AutoInterfaceVisibilityPublic]
                public int this[int i] { get; }

                [AutoInterfaceVisibilityPublic]
                public event Action aTest;

                [AutoInterfaceVisibilityPublic]
                public event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask AutoInterfaceVisibilityInternal() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test : ITest {
                [AutoInterfaceVisibilityInternal]
                int ITest.MTest() => 1;

                [AutoInterfaceVisibilityInternal]
                int ITest.PTest => 1;

                [AutoInterfaceVisibilityInternal]
                int ITest.this[int i] {
                    get => i;
                }

                [AutoInterfaceVisibilityInternal]
                public event Action aTest;

                [AutoInterfaceVisibilityInternal]
                event Action ITest.ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                [AutoInterfaceVisibilityInternal]
                internal int MTest();

                [AutoInterfaceVisibilityInternal]
                internal int PTest { get; }

                [AutoInterfaceVisibilityInternal]
                internal int this[int i] { get; }

                [AutoInterfaceVisibilityInternal]
                internal event Action aTest;

                [AutoInterfaceVisibilityInternal]
                internal event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask AutoInterfaceVisibilityProtected() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test : ITest {
                [AutoInterfaceVisibilityProtected]
                int ITest.MTest() => 1;

                [AutoInterfaceVisibilityProtected]
                int ITest.PTest => 1;

                [AutoInterfaceVisibilityProtected]
                int ITest.this[int i] {
                    get => i;
                }

                [AutoInterfaceVisibilityProtected]
                public event Action aTest;

                [AutoInterfaceVisibilityProtected]
                event Action ITest.ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                [AutoInterfaceVisibilityProtected]
                protected int MTest();

                [AutoInterfaceVisibilityProtected]
                protected int PTest { get; }

                [AutoInterfaceVisibilityProtected]
                protected int this[int i] { get; }

                [AutoInterfaceVisibilityProtected]
                protected event Action aTest;

                [AutoInterfaceVisibilityProtected]
                protected event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask AutoInterfaceVisibilityProtectedInternal() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test : ITest {
                [AutoInterfaceVisibilityProtectedInternal]
                int ITest.MTest() => 1;

                [AutoInterfaceVisibilityProtectedInternal]
                int ITest.PTest => 1;

                [AutoInterfaceVisibilityProtectedInternal]
                int ITest.this[int i] {
                    get => i;
                }

                [AutoInterfaceVisibilityProtectedInternal]
                public event Action aTest;

                [AutoInterfaceVisibilityProtectedInternal]
                event Action ITest.ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                [AutoInterfaceVisibilityProtectedInternal]
                protected internal int MTest();

                [AutoInterfaceVisibilityProtectedInternal]
                protected internal int PTest { get; }

                [AutoInterfaceVisibilityProtectedInternal]
                protected internal int this[int i] { get; }

                [AutoInterfaceVisibilityProtectedInternal]
                protected internal event Action aTest;

                [AutoInterfaceVisibilityProtectedInternal]
                protected internal event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask AutoInterfaceVisibilityPrivateProtected() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test : ITest {
                [AutoInterfaceVisibilityPrivateProtected]
                int ITest.MTest() => 1;

                [AutoInterfaceVisibilityPrivateProtected]
                int ITest.PTest => 1;

                [AutoInterfaceVisibilityPrivateProtected]
                int ITest.this[int i] {
                    get => i;
                }

                [AutoInterfaceVisibilityPrivateProtected]
                public event Action aTest;

                [AutoInterfaceVisibilityPrivateProtected]
                event Action ITest.ATest { add { } remove { } }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                [AutoInterfaceVisibilityPrivateProtected]
                private protected int MTest();

                [AutoInterfaceVisibilityPrivateProtected]
                private protected int PTest { get; }

                [AutoInterfaceVisibilityPrivateProtected]
                private protected int this[int i] { get; }

                [AutoInterfaceVisibilityPrivateProtected]
                private protected event Action aTest;

                [AutoInterfaceVisibilityPrivateProtected]
                private protected event Action ATest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask IgnoreAutoInterface() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test : ITest {
                [IgnoreAutoInterface]
                int ITest.MTest() => 1;

                [IgnoreAutoInterface]
                int ITest.PTest => 1;

                [IgnoreAutoInterface]
                int ITest.this[int i] {
                    get => i;
                }

                [IgnoreAutoInterface]
                event Action ITest.aTest;

                [IgnoreAutoInterface]
                event Action ITest.ATest { add { } remove { } }
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
}
