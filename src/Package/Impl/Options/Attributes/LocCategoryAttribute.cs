using System;
using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes
{
    /// <summary>
    /// Defines localizable category attribute. Category is visible in the editor Tools | Options page.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class LocCategoryAttribute : CategoryAttribute
    {
        #region Constructors
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="description">Category name.</param>
        public LocCategoryAttribute(string category)
            : base(category)
        {
        }
        #endregion

        #region Overriden Implementation
        /// <summary>
        /// Gets localized category name.
        /// </summary>
        protected override string GetLocalizedString(string value)
        {
            return Resources.ResourceManager.GetString(base.Category);
        }
        #endregion
    }
}
