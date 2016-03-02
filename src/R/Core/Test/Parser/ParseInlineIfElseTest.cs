// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]    
    public class ParseInlineIfElseTest {
        [Test]
        [Category.R.Parser]
        public void ParseInlineIfElseTest01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- b + if(x > 1) 1 else 2]
        Expression  [a <- b + if(x > 1) 1 else 2]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [+ [7...8)]
                    Variable  [b]
                    TokenNode  [+ [7...8)]
                    InlineIf  []
                        TokenNode  [if [9...11)]
                        TokenNode  [( [11...12)]
                        Expression  [x > 1]
                            TokenOperator  [> [14...15)]
                                Variable  [x]
                                TokenNode  [> [14...15)]
                                NumericalValue  [1 [16...17)]
                        TokenNode  [) [17...18)]
                        SimpleScope  [19...20)
                            ExpressionStatement  [1]
                                Expression  [1]
                                    NumericalValue  [1 [19...20)]
                        KeywordScopeStatement  []
                            TokenNode  [else [21...25)]
                            SimpleScope  [26...27)
                                ExpressionStatement  [2]
                                    Expression  [2]
                                        NumericalValue  [2 [26...27)]
";
            ParserTest.VerifyParse(expected, "a <- b + if(x > 1) 1 else 2");
        }

        [Test]
        [Category.R.Parser]
        public void ParseInlineIfElseTest02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- b + if(x > 1) {1+3} else {4-5}]
        Expression  [a <- b + if(x > 1) {1+3} else {4-5}]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [+ [7...8)]
                    Variable  [b]
                    TokenNode  [+ [7...8)]
                    InlineIf  []
                        TokenNode  [if [9...11)]
                        TokenNode  [( [11...12)]
                        Expression  [x > 1]
                            TokenOperator  [> [14...15)]
                                Variable  [x]
                                TokenNode  [> [14...15)]
                                NumericalValue  [1 [16...17)]
                        TokenNode  [) [17...18)]
                        Scope  []
                            TokenNode  [{ [19...20)]
                            ExpressionStatement  [1+3]
                                Expression  [1+3]
                                    TokenOperator  [+ [21...22)]
                                        NumericalValue  [1 [20...21)]
                                        TokenNode  [+ [21...22)]
                                        NumericalValue  [3 [22...23)]
                            TokenNode  [} [23...24)]
                        KeywordScopeStatement  []
                            TokenNode  [else [25...29)]
                            Scope  []
                                TokenNode  [{ [30...31)]
                                ExpressionStatement  [4-5]
                                    Expression  [4-5]
                                        TokenOperator  [- [32...33)]
                                            NumericalValue  [4 [31...32)]
                                            TokenNode  [- [32...33)]
                                            NumericalValue  [5 [33...34)]
                                TokenNode  [} [34...35)]
";
            ParserTest.VerifyParse(expected, "a <- b + if(x > 1) {1+3} else {4-5}");
        }

        [Test]
        [Category.R.Parser]
        public void ParseInlineIfElseTest03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- b + if(x > 1) { function(a) { return(x) }}]
        Expression  [a <- b + if(x > 1) { function(a) { return(x) }}]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                TokenOperator  [+ [7...8)]
                    Variable  [b]
                    TokenNode  [+ [7...8)]
                    InlineIf  []
                        TokenNode  [if [9...11)]
                        TokenNode  [( [11...12)]
                        Expression  [x > 1]
                            TokenOperator  [> [14...15)]
                                Variable  [x]
                                TokenNode  [> [14...15)]
                                NumericalValue  [1 [16...17)]
                        TokenNode  [) [17...18)]
                        Scope  []
                            TokenNode  [{ [19...20)]
                            FunctionStatement  [21...46)
                                TokenNode  [function [21...29)]
                                TokenNode  [( [29...30)]
                                ArgumentList  [30...31)
                                    ExpressionArgument  [30...31)
                                        Expression  [a]
                                            Variable  [a]
                                TokenNode  [) [31...32)]
                                Scope  []
                                    TokenNode  [{ [33...34)]
                                    ExpressionStatement  [return(x)]
                                        Expression  [return(x)]
                                            FunctionCall  [35...44)
                                                Variable  [return]
                                                TokenNode  [( [41...42)]
                                                ArgumentList  [42...43)
                                                    ExpressionArgument  [42...43)
                                                        Expression  [x]
                                                            Variable  [x]
                                                TokenNode  [) [43...44)]
                                    TokenNode  [} [45...46)]
                            TokenNode  [} [46...47)]
";
            ParserTest.VerifyParse(expected, "a <- b + if(x > 1) { function(a) { return(x) }}");
        }

        [Test]
        [Category.R.Parser]
        public void ParseInlineIfElseTest04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- b(c, if(TRUE) 1 else 2, d = if(x) 3 else 4)]
        Expression  [a <- b(c, if(TRUE) 1 else 2, d = if(x) 3 else 4)]
            TokenOperator  [<- [2...4)]
                Variable  [a]
                TokenNode  [<- [2...4)]
                FunctionCall  [5...48)
                    Variable  [b]
                    TokenNode  [( [6...7)]
                    ArgumentList  [7...47)
                        ExpressionArgument  [7...9)
                            Expression  [c]
                                Variable  [c]
                            TokenNode  [, [8...9)]
                        ExpressionArgument  [10...28)
                            Expression  [if(TRUE) 1 else 2]
                                InlineIf  []
                                    TokenNode  [if [10...12)]
                                    TokenNode  [( [12...13)]
                                    Expression  [TRUE]
                                        LogicalValue  [TRUE [13...17)]
                                    TokenNode  [) [17...18)]
                                    SimpleScope  [19...20)
                                        ExpressionStatement  [1]
                                            Expression  [1]
                                                NumericalValue  [1 [19...20)]
                                    KeywordScopeStatement  []
                                        TokenNode  [else [21...25)]
                                        SimpleScope  [26...27)
                                            ExpressionStatement  [2]
                                                Expression  [2]
                                                    NumericalValue  [2 [26...27)]
                            TokenNode  [, [27...28)]
                        NamedArgument  [29...47)
                            TokenNode  [d [29...30)]
                            TokenNode  [= [31...32)]
                            Expression  [if(x) 3 else 4]
                                InlineIf  []
                                    TokenNode  [if [33...35)]
                                    TokenNode  [( [35...36)]
                                    Expression  [x]
                                        Variable  [x]
                                    TokenNode  [) [37...38)]
                                    SimpleScope  [39...40)
                                        ExpressionStatement  [3]
                                            Expression  [3]
                                                NumericalValue  [3 [39...40)]
                                    KeywordScopeStatement  []
                                        TokenNode  [else [41...45)]
                                        SimpleScope  [46...47)
                                            ExpressionStatement  [4]
                                                Expression  [4]
                                                    NumericalValue  [4 [46...47)]
                    TokenNode  [) [47...48)]
";
            ParserTest.VerifyParse(expected, "a <- b(c, if(TRUE) 1 else 2, d = if(x) 3 else 4)");
        }
    }
}
