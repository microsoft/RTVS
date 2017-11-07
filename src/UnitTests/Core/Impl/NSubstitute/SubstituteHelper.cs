// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using NSubstitute;

namespace Microsoft.UnitTests.Core.NSubstitute {
    [ExcludeFromCodeCoverage]
    public static class SubstituteHelper {
        public static Tuple<T1, T2> For<T1, T2>(params object[] constructorArguments) {
            var obj = Substitute.For(new[] { typeof(T1), typeof(T2) }, constructorArguments);
            return new Tuple<T1, T2>((T1)obj, (T2)obj);
        }
    }
}
