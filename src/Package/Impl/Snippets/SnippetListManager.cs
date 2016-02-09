using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Items;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    /// <summary>
    /// Manages a list of all available snippets, and returns filtered 
    /// subsets of that list to the code population IntelliSense completion
    /// window.
    /// </summary>
    public sealed partial class SnippetListManager : IVsExpansionEvents, IDisposable {
        /// <summary>
        /// Singleton implementation
        /// </summary>
        private static Lazy<SnippetListManager> _instance = Lazy.Create(() => new SnippetListManager());
        private IVsExpansionManager _expansionManager;

        private Dictionary<Guid, SnippetCache> _perLanguageSnippetCache;                    // Cached data
        private Dictionary<Guid, SnippetCache> _perLanguageLoadingSnippetCache;             // Data being loaded in a background thread
        private Dictionary<Guid, ManualResetEvent> _snippetCacheInitialized;                // "Cache has actual data" events (per language service)
        private Dictionary<Guid, IdleTimeAsyncTask> _perLanguageSnippetCachingTasks;
        private const int _snippetCacheWaitMillisecondTimeout = 2000;                       // Timeout in milliseconds when waiting for cache init
        private object _sync = new object();                                                // Protection for _snippetCache object

        ConnectionPointCookie _expansionEventsConnectionPointCookie;

        /// <summary>
        /// Manages a list of snippets for purposes of 
        /// using them via IntelliSense Statement Completion List.
        /// </summary>
        /// <remarks>
        /// NOTE: This is a singleton object. Do not create instances of 
        /// it explicitely. Instead please use static factory method GetInstance().
        /// </remarks>
        /// <param name="serviceProvider"></param>
        private SnippetListManager() {
            _expansionManager = CompletionUtilities.GetExpansionManager();

            int languageServiceCount = SnippetCache.LanguageServiceGuidToContentType.Count;

            _perLanguageSnippetCache = new Dictionary<Guid, SnippetCache>(languageServiceCount);
            _perLanguageLoadingSnippetCache = new Dictionary<Guid, SnippetCache>(languageServiceCount);
            _snippetCacheInitialized = new Dictionary<Guid, ManualResetEvent>(languageServiceCount);
            _perLanguageSnippetCachingTasks = new Dictionary<Guid, IdleTimeAsyncTask>(languageServiceCount);

            foreach (Guid languageServiceGuid in SnippetCache.LanguageServiceGuidToContentType.Keys) {
                _perLanguageSnippetCache[languageServiceGuid] = new SnippetCache(_expansionManager, languageServiceGuid);
                _snippetCacheInitialized[languageServiceGuid] = new ManualResetEvent(false);
            }

            RegisterForExpansionEvents();

            VsAppShell.Current.Terminating += (s, e) => Dispose();
        }

        /// <summary>
        /// Singleton implementation
        /// </summary>
        public static SnippetListManager FromContentType(IContentType contentType) {
            _instance.Value.EnsureInitialized(contentType);
            return _instance.Value;
        }

        void RegisterForExpansionEvents() {
            // Just in case, so that we never register twice. Harmless if we aren't registered.
            UnRegisterFromExpansionEvents();

            // Now actually register for the events.
            _expansionEventsConnectionPointCookie =
                new ConnectionPointCookie(_expansionManager, this, typeof(IVsExpansionEvents));
        }

        void UnRegisterFromExpansionEvents() {
            if (_expansionEventsConnectionPointCookie != null) {
                _expansionEventsConnectionPointCookie.Dispose();
                _expansionEventsConnectionPointCookie = null;
            }
        }

        void EnsureInitialized(Guid languageServiceGuid) {
            try {
                bool alreadyInitialized = false;
                lock (_sync) {
                    alreadyInitialized = _perLanguageSnippetCache[languageServiceGuid].IsInitialized ||
                                         _perLanguageLoadingSnippetCache.ContainsKey(languageServiceGuid);
                }

                // If we are already loading snippets for the specified language service, 
                // there is no need to cancel and re-start
                if (!alreadyInitialized) {
                    _snippetCacheInitialized[languageServiceGuid].Reset();
                    ResetSnippetCache(languageServiceGuid);
                }
            } catch {
                // Snippet list manager is a singleton. If initialization fails, we provide an empty
                // snippets cache and set the "cache has actual data" flag, which will eliminate the
                // possibility of deadlock when the main thread waits for initialization of the cache.
                _snippetCacheInitialized[languageServiceGuid].Set();
                throw;
            }
        }

        void ResetSnippetCache(Guid languageServiceGuid) {
            // Initialize cache in the new thread (fire and forget).
            IdleTimeAsyncTask previousSnippetCachingTask;
            if (_perLanguageSnippetCachingTasks.TryGetValue(languageServiceGuid, out previousSnippetCachingTask) && previousSnippetCachingTask != null) {
                previousSnippetCachingTask.Dispose();
            }

            lock (_sync) {
                SnippetCache loadingSnippetCache;
                if (_perLanguageLoadingSnippetCache.TryGetValue(languageServiceGuid, out loadingSnippetCache) && loadingSnippetCache != null) {
                    loadingSnippetCache.IsAbandoned = true;
                }

                _perLanguageLoadingSnippetCache[languageServiceGuid] = new SnippetCache(_expansionManager, languageServiceGuid);
            }

            _perLanguageSnippetCachingTasks[languageServiceGuid] = new IdleTimeAsyncTask(() => EnsureSnippetCacheWorker(_perLanguageLoadingSnippetCache[languageServiceGuid]));
            _perLanguageSnippetCachingTasks[languageServiceGuid].DoTaskOnIdle(500);
        }

        void ResetSnippetCache() {
            ResetSnippetCache(HtmlGuids.HtmlLegacyLanguageServiceGuid);
            ResetSnippetCache(CssGuids.CssLanguageServiceGuid);
        }

        object EnsureSnippetCacheWorker(SnippetCache newCache) {
            // We want to signal that we are done if either we actually updated the cache
            // or if an exception was thrown (we don't want to deadlock).
            bool cacheUpdated = true;

            try {
                newCache.Load();

                // Submit the changes. Double time stamp check here is used to avoid unnecessary locking in case thread's snippet
                // cache object (newCache) is already obsolete and must be ignored. 

                if (_perLanguageSnippetCache[newCache.LanguageServiceGuid].SequenceId < newCache.SequenceId && !newCache.IsAbandoned) {
                    lock (_sync) {
                        if (_perLanguageSnippetCache[newCache.LanguageServiceGuid].SequenceId < newCache.SequenceId) {
                            _perLanguageSnippetCache[newCache.LanguageServiceGuid] = newCache;
                        }

                        if (_perLanguageLoadingSnippetCache[newCache.LanguageServiceGuid] == newCache) {
                            _perLanguageLoadingSnippetCache[newCache.LanguageServiceGuid] = null;
                        }
                    }
                } else {
                    cacheUpdated = false;
                }
            } finally {
                // If the worker thread throws an exception, the "cache initialized" event may never be set. 
                // That may result in a deadlock in GetSnippetCache method, which waits for this event before
                // returning the data.
                if (cacheUpdated && _snippetCacheInitialized != null) {
                    _snippetCacheInitialized[newCache.LanguageServiceGuid].Set();
                }
            }

            return null;
        }

        /// <summary>
        /// The function returns actual snippet cache object.
        /// </summary>
        /// <param name="ensureInitialized">When true, the method will wait for pending initializations to complete</param>
        /// <returns>Snippet cache objects</returns>
        private SnippetCache GetSnippetCache(Guid languageServiceGuid, bool ensureInitialized) {
            if (ensureInitialized)
                _snippetCacheInitialized[languageServiceGuid].WaitOne(_snippetCacheWaitMillisecondTimeout);

            return _perLanguageSnippetCache[languageServiceGuid];
        }

        public void Dispose() {
            WebEditor.OnTerminate -= WebEditor_OnTerminate;

            UnRegisterFromExpansionEvents();

            if (_snippetCacheInitialized != null) {
                foreach (Guid languageServiceGuid in _snippetCacheInitialized.Keys) {
                    if (_snippetCacheInitialized[languageServiceGuid] != null) {
                        _snippetCacheInitialized[languageServiceGuid].Dispose();
                    }
                }

                _snippetCacheInitialized = null;
            }

            if (_perLanguageSnippetCachingTasks != null) {
                foreach (Guid languageServiceGuid in _perLanguageSnippetCachingTasks.Keys) {
                    _perLanguageSnippetCachingTasks[languageServiceGuid].Dispose();
                }

                _perLanguageSnippetCachingTasks = null;
            }

            _instance = null;
        }

        #region IVsExpansionEvents Members

        int IVsExpansionEvents.OnAfterSnippetsKeyBindingChange(uint dwCmdGuid, uint dwCmdId, int fBound) {
            return VSConstants.S_OK;
        }

        int IVsExpansionEvents.OnAfterSnippetsUpdate() {
            ResetSnippetCache();
            return VSConstants.S_OK;
        }

        #endregion

        #region ISnippetListManager Members

        public SnippetInfo[] GetApplicableSnippetList(ITextBuffer textBuffer, string kind, bool waitForInitialization) {
            string currentFilePath = textBuffer.GetFileName();
            string currentProjectTypeGuids = HtmlUtilities.GetCurrentProjectTypeGuids(currentFilePath);
            string currentFileExtension = Path.GetExtension(currentFilePath);
            Guid currentLanguageServiceGuid = HtmlUtilities.GetCurrentLanguageServiceGuid(textBuffer);

            return GetSnippetList(currentProjectTypeGuids, currentFileExtension, currentLanguageServiceGuid, kind, waitForInitialization);
        }

        public SnippetInfo[] GetSnippetList(string projectTypeIds, string fileExtension, Guid languageId, string kind, bool waitForInitialization) {
            return GetSnippetCache(languageId, ensureInitialized: waitForInitialization).GetSnippetList(projectTypeIds, fileExtension, languageId, kind);
        }

        public void GetGenericSnippetInfo(string shortcut, string path, Guid languageServiceId, out MSXML.IXMLDOMNode snippetXMLNode, out string relativePath) {
            GetSnippetCache(languageServiceId, ensureInitialized: true).GetGenericSnippetInfo(shortcut, path, out snippetXMLNode, out relativePath);
        }

        public void GetGenericSnippetFileInfo(string filePath, Guid languageServiceId, out MSXML.IXMLDOMNode snippetXMLNode, out string relativePath) {
            GetSnippetCache(languageServiceId, ensureInitialized: true).GetGenericSnippetFileInfo(filePath, out snippetXMLNode, out relativePath);
        }

        public bool IsGenericSnippet(string shortcut, Guid languageServiceId) {
            return GetSnippetCache(languageServiceId, ensureInitialized: true).IsGenericSnippet(shortcut);
        }

        public bool IsGenericSnippetFile(string filePath, Guid languageServiceId) {
            return GetSnippetCache(languageServiceId, true).IsGenericSnippetFile(filePath);
        }

        public bool IsSnippetShortcut(Guid snippetLanguageId, string shortcut) {
            return GetSnippetCache(snippetLanguageId, ensureInitialized: true).IsSnippetShortcut(shortcut);
        }
        #endregion
    }
}
