namespace VIPL
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
            this.ProcessButton = new System.Windows.Forms.Button();
            this.BrowseFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.InputFolderBrowseButton = new System.Windows.Forms.Button();
            this.InputFolderTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.HDLODDistTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.OffsetXTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.OffsetYTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.OffsetZTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.CarGenFlagsTextBox = new System.Windows.Forms.TextBox();
            this.CarGeneratorsCheckBox = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.SLODLODDistTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.LODLODDistTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(103, 245);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(68, 23);
            this.ProcessButton.TabIndex = 25;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input folder:";
            // 
            // InputFolderBrowseButton
            // 
            this.InputFolderBrowseButton.Location = new System.Drawing.Point(492, 46);
            this.InputFolderBrowseButton.Name = "InputFolderBrowseButton";
            this.InputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.InputFolderBrowseButton.TabIndex = 4;
            this.InputFolderBrowseButton.Text = "...";
            this.InputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.InputFolderBrowseButton.Click += new System.EventHandler(this.InputFolderBrowseButton_Click);
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Location = new System.Drawing.Point(103, 47);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.InputFolderTextBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(492, 82);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.OutputFolderBrowseButton.TabIndex = 7;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(103, 83);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.OutputFolderTextBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "LOD Dist:";
            // 
            // HDLODDistTextBox
            // 
            this.HDLODDistTextBox.Location = new System.Drawing.Point(103, 127);
            this.HDLODDistTextBox.Name = "HDLODDistTextBox";
            this.HDLODDistTextBox.Size = new System.Drawing.Size(68, 20);
            this.HDLODDistTextBox.TabIndex = 10;
            this.HDLODDistTextBox.Text = "200";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 168);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Offset:";
            // 
            // OffsetXTextBox
            // 
            this.OffsetXTextBox.Location = new System.Drawing.Point(103, 165);
            this.OffsetXTextBox.Name = "OffsetXTextBox";
            this.OffsetXTextBox.Size = new System.Drawing.Size(68, 20);
            this.OffsetXTextBox.TabIndex = 17;
            this.OffsetXTextBox.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(80, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "X:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(199, 168);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Y:";
            // 
            // OffsetYTextBox
            // 
            this.OffsetYTextBox.Location = new System.Drawing.Point(222, 165);
            this.OffsetYTextBox.Name = "OffsetYTextBox";
            this.OffsetYTextBox.Size = new System.Drawing.Size(68, 20);
            this.OffsetYTextBox.TabIndex = 19;
            this.OffsetYTextBox.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(323, 168);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Z:";
            // 
            // OffsetZTextBox
            // 
            this.OffsetZTextBox.Location = new System.Drawing.Point(346, 165);
            this.OffsetZTextBox.Name = "OffsetZTextBox";
            this.OffsetZTextBox.Size = new System.Drawing.Size(68, 20);
            this.OffsetZTextBox.TabIndex = 21;
            this.OffsetZTextBox.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(238, 199);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(102, 13);
            this.label12.TabIndex = 23;
            this.label12.Text = "Car generator Flags:";
            // 
            // CarGenFlagsTextBox
            // 
            this.CarGenFlagsTextBox.Location = new System.Drawing.Point(346, 196);
            this.CarGenFlagsTextBox.Name = "CarGenFlagsTextBox";
            this.CarGenFlagsTextBox.Size = new System.Drawing.Size(68, 20);
            this.CarGenFlagsTextBox.TabIndex = 24;
            this.CarGenFlagsTextBox.Text = "3680";
            // 
            // CarGeneratorsCheckBox
            // 
            this.CarGeneratorsCheckBox.AutoSize = true;
            this.CarGeneratorsCheckBox.Checked = true;
            this.CarGeneratorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CarGeneratorsCheckBox.Location = new System.Drawing.Point(103, 198);
            this.CarGeneratorsCheckBox.Name = "CarGeneratorsCheckBox";
            this.CarGeneratorsCheckBox.Size = new System.Drawing.Size(95, 17);
            this.CarGeneratorsCheckBox.TabIndex = 22;
            this.CarGeneratorsCheckBox.Text = "Car generators";
            this.CarGeneratorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(301, 130);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(39, 13);
            this.label11.TabIndex = 13;
            this.label11.Text = "SLOD:";
            // 
            // SLODLODDistTextBox
            // 
            this.SLODLODDistTextBox.Location = new System.Drawing.Point(346, 127);
            this.SLODLODDistTextBox.Name = "SLODLODDistTextBox";
            this.SLODLODDistTextBox.Size = new System.Drawing.Size(68, 20);
            this.SLODLODDistTextBox.TabIndex = 14;
            this.SLODLODDistTextBox.Text = "20000";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(71, 130);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "HD:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(184, 130);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "LOD:";
            // 
            // LODLODDistTextBox
            // 
            this.LODLODDistTextBox.Location = new System.Drawing.Point(222, 127);
            this.LODLODDistTextBox.Name = "LODLODDistTextBox";
            this.LODLODDistTextBox.Size = new System.Drawing.Size(68, 20);
            this.LODLODDistTextBox.TabIndex = 12;
            this.LODLODDistTextBox.Text = "2000";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(71, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(172, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Convert .ipl files to .ymap.xml files...";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 286);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.CarGenFlagsTextBox);
            this.Controls.Add(this.CarGeneratorsCheckBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.SLODLODDistTextBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.LODLODDistTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.OffsetZTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.OffsetYTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.OffsetXTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.HDLODDistTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InputFolderBrowseButton);
            this.Controls.Add(this.InputFolderTextBox);
            this.Controls.Add(this.ProcessButton);
            this.Name = "Form1";
            this.Text = "VIPL by dexyfex";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.FolderBrowserDialog BrowseFolderDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button InputFolderBrowseButton;
        private System.Windows.Forms.TextBox InputFolderTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button OutputFolderBrowseButton;
        private System.Windows.Forms.TextBox OutputFolderTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox HDLODDistTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox OffsetXTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox OffsetYTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox OffsetZTextBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox CarGenFlagsTextBox;
        private System.Windows.Forms.CheckBox CarGeneratorsCheckBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox SLODLODDistTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox LODLODDistTextBox;
        private System.Windows.Forms.Label label8;
    }
}

