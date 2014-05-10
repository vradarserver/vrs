// Copyright © 2013 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The sheet that displays options for a single receiver.
    /// </summary>
    class SheetReceiverOptions : Sheet<SheetReceiverOptions>
    {
        /// <summary>
        /// See base class docs.
        /// </summary>
        public override string SheetTitle { get { return Name ?? ""; } }

        // The number and display order of categories on the sheet
        private const int GeneralCategory = 0;
        private const int DataFeedCategory = 1;
        private const int NetworkCategory = 2;
        private const int SerialCategory = 3;
        private const int TotalCategories = 4;

        [DisplayOrder(10)]
        [LocalisedDisplayName("Enabled")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeReceiverEnabled")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool Enabled { get; set; }
        public bool ShouldSerializeEnabled() { return ValueHasChanged(r => r.Enabled); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("Name")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeReceiverName")]
        [RaisesValuesChanged]
        public string Name { get; set; }
        public bool ShouldSerializeName() { return ValueHasChanged(r => Name); }

        [DisplayOrder(30)]
        [LocalisedDisplayName("ReceiverLocation")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeRawFeedReceiverLocation")]
        [TypeConverter(typeof(ReceiverLocationsTypeConverter))]
        [RaisesValuesChanged]
        public int ReceiverLocationId { get; set; }
        public bool ShouldSerializeReceiverLocationId() { return ValueHasChanged(r => ReceiverLocationId); }

        [DisplayOrder(40)]
        [LocalisedDisplayName("DataSource")]
        [LocalisedCategory("OptionsDataSourcesDataFeed", DataFeedCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesDataSource")]
        [TypeConverter(typeof(DataSourceEnumConverter))]
        public DataSource DataSource { get; set; }
        public bool ShouldSerializeDataSource() { return ValueHasChanged(r => r.DataSource); }

        [DisplayOrder(50)]
        [LocalisedDisplayName("ConnectionType")]
        [LocalisedCategory("OptionsDataSourcesDataFeed", DataFeedCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesConnectionType")]
        [TypeConverter(typeof(ConnectionTypeEnumConverter))]
        public ConnectionType ConnectionType { get; set; }
        public bool ShouldSerializeConnectionType() { return ValueHasChanged(r => r.ConnectionType); }

        [DisplayOrder(60)]
        [LocalisedDisplayName("AutoReconnectAtStartup")]
        [LocalisedCategory("OptionsDataSourcesDataFeed", DataFeedCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesAutoReconnectAtStartup")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool AutoReconnectAtStartup { get; set; }
        public bool ShouldSerializeAutoReconnectAtStartup() { return ValueHasChanged(r => r.AutoReconnectAtStartup); }

        [DisplayOrder(70)]
        [LocalisedDisplayName("UNC")]
        [LocalisedCategory("Network", NetworkCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesAddress")]
        [RaisesValuesChanged]
        public string Address { get; set; }
        public bool ShouldSerializeAddress() { return ValueHasChanged(r => r.Address); }

        [DisplayOrder(80)]
        [LocalisedDisplayName("Port")]
        [LocalisedCategory("Network", NetworkCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesPort")]
        [RaisesValuesChanged]
        public int Port { get; set; }
        public bool ShouldSerializePort() { return ValueHasChanged(r => r.Port); }

        [DisplayOrder(90)]
        [LocalisedDisplayName("SerialComPort")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesComPort")]
        [TypeConverter(typeof(ComPortConverter))]
        [RaisesValuesChanged]
        public string ComPort { get; set; }
        public bool ShouldSerializeComPort() { return ValueHasChanged(r => r.ComPort); }

        [DisplayOrder(100)]
        [LocalisedDisplayName("SerialBaudRate")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesBaudRate")]
        [TypeConverter(typeof(BaudRateConverter))]
        public int BaudRate { get; set; }
        public bool ShouldSerializeBaudRate() { return ValueHasChanged(r => r.BaudRate); }

        [DisplayOrder(110)]
        [LocalisedDisplayName("SerialDataBits")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesDataBits")]
        [TypeConverter(typeof(DataBitsConverter))]
        public int DataBits { get; set; }
        public bool ShouldSerializeDataBits() { return ValueHasChanged(r => r.DataBits); }

        [DisplayOrder(120)]
        [LocalisedDisplayName("SerialStopBits")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesStopBits")]
        [TypeConverter(typeof(StopBitsEnumConverter))]
        public StopBits StopBits { get; set; }
        public bool ShouldSerializeStopBits() { return ValueHasChanged(r => r.StopBits); }

        [DisplayOrder(130)]
        [LocalisedDisplayName("SerialParity")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesParity")]
        [TypeConverter(typeof(ParityEnumConverter))]
        public Parity Parity { get; set; }
        public bool ShouldSerializeParity() { return ValueHasChanged(r => r.Parity); }

        [DisplayOrder(140)]
        [LocalisedDisplayName("SerialHandshake")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesHandshake")]
        [TypeConverter(typeof(HandshakeEnumConverter))]
        public Handshake Handshake { get; set; }
        public bool ShouldSerializeHandshake() { return ValueHasChanged(r => r.Handshake); }

        [DisplayOrder(150)]
        [LocalisedDisplayName("SerialStartupText")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesStartupText")]
        public string StartupText { get; set; }
        public bool ShouldSerializeStartupText() { return ValueHasChanged(r => r.StartupText); }

        [DisplayOrder(160)]
        [LocalisedDisplayName("SerialShutdownText")]
        [LocalisedCategory("SerialSettings", SerialCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesShutdownText")]
        public string ShutdownText { get; set; }
        public bool ShouldSerializeShutdownText() { return ValueHasChanged(r => r.ShutdownText); }
    }
}
