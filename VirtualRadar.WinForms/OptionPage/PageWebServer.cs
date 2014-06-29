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
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageWebServer : Page
    {
        public override string PageTitle { get { return Strings.OptionsWebServerSheetTitle; } }

        public override Image PageIcon { get { return Images.Server16x16; } }

        public PageWebServerAuthentication Authentication { get; private set; }

        [LocalisedDisplayName("EnableUPnp")]
        [LocalisedDescription("OptionsDescribeWebServerEnableUPnpFeatures")]
        public Observable<bool> EnableUPnpFeatures { get; private set; }

        [LocalisedDisplayName("ResetPortAssignmentsOnStartup")]
        [LocalisedDescription("OptionsDescribeWebServerIsOnlyVirtualRadarServerOnLan")]
        public Observable<bool> IsOnlyVirtualRadarServerOnLan { get; private set; }

        [LocalisedDisplayName("AutoStartUPnP")]
        [LocalisedDescription("OptionsDescribeWebServerAutoStartUPnp")]
        public Observable<bool> AutoStartUPnp { get; private set; }

        [LocalisedDisplayName("UPnpPort")]
        [LocalisedDescription("OptionsDescribeWebServerUPnpPort")]
        [ValidationField(ValidationField.UPnpPortNumber)]
        public Observable<int> UPnpPort { get; private set; }

        [LocalisedDisplayName("InternetUsersCanRunReports")]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanRunReports")]
        public Observable<bool> InternetClientCanRunReports { get; private set; }

        [LocalisedDisplayName("InternetUsersCanListenToAudio")]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanPlayAudio")]
        public Observable<bool> InternetClientCanPlayAudio { get; private set; }

        [LocalisedDisplayName("InternetUsersCanViewPictures")]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanSeePictures")]
        public Observable<bool> InternetClientCanSeePictures { get; private set; }

        [LocalisedDisplayName("InternetUserIdleTimeout")]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientTimeoutMinutes")]
        [ValidationField(ValidationField.InternetUserIdleTimeout)]
        public Observable<int> InternetClientTimeoutMinutes { get; private set; }

        [LocalisedDisplayName("InternetUserCanSeeLabels")]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanSeeLabels")]
        public Observable<bool> InternetClientCanSeeLabels { get; private set; }

        [LocalisedDisplayName("AllowInternetProximityGadgets")]
        [LocalisedDescription("OptionsDescribeWebServerAllowInternetProximityGadgets")]
        public Observable<bool> AllowInternetProximityGadgets { get; private set; }

        [LocalisedDisplayName("InternetClientCanSubmitRoutes")]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanSubmitRoutes")]
        public Observable<bool> InternetClientCanSubmitRoutes { get; private set; }

        [LocalisedDisplayName("InternetClientCanShowPolarPlots")]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanShowPolarPlots")]
        public Observable<bool> InternetClientCanShowPolarPlots { get; private set; }

        public PageWebServer()
        {
            Authentication = new PageWebServerAuthentication();
            ChildPages.Add(Authentication);

            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            EnableUPnpFeatures =                BindProperty<bool>(checkBoxEnableUPnp);
            IsOnlyVirtualRadarServerOnLan =     BindProperty<bool>(checkBoxResetPortAssignmentsOnStartup);
            AutoStartUPnp =                     BindProperty<bool>(checkBoxAutoStartUPnP);
            UPnpPort =                          BindProperty<int>(numericUPnpPort);

            InternetClientCanRunReports =       BindProperty<bool>(checkBoxInternetUsersCanRunReports);
            InternetClientCanPlayAudio =        BindProperty<bool>(checkBoxInternetUsersCanListenToAudio);
            InternetClientCanSeePictures =      BindProperty<bool>(checkBoxInternetUsersCanViewPictures);
            InternetClientTimeoutMinutes =      BindProperty<int>(numericInternetUserIdleTimeout);
            InternetClientCanSeeLabels =        BindProperty<bool>(checkBoxInternetUserCanSeeLabels);
            AllowInternetProximityGadgets =     BindProperty<bool>(checkBoxAllowInternetProximityGadgets);
            InternetClientCanSubmitRoutes =     BindProperty<bool>(checkBoxInternetClientCanSubmitRoutes);
            InternetClientCanShowPolarPlots =   BindProperty<bool>(checkBoxInternetClientCanShowPolarPlots);
        }
    }
}
