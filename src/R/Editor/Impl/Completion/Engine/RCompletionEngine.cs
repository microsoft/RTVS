using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.Composition;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Support.Help;

namespace Microsoft.R.Editor.Completion.Engine
{
    internal static class RCompletionEngine
    {
        private static IEnumerable<Lazy<IRCompletionListProvider>> CompletionProviders { get; set; }
        private static RHelpDataSource _helpDataSource;

        /// <summary>
        /// Provides list of completion entries for a given location in the AST.
        /// </summary>
        /// <param name="tree">Document tree</param>
        /// <param name="position">Caret position in the document</param>
        /// <param name="autoShownCompletion">True if completion is forced (like when typing Ctrl+Space)</param>
        /// <returns>List of completion entries for a given location in the AST</returns>
        public static IReadOnlyCollection<IRCompletionListProvider> GetCompletionForLocation(AstRoot ast, int position, bool autoShownCompletion)
        {
            List<IRCompletionListProvider> providers = new List<IRCompletionListProvider>();

            foreach (var p in CompletionProviders)
            {
                providers.Add(p.Value);
            }

            return providers;
        }

        public static void Initialize()
        {
            if (_helpDataSource == null)
            {
                _helpDataSource = new RHelpDataSource();
            }

            if (CompletionProviders == null)
            {
                CompletionProviders = ComponentLocator<IRCompletionListProvider>.ImportMany();
            }
        }

        public static RHelpDataSource HelpDataSource
        {
            get { return _helpDataSource; }
        }
    }
}
