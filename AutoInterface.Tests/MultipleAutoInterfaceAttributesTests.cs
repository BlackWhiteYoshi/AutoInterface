using Xunit;

namespace AutoInterface.Tests;

public static class MultipleAutoInterfaceAttributesTests {
    [Fact]
    public static void TwoAutoInterfaceAttributes() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Name = "ITest1")]
            [AutoInterface(Name = "ITest2")]
            public class Test {
                public int GetNumber() => 1;
            }

            """;
        string[] sourceText = input.GenerateSourceText(out _, out _);
        sourceText = sourceText.Skip(sourceText.Length - 2).Take(2).ToArray();

        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITest1 {
                    int GetNumber();
                }

                """;
            Assert.Equal(expected, sourceText[0]);
        }
        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITest2 {
                    int GetNumber();
                }

                """;
            Assert.Equal(expected, sourceText[1]);
        }
    }

    [Fact]
    public static void TwoAutoInterfaceAttributes_Summary() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;


            /// <summary>
            /// my description
            /// </summary>
            [AutoInterface(Name = "ITest1")]
            [AutoInterface(Name = "ITest2")]
            public class Test {
                public int GetNumber() => 1;
            }

            """;
        string[] sourceText = input.GenerateSourceText(out _, out _);
        sourceText = sourceText.Skip(sourceText.Length - 2).Take(2).ToArray();

        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                /// <summary>
                /// my description
                /// </summary>
                public partial interface ITest1 {
                    int GetNumber();
                }

                """;
            Assert.Equal(expected, sourceText[0]);
        }
        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                /// <summary>
                /// my description
                /// </summary>
                public partial interface ITest2 {
                    int GetNumber();
                }

                """;
            Assert.Equal(expected, sourceText[1]);
        }
    }

    [Fact]
    public static void TwoAutoInterfaceAttributes_Explicit() {
        const string input = $$"""
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface(Name = "ITest1")]
            [AutoInterface(Name = "ITest2")]
            public class Test {
                int ITest1.GetNumber() => 1;

                string ITest2.GetString() => "";
            }

            """;
        string[] sourceText = input.GenerateSourceText(out _, out _);
        sourceText = sourceText.Skip(sourceText.Length - 2).Take(2).ToArray();

        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITest1 {
                    int GetNumber();
                }

                """;
            Assert.Equal(expected, sourceText[0]);
        }
        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITest2 {
                    string GetString();
                }

                """;
            Assert.Equal(expected, sourceText[1]);
        }
    }
}
