namespace AutoInterface.Tests;

public sealed class InheritanceTests {
    #region Method

    [Test]
    public async ValueTask Method() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestMethod() => 0;
            }

            public interface IBase {
               int TestMethod();
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_OtherTypeMember() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestMethod() => 0;
            }

            public interface IBase {
               int TestMethod { get; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_OtherReturnType() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestMethod() => 0;
            }

            public interface IBase {
               string TestMethod();
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_OtherTypeParameters() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestMethod() => 0;
            }

            public interface IBase {
               int TestMethod<T>();
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_OtherParameters() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestMethod() => 0;
            }

            public interface IBase {
               int TestMethod(int number);
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Method_OtherStatic() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestMethod() => 0;
            }

            public interface IBase {
               static abstract int TestMethod();
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion


    #region Property

    [Test]
    public async ValueTask Property() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestProperty => 0;
            }

            public interface IBase {
               int TestProperty { get; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_OtherTypeMember() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestProperty => 0;
            }

            public interface IBase {
               int TestProperty();
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_OtherReturnType() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestProperty => 0;
            }

            public interface IBase {
               string TestProperty { get; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_OtherAccessor() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestProperty { get; } = 0;
            }

            public interface IBase {
               int TestProperty { get; init; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Property_OtherStatic() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int TestProperty => 0;
            }

            public interface IBase {
               static abstract int TestProperty { get; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion


    #region Indexer

    [Test]
    public async ValueTask Indexer() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int this[int i] => i;
            }

            public interface IBase {
                int this[int i] { get; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_OtherTypeMember() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int this[int i] => i;
            }

            public interface IBase {
               int TestMethos(int i);
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_OtherReturnType() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int this[int i] => i;
            }

            public interface IBase {
               string this[int i] { get; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_OtherAccessor() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int this[int i] => i;
            }

            public interface IBase {
               int this[int i] { get; set; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Indexer_OtherParameters() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public int this[int i] => i;
            }

            public interface IBase {
               int this[string i] { get; }
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion


    #region EventField

    [Test]
    public async ValueTask EventField() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public event Action eventTest;
            }

            public interface IBase {
                event Action eventTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventField_OtherType() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public event Action eventTest;
            }

            public interface IBase {
                event Action<int> eventTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                event Action eventTest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventField_OtherStatic() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public event Action eventTest;
            }

            public interface IBase {
                static abstract event Action eventTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                event Action eventTest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion


    #region EventProperty

    [Test]
    public async ValueTask EventProperty() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public event Action EventTest { add { } remove { } }
            }

            public interface IBase {
                event Action EventTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventProperty_OtherType() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public event Action EventTest { add { } remove { } }
            }

            public interface IBase {
                event Action<int> EventTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                event Action EventTest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask EventProperty_OtherStatic() {
        string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Inheritance = [typeof(IBase)])]
            public class Test : ITest {
                public event Action EventTest { add { } remove { } }
            }

            public interface IBase {
                static abstract event Action< EventTest;
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest : MyCode.IBase {
                event Action EventTest;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    #endregion
}
