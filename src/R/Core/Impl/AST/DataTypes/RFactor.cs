// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Represents R factor. Vector with label vector attached
    /// </summary>
    [DebuggerDisplay("[Factor:{Mode}, {Length}]")]
    public class RFactor : RVector<RNumber> {
        public RFactor(int length, IRVector<RString> label, bool ordered = false)
            : base(RMode.Numeric, length) {
            Label = label;
            Ordered = ordered;
        }

        public IRVector<RString> Label { get; }

        public bool Ordered { get; }

        /// <summary>
        /// R language returns label value with index operator. For that purpose, use LabelOf() method
        /// </summary>
        public override RNumber this[int index] {
            get => base[index];

            set {
                if (value.Value <= 0 || value.Value > Label.Length) // R is one-based index unlike C family languages
                {
                    throw new ArgumentException("RFactor value is out of Label's range");
                }

                base[index] = value;
            }
        }

        public RString LabelOf(int index) {
            var number = this[index];
            var labelIndex = (int)number.Value - 1; // R is one-based index
            return Label[labelIndex];
        }
    }
}
