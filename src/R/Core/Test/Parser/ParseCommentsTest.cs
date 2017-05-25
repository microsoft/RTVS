// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseCommentsTest
    {
        [Test]
        [Category.R.Parser]
        public void ParseCommentsTest01()
        {
            AstRoot ast = RParser.Parse("#Not");

            ast.Comments.Should().ContainSingle();
            ast.Comments[0].Start.Should().Be(0);
            ast.Comments[0].Length.Should().Be(4);

            ast.Comments.Contains(0).Should().BeFalse();
            ast.Comments.Contains(1).Should().BeTrue();
            ast.Comments.Contains(4).Should().BeTrue();
        }
    }
}
