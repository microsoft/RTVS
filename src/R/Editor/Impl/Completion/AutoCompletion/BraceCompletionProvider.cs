using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Completion.AutoCompletion {

    //[Export(typeof(IBraceCompletionDefaultProvider))]
    //[ContentType(RContentTypeDefinition.ContentType)]
    //[BracePair('{', '}')]
    //[BracePair('[', ']')]
    //[BracePair('(', ')')]
    //[BracePair('\'', '\'')]
    //[BracePair('\"', '\"')]
    internal sealed class BraceCompletionProvider : IBraceCompletionDefaultProvider  {
    }
}
