// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Localisation;
using System.ComponentModel;
using System.Drawing.Design;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The property sheet for web server settings.
    /// </summary>
    class SheetWebServerOptions : Sheet<SheetWebServerOptions>
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string SheetTitle { get { return Strings.OptionsWebServerSheetTitle; } }

        // Category display order and total
        private const int AuthenticationCategory = 0;
        private const int UPnpCategory = 1;
        private const int InternetClientCategory = 2;
        private const int TotalCategories = 3;

        [DisplayOrder(10)]
        [LocalisedDisplayName("UserMustAuthenticate")]
        [LocalisedCategory("Authentication", AuthenticationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerUserMustAuthenticate")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool UserMustAuthenticate { get; set; }
        public bool ShouldSerializeUserMustAuthenticate() { return ValueHasChanged(r => r.UserMustAuthenticate); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("PermittedUsers")]
        [LocalisedCategory("Authentication", AuthenticationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerPermittedUsers")]
        [TypeConverter(typeof(UsersListTypeConverter))]
        [Editor(typeof(UsersListUITypeEditor), typeof(UITypeEditor))]
        public List<string> PermittedUserIds { get; set; }
        public bool ShouldSerializePermittedUserIds() { return ValueHasChanged(r => r.PermittedUserIds); }

        [DisplayOrder(40)]
        [LocalisedDisplayName("EnableUPnp")]
        [LocalisedCategory("OptionsUPnPCategory", UPnpCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerEnableUPnpFeatures")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool EnableUPnpFeatures { get; set; }
        public bool ShouldSerializeEnableUPnpFeatures() { return ValueHasChanged(r => r.EnableUPnpFeatures); }

        [DisplayOrder(50)]
        [LocalisedDisplayName("ResetPortAssignmentsOnStartup")]
        [LocalisedCategory("OptionsUPnPCategory", UPnpCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerIsOnlyVirtualRadarServerOnLan")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool IsOnlyVirtualRadarServerOnLan { get; set; }
        public bool ShouldSerializeIsOnlyVirtualRadarServerOnLan() { return ValueHasChanged(r => r.IsOnlyVirtualRadarServerOnLan); }

        [DisplayOrder(60)]
        [LocalisedDisplayName("AutoStartUPnP")]
        [LocalisedCategory("OptionsUPnPCategory", UPnpCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerAutoStartUPnp")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool AutoStartUPnp { get; set; }
        public bool ShouldSerializeAutoStartUPnp() { return ValueHasChanged(r => r.AutoStartUPnp); }

        [DisplayOrder(70)]
        [LocalisedDisplayName("UPnpPort")]
        [LocalisedCategory("OptionsUPnPCategory", UPnpCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerUPnpPort")]
        [RaisesValuesChanged]
        public int UPnpPort { get; set; }
        public bool ShouldSerializeUPnpPort() { return ValueHasChanged(r => r.UPnpPort); }

        [DisplayOrder(80)]
        [LocalisedDisplayName("InternetUsersCanRunReports")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanRunReports")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool InternetClientCanRunReports { get; set; }
        public bool ShouldSerializeInternetClientCanRunReports() { return ValueHasChanged(r => r.InternetClientCanRunReports); }

        [DisplayOrder(90)]
        [LocalisedDisplayName("InternetUsersCanListenToAudio")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanPlayAudio")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool InternetClientCanPlayAudio { get; set; }
        public bool ShouldSerializeInternetClientCanPlayAudio() { return ValueHasChanged(r => r.InternetClientCanPlayAudio); }

        [DisplayOrder(100)]
        [LocalisedDisplayName("InternetUsersCanViewPictures")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanSeePictures")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool InternetClientCanSeePictures { get; set; }
        public bool ShouldSerializeInternetClientCanSeePictures() { return ValueHasChanged(r => r.InternetClientCanSeePictures); }

        [DisplayOrder(110)]
        [LocalisedDisplayName("InternetUserIdleTimeout")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientTimeoutMinutes")]
        [RaisesValuesChanged]
        public int InternetClientTimeoutMinutes { get; set; }
        public bool ShouldSerializeInternetClientTimeoutMinutes() { return ValueHasChanged(r => r.InternetClientTimeoutMinutes); }

        [DisplayOrder(120)]
        [LocalisedDisplayName("InternetUserCanSeeLabels")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanSeeLabels")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool InternetClientCanSeeLabels { get; set; }
        public bool ShouldSerializeInternetClientCanSeeLabels() { return ValueHasChanged(r => r.InternetClientCanSeeLabels); }

        [DisplayOrder(130)]
        [LocalisedDisplayName("AllowInternetProximityGadgets")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerAllowInternetProximityGadgets")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool AllowInternetProximityGadgets { get; set; }
        public bool ShouldSerializeAllowInternetProximityGadgets() { return ValueHasChanged(r => r.AllowInternetProximityGadgets); }

        [DisplayOrder(140)]
        [LocalisedDisplayName("InternetClientCanSubmitRoutes")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanSubmitRoutes")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool InternetClientCanSubmitRoutes { get; set; }
        public bool ShouldSerializeInternetClientCanSubmitRoutes() { return ValueHasChanged(r => r.InternetClientCanSubmitRoutes); }

        [DisplayOrder(150)]
        [LocalisedDisplayName("InternetClientCanShowPolarPlots")]
        [LocalisedCategory("OptionsWebServerInternetClientsCategory", InternetClientCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebServerInternetClientCanShowPolarPlots")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool InternetClientCanShowPolarPlots { get; set; }
        public bool ShouldSerializeInternetClientCanShowPolarPlots() { return ValueHasChanged(r => r.InternetClientCanShowPolarPlots); }
    }
}
