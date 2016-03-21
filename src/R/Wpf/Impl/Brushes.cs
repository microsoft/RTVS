using System.Windows;

namespace Microsoft.R.Wpf {
    public static class Brushes {
        public static object ActiveBorderKey { get; set; } = SystemColors.ActiveBorderBrushKey;
        public static object BackgroundBrushKey { get; set; } = SystemColors.WindowBrushKey;
        public static object BorderBrush { get; set; } = SystemColors.InactiveBorderBrushKey;
        public static object ComboBoxBorderKey { get; set; } = SystemColors.InactiveBorderBrushKey;
        public static object ContentBrushKey { get; set; } = SystemColors.WindowBrushKey;
        public static object ContentInactiveSelectedBrushKey { get; set; } = SystemColors.ControlTextBrushKey;
        public static object ContentInactiveSelectedTextBrushKey { get; set; } = SystemColors.ControlTextBrushKey;
        public static object ContentMouseOverBrushKey { get; set; } = SystemColors.ControlTextBrushKey;
        public static object ContentMouseOverTextBrushKey { get; set; } = SystemColors.ControlTextBrushKey;
        public static object ContentSelectedBrushKey { get; set; } = SystemColors.ActiveCaptionBrushKey;
        public static object ContentSelectedTextBrushKey { get; set; } = SystemColors.ActiveCaptionTextBrushKey;
        public static object ControlLinkTextHoverKey { get; set; } = SystemColors.HighlightBrushKey;
        public static object ControlLinkTextKey { get; set; } = SystemColors.HighlightBrushKey;
        public static object DetailPaneBackground { get; set; } = SystemColors.WindowBrushKey;
        public static object HeaderBackground { get; set; } = SystemColors.WindowBrushKey;
        public static object HeaderColorsDefaultBrushKey { get; set; } = SystemColors.WindowBrushKey;
        public static object HeaderColorsDefaultTextBrushKey { get; set; } = SystemColors.WindowTextBrushKey;
        public static object HeaderColorsMouseDownBrushKey { get; set; } = SystemColors.WindowBrushKey;
        public static object HeaderColorsMouseDownTextBrushKey { get; set; } = SystemColors.WindowTextBrushKey;
        public static object HeaderColorsMouseOverBrushKey { get; set; } = SystemColors.WindowBrushKey;
        public static object HeaderColorsMouseOverTextBrushKey { get; set; } = SystemColors.WindowTextBrushKey;
        public static object HeaderColorsSeparatorLineBrushKey { get; set; } = SystemColors.ActiveBorderBrushKey;
        public static object IndicatorFillBrushKey { get; set; } = SystemColors.WindowFrameColor;
        public static object InfoBackgroundKey { get; set; } = SystemColors.InfoBrushKey;
        public static object InfoTextKey { get; set; } = SystemColors.InfoTextBrushKey;
        public static object LegalMessageBackground { get; set; } = SystemColors.ControlBrushKey;
        public static object ListPaneBackground { get; set; } = SystemColors.WindowBrushKey;
        public static object SplitterBackgroundKey { get; set; } = SystemColors.WindowBrushKey;
        public static object ToolWindowBorderKey { get; set; } = SystemColors.WindowBrushKey;
        public static object ToolWindowButtonDownBorderKey { get; set; } = SystemColors.WindowBrushKey;
        public static object ToolWindowButtonDownKey { get; set; } = SystemColors.WindowBrushKey;
        public static object ToolWindowButtonHoverActiveBorderKey { get; set; } = SystemColors.WindowBrushKey;
        public static object ToolWindowButtonHoverActiveKey { get; set; } = SystemColors.WindowBrushKey;
        public static object UIText { get; set; } = SystemColors.ControlTextBrushKey;
        public static object WindowTextKey { get; set; } = SystemColors.WindowTextBrushKey;

