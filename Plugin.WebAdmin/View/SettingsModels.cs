// Copyright © 2016 onwards, Andrew Whewell
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
using System.IO.Ports;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.WebAdmin.View.Settings
{
    public class ViewModel
    {
        public ConfigurationModel Configuration { get; set; }

        public string Outcome { get; set; }

        public ReceiverModel NewReceiver { get; set; }

        public ReceiverLocationModel NewReceiverLocation { get; set; }

        public EnumModel[] ConnectionTypes { get; private set; }

        public EnumModel[] DataSources { get; private set; }

        public EnumModel[] DefaultAccesses { get; private set; }

        public EnumModel[] Handshakes { get; private set; }

        public EnumModel[] Parities { get; private set; }

        public EnumModel[] ReceiverUsages { get; private set; }

        public EnumModel[] StopBits { get; private set; }

        public ViewModel()
        {
            Configuration = new ConfigurationModel();

            DataSources = EnumModel.CreateFromEnum<DataSource>(r => Describe.DataSource(r));
            DefaultAccesses = EnumModel.CreateFromEnum<DefaultAccess>(r => Describe.DefaultAccess(r));
            ConnectionTypes = EnumModel.CreateFromEnum<ConnectionType>(r => Describe.ConnectionType(r));
            StopBits = EnumModel.CreateFromEnum<StopBits>(r => Describe.StopBits(r));
            Parities = EnumModel.CreateFromEnum<Parity>(r => Describe.Parity(r));
            Handshakes = EnumModel.CreateFromEnum<Handshake>(r => Describe.Handshake(r));
            ReceiverUsages = EnumModel.CreateFromEnum<ReceiverUsage>(r => Describe.ReceiverUsage(r));
        }

        public object FindViewModelForRecord(ValidationResult validationResult)
        {
            object result = null;

            if(validationResult.Record != null) {
                var receiverLocation = validationResult.Record as ReceiverLocation;
                if(receiverLocation != null) {
                    result = Configuration.ReceiverLocations.FirstOrDefault(r => r.UniqueId == receiverLocation.UniqueId);
                }
            }

            return result;
        }
    }

    public class TestConnectionOutcomeModel
    {
        public string Title { get; set; }

        public string Message { get; set; }
    }

    public class ConfigurationModel
    {
        public int DataVersion { get; set; }

        public string OnlineLookupSupplierName { get; set; }

        public string OnlineLookupSupplierCredits { get; set; }

        public string OnlineLookupSupplierUrl { get; set; }

        public BaseStationSettingsModel BaseStationSettingsModel { get; private set; }

        public List<ReceiverModel> Receivers { get; private set; }

        public List<ReceiverLocationModel> ReceiverLocations { get; private set; }

        public ConfigurationModel()
        {
            BaseStationSettingsModel = new BaseStationSettingsModel();
            Receivers = new List<ReceiverModel>();
            ReceiverLocations = new List<ReceiverLocationModel>();
        }

        public ConfigurationModel(Configuration configuration) : this()
        {
            RefreshFromConfiguration(configuration);
        }

        public void RefreshFromConfiguration(Configuration configuration)
        {
            DataVersion = configuration.DataVersion;

            BaseStationSettingsModel.RefreshFromSettings(configuration.BaseStationSettings);
            CollectionHelper.ApplySourceToDestination(configuration.Receivers, Receivers,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new ReceiverModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
            Receivers.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));
            CollectionHelper.ApplySourceToDestination(configuration.ReceiverLocations, ReceiverLocations,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new ReceiverLocationModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
            ReceiverLocations.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));
        }

        public void CopyToConfiguration(Configuration configuration)
        {
            configuration.DataVersion = DataVersion;

            BaseStationSettingsModel.CopyToSettings(configuration.BaseStationSettings);
            CollectionHelper.ApplySourceToDestination(Receivers, configuration.Receivers,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(new Receiver()),
                (source, dest) => source.CopyToSettings(dest)
            );
            CollectionHelper.ApplySourceToDestination(ReceiverLocations, configuration.ReceiverLocations,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(new ReceiverLocation()),
                (source, dest) => source.CopyToSettings(dest)
            );
        }
    }

    public class AccessModel
    {
        public int DefaultAccess { get; set; }              // DefaultAccess

        public List<string> Addresses { get; private set; }

        public AccessModel()
        {
            Addresses = new List<string>();
        }

        public AccessModel(Access settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(Access settings)
        {
            DefaultAccess = (int)settings.DefaultAccess;
            Addresses.Clear();
            Addresses.AddRange(settings.Addresses);
        }

        public Access CopyToSettings(Access access)
        {
            access.DefaultAccess = EnumModel.CastFromInt<DefaultAccess>(DefaultAccess);
            access.Addresses.Clear();
            access.Addresses.AddRange(Addresses);

            return access;
        }
    }

    public class BaseStationSettingsModel
    {
        public string DatabaseFileName { get; set; }

        [ValidationModelField(ValidationField.BaseStationDatabase)]
        public ValidationModelField DatabaseFileNameValidation { get; set; }

        public string OperatorFlagsFolder { get; set; }

        [ValidationModelField(ValidationField.FlagsFolder)]
        public ValidationModelField OperatorFlagsFolderValidation { get; set; }

        public string SilhouettesFolder { get; set; }

        [ValidationModelField(ValidationField.SilhouettesFolder)]
        public ValidationModelField SilhouettesFolderValidation { get; set; }

        public string PicturesFolder { get; set; }

        [ValidationModelField(ValidationField.PicturesFolder)]
        public ValidationModelField PicturesFolderValidation { get; set; }

        public bool SearchPictureSubFolders { get; set; }

        public int DisplayTimeoutSeconds { get; set; }

        public int TrackingTimeoutSeconds { get; set; }

        public bool MinimiseToSystemTray { get; set; }

        public int AutoSavePolarPlotsMinutes { get; set; }

        public bool LookupAircraftDetailsOnline { get; set; }

        public BaseStationSettingsModel()
        {
        }

        public BaseStationSettingsModel(BaseStationSettings settings) : this()
        {
            ValidationModelHelper.CreateEmptyViewModelValidationFields(this);
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(BaseStationSettings settings)
        {
            DatabaseFileName =              settings.DatabaseFileName;
            OperatorFlagsFolder =           settings.OperatorFlagsFolder;
            SilhouettesFolder =             settings.SilhouettesFolder;
            PicturesFolder =                settings.PicturesFolder;
            SearchPictureSubFolders =       settings.SearchPictureSubFolders;
            DisplayTimeoutSeconds =         settings.DisplayTimeoutSeconds;
            TrackingTimeoutSeconds =        settings.TrackingTimeoutSeconds;
            MinimiseToSystemTray =          settings.MinimiseToSystemTray;
            AutoSavePolarPlotsMinutes =     settings.AutoSavePolarPlotsMinutes;
            LookupAircraftDetailsOnline =   settings.LookupAircraftDetailsOnline;
        }

        public BaseStationSettings CopyToSettings(BaseStationSettings settings)
        {
            settings.DatabaseFileName =             DatabaseFileName;
            settings.OperatorFlagsFolder =          OperatorFlagsFolder;
            settings.SilhouettesFolder =            SilhouettesFolder;
            settings.PicturesFolder =               PicturesFolder;
            settings.SearchPictureSubFolders =      SearchPictureSubFolders;
            settings.DisplayTimeoutSeconds =        DisplayTimeoutSeconds;
            settings.TrackingTimeoutSeconds =       TrackingTimeoutSeconds;
            settings.MinimiseToSystemTray =         MinimiseToSystemTray;
            settings.AutoSavePolarPlotsMinutes =    AutoSavePolarPlotsMinutes;
            settings.LookupAircraftDetailsOnline =  LookupAircraftDetailsOnline;

            return settings;
        }
    }

    public class ReceiverModel
    {
        public bool Enabled { get; set; }

        [ValidationModelField(ValidationField.Enabled)]
        public ValidationModelField EnabledValidation { get; set; }

        public int UniqueId { get; set; }

        public string Name { get; set; }

        [ValidationModelField(ValidationField.Name)]
        public ValidationModelField NameValidation { get; set; }

        public int DataSource { get; set; }                 // DataSource

        public int ConnectionType { get; set; }             // ConnectionType

        public bool AutoReconnectAtStartup { get; set; }

        public bool IsPassive { get; set; }

        [ValidationModelField(ValidationField.IsPassive)]
        public ValidationModelField IsPassiveValidation { get; set; }

        public AccessModel Access { get; private set; }

        public string Address { get; set; }

        [ValidationModelField(ValidationField.BaseStationAddress)]
        public ValidationModelField AddressValidation { get; set; }

        public int Port { get; set; }

        [ValidationModelField(ValidationField.BaseStationPort)]
        public ValidationModelField PortValidation { get; set; }

        public bool UseKeepAlive { get; set; }

        [ValidationModelField(ValidationField.UseKeepAlive)]
        public ValidationModelField UseKeepAliveValidation { get; set; }

        public int IdleTimeoutMilliseconds { get; set; }

        [ValidationModelField(ValidationField.IdleTimeout)]
        public ValidationModelField IdleTimeoutValidation { get; set; }

        public string Passphrase { get; set; }

        public string ComPort { get; set; }

        [ValidationModelField(ValidationField.ComPort)]
        public ValidationModelField ComPortValidation { get; set; }

        public int BaudRate { get; set; }

        [ValidationModelField(ValidationField.BaudRate)]
        public ValidationModelField BaudRateValidation { get; set; }

        public int DataBits { get; set; }

        [ValidationModelField(ValidationField.DataBits)]
        public ValidationModelField DataBitsValidation { get; set; }

        public int StopBits { get; set; }                   // StopBits

        public int Parity { get; set; }                     // Parity

        public int Handshake { get; set; }                  // Handshake

        public string StartupText { get; set; }

        public string ShutdownText { get; set; }

        public int ReceiverLocationId { get; set; }

        [ValidationModelField(ValidationField.Location)]
        public ValidationModelField ReceiverLocationIdValidation { get; set; }

        public int ReceiverUsage { get; set; }              // ReceiverUsage

        public ReceiverModel()
        {
            Access = new AccessModel();
        }

        public ReceiverModel(Receiver settings) : this()
        {
            ValidationModelHelper.CreateEmptyViewModelValidationFields(this);
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(Receiver settings)
        {
            Enabled =                       settings.Enabled;
            UniqueId =                      settings.UniqueId;
            Name =                          settings.Name;
            DataSource =                    (int)settings.DataSource;
            ConnectionType =                (int)settings.ConnectionType;
            AutoReconnectAtStartup =        settings.AutoReconnectAtStartup;
            IsPassive =                     settings.IsPassive;
            Address =                       settings.Address;
            Port =                          settings.Port;
            UseKeepAlive =                  settings.UseKeepAlive;
            IdleTimeoutMilliseconds =       settings.IdleTimeoutMilliseconds;
            Passphrase =                    settings.Passphrase;
            ComPort =                       settings.ComPort;
            BaudRate =                      settings.BaudRate;
            DataBits =                      settings.DataBits;
            StopBits =                      (int)settings.StopBits;
            Parity =                        (int)settings.Parity;
            Handshake =                     (int)settings.Handshake;
            StartupText =                   settings.StartupText;
            ShutdownText =                  settings.ShutdownText;
            ReceiverLocationId =            settings.ReceiverLocationId;
            ReceiverUsage =                 (int)settings.ReceiverUsage;

            Access.RefreshFromSettings(settings.Access);
        }

        public Receiver CopyToSettings(Receiver settings)
        {
            settings.Enabled =                  Enabled;
            settings.UniqueId =                 UniqueId;
            settings.Name =                     Name;
            settings.DataSource =               EnumModel.CastFromInt<DataSource>(DataSource);
            settings.ConnectionType =           EnumModel.CastFromInt<ConnectionType>(ConnectionType);
            settings.AutoReconnectAtStartup =   AutoReconnectAtStartup;
            settings.IsPassive =                IsPassive;
            settings.Address =                  Address;
            settings.Port =                     Port;
            settings.UseKeepAlive =             UseKeepAlive;
            settings.IdleTimeoutMilliseconds =  IdleTimeoutMilliseconds;
            settings.Passphrase =               Passphrase;
            settings.ComPort =                  ComPort;
            settings.BaudRate =                 BaudRate;
            settings.DataBits =                 DataBits;
            settings.StopBits =                 EnumModel.CastFromInt<StopBits>(StopBits);
            settings.Parity =                   EnumModel.CastFromInt<Parity>(Parity);
            settings.Handshake =                EnumModel.CastFromInt<Handshake>(Handshake);
            settings.StartupText =              StartupText;
            settings.ShutdownText =             ShutdownText;
            settings.ReceiverLocationId =       ReceiverLocationId;
            settings.ReceiverUsage =            EnumModel.CastFromInt<ReceiverUsage>(ReceiverUsage);

            Access.CopyToSettings(settings.Access);

            return settings;
        }
    }

    public class ReceiverLocationModel
    {
        public int UniqueId { get; set; }

        public string Name { get; set; }

        [ValidationModelField(ValidationField.Location)]
        public ValidationModelField NameValidation { get; set; }

        public double Latitude { get; set; }

        [ValidationModelField(ValidationField.Latitude)]
        public ValidationModelField LatitudeValidation { get; set; }

        public double Longitude { get; set; }

        [ValidationModelField(ValidationField.Longitude)]
        public ValidationModelField LongitudeValidation { get; set; }

        public bool IsBaseStationLocation { get; set; }

        public ReceiverLocationModel()
        {
        }

        public ReceiverLocationModel(ReceiverLocation settings)
        {
            ValidationModelHelper.CreateEmptyViewModelValidationFields(this);
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(ReceiverLocation settings)
        {
            UniqueId =              settings.UniqueId;
            Name =                  settings.Name;
            Latitude =              settings.Latitude;
            Longitude =             settings.Longitude;
            IsBaseStationLocation = settings.IsBaseStationLocation;
        }

        public ReceiverLocation CopyToSettings(ReceiverLocation settings)
        {
            settings.UniqueId =                 UniqueId;
            settings.Name =                     Name;
            settings.Latitude =                 Latitude;
            settings.Longitude =                Longitude;
            settings.IsBaseStationLocation =    IsBaseStationLocation;

            return settings;
        }
    }
}
