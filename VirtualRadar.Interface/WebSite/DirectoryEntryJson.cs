using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The JSON object that carries directory information back to the VRS mothership.
    /// </summary>
    /// <remarks>
    /// These are not sent automatically - rather the VRS site requests one when it is
    /// checking to see if the site is still alive. The user has to register the site
    /// with the directory before the mothership will start requesting directory
    /// entries.
    /// </remarks>
    [DataContract]
    public class DirectoryEntryJson
    {
        /// <summary>
        /// Gets or sets the version number of VRS that the site is running.
        /// </summary>
        [DataMember]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the number of feeds that can be viewed from the site.
        /// </summary>
        [DataMember]
        public int NumberOfFeeds { get; set; }

        /// <summary>
        /// Gets or sets the highest number of aircraft across all feeds that can be viewed
        /// from the site.
        /// </summary>
        [DataMember]
        public int NumberOfAircraft { get; set; }
    }
}
