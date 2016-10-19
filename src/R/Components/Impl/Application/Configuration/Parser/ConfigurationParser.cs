// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.R.Components.Application.Configuration.Parser {
    internal sealed class ConfigurationParser {
        private readonly List<ConfigurationError> _errors = new List<ConfigurationError>();
        private readonly StreamReader _sr;
        private string _bufferedLine;
        private int _lineNumber;

        public IReadOnlyList<ConfigurationError> Errors => _errors;

        public ConfigurationParser(StreamReader sr) {
            _sr = sr;
        }

        public IConfigurationSetting ReadSetting() {
            var setting = new ConfigurationSetting();
            while (!IsEndOfStream()) {
                var line = ReadLine();
                if (!string.IsNullOrWhiteSpace(line)) {
                    if (line.TrimStart()[0] == '#') {
                        ReadAttributeValue(line.Substring(1), setting);
                    } else {
                        // Expression can be multi-line so read all lines up to the next comment
                        int startingLineNumber = _lineNumber;
                        var text = line + ReadRemainingExpressionText();
                        if (ParseSetting(text, startingLineNumber, setting)) {
                            if (IsEnvNew(setting)) {
                                // Skip, as it's not a real setting, it's the creation of the environment
                                setting = new ConfigurationSetting();
                                continue;
                            }
                            return setting;
                        }
                        break;
                    }
                }
            }
            return null;
        }

        private bool IsEndOfStream() {
            return _bufferedLine == null && _sr.EndOfStream;
        }
        private string ReadLine() {
            var line = _bufferedLine ?? _sr.ReadLine();
            _lineNumber++;
            _bufferedLine = null;
            return line;
        }

        private void UnreadLine(string line) {
            Debug.Assert(_bufferedLine == null);
            _bufferedLine = line;
            _lineNumber--;
        }

        private string ReadRemainingExpressionText() {
            var sb = new StringBuilder();
            while (!IsEndOfStream()) {
                var line = ReadLine();
                if (string.IsNullOrWhiteSpace(line)) {
                    break;
                } else if (line.Length > 0 && line.TrimStart()[0] == '#') {
                    UnreadLine(line);
                    break;
                }
                sb.Append(Environment.NewLine);
                sb.Append(line);
            }
            return sb.ToString();
        }

        private bool ParseSetting(string text, int lineNumber, ConfigurationSetting s) {
            if (string.IsNullOrWhiteSpace(text)) {
                return false;
            }

            IRValueNode leftOperand;
            IRValueNode rightOperand;
            if (ParseAssignment(text, out leftOperand, out rightOperand)) {
                var listOp = leftOperand as IOperator;
                if (listOp != null) {
                    // Look for assignment on settings environment:
                    //   settings$name1 <- "value1"
                    //   settings$name1 <- expr1
                    if (listOp.OperatorType == OperatorType.ListIndex && listOp.LeftOperand != null && listOp.RightOperand != null) {
                        var value = text.Substring(rightOperand.Start, rightOperand.Length);
                        var listName = (listOp.LeftOperand as Variable)?.Name;
                        var settingsName = (listOp.RightOperand as Variable)?.Name;
                        var result = !string.IsNullOrEmpty(settingsName) && !string.IsNullOrEmpty(value);
                        if (result && listName == "settings") {
                            try {
                                s.Name = settingsName;
                                s.ValueType = value[0] == '\'' || value[0] == '\"' ? ConfigurationSettingValueType.String : ConfigurationSettingValueType.Expression;
                                s.Value = s.ValueType == ConfigurationSettingValueType.String ? value.FromRStringLiteral() : value;
                                return true;
                            } catch (FormatException) {
                            }
                        }
                    }
                } else {
                    // Look for assignment with no environment
                    // (backwards compat with RTVS 0.5 + creation of settings environment):
                    //   name1 <- "value1"
                    //   name1 <- expr1
                    var name = (leftOperand as Variable)?.Name;
                    var value = text.Substring(rightOperand.Start, rightOperand.Length);
                    var result = !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value);
                    if (result) {
                        try {
                            s.Name = name;
                            s.ValueType = value[0] == '\'' || value[0] == '\"' ? ConfigurationSettingValueType.String : ConfigurationSettingValueType.Expression;
                            s.Value = s.ValueType == ConfigurationSettingValueType.String ? value.FromRStringLiteral() : value;
                            return true;
                        } catch (FormatException) {
                        }
                    }
                }
            }
            _errors.Add(new ConfigurationError(lineNumber, Resources.ConfigurationError_Syntax));
            return false;
        }

        private bool ParseAssignment(string text, out IRValueNode leftOperand, out IRValueNode rightOperand) {
            leftOperand = null;
            rightOperand = null;

            // Parse the expression
            var ast = RParser.Parse(text);
            if (ast.Errors.Count == 0) {
                // Expected 'Variable <- Expression'
                var scope = ast.Children[0] as GlobalScope;
                if (scope?.Children.Count > 0) {
                    var exp = (scope.Children[0] as IExpressionStatement)?.Expression;
                    if (exp?.Children.Count == 1) {
                        var op = exp.Children[0] as IOperator;
                        if (op != null) {
                            if (op.OperatorType == OperatorType.LeftAssign && op.LeftOperand != null && op.RightOperand != null) {
                                leftOperand = op.LeftOperand;
                                rightOperand = op.RightOperand;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool IsEnvNew(ConfigurationSetting s) {
            return s.Name == "settings" && s.Value == "as.environment(list())" && s.ValueType == ConfigurationSettingValueType.Expression;
        }

        private bool ReadAttributeValue(string line, ConfigurationSetting s) {
            string attributeName;
            line = line.TrimStart();
            if (line.Length > 0 && line[0] == '[') {
                var closeBraceIndex = line.IndexOf(']');
                if (closeBraceIndex >= 0) {
                    attributeName = line.Substring(1, closeBraceIndex - 1);
                    var value = line.Substring(closeBraceIndex + 1).Trim();
                    if (attributeName.EqualsOrdinal(ConfigurationSettingAttributeNames.Category)) {
                        s.Category = value;
                        return true;
                    } else if (attributeName.EqualsOrdinal(ConfigurationSettingAttributeNames.Description)) {
                        s.Description = value;
                        return true;
                    } else if (attributeName.EqualsOrdinal(ConfigurationSettingAttributeNames.Editor)) {
                        s.EditorType = value;
                        return true;
                    }
                }
            }
            return false;
        }

        public static string GetPersistentKey(string attribute) {
            return Invariant($"[{attribute}]");
        }
    }
}
