using Xunit;

namespace AutoInterface.Tests;

public static class InheritanceTests {
    #region Method

    [Fact]
    public static void Method() {
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

            public interface ITest : MyCode.IBase {}

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_OtherTypeMember() {
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

            public interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_OtherReturnType() {
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

            public interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_OtherTypeParameters() {
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

            public interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_OtherParameters() {
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

            public interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Method_OtherStatic() {
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

            public interface ITest : MyCode.IBase {
                int TestMethod();
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    #endregion


    #region Property

    [Fact]
    public static void Property() {
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

            public interface ITest : MyCode.IBase {}

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_OtherTypeMember() {
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

            public interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_OtherReturnType() {
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

            public interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_OtherAccessor() {
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

            public interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Property_OtherStatic() {
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

            public interface ITest : MyCode.IBase {
                int TestProperty { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    #endregion


    #region Indexer

    [Fact]
    public static void Indexer() {
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

            public interface ITest : MyCode.IBase {}

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_OtherTypeMember() {
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

            public interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_OtherReturnType() {
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

            public interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_OtherAccessor() {
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

            public interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Indexer_OtherParameters() {
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

            public interface ITest : MyCode.IBase {
                int this[int i] { get; }
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    #endregion


    #region EventField

    [Fact]
    public static void EventField() {
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

            public interface ITest : MyCode.IBase {}

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventField_OtherType() {
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

            public interface ITest : MyCode.IBase {
                event Action eventTest;
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventField_OtherStatic() {
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

            public interface ITest : MyCode.IBase {
                event Action eventTest;
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    #endregion


    #region EventProperty

    [Fact]
    public static void EventProperty() {
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

            public interface ITest : MyCode.IBase {}

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventProperty_OtherType() {
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

            public interface ITest : MyCode.IBase {
                event Action EventTest;
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void EventProperty_OtherStatic() {
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

            public interface ITest : MyCode.IBase {
                event Action EventTest;
            }

            """;
        Assert.Equal(expected, sourceText);
    }

    #endregion
}
