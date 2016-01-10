 
/// <reference path="Enums.ts" />

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
declare module VirtualRadar.Interface.View {
    interface IServerRequest {
        RemoteEndPoint: System.Net.IIPEndPoint;
        DataVersion: number;
        UserName: string;
        RemoteAddress: string;
        RemotePort: number;
    }
    interface IFeedStatus {
        FeedId: number;
        DataVersion: number;
        Name: string;
        IsMergedFeed: boolean;
        HasPolarPlot: boolean;
        HasAircraftList: boolean;
        ConnectionStatus: VirtualRadar.Interface.Network.ConnectionStatus;
        ConnectionStatusDescription: string;
        TotalMessages: number;
        TotalBadMessages: number;
        TotalAircraft: number;
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
        RebroadcastServerId: number;
        Name: string;
        LocalPort: number;
        EndpointIPAddress: System.Net.IIPAddress;
        RemoteAddress: string;
        EndpointPort: number;
        BytesBuffered: number;
        BytesWritten: number;
        StaleBytesDiscarded: number;
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
declare module VirtualRadar.Interface.View {
    interface IServerRequest_KO {
        RemoteEndPoint: KnockoutObservable<System.Net.IIPEndPoint_KO>;
        DataVersion: KnockoutObservable<number>;
        UserName: KnockoutObservable<string>;
        RemoteAddress: KnockoutObservable<string>;
        RemotePort: KnockoutObservable<number>;
    }
    interface IFeedStatus_KO {
        FeedId: KnockoutObservable<number>;
        DataVersion: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        IsMergedFeed: KnockoutObservable<boolean>;
        HasPolarPlot: KnockoutObservable<boolean>;
        HasAircraftList: KnockoutObservable<boolean>;
        ConnectionStatus: KnockoutObservable<VirtualRadar.Interface.Network.ConnectionStatus>;
        ConnectionStatusDescription: KnockoutObservable<string>;
        TotalMessages: KnockoutObservable<number>;
        TotalBadMessages: KnockoutObservable<number>;
        TotalAircraft: KnockoutObservable<number>;
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
        RebroadcastServerId: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
        LocalPort: KnockoutObservable<number>;
        EndpointIPAddress: KnockoutObservable<System.Net.IIPAddress_KO>;
        RemoteAddress: KnockoutObservable<string>;
        EndpointPort: KnockoutObservable<number>;
        BytesBuffered: KnockoutObservable<number>;
        BytesWritten: KnockoutObservable<number>;
        StaleBytesDiscarded: KnockoutObservable<number>;
    }
}


