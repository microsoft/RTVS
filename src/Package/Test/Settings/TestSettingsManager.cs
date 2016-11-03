// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Settings;

namespace Microsoft.VisualStudio.R.Package.Test.Settings {
    internal sealed class TestSettingsManager : SettingsManager {
        private readonly TestSettingsStore _store = new TestSettingsStore();

        public override string GetApplicationDataFolder(ApplicationDataFolder folder) {
            throw new NotImplementedException();
        }

        public override EnclosingScopes GetCollectionScopes(string collectionPath) {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetCommonExtensionsSearchPaths() {
            throw new NotImplementedException();
        }

        public override EnclosingScopes GetPropertyScopes(string collectionPath, string propertyName) {
            throw new NotImplementedException();
        }

        public override SettingsStore GetReadOnlySettingsStore(SettingsScope scope) => _store;
        public override WritableSettingsStore GetWritableSettingsStore(SettingsScope scope) => _store;
    }
}
