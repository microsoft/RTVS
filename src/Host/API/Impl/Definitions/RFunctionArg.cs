// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Represents argument to R function
    /// </summary>
    public sealed class RFunctionArg {
        /// <summary>
        /// Argument name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Argument value
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Creates unnamed argument
        /// </summary>
        /// <param name="value">Argument value</param>
        public RFunctionArg(string value) : this(null, value) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="value">Argument value</param>
        public RFunctionArg(string name, object value) {
            Name = name;
            Value = value;
        }
    }
}
