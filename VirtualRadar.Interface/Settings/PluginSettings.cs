// Copyright © 2010 onwards, Andrew Whewell
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
using System.Collections.Specialized;
using System.Globalization;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The class that holds the settings for the plugins. These are loaded and saved via <see cref="IPluginSettingsStorage"/>.
    /// </summary>
    /// <remarks><para>
    /// It is important to note that this object will contain every setting for every plugin, not just the plugin that
    /// loaded it. When writing settings out you should read the old settings first and amend them rather than create
    /// new settings and save them, otherwise you will obliterate the settings for all of the other plugins.
    /// </para><para>
    /// The settings are stored as strings in the <see cref="Values"/> collection. The collection is indexed by a key
    /// which must be unique. If you use the supplied Read and Write methods then the <see cref="IPlugin.Id"/> property
    /// is joined to the key supplied to the method to form a full key that should be unique to your plugin. The Read
    /// and Write methods also ensure that when ValueTypes are converted to and from strings they use the invariant
    /// culture, e.g. the same string is written for the floating point value 1.234 regardless of the region setting
    /// that Windows is configured for.
    /// </para></remarks>
    /// <example>
    /// To load a setting from within an <see cref="IPlugin"/>:
    /// <code>
    /// IPluginSettingsStorage storage = Factory.Singleton.Resolve&lt;IPluginSettingsStorage&gt;().Singleton;
    /// PluginSettings settings = storage.Load();
    /// 
    /// bool mySetting = ReadBool(this, "Key", true);
    /// </code>
    /// To save a setting from within an <see cref="IPlugin"/>:
    /// IPluginSettingsStorage storage = Factory.Singleton.Resolve&lt;IPluginSettingsStorage&gt;().Singleton;
    /// PluginSettings settings = storage.Load();
    /// settings.Write(this, "Key", false);
    /// storage.Save(settings);
    /// </example>
    public class PluginSettings
    {
        /// <summary>
        /// Gets a collection of property names and their values. Plugins should ensure the keys for the values
        /// are unique across all plugins.
        /// </summary>
        /// <remarks>
        /// This collection is shared by all plugins. The keys should be prefaced by a string unique to each plugin,
        /// for example by the plugin ID string.
        /// </remarks>
        public NameValueCollection Values { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PluginSettings()
        {
            Values = new NameValueCollection();
        }

        /// <summary>
        /// Creates a nicely formed key name from the plugin ID and key passed across.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string FormKey(IPlugin plugin, string key)
        {
            return String.Format("{0}.{1}", plugin.Id, key);
        }

        /// <summary>
        /// Returns a string from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ReadString(IPlugin plugin, string key)
        {
            return Values[FormKey(plugin, key)];
        }

        /// <summary>
        /// Returns a nullable bool from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool? ReadBool(IPlugin plugin, string key)
        {
            var text = ReadString(plugin, key);
            bool? result = text == "1" ? true : text == "0" ? false : (bool?)null;

            return result;
        }

        /// <summary>
        /// Returns a bool from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool ReadBool(IPlugin plugin, string key, bool defaultValue)
        {
            return ReadBool(plugin, key) ?? defaultValue;
        }

        /// <summary>
        /// Returns a nullable int from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public int? ReadInt(IPlugin plugin, string key)
        {
            int? result = null;

            var text = ReadString(plugin, key);
            if(text != null) {
                int value;
                if(int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) result = value;
            }

            return result;
        }

        /// <summary>
        /// Returns an integer from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int ReadInt(IPlugin plugin, string key, int defaultValue)
        {
            return ReadInt(plugin, key) ?? defaultValue;
        }

        /// <summary>
        /// Returns a nullable long from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public long? ReadLong(IPlugin plugin, string key)
        {
            long? result = null;

            var text = ReadString(plugin, key);
            if(text != null) {
                long value;
                if(long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) result = value;
            }

            return result;
        }

        /// <summary>
        /// Returns a long from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public long ReadLong(IPlugin plugin, string key, long defaultValue)
        {
            return ReadLong(plugin, key) ?? defaultValue;
        }

        /// <summary>
        /// Returns a nullable double from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public double? ReadDouble(IPlugin plugin, string key)
        {
            double? result = null;

            var text = ReadString(plugin, key);
            if(text != null) {
                double value;
                if(double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) result = value;
            }

            return result;
        }

        /// <summary>
        /// Returns a double from <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public double ReadDouble(IPlugin plugin, string key, double defaultValue)
        {
            return ReadDouble(plugin, key) ?? defaultValue;
        }

        /// <summary>
        /// Returns a nullable DateTime from <see cref="Values"/>. The DateTimeKind is local.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public DateTime? ReadDateTime(IPlugin plugin, string key)
        {
            DateTime? result = null;

            var text = ReadString(plugin, key);
            if(text != null) {
                DateTime value;
                if(DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value)) result = value;
            }

            return result;
        }

        /// <summary>
        /// Returns a DateTime from <see cref="Values"/>. The DateTimeKind is local.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public DateTime ReadDateTime(IPlugin plugin, string key, DateTime defaultValue)
        {
            return ReadDateTime(plugin, key) ?? defaultValue;
        }

        /// <summary>
        /// Writes a bool into <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Write(IPlugin plugin, string key, bool value)
        {
            Values[FormKey(plugin, key)] = value ? "1" : "0";
        }

        /// <summary>
        /// Writes an int into <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Write(IPlugin plugin, string key, int value)
        {
            Values[FormKey(plugin, key)] = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes a long into <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Write(IPlugin plugin, string key, long value)
        {
            Values[FormKey(plugin, key)] = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes a double into <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Write(IPlugin plugin, string key, double value)
        {
            Values[FormKey(plugin, key)] = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes a DateTime into <see cref="Values"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Write(IPlugin plugin, string key, DateTime value)
        {
            Values[FormKey(plugin, key)] = value.ToUniversalTime().ToString("u");
        }

        /// <summary>
        /// Writes a value into <see cref="Values"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Write<T>(IPlugin plugin, string key, T value)
        {
            Values[FormKey(plugin, key)] = value == null ? null : value.ToString();
        }
    }
}
