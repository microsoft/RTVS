// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    /// <summary>
    /// Defines localizable description attribute. Description is a help string
    /// that is displayed in the editor Tools | Options page.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class LocDescriptionAttribute : DescriptionAttribute {
        private bool _replaced;

        #region Constructors
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="description">Attribute description.</param>
        public LocDescriptionAttribute(string description)
            : base(description) { }
        #endregion

        #region Overriden Implementation
        /// <summary>
        /// Gets attribute description.
        /// </summary>
        public override string Description {
            get {
                if (!_replaced) {
                    _replaced = true;
                    DescriptionValue = Resources.ResourceManager.GetString(base.Description);
                }

                return base.Description;
            }
        }
        #endregion
    }
}
