using AdSecGH;
namespace AdSecGH.UI
{
    partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            components = new System.ComponentModel.Container();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            logoPictureBox = new System.Windows.Forms.PictureBox();
            labelProductName = new System.Windows.Forms.Label();
            labelVersion = new System.Windows.Forms.Label();
            labelApiVersion = new System.Windows.Forms.Label();
            labelCompanyName = new System.Windows.Forms.Label();
            okButton = new System.Windows.Forms.Button();
            labelContact = new System.Windows.Forms.Label();
            linkWebsite = new System.Windows.Forms.LinkLabel();
            linkEmail = new System.Windows.Forms.LinkLabel();
            Check = new System.Windows.Forms.Button();
            imageList1 = new System.Windows.Forms.ImageList(components);
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).BeginInit();
            SuspendLayout();
            //
            // tableLayoutPanel
            //
            tableLayoutPanel.ColumnCount = 3;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.66939F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.4964F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.88969F));
            tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
            tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
            tableLayoutPanel.Controls.Add(labelVersion, 1, 1);
            tableLayoutPanel.Controls.Add(labelApiVersion, 1, 2);
            tableLayoutPanel.Controls.Add(labelCompanyName, 1, 3);
            tableLayoutPanel.Controls.Add(okButton, 2, 5);
            tableLayoutPanel.Controls.Add(labelContact, 1, 4);
            tableLayoutPanel.Controls.Add(linkWebsite, 0, 5);
            tableLayoutPanel.Controls.Add(linkEmail, 2, 4);
            tableLayoutPanel.Controls.Add(Check, 2, 1);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 6;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            tableLayoutPanel.Size = new System.Drawing.Size(417, 163);
            tableLayoutPanel.TabIndex = 0;
            //
            // logoPictureBox
            //
            logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            logoPictureBox.Image = global::AdSecGH.Properties.Resources.AdSecLogo128;
            logoPictureBox.Location = new System.Drawing.Point(3, 3);
            logoPictureBox.Name = "logoPictureBox";
            tableLayoutPanel.SetRowSpan(logoPictureBox, 5);
            logoPictureBox.Size = new System.Drawing.Size(130, 129);
            logoPictureBox.TabIndex = 12;
            logoPictureBox.TabStop = false;
            //
            // labelProductName
            //
            tableLayoutPanel.SetColumnSpan(labelProductName, 2);
            labelProductName.Dock = System.Windows.Forms.DockStyle.Fill;
            labelProductName.Location = new System.Drawing.Point(142, 0);
            labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
            labelProductName.Name = "labelProductName";
            labelProductName.Size = new System.Drawing.Size(272, 17);
            labelProductName.TabIndex = 19;
            labelProductName.Text = "AdSec Grasshopper Plugin";
            labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // labelVersion
            //
            labelVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            labelVersion.Location = new System.Drawing.Point(142, 27);
            labelVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new System.Drawing.Size(113, 17);
            labelVersion.TabIndex = 0;
            labelVersion.Text = "Plugin Version";
            labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // labelApiVersion
            //
            tableLayoutPanel.SetColumnSpan(labelApiVersion, 2);
            labelApiVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            labelApiVersion.Location = new System.Drawing.Point(142, 54);
            labelApiVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            labelApiVersion.MaximumSize = new System.Drawing.Size(0, 17);
            labelApiVersion.Name = "labelApiVersion";
            labelApiVersion.Size = new System.Drawing.Size(272, 17);
            labelApiVersion.TabIndex = 21;
            labelApiVersion.Text = "API Version";
            labelApiVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // labelCompanyName
            //
            tableLayoutPanel.SetColumnSpan(labelCompanyName, 2);
            labelCompanyName.Dock = System.Windows.Forms.DockStyle.Fill;
            labelCompanyName.Location = new System.Drawing.Point(142, 81);
            labelCompanyName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            labelCompanyName.MaximumSize = new System.Drawing.Size(0, 17);
            labelCompanyName.Name = "labelCompanyName";
            labelCompanyName.Size = new System.Drawing.Size(272, 17);
            labelCompanyName.TabIndex = 22;
            labelCompanyName.Text = "Company";
            labelCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // okButton
            //
            okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            okButton.Location = new System.Drawing.Point(339, 139);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 21);
            okButton.TabIndex = 24;
            okButton.Text = "&OK";
            okButton.Click += new System.EventHandler(okButton_Click);
            //
            // labelContact
            //
            labelContact.Location = new System.Drawing.Point(142, 108);
            labelContact.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            labelContact.Name = "labelContact";
            labelContact.Size = new System.Drawing.Size(113, 17);
            labelContact.TabIndex = 22;
            labelContact.Text = "Contact and support:";
            labelContact.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // linkWebsite
            //
            linkWebsite.Anchor = System.Windows.Forms.AnchorStyles.Left;
            linkWebsite.AutoSize = true;
            tableLayoutPanel.SetColumnSpan(linkWebsite, 2);
            linkWebsite.Location = new System.Drawing.Point(6, 142);
            linkWebsite.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            linkWebsite.Name = "linkWebsite";
            linkWebsite.Size = new System.Drawing.Size(127, 13);
            linkWebsite.TabIndex = 25;
            linkWebsite.TabStop = true;
            linkWebsite.Text = "www.oasys-software.com";
            linkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
            //
            // linkEmail
            //
            linkEmail.Location = new System.Drawing.Point(264, 108);
            linkEmail.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            linkEmail.Name = "linkEmail";
            linkEmail.Size = new System.Drawing.Size(107, 17);
            linkEmail.TabIndex = 26;
            linkEmail.TabStop = true;
            linkEmail.Text = "oasys@arup.com";
            linkEmail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            linkEmail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkEmail_LinkClicked);
            //
            // Check
            //
            Check.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            Check.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            Check.Location = new System.Drawing.Point(302, 30);
            Check.Name = "Check";
            Check.Size = new System.Drawing.Size(112, 21);
            Check.TabIndex = 27;
            Check.Text = "&Check for Updates";
            Check.UseVisualStyleBackColor = true;
            Check.Click += new System.EventHandler(button1_Click);
            //
            // imageList1
            //
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList1.ImageSize = new System.Drawing.Size(16, 16);
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            //
            // AboutAdSecBox
            //
            AcceptButton = okButton;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(435, 181);
            Controls.Add(tableLayoutPanel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutAdSecBox";
            Padding = new System.Windows.Forms.Padding(9);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "About AdSec";
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelApiVersion;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label labelCompanyName;
        private System.Windows.Forms.LinkLabel linkWebsite;
        private System.Windows.Forms.Label labelContact;
        private System.Windows.Forms.LinkLabel linkEmail;
        private System.Windows.Forms.Button Check;
        private System.Windows.Forms.ImageList imageList1;
    }
}
