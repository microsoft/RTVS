// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.DataInspection;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Host.Client;
using Microsoft.R.StackTracing;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Editor.Data {
    /// <summary>
    /// Provides name of variables and members declared in REPL workspace
    /// </summary>
    internal sealed class WorkspaceVariableProvider : RSessionChangeWatcher, IVariablesProvider {
        private static readonly char[] _selectors = { '$', '@' };
        private const int _maxWaitTime = 2000;
        private const int _maxResults = 100;

        private readonly IServiceContainer _services;

        /// <summary>
        /// Collection of top-level variables
        /// </summary>
        private readonly Dictionary<string, IRSessionDataObject> _topLevelVariables = new Dictionary<string, IRSessionDataObject>();
        private bool _updating;

        public WorkspaceVariableProvider(IServiceContainer services) :
            base(services.GetService<IRInteractiveWorkflowProvider>()) {
            _services = services;
        }

        #region IVariablesProvider
        /// <summary>
        /// Given variable name determines number of members
        /// </summary>
        /// <param name="variableName">Variable name or null if global scope</param>
        public int GetMemberCount(string variableName) {
            if (string.IsNullOrEmpty(variableName)) {
                // Global scope
                return _topLevelVariables.Values.Count;
            }

            // TODO: do estimate
            return _maxResults;
        }

        /// <summary>
        /// Given variable name returns variable members
        /// adhering to specified criteria. Last member name
        /// may be partial such as abc$def$g
        /// </summary>
        /// <param name="variableName">
        /// Variable name such as abc$def$g. 'g' may be partially typed
        /// in which case providers returns members of 'def' filtered to 'g' prefix.
        /// </param>
        /// <param name="maxCount">Max number of members to return</param>
        public IReadOnlyCollection<INamedItemInfo> GetMembers(string variableName, int maxCount) {
            try {
                // Split abc$def$g into parts. String may also be empty or end with $ or @.
                var parts = variableName.Split(_selectors);

                if ((parts.Length == 0 || parts[0].Length == 0) && variableName.Length > 0) {
                    // Something odd like $$ or $@ so we got empty parts
                    // and yet variable name is not empty. Don't show anything.
                    return new INamedItemInfo[0];
                }

                if (parts.Length == 0 || parts[0].Length == 0 || variableName.IndexOfAny(_selectors) < 0) {
                    // Global scope
                    return _topLevelVariables.Values
                        .Where(x => !x.IsHidden)
                        .Take(maxCount)
                        .Select(m => new VariableInfo(m))
                        .ToArray();
                }

                // May be a package object line mtcars$
                var memberName = TrimToLastSelector(variableName);

                IReadOnlyList<IREvaluationResultInfo> infoList = null;

                Task.Run(async () => {
                    try {
                        var result = await Session.TryEvaluateAndDescribeAsync(memberName, REvaluationResultProperties.None, null);
                        if (!(result is IRErrorInfo)) {
                            infoList = await Session.DescribeChildrenAsync(REnvironments.GlobalEnv,
                                memberName, HasChildrenProperty | AccessorKindProperty, null, _maxResults);
                        }

                    } catch (Exception ex) when (!ex.IsCriticalException()) { }
                }).Wait(_maxWaitTime);

                if (infoList != null) {
                    return infoList
                        .OfType<IRValueInfo>()
                        .Where(m => m.AccessorKind == RChildAccessorKind.At || m.AccessorKind == RChildAccessorKind.Dollar)
                        .Take(maxCount)
                        .Select(m => new VariableInfo(TrimLeadingSelector(m.Name), string.Empty))
                        .ToArray();
                }
            } catch (OperationCanceledException) { } catch (RException) { }

            return new VariableInfo[0];
        }
        #endregion

        private static string TrimToLastSelector(string name) {
            var index = name.LastIndexOfAny(_selectors);
            return index >= 0 ? name.Substring(0, index) : name;
        }

        private static string TrimLeadingSelector(string name) {
            if (name.StartsWithOrdinal("$") || name.StartsWithOrdinal("@")) {
                return name.Substring(1);
            }
            return name;
        }

        protected override void SessionMutated() => UpdateList().DoNotWait();

        private async Task UpdateList() {
            if (_updating) {
                return;
            }

            try {
                _updating = true;
                // May be null in tests
                if (Session.IsHostRunning) {
                    var stackFrames = await Session.TracebackAsync();

                    var globalStackFrame = stackFrames.FirstOrDefault(s => s.IsGlobal);
                    if (globalStackFrame != null) {
                        const REvaluationResultProperties properties =
                            ExpressionProperty |
                            AccessorKindProperty |
                            TypeNameProperty |
                            ClassesProperty |
                            LengthProperty |
                            SlotCountProperty |
                            AttributeCountProperty |
                            DimProperty |
                            FlagsProperty;
                        var evaluation = await globalStackFrame.TryEvaluateAndDescribeAsync("base::environment()", "Global Environment", properties, RValueRepresentations.Str());
                        var settings = _services.GetService<IRSettings>();
                        var e = new RSessionDataObject(evaluation, settings.EvaluateActiveBindings);  // root level doesn't truncate children and return every variables

                        _topLevelVariables.Clear();

                        var children = await e.GetChildrenAsync();
                        if (children != null) {
                            foreach (var x in children) {
                                _topLevelVariables[x.Name] = x; // TODO: BUGBUG: this doesn't address removed variables
                            }
                        }
                    }
                }
            } catch (REvaluationException) { } finally {
                _updating = false;
            }
        }

        private class VariableInfo : INamedItemInfo {
            public VariableInfo(IRSessionDataObject e) :
                this(e.Name, e.TypeName) { }

            public VariableInfo(string name, string typeName) {
                Name = name;
                if (typeName.EqualsOrdinal("closure") || typeName.EqualsOrdinal("builtin")) {
                    ItemType = NamedItemType.Function;
                } else {
                    ItemType = NamedItemType.Variable;
                }
            }

            public string Description { get; } = string.Empty;

            public NamedItemType ItemType { get; }

            public string Name { get; }
        }
    }
}
