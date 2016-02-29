using System;
<<<<<<< HEAD
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
=======
using System.Diagnostics;
>>>>>>> e8f84d926731d276426348f7abea135faeefadda
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Documentation {
    internal class OpenDocumentationCommand : PackageCommand {
        private string _url;

        public OpenDocumentationCommand(Guid group, int id, string url) :
            base(group, id) {
            _url = url;
        }

<<<<<<< HEAD
        internal override void SetStatus() {
            Enabled = true;
        }

        internal override void Handle() {
=======
        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
>>>>>>> e8f84d926731d276426348f7abea135faeefadda
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = _url;
            Process.Start(psi);
        }
    }
}
