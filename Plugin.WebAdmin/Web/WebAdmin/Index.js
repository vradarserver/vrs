var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Index;
        (function (Index) {
            var PageHandler = (function () {
                function PageHandler() {
                    this._ViewId = new WebAdmin.ViewId('Index');
                    this.refreshState();
                }
                PageHandler.prototype.refreshState = function () {
                    var _this = this;
                    this._ViewId.ajax('GetState', {
                        success: function (data) {
                            _this.applyState(data);
                            setTimeout(function () { return _this.refreshState(); }, 1000);
                        },
                        error: function () {
                            setTimeout(function () { return _this.refreshState(); }, 5000);
                        }
                    }, false);
                };
                PageHandler.prototype.toggleUPnpStatus = function () {
                    this._ViewId.ajax('RaiseToggleUPnpStatus');
                };
                PageHandler.prototype.resetPolarPlot = function () {
                    var feed = this._Model.SelectedFeed();
                    this._ViewId.ajax('RaiseResetPolarPlot', {
                        data: {
                            feedId: feed.Id()
                        }
                    });
                };
                PageHandler.prototype.reconnectFeed = function () {
                    var feed = this._Model.SelectedFeed();
                    this._ViewId.ajax('RaiseReconnectFeed', {
                        data: {
                            feedId: feed.Id()
                        }
                    });
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
                                        { page: '/settings.html', description: VRS.Server.$$.SettingsPage },
                                    ];
                                    var initialPerspective = 0;
                                    var currentUrl = window.location.href.toLowerCase();
                                    if (currentUrl.indexOf(root.LanRoot().toLowerCase()) !== -1) {
                                        initialPerspective = 1;
                                    }
                                    else if (currentUrl.indexOf(root.PublicRoot().toLowerCase()) !== -1) {
                                        initialPerspective = 2;
                                    }
                                    root.SelectedAddressPerspective = ko.observable(root.AddressPerspectives[initialPerspective]);
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
                                    feed.StatusClass = ko.computed(function () { return feed.ConnDesc() === VRS.Server.$$.Connected ? '' : 'bg-danger'; });
                                    feed.ConnectorActivityLogUrl = ko.computed(function () { return feed.Merged() ? '' : 'ConnectorActivityLog.html?connectorName=' + encodeURIComponent(feed.Name()); });
                                    feed.StatisticsUrl = ko.computed(function () { return feed.Merged() ? '' : 'Statistics.html?feedId=' + encodeURIComponent(String(feed.Id())); });
                                    feed.StatisticsTarget = ko.computed(function () { return 'statistics-' + feed.Id(); });
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
            }());
            Index.PageHandler = PageHandler;
        })(Index = WebAdmin.Index || (WebAdmin.Index = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=Index.js.map