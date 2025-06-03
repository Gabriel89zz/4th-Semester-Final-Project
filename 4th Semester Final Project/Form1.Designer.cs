namespace _4th_Semester_Final_Project
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtData = new TextBox();
            dgvData = new DataGridView();
            treeViewMovies = new TreeView();
            graphic = new ScottPlot.WinForms.FormsPlot();
            btnOpen = new Button();
            label1 = new Label();
            txtFilter = new TextBox();
            btnSave = new Button();
            btnExport = new Button();
            btnCreate = new Button();
            cmbFilter = new ComboBox();
            btnSendByEmail = new Button();
            btnLoadToAPI = new Button();
            txtAddressee = new TextBox();
            label2 = new Label();
            btnBD = new Button();
            btnSaveInBD = new Button();
            lblRecordCount = new Label();
            btnShowTreeview = new Button();
            cmbMovieType = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            SuspendLayout();
            // 
            // txtData
            // 
            txtData.Location = new Point(27, 104);
            txtData.Multiline = true;
            txtData.Name = "txtData";
            txtData.Size = new Size(785, 285);
            txtData.TabIndex = 0;
            // 
            // dgvData
            // 
            dgvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Location = new Point(27, 421);
            dgvData.Name = "dgvData";
            dgvData.Size = new Size(785, 285);
            dgvData.TabIndex = 1;
            dgvData.SelectionChanged += dgvData_SelectionChanged;
            // 
            // treeViewMovies
            // 
            treeViewMovies.Anchor = AnchorStyles.Right;
            treeViewMovies.Location = new Point(842, 104);
            treeViewMovies.Name = "treeViewMovies";
            treeViewMovies.Size = new Size(343, 285);
            treeViewMovies.TabIndex = 2;
            // 
            // graphic
            // 
            graphic.DisplayScale = 1F;
            graphic.Location = new Point(818, 412);
            graphic.Name = "graphic";
            graphic.Size = new Size(385, 304);
            graphic.TabIndex = 3;
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(328, 14);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(98, 33);
            btnOpen.TabIndex = 4;
            btnOpen.Text = "Open";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(27, 9);
            label1.Name = "label1";
            label1.Size = new Size(52, 15);
            label1.TabIndex = 5;
            label1.Text = "Filter by:";
            // 
            // txtFilter
            // 
            txtFilter.Font = new Font("Segoe UI", 14F);
            txtFilter.Location = new Point(27, 56);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(251, 32);
            txtFilter.TabIndex = 6;
            txtFilter.TextChanged += textBox1_TextChanged;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(444, 14);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(98, 33);
            btnSave.TabIndex = 7;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(328, 63);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(98, 33);
            btnExport.TabIndex = 9;
            btnExport.Text = "Export";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnCreate
            // 
            btnCreate.Location = new Point(444, 63);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(98, 33);
            btnCreate.TabIndex = 10;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // cmbFilter
            // 
            cmbFilter.FormattingEnabled = true;
            cmbFilter.Location = new Point(27, 27);
            cmbFilter.Name = "cmbFilter";
            cmbFilter.Size = new Size(121, 23);
            cmbFilter.TabIndex = 11;
            // 
            // btnSendByEmail
            // 
            btnSendByEmail.Location = new Point(1105, 44);
            btnSendByEmail.Name = "btnSendByEmail";
            btnSendByEmail.Size = new Size(98, 33);
            btnSendByEmail.TabIndex = 12;
            btnSendByEmail.Text = "Send";
            btnSendByEmail.UseVisualStyleBackColor = true;
            btnSendByEmail.Click += btnEmail_Click;
            // 
            // btnLoadToAPI
            // 
            btnLoadToAPI.Location = new Point(602, 14);
            btnLoadToAPI.Name = "btnLoadToAPI";
            btnLoadToAPI.Size = new Size(98, 33);
            btnLoadToAPI.TabIndex = 13;
            btnLoadToAPI.Text = "API";
            btnLoadToAPI.UseVisualStyleBackColor = true;
            btnLoadToAPI.Click += btnLoadToAPI_Click;
            // 
            // txtAddressee
            // 
            txtAddressee.Font = new Font("Segoe UI", 14F);
            txtAddressee.Location = new Point(831, 44);
            txtAddressee.Name = "txtAddressee";
            txtAddressee.Size = new Size(251, 32);
            txtAddressee.TabIndex = 14;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(831, 26);
            label2.Name = "label2";
            label2.Size = new Size(47, 15);
            label2.TabIndex = 15;
            label2.Text = "mail to:";
            // 
            // btnBD
            // 
            btnBD.Location = new Point(602, 63);
            btnBD.Name = "btnBD";
            btnBD.Size = new Size(98, 33);
            btnBD.TabIndex = 16;
            btnBD.Text = "BD";
            btnBD.UseVisualStyleBackColor = true;
            btnBD.Click += btnBD_Click;
            // 
            // btnSaveInBD
            // 
            btnSaveInBD.Location = new Point(706, 63);
            btnSaveInBD.Name = "btnSaveInBD";
            btnSaveInBD.Size = new Size(98, 33);
            btnSaveInBD.TabIndex = 17;
            btnSaveInBD.Text = "Save In BD";
            btnSaveInBD.UseVisualStyleBackColor = true;
            btnSaveInBD.Click += btnSaveInBD_Click;
            // 
            // lblRecordCount
            // 
            lblRecordCount.AutoSize = true;
            lblRecordCount.Location = new Point(27, 713);
            lblRecordCount.Name = "lblRecordCount";
            lblRecordCount.Size = new Size(0, 15);
            lblRecordCount.TabIndex = 18;
            // 
            // btnShowTreeview
            // 
            btnShowTreeview.Location = new Point(706, 14);
            btnShowTreeview.Name = "btnShowTreeview";
            btnShowTreeview.Size = new Size(98, 33);
            btnShowTreeview.TabIndex = 19;
            btnShowTreeview.Text = "Show TreeView";
            btnShowTreeview.UseVisualStyleBackColor = true;
            btnShowTreeview.Click += btnShowTreeview_Click;
            // 
            // cmbMovieType
            // 
            cmbMovieType.FormattingEnabled = true;
            cmbMovieType.Location = new Point(27, 395);
            cmbMovieType.Name = "cmbMovieType";
            cmbMovieType.Size = new Size(121, 23);
            cmbMovieType.TabIndex = 20;
            cmbMovieType.Visible = false;
            cmbMovieType.SelectedIndexChanged += cmbMovieType_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1229, 737);
            Controls.Add(cmbMovieType);
            Controls.Add(btnShowTreeview);
            Controls.Add(lblRecordCount);
            Controls.Add(btnSaveInBD);
            Controls.Add(btnBD);
            Controls.Add(label2);
            Controls.Add(txtAddressee);
            Controls.Add(btnLoadToAPI);
            Controls.Add(btnSendByEmail);
            Controls.Add(cmbFilter);
            Controls.Add(btnCreate);
            Controls.Add(btnExport);
            Controls.Add(btnSave);
            Controls.Add(txtFilter);
            Controls.Add(label1);
            Controls.Add(btnOpen);
            Controls.Add(graphic);
            Controls.Add(treeViewMovies);
            Controls.Add(dgvData);
            Controls.Add(txtData);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtData;
        private DataGridView dgvData;
        private TreeView treeViewMovies;
        private ScottPlot.WinForms.FormsPlot graphic;
        private Button btnOpen;
        private Label label1;
        private TextBox txtFilter;
        private Button btnSave;
        private Button btnExport;
        private Button btnCreate;
        private ComboBox cmbFilter;
        private Button btnSendByEmail;
        private Button btnLoadToAPI;
        private TextBox txtAddressee;
        private Label label2;
        private Button btnBD;
        private Button btnSaveInBD;
        private Label lblRecordCount;
        private Button btnShowTreeview;
        private ComboBox cmbMovieType;
    }
}
