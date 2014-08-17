using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that takes a binding list of CIDR addresses, shows it to the user
    /// and lets them edit it.
    /// </summary>
    public partial class BindingCidrList : BaseUserControl
    {
        #region Private class - Comparer
        /// <summary>
        /// An implementation of <see cref="AutoListViewSorter"/> that adds automatic
        /// sorting to the list.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            private BindingCidrList _List;

            public Sorter(BindingCidrList list) : base(list.bindingListView.ListView)
            {
                _List = list;
            }

            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                var cidrText = listViewItem.Text;
                Cidr cidr;
                Cidr.TryParse(cidrText, out cidr);

                IComparable result = base.GetRowValue(listViewItem);
                if(SortColumn == null || SortColumn == _List.columnHeaderCidr)  result = new ByteArrayComparable(cidr);
                else if(SortColumn == _List.columnHeaderFromAddress)            result = new ByteArrayComparable(cidr.FirstMatchingAddress);
                else if(SortColumn == _List.columnHeaderToAddress)              result = new ByteArrayComparable(cidr.LastMatchingAddress);

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that auto-sorts the list for us.
        /// </summary>
        private Sorter _Sorter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the list of CIDR addresses to bind to.
        /// </summary>
        public object DataSource
        {
            get { return bindingListView.DataSource; }
            set { bindingListView.DataSource = value; }
        }

        /// <summary>
        /// Gets the list of CIDR addresses that the control is bound to.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BindingList<string> CidrList
        {
            get {
                var currencyManager = BindingContext[DataSource] as CurrencyManager;
                var result = currencyManager == null ? null : currencyManager.List as BindingList<string>;
                return result;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the <see cref="DataSource"/> changes.
        /// </summary>
        public event EventHandler DataSourceChanged;

        /// <summary>
        /// Raises <see cref="DataSourceChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDataSourceChanged(EventArgs args)
        {
            if(DataSourceChanged != null) DataSourceChanged(this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BindingCidrList()
        {
            InitializeComponent();
            _Sorter = new Sorter(this);
            bindingListView.ListView.ListViewItemSorter = _Sorter;
        }
        #endregion

        #region AddOrInsertCidr
        /// <summary>
        /// Adds or inserts the CIDR address to the CIDR list.
        /// </summary>
        /// <param name="cidrList"></param>
        /// <param name="cidrText"></param>
        /// <param name="index"></param>
        private void AddOrInsertCidr(BindingList<string> cidrList, string cidrText, int index = -1)
        {
            Cidr cidr;
            if(Cidr.TryParse(cidrText, out cidr)) {
                var normalisedCidr = String.Format("{0}/{1}", cidr.MaskedAddress, cidr.BitmaskBits);
                if(!cidrList.Contains(normalisedCidr)) {
                    if(index == -1 || index >= cidrList.Count) cidrList.Add(normalisedCidr);
                    else                                       cidrList.Insert(index, normalisedCidr);
                }

                bindingListView.SelectedRecord = normalisedCidr;
            }
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the control has loaded but before it's shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                _Sorter.RefreshSortIndicators();
            }
        }
        #endregion

        #region Events subscribed
        private void bindingListView_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            Cidr cidr = null;
            var cidrText = e.Record as string;
            if(!String.IsNullOrEmpty(cidrText)) Cidr.TryParse(cidrText, out cidr);

            e.ColumnTexts.Add(cidrText);
            e.ColumnTexts.Add(cidr == null ? "" : cidr.FirstMatchingAddress.ToString());
            e.ColumnTexts.Add(cidr == null ? "" : cidr.LastMatchingAddress.ToString());
        }

        private void bindingListView_AddClicked(object sender, EventArgs e)
        {
            var cidrList = CidrList;
            if(cidrList != null) {
                using(var dialog = new CidrEditView()) {
                    dialog.ShowDialog();
                    if(dialog.CidrIsValid) {
                        AddOrInsertCidr(cidrList, dialog.Cidr);
                    }
                }
            }
        }

        private void bindingListView_EditClicked(object sender, EventArgs e)
        {
            var cidrList = CidrList;
            if(cidrList != null) {
                var selectedCidrText = bindingListView.SelectedRecord as string;
                var index = selectedCidrText == null ? -1 : cidrList.IndexOf(selectedCidrText);
                if(index != -1) {
                    using(var dialog = new CidrEditView()) {
                        dialog.Cidr = selectedCidrText;
                        dialog.ShowDialog();
                        if(dialog.CidrIsValid) {
                            cidrList.RemoveAt(index);
                            AddOrInsertCidr(cidrList, dialog.Cidr, index);
                        }
                    }
                }
            }
        }

        private void bindingListView_DeleteClicked(object sender, EventArgs e)
        {
            var cidrList = CidrList;
            if(cidrList != null) {
                var selectedCidrs = bindingListView.SelectedRecords.OfType<string>().ToArray();
                foreach(var selectedCidr in selectedCidrs) {
                    var index = selectedCidr == null ? -1 : cidrList.IndexOf(selectedCidr);
                    if(index != -1) cidrList.RemoveAt(index);
                }
            }
        }
        #endregion
    }
}
