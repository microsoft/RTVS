// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// Base class for all R objects
    /// </summary>
    public abstract class RObject {
        public bool IsScalar {
            get {
                IRVector vector = this as IRVector;

                if (vector != null && vector.Length == 1) {
                    switch (vector.Mode) {
                        case RMode.Character:
                        case RMode.Complex:
                        case RMode.Numeric:
                        case RMode.Logical:
                            return true;
                    }
                }

                return false;
            }
        }

        public bool IsNumber {
            get {
                IRVector vector = this as IRVector;
                return vector != null && vector.Length == 1 && (vector.Mode == RMode.Numeric || vector.Mode == RMode.Complex);
            }
        }

        public bool IsString {
            get {
                IRVector vector = this as IRVector;
                return vector != null && vector.Length == 1 && vector.Mode == RMode.Character;
            }
        }

        public bool IsBoolean {
            get {
                IRVector vector = this as IRVector;
                return vector != null && vector.Length == 1 && vector.Mode == RMode.Logical;
            }
        }

        public bool IsFunction {
            get {
                IRVector vector = this as IRVector;
                return vector != null && vector.Length == 1 && vector.Mode == RMode.Function;
            }
        }
    }
}
