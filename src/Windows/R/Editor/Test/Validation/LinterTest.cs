// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Roxygen;
using Microsoft.R.Editor.Test.Completions;
using Microsoft.R.Editor.Validation;
using Microsoft.R.Editor.Validation.Errors;
using Microsoft.R.Editor.Validation.Lint;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Test.Roxygen {
    [ExcludeFromCodeCoverage]
    [Category.R.Linter]
    public class LinterTest {
        private readonly ConcurrentQueue<IValidationError> _results = new ConcurrentQueue<IValidationError>();
        private readonly IServiceContainer _services;
        private readonly IValidatorAggregator _validator;
        private readonly LintOptions _options;

        public LinterTest(IServiceContainer services) {
            _services = services;
            _validator = new ValidatorAggregator(_services);
            _options = _services.GetService<IREditorSettings>().LintOptions;
            _options.Enabled = true;
        }


        [CompositeTest]
        [InlineData("", 0, 0, null, null)]
        [InlineData("x = 1", 2, 1, "Lint_Assignment", "AssignmentType")]
        [InlineData("f(x = 1)", 0, 0, null, null)]
        [InlineData("UPPERCASE <- 1", 0, 9, "Lint_Uppercase", "UpperCase")]
        [InlineData("camelCase <- 1", 0, 9, "Lint_CamelCase", "CamelCase")]
        [InlineData("snake_case <- 1", 0, 10, "Lint_SnakeCase", "SnakeCase")]
        [InlineData("PascalCase <- 1", 0, 10, "Lint_PascalCase", "PascalCase")]
        [InlineData("mul.tiple.dots <- 1", 0, 14, "Lint_MultileDots", "MultipleDots")]
        [InlineData(".\tx <- 1", 1, 1, "Lint_Tabs", "NoTabs")]
        [InlineData("x <- 1 ", 6, 1, "Lint_TrailingWhitespace", "TrailingWhitespace")]
        [InlineData("x <- 1\n\n", 7, 1, "Lint_TrailingBlankLines", "TrailingBlankLines")]
        [InlineData("x<- 1", 1, 2, "Lint_OperatorSpaces", "SpacesAroundOperators")]
        [InlineData("x <-1", 2, 2, "Lint_OperatorSpaces", "SpacesAroundOperators")]
        [InlineData("x <- !y", 0, 0, null, null)]
        [InlineData("x <- 1;", 6, 1, "Lint_Semicolons", "Semicolons")]
        [InlineData("x <- 1; # comment", 6, 1, "Lint_Semicolons", "Semicolons")]
        [InlineData("x <- 1; # comment\ny <- 2", 6, 1, "Lint_Semicolons", "Semicolons")]
        [InlineData("x <- 1;y <- 2", 7, 6, "Lint_MultipleStatementsInLine", "MultipleStatements")]
        [InlineData("f(,,,)", 0, 0, null, null)]
        [InlineData("x <- T", 5, 1, "Lint_TrueFalseNames", "TrueFalseNames")]
        [InlineData("x <- F", 5, 1, "Lint_TrueFalseNames", "TrueFalseNames")]
        [InlineData("f  (z)", 1, 2, "Lint_SpaceAfterFunctionName", "NoSpaceAfterFunctionName")]
        [InlineData("f(z)", 0, 0, null, null)]
        [InlineData("if()", 2, 1, "Lint_SpaceBeforeOpenBrace", "SpaceBeforeOpenBrace")]
        [InlineData("while (z)", 0, 0, null, null)]
        [InlineData("x(  a, b)", 2, 2, "Lint_SpaceAfterLeftParenthesis", "SpacesInsideParenthesis")]
        [InlineData("x(a, b   )", 6, 3, "Lint_SpaceBeforeClosingBrace", "SpacesInsideParenthesis")]
        [InlineData("x(\n  a,\n  b\n)", 0, 0, null, null)]
        [InlineData("x[1, ]", 0, 0, null, null)]
        [InlineData("x[1,, ]", 0, 0, null, null)]
        [InlineData("x <- 'str'", 5, 5, "Lint_DoubleQuotes", "DoubleQuotes")]
        [InlineData("x <- \"str\"", 0, 0, null, null)]
        [InlineData("if (TRUE)\n{", 10, 1, "Lint_OpenCurlyPosition", "OpenCurlyPosition")]
        [InlineData("if (TRUE)\n{\n", 10, 1, "Lint_OpenCurlyPosition", "OpenCurlyPosition")]
        [InlineData("if (TRUE) {\n}", 0, 0, null, null)]
        [InlineData("if (TRUE) {\n  x <- 1 }", 21, 1, "Lint_CloseCurlySeparateLine", "CloseCurlySeparateLine")]
        [InlineData("if (TRUE) {\n  x <- 1\n}", 0, 0, null, null)]
        [InlineData("if (TRUE) {\n  x <- 1\n} else {", 0, 0, null, null)]
        public async Task Validate(string content, int start, int length, string message, string propertyName) {

            var prop = propertyName != null ? _options.GetType().GetProperty(propertyName) : null;
            prop?.SetValue(_options, true);

            var ast = RParser.Parse(content);
            await _validator.RunAsync(ast, _results, CancellationToken.None);
            _results.Should().HaveCount(length > 0 ? 1 : 0);

            if (length > 0) {
                _results.TryPeek(out IValidationError e);
                e.Start.Should().Be(start);
                e.Length.Should().Be(length);
                e.Message.Should().Be(Resources.ResourceManager.GetString(message));
                e.Severity.Should().Be(ErrorSeverity.Warning);
            }

            if (prop != null) {
                prop.SetValue(_options, false);
                var r = new ConcurrentQueue<IValidationError>();
                await _validator.RunAsync(ast, r, CancellationToken.None);
                r.Should().BeEmpty();
            }
        }
        [CompositeTest]
        [InlineData("x[1,]", 2, 1, "Lint_NoSpaceBetweenCommaAndClosingBracket", "SpacesAroundComma")]
        [InlineData("x[[1,]]", 2, 1, "Lint_NoSpaceBetweenCommaAndClosingBracket", "SpacesAroundComma")]
        [InlineData("if (TRUE) { x <- 1 }", 19, 1, "Lint_CloseCurlySeparateLine", "CloseCurlySeparateLine")]
        public async Task Validate2(string content, int start, int length, string message, string propertyName) {

            var prop = propertyName != null ? _options.GetType().GetProperty(propertyName) : null;
            prop?.SetValue(_options, true);

            var ast = RParser.Parse(content);
            await _validator.RunAsync(ast, _results, CancellationToken.None);
            _results.Should().HaveCount(length > 0 ? 1 : 0);

            if (length > 0) {
                _results.TryPeek(out IValidationError e);
                e.Start.Should().Be(start);
                e.Length.Should().Be(length);
                e.Message.Should().Be(Resources.ResourceManager.GetString(message));
                e.Severity.Should().Be(ErrorSeverity.Warning);
            }

            if (prop != null) {
                prop.SetValue(_options, false);
                var r = new ConcurrentQueue<IValidationError>();
                await _validator.RunAsync(ast, r, CancellationToken.None);
                r.Should().BeEmpty();
            }
        }
    }
}
