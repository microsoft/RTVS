// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Validation;
using Microsoft.R.Editor.Validation.Errors;
using Microsoft.R.Editor.Validation.Lint;
using Microsoft.UnitTests.Core.XUnit;
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
        [InlineData("if (TRUE) {\n  x <- 1\n} else {\n", 0, 0, null, null)]
        [InlineData("x=1", 1, 1, "Lint_Assignment", "AssignmentType")]
        [InlineData("1:10", 0, 0, null, null)]
        [InlineData("a::b", 0, 0, null, null)]
        [InlineData("a:::b", 0, 0, null, null)]
        public async Task Validate(string content, int start, int length, string message, string propertyName) {

            var prop = propertyName != null ? _options.GetType().GetProperty(propertyName) : null;
            prop?.SetValue(_options, true);

            var ast = RParser.Parse(content);
            await _validator.RunAsync(ast, false, _results, CancellationToken.None);
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
                await _validator.RunAsync(ast, false, r, CancellationToken.None);
                r.Should().BeEmpty();
            }
        }

        [CompositeTest]
        [InlineData("x[1,]", new[] { 3, 4 }, new[] { 1, 1 }, new[] { "Lint_CommaSpaces", "Lint_NoSpaceBetweenCommaAndClosingBracket" })]
        [InlineData("x[[1,]]", new[] { 4, 5 }, new[] { 1, 2 }, new[] { "Lint_CommaSpaces", "Lint_NoSpaceBetweenCommaAndClosingBracket" })]
        [InlineData("if (TRUE) { x <- 1 }", new[] { 10, 19 }, new[] { 1, 1 }, new[] { "Lint_OpenCurlyPosition", "Lint_CloseCurlySeparateLine" })]
        [InlineData("x <- 1;y <- 2", new [] { 6, 7 }, new [] { 1, 6 }, new [] {"Lint_Semicolons", "Lint_MultipleStatementsInLine", })]
        public async Task Validate2(string content, int[] start, int[] length, string[] message) {

            var ast = RParser.Parse(content);
            await _validator.RunAsync(ast, false, _results, CancellationToken.None);
            _results.Should().HaveCount(start.Length);

            for (var i = 0; i < start.Length; i++) {
                _results.TryDequeue(out IValidationError e);
                e.Start.Should().Be(start[i]);
                e.Length.Should().Be(length[i]);
                e.Message.Should().Be(Resources.ResourceManager.GetString(message[i]));
                e.Severity.Should().Be(ErrorSeverity.Warning);
            }
        }

        [CompositeTest]
        [InlineData("x <- \"012345678901234567890123456789\"", 20, 0, 37, "Lint_LineTooLong", "LineLength", "MaxLineLength")]
        [InlineData("x <- 1\nx <- \"012345678901234567890123456789\"\ny <- 3", 20, 7, 37, "Lint_LineTooLong", "LineLength", "MaxLineLength")]
        [InlineData(" abcdefghijklmnopqrstuvwxyz <- 1", 20, 1, 26, "Lint_NameTooLong", "NameLength", "MaxNameLength")]
        public async Task LengthLimit(string content, int maxLength, int start, int length, string message, string propertyName, string propertyLimitName) {

            var prop = propertyName != null ? _options.GetType().GetProperty(propertyName) : null;
            prop?.SetValue(_options, true);
            var propLimit = propertyLimitName != null ? _options.GetType().GetProperty(propertyLimitName) : null;
            propLimit?.SetValue(_options, maxLength);

            var ast = RParser.Parse(content);
            await _validator.RunAsync(ast, false, _results, CancellationToken.None);
            _results.Should().HaveCount(1);

            _results.TryPeek(out IValidationError e);
            e.Start.Should().Be(start);
            e.Length.Should().Be(length);
            var m = Resources.ResourceManager.GetString(message).FormatInvariant(e.Length, maxLength);
            e.Message.Should().Be(m);
            e.Severity.Should().Be(ErrorSeverity.Warning);

            if (prop != null) {
                prop.SetValue(_options, false);
                var r = new ConcurrentQueue<IValidationError>();
                await _validator.RunAsync(ast, false, r, CancellationToken.None);
                r.Should().BeEmpty();
            }
        }

        [CompositeTest]
        [InlineData("x <- 1\n\n", 0, 0, null, "TrailingBlankLines")]
        [InlineData("{r x = 1, y = 2}", 0, 0, null, "OpenCurlyPosition")]
        [InlineData("{ r x = 1, y = 2}", 0, 0, null, "OpenCurlyPosition")]
        [InlineData("{r x = 1, y = 2}", 0, 0, null, "CloseCurlySeparateLine")]
        [InlineData("{ r x = 1, y = 2}", 0, 0, null, "CloseCurlySeparateLine")]
        public async Task Projected(string content, int start, int length, string message, string propertyName) {

            var prop = propertyName != null ? _options.GetType().GetProperty(propertyName) : null;
            prop?.SetValue(_options, true);

            var ast = RParser.Parse(content);
            await _validator.RunAsync(ast, true, _results, CancellationToken.None);
            _results.Should().BeEmpty();
        }
    }
}
