// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core.Logging;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Options {
    [ExcludeFromCodeCoverage]
    [Category.R.Settings]
    [Collection(CollectionNames.NonParallel)]   // Since they call VsAppShell and that causes composition
    public class LogVerbosityConverterTest {
        private static readonly string[] _permittedSettings = {
            Resources.LoggingLevel_None,
            Resources.LoggingLevel_Minimal,
            Resources.LoggingLevel_Normal,
            Resources.LoggingLevel_Traffic
        };

        private readonly Array _enumValues;

        public LogVerbosityConverterTest() {
            _enumValues = Enum.GetValues(typeof(LogVerbosity));
        }

        [Test]
        public void Defaults() {
            var converter = new LogVerbosityTypeConverter();
            converter.GetStandardValuesSupported().Should().BeTrue();
            converter.GetStandardValuesExclusive().Should().BeTrue();
            converter.GetStandardValues().Should().HaveCount(4).And.ContainInOrder(_permittedSettings);

            converter.CanConvertFrom(typeof(string)).Should().BeTrue();
            converter.CanConvertFrom(typeof(LogVerbosity)).Should().BeTrue();
            converter.CanConvertFrom(typeof(double)).Should().BeFalse();

            converter.CanConvertTo(typeof(string)).Should().BeTrue();
            converter.CanConvertTo(typeof(LogVerbosity)).Should().BeTrue();
            converter.CanConvertTo(typeof(double)).Should().BeFalse();
        }

        [Test]
        public void ConvertFrom() {
            var converter = new LogVerbosityTypeConverter();
            for (int i = 0; i < _enumValues.Length; i++) {
                var fromValue = _permittedSettings[i];
                var toValue = (LogVerbosity)_enumValues.GetValue(i);
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, fromValue).Should().Be(toValue);
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, toValue).Should().Be(toValue);
            }
        }

        [Test]
        public void ConvertTo() {
            var converter = new LogVerbosityTypeConverter();
            for (int i = 0; i < _enumValues.Length; i++) {
                var toValue = _permittedSettings[i];
                var fromValue = (LogVerbosity)_enumValues.GetValue(i);
                converter.ConvertTo(null, CultureInfo.InvariantCulture, fromValue, typeof(string)).Should().Be(toValue);
                converter.ConvertTo(null, CultureInfo.InvariantCulture, fromValue, typeof(LogVerbosity)).Should().Be(fromValue);
            }
        }

        [Test]
        public void MinMaxTest() {
            for (int i = 0; i < _enumValues.Length; i++) {
                var v = (LogVerbosity)_enumValues.GetValue(i);
                var converter = new LogVerbosityTypeConverter(v);
                var stdValues = converter.GetStandardValues();
                stdValues.Should().HaveCount(i + 1).And.ContainInOrder(_permittedSettings.Take(i + 1));
            }
        }
    }
}
