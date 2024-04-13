using Xunit;

namespace AutoInterface.Tests;

public static class AttributeTests {
    [Theory]
    [InlineData("Example")]
    [InlineData("asdf")]
    [InlineData("TestInterface")]
    [InlineData("WUWU")]
    public static void Name(string name) {
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
        Assert.Equal(expected, sourceText);
    }


    [Theory]
    [InlineData("internal")]
    [InlineData("public partial")]
    [InlineData("internal partial")]
    public static void Modifier(string modifier) {
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
        Assert.Equal(expected, sourceText);
    }


    [Theory]
    [InlineData("MySpace")]
    [InlineData("System.Generics")]
    [InlineData("BLAHBLAH.BLAH")]
    public static void Namespace(string namspace) {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Namespace_Empty() {
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
        Assert.Equal(expected, sourceText);
    }


    [Theory]
    [InlineData("[]", "")]
    [InlineData("[typeof(ICore)]", ": MyCode.ICore ")]
    [InlineData("[typeof(A), typeof(B), typeof(C)]", ": MyCode.A, MyCode.B, MyCode.C ")]
    [InlineData("new[] { }", "")]
    [InlineData("new[] { typeof(ICore) }", ": MyCode.ICore ")]
    [InlineData("new[] { typeof(A), typeof(B), typeof(C) }", ": MyCode.A, MyCode.B, MyCode.C ")]
    //[InlineData("new Type[] { }", "")]
    //[InlineData("new Type[] { typeof(ICore) }", ": MyCode.ICore ")]
    //[InlineData("new Type[] { typeof(A), typeof(B), typeof(C) }", ": MyCode.A, MyCode.B, MyCode.C ")]
    public static void Inheritance(string inheritance, string result) {
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
        Assert.Equal(expected, sourceText);
    }


    [Fact]
    public static void Nested() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void Nested_Triple() {
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
        Assert.Equal(expected, sourceText);
    }

    [Fact]
    public static void NestedWithMembers() {
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
        Assert.Equal(expected, sourceText);
    }


    [Fact]
    public static void StaticMembers() {
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
        Assert.Equal(expected, sourceText);
    }
}
