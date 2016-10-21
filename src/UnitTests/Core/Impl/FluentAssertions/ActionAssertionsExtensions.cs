// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using FluentAssertions.Specialized;

namespace Microsoft.UnitTests.Core.FluentAssertions {
    public static class ActionAssertionsExtensions {
        public static Task ShouldNotThrowAsync(this Func<Task> asyncAction, string because = "", params object[] becauseArgs)
            => new AsyncAssertions(asyncAction).ShouldNotThrowAsync(because, becauseArgs);

        public static Task<ExceptionAssertions<TException>> ShouldThrowAsync<TException>(this Func<Task> asyncAction, string because = "", params object[] becauseArgs)
            where TException : Exception => new AsyncAssertions(asyncAction).ShouldThrowAsync<TException>(because, becauseArgs);
    }
}
