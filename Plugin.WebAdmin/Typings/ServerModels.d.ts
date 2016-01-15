 
/// <reference path="Enums.ts" />

declare module VirtualRadar.Plugin.WebAdmin {
    interface IJsonMenuEntry {
        HtmlFileName: string;
        Name: string;
        IsPlugin: boolean;
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


declare module VirtualRadar.Plugin.WebAdmin {
    interface IJsonMenuEntry_KO {
        HtmlFileName: KnockoutObservable<string>;
        Name: KnockoutObservable<string>;
        IsPlugin: KnockoutObservable<boolean>;
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
        LogLines: KnockoutObservableArray<string>;
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
        Requests: KnockoutObservableArray<VirtualRadar.Interface.View.IServerRequest_KO>;
        Feeds: KnockoutObservableArray<VirtualRadar.Interface.View.IFeedStatus_KO>;
        Rebroadcasters: KnockoutObservableArray<VirtualRadar.Interface.IRebroadcastServerConnection_KO>;
    }
}
declare module VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog {
    interface IViewModel_KO {
        Connectors: KnockoutObservableArray<VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog.IConnectorModel_KO>;
        Events: KnockoutObservableArray<VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog.IEventModel_KO>;
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
        RemoteEndPoint: KnockoutObservable<System.Net.IIPEndPoint_KO>;
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
}
declare module System.Net {
    interface IIPEndPoint_KO extends System.Net.IEndPoint_KO {
        AddressFamily: KnockoutObservable<System.Net.Sockets.AddressFamily>;
        Address: KnockoutObservable<System.Net.IIPAddress_KO>;
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
        EndpointIPAddress: KnockoutObservable<System.Net.IIPAddress_KO>;
        RemoteAddr: KnockoutObservable<string>;
        RemotePort: KnockoutObservable<number>;
        Buffered: KnockoutObservable<number>;
        Written: KnockoutObservable<number>;
        Discarded: KnockoutObservable<number>;
    }
}


