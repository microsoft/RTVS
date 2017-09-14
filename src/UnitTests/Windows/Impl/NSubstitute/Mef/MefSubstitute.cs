// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.UnitTests.Core.NSubstitute.Mef
{
    [ExcludeFromCodeCoverage]
    public class MefSubstitute
    {
        private readonly NSubstituteCompositionContainer _container;

        public static MefSubstitute CreateEmpty()
        {
            return new MefSubstitute(new NSubstituteCompositionContainer());
        }

        internal static MefSubstitute CreateFrom(NSubstituteCompositionContainer container)
        {
            return new MefSubstitute(container);
        }

        public static MefSubstituteBuilder Add<T>()
        {
            return new MefSubstituteBuilder().Add<T>();
        }

        public static MefSubstituteBuilder AddWithNested<T>()
        {
            return new MefSubstituteBuilder().AddWithNested<T>();
        }

        public static MefSubstituteBuilder AddType(Type type)
        {
            return new MefSubstituteBuilder().AddType(type);
        }

        public static MefSubstituteBuilder AddTypeWithNested(Type type)
        {
            return new MefSubstituteBuilder().AddTypeWithNested(type);
        }

        public static MefSubstituteBuilder AddValue<T>(T value)
        {
            return new MefSubstituteBuilder().AddValue(value);
        }

        public static MefSubstituteBuilder AddValueFactory<T, TValue>(Func<T, TValue> valueFactory)
        {
            return new MefSubstituteBuilder().AddValueFactory(valueFactory);
        }

        public static MefSubstituteBuilder AddValueFactory<T1, T2, TValue>(Func<T1, T2, TValue> valueFactory)
        {
            return new MefSubstituteBuilder().AddValueFactory(valueFactory);
        }

        public static MefSubstituteBuilder AddValueFactory<T1, T2, T3, TValue>(Func<T1, T2, T3, TValue> valueFactory)
        {
            return new MefSubstituteBuilder().AddValueFactory(valueFactory);
        }

        public static MefSubstituteBuilder AddValueFactory<T1, T2, T3, T4, TValue>(Func<T1, T2, T3, T4, TValue> valueFactory)
        {
            return new MefSubstituteBuilder().AddValueFactory(valueFactory);
        }

        private MefSubstitute(NSubstituteCompositionContainer container)
        {
            _container = container;
        }

        public T Get<T>() where T : class
        {
            return _container.GetExportedValue<T>();
        }

        public T Get<T>(string contractName) where T : class
        {
            return _container.GetExportedValue<T>(contractName);
        }

        public T Get<T>(Type type) where T : class
        {
            string contractName = AttributedModelServices.GetContractName(type);
            return _container.GetExportedValue<T>(contractName);
        }
    }
}
