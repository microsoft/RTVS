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

namespace Microsoft.VisualStudio.R.Package.Wpf
{
    public static class VsWpfOverrides
    {
        private static readonly Lazy<Assembly> ExtensionsExplorerUIAssemblyLazy = Lazy.Create(() => AppDomain.CurrentDomain.Load("Microsoft.VisualStudio.ExtensionsExplorer.UI"));

        public static void Apply()
        {
            OverrideBrushes();
            OverrideStyleKeys();
        }

        private static void OverrideBrushes()
        {
            Brushes.ActiveBorderKey = VsBrushes.ActiveBorderKey;
            Brushes.BorderBrushKey = VsBrushes.BrandedUIBorderKey;
            Brushes.ButtonFaceBrushKey = VsBrushes.ButtonFaceKey;
            Brushes.ButtonTextBrushKey = VsBrushes.ButtonTextKey;
            Brushes.ComboBoxBorderKey = VsBrushes.ComboBoxBorderKey;
            Brushes.ControlLinkTextHoverKey = VsBrushes.ControlLinkTextHoverKey;
            Brushes.ControlLinkTextKey = VsBrushes.ControlLinkTextKey;
            Brushes.DetailPaneBackgroundKey = VsBrushes.BrandedUIBackgroundKey;
            Brushes.GrayTextBrushKey = VsBrushes.GrayTextKey;
            Brushes.HeaderBackgroundKey = VsBrushes.BrandedUIBackgroundKey;
            Brushes.InfoBackgroundKey = VsBrushes.InfoBackgroundKey;
            Brushes.InfoTextKey = VsBrushes.InfoTextKey;
            Brushes.LegalMessageBackgroundKey = VsBrushes.BrandedUIBackgroundKey;
            Brushes.ListPaneBackgroundKey = VsBrushes.BrandedUIBackgroundKey;
            Brushes.SplitterBackgroundKey = VsBrushes.CommandShelfBackgroundGradientKey;

            Brushes.ToolWindowBackgroundKey = VsBrushes.ToolWindowBackgroundKey;
            Brushes.ToolWindowBorderKey = VsBrushes.ToolWindowBorderKey;
            Brushes.ToolWindowButtonDownBorderKey = VsBrushes.ToolWindowButtonDownBorderKey;
            Brushes.ToolWindowButtonDownKey = VsBrushes.ToolWindowButtonDownKey;
            Brushes.ToolWindowButtonHoverActiveBorderKey = VsBrushes.ToolWindowButtonHoverActiveBorderKey;
            Brushes.ToolWindowButtonHoverActiveKey = VsBrushes.ToolWindowButtonHoverActiveKey;
            Brushes.UITextKey = VsBrushes.BrandedUITextKey;
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
            Brushes.BackgroundBrushKey = colorResources.TryGetThemeKey("BackgroundBrushKey");
            Brushes.ContentMouseOverBrushKey = colorResources.TryGetThemeKey("ContentMouseOverBrushKey");
            Brushes.ContentMouseOverTextBrushKey = colorResources.TryGetThemeKey("ContentMouseOverTextBrushKey");
            Brushes.ContentInactiveSelectedBrushKey = colorResources.TryGetThemeKey("ContentInactiveSelectedBrushKey");
            Brushes.ContentInactiveSelectedTextBrushKey = colorResources.TryGetThemeKey("ContentInactiveSelectedTextBrushKey");
            Brushes.ContentSelectedBrushKey = colorResources.TryGetThemeKey("ContentSelectedBrushKey");
            Brushes.ContentSelectedTextBrushKey = colorResources.TryGetThemeKey("ContentSelectedTextBrushKey");
            Brushes.ContentBrushKey = colorResources.TryGetThemeKey("ContentBrushKey");

            Brushes.ScrollBarBackgroundBrushKey = EnvironmentColors.ScrollBarBackgroundBrushKey;

            Brushes.TreeViewBackgroundBrushKey = TreeViewColors.BackgroundBrushKey;
            Brushes.TreeViewBackgroundTextBrushKey = TreeViewColors.BackgroundTextBrushKey;
            Brushes.TreeViewSelectedItemActiveBrushKey = TreeViewColors.SelectedItemActiveBrushKey;
            Brushes.TreeViewSelectedItemActiveTextBrushKey = TreeViewColors.SelectedItemActiveTextBrushKey;
            Brushes.TreeViewGlyphBrushKey = TreeViewColors.GlyphBrushKey;
            Brushes.TreeViewGlyphMouseOverBrushKey = TreeViewColors.GlyphMouseOverBrushKey;

            Brushes.GridLineBrushKey = EnvironmentColors.GridLineBrushKey;
        }

        private static object TryGetThemeKey(this IDictionary<string, ThemeResourceKey> dict, string name)
        {
            ThemeResourceKey k;
            dict.TryGetValue(name, out k);
            return k;
        }

        private static void OverrideStyleKeys()
        {
            var comboBoxType = ExtensionsExplorerUIAssemblyLazy.Value.GetType("Microsoft.VisualStudio.ExtensionsExplorer.UI.AutomationComboBox");
            StyleKeys.ThemedComboStyleKey = new ComponentResourceKey(comboBoxType, "ThemedComboBoxStyle");
            StyleKeys.ScrollBarStyleKey = VsResourceKeys.ScrollBarStyleKey;
            StyleKeys.ScrollViewerStyleKey = VsResourceKeys.ScrollViewerStyleKey;
            StyleKeys.ButtonStyleKey = VsResourceKeys.ButtonStyleKey;
            StyleKeys.TextBoxStyleKey = VsResourceKeys.TextBoxStyleKey;
        }

        private static IDictionary<string, ThemeResourceKey> GetColorResources()
        {
            // use colors of VisualStudio UI.
            var colorResources = ExtensionsExplorerUIAssemblyLazy.Value.GetType("Microsoft.VisualStudio.ExtensionsExplorer.UI.ColorResources");

            return colorResources.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(ThemeResourceKey))
                .ToDictionary(p => p.Name, p => (ThemeResourceKey)p.GetValue(null));
        }
    }
}
