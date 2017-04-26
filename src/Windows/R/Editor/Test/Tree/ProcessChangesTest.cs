// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class ProcessChangesTest {
        private readonly IServiceContainer _services;

        public ProcessChangesTest(IServiceContainer services) {
            _services = services;
        }

        [Test]
        public void ProcessChange_EditExpression01() {
            string expression = "if(true) x <- 1";
            string expected1 =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [true]
            Variable  [true]
        TokenNode  [) [7...8)]
        SimpleScope  [9...15)
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [11...13)]
                        Variable  [x]
                        TokenNode  [<- [11...13)]
                        NumericalValue  [1 [14...15)]
";
            ParserTest.VerifyParse(expected1, expression);

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 3, 4, 5, "false"))
            {
                tree.IsDirty.Should().BeTrue();
                tree.ProcessChanges();

                string expected2 =
    @"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [false]
            Variable  [false]
        TokenNode  [) [8...9)]
        SimpleScope  [10...16)
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [12...14)]
                        Variable  [x]
                        TokenNode  [<- [12...14)]
                        NumericalValue  [1 [15...16)]
";
                ParserTest.CompareTrees(expected2, tree.AstRoot);
            }
        }

        [Test]
        public void ProcessChange_EditIfElse01() {
            string expression = "if(true) x <- 1 else x <- 2";
            string expected1 =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [true]
            Variable  [true]
        TokenNode  [) [7...8)]
        SimpleScope  [9...15)
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [11...13)]
                        Variable  [x]
                        TokenNode  [<- [11...13)]
                        NumericalValue  [1 [14...15)]
        KeywordScopeStatement  []
            TokenNode  [else [16...20)]
            SimpleScope  [21...27)
                ExpressionStatement  [x <- 2]
                    Expression  [x <- 2]
                        TokenOperator  [<- [23...25)]
                            Variable  [x]
                            TokenNode  [<- [23...25)]
                            NumericalValue  [2 [26...27)]
";
            ParserTest.VerifyParse(expected1, expression);

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 15, 0, 1, "\n"))
            {
                tree.IsDirty.Should().BeTrue();
                tree.ProcessChanges();

                string expected2 =
    @"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [true]
            Variable  [true]
        TokenNode  [) [7...8)]
        SimpleScope  [9...15)
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [11...13)]
                        Variable  [x]
                        TokenNode  [<- [11...13)]
                        NumericalValue  [1 [14...15)]
    ExpressionStatement  [x <- 2]
        Expression  [x <- 2]
            TokenOperator  [<- [24...26)]
                Variable  [x]
                TokenNode  [<- [24...26)]
                NumericalValue  [2 [27...28)]

UnexpectedToken Token [17...21)
";
                ParserTest.CompareTrees(expected2, tree.AstRoot);
            }
        }

        [Test]
        public void ProcessChange_EditIfElse02() {
            string expression = "if(true) {x <- 1} else x <- 2";
            string expected1 =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [true]
            Variable  [true]
        TokenNode  [) [7...8)]
        Scope  []
            TokenNode  [{ [9...10)]
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [12...14)]
                        Variable  [x]
                        TokenNode  [<- [12...14)]
                        NumericalValue  [1 [15...16)]
            TokenNode  [} [16...17)]
        KeywordScopeStatement  []
            TokenNode  [else [18...22)]
            SimpleScope  [23...29)
                ExpressionStatement  [x <- 2]
                    Expression  [x <- 2]
                        TokenOperator  [<- [25...27)]
                            Variable  [x]
                            TokenNode  [<- [25...27)]
                            NumericalValue  [2 [28...29)]
";
            ParserTest.VerifyParse(expected1, expression);

            using (var tree = EditorTreeTest.ApplyTextChange(_services, expression, 17, 0, 1, "\n")) { 
                tree.IsDirty.Should().BeTrue();
                tree.ProcessChanges();

                string expected2 =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [true]
            Variable  [true]
        TokenNode  [) [7...8)]
        Scope  []
            TokenNode  [{ [9...10)]
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [12...14)]
                        Variable  [x]
                        TokenNode  [<- [12...14)]
                        NumericalValue  [1 [15...16)]
            TokenNode  [} [16...17)]
    ExpressionStatement  [x <- 2]
        Expression  [x <- 2]
            TokenOperator  [<- [26...28)]
                Variable  [x]
                TokenNode  [<- [26...28)]
                NumericalValue  [2 [29...30)]

UnexpectedToken Token [19...23)
";
                ParserTest.CompareTrees(expected2, tree.AstRoot);
            }
        }
    }
}
