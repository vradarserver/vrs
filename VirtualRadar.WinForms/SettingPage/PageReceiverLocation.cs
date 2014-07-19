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
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Allows data entry for a single receiver location.
    /// </summary>
    public partial class PageReceiverLocation : Page
    {
        public override Image PageIcon { get { return Images.Location16x16; } }

        public ReceiverLocation ReceiverLocation { get { return PageObject as ReceiverLocation; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageReceiverLocation()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            SetPageTitleProperty<ReceiverLocation>(r => r.Name, () => ReceiverLocation.Name);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(ReceiverLocation, textBoxName,       r => r.Name,        r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(ReceiverLocation, numericLatitude,   r => r.Latitude,    r => r.Value,   DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(ReceiverLocation, numericLongitude,  r => r.Longitude,   r => r.Value,   DataSourceUpdateMode.OnPropertyChanged);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<Interface.View.ValidationField,Control>() {
                { ValidationField.Location,     textBoxName },
                { ValidationField.Latitude,     numericLatitude },
                { ValidationField.Longitude,    numericLongitude },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(textBoxName,      Strings.Name,       Strings.OptionsDescribeReceiverLocationName);
            SetInlineHelp(numericLatitude,  Strings.Latitude,   Strings.OptionsDescribeReceiverLocationLatitude);
            SetInlineHelp(numericLongitude, Strings.Longitude,  Strings.OptionsDescribeReceiverLocationLongitude);
        }
    }
}
