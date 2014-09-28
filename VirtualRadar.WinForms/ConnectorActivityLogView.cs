using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="IConnectorActivityLogView"/>.
    /// </summary>
    public partial class ConnectorActivityLogView : BaseForm, IConnectorActivityLogView
    {
        /// <summary>
        /// Gets the presenter that is controlling the view.
        /// </summary>
        private IConnectorActivityLogPresenter _Presenter;

        /// <summary>
        /// True while the controls are being populated.
        /// </summary>
        private bool _Populating;

        /// <summary>
        /// The text that represents the none option in filters.
        /// </summary>
        private string _NoneText;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IConnector Connector { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HideConnectorName { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ConnectorActivityEvent[] SelectedConnectorActivityEvents
        {
            get { return GetAllSelectedListViewTag<ConnectorActivityEvent>(listView); }
            set { SelectListViewItemsByTags(listView, value); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ConnectorActivityEvent[] ConnectorActivityEvents
        {
            get { return listView.Items.OfType<ListViewItem>().Select(r => r.Tag as ConnectorActivityEvent).Where(r => r != null).ToArray(); }
        }

        /// <summary>
        /// Gets or sets the selected connector filter.
        /// </summary>
        public string SelectedConnectorFilter
        {
            get { return comboBoxConnectorFilter.SelectedItem as string; }
            set { comboBoxConnectorFilter.SelectedItem = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler RefreshClicked;

        /// <summary>
        /// Raises <see cref="RefreshClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnRefreshClicked(EventArgs args)
        {
            if(RefreshClicked != null) RefreshClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CopySelectedItemsToClipboardClicked;

        /// <summary>
        /// Raises <see cref="CopySelectedItemsToClipboardClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCopySelectedItemsToClipboardClicked(EventArgs args)
        {
            if(CopySelectedItemsToClipboardClicked != null) CopySelectedItemsToClipboardClicked(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectorActivityLogView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called after the control has loaded but before it displays anything to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);
                _NoneText = String.Format("<{0}>", Strings.None);

                if(HideConnectorName) {
                    listView.Columns.Remove(columnHeaderConnector);
                    columnHeaderMessage.Width += columnHeaderConnector.Width;
                }

                _Presenter = Factory.Singleton.Resolve<IConnectorActivityLogPresenter>();
                _Presenter.Initialise(this);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="connectorActivityEvents"></param>
        public void Populate(IEnumerable<ConnectorActivityEvent> connectorActivityEvents)
        {
            if(!_Populating) {
                _Populating = true;
                try {
                    PopulateConnectorFilter(connectorActivityEvents.Select(r => r.ConnectorName).Where(r => !String.IsNullOrEmpty(r)).Distinct());

                    var connectorFilter = SelectedConnectorFilter;
                    if(connectorFilter == _NoneText) connectorFilter = null;
                    var filteredConnectorActivityEvents = connectorFilter == null ? connectorActivityEvents : connectorActivityEvents.Where(r => r.ConnectorName == connectorFilter);
                    PopulateListView(listView, filteredConnectorActivityEvents, null, PopulateListViewItem, null);
                } finally {
                    _Populating = false;
                }
            }
        }

        /// <summary>
        /// Populates the combo box that holds the connector filters.
        /// </summary>
        /// <param name="connectorNames"></param>
        private void PopulateConnectorFilter(IEnumerable<string> connectorNames)
        {
            var selectedItem = SelectedConnectorFilter;
            comboBoxConnectorFilter.Items.Clear();
            comboBoxConnectorFilter.Items.Add(_NoneText);
            foreach(var name in connectorNames.OrderBy(r => r)) {
                comboBoxConnectorFilter.Items.Add(name);
            }

            SelectedConnectorFilter = connectorNames.Contains(selectedItem) ? selectedItem : _NoneText;
        }

        /// <summary>
        /// Populates an individual listview item.
        /// </summary>
        /// <param name="listViewItem"></param>
        private void PopulateListViewItem(ListViewItem listViewItem)
        {
            FillListViewItem<ConnectorActivityEvent>(listViewItem, r => {
                var time =      _Presenter.FormatTime(r.Time);
                var name =      r.ConnectorName ?? "";
                var activity =  Describe.ConnectorActivityType(r.Type);
                var message =   r.Message ?? "";
                return HideConnectorName ? new string[] {
                    time,
                    activity,
                    message
                } : new string[] {
                    time,
                    name,
                    activity,
                    message
                };
            });
        }

        private void buttonCopySelectedItemsToClipboard_Click(object sender, EventArgs e)
        {
            OnCopySelectedItemsToClipboardClicked(e);
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            OnRefreshClicked(e);
        }

        private void comboBoxConnectorFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!_Populating) {
                OnRefreshClicked(e);
            }
        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            var firstSelectedItem = SelectedConnectorActivityEvents.FirstOrDefault();
            if(firstSelectedItem != null) {
                var title = _Presenter.FormatDetailTitle(firstSelectedItem);
                var text = _Presenter.FormatDetailText(firstSelectedItem);
                MessageBox.Show(text, title);
            }
        }
    }
}
