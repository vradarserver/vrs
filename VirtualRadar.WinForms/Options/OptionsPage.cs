using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The base user control for sheets and parent pages that show editable controls to the user.
    /// </summary>
    public partial class OptionsPage : BaseUserControl
    {
        #region Private Class - ControlDescription
        /// <summary>
        /// The localised description assigned to a control.
        /// </summary>
        class ControlDescription
        {
            public string Title;
            public string Description;

            public ControlDescription(string title, string description)
            {
                Title = title;
                Description = description;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// A map of controls to their localised descriptions.
        /// </summary>
        private Dictionary<Control, ControlDescription> _LocalisedDescriptions = new Dictionary<Control,ControlDescription>();

        /// <summary>
        /// A list of controls that have had their Enter event hooked.
        /// </summary>
        private List<Control> _HookedEnterControls = new List<Control>();

        /// <summary>
        /// A list of controls that have had the appropriate ValueChanged event hooked.
        /// </summary>
        private List<Control> _HookedValueChangedControls = new List<Control>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the owning view.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OptionsPropertySheetView OptionsView { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Image Icon { get { return null; } }

        /// <summary>
        /// Gets or sets the description title label's text.
        /// </summary>
        private string DescriptionTitle
        {
            get { return labelDescriptionTitle.Text; }
            set { labelDescriptionTitle.Text = value; }
        }

        /// <summary>
        /// Gets or sets the description label's text.
        /// </summary>
        private string Description
        {
            get { return labelDescription.Text; }
            set { labelDescription.Text = value; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Event hookers
        /// <summary>
        /// Hooks the enter event for the control.
        /// </summary>
        /// <param name="control"></param>
        private void HookControlEnter(Control control)
        {
            if(!_HookedEnterControls.Contains(control)) {
                control.Enter += Control_Enter;
                _HookedEnterControls.Add(control);
            }
        }

        /// <summary>
        /// Hooks the value changed event for a control.
        /// </summary>
        /// <param name="control"></param>
        private void HookValueChanged(Control control)
        {
            if(!_HookedValueChangedControls.Contains(control)) {
                var hooked = false;

                if(!hooked) hooked = HookValueChanged(control as TextBox);
                if(!hooked) hooked = HookValueChanged(control as CheckBox);
                if(!hooked) hooked = HookValueChanged(control as FileNameControl);
                if(!hooked) hooked = HookValueChanged(control as FolderControl);
                if(!hooked) hooked = HookValueChanged(control as OptionsFeedSelectControl);
                if(!hooked) throw new NotImplementedException();

                _HookedValueChangedControls.Add(control);
            }
        }

        private bool HookValueChanged(TextBox control)
        {
            if(control != null) control.TextChanged += Control_ValueChanged;
            return control != null;
        }

        private bool HookValueChanged(CheckBox control)
        {
            if(control != null) control.CheckedChanged += Control_ValueChanged;
            return control != null;
        }

        private bool HookValueChanged(FileNameControl control)
        {
            if(control != null) control.FileNameTextChanged += Control_ValueChanged;
            return control != null;
        }

        private bool HookValueChanged(FolderControl control)
        {
            if(control != null) control.FolderTextChanged += Control_ValueChanged;
            return control != null;
        }

        private bool HookValueChanged(OptionsFeedSelectControl control)
        {
            if(control != null) control.SelectedFeedIdChanged += Control_ValueChanged;
            return control != null;
        }
        #endregion

        #region AddLocalisedDescription
        /// <summary>
        /// Attaches a localised description to a control.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        protected void AddLocalisedDescription(Control control, string title, string description)
        {
            if(!_LocalisedDescriptions.ContainsKey(control)) {
                _LocalisedDescriptions.Add(control, new ControlDescription(title, description));
                HookControlEnter(control);
            }
        }

        /// <summary>
        /// Displays the localised description associated with the control.
        /// </summary>
        /// <param name="control"></param>
        protected void DisplayLocalisedDescription(Control control)
        {
            ControlDescription description;
            if(!_LocalisedDescriptions.TryGetValue(control, out description)) {
                DescriptionTitle = Description = "";
            } else {
                DescriptionTitle = description.Title;
                Description = description.Description;
            }
        }
        #endregion

        #region RaisesValueChanged, DoValueChanged
        /// <summary>
        /// Registers a control that raises the ValueChanged event.
        /// </summary>
        /// <param name="control"></param>
        protected void RaisesValueChanged(Control control)
        {
            HookValueChanged(control);
        }

        /// <summary>
        /// Registers many controls that raise the ValueChanged event.
        /// </summary>
        /// <param name="controls"></param>
        protected void RaisesValueChanged(params Control[] controls)
        {
            foreach(var control in controls) {
                RaisesValueChanged(control);
            }
        }

        /// <summary>
        /// Raises the ValueChanged event on the owning form. Would have called this
        /// RaiseValueChanged but the potential for Intellisense confusion was too
        /// great.
        /// </summary>
        /// <param name="control"></param>
        protected void DoValueChanged(Control control)
        {
            if(_HookedValueChangedControls.Contains(control)) {
                OptionsView.RaiseValueChanged(control, EventArgs.Empty);
            }
        }
        #endregion

        #region Event handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                DescriptionTitle = "";
                Description = "";
                Localise.Control(this);
                _ValidationHelper = new ValidationHelper(errorProvider, warningProvider);
            }
        }

        private void Control_Enter(object sender, EventArgs args)
        {
            DisplayLocalisedDescription((Control)sender);
        }

        private void Control_ValueChanged(object sender, EventArgs args)
        {
            DoValueChanged((Control)sender);
        }
        #endregion
    }
}
