// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public struct LocatorResult {
        public bool Clicked { get; }
        public int X { get; }
        public int Y { get; }

        public LocatorResult(bool clicked, int x, int y) {
            Clicked = clicked;
            X = x;
            Y = y;
        }

        public static LocatorResult CreateClicked(int x, int y) => new LocatorResult(true, x, y);
        public static LocatorResult CreateNotClicked() => new LocatorResult(false, 0, 0);
    }
}
