namespace CT90
{
    partial class License
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
            this.mtTxtLicense = new MetroFramework.Controls.MetroTextBox();
            this.SuspendLayout();
            // 
            // mtTxtLicense
            // 
            // 
            // 
            // 
            this.mtTxtLicense.CustomButton.Image = null;
            this.mtTxtLicense.CustomButton.Location = new System.Drawing.Point(-8, 2);
            this.mtTxtLicense.CustomButton.Name = "";
            this.mtTxtLicense.CustomButton.Size = new System.Drawing.Size(715, 715);
            this.mtTxtLicense.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.mtTxtLicense.CustomButton.TabIndex = 1;
            this.mtTxtLicense.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.mtTxtLicense.CustomButton.UseSelectable = true;
            this.mtTxtLicense.CustomButton.Visible = false;
            this.mtTxtLicense.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mtTxtLicense.Enabled = false;
            this.mtTxtLicense.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.mtTxtLicense.Lines = new string[] {
        ".."};
            this.mtTxtLicense.Location = new System.Drawing.Point(20, 60);
            this.mtTxtLicense.MaxLength = 32767;
            this.mtTxtLicense.Multiline = true;
            this.mtTxtLicense.Name = "mtTxtLicense";
            this.mtTxtLicense.PasswordChar = '\0';
            this.mtTxtLicense.ReadOnly = true;
            this.mtTxtLicense.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.mtTxtLicense.SelectedText = "";
            this.mtTxtLicense.SelectionLength = 0;
            this.mtTxtLicense.SelectionStart = 0;
            this.mtTxtLicense.ShortcutsEnabled = true;
            this.mtTxtLicense.Size = new System.Drawing.Size(710, 720);
            this.mtTxtLicense.TabIndex = 0;
            this.mtTxtLicense.Text = "..";
            this.mtTxtLicense.UseSelectable = true;
            this.mtTxtLicense.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.mtTxtLicense.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // License
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 800);
            this.Controls.Add(this.mtTxtLicense);
            this.Name = "License";
            this.Text = "License";
            this.Load += new System.EventHandler(this.License_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTextBox mtTxtLicense;
    }
}