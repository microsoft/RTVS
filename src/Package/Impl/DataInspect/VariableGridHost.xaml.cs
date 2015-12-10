using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Interaction logic for VariableGridHost.xaml
    /// </summary>
    public partial class VariableGridHost : UserControl {
        public VariableGridHost() {
            InitializeComponent();
        }

        public IList RowHeaderSource { get; set; }

        public IList ColumnHeaderSource { get; set; }

        internal void SetEvaluation(EvaluationWrapper evaluation) {
            var rowPageManager = new PageManager<string>(
                new HeaderProvider(evaluation, true),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            var columnPageManager = new PageManager<string>(
                new HeaderProvider(evaluation, false),
                64,
                TimeSpan.FromMinutes(1.0),
                4);

            var pageManager = new Page2DManager<string>(
                new ItemsProvider(evaluation),
                64,
                TimeSpan.FromMinutes(1.0),
                4);


            this.VariableGrid.RowHeaderSource = new DelegateList<PageItem<string>>(0, (i) => rowPageManager.GetItem(i), rowPageManager.Count);
            this.VariableGrid.ColumnHeaderSource = new DelegateList<PageItem<string>>(0, (i) => columnPageManager.GetItem(i), columnPageManager.Count);
            this.VariableGrid.ItemsSource = pageManager.GetItemsSource();
        }
    }

    internal class HeaderProvider : IListProvider<string> {

        private EvaluationWrapper _evaluatiion;
        private bool _isRow;

        public HeaderProvider(EvaluationWrapper evaluation, bool isRow) {
            _evaluatiion = evaluation;
            _isRow = isRow;
            if (isRow) {
                Count = evaluation.Dimensions[0];
            } else {
                Count = evaluation.Dimensions[1];
            }
        }

        public int Count { get; }

        public async Task<IList<string>> GetRangeAsync(Range range) {
            await TaskUtilities.SwitchToBackgroundThread();

            string rRange = RangeToRString(range);

            var result = await VariableProvider.Current.EvaluateGridHeaderAsync(_evaluatiion.Name, rRange, _isRow);

            if (result.Headers == null || result.Headers.Count == 0) {
                return IndexedHeader(range, _isRow);
            } else {
                return new List<string>(result.Headers);
            }
        }

        private static string RangeToRString(Range range) {
            return $"{range.Start + 1}:{range.Start + range.Count}";
        }

        private static List<string> IndexedHeader(Range range, bool isRow) {
            List<string> header = new List<string>();
            if (isRow) {
                for (int i = range.Start; i < range.Start + range.Count; i++) {
                    header.Add($"[{i},]");
                }
            } else {
                for (int i = range.Start; i < range.Start + range.Count; i++) {
                    header.Add($"[,{i}]");
                }
            }
            return header;
        }
    }

    internal class ItemsProvider : IGridProvider<string> {

        private EvaluationWrapper _evaluation;

        public ItemsProvider(EvaluationWrapper evaluation) {
            _evaluation = evaluation;
            RowCount = evaluation.Dimensions[0];
            ColumnCount = evaluation.Dimensions[1];
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public async Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            await TaskUtilities.SwitchToBackgroundThread();

            string rows = RangeToRString(gridRange.Rows);
            string cols = RangeToRString(gridRange.Columns);

            var result = await VariableProvider.Current.EvaluateGridDataAsync(_evaluation.Name, rows, cols);

            if (result["matrix"] != null) {
                JArray array = result["matrix"] as JArray;

                List<string> list = new List<string>();
                foreach (var token in array) {
                    list.Add(token.Value<string>());
                }

                return new Grid<string>(gridRange.Rows.Count, gridRange.Columns.Count, list);
            } else if (result["dataframe"] != null) {
                JObject obj = result["dataframe"] as JObject;

                List<string> list = new List<string>();
                foreach (var token in obj.Values()) {
                    JArray array = token.Value<JArray>();

                    foreach (var arrayToken in array) {
                        list.Add(arrayToken.Value<string>());
                    }
                }

                return new Grid<string>(gridRange.Rows.Count, gridRange.Columns.Count, list);
            } else {
                throw new NotSupportedException("Evaluation result is not understood");
            }
        }

        private static string RangeToRString(Range range) {
            return $"{range.Start + 1}:{range.Start + range.Count}";
        }
    }
}
