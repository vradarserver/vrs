using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Carries information in events from <see cref="IConfigurationListener"/>.
    /// </summary>
    public class ConfigurationListenerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the configuration that was changed.
        /// </summary>
        public Configuration Configuration { get; private set; }

        /// <summary>
        /// Gets the record that was changed (either a <see cref="Configuration"/> or more typically
        /// one of the child objects such as <see cref="BaseStationSettings"/>, <see cref="MergedFeed"/> etc).
        /// </summary>
        public object Record { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Record"/> is a child object in a list.
        /// </summary>
        public bool IsListChild { get; private set; }

        /// <summary>
        /// Gets the group of settings that were changed.
        /// </summary>
        public ConfigurationListenerGroup Group { get; private set; }

        /// <summary>
        /// Gets the name of the property that has been changed.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="record"></param>
        /// <param name="isListChild"></param>
        /// <param name="group"></param>
        /// <param name="propertyName"></param>
        public ConfigurationListenerEventArgs(Configuration configuration, object record, bool isListChild, ConfigurationListenerGroup group, string propertyName)
        {
            Configuration = configuration;
            Record = record;
            IsListChild = isListChild;
            Group = group;
            PropertyName = propertyName;
        }
    }
}
