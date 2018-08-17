namespace VIDE
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
            this.GtxdMetaCheckBox = new System.Windows.Forms.CheckBox();
            this.IDEFormatComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.LODsCheckBox = new System.Windows.Forms.CheckBox();
            this.VOPLInteriorsCheckBox = new System.Windows.Forms.CheckBox();
            this.IDEPhysDictNamesCheckBox = new System.Windows.Forms.CheckBox();
            this.IDEClipDictsCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 13);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(499, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Convert .ide files to .ytyp.xml files...    Put extra strings for hash conversion" +
    " in strings.txt in the Input folder.\r\n";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output folder:";
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(490, 78);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.OutputFolderBrowseButton.TabIndex = 7;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Location = new System.Drawing.Point(101, 79);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.OutputFolderTextBox.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input folder:";
            // 
            // InputFolderBrowseButton
            // 
            this.InputFolderBrowseButton.Location = new System.Drawing.Point(490, 42);
            this.InputFolderBrowseButton.Name = "InputFolderBrowseButton";
            this.InputFolderBrowseButton.Size = new System.Drawing.Size(27, 22);
            this.InputFolderBrowseButton.TabIndex = 4;
            this.InputFolderBrowseButton.Text = "...";
            this.InputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.InputFolderBrowseButton.Click += new System.EventHandler(this.InputFolderBrowseButton_Click);
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Location = new System.Drawing.Point(101, 43);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(383, 20);
            this.InputFolderTextBox.TabIndex = 3;
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(101, 257);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(68, 23);
            this.ProcessButton.TabIndex = 20;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // GtxdMetaCheckBox
            // 
            this.GtxdMetaCheckBox.AutoSize = true;
            this.GtxdMetaCheckBox.Checked = true;
            this.GtxdMetaCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GtxdMetaCheckBox.Location = new System.Drawing.Point(101, 154);
            this.GtxdMetaCheckBox.Name = "GtxdMetaCheckBox";
            this.GtxdMetaCheckBox.Size = new System.Drawing.Size(107, 17);
            this.GtxdMetaCheckBox.TabIndex = 10;
            this.GtxdMetaCheckBox.Text = "Output gtxd.meta";
            this.GtxdMetaCheckBox.UseVisualStyleBackColor = true;
            // 
            // IDEFormatComboBox
            // 
            this.IDEFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.IDEFormatComboBox.FormattingEnabled = true;
            this.IDEFormatComboBox.Items.AddRange(new object[] {
            "IV"});
            this.IDEFormatComboBox.Location = new System.Drawing.Point(101, 114);
            this.IDEFormatComboBox.Name = "IDEFormatComboBox";
            this.IDEFormatComboBox.Size = new System.Drawing.Size(121, 21);
            this.IDEFormatComboBox.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "IDE format:";
            // 
            // LODsCheckBox
            // 
            this.LODsCheckBox.AutoSize = true;
            this.LODsCheckBox.Checked = true;
            this.LODsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LODsCheckBox.Location = new System.Drawing.Point(260, 154);
            this.LODsCheckBox.Name = "LODsCheckBox";
            this.LODsCheckBox.Size = new System.Drawing.Size(169, 17);
            this.LODsCheckBox.TabIndex = 11;
            this.LODsCheckBox.Text = "Split lod types to separate ytyp";
            this.LODsCheckBox.UseVisualStyleBackColor = true;
            // 
            // VOPLInteriorsCheckBox
            // 
            this.VOPLInteriorsCheckBox.AutoSize = true;
            this.VOPLInteriorsCheckBox.Checked = true;
            this.VOPLInteriorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.VOPLInteriorsCheckBox.Location = new System.Drawing.Point(101, 177);
            this.VOPLInteriorsCheckBox.Name = "VOPLInteriorsCheckBox";
            this.VOPLInteriorsCheckBox.Size = new System.Drawing.Size(323, 17);
            this.VOPLInteriorsCheckBox.TabIndex = 21;
            this.VOPLInteriorsCheckBox.Text = "Output VOPL interiors data (put those in the VOPL input folder!)";
            this.VOPLInteriorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // IDEPhysDictNamesCheckBox
            // 
            this.IDEPhysDictNamesCheckBox.AutoSize = true;
            this.IDEPhysDictNamesCheckBox.Location = new System.Drawing.Point(101, 200);
            this.IDEPhysDictNamesCheckBox.Name = "IDEPhysDictNamesCheckBox";
            this.IDEPhysDictNamesCheckBox.Size = new System.Drawing.Size(327, 17);
            this.IDEPhysDictNamesCheckBox.TabIndex = 22;
            this.IDEPhysDictNamesCheckBox.Text = "Output IDE name for physicsDictionary (for embedded collisions)";
            this.IDEPhysDictNamesCheckBox.UseVisualStyleBackColor = true;
            // 
            // IDEClipDictsCheckBox
            // 
            this.IDEClipDictsCheckBox.AutoSize = true;
            this.IDEClipDictsCheckBox.Location = new System.Drawing.Point(101, 223);
            this.IDEClipDictsCheckBox.Name = "IDEClipDictsCheckBox";
            this.IDEClipDictsCheckBox.Size = new System.Drawing.Size(219, 17);
            this.IDEClipDictsCheckBox.TabIndex = 23;
            this.IDEClipDictsCheckBox.Text = "Output clipDictionary for IDE anim entries";
            this.IDEClipDictsCheckBox.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 302);
            this.Controls.Add(this.IDEClipDictsCheckBox);
            this.Controls.Add(this.IDEPhysDictNamesCheckBox);
            this.Controls.Add(this.VOPLInteriorsCheckBox);
            this.Controls.Add(this.LODsCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.IDEFormatComboBox);
            this.Controls.Add(this.GtxdMetaCheckBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InputFolderBrowseButton);
            this.Controls.Add(this.InputFolderTextBox);
            this.Controls.Add(this.ProcessButton);
            this.Name = "Form1";
            this.Text = "VIDE by dexyfex";
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
        private System.Windows.Forms.CheckBox GtxdMetaCheckBox;
        private System.Windows.Forms.ComboBox IDEFormatComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox LODsCheckBox;
        private System.Windows.Forms.CheckBox VOPLInteriorsCheckBox;
        private System.Windows.Forms.CheckBox IDEPhysDictNamesCheckBox;
        private System.Windows.Forms.CheckBox IDEClipDictsCheckBox;
    }
}

