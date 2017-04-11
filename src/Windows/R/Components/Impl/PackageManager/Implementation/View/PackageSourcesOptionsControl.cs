using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.R.Components.PackageManager.Implementation.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    public partial class PackageSourcesOptionsControl : UserControl {
        private Size _checkBoxSize;

        public PackageSourcesOptionsControl() {
            InitializeComponent();
            UpdateDpi();
        }

        private void UpdateDpi() {
            var imgs = images16px;
            if (addButton.Height > 72) {
                imgs = images64px;
            } else if (addButton.Height > 40) {
                imgs = images32px;
            }

            addButton.ImageList = imgs;
            removeButton.ImageList = imgs;
            MoveUpButton.ImageList = imgs;
            MoveDownButton.ImageList = imgs;
        }


        private void PackageSourcesContextMenu_ItemClick(object sender, ToolStripItemClickedEventArgs e) {
            throw new NotImplementedException();
        }

        private void AddButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void RemoveButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void UpdateButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void BrowseButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void PackageSourcesListBox_DrawItem(object sender, DrawItemEventArgs e) {
            var currentListBox = (ListBox)sender;
            Graphics graphics = e.Graphics;
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= currentListBox.Items.Count) {
                return;
            }

            var currentItem = (RPackageSourceViewModel)currentListBox.Items[e.Index];

            using (var drawFormat = new StringFormat()) {
                using (var foreBrush = new SolidBrush(currentListBox.SelectionMode == SelectionMode.None ? SystemColors.WindowText : e.ForeColor)) {
                    drawFormat.Alignment = StringAlignment.Near;
                    drawFormat.Trimming = StringTrimming.EllipsisCharacter;
                    drawFormat.LineAlignment = StringAlignment.Near;
                    drawFormat.FormatFlags = StringFormatFlags.NoWrap;

                    // the margin between the checkbox and the edge of the list box
                    const int edgeMargin = 8;
                    // the margin between the checkbox and the text
                    const int textMargin = 4;

                    // draw the enabled/disabled checkbox
                    CheckBoxState checkBoxState = currentItem.IsEnabled ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    Size checkBoxSize = CheckBoxRenderer.GetGlyphSize(graphics, checkBoxState);
                    CheckBoxRenderer.DrawCheckBox(
                        graphics,
                        new Point(edgeMargin, e.Bounds.Top + edgeMargin),
                        checkBoxState);

                    if (_checkBoxSize.IsEmpty) {
                        // save the checkbox size so that we can detect mouse click on the 
                        // checkbox in the MouseUp event handler.
                        // here we assume that all checkboxes have the same size, which is reasonable. 
                        _checkBoxSize = checkBoxSize;
                    }

                    GraphicsState oldState = graphics.Save();
                    try {
                        // turn on high quality text rendering mode
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                        // draw each package source as
                        // 
                        // [checkbox] Name
                        //            Source (italics)

                        int textWidth = e.Bounds.Width - checkBoxSize.Width - edgeMargin - textMargin;

                        SizeF nameSize = graphics.MeasureString(currentItem.Name, e.Font, textWidth, drawFormat);

                        // resize the bound rectangle to make room for the checkbox above
                        var nameBounds = new Rectangle(
                            e.Bounds.Left + checkBoxSize.Width + edgeMargin + textMargin,
                            e.Bounds.Top,
                            textWidth,
                            (int)nameSize.Height);

                        graphics.DrawString(currentItem.Name, e.Font, foreBrush, nameBounds, drawFormat);

                        var sourceBounds = new Rectangle(
                            nameBounds.Left,
                            nameBounds.Bottom,
                            textWidth,
                            e.Bounds.Bottom - nameBounds.Bottom);
                        graphics.DrawString(currentItem.Source, e.Font, foreBrush, sourceBounds, drawFormat);
                    } finally {
                        graphics.Restore(oldState);
                    }

                    // If the ListBox has focus, draw a focus rectangle around the selected item.
                    e.DrawFocusRectangle();
                }
            }
        }

        private void PackageSourcesListBox_MeasureItem(object sender, MeasureItemEventArgs e) {
            var currentListBox = (ListBox)sender;
            if (e.Index < 0
                || e.Index >= currentListBox.Items.Count) {
                return;
            }

            var currentItem = (RPackageSourceViewModel)currentListBox.Items[e.Index];
            using (StringFormat drawFormat = new StringFormat()) {
                using (Font italicFont = new Font(Font, FontStyle.Italic)) {
                    drawFormat.Alignment = StringAlignment.Near;
                    drawFormat.Trimming = StringTrimming.EllipsisCharacter;
                    drawFormat.LineAlignment = StringAlignment.Near;
                    drawFormat.FormatFlags = StringFormatFlags.NoWrap;

                    SizeF nameLineHeight = e.Graphics.MeasureString(currentItem.Name, Font, e.ItemWidth, drawFormat);
                    SizeF sourceLineHeight = e.Graphics.MeasureString(currentItem.Source, italicFont, e.ItemWidth, drawFormat);

                    e.ItemHeight = (int)Math.Ceiling(nameLineHeight.Height + sourceLineHeight.Height);
                }
            }
        }

        private void PackageSourcesListBox_KeyUp(object sender, KeyEventArgs e) {
            var currentListBox = (ListBox)sender;
            if (e.KeyCode == Keys.C && e.Control) {
                ((RPackageSourceViewModel)currentListBox.SelectedItem).CopyToClipboard();
                e.Handled = true;
            } else if (e.KeyCode == Keys.Space) {
                TogglePackageSourceEnabled(currentListBox.SelectedIndex, currentListBox);
                e.Handled = true;
            }
        }

        private void PackageSourcesListBox_MouseMove(object sender, MouseEventArgs e) {
            var currentListBox = (ListBox)sender;
            int index = currentListBox.IndexFromPoint(e.X, e.Y);

            if (index >= 0
                && index < currentListBox.Items.Count
                && e.Y <= currentListBox.PreferredHeight) {
                var source = (RPackageSourceViewModel)currentListBox.Items[index];
                string newToolTip = source.Source;
                string currentToolTip = packageListToolTip.GetToolTip(currentListBox);
                if (currentToolTip != newToolTip) {
                    packageListToolTip.SetToolTip(currentListBox, newToolTip);
                }
            } else {
                packageListToolTip.SetToolTip(currentListBox, null);
                packageListToolTip.Hide(currentListBox);
            }
        }

        private void PackageSourcesListBox_MouseUp(object sender, MouseEventArgs e) {
            var currentListBox = (ListBox)sender;
            if (e.Button == MouseButtons.Right) {
                currentListBox.SelectedIndex = currentListBox.IndexFromPoint(e.Location);
            } else if (e.Button == MouseButtons.Left) {
                int itemIndex = currentListBox.SelectedIndex;
                if (itemIndex >= 0
                    && itemIndex < currentListBox.Items.Count) {
                    Rectangle checkBoxRectangle = GetCheckBoxRectangleForListBoxItem(currentListBox, itemIndex);
                    // if the mouse click position is inside the checkbox, toggle the IsEnabled property
                    if (checkBoxRectangle.Contains(e.Location)) {
                        TogglePackageSourceEnabled(itemIndex, currentListBox);
                    }
                }
            }
        }

        private void TogglePackageSourceEnabled(int itemIndex, ListBox currentListBox) {
            if (itemIndex < 0
                || itemIndex >= currentListBox.Items.Count) {
                return;
            }

            var item = (RPackageSourceViewModel)currentListBox.Items[itemIndex];
            item.IsEnabled = !item.IsEnabled;

            currentListBox.Invalidate(GetCheckBoxRectangleForListBoxItem(currentListBox, itemIndex));
        }

        private Rectangle GetCheckBoxRectangleForListBoxItem(ListBox currentListBox, int itemIndex) {
            const int edgeMargin = 8;

            Rectangle itemRectangle = currentListBox.GetItemRectangle(itemIndex);

            // this is the bound of the checkbox
            var checkBoxRectangle = new Rectangle(
                itemRectangle.Left + edgeMargin + 2,
                itemRectangle.Top + edgeMargin,
                _checkBoxSize.Width,
                _checkBoxSize.Height);

            return checkBoxRectangle;
        }
    }
}
