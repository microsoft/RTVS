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
        [InlineData("UPPERCASE <- 1", 0, 9, "Lint_Uppercase", "UpperCase")]
        [InlineData("camelCase <- 1", 0, 9, "Lint_CamelCase", "CamelCase")]
        [InlineData("snake_case <- 1", 0, 10, "Lint_SnakeCase", "SnakeCase")]
        [InlineData("PascalCase <- 1", 0, 10, "Lint_PascalCase", "PascalCase")]
        [InlineData("mul.tiple.dots <- 1", 0, 14, "Lint_MultileDots", "MultipleDots")]
        public async Task Validate(string content, int start, int length, string message, string propertyName) {

            var prop = propertyName != null ?_options.GetType().GetProperty(propertyName) : null;
            prop?.SetValue(_options, true);

            var ast = RParser.Parse(content);
            await _validator.RunAsync(ast, _results, CancellationToken.None);
            _results.Should().HaveCount(length > 0 ? 1 : 0);

            if (length > 0) {
                _results.TryPeek(out IValidationError e);
                e.Start.Should().Be(start);
                e.Length.Should().Be(length);
                e.Message.Should().Be(Resources.ResourceManager.GetString(message));
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
