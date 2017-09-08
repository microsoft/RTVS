// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using Microsoft.R.Platform.Composition;

namespace Microsoft.UnitTests.Core.NSubstitute.Mef {
    public partial class MefSubstituteBuilder {
        #region Func<T, TValue>
        public MefSubstituteBuilder AddValueFactory<T, TValue>(Func<T, TValue> valueFactory) {
            return AddValueFactory<T, TValue, TValue>(valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T, TContract, TValue>(Func<T, TValue> valueFactory)
            where TValue : TContract {
            string contractName = AttributedModelServices.GetContractName(typeof(TContract));
            return AddValueFactory(contractName, valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T, TValue>(string contractName, Func<T, TValue> valueFactory) {
            return AddValueFactory(contractName, AttributedModelServices.CreatePartDefinition(typeof(ImportStub<T>), null), typeof(TValue), valueFactory);
        }
        #endregion

        #region Func<T1, T2, TValue>
        public MefSubstituteBuilder AddValueFactory<T1, T2, TValue>(Func<T1, T2, TValue> valueFactory) {
            return AddValueFactory<T1, T2, TValue, TValue>(valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T1, T2, TContract, TValue>(Func<T1, T2, TValue> valueFactory)
            where TValue : TContract {
            string contractName = AttributedModelServices.GetContractName(typeof(TContract));
            return AddValueFactory(contractName, valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T1, T2, TValue>(string contractName, Func<T1, T2, TValue> valueFactory) {
            return AddValueFactory(contractName, AttributedModelServices.CreatePartDefinition(typeof(ImportStub<T1, T2>), null), typeof(TValue), valueFactory);
        }
        #endregion

        #region Func<T1, T2, T3, TValue>
        public MefSubstituteBuilder AddValueFactory<T1, T2, T3, TValue>(Func<T1, T2, T3, TValue> valueFactory) {
            return AddValueFactory<T1, T2, T3, TValue, TValue>(valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T1, T2, T3, TContract, TValue>(Func<T1, T2, T3, TValue> valueFactory)
            where TValue : TContract {
            string contractName = AttributedModelServices.GetContractName(typeof(TContract));
            return AddValueFactory(contractName, valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T1, T2, T3, TValue>(string contractName, Func<T1, T2, T3, TValue> valueFactory) {
            return AddValueFactory(contractName, AttributedModelServices.CreatePartDefinition(typeof(ImportStub<T1, T2, T3>), null), typeof(TValue), valueFactory);
        }
        #endregion

        #region Func<T1, T2, T3, T4, TValue>
        public MefSubstituteBuilder AddValueFactory<T1, T2, T3, T4, TValue>(Func<T1, T2, T3, T4, TValue> valueFactory) {
            return AddValueFactory<T1, T2, T3, T4, TValue, TValue>(valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T1, T2, T3, T4, TContract, TValue>(Func<T1, T2, T3, T4, TValue> valueFactory)
            where TValue : TContract {
            string contractName = AttributedModelServices.GetContractName(typeof(TContract));
            return AddValueFactory(contractName, valueFactory);
        }

        public MefSubstituteBuilder AddValueFactory<T1, T2, T3, T4, TValue>(string contractName, Func<T1, T2, T3, T4, TValue> valueFactory) {
            return AddValueFactory(contractName, AttributedModelServices.CreatePartDefinition(typeof(ImportStub<T1, T2, T3, T4>), null), typeof(TValue), valueFactory);
        }
        #endregion

        private MefSubstituteBuilder AddValueFactory(string contractName, ComposablePartDefinition ctorDefinition, Type type, Delegate factory) {
            _batch.AddValueFactory(contractName, ctorDefinition, type, factory);
            return this;
        }

        private class ImportStub<T> {
            [ImportingConstructor]
            public ImportStub(T parameter) { }
        }

        private class ImportStub<T1, T2> {
            [ImportingConstructor]
            public ImportStub(T1 parameter1, T2 parameter2) { }
        }

        private class ImportStub<T1, T2, T3> {
            [ImportingConstructor]
            public ImportStub(T1 parameter1, T2 parameter2, T3 parameter3) { }
        }

        private class ImportStub<T1, T2, T3, T4> {
            [ImportingConstructor]
            public ImportStub(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4) { }
        }
    }
}
