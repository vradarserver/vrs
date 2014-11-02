// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using InterfaceFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.Interface.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// The base class for all setting pages.
    /// </summary>
    public partial class Page : BaseUserControl
    {
        #region Private class - InlineHelp
        /// <summary>
        /// Describes an article of inline help.
        /// </summary>
        protected class InlineHelp
        {
            /// <summary>
            /// Gets or sets the help text's title.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the help text.
            /// </summary>
            public string Help { get; set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="title"></param>
            /// <param name="help"></param>
            public InlineHelp(string title, string help)
            {
                Title = title;
                Help = (help ?? "").Replace(@"\r", "\r").Replace(@"\n", "\n");
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// True when the page has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// A map of controls to their inline help.
        /// </summary>
        protected Dictionary<Control, InlineHelp> _InlineHelpMap = new Dictionary<Control,InlineHelp>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the page summary associated with this page.
        /// </summary>
        public PageSummary PageSummary { get; set; }

        /// <summary>
        /// If true then the page is stretched vertically to fill the available space.
        /// </summary>
        public virtual bool PageUseFullHeight { get { return false; } }

        private SettingsView _SettingsView;
        /// <summary>
        /// Gets the owning settings view.
        /// </summary>
        public SettingsView SettingsView
        {
            get { return PageSummary.SettingsView; }
        }

        /// <summary>
        /// Gets a collection of every child page under this page.
        /// </summary>
        public NotifyList<Page> ChildPages { get; private set; }

        /// <summary>
        /// Gets a value indicating that the page is on display in the <see cref="SettingsView"/>.
        /// </summary>
        public bool IsInSettingsView { get { return PageSummary.TreeNode != null; } }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Page()
        {
            ChildPages = new NotifyList<Page>();

            InitializeComponent();
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the page has been loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Control(this);
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// Initialises the page.
        /// </summary>
        public void Initialise()
        {
            if(!_Initialised) {
                _Initialised = true;

                InitialiseControls();
                CreateBindings();
                InitialiseControlBinders();
                AssociateValidationFields();
                AssociateInlineHelp();
            }
        }

        /// <summary>
        /// Called before anything is bound to the controls - gives the derivee the opportunity
        /// to initialise controls that need to be initialised.
        /// </summary>
        protected virtual void InitialiseControls()
        {
            ;
        }

        /// <summary>
        /// When overridden by the derivee this creates bindings between the page and objects
        /// on the owning <see cref="SettingsView"/>.
        /// </summary>
        protected virtual void CreateBindings()
        {
            ;
        }

        /// <summary>
        /// Removes all bindings from the page.
        /// </summary>
        protected virtual void ClearBindings()
        {
            foreach(var bindingControl in GetAllChildControls(includeThis: true).Where(r => r.DataBindings.Count > 0)) {
                bindingControl.DataBindings.Clear();
            }
        }

        /// <summary>
        /// Associates controls with validation fields.
        /// </summary>
        protected virtual void AssociateValidationFields()
        {
            ;
        }

        /// <summary>
        /// Associates controls with inline help.
        /// </summary>
        protected virtual void AssociateInlineHelp()
        {
            ;
        }
        #endregion

        #region SetValidationFields
        /// <summary>
        /// Associates validation fields with controls.
        /// </summary>
        /// <param name="fields"></param>
        protected void SetValidationFields(Dictionary<ValidationField, Control> fields)
        {
            PageSummary.SetValidationFields(fields);
        }
        #endregion

        #region SetInlineHelp
        /// <summary>
        /// Sets the inline help up for the control passed across.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="title"></param>
        /// <param name="help"></param>
        protected virtual void SetInlineHelp(Control control, string title, string help)
        {
            if(_InlineHelpMap.ContainsKey(control)) {
                control.Enter -= Control_InlineHelp_Enter;
                _InlineHelpMap.Remove(control);
            }

            _InlineHelpMap.Add(control, new InlineHelp(title, help));
            control.Enter += Control_InlineHelp_Enter;
            control.Leave += Control_InlineHelp_Leave;
        }

        /// <summary>
        /// Displays the inline help associated with the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_InlineHelp_Enter(object sender, EventArgs args)
        {
            InlineHelp help;
            if(_InlineHelpMap.TryGetValue((Control)sender, out help)) {
                if(SettingsView != null) {
                    SettingsView.InlineHelpTitle = help.Title;
                    SettingsView.InlineHelp = help.Help;
                }
            }
        }

        /// <summary>
        /// Removes the inline help associated with the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_InlineHelp_Leave(object sender, EventArgs args)
        {
            if(_InlineHelpMap.ContainsKey((Control)sender)) {
                if(SettingsView != null) {
                    SettingsView.InlineHelpTitle = "";
                    SettingsView.InlineHelp = "";
                }
            }
        }
        #endregion

        #region PageSelected, ConfigurationChanged, PageDetached
        /// <summary>
        /// Called after the page is selected.
        /// </summary>
        internal virtual void PageSelected()
        {
        }

        /// <summary>
        /// Called when a configuration property somewhere has changed.
        /// </summary>
        /// <param name="args"></param>
        internal virtual void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
        }

        /// <summary>
        /// Called when the users list, or any user in the list, changes.
        /// </summary>
        /// <param name="users"></param>
        /// <param name="args"></param>
        internal virtual void UsersChanged(IList<IUser> users, ListChangedEventArgs args)
        {
        }

        /// <summary>
        /// Called when the page is about to be removed from display while the parent form is still active.
        /// </summary>
        internal virtual void PageDetaching()
        {
        }

        /// <summary>
        /// Called when the page has been removed from display while the parent form is still active.
        /// </summary>
        internal virtual void PageDetached()
        {
        }
        #endregion
    }
}
