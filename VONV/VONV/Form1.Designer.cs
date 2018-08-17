namespace VONV
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
            this.label7 = new System.Windows.Forms.Label();
            this.OffsetZTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.OffsetYTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.OffsetXTextBox = new System.Windows.Forms.TextBox();
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
            this.label8.Location = new System.Drawing.Point(37, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(148, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "Convert .onv files to .ynv files.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(332, 158);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "Z:";
            // 
            // OffsetZTextBox
            // 
            this.OffsetZTextBox.Location = new System.Drawing.Point(355, 155);
            this.OffsetZTextBox.Name = "OffsetZTextBox";
            this.OffsetZTextBox.Size = new System.Drawing.Size(68, 20);
            this.OffsetZTextBox.TabIndex = 39;
            this.OffsetZTextBox.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(208, 158);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 13);
            this.label6.TabIndex = 36;
            this.label6.Text = "Y:";
            // 
            // OffsetYTextBox
            // 
            this.OffsetYTextBox.Location = new System.Drawing.Point(231, 155);
            this.OffsetYTextBox.Name = "OffsetYTextBox";
            this.OffsetYTextBox.Size = new System.Drawing.Size(68, 20);
            this.OffsetYTextBox.TabIndex = 37;
            this.OffsetYTextBox.Text = "-3256.298";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(89, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 13);
            this.label5.TabIndex = 34;
            this.label5.Text = "X:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(34, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 33;
            this.label4.Text = "Offset:";
            // 
            // OffsetXTextBox
            // 
            this.OffsetXTextBox.Location = new System.Drawing.Point(112, 155);
            this.OffsetXTextBox.Name = "OffsetXTextBox";
            this.OffsetXTextBox.Size = new System.Drawing.Size(68, 20);
            this.OffsetXTextBox.TabIndex = 35;
            this.OffsetXTextBox.Text = "5188.185";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(501, 79);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.OutputFolderBrowseButton.TabIndex = 32;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(112, 80);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.OutputFolderTextBox.TabIndex = 31;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Input folder:";
            // 
            // InputFolderBrowseButton
            // 
            this.InputFolderBrowseButton.Location = new System.Drawing.Point(501, 43);
            this.InputFolderBrowseButton.Name = "InputFolderBrowseButton";
            this.InputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.InputFolderBrowseButton.TabIndex = 29;
            this.InputFolderBrowseButton.Text = "...";
            this.InputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.InputFolderBrowseButton.Click += new System.EventHandler(this.InputFolderBrowseButton_Click);
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Location = new System.Drawing.Point(112, 44);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.InputFolderTextBox.TabIndex = 28;
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(112, 233);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(68, 23);
            this.ProcessButton.TabIndex = 40;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 289);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.OffsetZTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.OffsetYTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.OffsetXTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InputFolderBrowseButton);
            this.Controls.Add(this.InputFolderTextBox);
            this.Controls.Add(this.ProcessButton);
            this.Name = "Form1";
            this.Text = "VONV by dexyfex";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox OffsetZTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox OffsetYTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox OffsetXTextBox;
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

