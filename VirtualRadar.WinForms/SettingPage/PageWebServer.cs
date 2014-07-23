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
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Lets the user view and change the web server settings.
    /// </summary>
    public partial class PageWebServer : Page
    {
        public override string PageTitle { get { return Strings.OptionsWebServerSheetTitle; } }

        public override Image PageIcon { get { return Images.Server16x16; } }

        public PageWebServer()
        {
            InitializeComponent();
        }

        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            ChildPages.Add(new PageWebServerAuthentication());
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, checkBoxEnableUPnp,                        r => r.Configuration.WebServerSettings.EnableUPnp,                  r => r.Checked);
            AddBinding(SettingsView, checkBoxResetPortAssignmentsOnStartup,     r => r.Configuration.WebServerSettings.IsOnlyInternetServerOnLan,   r => r.Checked);
            AddBinding(SettingsView, checkBoxAutoStartUPnP,                     r => r.Configuration.WebServerSettings.AutoStartUPnP,               r => r.Checked);
            AddBinding(SettingsView, numericUPnpPort,                           r => r.Configuration.WebServerSettings.UPnpPort,                    r => r.Value);

            AddBinding(SettingsView, numericInternetUserIdleTimeout,            r => r.Configuration.InternetClientSettings.TimeoutMinutes,                 r => r.Value);
            AddBinding(SettingsView, checkBoxInternetUsersCanRunReports,        r => r.Configuration.InternetClientSettings.CanRunReports,                  r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetUsersCanListenToAudio,     r => r.Configuration.InternetClientSettings.CanPlayAudio,                   r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetUsersCanViewPictures,      r => r.Configuration.InternetClientSettings.CanShowPictures,                r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetUserCanSeeLabels,          r => r.Configuration.InternetClientSettings.CanShowPinText,                 r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetClientCanSubmitRoutes,     r => r.Configuration.InternetClientSettings.CanSubmitRoutes,                r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetClientCanShowPolarPlots,   r => r.Configuration.InternetClientSettings.CanShowPolarPlots,              r => r.Checked);
            AddBinding(SettingsView, checkBoxAllowInternetProximityGadgets,     r => r.Configuration.InternetClientSettings.AllowInternetProximityGadgets,  r => r.Checked);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.UPnpPortNumber,           numericUPnpPort },
                { ValidationField.InternetUserIdleTimeout,  numericInternetUserIdleTimeout },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnableUPnp,                       Strings.EnableUPnp,                     Strings.OptionsDescribeWebServerEnableUPnpFeatures);
            SetInlineHelp(checkBoxResetPortAssignmentsOnStartup,    Strings.ResetPortAssignmentsOnStartup,  Strings.OptionsDescribeWebServerIsOnlyVirtualRadarServerOnLan);
            SetInlineHelp(checkBoxAutoStartUPnP,                    Strings.AutoStartUPnP,                  Strings.OptionsDescribeWebServerAutoStartUPnp);
            SetInlineHelp(numericUPnpPort,                          Strings.UPnpPort,                       Strings.OptionsDescribeWebServerUPnpPort);

            SetInlineHelp(numericInternetUserIdleTimeout,           Strings.IdleTimeout,                        Strings.OptionsDescribeWebServerInternetClientTimeoutMinutes);
            SetInlineHelp(checkBoxInternetUsersCanRunReports,       Strings.InternetUsersCanRunReports,         Strings.OptionsDescribeWebServerInternetClientCanRunReports);
            SetInlineHelp(checkBoxInternetUsersCanListenToAudio,    Strings.InternetUsersCanListenToAudio,      Strings.OptionsDescribeWebServerInternetClientCanPlayAudio);
            SetInlineHelp(checkBoxInternetUsersCanViewPictures,     Strings.InternetUsersCanViewPictures,       Strings.OptionsDescribeWebServerInternetClientCanSeePictures);
            SetInlineHelp(checkBoxInternetUserCanSeeLabels,         Strings.InternetUserCanSeeLabels,           Strings.OptionsDescribeWebServerInternetClientCanSeeLabels);
            SetInlineHelp(checkBoxInternetClientCanSubmitRoutes,    Strings.InternetClientCanSubmitRoutes,      Strings.OptionsDescribeWebServerInternetClientCanSubmitRoutes);
            SetInlineHelp(checkBoxInternetClientCanShowPolarPlots,  Strings.InternetClientCanShowPolarPlots,    Strings.OptionsDescribeWebServerInternetClientCanShowPolarPlots);
            SetInlineHelp(checkBoxAllowInternetProximityGadgets,    Strings.AllowInternetProximityGadgets,      Strings.OptionsDescribeWebServerAllowInternetProximityGadgets);
        }
    }
}
