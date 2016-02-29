// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
    public class TraceFailException : Exception
    {
        public TraceFailException(string message)
            : base(message)
        {
        }

        public TraceFailException(string message, string detailedMessage)
            : base(string.Join(Environment.NewLine, message, detailedMessage))
        {
        }
    }
}