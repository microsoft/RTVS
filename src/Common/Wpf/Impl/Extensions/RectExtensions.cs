// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace Microsoft.Common.Wpf.Extensions {
    public static class RectExtensions {
        public static bool Contains(this Rect rect1, Rect rect2) 
            => rect1.Top.LessOrCloseTo(rect2.Top) 
            && rect1.Bottom.GreaterOrCloseTo(rect2.Bottom)
            && rect1.Left.LessOrCloseTo(rect2.Left) 
            && rect1.Right.GreaterOrCloseTo(rect2.Right);
    }
}