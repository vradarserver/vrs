 
/// <reference path="Enums.ts" />

declare module VirtualRadar.Plugin.WebAdmin {
    interface IJsonMenuEntry {
        HtmlFileName: string;
        Name: string;
        IsPlugin: boolean;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog {
    interface IViewModel {
        LogEntries: VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog.ILogEntry[];
    }
    interface ILogEntry {
        Time: string;
        Icao: string;
        Registration: string;
        Country: string;
        Manufacturer: string;
        Model: string;
        ModelIcao: string;
        Operator: string;
        OperatorIcao: string;
        Serial: string;
        YearBuilt: number;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View {
    interface IAboutView {
        Caption: string;
        ProductName: string;
        Version: string;
        BuildDate: Date;
        FormattedBuildDate: string;
        Copyright: string;
        Description: string;
        IsMono: boolean;
    }
    interface ILogView {
        LogLines: string[];
    }
    interface IMainView {
        BadPlugins: number;
        NewVer: boolean;
        NewVerUrl: string;
        Upnp: boolean;
        UpnpRouter: boolean;
        UpnpOn: boolean;
        LocalRoot: string;
        LanRoot: string;
        PublicRoot: string;
        Requests: VirtualRadar.Interface.View.IServerRequest[];
        Feeds: VirtualRadar.Interface.View.IFeedStatus[];
        Rebroadcasters: VirtualRadar.Interface.IRebroadcastServerConnection[];
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog {
    interface IViewModel {
        Connectors: VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog.IConnectorModel[];
        Events: VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog.IEventModel[];
    }
    interface IConnectorModel {
        Name: string;
    }
    interface IEventModel {
        Id: number;
        ConnectorName: string;
        Time: string;
        Type: string;
        Message: string;
    }
}
declare module VirtualRadar.Interface.View {
    interface IServerRequest {
        RemoteEndPoint: System.Net.IIPEndPoint;
        DataVersion: number;
        User: string;
        RemoteAddr: string;
        RemotePort: number;
        LastRequest: Date;
        Bytes: number;
        LastUrl: string;
    }
    interface IFeedStatus {
        Id: number;
        DataVersion: number;
        Name: string;
        Merged: boolean;
        Polar: boolean;
        HasAircraftList: boolean;
        Connection: VirtualRadar.Interface.Network.ConnectionStatus;
        ConnDesc: string;
        Msgs: number;
        BadMsgs: number;
        Tracked: number;
    }
    interface IValidationModelField {
        IsWarning: boolean;
        IsError: boolean;
        IsValid: boolean;
        Message: string;
    }
    interface IEnumModel {
        Value: number;
        Description: string;
    }
}
declare module System.Net {
    interface IIPEndPoint extends System.Net.IEndPoint {
        AddressFamily: System.Net.Sockets.AddressFamily;
        Address: System.Net.IIPAddress;
        Port: number;
    }
    interface IEndPoint {
        AddressFamily: System.Net.Sockets.AddressFamily;
    }
    interface IIPAddress {
        Address: number;
        AddressFamily: System.Net.Sockets.AddressFamily;
        ScopeId: number;
        IsIPv6Multicast: boolean;
        IsIPv6LinkLocal: boolean;
        IsIPv6SiteLocal: boolean;
        IsIPv6Teredo: boolean;
        IsIPv4MappedToIPv6: boolean;
    }
}
declare module VirtualRadar.Interface {
    interface IRebroadcastServerConnection {
        Id: number;
        Name: string;
        LocalPort: number;
        EndpointIPAddress: System.Net.IIPAddress;
        RemoteAddr: string;
        RemotePort: number;
        Buffered: number;
        Written: number;
        Discarded: number;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Queues {
    interface IViewModel {
        Queues: VirtualRadar.Plugin.WebAdmin.View.Queues.IQueueModel[];
    }
    interface IQueueModel {
        Name: string;
        CountQueuedItems: number;
        PeakQueuedItems: number;
        CountDroppedItems: number;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Settings {
    interface IViewModel {
        CurrentUserName: string;
        Configuration: VirtualRadar.Plugin.WebAdmin.View.Settings.IConfigurationModel;
        Outcome: string;
        NewMergedFeed: VirtualRadar.Plugin.WebAdmin.View.Settings.IMergedFeedModel;
        NewRebroadcastServer: VirtualRadar.Plugin.WebAdmin.View.Settings.IRebroadcastServerModel;
        NewReceiver: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverModel;
        NewReceiverLocation: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel;
        NewUser: VirtualRadar.Plugin.WebAdmin.View.Settings.IUserModel;
        ConnectionTypes: VirtualRadar.Interface.View.IEnumModel[];
        DataSources: VirtualRadar.Interface.View.IEnumModel[];
        DefaultAccesses: VirtualRadar.Interface.View.IEnumModel[];
        DistanceUnits: VirtualRadar.Interface.View.IEnumModel[];
        Handshakes: VirtualRadar.Interface.View.IEnumModel[];
        HeightUnits: VirtualRadar.Interface.View.IEnumModel[];
        Parities: VirtualRadar.Interface.View.IEnumModel[];
        ProxyTypes: VirtualRadar.Interface.View.IEnumModel[];
        RebroadcastFormats: VirtualRadar.Interface.View.IEnumModel[];
        ReceiverUsages: VirtualRadar.Interface.View.IEnumModel[];
        SpeedUnits: VirtualRadar.Interface.View.IEnumModel[];
        StopBits: VirtualRadar.Interface.View.IEnumModel[];
        ComPortNames: string[];
    }
    interface IConfigurationModel {
        DataVersion: number;
        OnlineLookupSupplierName: string;
        OnlineLookupSupplierCredits: string;
        OnlineLookupSupplierUrl: string;
        BaseStationSettings: VirtualRadar.Plugin.WebAdmin.View.Settings.IBaseStationSettingsModel;
        GoogleMapSettings: VirtualRadar.Plugin.WebAdmin.View.Settings.IGoogleMapSettingsModel;
        RawDecodingSettings: VirtualRadar.Plugin.WebAdmin.View.Settings.IRawDecodingSettingModel;
        MergedFeeds: VirtualRadar.Plugin.WebAdmin.View.Settings.IMergedFeedModel[];
        RebroadcastSettings: VirtualRadar.Plugin.WebAdmin.View.Settings.IRebroadcastServerModel[];
        Receivers: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverModel[];
        ReceiverLocations: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel[];
        Users: VirtualRadar.Plugin.WebAdmin.View.Settings.IUserModel[];
    }
    interface IBaseStationSettingsModel {
        DatabaseFileName: string;
        DatabaseFileNameValidation: VirtualRadar.Interface.View.IValidationModelField;
        OperatorFlagsFolder: string;
        OperatorFlagsFolderValidation: VirtualRadar.Interface.View.IValidationModelField;
        SilhouettesFolder: string;
        SilhouettesFolderValidation: VirtualRadar.Interface.View.IValidationModelField;
        PicturesFolder: string;
        PicturesFolderValidation: VirtualRadar.Interface.View.IValidationModelField;
        SearchPictureSubFolders: boolean;
        DisplayTimeoutSeconds: number;
        TrackingTimeoutSeconds: number;
        MinimiseToSystemTray: boolean;
        AutoSavePolarPlotsMinutes: number;
        LookupAircraftDetailsOnline: boolean;
    }
    interface IGoogleMapSettingsModel {
        InitialSettings: string;
        InitialMapLatitude: number;
        InitialMapLongitude: number;
        InitialMapType: string;
        InitialMapZoom: number;
        InitialRefreshSeconds: number;
        MinimumRefreshSeconds: number;
        ShortTrailLengthSeconds: number;
        InitialDistanceUnit: number;
        InitialHeightUnit: number;
        InitialSpeedUnit: number;
        PreferIataAirportCodes: boolean;
        EnableBundling: boolean;
        EnableMinifying: boolean;
        EnableCompression: boolean;
        WebSiteReceiverId: number;
        WebSiteReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField;
        DirectoryEntryKey: string;
        ClosestAircraftReceiverId: number;
        ClosestAircraftReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField;
        FlightSimulatorXReceiverId: number;
        FlightSimulatorXReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField;
        ProxyType: number;
    }
    interface IRawDecodingSettingModel {
        ReceiverRange: number;
        ReceiverRangeValidation: VirtualRadar.Interface.View.IValidationModelField;
        IgnoreMilitaryExtendedSquitter: boolean;
        SuppressReceiverRangeCheck: boolean;
        UseLocalDecodeForInitialPosition: boolean;
        AirborneGlobalPositionLimit: number;
        AirborneGlobalPositionLimitValidation: VirtualRadar.Interface.View.IValidationModelField;
        FastSurfaceGlobalPositionLimit: number;
        FastSurfaceGlobalPositionLimitValidation: VirtualRadar.Interface.View.IValidationModelField;
        SlowSurfaceGlobalPositionLimit: number;
        SlowSurfaceGlobalPositionLimitValidation: VirtualRadar.Interface.View.IValidationModelField;
        AcceptableAirborneSpeed: number;
        AcceptableAirborneSpeedValidation: VirtualRadar.Interface.View.IValidationModelField;
        AcceptableAirSurfaceTransitionSpeed: number;
        AcceptableAirSurfaceTransitionSpeedValidation: VirtualRadar.Interface.View.IValidationModelField;
        AcceptableSurfaceSpeed: number;
        AcceptableSurfaceSpeedValidation: VirtualRadar.Interface.View.IValidationModelField;
        IgnoreCallsignsInBds20: boolean;
        AcceptIcaoInPI0Count: number;
        AcceptIcaoInPI0CountValidation: VirtualRadar.Interface.View.IValidationModelField;
        AcceptIcaoInPI0Seconds: number;
        AcceptIcaoInPI0SecondsValidation: VirtualRadar.Interface.View.IValidationModelField;
        AcceptIcaoInNonPICount: number;
        AcceptIcaoInNonPICountValidation: VirtualRadar.Interface.View.IValidationModelField;
        AcceptIcaoInNonPISeconds: number;
        AcceptIcaoInNonPISecondsValidation: VirtualRadar.Interface.View.IValidationModelField;
        SuppressIcao0: boolean;
        IgnoreInvalidCodeBlockInParityMessages: boolean;
        IgnoreInvalidCodeBlockInOtherMessages: boolean;
        SuppressTisbDecoding: boolean;
    }
    interface IMergedFeedModel {
        Enabled: boolean;
        UniqueId: number;
        Name: string;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField;
        ReceiverIds: number[];
        ReceiverIdsValidation: VirtualRadar.Interface.View.IValidationModelField;
        ReceiverFlags: VirtualRadar.Plugin.WebAdmin.View.Settings.IMergedFeedReceiverModel[];
        IcaoTimeout: number;
        IcaoTimeoutValidation: VirtualRadar.Interface.View.IValidationModelField;
        IgnoreAircraftWithNoPosition: boolean;
        ReceiverUsage: VirtualRadar.Interface.Settings.ReceiverUsage;
    }
    interface IMergedFeedReceiverModel {
        UniqueId: number;
        IsMlatFeed: boolean;
    }
    interface IRebroadcastServerModel {
        UniqueId: number;
        Name: string;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField;
        Enabled: boolean;
        ReceiverId: number;
        ReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField;
        Format: number;
        FormatValidation: VirtualRadar.Interface.View.IValidationModelField;
        IsTransmitter: boolean;
        IsTransmitterValidation: VirtualRadar.Interface.View.IValidationModelField;
        TransmitAddress: string;
        TransmitAddressValidation: VirtualRadar.Interface.View.IValidationModelField;
        Port: number;
        PortValidation: VirtualRadar.Interface.View.IValidationModelField;
        UseKeepAlive: boolean;
        UseKeepAliveValidation: VirtualRadar.Interface.View.IValidationModelField;
        IdleTimeoutMilliseconds: number;
        IdleTimeoutMillisecondsValidation: VirtualRadar.Interface.View.IValidationModelField;
        StaleSeconds: number;
        StaleSecondsValidation: VirtualRadar.Interface.View.IValidationModelField;
        Access: VirtualRadar.Plugin.WebAdmin.View.Settings.IAccessModel;
        Passphrase: string;
        SendIntervalMilliseconds: number;
        SendIntervalMillisecondsValidation: VirtualRadar.Interface.View.IValidationModelField;
    }
    interface IAccessModel {
        DefaultAccess: number;
        Addresses: string[];
    }
    interface IReceiverModel {
        Enabled: boolean;
        EnabledValidation: VirtualRadar.Interface.View.IValidationModelField;
        UniqueId: number;
        Name: string;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField;
        DataSource: number;
        ConnectionType: number;
        AutoReconnectAtStartup: boolean;
        IsPassive: boolean;
        IsPassiveValidation: VirtualRadar.Interface.View.IValidationModelField;
        Access: VirtualRadar.Plugin.WebAdmin.View.Settings.IAccessModel;
        Address: string;
        AddressValidation: VirtualRadar.Interface.View.IValidationModelField;
        Port: number;
        PortValidation: VirtualRadar.Interface.View.IValidationModelField;
        UseKeepAlive: boolean;
        UseKeepAliveValidation: VirtualRadar.Interface.View.IValidationModelField;
        IdleTimeoutMilliseconds: number;
        IdleTimeoutValidation: VirtualRadar.Interface.View.IValidationModelField;
        Passphrase: string;
        ComPort: string;
        ComPortValidation: VirtualRadar.Interface.View.IValidationModelField;
        BaudRate: number;
        BaudRateValidation: VirtualRadar.Interface.View.IValidationModelField;
        DataBits: number;
        DataBitsValidation: VirtualRadar.Interface.View.IValidationModelField;
        StopBits: number;
        Parity: number;
        Handshake: number;
        StartupText: string;
        ShutdownText: string;
        ReceiverLocationId: number;
        ReceiverLocationIdValidation: VirtualRadar.Interface.View.IValidationModelField;
        ReceiverUsage: number;
    }
    interface IReceiverLocationModel {
        UniqueId: number;
        Name: string;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField;
        Latitude: number;
        LatitudeValidation: VirtualRadar.Interface.View.IValidationModelField;
        Longitude: number;
        LongitudeValidation: VirtualRadar.Interface.View.IValidationModelField;
        IsBaseStationLocation: boolean;
    }
    interface IUserModel {
        UniqueId: string;
        Enabled: boolean;
        LoginName: string;
        LoginNameValidation: VirtualRadar.Interface.View.IValidationModelField;
        Name: string;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField;
        UIPassword: string;
        UIPasswordValidation: VirtualRadar.Interface.View.IValidationModelField;
    }
    interface ITestConnectionOutcomeModel {
        Title: string;
        Message: string;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Statistics {
    interface IViewModel {
        Name: string;
        BytesReceived: number;
        ConnectedDuration: string;
        ReceiverThroughput: number;
        ReceiverBadChecksum: number;
        CurrentBufferSize: number;
        BaseStationMessages: number;
        BadlyFormedBaseStationMessages: number;
        BadlyFormedBaseStationMessagesRatio: number;
        ModeSMessageCount: number;
        ModeSNoAdsbPayload: number;
        ModeSNoAdsbPayloadRatio: number;
        ModeSShortFrame: number;
        ModeSShortFrameUnusable: number;
        ModeSShortFrameUnusableRatio: number;
        ModeSLongFrame: number;
        ModeSWithPI: number;
        ModeSPIBadParity: number;
        ModeSPIBadParityRatio: number;
        ModeSDFCount: VirtualRadar.Plugin.WebAdmin.View.Statistics.IModeSDFCountModel[];
        AdsbMessages: number;
        AdsbRejected: number;
        AdsbRejectedRatio: number;
        PositionSpeedCheckExceeded: number;
        PositionsReset: number;
        PositionsOutOfRange: number;
        AdsbMessageTypeCount: VirtualRadar.Plugin.WebAdmin.View.Statistics.IAdsbMessageTypeCountModel[];
        AdsbMessageFormatCount: VirtualRadar.Plugin.WebAdmin.View.Statistics.IAdsbMessageFormatCountModel[];
    }
    interface IModeSDFCountModel {
        DF: number;
        Val: number;
    }
    interface IAdsbMessageTypeCountModel {
        N: number;
        Val: number;
    }
    interface IAdsbMessageFormatCountModel {
        Fmt: string;
        Val: number;
    }
}


declare module VirtualRadar.Plugin.WebAdmin {
    interface IJsonMenuEntry_KO {
        HtmlFileName: KnockoutObservable<string>;
        Name: KnockoutObservable<string>;
        IsPlugin: KnockoutObservable<boolean>;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog {
    interface IViewModel_KO {
        LogEntries: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog.ILogEntry_KO>;
    }
    interface ILogEntry_KO {
        Time: KnockoutObservable<string>;
        Icao: KnockoutObservable<string>;
        Registration: KnockoutObservable<string>;
        Country: KnockoutObservable<string>;
        Manufacturer: KnockoutObservable<string>;
        Model: KnockoutObservable<string>;
        ModelIcao: KnockoutObservable<string>;
        Operator: KnockoutObservable<string>;
        OperatorIcao: KnockoutObservable<string>;
        Serial: KnockoutObservable<string>;
        YearBuilt: KnockoutObservable<number>;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View {
    interface IAboutView_KO {
        Caption: KnockoutObservable<string>;
        ProductName: KnockoutObservable<string>;
        Version: KnockoutObservable<string>;
        BuildDate: KnockoutObservable<Date>;
        FormattedBuildDate: KnockoutObservable<string>;
        Copyright: KnockoutObservable<string>;
        Description: KnockoutObservable<string>;
        IsMono: KnockoutObservable<boolean>;
    }
    interface ILogView_KO {
        LogLines: KnockoutViewModelArray<string>;
    }
    interface IMainView_KO {
        BadPlugins: KnockoutObservable<number>;
        NewVer: KnockoutObservable<boolean>;
        NewVerUrl: KnockoutObservable<string>;
        Upnp: KnockoutObservable<boolean>;
        UpnpRouter: KnockoutObservable<boolean>;
        UpnpOn: KnockoutObservable<boolean>;
        LocalRoot: KnockoutObservable<string>;
        LanRoot: KnockoutObservable<string>;
        PublicRoot: KnockoutObservable<string>;
        Requests: KnockoutViewModelArray<VirtualRadar.Interface.View.IServerRequest_KO>;
        Feeds: KnockoutViewModelArray<VirtualRadar.Interface.View.IFeedStatus_KO>;
        Rebroadcasters: KnockoutViewModelArray<VirtualRadar.Interface.IRebroadcastServerConnection_KO>;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog {
    interface IViewModel_KO {
        Connectors: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog.IConnectorModel_KO>;
        Events: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog.IEventModel_KO>;
    }
    interface IConnectorModel_KO {
        Name: KnockoutObservable<string>;
    }
    interface IEventModel_KO {
        Id: KnockoutObservable<number>;
        ConnectorName: KnockoutObservable<string>;
        Time: KnockoutObservable<string>;
        Type: KnockoutObservable<string>;
        Message: KnockoutObservable<string>;
    }
}
declare module VirtualRadar.Interface.View {
    interface IServerRequest_KO {
        RemoteEndPoint: System.Net.IIPEndPoint_KO;
        DataVersion: KnockoutObservable<number>;
        User: KnockoutObservable<string>;
        RemoteAddr: KnockoutObservable<string>;
        RemotePort: KnockoutObservable<number>;
        LastRequest: KnockoutObservable<Date>;
        Bytes: KnockoutObservable<number>;
        LastUrl: KnockoutObservable<string>;
    }
    interface IFeedStatus_KO {
        Id: KnockoutObservable<number>;
        DataVersion: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        Merged: KnockoutObservable<boolean>;
        Polar: KnockoutObservable<boolean>;
        HasAircraftList: KnockoutObservable<boolean>;
        Connection: KnockoutObservable<VirtualRadar.Interface.Network.ConnectionStatus>;
        ConnDesc: KnockoutObservable<string>;
        Msgs: KnockoutObservable<number>;
        BadMsgs: KnockoutObservable<number>;
        Tracked: KnockoutObservable<number>;
    }
    interface IValidationModelField_KO {
        IsWarning: KnockoutObservable<boolean>;
        IsError: KnockoutObservable<boolean>;
        IsValid: KnockoutObservable<boolean>;
        Message: KnockoutObservable<string>;
    }
    interface IEnumModel_KO {
        Value: KnockoutObservable<number>;
        Description: KnockoutObservable<string>;
    }
}
declare module System.Net {
    interface IIPEndPoint_KO extends System.Net.IEndPoint_KO {
        AddressFamily: KnockoutObservable<System.Net.Sockets.AddressFamily>;
        Address: System.Net.IIPAddress_KO;
        Port: KnockoutObservable<number>;
    }
    interface IEndPoint_KO {
        AddressFamily: KnockoutObservable<System.Net.Sockets.AddressFamily>;
    }
    interface IIPAddress_KO {
        Address: KnockoutObservable<number>;
        AddressFamily: KnockoutObservable<System.Net.Sockets.AddressFamily>;
        ScopeId: KnockoutObservable<number>;
        IsIPv6Multicast: KnockoutObservable<boolean>;
        IsIPv6LinkLocal: KnockoutObservable<boolean>;
        IsIPv6SiteLocal: KnockoutObservable<boolean>;
        IsIPv6Teredo: KnockoutObservable<boolean>;
        IsIPv4MappedToIPv6: KnockoutObservable<boolean>;
    }
}
declare module VirtualRadar.Interface {
    interface IRebroadcastServerConnection_KO {
        Id: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        LocalPort: KnockoutObservable<number>;
        EndpointIPAddress: System.Net.IIPAddress_KO;
        RemoteAddr: KnockoutObservable<string>;
        RemotePort: KnockoutObservable<number>;
        Buffered: KnockoutObservable<number>;
        Written: KnockoutObservable<number>;
        Discarded: KnockoutObservable<number>;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Queues {
    interface IViewModel_KO {
        Queues: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Queues.IQueueModel_KO>;
    }
    interface IQueueModel_KO {
        Name: KnockoutObservable<string>;
        CountQueuedItems: KnockoutObservable<number>;
        PeakQueuedItems: KnockoutObservable<number>;
        CountDroppedItems: KnockoutObservable<number>;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Settings {
    interface IViewModel_KO {
        CurrentUserName: KnockoutObservable<string>;
        Configuration: VirtualRadar.Plugin.WebAdmin.View.Settings.IConfigurationModel_KO;
        Outcome: KnockoutObservable<string>;
        NewMergedFeed: VirtualRadar.Plugin.WebAdmin.View.Settings.IMergedFeedModel_KO;
        NewRebroadcastServer: VirtualRadar.Plugin.WebAdmin.View.Settings.IRebroadcastServerModel_KO;
        NewReceiver: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverModel_KO;
        NewReceiverLocation: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel_KO;
        NewUser: VirtualRadar.Plugin.WebAdmin.View.Settings.IUserModel_KO;
        ConnectionTypes: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        DataSources: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        DefaultAccesses: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        DistanceUnits: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        Handshakes: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        HeightUnits: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        Parities: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        ProxyTypes: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        RebroadcastFormats: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        ReceiverUsages: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        SpeedUnits: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        StopBits: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
        ComPortNames: KnockoutViewModelArray<string>;
    }
    interface IConfigurationModel_KO {
        DataVersion: KnockoutObservable<number>;
        OnlineLookupSupplierName: KnockoutObservable<string>;
        OnlineLookupSupplierCredits: KnockoutObservable<string>;
        OnlineLookupSupplierUrl: KnockoutObservable<string>;
        BaseStationSettings: VirtualRadar.Plugin.WebAdmin.View.Settings.IBaseStationSettingsModel_KO;
        GoogleMapSettings: VirtualRadar.Plugin.WebAdmin.View.Settings.IGoogleMapSettingsModel_KO;
        RawDecodingSettings: VirtualRadar.Plugin.WebAdmin.View.Settings.IRawDecodingSettingModel_KO;
        MergedFeeds: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Settings.IMergedFeedModel_KO>;
        RebroadcastSettings: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Settings.IRebroadcastServerModel_KO>;
        Receivers: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverModel_KO>;
        ReceiverLocations: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel_KO>;
        Users: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Settings.IUserModel_KO>;
    }
    interface IBaseStationSettingsModel_KO {
        DatabaseFileName: KnockoutObservable<string>;
        DatabaseFileNameValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        OperatorFlagsFolder: KnockoutObservable<string>;
        OperatorFlagsFolderValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        SilhouettesFolder: KnockoutObservable<string>;
        SilhouettesFolderValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        PicturesFolder: KnockoutObservable<string>;
        PicturesFolderValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        SearchPictureSubFolders: KnockoutObservable<boolean>;
        DisplayTimeoutSeconds: KnockoutObservable<number>;
        TrackingTimeoutSeconds: KnockoutObservable<number>;
        MinimiseToSystemTray: KnockoutObservable<boolean>;
        AutoSavePolarPlotsMinutes: KnockoutObservable<number>;
        LookupAircraftDetailsOnline: KnockoutObservable<boolean>;
    }
    interface IGoogleMapSettingsModel_KO {
        InitialSettings: KnockoutObservable<string>;
        InitialMapLatitude: KnockoutObservable<number>;
        InitialMapLongitude: KnockoutObservable<number>;
        InitialMapType: KnockoutObservable<string>;
        InitialMapZoom: KnockoutObservable<number>;
        InitialRefreshSeconds: KnockoutObservable<number>;
        MinimumRefreshSeconds: KnockoutObservable<number>;
        ShortTrailLengthSeconds: KnockoutObservable<number>;
        InitialDistanceUnit: KnockoutObservable<number>;
        InitialHeightUnit: KnockoutObservable<number>;
        InitialSpeedUnit: KnockoutObservable<number>;
        PreferIataAirportCodes: KnockoutObservable<boolean>;
        EnableBundling: KnockoutObservable<boolean>;
        EnableMinifying: KnockoutObservable<boolean>;
        EnableCompression: KnockoutObservable<boolean>;
        WebSiteReceiverId: KnockoutObservable<number>;
        WebSiteReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        DirectoryEntryKey: KnockoutObservable<string>;
        ClosestAircraftReceiverId: KnockoutObservable<number>;
        ClosestAircraftReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        FlightSimulatorXReceiverId: KnockoutObservable<number>;
        FlightSimulatorXReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        ProxyType: KnockoutObservable<number>;
    }
    interface IRawDecodingSettingModel_KO {
        ReceiverRange: KnockoutObservable<number>;
        ReceiverRangeValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        IgnoreMilitaryExtendedSquitter: KnockoutObservable<boolean>;
        SuppressReceiverRangeCheck: KnockoutObservable<boolean>;
        UseLocalDecodeForInitialPosition: KnockoutObservable<boolean>;
        AirborneGlobalPositionLimit: KnockoutObservable<number>;
        AirborneGlobalPositionLimitValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        FastSurfaceGlobalPositionLimit: KnockoutObservable<number>;
        FastSurfaceGlobalPositionLimitValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        SlowSurfaceGlobalPositionLimit: KnockoutObservable<number>;
        SlowSurfaceGlobalPositionLimitValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        AcceptableAirborneSpeed: KnockoutObservable<number>;
        AcceptableAirborneSpeedValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        AcceptableAirSurfaceTransitionSpeed: KnockoutObservable<number>;
        AcceptableAirSurfaceTransitionSpeedValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        AcceptableSurfaceSpeed: KnockoutObservable<number>;
        AcceptableSurfaceSpeedValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        IgnoreCallsignsInBds20: KnockoutObservable<boolean>;
        AcceptIcaoInPI0Count: KnockoutObservable<number>;
        AcceptIcaoInPI0CountValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        AcceptIcaoInPI0Seconds: KnockoutObservable<number>;
        AcceptIcaoInPI0SecondsValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        AcceptIcaoInNonPICount: KnockoutObservable<number>;
        AcceptIcaoInNonPICountValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        AcceptIcaoInNonPISeconds: KnockoutObservable<number>;
        AcceptIcaoInNonPISecondsValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        SuppressIcao0: KnockoutObservable<boolean>;
        IgnoreInvalidCodeBlockInParityMessages: KnockoutObservable<boolean>;
        IgnoreInvalidCodeBlockInOtherMessages: KnockoutObservable<boolean>;
        SuppressTisbDecoding: KnockoutObservable<boolean>;
    }
    interface IMergedFeedModel_KO {
        Enabled: KnockoutObservable<boolean>;
        UniqueId: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        ReceiverIds: KnockoutViewModelArray<number>;
        ReceiverIdsValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        ReceiverFlags: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Settings.IMergedFeedReceiverModel_KO>;
        IcaoTimeout: KnockoutObservable<number>;
        IcaoTimeoutValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        IgnoreAircraftWithNoPosition: KnockoutObservable<boolean>;
        ReceiverUsage: KnockoutObservable<VirtualRadar.Interface.Settings.ReceiverUsage>;
    }
    interface IMergedFeedReceiverModel_KO {
        UniqueId: KnockoutObservable<number>;
        IsMlatFeed: KnockoutObservable<boolean>;
    }
    interface IRebroadcastServerModel_KO {
        UniqueId: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Enabled: KnockoutObservable<boolean>;
        ReceiverId: KnockoutObservable<number>;
        ReceiverIdValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Format: KnockoutObservable<number>;
        FormatValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        IsTransmitter: KnockoutObservable<boolean>;
        IsTransmitterValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        TransmitAddress: KnockoutObservable<string>;
        TransmitAddressValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Port: KnockoutObservable<number>;
        PortValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        UseKeepAlive: KnockoutObservable<boolean>;
        UseKeepAliveValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        IdleTimeoutMilliseconds: KnockoutObservable<number>;
        IdleTimeoutMillisecondsValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        StaleSeconds: KnockoutObservable<number>;
        StaleSecondsValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Access: VirtualRadar.Plugin.WebAdmin.View.Settings.IAccessModel_KO;
        Passphrase: KnockoutObservable<string>;
        SendIntervalMilliseconds: KnockoutObservable<number>;
        SendIntervalMillisecondsValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
    }
    interface IAccessModel_KO {
        DefaultAccess: KnockoutObservable<number>;
        Addresses: KnockoutViewModelArray<string>;
    }
    interface IReceiverModel_KO {
        Enabled: KnockoutObservable<boolean>;
        EnabledValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        UniqueId: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        DataSource: KnockoutObservable<number>;
        ConnectionType: KnockoutObservable<number>;
        AutoReconnectAtStartup: KnockoutObservable<boolean>;
        IsPassive: KnockoutObservable<boolean>;
        IsPassiveValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Access: VirtualRadar.Plugin.WebAdmin.View.Settings.IAccessModel_KO;
        Address: KnockoutObservable<string>;
        AddressValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Port: KnockoutObservable<number>;
        PortValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        UseKeepAlive: KnockoutObservable<boolean>;
        UseKeepAliveValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        IdleTimeoutMilliseconds: KnockoutObservable<number>;
        IdleTimeoutValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Passphrase: KnockoutObservable<string>;
        ComPort: KnockoutObservable<string>;
        ComPortValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        BaudRate: KnockoutObservable<number>;
        BaudRateValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        DataBits: KnockoutObservable<number>;
        DataBitsValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        StopBits: KnockoutObservable<number>;
        Parity: KnockoutObservable<number>;
        Handshake: KnockoutObservable<number>;
        StartupText: KnockoutObservable<string>;
        ShutdownText: KnockoutObservable<string>;
        ReceiverLocationId: KnockoutObservable<number>;
        ReceiverLocationIdValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        ReceiverUsage: KnockoutObservable<number>;
    }
    interface IReceiverLocationModel_KO {
        UniqueId: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Latitude: KnockoutObservable<number>;
        LatitudeValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Longitude: KnockoutObservable<number>;
        LongitudeValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        IsBaseStationLocation: KnockoutObservable<boolean>;
    }
    interface IUserModel_KO {
        UniqueId: KnockoutObservable<string>;
        Enabled: KnockoutObservable<boolean>;
        LoginName: KnockoutObservable<string>;
        LoginNameValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        Name: KnockoutObservable<string>;
        NameValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        UIPassword: KnockoutObservable<string>;
        UIPasswordValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
    }
    interface ITestConnectionOutcomeModel_KO {
        Title: KnockoutObservable<string>;
        Message: KnockoutObservable<string>;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Statistics {
    interface IViewModel_KO {
        Name: KnockoutObservable<string>;
        BytesReceived: KnockoutObservable<number>;
        ConnectedDuration: KnockoutObservable<string>;
        ReceiverThroughput: KnockoutObservable<number>;
        ReceiverBadChecksum: KnockoutObservable<number>;
        CurrentBufferSize: KnockoutObservable<number>;
        BaseStationMessages: KnockoutObservable<number>;
        BadlyFormedBaseStationMessages: KnockoutObservable<number>;
        BadlyFormedBaseStationMessagesRatio: KnockoutObservable<number>;
        ModeSMessageCount: KnockoutObservable<number>;
        ModeSNoAdsbPayload: KnockoutObservable<number>;
        ModeSNoAdsbPayloadRatio: KnockoutObservable<number>;
        ModeSShortFrame: KnockoutObservable<number>;
        ModeSShortFrameUnusable: KnockoutObservable<number>;
        ModeSShortFrameUnusableRatio: KnockoutObservable<number>;
        ModeSLongFrame: KnockoutObservable<number>;
        ModeSWithPI: KnockoutObservable<number>;
        ModeSPIBadParity: KnockoutObservable<number>;
        ModeSPIBadParityRatio: KnockoutObservable<number>;
        ModeSDFCount: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Statistics.IModeSDFCountModel_KO>;
        AdsbMessages: KnockoutObservable<number>;
        AdsbRejected: KnockoutObservable<number>;
        AdsbRejectedRatio: KnockoutObservable<number>;
        PositionSpeedCheckExceeded: KnockoutObservable<number>;
        PositionsReset: KnockoutObservable<number>;
        PositionsOutOfRange: KnockoutObservable<number>;
        AdsbMessageTypeCount: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Statistics.IAdsbMessageTypeCountModel_KO>;
        AdsbMessageFormatCount: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Statistics.IAdsbMessageFormatCountModel_KO>;
    }
    interface IModeSDFCountModel_KO {
        DF: KnockoutObservable<number>;
        Val: KnockoutObservable<number>;
    }
    interface IAdsbMessageTypeCountModel_KO {
        N: KnockoutObservable<number>;
        Val: KnockoutObservable<number>;
    }
    interface IAdsbMessageFormatCountModel_KO {
        Fmt: KnockoutObservable<string>;
        Val: KnockoutObservable<number>;
    }
}


