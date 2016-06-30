namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    partial class SettingsPageControl {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.filesList = new System.Windows.Forms.ComboBox();
            this.variableName = new System.Windows.Forms.TextBox();
            this.addSettingButton = new System.Windows.Forms.Button();
            this.variableNameLabel = new System.Windows.Forms.Label();
            this.explanationText = new System.Windows.Forms.TextBox();
            this.variableValueLabel = new System.Windows.Forms.Label();
            this.variableValue = new System.Windows.Forms.TextBox();
            this.variableTypeList = new System.Windows.Forms.ComboBox();
            this.variableTypeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Location = new System.Drawing.Point(0, 56);
            this.propertyGrid.Margin = new System.Windows.Forms.Padding(37, 37, 37, 37);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(803, 391);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // filesList
            // 
            this.filesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filesList.FormattingEnabled = true;
            this.filesList.Location = new System.Drawing.Point(0, 0);
            this.filesList.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.filesList.Name = "filesList";
            this.filesList.Size = new System.Drawing.Size(455, 32);
            this.filesList.TabIndex = 0;
            // 
            // variableName
            // 
            this.variableName.Location = new System.Drawing.Point(1, 488);
            this.variableName.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.variableName.Name = "variableName";
            this.variableName.Size = new System.Drawing.Size(132, 29);
            this.variableName.TabIndex = 3;
            // 
            // addSettingButton
            // 
            this.addSettingButton.Location = new System.Drawing.Point(665, 481);
            this.addSettingButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.addSettingButton.Name = "addSettingButton";
            this.addSettingButton.Size = new System.Drawing.Size(138, 42);
            this.addSettingButton.TabIndex = 8;
            this.addSettingButton.Text = "&Add";
            this.addSettingButton.UseVisualStyleBackColor = true;
            // 
            // variableNameLabel
            // 
            this.variableNameLabel.AutoSize = true;
            this.variableNameLabel.Location = new System.Drawing.Point(-4, 457);
            this.variableNameLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.variableNameLabel.Name = "variableNameLabel";
            this.variableNameLabel.Size = new System.Drawing.Size(70, 25);
            this.variableNameLabel.TabIndex = 2;
            this.variableNameLabel.Text = "Name:";
            // 
            // explanationText
            // 
            this.explanationText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.explanationText.Location = new System.Drawing.Point(825, 56);
            this.explanationText.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.explanationText.Multiline = true;
            this.explanationText.Name = "explanationText";
            this.explanationText.ReadOnly = true;
            this.explanationText.Size = new System.Drawing.Size(508, 391);
            this.explanationText.TabIndex = 6;
            // 
            // variableValueLabel
            // 
            this.variableValueLabel.AutoSize = true;
            this.variableValueLabel.Location = new System.Drawing.Point(159, 459);
            this.variableValueLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.variableValueLabel.Name = "variableValueLabel";
            this.variableValueLabel.Size = new System.Drawing.Size(69, 25);
            this.variableValueLabel.TabIndex = 4;
            this.variableValueLabel.Text = "Value:";
            // 
            // variableValue
            // 
            this.variableValue.Location = new System.Drawing.Point(164, 488);
            this.variableValue.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.variableValue.Name = "variableValue";
            this.variableValue.Size = new System.Drawing.Size(286, 29);
            this.variableValue.TabIndex = 5;
            // 
            // variableTypeList
            // 
            this.variableTypeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.variableTypeList.FormattingEnabled = true;
            this.variableTypeList.Location = new System.Drawing.Point(483, 486);
            this.variableTypeList.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.variableTypeList.Name = "variableTypeList";
            this.variableTypeList.Size = new System.Drawing.Size(142, 32);
            this.variableTypeList.TabIndex = 7;
            // 
            // variableTypeLabel
            // 
            this.variableTypeLabel.AutoSize = true;
            this.variableTypeLabel.Location = new System.Drawing.Point(476, 455);
            this.variableTypeLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.variableTypeLabel.Name = "variableTypeLabel";
            this.variableTypeLabel.Size = new System.Drawing.Size(63, 25);
            this.variableTypeLabel.TabIndex = 6;
            this.variableTypeLabel.Text = "Type:";
            // 
            // SettingsPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.variableTypeLabel);
            this.Controls.Add(this.variableTypeList);
            this.Controls.Add(this.variableValue);
            this.Controls.Add(this.variableValueLabel);
            this.Controls.Add(this.explanationText);
            this.Controls.Add(this.variableNameLabel);
            this.Controls.Add(this.addSettingButton);
            this.Controls.Add(this.variableName);
            this.Controls.Add(this.filesList);
            this.Controls.Add(this.propertyGrid);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MinimumSize = new System.Drawing.Size(367, 369);
            this.Name = "SettingsPageControl";
            this.Size = new System.Drawing.Size(1339, 529);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ComboBox filesList;
        private System.Windows.Forms.TextBox variableName;
        private System.Windows.Forms.Button addSettingButton;
        private System.Windows.Forms.Label variableNameLabel;
        private System.Windows.Forms.TextBox explanationText;
        private System.Windows.Forms.Label variableValueLabel;
        private System.Windows.Forms.TextBox variableValue;
        private System.Windows.Forms.ComboBox variableTypeList;
        private System.Windows.Forms.Label variableTypeLabel;
    }
}