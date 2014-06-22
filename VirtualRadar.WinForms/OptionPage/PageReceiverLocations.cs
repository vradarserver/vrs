using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageReceiverLocations : Page
    {
        public override string PageTitle { get { return Strings.ReceiverLocationsTitle; } }

        public override Image PageIcon { get { return Images.iconmonstr_location_3_icon; } }

        public ObservableList<ReceiverLocation> ReceiverLocations { get; private set; }

        public PageReceiverLocations()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            ReceiverLocations = BindListProperty<ReceiverLocation>(listReceiverLocations);
        }
    }
}
