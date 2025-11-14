namespace CT90
{
    partial class Main
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.tmrOrder = new System.Windows.Forms.Timer(this.components);
            this.txtRcv = new System.Windows.Forms.TextBox();
            this.lblSckStatus = new System.Windows.Forms.Label();
            this.lblDbStatus = new System.Windows.Forms.Label();
            this.txtSnd = new System.Windows.Forms.TextBox();
            this.btnDevTest = new System.Windows.Forms.Button();
            this.lblHisType = new System.Windows.Forms.Label();
            this.lblOrdTmr = new System.Windows.Forms.Label();
            this.lblSetFirstOrdTmr = new System.Windows.Forms.Label();
            this.mtTitleSend = new MetroFramework.Controls.MetroTile();
            this.mtLblSend = new MetroFramework.Controls.MetroLabel();
            this.mtTitleRceive = new MetroFramework.Controls.MetroTile();
            this.mtLblRcv = new MetroFramework.Controls.MetroLabel();
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.metroTextBox2 = new MetroFramework.Controls.MetroTextBox();
            this.metroTextBox1 = new MetroFramework.Controls.MetroTextBox();
            this.mtGrdList = new MetroFramework.Controls.MetroGrid();
            this.Seq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Datetime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Equipment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SpcNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rack = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Pos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mtTitleHIS = new MetroFramework.Controls.MetroTile();
            this.mtTitleSck = new MetroFramework.Controls.MetroTile();
            this.mtTitleDB = new MetroFramework.Controls.MetroTile();
            this.mtTitleNetwork = new MetroFramework.Controls.MetroTile();
            this.mtTitleLicense = new MetroFramework.Controls.MetroTile();
            this.tmrBgw = new System.Windows.Forms.Timer(this.components);
            this.mtEtc = new MetroFramework.Controls.MetroTile();
            this.btnMnuTst = new MetroFramework.Controls.MetroButton();
            this.txtBarNo = new MetroFramework.Controls.MetroTextBox();
            this.btnReload = new MetroFramework.Controls.MetroButton();
            this.chkArchive = new System.Windows.Forms.CheckBox();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mtGrdList)).BeginInit();
            this.SuspendLayout();
            // 
            // tmrOrder
            // 
            this.tmrOrder.Tick += new System.EventHandler(this.tmrOrder_Tick);
            // 
            // txtRcv
            // 
            this.txtRcv.Location = new System.Drawing.Point(510, 45);
            this.txtRcv.Multiline = true;
            this.txtRcv.Name = "txtRcv";
            this.txtRcv.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRcv.Size = new System.Drawing.Size(50, 25);
            this.txtRcv.TabIndex = 0;
            // 
            // lblSckStatus
            // 
            this.lblSckStatus.AutoSize = true;
            this.lblSckStatus.BackColor = System.Drawing.SystemColors.Control;
            this.lblSckStatus.Location = new System.Drawing.Point(476, 35);
            this.lblSckStatus.Name = "lblSckStatus";
            this.lblSckStatus.Size = new System.Drawing.Size(28, 13);
            this.lblSckStatus.TabIndex = 1;
            this.lblSckStatus.Text = "SCK";
            // 
            // lblDbStatus
            // 
            this.lblDbStatus.AutoSize = true;
            this.lblDbStatus.BackColor = System.Drawing.SystemColors.Control;
            this.lblDbStatus.Location = new System.Drawing.Point(449, 35);
            this.lblDbStatus.Name = "lblDbStatus";
            this.lblDbStatus.Size = new System.Drawing.Size(21, 13);
            this.lblDbStatus.TabIndex = 3;
            this.lblDbStatus.Text = "DB";
            // 
            // txtSnd
            // 
            this.txtSnd.Location = new System.Drawing.Point(566, 45);
            this.txtSnd.Multiline = true;
            this.txtSnd.Name = "txtSnd";
            this.txtSnd.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSnd.Size = new System.Drawing.Size(50, 25);
            this.txtSnd.TabIndex = 4;
            // 
            // btnDevTest
            // 
            this.btnDevTest.Location = new System.Drawing.Point(455, 51);
            this.btnDevTest.Name = "btnDevTest";
            this.btnDevTest.Size = new System.Drawing.Size(49, 19);
            this.btnDevTest.TabIndex = 5;
            this.btnDevTest.Text = "TEST";
            this.btnDevTest.UseVisualStyleBackColor = true;
            this.btnDevTest.Visible = false;
            this.btnDevTest.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblHisType
            // 
            this.lblHisType.AutoSize = true;
            this.lblHisType.BackColor = System.Drawing.SystemColors.Control;
            this.lblHisType.Location = new System.Drawing.Point(510, 35);
            this.lblHisType.Name = "lblHisType";
            this.lblHisType.Size = new System.Drawing.Size(28, 13);
            this.lblHisType.TabIndex = 6;
            this.lblHisType.Text = "HIS";
            // 
            // lblOrdTmr
            // 
            this.lblOrdTmr.AutoSize = true;
            this.lblOrdTmr.BackColor = System.Drawing.SystemColors.Control;
            this.lblOrdTmr.Location = new System.Drawing.Point(578, 35);
            this.lblOrdTmr.Name = "lblOrdTmr";
            this.lblOrdTmr.Size = new System.Drawing.Size(21, 13);
            this.lblOrdTmr.TabIndex = 7;
            this.lblOrdTmr.Text = "On";
            // 
            // lblSetFirstOrdTmr
            // 
            this.lblSetFirstOrdTmr.AutoSize = true;
            this.lblSetFirstOrdTmr.BackColor = System.Drawing.SystemColors.Control;
            this.lblSetFirstOrdTmr.Location = new System.Drawing.Point(544, 35);
            this.lblSetFirstOrdTmr.Name = "lblSetFirstOrdTmr";
            this.lblSetFirstOrdTmr.Size = new System.Drawing.Size(28, 13);
            this.lblSetFirstOrdTmr.TabIndex = 8;
            this.lblSetFirstOrdTmr.Text = "Off";
            // 
            // mtTitleSend
            // 
            this.mtTitleSend.ActiveControl = null;
            this.mtTitleSend.Location = new System.Drawing.Point(23, 502);
            this.mtTitleSend.Name = "mtTitleSend";
            this.mtTitleSend.Size = new System.Drawing.Size(76, 38);
            this.mtTitleSend.TabIndex = 16;
            this.mtTitleSend.Text = "Send";
            this.mtTitleSend.TileImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.mtTitleSend.UseSelectable = true;
            this.mtTitleSend.UseTileImage = true;
            // 
            // mtLblSend
            // 
            this.mtLblSend.ForeColor = System.Drawing.Color.White;
            this.mtLblSend.Location = new System.Drawing.Point(105, 502);
            this.mtLblSend.Name = "mtLblSend";
            this.mtLblSend.Size = new System.Drawing.Size(880, 38);
            this.mtLblSend.TabIndex = 15;
            this.mtLblSend.Text = "Send..";
            this.mtLblSend.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtTitleRceive
            // 
            this.mtTitleRceive.ActiveControl = null;
            this.mtTitleRceive.Location = new System.Drawing.Point(23, 458);
            this.mtTitleRceive.Name = "mtTitleRceive";
            this.mtTitleRceive.Size = new System.Drawing.Size(76, 38);
            this.mtTitleRceive.TabIndex = 14;
            this.mtTitleRceive.Text = "Receive";
            this.mtTitleRceive.TileImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.mtTitleRceive.UseSelectable = true;
            this.mtTitleRceive.UseTileImage = true;
            // 
            // mtLblRcv
            // 
            this.mtLblRcv.ForeColor = System.Drawing.Color.White;
            this.mtLblRcv.Location = new System.Drawing.Point(105, 458);
            this.mtLblRcv.Name = "mtLblRcv";
            this.mtLblRcv.Size = new System.Drawing.Size(880, 38);
            this.mtLblRcv.TabIndex = 13;
            this.mtLblRcv.Text = "Receive..";
            this.mtLblRcv.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metroPanel1
            // 
            this.metroPanel1.Controls.Add(this.metroTextBox2);
            this.metroPanel1.Controls.Add(this.metroTextBox1);
            this.metroPanel1.Controls.Add(this.mtGrdList);
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 10;
            this.metroPanel1.Location = new System.Drawing.Point(23, 88);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(962, 364);
            this.metroPanel1.TabIndex = 18;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 10;
            // 
            // metroTextBox2
            // 
            // 
            // 
            // 
            this.metroTextBox2.CustomButton.Image = null;
            this.metroTextBox2.CustomButton.Location = new System.Drawing.Point(83, 2);
            this.metroTextBox2.CustomButton.Name = "";
            this.metroTextBox2.CustomButton.Size = new System.Drawing.Size(91, 91);
            this.metroTextBox2.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBox2.CustomButton.TabIndex = 1;
            this.metroTextBox2.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBox2.CustomButton.UseSelectable = true;
            this.metroTextBox2.CustomButton.Visible = false;
            this.metroTextBox2.Lines = new string[] {
        "metroTextBox2"};
            this.metroTextBox2.Location = new System.Drawing.Point(598, 168);
            this.metroTextBox2.MaxLength = 32767;
            this.metroTextBox2.Name = "metroTextBox2";
            this.metroTextBox2.PasswordChar = '\0';
            this.metroTextBox2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBox2.SelectedText = "";
            this.metroTextBox2.SelectionLength = 0;
            this.metroTextBox2.SelectionStart = 0;
            this.metroTextBox2.ShortcutsEnabled = true;
            this.metroTextBox2.Size = new System.Drawing.Size(177, 96);
            this.metroTextBox2.TabIndex = 20;
            this.metroTextBox2.Text = "metroTextBox2";
            this.metroTextBox2.UseSelectable = true;
            this.metroTextBox2.Visible = false;
            this.metroTextBox2.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBox2.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroTextBox1
            // 
            // 
            // 
            // 
            this.metroTextBox1.CustomButton.Image = null;
            this.metroTextBox1.CustomButton.Location = new System.Drawing.Point(83, 2);
            this.metroTextBox1.CustomButton.Name = "";
            this.metroTextBox1.CustomButton.Size = new System.Drawing.Size(91, 91);
            this.metroTextBox1.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBox1.CustomButton.TabIndex = 1;
            this.metroTextBox1.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBox1.CustomButton.UseSelectable = true;
            this.metroTextBox1.CustomButton.Visible = false;
            this.metroTextBox1.Lines = new string[] {
        "metroTextBox1"};
            this.metroTextBox1.Location = new System.Drawing.Point(415, 168);
            this.metroTextBox1.MaxLength = 32767;
            this.metroTextBox1.Name = "metroTextBox1";
            this.metroTextBox1.PasswordChar = '\0';
            this.metroTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBox1.SelectedText = "";
            this.metroTextBox1.SelectionLength = 0;
            this.metroTextBox1.SelectionStart = 0;
            this.metroTextBox1.ShortcutsEnabled = true;
            this.metroTextBox1.Size = new System.Drawing.Size(177, 96);
            this.metroTextBox1.TabIndex = 19;
            this.metroTextBox1.Text = "metroTextBox1";
            this.metroTextBox1.UseSelectable = true;
            this.metroTextBox1.Visible = false;
            this.metroTextBox1.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBox1.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // mtGrdList
            // 
            this.mtGrdList.AllowUserToOrderColumns = true;
            this.mtGrdList.AllowUserToResizeRows = false;
            this.mtGrdList.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.mtGrdList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mtGrdList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.mtGrdList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mtGrdList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.mtGrdList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mtGrdList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Seq,
            this.Type,
            this.Datetime,
            this.Equipment,
            this.SpcNo,
            this.Rack,
            this.Pos,
            this.Result});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.mtGrdList.DefaultCellStyle = dataGridViewCellStyle2;
            this.mtGrdList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mtGrdList.EnableHeadersVisualStyles = false;
            this.mtGrdList.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.mtGrdList.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.mtGrdList.Location = new System.Drawing.Point(0, 0);
            this.mtGrdList.Name = "mtGrdList";
            this.mtGrdList.ReadOnly = true;
            this.mtGrdList.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mtGrdList.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.mtGrdList.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.mtGrdList.RowTemplate.Height = 23;
            this.mtGrdList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.mtGrdList.Size = new System.Drawing.Size(962, 364);
            this.mtGrdList.TabIndex = 18;
            this.mtGrdList.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // Seq
            // 
            this.Seq.HeaderText = "순번";
            this.Seq.Name = "Seq";
            this.Seq.ReadOnly = true;
            this.Seq.Width = 40;
            // 
            // Type
            // 
            this.Type.HeaderText = "구분";
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            // 
            // Datetime
            // 
            this.Datetime.HeaderText = "일시";
            this.Datetime.Name = "Datetime";
            this.Datetime.ReadOnly = true;
            this.Datetime.Width = 200;
            // 
            // Equipment
            // 
            this.Equipment.HeaderText = "장비";
            this.Equipment.Name = "Equipment";
            this.Equipment.ReadOnly = true;
            // 
            // SpcNo
            // 
            this.SpcNo.HeaderText = "검체번호";
            this.SpcNo.Name = "SpcNo";
            this.SpcNo.ReadOnly = true;
            // 
            // Rack
            // 
            this.Rack.HeaderText = "Rack";
            this.Rack.Name = "Rack";
            this.Rack.ReadOnly = true;
            // 
            // Pos
            // 
            this.Pos.HeaderText = "Pos";
            this.Pos.Name = "Pos";
            this.Pos.ReadOnly = true;
            this.Pos.Width = 60;
            // 
            // Result
            // 
            this.Result.HeaderText = "결과";
            this.Result.Name = "Result";
            this.Result.ReadOnly = true;
            this.Result.Width = 200;
            // 
            // mtTitleHIS
            // 
            this.mtTitleHIS.ActiveControl = null;
            this.mtTitleHIS.Location = new System.Drawing.Point(363, 21);
            this.mtTitleHIS.Name = "mtTitleHIS";
            this.mtTitleHIS.Size = new System.Drawing.Size(80, 36);
            this.mtTitleHIS.TabIndex = 21;
            this.mtTitleHIS.Text = "EMR DB";
            this.mtTitleHIS.TileImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.mtTitleHIS.UseCustomBackColor = true;
            this.mtTitleHIS.UseCustomForeColor = true;
            this.mtTitleHIS.UseSelectable = true;
            // 
            // mtTitleSck
            // 
            this.mtTitleSck.ActiveControl = null;
            this.mtTitleSck.Location = new System.Drawing.Point(191, 21);
            this.mtTitleSck.Name = "mtTitleSck";
            this.mtTitleSck.Size = new System.Drawing.Size(80, 36);
            this.mtTitleSck.TabIndex = 20;
            this.mtTitleSck.Text = "Socket";
            this.mtTitleSck.TileImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mtTitleSck.UseCustomBackColor = true;
            this.mtTitleSck.UseCustomForeColor = true;
            this.mtTitleSck.UseSelectable = true;
            // 
            // mtTitleDB
            // 
            this.mtTitleDB.ActiveControl = null;
            this.mtTitleDB.Location = new System.Drawing.Point(105, 21);
            this.mtTitleDB.Name = "mtTitleDB";
            this.mtTitleDB.Size = new System.Drawing.Size(80, 36);
            this.mtTitleDB.TabIndex = 19;
            this.mtTitleDB.Text = "I/F DB";
            this.mtTitleDB.TileImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mtTitleDB.UseCustomBackColor = true;
            this.mtTitleDB.UseCustomForeColor = true;
            this.mtTitleDB.UseSelectable = true;
            // 
            // mtTitleNetwork
            // 
            this.mtTitleNetwork.ActiveControl = null;
            this.mtTitleNetwork.Location = new System.Drawing.Point(277, 21);
            this.mtTitleNetwork.Name = "mtTitleNetwork";
            this.mtTitleNetwork.Size = new System.Drawing.Size(80, 36);
            this.mtTitleNetwork.TabIndex = 22;
            this.mtTitleNetwork.Text = "Network";
            this.mtTitleNetwork.TileImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.mtTitleNetwork.UseCustomBackColor = true;
            this.mtTitleNetwork.UseCustomForeColor = true;
            this.mtTitleNetwork.UseSelectable = true;
            // 
            // mtTitleLicense
            // 
            this.mtTitleLicense.ActiveControl = null;
            this.mtTitleLicense.Location = new System.Drawing.Point(905, 63);
            this.mtTitleLicense.Name = "mtTitleLicense";
            this.mtTitleLicense.Size = new System.Drawing.Size(80, 36);
            this.mtTitleLicense.TabIndex = 24;
            this.mtTitleLicense.Text = "License";
            this.mtTitleLicense.TileImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.mtTitleLicense.UseCustomBackColor = true;
            this.mtTitleLicense.UseCustomForeColor = true;
            this.mtTitleLicense.UseSelectable = true;
            this.mtTitleLicense.Visible = false;
            this.mtTitleLicense.Click += new System.EventHandler(this.mtTitleLicense_Click);
            // 
            // tmrBgw
            // 
            this.tmrBgw.Tick += new System.EventHandler(this.tmrBgw_Tick);
            // 
            // mtEtc
            // 
            this.mtEtc.ActiveControl = null;
            this.mtEtc.Location = new System.Drawing.Point(924, 21);
            this.mtEtc.Name = "mtEtc";
            this.mtEtc.Size = new System.Drawing.Size(80, 36);
            this.mtEtc.TabIndex = 31;
            this.mtEtc.Text = "...";
            this.mtEtc.TileImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.mtEtc.UseCustomBackColor = true;
            this.mtEtc.UseCustomForeColor = true;
            this.mtEtc.UseSelectable = true;
            this.mtEtc.Visible = false;
            // 
            // btnMnuTst
            // 
            this.btnMnuTst.Location = new System.Drawing.Point(840, 13);
            this.btnMnuTst.Name = "btnMnuTst";
            this.btnMnuTst.Size = new System.Drawing.Size(78, 35);
            this.btnMnuTst.TabIndex = 35;
            this.btnMnuTst.Text = "Test...";
            this.btnMnuTst.UseSelectable = true;
            this.btnMnuTst.Click += new System.EventHandler(this.btnMnuTst_Click);
            // 
            // txtBarNo
            // 
            // 
            // 
            // 
            this.txtBarNo.CustomButton.Image = null;
            this.txtBarNo.CustomButton.Location = new System.Drawing.Point(150, 1);
            this.txtBarNo.CustomButton.Name = "";
            this.txtBarNo.CustomButton.Size = new System.Drawing.Size(33, 33);
            this.txtBarNo.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.txtBarNo.CustomButton.TabIndex = 1;
            this.txtBarNo.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.txtBarNo.CustomButton.UseSelectable = true;
            this.txtBarNo.CustomButton.Visible = false;
            this.txtBarNo.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.txtBarNo.Lines = new string[0];
            this.txtBarNo.Location = new System.Drawing.Point(650, 13);
            this.txtBarNo.MaxLength = 32767;
            this.txtBarNo.Name = "txtBarNo";
            this.txtBarNo.PasswordChar = '\0';
            this.txtBarNo.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtBarNo.SelectedText = "";
            this.txtBarNo.SelectionLength = 0;
            this.txtBarNo.SelectionStart = 0;
            this.txtBarNo.ShortcutsEnabled = true;
            this.txtBarNo.Size = new System.Drawing.Size(184, 35);
            this.txtBarNo.TabIndex = 34;
            this.txtBarNo.UseSelectable = true;
            this.txtBarNo.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.txtBarNo.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(566, 13);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(78, 35);
            this.btnReload.TabIndex = 33;
            this.btnReload.Text = "Reload Rules";
            this.btnReload.UseSelectable = true;
            // 
            // chkArchive
            // 
            this.chkArchive.AutoSize = true;
            this.chkArchive.Location = new System.Drawing.Point(478, 13);
            this.chkArchive.Name = "chkArchive";
            this.chkArchive.Size = new System.Drawing.Size(82, 17);
            this.chkArchive.TabIndex = 32;
            this.chkArchive.Text = "위치관리";
            this.chkArchive.UseVisualStyleBackColor = true;
            // 
            // metroButton1
            // 
            this.metroButton1.Location = new System.Drawing.Point(840, 54);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(78, 35);
            this.metroButton1.TabIndex = 36;
            this.metroButton1.Text = "Test...";
            this.metroButton1.UseSelectable = true;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 551);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.btnMnuTst);
            this.Controls.Add(this.txtBarNo);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.chkArchive);
            this.Controls.Add(this.mtEtc);
            this.Controls.Add(this.mtTitleLicense);
            this.Controls.Add(this.mtTitleNetwork);
            this.Controls.Add(this.mtTitleHIS);
            this.Controls.Add(this.mtTitleSck);
            this.Controls.Add(this.mtTitleDB);
            this.Controls.Add(this.metroPanel1);
            this.Controls.Add(this.mtTitleSend);
            this.Controls.Add(this.mtLblSend);
            this.Controls.Add(this.mtTitleRceive);
            this.Controls.Add(this.mtLblRcv);
            this.Controls.Add(this.lblSetFirstOrdTmr);
            this.Controls.Add(this.lblOrdTmr);
            this.Controls.Add(this.lblHisType);
            this.Controls.Add(this.btnDevTest);
            this.Controls.Add(this.txtSnd);
            this.Controls.Add(this.lblDbStatus);
            this.Controls.Add(this.lblSckStatus);
            this.Controls.Add(this.txtRcv);
            this.Font = new System.Drawing.Font("굴림체", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "CT-90";
            this.TransparencyKey = System.Drawing.Color.Empty;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.metroPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mtGrdList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.Timer tmrOrder;
		private System.Windows.Forms.TextBox txtRcv;
		private System.Windows.Forms.Label lblSckStatus;
		private System.Windows.Forms.Label lblDbStatus;
		private System.Windows.Forms.TextBox txtSnd;
        private System.Windows.Forms.Button btnDevTest;
        private System.Windows.Forms.Label lblHisType;
        private System.Windows.Forms.Label lblOrdTmr;
        private System.Windows.Forms.Label lblSetFirstOrdTmr;
        private MetroFramework.Controls.MetroTile mtTitleSend;
        private MetroFramework.Controls.MetroLabel mtLblSend;
        private MetroFramework.Controls.MetroTile mtTitleRceive;
        private MetroFramework.Controls.MetroLabel mtLblRcv;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroGrid mtGrdList;
        private MetroFramework.Controls.MetroTile mtTitleDB;
        private MetroFramework.Controls.MetroTile mtTitleSck;
        private MetroFramework.Controls.MetroTile mtTitleHIS;
        private MetroFramework.Controls.MetroTile mtTitleNetwork;
        private MetroFramework.Controls.MetroTile mtTitleLicense;
        private System.Windows.Forms.DataGridViewTextBoxColumn Seq;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Datetime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Equipment;
        private System.Windows.Forms.DataGridViewTextBoxColumn SpcNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Rack;
        private System.Windows.Forms.DataGridViewTextBoxColumn Pos;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result;
        private MetroFramework.Controls.MetroTextBox metroTextBox1;
        private MetroFramework.Controls.MetroTextBox metroTextBox2;
        private System.Windows.Forms.Timer tmrBgw;
        private MetroFramework.Controls.MetroTile mtEtc;
        private MetroFramework.Controls.MetroButton btnMnuTst;
        private MetroFramework.Controls.MetroTextBox txtBarNo;
        private MetroFramework.Controls.MetroButton btnReload;
        private System.Windows.Forms.CheckBox chkArchive;
        private MetroFramework.Controls.MetroButton metroButton1;
    }
}

