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
            Brushes.ActiveBorder = VsBrushes.ActiveBorderKey;
            Brushes.BorderBrush = VsBrushes.BrandedUIBorderKey;
            Brushes.ButtonFaceBrush = VsBrushes.ButtonFaceKey;
            Brushes.ButtonTextBrush = VsBrushes.ButtonTextKey;
            Brushes.ComboBoxBorder = VsBrushes.ComboBoxBorderKey;
            Brushes.ControlLinkTextHover = VsBrushes.ControlLinkTextHoverKey;
            Brushes.ControlLinkText = VsBrushes.ControlLinkTextKey;
            Brushes.DetailPaneBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.GrayTextBrush = VsBrushes.GrayTextKey;
            Brushes.HeaderBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.InfoBackground = VsBrushes.InfoBackgroundKey;
            Brushes.InfoText = VsBrushes.InfoTextKey;
            Brushes.LegalMessageBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.ListPaneBackground = VsBrushes.BrandedUIBackgroundKey;
            Brushes.SplitterBackground = VsBrushes.CommandShelfBackgroundGradientKey;
            Brushes.ToolWindowBorder = VsBrushes.ToolWindowBorderKey;
            Brushes.ToolWindowButtonDownBorder = VsBrushes.ToolWindowButtonDownBorderKey;
            Brushes.ToolWindowButtonDown = VsBrushes.ToolWindowButtonDownKey;
            Brushes.ToolWindowButtonHoverActiveBorder = VsBrushes.ToolWindowButtonHoverActiveBorderKey;
            Brushes.ToolWindowButtonHoverActive = VsBrushes.ToolWindowButtonHoverActiveKey;
            Brushes.UIText = VsBrushes.BrandedUITextKey;
            Brushes.WindowText = VsBrushes.WindowTextKey;
            Brushes.Window = VsBrushes.WindowKey;

            Brushes.HeaderColorsDefaultBrush = HeaderColors.DefaultBrushKey;
            Brushes.HeaderColorsDefaultTextBrush = HeaderColors.DefaultTextBrushKey;
            Brushes.HeaderColorsMouseDownBrush = HeaderColors.MouseDownBrushKey;
            Brushes.HeaderColorsMouseDownTextBrush = HeaderColors.MouseDownTextBrushKey;
            Brushes.HeaderColorsMouseOverBrush = HeaderColors.MouseOverBrushKey;
            Brushes.HeaderColorsMouseOverTextBrush = HeaderColors.MouseOverTextBrushKey;
            Brushes.HeaderColorsSeparatorLineBrush = HeaderColors.SeparatorLineBrushKey;

            Brushes.IndicatorFillBrush = ProgressBarColors.IndicatorFillBrushKey;

            var colorResources = GetColorResources();
            Brushes.BackgroundBrush = colorResources.TryGetThemeKey("BackgroundBrushKey");
            Brushes.ContentMouseOverBrush = colorResources.TryGetThemeKey("ContentMouseOverBrushKey");
            Brushes.ContentMouseOverTextBrush = colorResources.TryGetThemeKey("ContentMouseOverTextBrushKey");
            Brushes.ContentInactiveSelectedBrush = colorResources.TryGetThemeKey("ContentInactiveSelectedBrushKey");
            Brushes.ContentInactiveSelectedTextBrush = colorResources.TryGetThemeKey("ContentInactiveSelectedTextBrushKey");
            Brushes.ContentSelectedBrush = colorResources.TryGetThemeKey("ContentSelectedBrushKey");
            Brushes.ContentSelectedTextBrush = colorResources.TryGetThemeKey("ContentSelectedTextBrushKey");
            Brushes.ContentBrush = colorResources.TryGetThemeKey("ContentBrushKey");
        }

        private static object TryGetThemeKey(this IDictionary<string, ThemeResourceKey> dict, string name) {
            ThemeResourceKey k;
            dict.TryGetValue(name, out k);
            return k;
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
