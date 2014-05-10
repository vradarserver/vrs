/**
 * @license Copyright © 2013 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview Aircraft list fetching.
 */

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListUrl = VRS.globalOptions.aircraftListUrl || 'AircraftList.json';                   // The URL to fetch aircraft lists from.
    VRS.globalOptions.aircraftListFlightSimUrl = VRS.globalOptions.aircraftListFlightSimUrl || 'FlightSimList.json';// The URL to fetch the flight simulator aircraft list from.
    VRS.globalOptions.aircraftListDataType = VRS.globalOptions.aircraftListDataType || 'json';                      // The data type to use when fetching aircraft lists - use JSONP if cross-domain.
    VRS.globalOptions.aircraftListTimeout = VRS.globalOptions.aircraftListTimeout || 10000;                         // The timeout in milliseconds for a fetch of aircraft.
    VRS.globalOptions.aircraftListRetryInterval = VRS.globalOptions.aircraftListRetryInterval || 15000;             // The number of milliseconds to wait before attempting again after a timeout.
    VRS.globalOptions.aircraftListFixedRefreshInterval = VRS.globalOptions.aircraftListFixedRefreshInterval || -1;  // The number of milliseconds between refreshes, -1 if the user can configure this themselves.
    VRS.globalOptions.aircraftListRequestFeedId = VRS.globalOptions.aircraftListRequestFeedId !== undefined ? VRS.globalOptions.aircraftListRequestFeedId : undefined;              // The receiver feed ID to request updates for.
    VRS.globalOptions.aircraftListUserCanChangeFeeds = VRS.globalOptions.aircraftListUserCanChangeFeeds !== undefined ? VRS.globalOptions.aircraftListUserCanChangeFeeds : true;    // True if the user is allowed to switch feeds, false if they are not.
    VRS.globalOptions.aircraftListHideAircraftNotOnMap = VRS.globalOptions.aircraftListHideAircraftNotOnMap !== undefined ? VRS.globalOptions.aircraftListHideAircraftNotOnMap : false; // True if aircraft that are not on the map are to be hidden from the list.
    //endregion

    //region AircraftListFetcher
    /**
     * An object that can periodically fetch updates for, and apply them to, an aircraft list.
     * @param {Object}                  settings                    The settings object.
     * @param {string=}                 settings.name               The name of the fetcher for state persistence.
     * @param {VRS.AircraftList}        settings.aircraftList       The aircraft list to fetch aircraft for.
     * @param {VRS.CurrentLocation}    [settings.currentLocation]   The object that is holding and managing the browser's current location.
     * @param {jQuery=}                 settings.mapJQ              The map to use when hiding aircraft that aren't visible. If not supplied then the setting has no effect.
     * @param {boolean}                [settings.fetchFsxList]      True if the Flight Simulator list should be fetched, false if the receiver aircraft list should be fetched. Defaults to false.
     * @constructor
     */
    VRS.AircraftListFetcher = function(settings)
    {
        settings = $.extend({
            name:               'default',
            aircraftList:       null,
            currentLocation:    null,
            mapJQ:              null,
            fetchFsxList:       false
        }, settings);

        if(!settings.aircraftList) throw 'The aircraft list to fetch values for must be supplied';

        //region -- Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftListFetcher'
        });
        var _Events = {
            pausedChanged:                  'pausedChanged',
            hideAircraftNotOnMapChanged:    'hideAircraftNotOnMapChanged'
        };

        /**
         * The ID of the feed used in the last request.
         * @type {number=}
         * @private
         */
        var _LastRequestFeedId = undefined;

        /**
         * The handle from the currently running timeout.
         * @type {number=}
         */
        var _TimeoutHandle;

        /**
         * The hook result from the global server configuration changed event.
         * @type {Object}
         */
        var _ServerConfigChangedHook = null;

        /**
         * The hook result from the site time out event.
         * @type {Object}
         * @private
         */
        var _SiteTimedOutHook = null;

        /**
         * The minimum number of milliseconds between refreshes.
         * @type {number}
         * @private
         */
        var _MinimumRefreshInterval = -1;

        /**
         * The default refresh interval (in milliseconds) as configured on the server.
         * @type {number}
         * @private
         */
        var _ServerConfigDefaultRefreshInterval = -1;
        //endregion

        //region -- Properties
        /**
         * Gets the name of the object for the purposes of saving state.
         * @returns {string}
         */
        this.getName = function() { return settings.name; };

        /**
         * An array of feeds as last transmitted by the server.
         * @type {VRS_RECEIVER[]}
         * @private
         */
        var _Feeds = [];
        /**
         * Gets an array of feeds that the server is currently listening to.
         * @returns {VRS_RECEIVER[]}
         */
        this.getFeeds = function() { return _Feeds; };
        /**
         * Gets an array of feeds sorted into alphabetical order.
         * @params {boolean} [includeDefaultFeed]       True if the default feed (undefined ID and a name of Default) should be included in the list. This is always sorted to appear at the start of the list.
         * @returns {Array}
         */
        this.getSortedFeeds = function(includeDefaultFeed) {
            var feeds = _Feeds.slice();
            if(includeDefaultFeed) feeds.push({ name: VRS.$$.DefaultSetting });
            feeds.sort(function(/** VRS_RECEIVER */ lhs, /** VRS_RECEIVER */ rhs) {
                if(lhs.id !== undefined && rhs.id !== undefined)        return lhs.name.localeCompare(rhs.name);
                else if(lhs.id === undefined && rhs.id === undefined)   return 0;
                else if(lhs.id === undefined)                           return -1;
                else                                                    return 1;
            });
            return feeds;
        };
        /**
         * Gets the feed for the ID passed across or null if no such feed exists.
         * @param {number} id
         * @returns {string}
         */
        this.getFeed = function(id)
        {
            var result = null;
            var length = _Feeds.length;
            for(var i = 0;i < length;++i) {
                var feed = _Feeds[i];
                if(feed.id === id) {
                    result = feed;
                    break;
                }
            }

            return result;
        };

        /**
         * The feed to ask the server for. If this is undefined then the default feed is requested.
         * @type {number=}
         * @private
         */
        var _RequestFeedId = VRS.globalOptions.aircraftListRequestFeedId;
        /**
         * Gets the currently requested feed or undefined if the default feed is to be used.
         * @returns {number=}
         */
        this.getRequestFeedId = function() { return _RequestFeedId; };
        /**
         * Sets the feed to ask the server for.
         * @param {number=} value The feed ID or undefined for the default feed.
         */
        this.setRequestFeedId = function(value)
        {
            _RequestFeedId = value === null ? undefined : value;
            if(_RequestFeedId) {
                _RequestFeedId = Number(_RequestFeedId);
                if(isNaN(_RequestFeedId)) _RequestFeedId = undefined;
            }
        };

        /**
         * The ID of the feed actually sent by the server. This will not be set until after the first reply from the
         * server. If _RequestFeedId is null or undefined then this tells us which feed we're actually using. Note that
         * this needn't correspond to a feed in _Feeds - sometimes the feed we're given isn't a real feed (e.g. when
         * we're displaying a Flight Simulator X 'feed').
         * @type {number=}
         * @private
         */
        var _ActualFeedId = undefined;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the ID of the feed last sent by the server.
         * @returns {number=}
         */
        this.getActualFeedId = function() { return _ActualFeedId; };

        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the feed object from _Feeds that we're reporting on, or null if none could be ascertained.
         * @returns {VRS_RECEIVER}
         */
        this.getActualFeed = function()
        {
            var result = null;
            if(_ActualFeedId !== undefined) {
                $.each(_Feeds, function(idx, feed) {
                    if(feed.id === _ActualFeedId) result = feed;
                    return result === null;
                });
            }

            return result;
        };

        var _IntervalMilliseconds = VRS.globalOptions.aircraftListFixedRefreshInterval;
        /**
         * Gets the interval in MS between updates.
         * @returns {number}
         */
        this.getInterval = function() {
            var result = _IntervalMilliseconds;
            if(result === -1) {
                result = _ServerConfigDefaultRefreshInterval;
                if(result === -1) result = 1000;
            }
            if(result < _MinimumRefreshInterval) result = _MinimumRefreshInterval;
            return result;
        };
        /**
         * Sets the interval between updates.
         * @param {number} value Interval in milliseconds.
         */
        this.setInterval = function(value)
        {
            if(_IntervalMilliseconds != value) {
                if(_TimeoutHandle) {
                    clearTimeout(_TimeoutHandle);
                    _TimeoutHandle = undefined;
                }
                _IntervalMilliseconds = value;
                if(!_Paused) fetch();
            }
        };

        var _Paused = true;
        /**
         * Gets a value indicating whether the object is active.
         * @returns {boolean}
         */
        this.getPaused = function() { return _Paused; };
        /**
         * Sets a value indicating whether the object is active.
         * @param {bool} value
         */
        this.setPaused = function(value)
        {
            if(value !== _Paused) {
                _Paused = value;

                if(!_Paused && VRS.timeoutManager) {
                    VRS.timeoutManager.restartTimedOutSite();
                    VRS.timeoutManager.resetTimer();
                }

                if(!_Paused && !_TimeoutHandle) fetch();
                else if(_Paused && _TimeoutHandle) {
                    clearTimeout(_TimeoutHandle);
                    _TimeoutHandle = undefined;
                }
                _Dispatcher.raise(_Events.pausedChanged);
            }
        };

        var _HideAircraftNotOnMap = VRS.globalOptions.aircraftListHideAircraftNotOnMap;
        /**
         * Gets a value indicating that we should ask the server not to send us details of aircraft that aren't visible
         * on the map.
         * @returns {boolean}
         */
        this.getHideAircraftNotOnMap = function() { return _HideAircraftNotOnMap; };
        this.setHideAircraftNotOnMap = function(/** bool */ value)
        {
            if(_HideAircraftNotOnMap !== value) {
                _HideAircraftNotOnMap = value;
                _Dispatcher.raise(_Events.hideAircraftNotOnMapChanged);
            }
        };

        /** @type {VRS.vrsMap} */ var _MapPlugin = null;
        /** @type {jQuery} */     var _MapJQ = null;
        /**
         * Gets the map that the HideAircraftNotOnMap setting will use.
         * @returns {jQuery=}
         */
        this.getMapJQ = function() { return _MapJQ; };
        this.setMapJQ = function(/** jQuery */ value) {
            _MapJQ = value;
            _MapPlugin = _MapJQ ? VRS.jQueryUIHelper.getMapPlugin(_MapJQ) : null;
        };
        this.setMapJQ(settings.mapJQ);
        //endregion

        //region -- Events exposed
        /**
         * Raised whenever the paused state is changed.
         * @param {function()} callback
         * @param {Object} forceThis
         * @returns {Object}
         */
        this.hookPausedChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.pausedChanged, callback, forceThis); };

        /**
         * Raised when the setting to hide aircraft that aren't on the map gets changed.
         * @param {function()} callback
         * @param {Object} forceThis
         * @returns {Object}
         */
        this.hookHideAircraftNotOnMapChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.hideAircraftNotOnMapChanged, callback, forceThis); };

        this.unhook = function(/** Object */ hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- construction
        _ServerConfigChangedHook = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, serverConfigChanged, this);
        _SiteTimedOutHook = VRS.timeoutManager ? VRS.timeoutManager.hookSiteTimedOut(siteTimedOut, this) : null;
        applyServerConfiguration();
        //endregion

        //region -- dispose
        /**
         * Releases resources allocated by the object.
         */
        this.dispose = function()
        {
            if(_ServerConfigChangedHook) {
                VRS.globalDispatch.unhook(_ServerConfigChangedHook);
                _ServerConfigChangedHook = null;
            }
            if(_SiteTimedOutHook) {
                VRS.timeoutManager.unhook(_SiteTimedOutHook);
                _SiteTimedOutHook = null;
            }
        };
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         * @returns {VRS_STATE_AIRCRAFTLISTFETCHER}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            var result = $.extend(createSettings(), savedSettings);

            if(VRS.globalOptions.aircraftListFixedRefreshInterval !== -1) result.interval = VRS.globalOptions.aircraftListFixedRefreshInterval;
            var minimumInterval = Math.max(1000, _MinimumRefreshInterval);
            if(result.interval < minimumInterval) result.interval = minimumInterval;

            return result;
        };

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_AIRCRAFTLISTFETCHER} settings
         */
        this.applyState = function(settings)
        {
            that.setInterval(settings.interval);
            that.setHideAircraftNotOnMap(settings.hideAircraftNotOnMap);
            that.setRequestFeedId(settings.requestFeedId);
        };

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key under which the state will be saved.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsAircraftListFetcher-' + that.getName();
        }

        /**
         * Creates the saved state object.
         * @returns {VRS_STATE_AIRCRAFTLISTFETCHER}
         */
        function createSettings()
        {
            return {
                interval: that.getInterval(),
                requestFeedId: that.getRequestFeedId(),
                hideAircraftNotOnMap: that.getHideAircraftNotOnMap()
            };
        }
        //endregion

        //region -- Configuration - createOptionPane
        /**
         * Creates the description of the options pane for the object.
         * @param {number} displayOrder
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder)
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAircraftListFetcherPane',
                titleKey:       'PaneDataFeed',
                displayOrder:   displayOrder
            });

            if(this.getFeeds().length > 1 && VRS.globalOptions.aircraftListUserCanChangeFeeds && !settings.fetchFsxList) {
                var values = [
                    new VRS.ValueText({ value: undefined, textKey: 'DefaultSetting' })
                ];
                var feeds = this.getSortedFeeds();
                $.each(feeds, function(idx, feed) {
                    values.push(new VRS.ValueText({ value: feed.id, text: feed.name }));
                });

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'feed',
                    labelKey:       'Receiver',
                    getValue:       that.getRequestFeedId,
                    setValue:       that.setRequestFeedId,
                    saveState:      that.saveState,
                    values:         values
                }));
            }

            if(VRS.globalOptions.aircraftListFixedRefreshInterval === -1) {
                pane.addField(new VRS.OptionFieldNumeric({
                    name:           'intervalMilliseconds',
                    labelKey:       'IntervalSeconds',
                    getValue:       getIntervalSeconds,
                    setValue:       setIntervalSeconds,
                    saveState:      that.saveState,
                    inputWidth:     VRS.InputWidth.ThreeChar,
                    min:            Math.floor(_MinimumRefreshInterval / 1000),
                    max:            60,
                    decimals:       0
                }));
            }

            if(!settings.fetchFsxList) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'hideAircraftNotOnMap',
                    labelKey:       'HideAircraftNotOnMap',
                    getValue:       that.getHideAircraftNotOnMap,
                    setValue:       that.setHideAircraftNotOnMap,
                    saveState:      this.saveState
                }));
            }

            return pane;
        };

        /**
         * Gets the refresh interval in seconds.
         * @returns {number}
         */
        function getIntervalSeconds()       { return Math.floor(that.getInterval() / 1000); }

        /**
         * Sets the refresh interval to a number of seconds.
         * @param {number} value
         */
        function setIntervalSeconds(value)  { that.setInterval(value * 1000); }
        //endregion

        //region -- applyServerConfiguration
        function applyServerConfiguration()
        {
            if(VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    _MinimumRefreshInterval = config.MinimumRefreshSeconds * 1000;
                    _ServerConfigDefaultRefreshInterval = config.RefreshSeconds * 1000;
                }
            }
        }
        //endregion

        //region -- fetch
        /**
         * Periodically fetches a new copy of the aircraft list from the server and applies it to the internal aircraft list.
         */
        function fetch()
        {
            _TimeoutHandle = undefined;

            var getFreshList = settings.aircraftList.getDataVersion() === '' || _RequestFeedId !== _LastRequestFeedId;
            _LastRequestFeedId = _RequestFeedId;

            var params = { };
            if(!getFreshList) params.ldv = settings.aircraftList.getDataVersion();
            if(_RequestFeedId !== undefined && _RequestFeedId !== null) params.feed = _RequestFeedId;

            if(settings.currentLocation) {
                var location = settings.currentLocation.getCurrentLocation();
                if(location) {
                    params.lat = location.lat;
                    params.lng = location.lng;
                }
            }

            if(settings.aircraftList) {
                var selectedAircraft = settings.aircraftList.getSelectedAircraft();
                if(selectedAircraft) params.selAc = selectedAircraft.id;
            }

            if(_HideAircraftNotOnMap && _MapPlugin) {
                var bounds = _MapPlugin.getBounds();
                if(!bounds) {
                    // This can happen when the map hasn't finished loading - wait a bit and try again
                    setTimeout(fetch, 500);
                    return;
                }
                if(bounds.tlLat || bounds.tlLng || bounds.brLat || bounds.brLng) {
                    params.fNBnd = bounds.tlLat;
                    params.fEBnd = bounds.brLng;
                    params.fSBnd = bounds.brLat;
                    params.fWBnd = bounds.tlLng;
                }
            }

            var headers = { };
            if(!getFreshList) headers['X-VirtualRadarServer-AircraftIds'] = settings.aircraftList.getAllAircraftIdsString();

            settings.aircraftList.raiseFetchingList(params, headers);

            $.ajax({
                url:      settings.fetchFsxList ? VRS.globalOptions.aircraftListFlightSimUrl : VRS.globalOptions.aircraftListUrl,
                data:     params,
                dataType: VRS.globalOptions.aircraftListDataType,
                headers:  headers,
                timeout:  VRS.globalOptions.aircraftListTimeout,
                success:  fetchSuccess,
                error:    fetchError
            });
        }

        /**
         * Called when the fetch succeeds. Applies the JSON from the server to the aircraft list and then starts another fetch.
         * @param {VRS_JSON_AIRCRAFTLIST} data
         */
        function fetchSuccess(data)
        {
            _TimeoutHandle = undefined;

            if(!_Paused) {
                if(data.feeds) {
                    _Feeds = data.feeds;
                    _ActualFeedId = data.srcFeed;
                }

                if(data.configChanged && VRS.serverConfig) {
                    VRS.serverConfig.fetch(function() {});
                }

                settings.aircraftList.applyJson(data, that);

                if(settings.fetchFsxList) {
                    var fsxAircraft = settings.aircraftList.findAircraftById(1);
                    var selectedAircraft = settings.aircraftList.getSelectedAircraft();
                    var newAircraft = selectedAircraft !== fsxAircraft;
                    settings.aircraftList.setSelectedAircraft(fsxAircraft, false);
                    if(newAircraft && fsxAircraft && fsxAircraft.getPosition() && _MapPlugin) _MapPlugin.panTo(fsxAircraft.getPosition());
                }

                _TimeoutHandle = setTimeout(fetch, _IntervalMilliseconds);
            }
        }

        /**
         * Called when the fetch fails. Starts another fetch but at a longer interval to avoid hammering the server.
         //* @param jqXHR
         //* @param textStatus
         //* @param errorThrown
         */
        function fetchError()
        {
            _TimeoutHandle = setTimeout(fetch, VRS.globalOptions.aircraftListRetryInterval);
        }
        //endregion

        //region -- serverConfigChanged
        /**
         * Called when the server's configuration has changed.
         */
        function serverConfigChanged()
        {
            applyServerConfiguration();
        }

        /**
         * Called when the site has timed out.
         */
        function siteTimedOut()
        {
            that.setPaused(true);
        }
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));