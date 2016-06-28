// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using static System.FormattableString;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSettingsStorage : IConfigurationSettingsStorage {
        public ConfigurationSettingsCollection Load(Stream stream) {
            throw new NotImplementedException();
        }

        public void Save(ConfigurationSettingsCollection settings, Stream stream) {
            var sw = new StreamWriter(stream);
            try {
                WriteHeader(sw);
                foreach (var s in settings) {
                    var v = s.Value.ToString();
                    if (!string.IsNullOrEmpty(v)) {
                        WriteAttributes(s, sw);
                        sw.WriteLine(Invariant($"{s.Name} <- {v}"));
                        sw.WriteLine(string.Empty);
                    }
                }
            } finally {
                sw.Close();
            }
        }

        private void WriteHeader(StreamWriter sw) {
            sw.WriteLine("# Application settings file.");
            sw.WriteLine(string.Format(CultureInfo.CurrentCulture, "File content was generated on {0}", DateTime.Now));
            sw.WriteLine(string.Empty);
        }

        private void WriteAttributes(IConfigurationSetting s, StreamWriter sw) {
            if (!string.IsNullOrEmpty(s.Category)) {
                sw.WriteLine(Invariant($"# [Category] {s.Category}"));
            }
            if (!string.IsNullOrEmpty(s.Description)) {
                sw.WriteLine(Invariant($"# [Description] {s.Description}"));
            }
            if (!string.IsNullOrEmpty(s.Editor)) {
                sw.WriteLine(Invariant($"# [Editor] {s.Editor}"));
            }
        }

        private bool ParseNameAndValue(string line, out string name, out string value) {
            name = value = null;
            var ast = RParser.Parse(line);
            if (ast.Errors.Count == 0 && ast.Children.Count > 0) {
                var scope = ast.Children[0] as GlobalScope;
                if (scope?.Children.Count > 0) {
                    var es = scope.Children[0] as IExpressionStatement;
                    var exp = es?.Expression;
                    if (exp?.Children.Count == 1) {
                        var op = exp.Children[0] as IOperator;
                        if (op?.Children.Count == 2) {
                            name = (op.Children[0] as Variable)?.Name;
                            value = line.Substring(op.Children[1].Start, op.Children[1].Length);
                            return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value);
                        }
                    }
                }
            }
            return false;
        }
    }
}
