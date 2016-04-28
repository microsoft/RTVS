// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    public class VariableRHostScript : RHostScript {
        private VariableViewModel _globalEnv;
        private SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        private IDebugSessionProvider _debugSessionProvider;

        public VariableRHostScript() :
            base(VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>()) {

            _debugSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>();
        }

        public VariableViewModel GlobalEnvrionment {
            get {
                return _globalEnv;
            }
        }

        public async Task<DebugEvaluationResult> EvaluateAsync(string rScript) {
            // One eval at a time
            await _sem.WaitAsync();
            try {
                var debugSession = await _debugSessionProvider.GetDebugSessionAsync(Session);

                var frames = await debugSession.GetStackFramesAsync();
                var frame = frames.FirstOrDefault(f => f.Index == 0);

                const DebugEvaluationResultFields fields = DebugEvaluationResultFields.Classes
                    | DebugEvaluationResultFields.Expression
                    | DebugEvaluationResultFields.TypeName
                    | DebugEvaluationResultFields.Dim
                    | DebugEvaluationResultFields.Length;
                const string repr = "rtvs:::make_repr_str()";
                var result = await frame.EvaluateAsync(rScript, fields, repr);

                var globalResult = await frame.EvaluateAsync("base::environment()", fields, repr);
                _globalEnv = new VariableViewModel(globalResult, VsAppShell.Current.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>());

                return result;
            } finally {
                _sem.Release();
            }
        }

        /// <summary>
        /// evaluate R script and assert if the expectation is not found in global environment
        /// </summary>
        public async Task<IRSessionDataObject> EvaluateAndAssert(
            string rScript,
            VariableExpectation expectation,
            Action<IRSessionDataObject, VariableExpectation> assertAction) {

            await EvaluateAsync(rScript);

            var children = await _globalEnv.GetChildrenAsync();

            // must contain one and only expectation in result
            var evaluation = children.First(v => v.Name == expectation.Name);
            assertAction(evaluation, expectation);

            return evaluation;
        }

        public static void AssertEvaluationWrapper(IRSessionDataObject rdo, VariableExpectation expectation) {
            var v = (VariableViewModel)rdo;
            v.ShouldBeEquivalentTo(expectation, o => o.ExcludingMissingMembers());
        }

        public static void AssertEvaluationWrapper_ValueStartWith(IRSessionDataObject rdo, VariableExpectation expectation) {
            var v = (VariableViewModel)rdo;
            v.Name.ShouldBeEquivalentTo(expectation.Name);
            v.Value.Should().StartWith(expectation.Value);
            v.Class.ShouldBeEquivalentTo(expectation.Class);
            v.TypeName.ShouldBeEquivalentTo(expectation.TypeName);
            v.HasChildren.ShouldBeEquivalentTo(expectation.HasChildren);
        }
     }
}
