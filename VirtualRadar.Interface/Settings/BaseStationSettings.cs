// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// An object that describes the source of data we should connect to.
    /// </summary>
    /// <remarks>
    /// Originally the application was only able to connect to instances of BaseStation. Later versions
    /// were able to deal with a wider variety of sources. Ideally the name of this class would be changed
    /// to something that doesn't imply that it's only talking about BaseStation connections but the name
    /// has been serialised into the configuration files and it's not worth the pain of changing it.
    /// </remarks>
    public class BaseStationSettings
    {
        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string DataSource { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public bool AutoReconnectAtStartup { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string Address { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public int Port { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string ComPort { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public int BaudRate { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public int DataBits { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string StopBits { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string Parity { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string Handshake { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string StartupText { get; set; }

        /// <summary>
        /// No longer used. Not marked as obsolete as that prevents serialisation.
        /// </summary>
        // Obsolete (we can't use the attribute)
        public string ShutdownText { get; set; }

        /// <summary>
        /// Gets or sets the full path to the BaseStation database file to use.
        /// </summary>
        public string DatabaseFileName { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Kinetic",
            "BaseStation",
            "BaseStation.sqb"
        );

        /// <summary>
        /// Gets or sets the folder that holds operator logo images to display to the user.
        /// </summary>
        public string OperatorFlagsFolder { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Kinetic",
            "BaseStation",
            "OperatorFlags"
        );

        /// <summary>
        /// Gets or sets the folder that holds aircraft silhouette images to display to the user.
        /// </summary>
        public string SilhouettesFolder { get; set; }

        /// <summary>
        /// No longer used.
        /// </summary>
        public string OutlinesFolder { get; set; }

        /// <summary>
        /// Gets or sets the folder that holds pictures of aircraft to display to the user.
        /// </summary>
        public string PicturesFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that sub-folders of <see cref="PicturesFolder"/> should be searched for aircraft pictures.
        /// </summary>
        public bool SearchPictureSubFolders { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds that aircraft should remain on display in the browser after their last transmission.
        /// </summary>
        public int DisplayTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the number of seconds that aircraft should remain in the aircraft list after their last transmission.
        /// </summary>
        public int TrackingTimeoutSeconds { get; set; } = 600;

        /// <summary>
        /// Gets or sets the number of minutes that aircraft on a satellite feed should remain on display after their last transmission.
        /// </summary>
        public int SatcomDisplayTimeoutMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets the number of minutes that aircraft on a satellite feed should remain in the aircraft list after their last transmission.
        /// </summary>
        public int SatcomTrackingTimeoutMinutes { get; set; } = 120;

        /// <summary>
        /// Gets or sets a value indicating that badly formatted messages on the feed should be ignored rather than triggering a disconnection
        /// from the feed.
        /// </summary>
        /// <remarks>
        /// This has been retired - the program sets this to true and the configuration loader is expected to force it to true on load. Bad
        /// messages will no longer disconnect the listener.
        /// </remarks>
        public bool IgnoreBadMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the program should minimise to the system tray rather than the taskbar.
        /// </summary>
        public bool MinimiseToSystemTray { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes to wait between automatic saves of polar plots.
        /// </summary>
        public int AutoSavePolarPlotsMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets a value indicating that the program should lookup aircraft details online.
        /// </summary>
        public bool LookupAircraftDetailsOnline { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that global air pressure readings should be downloaded so that
        /// true altitudes can be calculated for aircraft that report pressure altitudes.
        /// </summary>
        public bool DownloadGlobalAirPressureReadings { get; set; } = true;
    }
}
