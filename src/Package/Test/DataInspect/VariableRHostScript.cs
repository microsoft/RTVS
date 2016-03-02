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
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    public class VariableRHostScript : RHostScript {
        private VariableProvider _variableProvider;
        private EvaluationWrapper _globalEnv;

        public VariableRHostScript() :
            base(VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>()) {
            _variableProvider = new VariableProvider(base.SessionProvider, VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>());

            VsRHostScript.DoIdle(100);
        }

        public IVariableDataProvider VariableProvider {
            get {
                return _variableProvider;
            }
        }

        public EvaluationWrapper GlobalEnvrionment {
            get {
                return _globalEnv;
            }
        }

        private void OnGlobalEnvironmentEvaluated(DebugEvaluationResult result) {
            _globalEnv = new EvaluationWrapper(result);
            _mre.Set();
        }

        private ManualResetEventSlim _mre;
        public async Task EvaluateAsync(string rScript) {
            VariableSubscription subscription = null;
            try {
                _mre = new ManualResetEventSlim();

                _globalEnv = null;
                subscription = _variableProvider.Subscribe(0, "base::environment()", OnGlobalEnvironmentEvaluated);

                using (var evaluation = await base.Session.BeginEvaluationAsync()) {
                    await evaluation.EvaluateAsync(rScript);
                }

                if (System.Diagnostics.Debugger.IsAttached) {
                    _mre.Wait();
                } else {
                    if (!_mre.Wait(TimeSpan.FromSeconds(10))) {
                        throw new TimeoutException("Evaluate time out");
                    }
                }
            } finally {
                _variableProvider.Unsubscribe(subscription);
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
            var v = (EvaluationWrapper)rdo;
            v.ShouldBeEquivalentTo(expectation, o => o.ExcludingMissingMembers());
        }

        public static void AssertEvaluationWrapper_ValueStartWith(IRSessionDataObject rdo, VariableExpectation expectation) {
            var v = (EvaluationWrapper)rdo;
            v.Name.ShouldBeEquivalentTo(expectation.Name);
            v.Value.Should().StartWith(expectation.Value);
            v.Class.ShouldBeEquivalentTo(expectation.Class);
            v.TypeName.ShouldBeEquivalentTo(expectation.TypeName);
            v.HasChildren.ShouldBeEquivalentTo(expectation.HasChildren);
        }

        protected override void Dispose(bool disposing) {
            VsRHostScript.DoIdle(2000);

            base.Dispose(disposing);
            if (disposing) {
                _variableProvider.Dispose();
            }
        }
     }
}
