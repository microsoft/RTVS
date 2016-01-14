using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list members of a given variable
    /// </summary>
    public sealed class ObjectMembersCompletionProvider : IRAsyncCompletionListProvider {
        private IREditorWorkspace _workspace;
        private ImageSource _glyph;

        public ObjectMembersCompletionProvider() {
            var workspaceProvider = EditorShell.Current.ExportProvider.GetExportedValue<IREditorWorkspaceProvider>();
            _workspace = workspaceProvider.GetWorkspace();
            ImageSource keyWordGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemPublic);
        }

        #region IRAsyncCompletionListProvider
        public void GetEntriesAsync(RCompletionContext context,
                               Action<IReadOnlyCollection<RCompletion>, object> callback, object callbackParameter) {
            string variableName = RCompletionContext.GetVariableName(context.Session.TextView, context.TextBuffer.CurrentSnapshot);
            if (variableName.IndexOfAny(new char[] { '$' }) > 0) {
                _workspace.EvaluateExpression(
                            string.Format(CultureInfo.InvariantCulture, "paste0(colnames({0}), collapse = ' ')", variableName),
                            ParseResponse,
                            new CallBack() {
                                Action = callback,
                                Parameter = callbackParameter
                            });
            }
        }
        #endregion

        private void ParseResponse(string response, object p) {
            CallBack cb = p as CallBack;
            string[] names = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<RCompletion> completions = new List<RCompletion>();
            foreach(string n in names) {
                completions.Add(new RCompletion(n, n, string.Empty, _glyph));
            }
            cb.Action(completions, cb.Parameter);
        }

        class CallBack {
            public Action<IReadOnlyCollection<RCompletion>, object> Action;
            public object Parameter;
        }
    }
}
