var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListUrl = VRS.globalOptions.aircraftListUrl || 'AircraftList.json';
    VRS.globalOptions.aircraftListFlightSimUrl = VRS.globalOptions.aircraftListFlightSimUrl || 'FlightSimList.json';
    VRS.globalOptions.aircraftListDataType = VRS.globalOptions.aircraftListDataType || 'json';
    VRS.globalOptions.aircraftListTimeout = VRS.globalOptions.aircraftListTimeout || 10000;
    VRS.globalOptions.aircraftListRetryInterval = VRS.globalOptions.aircraftListRetryInterval || 15000;
    VRS.globalOptions.aircraftListFixedRefreshInterval = VRS.globalOptions.aircraftListFixedRefreshInterval || -1;
    VRS.globalOptions.aircraftListRequestFeedId = VRS.globalOptions.aircraftListRequestFeedId !== undefined ? VRS.globalOptions.aircraftListRequestFeedId : undefined;
    VRS.globalOptions.aircraftListUserCanChangeFeeds = VRS.globalOptions.aircraftListUserCanChangeFeeds !== undefined ? VRS.globalOptions.aircraftListUserCanChangeFeeds : true;
    VRS.globalOptions.aircraftListHideAircraftNotOnMap = VRS.globalOptions.aircraftListHideAircraftNotOnMap !== undefined ? VRS.globalOptions.aircraftListHideAircraftNotOnMap : false;
    var AircraftListFetcher = (function () {
        function AircraftListFetcher(settings) {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.AircraftListFetcher'
            });
            this._Events = {
                pausedChanged: 'pausedChanged',
                hideAircraftNotOnMapChanged: 'hideAircraftNotOnMapChanged'
            };
            this._ServerConfigChangedHook = null;
            this._SiteTimedOutHook = null;
            this._MinimumRefreshInterval = -1;
            this._ServerConfigDefaultRefreshInterval = -1;
            this._Feeds = [];
            this._RequestFeedId = VRS.globalOptions.aircraftListRequestFeedId;
            this._ActualFeedId = undefined;
            this._IntervalMilliseconds = VRS.globalOptions.aircraftListFixedRefreshInterval;
            this._Paused = true;
            this._HideAircraftNotOnMap = VRS.globalOptions.aircraftListHideAircraftNotOnMap;
            this._MapPlugin = null;
            this._MapJQ = null;
            this.loadAndApplyState = function () {
                this.applyState(this.loadState());
            };
            this.createOptionPane = function (displayOrder) {
                var pane = new VRS.OptionPane({
                    name: 'vrsAircraftListFetcherPane',
                    titleKey: 'PaneDataFeed',
                    displayOrder: displayOrder
                });
                if (this.getFeeds().length > 1 && VRS.globalOptions.aircraftListUserCanChangeFeeds && !this._Settings.fetchFsxList) {
                    var values = [
                        new VRS.ValueText({ value: undefined, textKey: 'DefaultSetting' })
                    ];
                    var feeds = this.getSortedFeeds();
                    $.each(feeds, function (idx, feed) {
                        values.push(new VRS.ValueText({ value: feed.id, text: feed.name }));
                    });
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'feed',
                        labelKey: 'Receiver',
                        getValue: this.getRequestFeedId,
                        setValue: this.setRequestFeedId,
                        saveState: this.saveState,
                        values: values
                    }));
                }
                if (VRS.globalOptions.aircraftListFixedRefreshInterval === -1) {
                    pane.addField(new VRS.OptionFieldNumeric({
                        name: 'intervalMilliseconds',
                        labelKey: 'IntervalSeconds',
                        getValue: this.getIntervalSeconds,
                        setValue: this.setIntervalSeconds,
                        saveState: this.saveState,
                        inputWidth: VRS.InputWidth.ThreeChar,
                        min: Math.floor(this._MinimumRefreshInterval / 1000),
                        max: 60,
                        decimals: 0
                    }));
                }
                if (!this._Settings.fetchFsxList) {
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'hideAircraftNotOnMap',
                        labelKey: 'HideAircraftNotOnMap',
                        getValue: this.getHideAircraftNotOnMap,
                        setValue: this.setHideAircraftNotOnMap,
                        saveState: this.saveState
                    }));
                }
                return pane;
            };
            if (!settings.aircraftList)
                throw 'The aircraft list to fetch values for must be supplied';
            this._Settings = $.extend({
                name: 'default',
                aircraftList: null,
                currentLocation: null,
                mapJQ: null,
                fetchFsxList: false
            }, settings);
            this.setMapJQ(settings.mapJQ);
            this._ServerConfigChangedHook = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.serverConfigChanged, this);
            this._SiteTimedOutHook = VRS.timeoutManager ? VRS.timeoutManager.hookSiteTimedOut(this.siteTimedOut, this) : null;
            this.applyServerConfiguration();
        }
        AircraftListFetcher.prototype.getName = function () {
            return this._Settings.name;
        };
        AircraftListFetcher.prototype.getFeeds = function () {
            return this._Feeds;
        };
        AircraftListFetcher.prototype.getSortedFeeds = function (includeDefaultFeed) {
            if (includeDefaultFeed === void 0) { includeDefaultFeed = false; }
            var feeds = this._Feeds.slice();
            if (includeDefaultFeed)
                feeds.push({ name: VRS.$$.DefaultSetting });
            feeds.sort(function (lhs, rhs) {
                if (lhs.id !== undefined && rhs.id !== undefined)
                    return lhs.name.localeCompare(rhs.name);
                else if (lhs.id === undefined && rhs.id === undefined)
                    return 0;
                else if (lhs.id === undefined)
                    return -1;
                else
                    return 1;
            });
            return feeds;
        };
        AircraftListFetcher.prototype.getFeed = function (id) {
            var result = null;
            var length = this._Feeds.length;
            for (var i = 0; i < length; ++i) {
                var feed = this._Feeds[i];
                if (feed.id === id) {
                    result = feed;
                    break;
                }
            }
            return result;
        };
        AircraftListFetcher.prototype.getRequestFeedId = function () {
            return this._RequestFeedId;
        };
        AircraftListFetcher.prototype.setRequestFeedId = function (value) {
            this._RequestFeedId = value === null ? undefined : value;
            if (this._RequestFeedId) {
                this._RequestFeedId = Number(this._RequestFeedId);
                if (isNaN(this._RequestFeedId))
                    this._RequestFeedId = undefined;
            }
        };
        AircraftListFetcher.prototype.getActualFeedId = function () {
            return this._ActualFeedId;
        };
        AircraftListFetcher.prototype.getActualFeed = function () {
            var result = null;
            var feedId = this._ActualFeedId;
            if (feedId !== undefined) {
                $.each(this._Feeds, function (idx, feed) {
                    if (feed.id === feedId)
                        result = feed;
                    return result === null;
                });
            }
            return result;
        };
        AircraftListFetcher.prototype.getInterval = function () {
            var result = this._IntervalMilliseconds;
            if (result === -1) {
                result = this._ServerConfigDefaultRefreshInterval;
                if (result === -1)
                    result = 1000;
            }
            if (result < this._MinimumRefreshInterval)
                result = this._MinimumRefreshInterval;
            return result;
        };
        AircraftListFetcher.prototype.setInterval = function (value) {
            if (this._IntervalMilliseconds != value) {
                if (this._TimeoutHandle) {
                    clearTimeout(this._TimeoutHandle);
                    this._TimeoutHandle = undefined;
                }
                this._IntervalMilliseconds = value;
                if (!this._Paused)
                    this.fetch();
            }
        };
        AircraftListFetcher.prototype.getPaused = function () {
            return this._Paused;
        };
        AircraftListFetcher.prototype.setPaused = function (value) {
            if (value !== this._Paused) {
                this._Paused = value;
                if (!this._Paused && VRS.timeoutManager) {
                    VRS.timeoutManager.restartTimedOutSite();
                    VRS.timeoutManager.resetTimer();
                }
                if (!this._Paused && !this._TimeoutHandle)
                    this.fetch();
                else if (this._Paused && this._TimeoutHandle) {
                    clearTimeout(this._TimeoutHandle);
                    this._TimeoutHandle = undefined;
                }
                this._Dispatcher.raise(this._Events.pausedChanged);
            }
        };
        AircraftListFetcher.prototype.getHideAircraftNotOnMap = function () {
            return this._HideAircraftNotOnMap;
        };
        AircraftListFetcher.prototype.setHideAircraftNotOnMap = function (value) {
            if (this._HideAircraftNotOnMap !== value) {
                this._HideAircraftNotOnMap = value;
                this._Dispatcher.raise(this._Events.hideAircraftNotOnMapChanged);
            }
        };
        AircraftListFetcher.prototype.getMapJQ = function () {
            return this._MapJQ;
        };
        AircraftListFetcher.prototype.setMapJQ = function (value) {
            this._MapJQ = value;
            this._MapPlugin = this._MapJQ ? VRS.jQueryUIHelper.getMapPlugin(this._MapJQ) : null;
        };
        ;
        AircraftListFetcher.prototype.hookPausedChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.pausedChanged, callback, forceThis);
        };
        AircraftListFetcher.prototype.hookHideAircraftNotOnMapChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.hideAircraftNotOnMapChanged, callback, forceThis);
        };
        AircraftListFetcher.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        AircraftListFetcher.prototype.dispose = function () {
            if (this._ServerConfigChangedHook) {
                VRS.globalDispatch.unhook(this._ServerConfigChangedHook);
                this._ServerConfigChangedHook = null;
            }
            if (this._SiteTimedOutHook) {
                VRS.timeoutManager.unhook(this._SiteTimedOutHook);
                this._SiteTimedOutHook = null;
            }
        };
        AircraftListFetcher.prototype.saveState = function () {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        };
        AircraftListFetcher.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);
            if (VRS.globalOptions.aircraftListFixedRefreshInterval !== -1)
                result.interval = VRS.globalOptions.aircraftListFixedRefreshInterval;
            var minimumInterval = Math.max(1000, this._MinimumRefreshInterval);
            if (result.interval < minimumInterval)
                result.interval = minimumInterval;
            return result;
        };
        AircraftListFetcher.prototype.applyState = function (settings) {
            this.setInterval(settings.interval);
            this.setHideAircraftNotOnMap(settings.hideAircraftNotOnMap);
            this.setRequestFeedId(settings.requestFeedId);
        };
        AircraftListFetcher.prototype.persistenceKey = function () {
            return 'vrsAircraftListFetcher-' + this.getName();
        };
        AircraftListFetcher.prototype.createSettings = function () {
            return {
                interval: this.getInterval(),
                requestFeedId: this.getRequestFeedId(),
                hideAircraftNotOnMap: this.getHideAircraftNotOnMap()
            };
        };
        AircraftListFetcher.prototype.getIntervalSeconds = function () {
            return Math.floor(this.getInterval() / 1000);
        };
        AircraftListFetcher.prototype.setIntervalSeconds = function (value) {
            this.setInterval(value * 1000);
        };
        AircraftListFetcher.prototype.applyServerConfiguration = function () {
            if (VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if (config) {
                    this._MinimumRefreshInterval = config.MinimumRefreshSeconds * 1000;
                    this._ServerConfigDefaultRefreshInterval = config.RefreshSeconds * 1000;
                }
            }
        };
        AircraftListFetcher.prototype.fetch = function () {
            this._TimeoutHandle = undefined;
            var getFreshList = !this._Settings.aircraftList.getDataVersion() || this._RequestFeedId !== this._LastRequestFeedId;
            this._LastRequestFeedId = this._RequestFeedId;
            var params = {};
            if (!getFreshList) {
                params.ldv = this._Settings.aircraftList.getDataVersion();
            }
            if (this._RequestFeedId !== undefined && this._RequestFeedId !== null) {
                params.feed = this._RequestFeedId;
            }
            if (this._Settings.currentLocation) {
                var location = this._Settings.currentLocation.getCurrentLocation();
                if (location) {
                    params.lat = location.lat;
                    params.lng = location.lng;
                }
            }
            if (this._Settings.aircraftList) {
                var selectedAircraft = this._Settings.aircraftList.getSelectedAircraft();
                if (selectedAircraft)
                    params.selAc = selectedAircraft.id;
            }
            if (this._HideAircraftNotOnMap && this._MapPlugin) {
                var bounds = this._MapPlugin.getBounds();
                if (!bounds) {
                    setTimeout($.proxy(this.fetch, this), 500);
                    return;
                }
                if (bounds.tlLat || bounds.tlLng || bounds.brLat || bounds.brLng) {
                    params.fNBnd = bounds.tlLat;
                    params.fEBnd = bounds.brLng;
                    params.fSBnd = bounds.brLat;
                    params.fWBnd = bounds.tlLng;
                }
            }
            var headers = {};
            var postBody = {};
            if (!getFreshList) {
                postBody.icaos = this._Settings.aircraftList.getAllAircraftIcaosString();
            }
            this._Settings.aircraftList.raiseFetchingList(params, headers, postBody);
            var url = VRS.browserHelper.formUrl(this._Settings.fetchFsxList ? VRS.globalOptions.aircraftListFlightSimUrl : VRS.globalOptions.aircraftListUrl, params, false);
            $.ajax({
                url: url,
                data: postBody,
                method: 'POST',
                dataType: VRS.globalOptions.aircraftListDataType,
                headers: headers,
                timeout: VRS.globalOptions.aircraftListTimeout,
                success: $.proxy(this.fetchSuccess, this),
                error: $.proxy(this.fetchError, this)
            });
        };
        AircraftListFetcher.prototype.fetchSuccess = function (data) {
            this._TimeoutHandle = undefined;
            if (!this._Paused) {
                if (data.feeds) {
                    this._Feeds = data.feeds;
                    this._ActualFeedId = data.srcFeed;
                }
                if (data.configChanged && VRS.serverConfig) {
                    VRS.serverConfig.fetch(function () { });
                }
                this._Settings.aircraftList.applyJson(data, this);
                if (this._Settings.fetchFsxList) {
                    var fsxAircraft = this._Settings.aircraftList.findAircraftById(1);
                    var selectedAircraft = this._Settings.aircraftList.getSelectedAircraft();
                    var newAircraft = selectedAircraft !== fsxAircraft;
                    this._Settings.aircraftList.setSelectedAircraft(fsxAircraft, false);
                    if (newAircraft && fsxAircraft && fsxAircraft.getPosition() && this._MapPlugin)
                        this._MapPlugin.panTo(fsxAircraft.getPosition());
                }
                this._TimeoutHandle = setTimeout($.proxy(this.fetch, this), this._IntervalMilliseconds);
            }
        };
        AircraftListFetcher.prototype.fetchError = function () {
            this._TimeoutHandle = setTimeout($.proxy(this.fetch, this), VRS.globalOptions.aircraftListRetryInterval);
        };
        AircraftListFetcher.prototype.serverConfigChanged = function () {
            this.applyServerConfiguration();
        };
        AircraftListFetcher.prototype.siteTimedOut = function () {
            this.setPaused(true);
        };
        return AircraftListFetcher;
    })();
    VRS.AircraftListFetcher = AircraftListFetcher;
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraftListFetcher.js.map