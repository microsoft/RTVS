// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.RData.Parser;
using Microsoft.R.Editor.Test;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.RData.Test.Help {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class GetFunctionInfoTest {
        private readonly EditorTestFilesFixture _files;

        public GetFunctionInfoTest(EditorTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public void GetRdFunctionArgumentsTest01() {
            const string rdData = @"\alias{abind}
\usage{
    abind(..., along=N, rev.along=NULL, new.names='abc', force.array=TRUE,
      make.names=use.anon.names, use.anon.names=1.75*(2/3),
      use.first.dimnames=FALSE, hier.names=FALSE, use.dnns=
      FALSE)
}";
            var functionInfos = RdParser.GetFunctionInfos("package", rdData);
            functionInfos.Should().ContainSingle()
                .Which.Signatures.Should().ContainSingle()
                .Which.Arguments.ShouldBeEquivalentTo(new [] {
                    new { Name = "...", DefaultValue = (string)null },
                    new { Name = "along", DefaultValue = "N" },
                    new { Name = "rev.along", DefaultValue = "NULL" },
                    new { Name = "new.names", DefaultValue = @"'abc'" },
                    new { Name = "force.array", DefaultValue = "TRUE" },
                    new { Name = "make.names", DefaultValue = "use.anon.names" },
                    new { Name = "use.anon.names", DefaultValue = "1.75*(2/3)" },
                    new { Name = "use.first.dimnames", DefaultValue = "FALSE" },
                    new { Name = "hier.names", DefaultValue = "FALSE" },
                    new { Name = "use.dnns", DefaultValue = "FALSE" },
                }, o => o.ExcludingMissingMembers());
        }

        [Test]
        public void GetRdFunctionArgumentsTest02() {
            const string rdData = @"
\usage{
matrix(data = NA, nrow = 1, ncol = 1, byrow = FALSE,
       dimnames = NULL)

as.matrix(x, \dots)
\method{as.matrix}{data.frame}(x, rownames.force = NA, \dots)

is.matrix(x)
}";
            var functionInfos = RdParser.GetFunctionInfos("package", rdData);
            functionInfos.Should().Equal(new[] {"matrix", "as.matrix", "is.matrix"}, (a, e) => a.Name == e)
                .And.OnlyContain(fi => fi.Signatures.Count == 1, "there should be only one signature");

            var arguments = functionInfos[0].Signatures[0].Arguments;
            arguments.Should().HaveCount(5);
            arguments[1].Name.Should().Be("nrow");
            arguments[1].DefaultValue.Should().Be("1");
        }

        [Test]
        public void GetRdFunctionArgumentsTest03() {
            const string rdData = @"
\title{Parentheses and Braces}\name{Paren}\alias{Paren}\alias{(}\alias{{}\keyword{programming}\description{
  Open parenthesis, \code{(}, and open brace, \code{{}, are \code{\link{.Primitive}} functions in \R.
}\usage{
}";
            var functionInfos = RdParser.GetFunctionInfos("package", rdData);
            functionInfos.Should().BeEmpty();
        }

        [Test]
        public void GetRdFunctionArgumentsDescriptionsTest01() {
            const string rdData = @"\alias{abind}
\usage {
    abind(..., along=N, rev.along=NULL, new.names='abc')
}
\arguments{
  \item{\dots}{ Any number of vectors }

\item{along}{ (optional) The dimension along which to bind the arrays.
  }
}
";
            var functionInfos = RdParser.GetFunctionInfos("package", rdData);

            functionInfos.Should().ContainSingle()
                .Which.Signatures.Should().ContainSingle()
                .Which.Arguments.ShouldBeEquivalentTo(new[] {
                    new { Name = "...", Description = "Any number of vectors" },
                    new { Name = "along", Description = "(optional) The dimension along which to bind the arrays." },
                    new { Name = "rev.along", Description = string.Empty },
                    new { Name = "new.names", Description = string.Empty }
                }, o => o.ExcludingMissingMembers());
        }

        [Test]
        public void GetRdFunctionInfoTest01() {
            var rdData = _files.LoadDestinationFile(@"Help\01.rd");
            var functionInfos = RdParser.GetFunctionInfos("package", rdData);

            functionInfos.Should().HaveCount(2);

            var functionInfo = functionInfos[0];
            functionInfo.Name.Should().Be("abs");
            functionInfo.Description.Should().Be("abs(x) computes the absolute value of x, sqrt(x) computes the (principal) square root of x, x. The naming follows the standard for computer languages such as C or Fortran.");
            functionInfo.Signatures.Should().ContainSingle()
                .Which.Arguments.Should().ContainSingle()
                .Which.Description.Should().Be("a numeric or complex vector or array.");
        }

        [Test]
        public void GetRdFunctionInfoTest02() {
            var rdData = _files.LoadDestinationFile(@"Help\02.rd");
            var functionInfos = RdParser.GetFunctionInfos("package", rdData);
            functionInfos.Should().Equal(new[] {
                    "lockEnvironment",
                    "environmentIsLocked",
                    "lockBinding",
                    "unlockBinding",
                    "bindingIsLocked",
                    "makeActiveBinding",
                    "bindingIsActive",
                }, (a, e) => a.Name == e)
                .And.OnlyContain(f => f.Signatures.Count == 1)
                .And.OnlyContain(f => f.Description == "These functions represent an experimental interface for adjustments to environments and bindings within environments. They allow for locking environments as well as individual bindings, and for linking a variable to a function.")
                .And.Equal(new [] {
                    2,1,2,2,2,3,2
                }, (a, e) => a.Signatures[0].Arguments.Count == e);

            functionInfos[0].Signatures[0].Arguments[0].Description.Should().Be("an environment.");
            functionInfos[0].Signatures[0].Arguments[1].Description.Should().Be("logical specifying whether bindings should be locked.");
            functionInfos[2].Signatures[0].Arguments[0].Description.Should().Be("a name object or character string.");
            functionInfos[5].Signatures[0].Arguments[1].Description.Should().Be("a function taking zero or one arguments.");
        }

        [Test]
        public void GetRdFunctionArgumentsBadData01() {
            var functionInfos = RdParser.GetFunctionInfos(string.Empty, string.Empty);
            functionInfos.Should().BeEmpty();
        }
    }
}
