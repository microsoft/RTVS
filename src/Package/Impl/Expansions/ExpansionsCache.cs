using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor.Snippets;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Expansions {
    internal sealed class ExpansionsCache : IExpansionsCache {
        private static ExpansionsCache _instance;
        private readonly Dictionary<string, VsExpansion> _expansions = new Dictionary<string, VsExpansion>();
        private static IServiceContainer _services;

        internal ExpansionsCache(IVsExpansionManager expansionManager, IServiceContainer services) {
            // Caching language expansion structs requires access to the IVsExpansionManager
            // service which is valid on the main thread only. So we create cache on the main 
            // thread so we can then access objects from background threads.
            _instance = this;
            _services = services;
            IdleTimeAction.Create(() => CacheLanguageExpansionStructs(expansionManager), 200, typeof(ExpansionsCache), services.GetService<IIdleTimeService>());
        }

        public static IExpansionsCache Current {
            get {
                if (_instance == null) {
                    Load(_services);
                }
                return _instance;
            }
        }

        public static void Load(IServiceContainer services) {
            var textManager2 = services.GetService<IVsTextManager2>(typeof(SVsTextManager));
            textManager2.GetExpansionManager(out IVsExpansionManager expansionManager);
            _instance = new ExpansionsCache(expansionManager, services);
        }

        public VsExpansion? GetExpansion(string shortcut) {
            if (_expansions.ContainsKey(shortcut)) {
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

            IVsExpansionEnumeration expansionEnumerator = null;

            int hr = expansionManager.EnumerateExpansions(
                RGuidList.RLanguageServiceGuid,
                0,    // return all info
                null, // return all types
                0,    // return all types
                0,    // do not return NULL type
                0,    // do not return duplicates
                out expansionEnumerator
            );
            ErrorHandler.ThrowOnFailure(hr);

            if (expansionEnumerator != null) {
                VsExpansion expansion = new VsExpansion();
                IntPtr[] pExpansionInfo = new IntPtr[1];
                try {
                    // Allocate enough memory for one VSExpansion structure.
                    // This memory is filled in by the Next method.
                    pExpansionInfo[0] = Marshal.AllocCoTaskMem(Marshal.SizeOf(expansion));

                    uint count = 0;
                    expansionEnumerator.GetCount(out count);
                    for (uint i = 0; i < count; i++) {
                        uint fetched = 0;
                        expansionEnumerator.Next(1, pExpansionInfo, out fetched);
                        if (fetched > 0) {
                            // Convert the returned blob of data into a structure.
                            expansion = (VsExpansion)Marshal.PtrToStructure(pExpansionInfo[0], typeof(VsExpansion));
                            if (!string.IsNullOrEmpty(expansion.shortcut)) {
                                _expansions[expansion.shortcut] = expansion;
                            }
                        }
                    }
                } finally {
                    if (pExpansionInfo[0] != null) {
                        Marshal.FreeCoTaskMem(pExpansionInfo[0]);
                    }
                }
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

        #region ISnippetInformationSource
        public bool IsSnippet(string name) {
            return _expansions.ContainsKey(name);
        }

        public IEnumerable<ISnippetInfo> Snippets {
            get {
                foreach (var e in _expansions) {
                    yield return new SnippetInfo(e.Key, e.Value.description);
                }
            }
        }
        #endregion

        class SnippetInfo : ISnippetInfo {
            public string Description { get; }
            public string Name { get; }

            public SnippetInfo(string name, string description) {
                Name = name;
                Description = description;
            }
        }
    }
}

