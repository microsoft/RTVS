using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.R.Package.Snippets.Definitions;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.TextManager.Interop;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    internal sealed class SnippetCache: ISnippetCache {
        private static SnippetCache _instance;
        private Dictionary<string, VsExpansion> _expansions = new Dictionary<string, VsExpansion>();

        internal SnippetCache(IVsExpansionManager expansionManager) {
            // Caching language expansion structs requires access to the IVsExpansionManager
            // service which is valid on the main thread only. So we create cache on the main 
            // thread so we can then access objects from background threads.
            CacheLanguageExpansionStructs(expansionManager);
            _instance = this;
        }

        public static ISnippetCache Current => _instance;

        public VsExpansion? GetExpansion(string shortcut) {
            if(_expansions.ContainsKey(shortcut)) {
                return _expansions[shortcut];
            }
            return null;
        }

        /// <summary>
        /// Caches expansions returned by IVsExpansionManager for a given language services.
        /// </summary>
        private void CacheLanguageExpansionStructs(IVsExpansionManager expansionManager) {
            if (_expansions.Keys.Count > 0) {
                return;
            }

            IVsExpansionEnumeration expansionEnumeration = null;

            int hr = expansionManager.EnumerateExpansions(
                RGuidList.RLanguageServiceGuid,
                0, /* fShortcutsOnly */
                ExpansionClient.AllStandardSnippetTypes,
                ExpansionClient.AllStandardSnippetTypes.Length,
                1, /* fIncludeNULLType */
                0, /* fIncludeDuplicates */
                out expansionEnumeration
            );
            ErrorHandler.ThrowOnFailure(hr);

            var buffer = new ExpansionBuffer();
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try {
                uint fetched;
                while (VSConstants.S_OK == (hr = expansionEnumeration.Next(1, new IntPtr[] { handle.AddrOfPinnedObject() }, out fetched))) {
                    var expansion = ConvertToVsExpansionAndFree((ExpansionBuffer)handle.Target);
                    Debug.Assert(!_expansions.ContainsKey(expansion.shortcut), Invariant($"Duplicate snippet shortcut {expansion.shortcut}"));
                    _expansions[expansion.shortcut] = expansion;
                }
                ErrorHandler.ThrowOnFailure(hr);
            } finally {
                handle.Free();
            }
        }

        private static VsExpansion ConvertToVsExpansionAndFree(ExpansionBuffer buffer) {
            VsExpansion expansion = new VsExpansion();

            ConvertToStringAndFree(ref buffer.descriptionPtr, ref expansion.description);
            ConvertToStringAndFree(ref buffer.pathPtr, ref expansion.path);
            ConvertToStringAndFree(ref buffer.shortcutPtr, ref expansion.shortcut);
            ConvertToStringAndFree(ref buffer.titlePtr, ref expansion.title);

            return expansion;
        }

        private static void ConvertToStringAndFree(ref IntPtr ptr, ref string s) {
            if (IntPtr.Zero != ptr) {
                s = Marshal.PtrToStringBSTR(ptr);
                Marshal.FreeBSTR(ptr);
                ptr = IntPtr.Zero;
            }
        }
    }
}

