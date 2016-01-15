var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Index;
        (function (Index) {
            var PageHandler = (function () {
                function PageHandler() {
                    this.refreshState();
                }
                PageHandler.prototype.refreshState = function () {
                    var _this = this;
                    $.ajax({
                        url: 'Index/GetState',
                        success: function (data) {
                            _this.applyState(data);
                            setTimeout(function () { return _this.refreshState(); }, 1000);
                        },
                        error: function () {
                            setTimeout(function () { return _this.refreshState(); }, 5000);
                        }
                    });
                };
                PageHandler.prototype.toggleUPnpStatus = function () {
                    $.ajax('Index/RaiseToggleUPnpStatus');
                };
                PageHandler.prototype.resetPolarPlot = function () {
                    var feed = this._Model.SelectedFeed();
                    $.ajax({
                        url: 'Index/RaiseResetPolarPlot',
                        data: {
                            feedId: feed.Id()
                        }
                    });
                };
                PageHandler.prototype.reconnectFeed = function () {
                    var feed = this._Model.SelectedFeed();
                    $.ajax({
                        url: 'Index/RaiseReconnectFeed',
                        data: {
                            feedId: feed.Id()
                        }
                    });
                };
                PageHandler.prototype.gotoConnectorActivityLog = function () {
                    var feed = this._Model.SelectedFeed();
                    if (!feed.Merged()) {
                        window.location.href = feed.ConnectorActivityLogUrl();
                    }
                };
                PageHandler.prototype.applyState = function (state) {
                    if (this._Model) {
                        ko.viewmodel.updateFromModel(this._Model, state.Response);
                    }
                    else {
                        this._Model = ko.viewmodel.fromModel(state.Response, {
                            arrayChildId: {
                                '{root}.Feeds': 'Id',
                                '{root}.Requests': 'RemoteAddr',
                                '{root}.Rebroadcasters': 'Id'
                            },
                            extend: {
                                '{root}': function (root) {
                                    root.HasFailedPlugins = ko.computed(function () { return root.BadPlugins() > 0; });
                                    root.FailedPluginsMessage = ko.computed(function () { return VRS.stringUtility.format(VRS.Server.$$.CountPluginsCouldNotBeLoaded, root.BadPlugins()); });
                                    root.UpnpStatus = ko.computed(function () {
                                        return !root.Upnp() && root.UpnpOn() ? VRS.Server.$$.UPnpActiveWhileDisabled :
                                            !root.Upnp() ? VRS.Server.$$.UPnpDisabled :
                                                !root.UpnpRouter() ? VRS.Server.$$.UPnPNotPresent :
                                                    !root.UpnpOn() ? VRS.Server.$$.UPnpInactive :
                                                        VRS.Server.$$.UPnpActive;
                                    });
                                    root.UpnpButtonText = ko.computed(function () { return root.UpnpOn() ? VRS.Server.$$.TakeOffInternet : VRS.Server.$$.PutOnInternet; });
                                    root.UpnpButtonDisabled = ko.computed(function () { return !root.UpnpRouter() || !root.Upnp(); });
                                    root.AddressPerspectives = [
                                        { perspective: 'local', description: VRS.Server.$$.ShowLocalAddress },
                                        { perspective: 'network', description: VRS.Server.$$.ShowNetworkAddress },
                                        { perspective: 'public', description: VRS.Server.$$.ShowInternetAddress },
                                    ];
                                    root.AddressPages = [
                                        { page: '/', description: VRS.Server.$$.DefaultVersion },
                                        { page: '/desktop.html', description: VRS.Server.$$.DesktopVersion },
                                        { page: '/mobile.html', description: VRS.Server.$$.MobileVersion },
                                        { page: '/fsx.html', description: VRS.Server.$$.FlightSimVersion },
                                        { page: '/GoogleMap.htm', description: VRS.Server.$$.DesktopVersionOld },
                                        { page: '/iPhoneMap.htm', description: VRS.Server.$$.MobileVersionOld },
                                        { page: '/FlightSim.htm', description: VRS.Server.$$.FlightSimVersionOld },
                                        { page: '/settings.html', description: VRS.Server.$$.SettingsPage },
                                    ];
                                    root.SelectedAddressPerspective = ko.observable(root.AddressPerspectives[0]);
                                    root.SelectedAddressPage = ko.observable(root.AddressPages[0]);
                                    root.AddressUrl = ko.computed(function () {
                                        var result = '#';
                                        if (root.SelectedAddressPerspective() && root.SelectedAddressPage()) {
                                            switch (root.SelectedAddressPerspective().perspective) {
                                                case 'local':
                                                    result = root.LocalRoot();
                                                    break;
                                                case 'network':
                                                    result = root.LanRoot();
                                                    break;
                                                case 'public':
                                                    result = root.PublicRoot();
                                                    break;
                                            }
                                            result += root.SelectedAddressPage().page;
                                        }
                                        return result;
                                    });
                                    root.SelectedFeed = ko.observable({});
                                    root.SelectFeed = function (feed) {
                                        root.SelectedFeed(feed);
                                    };
                                },
                                '{root}.Feeds[i]': function (feed) {
                                    feed.FormattedMsgs = ko.computed(function () { return VRS.stringUtility.format('{0:N0}', feed.Msgs()); });
                                    feed.FormattedBadMsgs = ko.computed(function () { return VRS.stringUtility.format('{0:N0}', feed.BadMsgs()); });
                                    feed.FormattedTracked = ko.computed(function () { return VRS.stringUtility.format('{0:N0}', feed.Tracked()); });
                                    feed.StatusClass = ko.computed(function () { return feed.ConnDesc() === VRS.Server.$$.Connected ? 'bg-success' : 'bg-danger'; });
                                    feed.ConnectorActivityLogUrl = ko.computed(function () { return feed.Merged() ? '' : 'ConnectorActivityLog.html?connectorName=' + encodeURIComponent(feed.Name()); });
                                },
                                '{root}.Requests[i]': function (request) {
                                    request.FormattedBytes = ko.computed(function () { return VRS.stringUtility.format('{0:N0}', request.Bytes()); });
                                },
                                '{root}.Rebroadcasters[i]': function (rebroadcast) {
                                    rebroadcast.FormattedBuffered = ko.computed(function () { return VRS.stringUtility.format('{0:N0}', rebroadcast.Buffered()); });
                                    rebroadcast.FormattedDiscarded = ko.computed(function () { return VRS.stringUtility.format('{0:N0}', rebroadcast.Discarded()); });
                                    rebroadcast.FormattedWritten = ko.computed(function () { return VRS.stringUtility.format('{0:N0}', rebroadcast.Written()); });
                                }
                            }
                        });
                        ko.applyBindings(this._Model);
                    }
                };
                return PageHandler;
            })();
            Index.PageHandler = PageHandler;
        })(Index = WebAdmin.Index || (WebAdmin.Index = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=Index.js.map