// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.R.Wpf;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Brushes = Microsoft.R.Wpf.Brushes;

namespace Microsoft.VisualStudio.R.Package.Wpf
{
    public static class VsWpfOverrides
    {
        private static readonly Lazy<Assembly> ExtensionsExplorerUIAssemblyLazy = Lazy.Create(() => AppDomain.CurrentDomain.Load("Microsoft.VisualStudio.ExtensionsExplorer.UI"));

        public static void Apply()
        {
            OverrideBrushes();
            OverrideFontKeys();
            OverrideImageSources();
            OverrideStyleKeys();
        }

        private static void OverrideBrushes() {
            Brushes.ActiveBorderKey = VsBrushes.ActiveBorderKey;
            Brushes.BorderBrushKey = VsBrushes.BrandedUIBorderKey;
            Brushes.ButtonFaceBrushKey = EnvironmentColors.SystemButtonFaceBrushKey;
            Brushes.ButtonHighlightBrushKey = EnvironmentColors.SystemButtonHighlightBrushKey;
            Brushes.ButtonShadowBrushKey = EnvironmentColors.SystemButtonShadowBrushKey;
            Brushes.ButtonTextBrushKey = EnvironmentColors.SystemButtonTextBrushKey;
            Brushes.ComboBoxBorderKey = VsBrushes.ComboBoxBorderKey;
            Brushes.ControlKey = VsBrushes.ThreeDFaceKey;
            Brushes.ControlDarkKey = VsBrushes.ThreeDShadowKey;
            Brushes.ControlLightKey = VsBrushes.ThreeDLightShadowKey;
            Brushes.ControlTextKey = VsBrushes.ButtonTextKey;
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

            Brushes.ToolWindowBackgroundColorKey = EnvironmentColors.ToolWindowBackgroundColorKey;
            Brushes.ToolWindowBackgroundBrushKey = EnvironmentColors.ToolWindowBackgroundBrushKey;
            Brushes.ToolWindowBorderColorKey = EnvironmentColors.ToolWindowBorderColorKey;
            Brushes.ToolWindowBorderBrushKey = EnvironmentColors.ToolWindowBorderBrushKey;
            Brushes.ToolWindowButtonActiveGlyphBrushKey = EnvironmentColors.ToolWindowButtonActiveGlyphBrushKey;
            Brushes.ToolWindowButtonDownBrushKey = EnvironmentColors.ToolWindowButtonDownBrushKey;
            Brushes.ToolWindowButtonDownActiveGlyphBrushKey = EnvironmentColors.ToolWindowButtonDownActiveGlyphBrushKey;
            Brushes.ToolWindowButtonDownBorderBrushKey = EnvironmentColors.ToolWindowButtonDownBorderBrushKey;
            Brushes.ToolWindowButtonDownInactiveGlyphBrushKey = EnvironmentColors.ToolWindowButtonDownInactiveGlyphBrushKey;
            Brushes.ToolWindowButtonHoverActiveBrushKey = EnvironmentColors.ToolWindowButtonHoverActiveBrushKey;
            Brushes.ToolWindowButtonHoverActiveBorderBrushKey = EnvironmentColors.ToolWindowButtonHoverActiveBorderBrushKey;
            Brushes.ToolWindowButtonHoverActiveGlyphBrushKey = EnvironmentColors.ToolWindowButtonHoverActiveGlyphBrushKey;
            Brushes.ToolWindowButtonHoverInactiveBrushKey = EnvironmentColors.ToolWindowButtonHoverInactiveBrushKey;
            Brushes.ToolWindowButtonHoverInactiveBorderBrushKey = EnvironmentColors.ToolWindowButtonHoverInactiveBorderBrushKey;
            Brushes.ToolWindowButtonHoverInactiveGlyphBrushKey = EnvironmentColors.ToolWindowButtonHoverInactiveGlyphBrushKey;
            Brushes.ToolWindowButtonInactiveBrushKey = EnvironmentColors.ToolWindowButtonInactiveBrushKey;
            Brushes.ToolWindowButtonInactiveBorderBrushKey = EnvironmentColors.ToolWindowButtonInactiveBorderBrushKey;
            Brushes.ToolWindowButtonInactiveGlyphBrushKey = EnvironmentColors.ToolWindowButtonInactiveGlyphBrushKey;
            Brushes.ToolWindowTextBrushKey = EnvironmentColors.ToolWindowTextBrushKey;

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
            Brushes.BackgroundBrushKey = colorResources["BackgroundBrushKey"];
            Brushes.ContentMouseOverBrushKey = colorResources["ContentMouseOverBrushKey"];
            Brushes.ContentMouseOverTextBrushKey = colorResources["ContentMouseOverTextBrushKey"];
            Brushes.ContentInactiveSelectedBrushKey = colorResources["ContentInactiveSelectedBrushKey"];
            Brushes.ContentInactiveSelectedTextBrushKey = colorResources["ContentInactiveSelectedTextBrushKey"];
            Brushes.ContentSelectedBrushKey = colorResources["ContentSelectedBrushKey"];
            Brushes.ContentSelectedTextBrushKey = colorResources["ContentSelectedTextBrushKey"];
            Brushes.ContentBrushKey = colorResources["ContentBrushKey"];

            Brushes.StatusBarBuildingBrushKey = EnvironmentColors.StatusBarBuildingBrushKey;
            Brushes.StatusBarBuildingColorKey = EnvironmentColors.StatusBarBuildingColorKey;
            Brushes.StatusBarBuildingTextBrushKey = EnvironmentColors.StatusBarBuildingTextBrushKey;
            Brushes.StatusBarBuildingTextColorKey = EnvironmentColors.StatusBarBuildingTextColorKey;
            Brushes.StatusBarDebuggingBrushKey = EnvironmentColors.StatusBarDebuggingBrushKey;
            Brushes.StatusBarDebuggingColorKey = EnvironmentColors.StatusBarDebuggingColorKey;
            Brushes.StatusBarDebuggingTextBrushKey = EnvironmentColors.StatusBarDebuggingTextBrushKey;
            Brushes.StatusBarDebuggingTextColorKey = EnvironmentColors.StatusBarDebuggingTextColorKey;
            Brushes.StatusBarDefaultBrushKey = EnvironmentColors.StatusBarDefaultBrushKey;
            Brushes.StatusBarDefaultColorKey = EnvironmentColors.StatusBarDefaultColorKey;
            Brushes.StatusBarDefaultTextBrushKey = EnvironmentColors.StatusBarDefaultTextBrushKey;
            Brushes.StatusBarDefaultTextColorKey = EnvironmentColors.StatusBarDefaultTextColorKey;
            Brushes.StatusBarHighlightBrushKey = EnvironmentColors.StatusBarHighlightBrushKey;
            Brushes.StatusBarHighlightColorKey = EnvironmentColors.StatusBarHighlightColorKey;
            Brushes.StatusBarHighlightTextBrushKey = EnvironmentColors.StatusBarHighlightTextBrushKey;
            Brushes.StatusBarHighlightTextColorKey = EnvironmentColors.StatusBarHighlightTextColorKey;
            Brushes.StatusBarNoSolutionBrushKey = EnvironmentColors.StatusBarNoSolutionBrushKey;
            Brushes.StatusBarNoSolutionColorKey = EnvironmentColors.StatusBarNoSolutionColorKey;
            Brushes.StatusBarNoSolutionTextBrushKey = EnvironmentColors.StatusBarNoSolutionTextBrushKey;
            Brushes.StatusBarNoSolutionTextColorKey = EnvironmentColors.StatusBarNoSolutionTextColorKey;
            Brushes.StatusBarTextBrushKey = EnvironmentColors.StatusBarTextBrushKey;
            Brushes.StatusBarTextColorKey = EnvironmentColors.StatusBarTextColorKey;

            Brushes.FailMessageTextBrushKey = TreeViewColors.ValidationSquigglesBrushKey;
            // TODO: may need to pick a better color of specify custom key
            Brushes.SuccessMessageTextBrushKey = ProgressBarColors.IndicatorFillBrushKey;
            Brushes.ScrollBarBackgroundBrushKey = EnvironmentColors.ScrollBarBackgroundBrushKey;

            Brushes.TreeViewBackgroundBrushKey = TreeViewColors.BackgroundBrushKey;
            Brushes.TreeViewBackgroundTextBrushKey = TreeViewColors.BackgroundTextBrushKey;
            Brushes.TreeViewSelectedItemActiveBrushKey = TreeViewColors.SelectedItemActiveBrushKey;
            Brushes.TreeViewSelectedItemActiveTextBrushKey = TreeViewColors.SelectedItemActiveTextBrushKey;
            Brushes.TreeViewSelectedItemInactiveBrushKey = TreeViewColors.SelectedItemInactiveBrushKey;
            Brushes.TreeViewSelectedItemInactiveTextBrushKey = TreeViewColors.SelectedItemInactiveTextBrushKey;
            Brushes.TreeViewGlyphBrushKey = TreeViewColors.GlyphBrushKey;
            Brushes.TreeViewGlyphMouseOverBrushKey = TreeViewColors.GlyphMouseOverBrushKey;

            Brushes.GridLineBrushKey = EnvironmentColors.GridLineBrushKey;
        }

