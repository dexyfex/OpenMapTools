namespace VOAD
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.InputFolderBrowseButton = new System.Windows.Forms.Button();
            this.InputFolderTextBox = new System.Windows.Forms.TextBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.BrowseFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(178, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Convert .oad/.onim files to .ycd files.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 37;
            this.label2.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(490, 86);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.OutputFolderBrowseButton.TabIndex = 39;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(101, 87);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.OutputFolderTextBox.TabIndex = 38;
            this.OutputFolderTextBox.Text = "C:\\GitHub\\CodeWalkerResearch\\YCD\\VOADout";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 34;
            this.label1.Text = "Input folder:";
            // 
            // InputFolderBrowseButton
            // 
            this.InputFolderBrowseButton.Location = new System.Drawing.Point(490, 50);
            this.InputFolderBrowseButton.Name = "InputFolderBrowseButton";
            this.InputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.InputFolderBrowseButton.TabIndex = 36;
            this.InputFolderBrowseButton.Text = "...";
            this.InputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.InputFolderBrowseButton.Click += new System.EventHandler(this.InputFolderBrowseButton_Click);
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Location = new System.Drawing.Point(101, 51);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.InputFolderTextBox.TabIndex = 35;
            this.InputFolderTextBox.Text = "C:\\GitHub\\CodeWalkerResearch\\YCD\\VOADin";
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(101, 204);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(68, 23);
            this.ProcessButton.TabIndex = 41;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 274);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InputFolderBrowseButton);
            this.Controls.Add(this.InputFolderTextBox);
            this.Name = "Form1";
            this.Text = "VOAD by dexyfex";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button OutputFolderBrowseButton;
        private System.Windows.Forms.TextBox OutputFolderTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button InputFolderBrowseButton;
        private System.Windows.Forms.TextBox InputFolderTextBox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.FolderBrowserDialog BrowseFolderDialog;
    }
}

