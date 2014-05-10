// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using System.Diagnostics;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="IAboutView"/>.
    /// </summary>
    public partial class AboutView : BaseForm, IAboutView
    {
        /// <summary>
        /// The presenter that is managing this view.
        /// </summary>
        private IAboutPresenter _Presenter;

        private OnlineHelpHelper _OnlineHelp;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Caption
        {
            get { return Text; }
            set { Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public new string ProductName
        {
            get { return labelProductName.Text; }
            set { labelProductName.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version
        {
            get { return labelVersion.Text; }
            set { labelVersion.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Copyright
        {
            get { return labelCopyright.Text; }
            set { labelCopyright.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Description
        {
            get { return textBoxDescription.Text; }
            set { textBoxDescription.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ConfigurationFolder
        {
            get { return linkLabelWorkingFolder.Text; }
            set { linkLabelWorkingFolder.Text = value; }
        }

        private bool _IsMono;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsMono
        {
            get { return _IsMono; }
            set { _IsMono = value; labelEnvironment.Text = value ? "Environment: Mono" : "Environment: .NET"; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler OpenConfigurationFolderClicked;

        /// <summary>
        /// Raises <see cref="OpenConfigurationFolderClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnOpenConfigurationFolderClicked(EventArgs args)
        {
            if(OpenConfigurationFolderClicked != null) OpenConfigurationFolderClicked(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AboutView() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when the form has been created but before it is on display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);
                logoPictureBox.Image = Resources.Images.HelpAbout;

                _Presenter = Factory.Singleton.Resolve<IAboutPresenter>();
                _Presenter.Initialise(this);

                _OnlineHelp = new OnlineHelpHelper(this, OnlineHelpAddress.WinFormsAboutDialog);
            }
        }

        /// <summary>
        /// Displays the content of the configuration folder to the user.
        /// </summary>
        public void ShowConfigurationFolderContents()
        {
            Process.Start(linkLabelWorkingFolder.Text);
        }

        /// <summary>
        /// Raised when the user clicks the link to display the configuration folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelWorkingFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OnOpenConfigurationFolderClicked(EventArgs.Empty);
        }
    }
}
