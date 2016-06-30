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
            this.explanationText1 = new System.Windows.Forms.TextBox();
            this.explanationText2 = new System.Windows.Forms.TextBox();
            this.explanationText3 = new System.Windows.Forms.TextBox();
            this.variableValueLabel = new System.Windows.Forms.Label();
            this.variableValue = new System.Windows.Forms.TextBox();
            this.variableTypeList = new System.Windows.Forms.ComboBox();
            this.variableTypeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Location = new System.Drawing.Point(20, 164);
            this.propertyGrid.Margin = new System.Windows.Forms.Padding(20);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(438, 212);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // filesList
            // 
            this.filesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filesList.FormattingEnabled = true;
            this.filesList.Location = new System.Drawing.Point(20, 124);
            this.filesList.Name = "filesList";
            this.filesList.Size = new System.Drawing.Size(250, 21);
            this.filesList.TabIndex = 0;
            // 
            // variableName
            // 
            this.variableName.Location = new System.Drawing.Point(23, 413);
            this.variableName.Name = "variableName";
            this.variableName.Size = new System.Drawing.Size(74, 20);
            this.variableName.TabIndex = 3;
            // 
            // addSettingButton
            // 
            this.addSettingButton.Location = new System.Drawing.Point(380, 412);
            this.addSettingButton.Name = "addSettingButton";
            this.addSettingButton.Size = new System.Drawing.Size(75, 23);
            this.addSettingButton.TabIndex = 8;
            this.addSettingButton.Text = "&Add";
            this.addSettingButton.UseVisualStyleBackColor = true;
            // 
            // variableNameLabel
            // 
            this.variableNameLabel.AutoSize = true;
            this.variableNameLabel.Location = new System.Drawing.Point(20, 396);
            this.variableNameLabel.Name = "variableNameLabel";
            this.variableNameLabel.Size = new System.Drawing.Size(38, 13);
            this.variableNameLabel.TabIndex = 2;
            this.variableNameLabel.Text = "Name:";
            // 
            // explanationText1
            // 
            this.explanationText1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.explanationText1.Location = new System.Drawing.Point(21, 0);
            this.explanationText1.Multiline = true;
            this.explanationText1.Name = "explanationText1";
            this.explanationText1.ReadOnly = true;
            this.explanationText1.Size = new System.Drawing.Size(435, 33);
            this.explanationText1.TabIndex = 6;
            // 
            // explanationText2
            // 
            this.explanationText2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.explanationText2.Location = new System.Drawing.Point(20, 37);
            this.explanationText2.Multiline = true;
            this.explanationText2.Name = "explanationText2";
            this.explanationText2.ReadOnly = true;
            this.explanationText2.Size = new System.Drawing.Size(435, 33);
            this.explanationText2.TabIndex = 7;
            // 
            // explanationText3
            // 
            this.explanationText3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.explanationText3.Location = new System.Drawing.Point(20, 75);
            this.explanationText3.Multiline = true;
            this.explanationText3.Name = "explanationText3";
            this.explanationText3.ReadOnly = true;
            this.explanationText3.Size = new System.Drawing.Size(435, 33);
            this.explanationText3.TabIndex = 8;
            // 
            // variableValueLabel
            // 
            this.variableValueLabel.AutoSize = true;
            this.variableValueLabel.Location = new System.Drawing.Point(109, 397);
            this.variableValueLabel.Name = "variableValueLabel";
            this.variableValueLabel.Size = new System.Drawing.Size(37, 13);
            this.variableValueLabel.TabIndex = 4;
            this.variableValueLabel.Text = "Value:";
            // 
            // variableValue
            // 
            this.variableValue.Location = new System.Drawing.Point(112, 413);
            this.variableValue.Name = "variableValue";
            this.variableValue.Size = new System.Drawing.Size(158, 20);
            this.variableValue.TabIndex = 5;
            // 
            // variableTypeList
            // 
            this.variableTypeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.variableTypeList.FormattingEnabled = true;
            this.variableTypeList.Location = new System.Drawing.Point(286, 413);
            this.variableTypeList.Name = "variableTypeList";
            this.variableTypeList.Size = new System.Drawing.Size(79, 21);
            this.variableTypeList.TabIndex = 7;
            // 
            // variableTypeLabel
            // 
            this.variableTypeLabel.AutoSize = true;
            this.variableTypeLabel.Location = new System.Drawing.Point(282, 395);
            this.variableTypeLabel.Name = "variableTypeLabel";
            this.variableTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.variableTypeLabel.TabIndex = 6;
            this.variableTypeLabel.Text = "Type:";
            // 
            // SettingsPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.variableTypeLabel);
            this.Controls.Add(this.variableTypeList);
            this.Controls.Add(this.variableValue);
            this.Controls.Add(this.variableValueLabel);
            this.Controls.Add(this.explanationText3);
            this.Controls.Add(this.explanationText2);
            this.Controls.Add(this.explanationText1);
            this.Controls.Add(this.variableNameLabel);
            this.Controls.Add(this.addSettingButton);
            this.Controls.Add(this.variableName);
            this.Controls.Add(this.filesList);
            this.Controls.Add(this.propertyGrid);
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.Name = "SettingsPageControl";
            this.Size = new System.Drawing.Size(478, 438);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ComboBox filesList;
        private System.Windows.Forms.TextBox variableName;
        private System.Windows.Forms.Button addSettingButton;
        private System.Windows.Forms.Label variableNameLabel;
        private System.Windows.Forms.TextBox explanationText1;
        private System.Windows.Forms.TextBox explanationText2;
        private System.Windows.Forms.TextBox explanationText3;
        private System.Windows.Forms.Label variableValueLabel;
        private System.Windows.Forms.TextBox variableValue;
        private System.Windows.Forms.ComboBox variableTypeList;
        private System.Windows.Forms.Label variableTypeLabel;
    }
}