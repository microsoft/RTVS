using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Editor.Tree.Definitions;

namespace Microsoft.R.Editor.Document.Definitions
{
    public interface IREditorDocument: IEditorDocument
    {
        IEditorTree EditorTree { get; }

        bool IsClosed { get; }
    }
}
