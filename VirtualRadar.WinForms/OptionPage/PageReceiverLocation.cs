using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Resources;
using VirtualRadar.Interface.Settings;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageReceiverLocation : Page
    {
        public override Image PageIcon { get { return Images.iconmonstr_location_3_icon; } }

        public ReceiverLocation ReceiverLocation { get { return PageObject as ReceiverLocation; } }

        [PageTitle]
        [LocalisedDisplayName("Name")]
        [LocalisedDescription("OptionsDescribeReceiverLocationName")]
        [ValidationField(ValidationField.Location)]
        public Observable<string> RecordName { get; private set; }

        [LocalisedDisplayName("Latitude")]
        [LocalisedDescription("OptionsDescribeReceiverLocationLatitude")]
        [ValidationField(ValidationField.Latitude)]
        public Observable<double> Latitude { get; private set; }

        [LocalisedDisplayName("Longitude")]
        [LocalisedDescription("OptionsDescribeReceiverLocationLongitude")]
        [ValidationField(ValidationField.Longitude)]
        public Observable<double> Longitude { get; private set; }

        public PageReceiverLocation()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RecordName = BindProperty<string>(textBoxName);
            Latitude = BindProperty<double>(numericLatitude);
            Longitude = BindProperty<double>(numericLongitude);
        }

        protected override void CopyRecordToObservables()
        {
            RecordName.Value = ReceiverLocation.Name;
            Latitude.Value = ReceiverLocation.Latitude;
            Longitude.Value = ReceiverLocation.Longitude;
        }

        protected override void CopyObservablesToRecord()
        {
            ReceiverLocation.Name = RecordName.Value;
            ReceiverLocation.Latitude = Latitude.Value;
            ReceiverLocation.Longitude = Longitude.Value;
        }
    }
}
