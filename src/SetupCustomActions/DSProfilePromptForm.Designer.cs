namespace SetupCustomActions {
    partial class DSProfilePromptForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DSProfilePromptForm));
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.PromptText = new System.Windows.Forms.Label();
            this.keepShortcuts = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonYes
            // 
            this.buttonYes.BackColor = System.Drawing.Color.Black;
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonYes.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonYes.ForeColor = System.Drawing.Color.Silver;
            this.buttonYes.Location = new System.Drawing.Point(344, 426);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(86, 30);
            this.buttonYes.TabIndex = 0;
            this.buttonYes.Text = "Yes";
            this.buttonYes.UseVisualStyleBackColor = false;
            this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
            // 
            // buttonNo
            // 
            this.buttonNo.BackColor = System.Drawing.Color.Black;
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonNo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonNo.ForeColor = System.Drawing.Color.Silver;
            this.buttonNo.Location = new System.Drawing.Point(465, 426);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(89, 30);
            this.buttonNo.TabIndex = 1;
            this.buttonNo.Text = "No";
            this.buttonNo.UseVisualStyleBackColor = false;
            this.buttonNo.Click += new System.EventHandler(this.buttonNo_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(18, 18);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(536, 328);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // PromptText
            // 
            this.PromptText.AutoSize = true;
            this.PromptText.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.PromptText.ForeColor = System.Drawing.Color.Silver;
            this.PromptText.Location = new System.Drawing.Point(15, 365);
            this.PromptText.Name = "PromptText";
            this.PromptText.Size = new System.Drawing.Size(492, 51);
            this.PromptText.TabIndex = 3;
            this.PromptText.Text = resources.GetString("PromptText.Text");
            // 
            // keepShortcuts
            // 
            this.keepShortcuts.AutoSize = true;
            this.keepShortcuts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.keepShortcuts.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.keepShortcuts.ForeColor = System.Drawing.Color.Silver;
            this.keepShortcuts.Location = new System.Drawing.Point(18, 430);
            this.keepShortcuts.Name = "keepShortcuts";
            this.keepShortcuts.Size = new System.Drawing.Size(281, 21);
            this.keepShortcuts.TabIndex = 4;
            this.keepShortcuts.Text = "Keep existing keyboard shotcuts unchanged";
            this.keepShortcuts.UseVisualStyleBackColor = true;
            this.keepShortcuts.CheckedChanged += new System.EventHandler(this.keepShorcuts_CheckedChanged);
            // 
            // DSProfilePromptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(574, 476);
            this.Controls.Add(this.keepShortcuts);
            this.Controls.Add(this.PromptText);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Name = "DSProfilePromptForm";
            this.Text = "R Tools for Visual Studio";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label PromptText;
        private System.Windows.Forms.CheckBox keepShortcuts;
    }
}