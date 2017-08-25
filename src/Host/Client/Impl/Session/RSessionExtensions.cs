// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client.Extensions;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionExtensions {
        public static async Task<string> GetRWorkingDirectoryAsync(this IRSession session) {
            if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    return await session.GetWorkingDirectoryAsync();
                } catch (RException) {
                } catch (OperationCanceledException) {
                }
            }
            return null;
        }

        public static async Task<string> GetRUserDirectoryAsync(this IRSession session, CancellationToken cancellationToken = default(CancellationToken)) {
            if (session.IsHostRunning) {
                await TaskUtilities.SwitchToBackgroundThread();
                try {
                    return await RSessionEvaluationCommands.GetRUserDirectoryAsync(session, cancellationToken);
                } catch (RException) {
                } catch (OperationCanceledException) { }
            }
            return null;
        }

        public static async Task<string> MakeRelativeToRUserDirectoryAsync(this IRSession session, string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var userDirectory = await session.GetRUserDirectoryAsync(cancellationToken);
            return name.MakeRRelativePath(userDirectory);
        }

        public static async Task<IEnumerable<string>> MakeRelativeToRUserDirectoryAsync(this IRSession session, IEnumerable<string> names, CancellationToken cancellationToken = default(CancellationToken)) {
            var userDirectory = await session.GetRUserDirectoryAsync(cancellationToken);
            return names.Select(n => n.MakeRRelativePath(userDirectory));
        }

        public static Task<string> GetFunctionCodeAsync(this IRSession session, string functionName, CancellationToken cancellationToken = default(CancellationToken)) 
            => session.EvaluateAsync<string>(Invariant($"paste0(deparse({functionName}), collapse='\n')"), REvaluationKind.Normal, cancellationToken);
    }
}