        //public static void LoadVsBrushes() {
        //    ActiveBorderKey = VsBrushes.ActiveBorderKey;
        //    BorderBrush = VsBrushes.BrandedUIBorderKey;
        //    ComboBoxBorderKey = VsBrushes.ComboBoxBorderKey;
        //    ControlLinkTextHoverKey = VsBrushes.ControlLinkTextHoverKey;
        //    ControlLinkTextKey = VsBrushes.ControlLinkTextKey;
        //    DetailPaneBackground = VsBrushes.BrandedUIBackgroundKey;
        //    HeaderBackground = VsBrushes.BrandedUIBackgroundKey;
        //    InfoBackgroundKey = VsBrushes.InfoBackgroundKey;
        //    InfoTextKey = VsBrushes.InfoTextKey;
        //    LegalMessageBackground = VsBrushes.BrandedUIBackgroundKey;
        //    ListPaneBackground = VsBrushes.BrandedUIBackgroundKey;
        //    SplitterBackgroundKey = VsBrushes.CommandShelfBackgroundGradientKey;
        //    ToolWindowBorderKey = VsBrushes.ToolWindowBorderKey;
        //    ToolWindowButtonDownBorderKey = VsBrushes.ToolWindowButtonDownBorderKey;
        //    ToolWindowButtonDownKey = VsBrushes.ToolWindowButtonDownKey;
        //    ToolWindowButtonHoverActiveBorderKey = VsBrushes.ToolWindowButtonHoverActiveBorderKey;
        //    ToolWindowButtonHoverActiveKey = VsBrushes.ToolWindowButtonHoverActiveKey;
        //    UIText = VsBrushes.BrandedUITextKey;
        //    WindowTextKey = VsBrushes.WindowTextKey;

        //    HeaderColorsDefaultBrushKey = HeaderColors.DefaultBrushKey;
        //    HeaderColorsDefaultTextBrushKey = HeaderColors.DefaultTextBrushKey;
        //    HeaderColorsMouseDownBrushKey = HeaderColors.MouseDownBrushKey;
        //    HeaderColorsMouseDownTextBrushKey = HeaderColors.MouseDownTextBrushKey;
        //    HeaderColorsMouseOverBrushKey = HeaderColors.MouseOverBrushKey;
        //    HeaderColorsMouseOverTextBrushKey = HeaderColors.MouseOverTextBrushKey;
        //    HeaderColorsSeparatorLineBrushKey = HeaderColors.SeparatorLineBrushKey;

        //    IndicatorFillBrushKey = ProgressBarColors.IndicatorFillBrushKey;

        //    var colorResources = GetColorResources();
        //    BackgroundBrushKey = colorResources["BackgroundBrushKey"];
        //    ContentMouseOverBrushKey = colorResources["ContentMouseOverBrushKey"];
        //    ContentMouseOverTextBrushKey = colorResources["ContentMouseOverTextBrushKey"];
        //    ContentInactiveSelectedBrushKey = colorResources["ContentInactiveSelectedBrushKey"];
        //    ContentInactiveSelectedTextBrushKey = colorResources["ContentInactiveSelectedTextBrushKey"];
        //    ContentSelectedBrushKey = colorResources["ContentSelectedBrushKey"];
        //    ContentSelectedTextBrushKey = colorResources["ContentSelectedTextBrushKey"];
        //    ContentBrushKey = colorResources["ContentBrushKey"];
        //}

        //private static IDictionary<string, ThemeResourceKey> GetColorResources() {
        //    // use colors of VisualStudio UI.
        //    var assembly = AppDomain.CurrentDomain.Load(
        //        "Microsoft.VisualStudio.ExtensionsExplorer.UI");
        //    var colorResources = assembly.GetType(
        //        "Microsoft.VisualStudio.ExtensionsExplorer.UI.ColorResources");

        //    var properties = colorResources.GetProperties(BindingFlags.Public | BindingFlags.Static);
        //    return properties
        //        .Where(p => p.PropertyType == typeof(ThemeResourceKey))
        //        .ToDictionary(p => p.Name, p => (ThemeResourceKey)p.GetValue(null));
        //}
    }
}
