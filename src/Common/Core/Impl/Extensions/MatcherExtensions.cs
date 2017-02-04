// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Microsoft.Common.Core {
    public static class MatcherExtensions {
        public static IEnumerable<string> GetMatchedFiles(this Matcher matcher,  IEnumerable<string> dirPaths) {
            List<string> files = new List<string>();
            foreach(string dir in dirPaths) {
                files.AddRange(matcher.GetResultsInFullPath(dir));
            }

            return files.Distinct();
        }
    }
}
