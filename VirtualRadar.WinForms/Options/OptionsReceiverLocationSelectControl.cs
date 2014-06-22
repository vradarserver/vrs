using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// A user control that presents a list of receiver locations. Intended for use on Options pages only.
    /// </summary>
    public partial class OptionsReceiverLocationSelectControl : BaseUserControl
    {
        /// <summary>
        /// A private class that describes a configuration item.
        /// </summary>
        class ReceiverLocationConfiguration
        {
            public int Id { get; private set; }
            public string Description { get; set; }

            public ReceiverLocationConfiguration(int id, string description)
            {
                Id = id;
                Description = description;
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as ReceiverLocationConfiguration;
                    result = other != null && other.Id == Id;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        /// <summary>
        /// The parent options view.
        /// </summary>
        private OptionsPropertySheetView _ParentOptionsView;

        /// <summary>
        /// The feed ID that had been selected for the control before the control was first populated, if any.
        /// </summary>
        private int _InitialSelectedLocationId = 0;

        /// <summary>
        /// Set to true once the control has been populated.
        /// </summary>
        private bool _Populated;

        /// <summary>
        /// Gets or sets the selected receiver location ID.
        /// </summary>
        /// <returns></returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedLocationId
        {
            get { return !_Populated ? _InitialSelectedLocationId : GetSelectedComboBoxValue<ReceiverLocationConfiguration>(comboBox, new ReceiverLocationConfiguration(0, "")).Id; }
            set
            {
                if(!_Populated) _InitialSelectedLocationId = value;
                else            SelectComboBoxItemByValue(comboBox, new ReceiverLocationConfiguration(value, ""));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating that the user can elect to have no receiver selected.
        /// Such a receiver gets an identifier of 0.
        /// </summary>
        [DefaultValue(false)]
        public bool ShowNoneOption { get; set; }

        /// <summary>
        /// Raised when <see cref="SelectedLocationId"/> changes.
        /// </summary>
        public event EventHandler SelectedLocationIdChanged;

        /// <summary>
        /// Raises <see cref="SelectedLocationIdChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSelectedLocationIdChanged(EventArgs args)
        {
            if(SelectedLocationIdChanged != null) SelectedLocationIdChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsReceiverLocationSelectControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called after the control has loaded but before it's shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                for(var parent = Parent;parent != null;parent = parent.Parent) {
                    _ParentOptionsView = parent as OptionsPropertySheetView;
                    if(_ParentOptionsView != null) break;
                }
                if(_ParentOptionsView == null) throw new InvalidOperationException("The OptionsReceiverLocationSelectControl can only be used on the Options screen or one of its children");

                Populate();
            }
        }

        /// <summary>
        /// Forces the control to be populated with items. Attempts to preserve the selected item.
        /// </summary>
        public void Populate()
        {
            var selectedItem = SelectedLocationId;
            _Populated = true;

            var items = new List<ReceiverLocationConfiguration>();
            if(_ParentOptionsView != null) {
                items.AddRange(_ParentOptionsView.RawDecodingReceiverLocations.Select(r => new ReceiverLocationConfiguration(r.UniqueId, r.Name)));
            }
            items.Sort((lhs, rhs) => String.Compare(lhs.Description ?? "", rhs.Description ?? "", ignoreCase: true));

            if(ShowNoneOption) {
                var noneItem = new ReceiverLocationConfiguration(0, String.Format("<{0}>", Strings.None));
                items.Insert(0, noneItem);
            }

            FillDropDownWithValues(comboBox, items, r => r.Description);

            SelectedLocationId = selectedItem;
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedLocationIdChanged(e);
        }

        private void comboBox_DropDown(object sender, EventArgs e)
        {
            Populate();
        }
    }
}
