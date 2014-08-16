using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// An enumeration of the different groups that can appear in a <see cref="ConfigurationListenerEventArgs"/>.
    /// </summary>
    public enum ConfigurationListenerGroup
    {
        /// <summary>
        /// The property belongs to the top-level Configuration object.
        /// </summary>
        Configuration,

        /// <summary>
        /// The property belongs to an <see cref="Access"/> object.
        /// </summary>
        Access,

        /// <summary>
        /// The property belongs to the <see cref="AudioSettings"/> object.
        /// </summary>
        Audio,

        /// <summary>
        /// The property belongs to the <see cref="BaseStationSettings"/> object.
        /// </summary>
        BaseStation,

        /// <summary>
        /// The property belongs to the <see cref="FlightRouteSettings"/> object.
        /// </summary>
        FlightRoute,

        /// <summary>
        /// The property belongs to the <see cref="GoogleMapSettings"/> object.
        /// </summary>
        GoogleMapSettings,

        /// <summary>
        /// The property belongs to the <see cref="InternetClientSettings"/> object.
        /// </summary>
        InternetClientSettings,

        /// <summary>
        /// The property belongs to a <see cref="MergedFeed"/> object.
        /// </summary>
        MergedFeed,

        /// <summary>
        /// The property belongs to the <see cref="RawDecodingSettings"/> object.
        /// </summary>
        RawDecodingSettings,

        /// <summary>
        /// The property belongs to a <see cref="RebroadcastSetting"/> object.
        /// </summary>
        RebroadcastSetting,

        /// <summary>
        /// The property belongs to a <see cref="Receiver"/> object.
        /// </summary>
        Receiver,

        /// <summary>
        /// The property belongs to a <see cref="ReceiverLocation"/> object.
        /// </summary>
        ReceiverLocation,

        /// <summary>
        /// The property belongs to the <see cref="VersionCheckSettings"/> object.
        /// </summary>
        VersionCheckSettings,

        /// <summary>
        /// The property belongs to the <see cref="WebServerSettings"/> object.
        /// </summary>
        WebServerSettings,
    }
}
