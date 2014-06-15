using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// A user control for use on the Options screen that lets the user choose a receiver and/or
    /// merged feed.
    /// </summary>
    public partial class OptionsFeedSelectControl : BaseUserControl
    {
        /// <summary>
        /// A private class that describes a feed configuration item.
        /// </summary>
        class FeedConfiguration
        {
            public int Id { get; private set; }
            public string Description { get; set; }

            public FeedConfiguration(int id, string description)
            {
                Id = id;
                Description = description;
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as FeedConfiguration;
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
        /// The parent options screen. Note that this control is only intended for use on option screen
        /// pages, it works with receiver and merged feed configuration items - not the feeds that are
        /// eventually built from those items. There's another control elsewhere for selecting those.
        /// </summary>
        private OptionsPropertySheetView _ParentOptionsScreen;

        /// <summary>
        /// The feed ID that had been selected for the control before the control was first populated, if any.
        /// </summary>
        private int _InitialSelectedFeedId = 0;

        /// <summary>
        /// Set to true once the control has been populated.
        /// </summary>
        private bool _Populated;

        /// <summary>
        /// Gets or sets the identifier of the selected receiver or merged feed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedFeedId
        {
            get { return !_Populated ? _InitialSelectedFeedId : GetSelectedComboBoxValue<FeedConfiguration>(comboBox, new FeedConfiguration(0, "")).Id; }
            set
            {
                if(!_Populated) _InitialSelectedFeedId = value;
                else            SelectComboBoxItemByValue(comboBox, new FeedConfiguration(value, null));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating that receivers should be shown in the list.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowReceivers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that merged feeds should be shown in the list.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowMergedFeeds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user can elect to have no receiver selected.
        /// Such a receiver gets an identifier of 0.
        /// </summary>
        [DefaultValue(false)]
        public bool ShowNoneOption { get; set; }

        /// <summary>
        /// Raised when <see cref="SelectedFeedId"/> changes.
        /// </summary>
        public event EventHandler SelectedFeedIdChanged;

        /// <summary>
        /// Raises <see cref="SelectedFeedIdChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSelectedFeedIdChanged(EventArgs args)
        {
            if(SelectedFeedIdChanged != null) SelectedFeedIdChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsFeedSelectControl()
        {
            InitializeComponent();
            ShowReceivers = true;
            ShowMergedFeeds = true;
        }

        /// <summary>
        /// Called after the control has been loaded but before it is shown on screen.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                for(var parent = Parent;parent != null;parent = parent.Parent) {
                    _ParentOptionsScreen = parent as OptionsPropertySheetView;
                    if(_ParentOptionsScreen != null) break;
                }
                if(_ParentOptionsScreen == null) throw new InvalidOperationException("The OptionsFeedSelectControl can only be used on the Options screen or one of its children");

                Populate();
            }
        }

        /// <summary>
        /// Forces the control to be populated with items. Attempts to preserve the selected item.
        /// </summary>
        public void Populate()
        {
            var selectedItem = SelectedFeedId;
            _Populated = true;

            var items = new List<FeedConfiguration>();
            if(ShowReceivers) AddReceivers(items);
            if(ShowMergedFeeds) AddMergedFeeds(items);
            items.Sort((lhs, rhs) => String.Compare(lhs.Description ?? "", rhs.Description ?? "", ignoreCase: true));

            if(ShowNoneOption) {
                var noneItem = new FeedConfiguration(0, String.Format("<{0}>", Strings.None));
                items.Insert(0, noneItem);
            }

            FillDropDownWithValues(comboBox, items, r => r.Description);

            SelectedFeedId = selectedItem;
        }

        private void AddReceivers(List<FeedConfiguration> items)
        {
            foreach(var receiver in _ParentOptionsScreen.Receivers) {
                var item = new FeedConfiguration(receiver.UniqueId, receiver.Name);
                items.Add(item);
            }
        }

        private void AddMergedFeeds(List<FeedConfiguration> items)
        {
            foreach(var mergedFeed in _ParentOptionsScreen.MergedFeeds) {
                var item = new FeedConfiguration(mergedFeed.UniqueId, mergedFeed.Name);
                items.Add(item);
            }
        }

        /// <summary>
        /// Called when the selected feed ID changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedFeedIdChanged(e);
        }

        /// <summary>
        /// Called when the dropdown is shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_DropDown(object sender, EventArgs e)
        {
            Populate();
        }
    }
}
