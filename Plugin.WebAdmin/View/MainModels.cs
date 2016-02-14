using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.WebAdmin.View.Main
{
    public class ViewModel
    {
        public int BadPlugins { get; set; }

        public bool NewVer { get; set; }

        public string NewVerUrl { get; set; }

        public bool Upnp { get; set; }

        public bool UpnpRouter { get; set; }

        public bool UpnpOn { get; set; }

        public string LocalRoot { get; set; }

        public string LanRoot { get; set; }

        public string PublicRoot { get; set; }

        public List<ServerRequestModel> Requests { get; private set; }

        public List<FeedStatusModel> Feeds { get; private set; }

        public List<RebroadcastServerConnectionModel> Rebroadcasters { get; private set; }

        public ViewModel()
        {
            Requests = new List<ServerRequestModel>();
            Feeds = new List<FeedStatusModel>();
            Rebroadcasters = new List<RebroadcastServerConnectionModel>();
        }

        public ViewModel(IMainView view) : this()
        {
            RefreshFromView(view);
        }

        public void RefreshFromView(IMainView view)
        {
            BadPlugins =    view.InvalidPluginCount;
            NewVer =        view.NewVersionAvailable;
            NewVerUrl =     view.NewVersionDownloadUrl;
            Upnp =          view.UPnpEnabled;
            UpnpRouter =    view.UPnpRouterPresent;
            UpnpOn =        view.UPnpPortForwardingActive;
            LocalRoot =     view.WebServerLocalAddress;
            LanRoot =       view.WebServerNetworkAddress;
            PublicRoot =    view.WebServerExternalAddress;
        }

        public void RefreshFeedStatuses(FeedStatus[] feedStatuses)
        {
            CollectionHelper.ApplySourceToDestination(feedStatuses, Feeds,
                (source, dest)  => source.FeedId == dest.Id,
                (source)        => new FeedStatusModel(source),
                (source, dest)  => dest.RefreshFromFeedStatus(source)
            );
            Feeds.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));
        }

        public void RefreshRebroadcastServerConnections(IList<RebroadcastServerConnection> connections)
        {
            CollectionHelper.ApplySourceToDestination(connections, Rebroadcasters,
                (source, dest)  => source.RebroadcastServerId == dest.Id,
                (source)        => new RebroadcastServerConnectionModel(source),
                (source, dest)  => dest.RefreshFromRebroadcastServerConnection(source)
            );
            Rebroadcasters.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));
        }

        public void RefreshServerRequests(ServerRequest[] serverRequests)
        {
            var bytesSentMap = new Dictionary<string, long>();
            foreach(var serverRequest in serverRequests) {
                var remoteAddress = serverRequest.RemoteAddress ?? "";
                var bytesSentToOtherPort = 0L;
                if(!bytesSentMap.TryGetValue(remoteAddress, out bytesSentToOtherPort)) {
                    bytesSentMap.Add(remoteAddress, 0L);
                }
                var bytesSent = bytesSentToOtherPort + serverRequest.BytesSent;
                bytesSentMap[remoteAddress] = bytesSent;

                var model = Requests.FirstOrDefault(r => r.RemoteAddr == remoteAddress);
                if(model == null) {
                    model = new ServerRequestModel(serverRequest);
                    Requests.Add(model);
                } else {
                    model.RefreshFromServerRequest(serverRequest);
                    model.Bytes = bytesSent;
                }
            }

            foreach(var deletedRequest in Requests.Where(r => !bytesSentMap.ContainsKey(r.RemoteAddr)).ToArray()) {
                Requests.Remove(deletedRequest);
            }

            Requests.Sort((lhs, rhs) => String.Compare(lhs.RemoteAddr, rhs.RemoteAddr));
        }
    }

    public class ServerRequestModel
    {
        public string User { get; set; }

        public string RemoteAddr { get; set; }

        public long Bytes { get; set; }

        public string LastUrl { get; set; }

        public ServerRequestModel()
        {
        }

        public ServerRequestModel(ServerRequest serverRequest) : this()
        {
            RefreshFromServerRequest(serverRequest);
        }

        public void RefreshFromServerRequest(ServerRequest serverRequest)
        {
            User =          String.IsNullOrEmpty(serverRequest.UserName) ? User : serverRequest.UserName;
            RemoteAddr =    serverRequest.RemoteAddress ?? "";
            Bytes =         serverRequest.BytesSent;
            LastUrl =       serverRequest.LastUrl;
        }
    }

    public class FeedStatusModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Merged { get; set; }

        public bool Polar { get; set; }

        public bool HasAircraftList { get; set; }

        public ConnectionStatus Connection { get; set; }

        public string ConnDesc { get; set; }

        public long Msgs { get; set; }

        public long BadMsgs { get; set; }

        public int Tracked { get; set; }

        public FeedStatusModel()
        {
        }

        public FeedStatusModel(FeedStatus feedStatus) : this()
        {
            RefreshFromFeedStatus(feedStatus);
        }

        public void RefreshFromFeedStatus(FeedStatus feedStatus)
        {
            Id =                feedStatus.FeedId;
            Name =              feedStatus.Name;
            Merged =            feedStatus.IsMergedFeed;
            Polar =             feedStatus.HasPolarPlot;
            HasAircraftList =   feedStatus.HasAircraftList;
            Connection =        feedStatus.ConnectionStatus;
            ConnDesc =          feedStatus.ConnectionStatusDescription;
            Msgs =              feedStatus.TotalMessages;
            BadMsgs =           feedStatus.TotalBadMessages;
            Tracked =           feedStatus.TotalAircraft;
        }
    }

    public class RebroadcastServerConnectionModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int LocalPort { get; set; }

        public string RemoteAddr { get; set; }

        public int RemotePort { get; set; }

        public long Buffered { get; set; }

        public long Written { get; set; }

        public long Discarded { get; set; }

        public RebroadcastServerConnectionModel()
        {
        }

        public RebroadcastServerConnectionModel(RebroadcastServerConnection connection) : this()
        {
            RefreshFromRebroadcastServerConnection(connection);
        }

        public void RefreshFromRebroadcastServerConnection(RebroadcastServerConnection connection)
        {
            Id =            connection.RebroadcastServerId;
            Name =          connection.Name;
            LocalPort =     connection.LocalPort;
            RemoteAddr =    connection.RemoteAddress;
            RemotePort =    connection.EndpointPort;
            Buffered =      connection.BytesBuffered;
            Written =       connection.BytesWritten;
            Discarded =     connection.StaleBytesDiscarded;
        }
    }
}
