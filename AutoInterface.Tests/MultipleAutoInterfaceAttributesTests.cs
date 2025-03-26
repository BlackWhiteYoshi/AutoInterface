namespace AutoInterface.Tests;

public sealed class MultipleAutoInterfaceAttributesTests {
    [Test]
    public async ValueTask TwoAutoInterfaceAttributes() {
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
            await Assert.That(sourceText[0]).IsEqualTo(expected);
        }
        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITest2 {
                    int GetNumber();
                }

                """;
            await Assert.That(sourceText[1]).IsEqualTo(expected);
        }
    }

    [Test]
    public async ValueTask TwoAutoInterfaceAttributes_Summary() {
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
        sourceText = [.. sourceText.Skip(sourceText.Length - 2).Take(2)];

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
            await Assert.That(sourceText[0]).IsEqualTo(expected);
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
            await Assert.That(sourceText[1]).IsEqualTo(expected);
        }
    }

    [Test]
    public async ValueTask TwoAutoInterfaceAttributes_Explicit() {
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
        sourceText = [.. sourceText.Skip(sourceText.Length - 2).Take(2)];

        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITest1 {
                    int GetNumber();
                }

                """;
            await Assert.That(sourceText[0]).IsEqualTo(expected);
        }
        {
            string expected = $$"""
                {{Shared.GENERATED_SOURCE_HEAD}}

                namespace MyCode;

                public partial interface ITest2 {
                    string GetString();
                }

                """;
            await Assert.That(sourceText[1]).IsEqualTo(expected);
        }
    }
}