        private static void OverrideFontKeys() {
            FontKeys.CaptionFontFamilyKey = VsFonts.CaptionFontFamilyKey;
            FontKeys.CaptionFontSizeKey = VsFonts.CaptionFontSizeKey;
            FontKeys.CaptionFontWeightKey = VsFonts.CaptionFontWeightKey;
            FontKeys.EnvironmentFontFamilyKey = VsFonts.EnvironmentFontFamilyKey;
            FontKeys.EnvironmentFontSizeKey = VsFonts.EnvironmentFontSizeKey;
            FontKeys.EnvironmentBoldFontWeightKey = VsFonts.EnvironmentBoldFontWeightKey;
        }

        private static void OverrideImageSources() {
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            ImageSources.ImageBackground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
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

        private static ImageSource GetImage(IVsImageService2 imageService, ImageMoniker imageMoniker) {
            var imageAttributes = new ImageAttributes {
                ImageType = (uint)_UIImageType.IT_Bitmap,
                Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags,
                Format = (uint)_UIDataFormat.DF_WPF,
                LogicalHeight = 16,
                LogicalWidth = 16,
                StructSize = Marshal.SizeOf(typeof(ImageAttributes))
            };

            IVsUIObject uiObject = imageService.GetImage(imageMoniker, imageAttributes);

            object data;
            if (uiObject.get_Data(out data) != VSConstants.S_OK) {
                return null;
            }

            var imageSource = data as ImageSource;
            imageSource?.Freeze();
            return imageSource;
        }
    }
}
