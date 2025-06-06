﻿namespace AutoInterface.Tests;

public sealed class SummaryTests {
    [Test]
    public async ValueTask Summary() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            /// <summary>
            /// my description
            /// </summary>
            [AutoInterface]
            public class Test { }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            /// <summary>
            /// my description
            /// </summary>
            public partial interface ITest {}

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Summary_Method() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                /// <summary>
                /// my description
                /// </summary>
                /// <param name="parameter1"></param>
                /// <param name="parameter2"></param>
                /// <returns></returns>
                /// <remarks></remarks>
                /// <exception cref=""></exception>
                public int SomeMethod(int parameter1, string parameter2) => 1;
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
                /// <param name="parameter1"></param>
                /// <param name="parameter2"></param>
                /// <returns></returns>
                /// <remarks></remarks>
                /// <exception cref=""></exception>
                int SomeMethod(int parameter1, string parameter2);
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask Summary_PreProcessorDirective() {
        const string input = """
            using AutoInterfaceAttributes;

            namespace MyCode;

            [AutoInterface]
            public class Test {
                #region
                public void SomeMethod() { }
                #endregion

                #region
                public int SomeProperty { get; init; }
                #endregion

                #region
                public int this[int i] => i;
                #endregion

                #region
                public event Action? someEvent;
                #endregion

                #region
                public event Action SomeEvent { add { } remove { } }
                #endregion
            }

            """;
        string sourceText = input.GenerateSourceText(out _, out _)[^1];

        const string expected = $$"""
            {{Shared.GENERATED_SOURCE_HEAD}}

            namespace MyCode;

            public partial interface ITest {
                void SomeMethod();

                int SomeProperty { get; init; }

                int this[int i] { get; }

                event Action? someEvent;

                event Action SomeEvent;
            }

            """;
        await Assert.That(sourceText).IsEqualTo(expected);
    }
}
