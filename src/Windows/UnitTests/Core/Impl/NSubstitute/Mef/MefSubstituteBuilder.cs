// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.R.Platform.Composition;

namespace Microsoft.UnitTests.Core.NSubstitute.Mef {
    [ExcludeFromCodeCoverage]
    public partial class MefSubstituteBuilder {
        private readonly HashSet<Type> _types;
        private readonly CompositionBatch _batch;

        public MefSubstituteBuilder() {
            _batch = new CompositionBatch();
            _types = new HashSet<Type>();
        }

        public MefSubstituteBuilder Add<T>() {
            return AddType(typeof(T));
        }

        public MefSubstituteBuilder AddWithNested<T>() {
            return AddTypeWithNested(typeof(T));
        }

        public MefSubstituteBuilder AddTypeWithNested(Type type) {
            AddType(type);

            var nestedTypesToAdd = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(t => t.GetCustomAttributes(typeof(ExportAttribute), inherit: true).Any());

            foreach (var nestedType in nestedTypesToAdd) {
                AddTypeWithNested(nestedType);
            }

            return this;
        }

        public MefSubstituteBuilder AddType(Type type) {
            _types.Add(type);
            return this;
        }

        public MefSubstituteBuilder AddValue<T>(T value) {
            return AddValue<T, T>(value);
        }

        public MefSubstituteBuilder AddValue<TContract, T>(T value)
            where T : TContract {
            var contractName = AttributedModelServices.GetContractName(typeof(TContract));
            AddValue(contractName, value);
            return this;
        }

        public MefSubstituteBuilder AddValue<T>(string contractName, T value) {
            _batch.AddValue(contractName, value);
            return this;
        }

        public MefSubstitute Build() {
            var compositionContainer = new NSubstituteCompositionContainer(new TypeCatalog(_types));
            compositionContainer.Compose(_batch);
            return MefSubstitute.CreateFrom(compositionContainer);
        }

    }
}