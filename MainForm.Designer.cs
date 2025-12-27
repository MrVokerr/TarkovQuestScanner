namespace TarkovQuestScanner
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.TrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.trayshow = new System.Windows.Forms.ToolStripMenuItem();
            this.trayexit = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ShowOverlay_Button = new System.Windows.Forms.Button();
            this.ShowOverlay_Desc = new System.Windows.Forms.Label();
            this.MinimizetoTrayWhenStartup = new System.Windows.Forms.CheckBox();
            this.check_idle_time = new System.Windows.Forms.Timer(this.components);
            this.panel8 = new System.Windows.Forms.Panel();
            this.modeSelector = new System.Windows.Forms.ComboBox();
            this.labelTarkovTrackerAPI = new System.Windows.Forms.Label();
            this.textBoxTarkovTrackerAPI = new System.Windows.Forms.TextBox();
            this.btnTestToken = new System.Windows.Forms.Button();
            this.openReport_b = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.Version = new System.Windows.Forms.Label();
            this.CheckUpdate = new System.Windows.Forms.Button();
            this.TrayMenu.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.TrayMenu;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "TarkovQuestScanner";
            this.TrayIcon.Visible = true;
            this.TrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TrayIcon_MouseDoubleClick);
            // 
            // TrayMenu
            // 
            this.TrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trayshow,
            this.trayexit});
            this.TrayMenu.Name = "TrayMenu";
            this.TrayMenu.Size = new System.Drawing.Size(105, 48);
            // 
            // trayshow
            // 
            this.trayshow.Name = "trayshow";
            this.trayshow.Size = new System.Drawing.Size(104, 22);
            this.trayshow.Text = "Show";
            this.trayshow.Click += new System.EventHandler(this.TrayShow_Click);
            // 
            // trayexit
            // 
            this.trayexit.Name = "trayexit";
            this.trayexit.Size = new System.Drawing.Size(104, 22);
            this.trayexit.Text = "Exit";
            this.trayexit.Click += new System.EventHandler(this.TrayExit_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panel1.Controls.Add(this.ShowOverlay_Button);
            this.panel1.Controls.Add(this.ShowOverlay_Desc);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(280, 50);
            this.panel1.TabIndex = 1;
            // 
            // ShowOverlay_Button
            // 
            this.ShowOverlay_Button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.ShowOverlay_Button.FlatAppearance.BorderSize = 0;
            this.ShowOverlay_Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShowOverlay_Button.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.ShowOverlay_Button.ForeColor = System.Drawing.Color.White;
            this.ShowOverlay_Button.Location = new System.Drawing.Point(140, 10);
            this.ShowOverlay_Button.Name = "ShowOverlay_Button";
            this.ShowOverlay_Button.Size = new System.Drawing.Size(130, 30);
            this.ShowOverlay_Button.TabIndex = 1;
            this.ShowOverlay_Button.TabStop = false;
            this.ShowOverlay_Button.Text = "F9";
            this.ShowOverlay_Button.UseVisualStyleBackColor = false;
            this.ShowOverlay_Button.Click += new System.EventHandler(this.Overlay_Button_Click);
            // 
            // ShowOverlay_Desc
            // 
            this.ShowOverlay_Desc.AutoSize = true;
            this.ShowOverlay_Desc.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.ShowOverlay_Desc.ForeColor = System.Drawing.Color.Gainsboro;
            this.ShowOverlay_Desc.Location = new System.Drawing.Point(10, 16);
            this.ShowOverlay_Desc.Name = "ShowOverlay_Desc";
            this.ShowOverlay_Desc.Size = new System.Drawing.Size(107, 17);
            this.ShowOverlay_Desc.TabIndex = 0;
            this.ShowOverlay_Desc.Text = "Scan Quests Key";
            // 
            // MinimizetoTrayWhenStartup
            // 
            this.MinimizetoTrayWhenStartup.AutoSize = true;
            this.MinimizetoTrayWhenStartup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimizetoTrayWhenStartup.ForeColor = System.Drawing.Color.Gainsboro;
            this.MinimizetoTrayWhenStartup.Location = new System.Drawing.Point(15, 75);
            this.MinimizetoTrayWhenStartup.Name = "MinimizetoTrayWhenStartup";
            this.MinimizetoTrayWhenStartup.Size = new System.Drawing.Size(173, 19);
            this.MinimizetoTrayWhenStartup.TabIndex = 6;
            this.MinimizetoTrayWhenStartup.TabStop = false;
            this.MinimizetoTrayWhenStartup.Text = "Minimize to Tray on Startup";
            this.MinimizetoTrayWhenStartup.UseVisualStyleBackColor = true;
            this.MinimizetoTrayWhenStartup.CheckedChanged += new System.EventHandler(this.MinimizetoTrayWhenStartup_CheckedChanged);
            // 
            // check_idle_time
            // 
            this.check_idle_time.Interval = 60000;
            this.check_idle_time.Tick += new System.EventHandler(this.check_idle_time_Tick);
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panel8.Controls.Add(this.modeSelector);
            this.panel8.Controls.Add(this.labelTarkovTrackerAPI);
            this.panel8.Controls.Add(this.textBoxTarkovTrackerAPI);
            this.panel8.Controls.Add(this.btnTestToken);
            this.panel8.Controls.Add(this.openReport_b);
            this.panel8.Location = new System.Drawing.Point(308, 12);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(300, 105);
            this.panel8.TabIndex = 17;
                        // 
                        // modeSelector
                        // 
                        this.modeSelector.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
                        this.modeSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                        this.modeSelector.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                        this.modeSelector.ForeColor = System.Drawing.Color.White;
                        this.modeSelector.Items.AddRange(new object[] {
                        "PVP",
                        "PVE"});
                        this.modeSelector.Location = new System.Drawing.Point(10, 10);
                        this.modeSelector.Name = "modeSelector";
                        this.modeSelector.Size = new System.Drawing.Size(75, 23);
                        this.modeSelector.TabIndex = 20;
                        this.modeSelector.SelectedIndexChanged += new System.EventHandler(this.modeSelector_SelectedIndexChanged);
                        // 
                        // labelTarkovTrackerAPI
                        // 
                        this.labelTarkovTrackerAPI.AutoSize = true;
                        this.labelTarkovTrackerAPI.ForeColor = System.Drawing.Color.Gainsboro;
                        this.labelTarkovTrackerAPI.Location = new System.Drawing.Point(10, 43);
                        this.labelTarkovTrackerAPI.Name = "labelTarkovTrackerAPI";
                        this.labelTarkovTrackerAPI.Size = new System.Drawing.Size(51, 12);
                        this.labelTarkovTrackerAPI.TabIndex = 21;
                        this.labelTarkovTrackerAPI.Text = "API Key:";
                        // 
                        // textBoxTarkovTrackerAPI
                        // 
                        this.textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
                        this.textBoxTarkovTrackerAPI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        this.textBoxTarkovTrackerAPI.ForeColor = System.Drawing.Color.White;
                        this.textBoxTarkovTrackerAPI.Location = new System.Drawing.Point(65, 40);
                        this.textBoxTarkovTrackerAPI.Name = "textBoxTarkovTrackerAPI";
                                    this.textBoxTarkovTrackerAPI.Size = new System.Drawing.Size(220, 21);
                                                this.textBoxTarkovTrackerAPI.TabIndex = 22;
                                                this.textBoxTarkovTrackerAPI.TextChanged += new System.EventHandler(this.textBoxTarkovTrackerAPI_TextChanged);
                                                this.textBoxTarkovTrackerAPI.Leave += new System.EventHandler(this.textBoxTarkovTrackerAPI_Leave);
                                                // 
                                                // btnTestToken
                                                // 
                                                this.btnTestToken.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
                                                this.btnTestToken.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
                                                this.btnTestToken.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                                                this.btnTestToken.ForeColor = System.Drawing.Color.Gainsboro;
                                                this.btnTestToken.Location = new System.Drawing.Point(235, 10);
                                                this.btnTestToken.Name = "btnTestToken";
                                                this.btnTestToken.Size = new System.Drawing.Size(50, 23);
                                                this.btnTestToken.TabIndex = 23;
                                                this.btnTestToken.Text = "Test";
                                                this.btnTestToken.UseVisualStyleBackColor = false;
                                                this.btnTestToken.Click += new System.EventHandler(this.btnTestToken_Click);
                                                // 
                                                // openReport_b
                                                // 
                                                this.openReport_b.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.openReport_b.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.openReport_b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openReport_b.ForeColor = System.Drawing.Color.Gainsboro;
            this.openReport_b.Location = new System.Drawing.Point(10, 72);
            this.openReport_b.Name = "openReport_b";
            this.openReport_b.Size = new System.Drawing.Size(275, 23);
            this.openReport_b.TabIndex = 99;
            this.openReport_b.Text = "Open Web Report";
            this.openReport_b.UseVisualStyleBackColor = false;
            this.openReport_b.Click += new System.EventHandler(this.OpenReport_Click);
            // 
            // logBox
            // 
            this.logBox.BackColor = System.Drawing.Color.Black;
            this.logBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.logBox.ForeColor = System.Drawing.Color.Lime;
            this.logBox.Location = new System.Drawing.Point(0, 110);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.logBox.Size = new System.Drawing.Size(620, 300);
            this.logBox.TabIndex = 20;
            this.logBox.Text = "";
            // 
            // Version
            // 
            this.Version.AutoSize = true;
            this.Version.ForeColor = System.Drawing.Color.DimGray;
            this.Version.Location = new System.Drawing.Point(550, 95);
            this.Version.Name = "Version";
            this.Version.Size = new System.Drawing.Size(48, 12);
            this.Version.TabIndex = 9;
            this.Version.Text = "Version";
            // 
            // CheckUpdate
            // 
            this.CheckUpdate.Location = new System.Drawing.Point(0, 0);
            this.CheckUpdate.Name = "CheckUpdate";
            this.CheckUpdate.Size = new System.Drawing.Size(0, 0);
            this.CheckUpdate.TabIndex = 99;
            this.CheckUpdate.TabStop = false;
            this.CheckUpdate.Text = "CheckUpdate";
            this.CheckUpdate.UseVisualStyleBackColor = true;
            this.CheckUpdate.Visible = false;
            this.CheckUpdate.Click += new System.EventHandler(this.CheckUpdate_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(620, 410);
            this.Controls.Add(this.CheckUpdate);
            this.Controls.Add(this.Version);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.panel8);
            this.Controls.Add(this.MinimizetoTrayWhenStartup);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TarkovQuestScanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.TrayMenu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenuStrip TrayMenu;
        private System.Windows.Forms.ToolStripMenuItem trayshow;
        private System.Windows.Forms.ToolStripMenuItem trayexit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ShowOverlay_Button;
        private System.Windows.Forms.Label ShowOverlay_Desc;
        private System.Windows.Forms.CheckBox MinimizetoTrayWhenStartup;
        private System.Windows.Forms.Timer check_idle_time;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.ComboBox modeSelector;
        private System.Windows.Forms.Label labelTarkovTrackerAPI;
        private System.Windows.Forms.TextBox textBoxTarkovTrackerAPI;
        private System.Windows.Forms.Button btnTestToken;
        private System.Windows.Forms.Button openReport_b;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.Label Version;
        private System.Windows.Forms.Button CheckUpdate;
    }
}