namespace VirtualRadar.WinForms
{
    partial class MainView
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
            if(disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuConnectionSessionLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuConnectionClientLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFlightSimulatorXModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpenVirtualRadarLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuReconnectToDataFeedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuResetReceiverRangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.menuDownloadDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.menuPluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCheckForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuAboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripDropDownButtonInvalidPluginCount = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripDropDownButtonLaterVersionAvailable = new System.Windows.Forms.ToolStripDropDownButton();
            this.splitContainerInner = new System.Windows.Forms.SplitContainer();
            this.feedStatusControl = new VirtualRadar.WinForms.Controls.FeedStatusControl();
            this.rebroadcastStatusControl = new VirtualRadar.WinForms.Controls.RebroadcastStatusControl();
            this.webServerStatusControl = new VirtualRadar.WinForms.Controls.WebServerStatusControl();
            this.splitContainerOuter = new System.Windows.Forms.SplitContainer();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStripNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.splitContainerInner.Panel1.SuspendLayout();
            this.splitContainerInner.Panel2.SuspendLayout();
            this.splitContainerInner.SuspendLayout();
            this.splitContainerOuter.Panel1.SuspendLayout();
            this.splitContainerOuter.Panel2.SuspendLayout();
            this.splitContainerOuter.SuspendLayout();
            this.contextMenuStripNotifyIcon.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileToolStripMenuItem,
            this.menuToolsToolStripMenuItem,
            this.menuHelpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(738, 24);
            this.menuStrip.TabIndex = 0;
            // 
            // menuFileToolStripMenuItem
            // 
            this.menuFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuConnectionSessionLogToolStripMenuItem,
            this.menuConnectionClientLogToolStripMenuItem,
            this.menuStatisticsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.menuFlightSimulatorXModeToolStripMenuItem,
            this.toolStripMenuItem3,
            this.menuExitToolStripMenuItem});
            this.menuFileToolStripMenuItem.Name = "menuFileToolStripMenuItem";
            this.menuFileToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.menuFileToolStripMenuItem.Text = "::menuFile::";
            this.menuFileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.menuFileToolStripMenuItem_DropDownOpening);
            // 
            // menuConnectionSessionLogToolStripMenuItem
            // 
            this.menuConnectionSessionLogToolStripMenuItem.Name = "menuConnectionSessionLogToolStripMenuItem";
            this.menuConnectionSessionLogToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.menuConnectionSessionLogToolStripMenuItem.Text = "::menuConnectionSessionLog::";
            this.menuConnectionSessionLogToolStripMenuItem.Click += new System.EventHandler(this.menuConnectionSessionLogToolStripMenuItem_Click);
            // 
            // menuConnectionClientLogToolStripMenuItem
            // 
            this.menuConnectionClientLogToolStripMenuItem.Name = "menuConnectionClientLogToolStripMenuItem";
            this.menuConnectionClientLogToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.menuConnectionClientLogToolStripMenuItem.Text = "::menuConnectionClientLog::";
            this.menuConnectionClientLogToolStripMenuItem.Click += new System.EventHandler(this.menuConnectionClientLogToolStripMenuItem_Click);
            // 
            // menuStatisticsToolStripMenuItem
            // 
            this.menuStatisticsToolStripMenuItem.Name = "menuStatisticsToolStripMenuItem";
            this.menuStatisticsToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.menuStatisticsToolStripMenuItem.Text = "::menuStatistics::";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(235, 6);
            // 
            // menuFlightSimulatorXModeToolStripMenuItem
            // 
            this.menuFlightSimulatorXModeToolStripMenuItem.Name = "menuFlightSimulatorXModeToolStripMenuItem";
            this.menuFlightSimulatorXModeToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.menuFlightSimulatorXModeToolStripMenuItem.Text = "::menuFlightSimulatorXMode::";
            this.menuFlightSimulatorXModeToolStripMenuItem.Click += new System.EventHandler(this.menuFlightSimulatorXModeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(235, 6);
            // 
            // menuExitToolStripMenuItem
            // 
            this.menuExitToolStripMenuItem.Name = "menuExitToolStripMenuItem";
            this.menuExitToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.menuExitToolStripMenuItem.Text = "::menuExit::";
            this.menuExitToolStripMenuItem.Click += new System.EventHandler(this.menuExitToolStripMenuItem_Click);
            // 
            // menuToolsToolStripMenuItem
            // 
            this.menuToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuOpenVirtualRadarLogToolStripMenuItem,
            this.menuReconnectToDataFeedToolStripMenuItem,
            this.menuResetReceiverRangeToolStripMenuItem,
            this.toolStripMenuItem4,
            this.menuDownloadDataToolStripMenuItem,
            this.toolStripMenuItem5,
            this.menuPluginsToolStripMenuItem,
            this.menuOptionsToolStripMenuItem});
            this.menuToolsToolStripMenuItem.Name = "menuToolsToolStripMenuItem";
            this.menuToolsToolStripMenuItem.Size = new System.Drawing.Size(91, 20);
            this.menuToolsToolStripMenuItem.Text = "::menuTools::";
            this.menuToolsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.menuToolsToolStripMenuItem_DropDownOpening);
            // 
            // menuOpenVirtualRadarLogToolStripMenuItem
            // 
            this.menuOpenVirtualRadarLogToolStripMenuItem.Name = "menuOpenVirtualRadarLogToolStripMenuItem";
            this.menuOpenVirtualRadarLogToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuOpenVirtualRadarLogToolStripMenuItem.Text = "::menuOpenVirtualRadarLog::";
            this.menuOpenVirtualRadarLogToolStripMenuItem.Click += new System.EventHandler(this.menuOpenVirtualRadarLogToolStripMenuItem_Click);
            // 
            // menuReconnectToDataFeedToolStripMenuItem
            // 
            this.menuReconnectToDataFeedToolStripMenuItem.Name = "menuReconnectToDataFeedToolStripMenuItem";
            this.menuReconnectToDataFeedToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuReconnectToDataFeedToolStripMenuItem.Text = "::menuReconnectToDataFeed::";
            // 
            // menuResetReceiverRangeToolStripMenuItem
            // 
            this.menuResetReceiverRangeToolStripMenuItem.Name = "menuResetReceiverRangeToolStripMenuItem";
            this.menuResetReceiverRangeToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuResetReceiverRangeToolStripMenuItem.Text = "::menuResetReceiverRange::";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(233, 6);
            // 
            // menuDownloadDataToolStripMenuItem
            // 
            this.menuDownloadDataToolStripMenuItem.Name = "menuDownloadDataToolStripMenuItem";
            this.menuDownloadDataToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuDownloadDataToolStripMenuItem.Text = "::menuDownloadData::";
            this.menuDownloadDataToolStripMenuItem.Click += new System.EventHandler(this.menuDownloadDataToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(233, 6);
            // 
            // menuPluginsToolStripMenuItem
            // 
            this.menuPluginsToolStripMenuItem.Name = "menuPluginsToolStripMenuItem";
            this.menuPluginsToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuPluginsToolStripMenuItem.Text = "::menuPlugins::";
            this.menuPluginsToolStripMenuItem.Click += new System.EventHandler(this.menuPluginsToolStripMenuItem_Click);
            // 
            // menuOptionsToolStripMenuItem
            // 
            this.menuOptionsToolStripMenuItem.Name = "menuOptionsToolStripMenuItem";
            this.menuOptionsToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuOptionsToolStripMenuItem.Text = "::menuOptions::";
            this.menuOptionsToolStripMenuItem.Click += new System.EventHandler(this.menuOptionsToolStripMenuItem_Click);
            // 
            // menuHelpToolStripMenuItem
            // 
            this.menuHelpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCheckForUpdatesToolStripMenuItem,
            this.toolStripMenuItem2,
            this.menuAboutToolStripMenuItem});
            this.menuHelpToolStripMenuItem.Name = "menuHelpToolStripMenuItem";
            this.menuHelpToolStripMenuItem.Size = new System.Drawing.Size(87, 20);
            this.menuHelpToolStripMenuItem.Text = "::menuHelp::";
            // 
            // menuCheckForUpdatesToolStripMenuItem
            // 
            this.menuCheckForUpdatesToolStripMenuItem.Name = "menuCheckForUpdatesToolStripMenuItem";
            this.menuCheckForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.menuCheckForUpdatesToolStripMenuItem.Text = "::menuCheckForUpdates::";
            this.menuCheckForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.menuCheckForUpdatesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(207, 6);
            // 
            // menuAboutToolStripMenuItem
            // 
            this.menuAboutToolStripMenuItem.Name = "menuAboutToolStripMenuItem";
            this.menuAboutToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.menuAboutToolStripMenuItem.Text = "::menuAbout::";
            this.menuAboutToolStripMenuItem.Click += new System.EventHandler(this.menuAboutToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButtonInvalidPluginCount,
            this.toolStripDropDownButtonLaterVersionAvailable});
            this.statusStrip.Location = new System.Drawing.Point(0, 551);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(738, 22);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripDropDownButtonInvalidPluginCount
            // 
            this.toolStripDropDownButtonInvalidPluginCount.AutoToolTip = false;
            this.toolStripDropDownButtonInvalidPluginCount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonInvalidPluginCount.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonInvalidPluginCount.Image")));
            this.toolStripDropDownButtonInvalidPluginCount.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonInvalidPluginCount.Name = "toolStripDropDownButtonInvalidPluginCount";
            this.toolStripDropDownButtonInvalidPluginCount.ShowDropDownArrow = false;
            this.toolStripDropDownButtonInvalidPluginCount.Size = new System.Drawing.Size(180, 20);
            this.toolStripDropDownButtonInvalidPluginCount.Text = "<InvalidPluginCountGoesHere>";
            this.toolStripDropDownButtonInvalidPluginCount.ToolTipText = " ";
            this.toolStripDropDownButtonInvalidPluginCount.Click += new System.EventHandler(this.toolStripDropDownButtonInvalidPluginCount_Click);
            // 
            // toolStripDropDownButtonLaterVersionAvailable
            // 
            this.toolStripDropDownButtonLaterVersionAvailable.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButtonLaterVersionAvailable.AutoToolTip = false;
            this.toolStripDropDownButtonLaterVersionAvailable.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonLaterVersionAvailable.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonLaterVersionAvailable.Image")));
            this.toolStripDropDownButtonLaterVersionAvailable.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonLaterVersionAvailable.Name = "toolStripDropDownButtonLaterVersionAvailable";
            this.toolStripDropDownButtonLaterVersionAvailable.ShowDropDownArrow = false;
            this.toolStripDropDownButtonLaterVersionAvailable.Size = new System.Drawing.Size(136, 20);
            this.toolStripDropDownButtonLaterVersionAvailable.Text = "::LaterVersionAvailable::";
            this.toolStripDropDownButtonLaterVersionAvailable.ToolTipText = " ";
            this.toolStripDropDownButtonLaterVersionAvailable.Click += new System.EventHandler(this.toolStripDropDownButtonLaterVersionAvailable_Click);
            // 
            // splitContainerInner
            // 
            this.splitContainerInner.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInner.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerInner.Location = new System.Drawing.Point(0, 0);
            this.splitContainerInner.Name = "splitContainerInner";
            this.splitContainerInner.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerInner.Panel1
            // 
            this.splitContainerInner.Panel1.Controls.Add(this.feedStatusControl);
            // 
            // splitContainerInner.Panel2
            // 
            this.splitContainerInner.Panel2.Controls.Add(this.rebroadcastStatusControl);
            this.splitContainerInner.Size = new System.Drawing.Size(714, 244);
            this.splitContainerInner.SplitterDistance = 107;
            this.splitContainerInner.TabIndex = 8;
            // 
            // feedStatusControl
            // 
            this.feedStatusControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.feedStatusControl.Location = new System.Drawing.Point(0, 0);
            this.feedStatusControl.Name = "feedStatusControl";
            this.feedStatusControl.Size = new System.Drawing.Size(714, 107);
            this.feedStatusControl.TabIndex = 0;
            this.feedStatusControl.ReconnectFeedId += new System.EventHandler<VirtualRadar.WinForms.Controls.FeedIdEventArgs>(this.feedStatusControl_ReconnectFeedId);
            this.feedStatusControl.ShowFeedIdStatistics += new System.EventHandler<VirtualRadar.WinForms.Controls.FeedIdEventArgs>(this.feedStatusControl_ShowFeedIdStatistics);
            this.feedStatusControl.ResetPolarPlotter += new System.EventHandler<VirtualRadar.WinForms.Controls.FeedIdEventArgs>(this.feedStatusControl_ResetPolarPlotter);
            this.feedStatusControl.ConfigureFeed += new System.EventHandler<VirtualRadar.WinForms.Controls.FeedIdEventArgs>(this.feedStatusControl_ConfigureFeed);
            // 
            // rebroadcastStatusControl
            // 
            this.rebroadcastStatusControl.Configuration = "None";
            this.rebroadcastStatusControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rebroadcastStatusControl.Location = new System.Drawing.Point(0, 0);
            this.rebroadcastStatusControl.Name = "rebroadcastStatusControl";
            this.rebroadcastStatusControl.Size = new System.Drawing.Size(714, 133);
            this.rebroadcastStatusControl.TabIndex = 7;
            this.rebroadcastStatusControl.ShowRebroadcastServersConfigurationClicked += new System.EventHandler(this.rebroadcastStatusControl_ShowRebroadcastServersConfigurationClicked);
            // 
            // webServerStatusControl
            // 
            this.webServerStatusControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webServerStatusControl.Location = new System.Drawing.Point(0, 0);
            this.webServerStatusControl.Name = "webServerStatusControl";
            this.webServerStatusControl.Size = new System.Drawing.Size(714, 273);
            this.webServerStatusControl.TabIndex = 1;
            this.webServerStatusControl.ToggleServerStatus += new System.EventHandler(this.webServerStatusControl_ToggleServerStatus);
            this.webServerStatusControl.ToggleUPnpStatus += new System.EventHandler(this.webServerStatusControl_ToggleUPnpStatus);
            // 
            // splitContainerOuter
            // 
            this.splitContainerOuter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerOuter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerOuter.Location = new System.Drawing.Point(12, 27);
            this.splitContainerOuter.Name = "splitContainerOuter";
            this.splitContainerOuter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerOuter.Panel1
            // 
            this.splitContainerOuter.Panel1.Controls.Add(this.webServerStatusControl);
            // 
            // splitContainerOuter.Panel2
            // 
            this.splitContainerOuter.Panel2.Controls.Add(this.splitContainerInner);
            this.splitContainerOuter.Size = new System.Drawing.Size(714, 521);
            this.splitContainerOuter.SplitterDistance = 273;
            this.splitContainerOuter.TabIndex = 0;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStripNotifyIcon;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Text goes here";
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // contextMenuStripNotifyIcon
            // 
            this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showWindowToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";
            this.contextMenuStripNotifyIcon.Size = new System.Drawing.Size(196, 48);
            // 
            // showWindowToolStripMenuItem
            // 
            this.showWindowToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.showWindowToolStripMenuItem.Name = "showWindowToolStripMenuItem";
            this.showWindowToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.showWindowToolStripMenuItem.Text = "::menuShowWindow::";
            this.showWindowToolStripMenuItem.Click += new System.EventHandler(this.showWindowToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exitToolStripMenuItem.Text = "::menuExit::";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Enabled = true;
            this.timerRefresh.Interval = 1000;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 573);
            this.Controls.Add(this.splitContainerOuter);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainView";
            this.Text = "::VirtualRadarServer::";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainerInner.Panel1.ResumeLayout(false);
            this.splitContainerInner.Panel2.ResumeLayout(false);
            this.splitContainerInner.ResumeLayout(false);
            this.splitContainerOuter.Panel1.ResumeLayout(false);
            this.splitContainerOuter.Panel2.ResumeLayout(false);
            this.splitContainerOuter.ResumeLayout(false);
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuCheckForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem menuAboutToolStripMenuItem;
        private Controls.WebServerStatusControl webServerStatusControl;
        private System.Windows.Forms.ToolStripMenuItem menuConnectionSessionLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuConnectionClientLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem menuFlightSimulatorXModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuOpenVirtualRadarLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuReconnectToDataFeedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem menuDownloadDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem menuOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPluginsToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonLaterVersionAvailable;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonInvalidPluginCount;
        private Controls.RebroadcastStatusControl rebroadcastStatusControl;
        private System.Windows.Forms.SplitContainer splitContainerInner;
        private System.Windows.Forms.ToolStripMenuItem menuStatisticsToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerOuter;
        private Controls.FeedStatusControl feedStatusControl;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem showWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuResetReceiverRangeToolStripMenuItem;
        private System.Windows.Forms.Timer timerRefresh;
    }
}