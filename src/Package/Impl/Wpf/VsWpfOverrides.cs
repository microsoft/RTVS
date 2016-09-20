// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.R.Wpf;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Wpf {
    public static class VsWpfOverrides {
        private static readonly Lazy<Assembly> ExtensionsExplorerUIAssemblyLazy = Lazy.Create(() => AppDomain.CurrentDomain.Load("Microsoft.VisualStudio.ExtensionsExplorer.UI"));

        public static void Apply() {
            OverrideBrushes();
            OverrideStyleKeys();
        }

        private static void OverrideBrushes() {
            Brushes.ActiveBorderKey = VsBrushes.ActiveBorderKey;
            Brushes.BorderBrushKey = VsBrushes.BrandedUIBorderKey;
            Brushes.ComboBoxBorderKey = VsBrushes.ComboBoxBorderKey;
            Brushes.ControlLinkTextHoverKey = VsBrushes.ControlLinkTextHoverKey;
            Brushes.ControlLinkTextKey = VsBrushes.ControlLinkTextKey;
            Brushes.DetailPaneBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.HeaderBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.InfoBackgroundKey = VsBrushes.InfoBackgroundKey;
            Brushes.InfoTextKey = VsBrushes.InfoTextKey;
            Brushes.LegalMessageBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.ListPaneBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.SplitterBackgroundKey = VsBrushes.CommandShelfBackgroundGradientKey;
            Brushes.ToolWindowBorderKey = VsBrushes.ToolWindowBorderKey;
            Brushes.ToolWindowButtonDownBorderKey = VsBrushes.ToolWindowButtonDownBorderKey;
            Brushes.ToolWindowButtonDownKey = VsBrushes.ToolWindowButtonDownKey;
            Brushes.ToolWindowButtonHoverActiveBorderKey = VsBrushes.ToolWindowButtonHoverActiveBorderKey;
            Brushes.ToolWindowButtonHoverActiveKey = VsBrushes.ToolWindowButtonHoverActiveKey;
            Brushes.UIText = VsBrushes.BrandedUITextKey;
            Brushes.WindowTextKey = VsBrushes.WindowTextKey;
            Brushes.WindowKey = VsBrushes.WindowKey;

            Brushes.HeaderColorsDefaultBrushKey = HeaderColors.DefaultBrushKey;
            Brushes.HeaderColorsDefaultTextBrushKey = HeaderColors.DefaultTextBrushKey;
            Brushes.HeaderColorsMouseDownBrushKey = HeaderColors.MouseDownBrushKey;
            Brushes.HeaderColorsMouseDownTextBrushKey = HeaderColors.MouseDownTextBrushKey;
            Brushes.HeaderColorsMouseOverBrushKey = HeaderColors.MouseOverBrushKey;
            Brushes.HeaderColorsMouseOverTextBrushKey = HeaderColors.MouseOverTextBrushKey;
            Brushes.HeaderColorsSeparatorLineBrushKey = HeaderColors.SeparatorLineBrushKey;

            Brushes.IndicatorFillBrushKey = ProgressBarColors.IndicatorFillBrushKey;

            var colorResources = GetColorResources();
            Brushes.BackgroundBrushKey = colorResources["BackgroundBrushKey"];
            Brushes.ContentMouseOverBrushKey = colorResources["ContentMouseOverBrushKey"];
            Brushes.ContentMouseOverTextBrushKey = colorResources["ContentMouseOverTextBrushKey"];
            Brushes.ContentInactiveSelectedBrushKey = colorResources["ContentInactiveSelectedBrushKey"];
            Brushes.ContentInactiveSelectedTextBrushKey = colorResources["ContentInactiveSelectedTextBrushKey"];
            Brushes.ContentSelectedBrushKey = colorResources["ContentSelectedBrushKey"];
            Brushes.ContentSelectedTextBrushKey = colorResources["ContentSelectedTextBrushKey"];
            Brushes.ContentBrushKey = colorResources["ContentBrushKey"];
        }

        private static void OverrideStyleKeys() {
            var comboBoxType = ExtensionsExplorerUIAssemblyLazy.Value.GetType("Microsoft.VisualStudio.ExtensionsExplorer.UI.AutomationComboBox");
            StyleKeys.ThemedComboStyleKey = new ComponentResourceKey(comboBoxType, "ThemedComboBoxStyle");
            StyleKeys.ScrollBarStyleKey = VsResourceKeys.ScrollBarStyleKey;
            StyleKeys.ScrollViewerStyleKey = VsResourceKeys.ScrollViewerStyleKey;
        }

        private static IDictionary<string, ThemeResourceKey> GetColorResources() {
            // use colors of VisualStudio UI.
            var colorResources = ExtensionsExplorerUIAssemblyLazy.Value.GetType("Microsoft.VisualStudio.ExtensionsExplorer.UI.ColorResources");

            return colorResources.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(ThemeResourceKey))
                .ToDictionary(p => p.Name, p => (ThemeResourceKey)p.GetValue(null));
        }
    }
}
