 
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
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Settings {
    interface IViewModel {
        Configuration: VirtualRadar.Plugin.WebAdmin.View.Settings.IConfigurationModel;
        Outcome: string;
        NewReceiverLocation: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel;
    }
    interface IConfigurationModel {
        DataVersion: number;
        OnlineLookupSupplierName: string;
        OnlineLookupSupplierCredits: string;
        OnlineLookupSupplierUrl: string;
        BaseStationSettingsModel: VirtualRadar.Plugin.WebAdmin.View.Settings.IBaseStationSettingsModel;
        ReceiverLocations: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel[];
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
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.Settings {
    interface IViewModel_KO {
        Configuration: VirtualRadar.Plugin.WebAdmin.View.Settings.IConfigurationModel_KO;
        Outcome: KnockoutObservable<string>;
        NewReceiverLocation: VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel_KO;
    }
    interface IConfigurationModel_KO {
        DataVersion: KnockoutObservable<number>;
        OnlineLookupSupplierName: KnockoutObservable<string>;
        OnlineLookupSupplierCredits: KnockoutObservable<string>;
        OnlineLookupSupplierUrl: KnockoutObservable<string>;
        BaseStationSettingsModel: VirtualRadar.Plugin.WebAdmin.View.Settings.IBaseStationSettingsModel_KO;
        ReceiverLocations: KnockoutViewModelArray<VirtualRadar.Plugin.WebAdmin.View.Settings.IReceiverLocationModel_KO>;
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


