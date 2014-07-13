using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using System.Collections.ObjectModel;
using VirtualRadar.Interface.View;

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
                Help = help;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// A map of validation fields to the controls that they represent.
        /// </summary>
        protected Dictionary<ValidationField, Control> _ValidationFieldControlMap = new Dictionary<ValidationField,Control>();

        /// <summary>
        /// A map of controls to their inline help.
        /// </summary>
        protected Dictionary<Control, InlineHelp> _InlineHelpMap = new Dictionary<Control,InlineHelp>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the title for the page.
        /// </summary>
        public virtual string PageTitle
        {
            get {
                return "Override PageTitle";
            }
        }
        
        /// <summary>
        /// Gets a value indicating that the page should be shown as enabled in the list of pages.
        /// </summary>
        public virtual bool PageEnabled
        {
            get {
                var result = true;

                return result;
            }
        }

        /// <summary>
        /// Gets the icon for the page.
        /// </summary>
        public virtual Image PageIcon { get { return null; } }

        /// <summary>
        /// Gets the settings object being modified by this page.
        /// </summary>
        public virtual object PageObject { get; protected set; }

        /// <summary>
        /// If true then the page is stretched vertically to fill the available space.
        /// </summary>
        public virtual bool PageUseFullHeight { get { return false; } }

        private SettingsView _SettingsView;
        /// <summary>
        /// Gets or sets the owning settings view.
        /// </summary>
        public SettingsView SettingsView
        {
            get { return _SettingsView; }
            set {
                if(_SettingsView != value) {
                    _SettingsView = value;
                    DoCreateBindings();
                }
            }
        }

        /// <summary>
        /// Gets a collection of every child page under this page.
        /// </summary>
        public ObservableCollection<Page> ChildPages { get; private set; }

        /// <summary>
        /// Gets or sets the tree node that represents this page in the owner view.
        /// </summary>
        public TreeNode TreeNode { get; set; }

        /// <summary>
        /// Gets a value indicating that the page is on display in the <see cref="SettingsView"/>.
        /// </summary>
        public bool IsInSettingsView { get { return TreeNode != null; } }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Page()
        {
            ChildPages = new ObservableCollection<Page>();

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

        #region CreateBindings
        /// <summary>
        /// Calls CreateBindings if the conditions are right.
        /// </summary>
        private void DoCreateBindings()
        {
            if(SettingsView == null) {
                ClearBindings();
            } else {
                InitialiseControls();
                CreateBindings();
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
            foreach(var binding in GetAllDataBindings(true)) {
                binding.Control.DataBindings.Clear();
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

        #region SetValidationFields, GetControlForValidationField
        /// <summary>
        /// Records which controls display which validation fields.
        /// </summary>
        /// <param name="fields"></param>
        protected void SetValidationFields(Dictionary<ValidationField, Control> fields)
        {
            foreach(var field in fields) {
                var validationField = field.Key;
                var control = field.Value;

                SettingsView.SetControlErrorAlignment(control, ErrorIconAlignment.MiddleLeft);

                if(!_ValidationFieldControlMap.ContainsKey(field.Key)) {
                    _ValidationFieldControlMap.Add(validationField, control);
                } else {
                    _ValidationFieldControlMap[validationField] = control;
                }
            }
        }

        /// <summary>
        /// Returns the control associated with the field passed across. Returns null if the pageObject passed
        /// across does not match the pageObject held by the page.
        /// </summary>
        /// <param name="pageObject"></param>
        /// <param name="validationField"></param>
        /// <returns></returns>
        public Control GetControlForValidationField(object pageObject, ValidationField validationField)
        {
            Control result = null;

            if(pageObject == PageObject) {
                _ValidationFieldControlMap.TryGetValue(validationField, out result);
            }

            return result;
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
        #endregion

        #region PageSelected
        /// <summary>
        /// Called after the page is selected.
        /// </summary>
        public virtual void PageSelected()
        {
            ;
        }
        #endregion
    }
}
