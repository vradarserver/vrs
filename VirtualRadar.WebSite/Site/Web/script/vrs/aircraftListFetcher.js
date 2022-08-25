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
            var _this = this;
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.AircraftListFetcher'
            });
            this._Events = {
                pausedChanged: 'pausedChanged',
                hideAircraftNotOnMapChanged: 'hideAircraftNotOnMapChanged',
                listFetched: 'listFetched'
            };
            this._ServerConfigChangedHook = null;
            this._SiteTimedOutHook = null;
            this._MinimumRefreshInterval = -1;
            this._ServerConfigDefaultRefreshInterval = -1;
            this._Feeds = [];
            this._RequestFeedId = VRS.globalOptions.aircraftListRequestFeedId;
            this._IntervalMilliseconds = VRS.globalOptions.aircraftListFixedRefreshInterval;
            this._Paused = true;
            this._HideAircraftNotOnMap = VRS.globalOptions.aircraftListHideAircraftNotOnMap;
            this._MapPlugin = null;
            this._MapJQ = null;
            this._ActualFeedId = undefined;
            this.getName = function () {
                return _this._Settings.name;
            };
            this.getFeeds = function () {
                return _this._Feeds;
            };
            this.getSortedFeeds = function (includeDefaultFeed) {
                if (includeDefaultFeed === void 0) { includeDefaultFeed = false; }
                var feeds = _this._Feeds.slice();
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
            this.getFeed = function (id) {
                var result = null;
                var length = _this._Feeds.length;
                for (var i = 0; i < length; ++i) {
                    var feed = _this._Feeds[i];
                    if (feed.id === id) {
                        result = feed;
                        break;
                    }
                }
                return result;
            };
            this.getRequestFeedId = function () {
                return _this._RequestFeedId;
            };
            this.setRequestFeedId = function (value) {
                _this._RequestFeedId = value === null ? undefined : value;
                if (_this._RequestFeedId) {
                    _this._RequestFeedId = Number(_this._RequestFeedId);
                    if (isNaN(_this._RequestFeedId))
                        _this._RequestFeedId = undefined;
                }
            };
            this.getActualFeedId = function () {
                return _this._ActualFeedId;
            };
            this.getActualFeed = function () {
                var result = null;
                var feedId = _this._ActualFeedId;
                if (feedId !== undefined) {
                    $.each(_this._Feeds, function (idx, feed) {
                        if (feed.id === feedId)
                            result = feed;
                        return result === null;
                    });
                }
                return result;
            };
            this.getInterval = function () {
                var result = _this._IntervalMilliseconds;
                if (result === -1) {
                    result = _this._ServerConfigDefaultRefreshInterval;
                    if (result === -1)
                        result = 1000;
                }
                if (result < _this._MinimumRefreshInterval)
                    result = _this._MinimumRefreshInterval;
                return result;
            };
            this.setInterval = function (value) {
                if (_this._IntervalMilliseconds != value) {
                    if (_this._TimeoutHandle) {
                        clearTimeout(_this._TimeoutHandle);
                        _this._TimeoutHandle = undefined;
                    }
                    _this._IntervalMilliseconds = value;
                    if (!_this._Paused)
                        _this.fetch();
                }
            };
            this.getPaused = function () {
                return _this._Paused;
            };
            this.setPaused = function (value) {
                if (value !== _this._Paused) {
                    _this._Paused = value;
                    if (!_this._Paused && VRS.timeoutManager) {
                        VRS.timeoutManager.restartTimedOutSite();
                        VRS.timeoutManager.resetTimer();
                    }
                    if (!_this._Paused && !_this._TimeoutHandle)
                        _this.fetch();
                    else if (_this._Paused && _this._TimeoutHandle) {
                        clearTimeout(_this._TimeoutHandle);
                        _this._TimeoutHandle = undefined;
                    }
                    _this._Dispatcher.raise(_this._Events.pausedChanged);
                }
            };
            this.getHideAircraftNotOnMap = function () {
                return _this._HideAircraftNotOnMap;
            };
            this.setHideAircraftNotOnMap = function (value) {
                if (_this._HideAircraftNotOnMap !== value) {
                    _this._HideAircraftNotOnMap = value;
                    _this._Dispatcher.raise(_this._Events.hideAircraftNotOnMapChanged);
                }
            };
            this.getMapJQ = function () {
                return _this._MapJQ;
            };
            this.setMapJQ = function (value) {
                _this._MapJQ = value;
                _this._MapPlugin = _this._MapJQ ? VRS.jQueryUIHelper.getMapPlugin(_this._MapJQ) : null;
            };
            this.hookPausedChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.pausedChanged, callback, forceThis);
            };
            this.hookHideAircraftNotOnMapChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.hideAircraftNotOnMapChanged, callback, forceThis);
            };
            this.hookListFetched = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.listFetched, callback, forceThis);
            };
            this.unhook = function (hookResult) {
                _this._Dispatcher.unhook(hookResult);
            };
            this.dispose = function () {
                if (_this._ServerConfigChangedHook) {
                    VRS.globalDispatch.unhook(_this._ServerConfigChangedHook);
                    _this._ServerConfigChangedHook = null;
                }
                if (_this._SiteTimedOutHook) {
                    VRS.timeoutManager.unhook(_this._SiteTimedOutHook);
                    _this._SiteTimedOutHook = null;
                }
            };
            this.saveState = function () {
                VRS.configStorage.save(_this.persistenceKey(), _this.createSettings());
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.load(_this.persistenceKey(), {});
                var result = $.extend(_this.createSettings(), savedSettings);
                if (VRS.globalOptions.aircraftListFixedRefreshInterval !== -1)
                    result.interval = VRS.globalOptions.aircraftListFixedRefreshInterval;
                var minimumInterval = Math.max(1000, _this._MinimumRefreshInterval);
                if (result.interval < minimumInterval)
                    result.interval = minimumInterval;
                return result;
            };
            this.applyState = function (settings) {
                _this.setInterval(settings.interval);
                _this.setHideAircraftNotOnMap(settings.hideAircraftNotOnMap);
                _this.setRequestFeedId(settings.requestFeedId);
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.persistenceKey = function () {
                return 'vrsAircraftListFetcher-' + _this.getName();
            };
            this.createSettings = function () {
                return {
                    interval: _this.getInterval(),
                    requestFeedId: _this.getRequestFeedId(),
                    hideAircraftNotOnMap: _this.getHideAircraftNotOnMap()
                };
            };
            this.createOptionPane = function (displayOrder) {
                var pane = new VRS.OptionPane({
                    name: 'vrsAircraftListFetcherPane',
                    titleKey: 'PaneDataFeed',
                    displayOrder: displayOrder
                });
                if (_this.getFeeds().length > 1 && VRS.globalOptions.aircraftListUserCanChangeFeeds && !_this._Settings.fetchFsxList) {
                    var values = [
                        new VRS.ValueText({ value: undefined, textKey: 'DefaultSetting' })
                    ];
                    var feeds = _this.getSortedFeeds();
                    $.each(feeds, function (idx, feed) {
                        values.push(new VRS.ValueText({ value: feed.id, text: feed.name }));
                    });
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'feed',
                        labelKey: 'Receiver',
                        getValue: _this.getRequestFeedId,
                        setValue: _this.setRequestFeedId,
                        saveState: _this.saveState,
                        values: values
                    }));
                }
                if (VRS.globalOptions.aircraftListFixedRefreshInterval === -1) {
                    pane.addField(new VRS.OptionFieldNumeric({
                        name: 'intervalMilliseconds',
                        labelKey: 'IntervalSeconds',
                        getValue: _this.getIntervalSeconds,
                        setValue: _this.setIntervalSeconds,
                        saveState: _this.saveState,
                        inputWidth: VRS.InputWidth.ThreeChar,
                        min: Math.floor(_this._MinimumRefreshInterval / 1000),
                        max: 60,
                        decimals: 0
                    }));
                }
                if (!_this._Settings.fetchFsxList) {
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'hideAircraftNotOnMap',
                        labelKey: 'HideAircraftNotOnMap',
                        getValue: _this.getHideAircraftNotOnMap,
                        setValue: _this.setHideAircraftNotOnMap,
                        saveState: _this.saveState
                    }));
                }
                return pane;
            };
            this.getIntervalSeconds = function () {
                return Math.floor(_this.getInterval() / 1000);
            };
            this.setIntervalSeconds = function (value) {
                _this.setInterval(value * 1000);
            };
            this.applyServerConfiguration = function () {
                if (VRS.serverConfig) {
                    var config = VRS.serverConfig.get();
                    if (config) {
                        _this._MinimumRefreshInterval = config.MinimumRefreshSeconds * 1000;
                        _this._ServerConfigDefaultRefreshInterval = config.RefreshSeconds * 1000;
                    }
                }
            };
            this.fetch = function () {
                _this._TimeoutHandle = undefined;
                var getFreshList = !_this._Settings.aircraftList.getDataVersion() || _this._RequestFeedId !== _this._LastRequestFeedId;
                _this._LastRequestFeedId = _this._RequestFeedId;
                var params = {};
                if (!getFreshList) {
                    params.ldv = _this._Settings.aircraftList.getDataVersion();
                    params.stm = _this._Settings.aircraftList.getServerTicks();
                }
                if (_this._RequestFeedId !== undefined && _this._RequestFeedId !== null) {
                    params.feed = _this._RequestFeedId;
                }
                if (_this._Settings.currentLocation) {
                    var location = _this._Settings.currentLocation.getCurrentLocation();
                    if (location) {
                        params.lat = location.lat;
                        params.lng = location.lng;
                    }
                }
                if (_this._Settings.aircraftList) {
                    var selectedAircraft = _this._Settings.aircraftList.getSelectedAircraft();
                    if (selectedAircraft)
                        params.selAc = selectedAircraft.id;
                }
                if (_this._HideAircraftNotOnMap && _this._MapPlugin) {
                    var bounds = _this._MapPlugin.getBounds();
                    if (!bounds) {
                        setTimeout($.proxy(_this.fetch, _this), 500);
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
                    postBody.ids = _this._Settings.aircraftList.getAllAircraftIdsHexHyphenString();
                }
                _this._Settings.aircraftList.raiseFetchingList(params, headers, postBody);
                var url = VRS.browserHelper.formUrl(_this._Settings.fetchFsxList ? VRS.globalOptions.aircraftListFlightSimUrl : VRS.globalOptions.aircraftListUrl, params, false);
                $.ajax({
                    url: url,
                    data: postBody,
                    method: 'POST',
                    dataType: VRS.globalOptions.aircraftListDataType,
                    headers: headers,
                    timeout: VRS.globalOptions.aircraftListTimeout,
                    success: $.proxy(_this.fetchSuccess, _this),
                    error: $.proxy(_this.fetchError, _this)
                });
            };
            this.fetchSuccess = function (data) {
                _this._TimeoutHandle = undefined;
                if (!_this._Paused) {
                    _this._Dispatcher.raise(_this._Events.listFetched, [data, _this._Settings.fetchFsxList]);
                    if (data.feeds) {
                        _this._Feeds = data.feeds;
                        _this._ActualFeedId = data.srcFeed;
                    }
                    if (data.configChanged && VRS.serverConfig) {
                        VRS.serverConfig.fetch(function () { });
                    }
                    _this._Settings.aircraftList.applyJson(data, _this);
                    _this._TimeoutHandle = setTimeout($.proxy(_this.fetch, _this), _this._IntervalMilliseconds);
                }
            };
            this.fetchError = function () {
                _this._TimeoutHandle = setTimeout($.proxy(_this.fetch, _this), VRS.globalOptions.aircraftListRetryInterval);
            };
            this.serverConfigChanged = function () {
                _this.applyServerConfiguration();
            };
            this.siteTimedOut = function () {
                _this.setPaused(true);
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
        return AircraftListFetcher;
    }());
    VRS.AircraftListFetcher = AircraftListFetcher;
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraftListFetcher.js.map