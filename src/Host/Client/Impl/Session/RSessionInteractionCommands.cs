// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionInteractionCommands {
        public static Task QuitAsync(this IRSessionInteraction interaction) {
            return interaction.RespondAsync("q()\n");
        }
    }
}