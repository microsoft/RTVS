using System;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Packages {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ProvideDebugPortPickerAttribute : RegistrationAttribute {
        private readonly Type _portPicker;

        public ProvideDebugPortPickerAttribute(Type portPicker) {
            _portPicker = portPicker;
        }

        public override void Register(RegistrationContext context) {
            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(_portPicker.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", _portPicker.Assembly.FullName);
            clsidGuidKey.SetValue("Class", _portPicker.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, _portPicker.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");
        }

        public override void Unregister(RegistrationContext context) {
        }
    }
}
