using System;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Packages {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ProvideComClassAttribute : RegistrationAttribute {
        private readonly Type _comClass;

        public ProvideComClassAttribute(Type comClass) {
            _comClass = comClass;
        }

        public override void Register(RegistrationContext context) {
            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(_comClass.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", _comClass.Assembly.FullName);
            clsidGuidKey.SetValue("Class", _comClass.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, _comClass.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");
        }

        public override void Unregister(RegistrationContext context) { }
    }
}
