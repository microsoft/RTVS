// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class DefaultHeaderData : IRange<string> {
        public enum Mode {
            Column,
            Row,
        }

        private readonly Mode _mode;
        private readonly bool _is1D;

        public DefaultHeaderData(Range range, Mode columnMode, bool is1D) {
            Range = range;
            _mode = columnMode;
            _is1D = is1D;
        }

        public string this[long index] {
            get {
                long rIndex = checked(index + 1);
                if (_is1D) {
                    switch (_mode) {
                        case Mode.Column:
                            return Invariant($"[]");
                        case Mode.Row:
                            return Invariant($"[{rIndex}]");
                    }
                } else {
                    switch (_mode) {
                        case Mode.Column:
                            return Invariant($"[,{rIndex}]");
                        case Mode.Row:
                            return Invariant($"[{rIndex},]");
                    }
                }

                throw new InvalidOperationException(nameof(DefaultHeaderData) + ": unknown mode " + _mode);
            }

            set {
                throw new NotImplementedException();
            }
        }

        public Range Range { get; }
    }
}
