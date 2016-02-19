using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    /// <summary>
    /// Main R editor command controller
    /// </summary>
    public class MdMainController : ViewController {
        public MdMainController(ITextView textView, ITextBuffer textBuffer)
            : base(textView, textBuffer) {
            ServiceManager.AddService(this, textView);
        }

        public static MdMainController Attach(ITextView textView, ITextBuffer textBuffer) {
            MdMainController controller = FromTextView(textView);
            if (controller == null) {
                controller = new MdMainController(textView, textBuffer);
            }

            return controller;
        }

        public static MdMainController FromTextView(ITextView textView) {
            return ServiceManager.GetService<MdMainController>(textView);
        }

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (TextView != null) {
                ServiceManager.RemoveService<MdMainController>(TextView);
            }

            base.Dispose(disposing);
        }
    }
}