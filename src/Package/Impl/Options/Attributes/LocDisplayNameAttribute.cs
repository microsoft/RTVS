using System;
using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes
{
    /// <summary>
    /// Defines a localizable name attribute. The name is visible in the editor Tools | Options page.
    /// The class LocDisplayNameAttribute doesn't seem to be able to load our strings, so this new class is needed.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class CustomLocDisplayNameAttribute : DisplayNameAttribute
    {
        private bool _replaced;

        public CustomLocDisplayNameAttribute(string name)
            : base(name)
        {
        }

        public override string DisplayName
        {
            get
            {
                if (!_replaced)
                {
                    _replaced = true;
                    DisplayNameValue = Resources.ResourceManager.GetString(DisplayNameValue);
                }

                return base.DisplayName;
            }
        }
    }
}
