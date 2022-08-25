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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListUrl = VRS.globalOptions.aircraftListUrl || 'AircraftList.json';                   // The URL to fetch aircraft lists from.
    VRS.globalOptions.aircraftListFlightSimUrl = VRS.globalOptions.aircraftListFlightSimUrl || 'FlightSimList.json';// The URL to fetch the flight simulator aircraft list from.
    VRS.globalOptions.aircraftListDataType = VRS.globalOptions.aircraftListDataType || 'json';                      // The data type to use when fetching aircraft lists - use JSONP if cross-domain.
    VRS.globalOptions.aircraftListTimeout = VRS.globalOptions.aircraftListTimeout || 10000;                         // The timeout in milliseconds for a fetch of aircraft.
    VRS.globalOptions.aircraftListRetryInterval = VRS.globalOptions.aircraftListRetryInterval || 15000;             // The number of milliseconds to wait before attempting again after a timeout.
    VRS.globalOptions.aircraftListFixedRefreshInterval = VRS.globalOptions.aircraftListFixedRefreshInterval || -1;  // The number of milliseconds between refreshes, -1 if the user can configure this themselves.
    VRS.globalOptions.aircraftListRequestFeedId = VRS.globalOptions.aircraftListRequestFeedId !== undefined ? VRS.globalOptions.aircraftListRequestFeedId : undefined;              // The receiver feed ID to request updates for.
    VRS.globalOptions.aircraftListUserCanChangeFeeds = VRS.globalOptions.aircraftListUserCanChangeFeeds !== undefined ? VRS.globalOptions.aircraftListUserCanChangeFeeds : true;    // True if the user is allowed to switch feeds, false if they are not.
    VRS.globalOptions.aircraftListHideAircraftNotOnMap = VRS.globalOptions.aircraftListHideAircraftNotOnMap !== undefined ? VRS.globalOptions.aircraftListHideAircraftNotOnMap : false; // True if aircraft that are not on the map are to be hidden from the list.

    /**
     * The settings that can be passed to the ctor for AircraftListFetcher
     */
    export interface AircraftListFetcher_Settings
    {
        aircraftList:       AircraftList;       // The aircraft list to fetch aircraft for.
        name?:              string;             // The name of the fetcher for state persistence.
        currentLocation?:   CurrentLocation;    // The object that is holding and managing the browser's current location.
        mapJQ?:             JQuery;             // The map to use when hiding aircraft that aren't visible. If not supplied then the setting has no effect.
        fetchFsxList?:      boolean;            // True if the Flight Simulator list should be fetched, false if the receiver aircraft list should be fetched. Defaults to false.
    }

    export interface AircraftListFetcher_SaveState
    {
        interval:               number;
        requestFeedId:          number;
        hideAircraftNotOnMap:   boolean;
    }

    /**
     * An object that can periodically fetch updates for, and apply them to, an aircraft list.
     */
    export class AircraftListFetcher implements ISelfPersist<AircraftListFetcher_SaveState>
    {
        private _Dispatcher: EventHandler = new EventHandler({
            name: 'VRS.AircraftListFetcher'
        });
        private _Events = {
            pausedChanged:                  'pausedChanged',
            hideAircraftNotOnMapChanged:    'hideAircraftNotOnMapChanged',
            listFetched:                    'listFetched'
        };
        private _Settings: AircraftListFetcher_Settings;

        private _LastRequestFeedId: number;                         // The ID of the feed used in the last request.
        private _TimeoutHandle: number;                             // The handle from the currently running timeout.
        private _ServerConfigChangedHook: IEventHandle = null;      // The hook result from the global server configuration changed event.
        private _SiteTimedOutHook: IEventHandle = null;             // The hook result from the site time out event.
        private _MinimumRefreshInterval: number = -1;               // The minimum number of milliseconds between refreshes.
        private _ServerConfigDefaultRefreshInterval: number = -1;   // The default refresh interval (in milliseconds) as configured on the server.
        private _Feeds: IReceiver[] = [];
        private _RequestFeedId: number = VRS.globalOptions.aircraftListRequestFeedId;
        private _IntervalMilliseconds: number = VRS.globalOptions.aircraftListFixedRefreshInterval;
        private _Paused = true;
        private _HideAircraftNotOnMap: boolean = VRS.globalOptions.aircraftListHideAircraftNotOnMap;
        private _MapPlugin: IMap = null;
        private _MapJQ: JQuery = null;

        /*
         * The ID of the feed actually sent by the server. This will not be set until after the first reply from the
         * server. If _RequestFeedId is null or undefined then this tells us which feed we're actually using. Note that
         * this needn't correspond to a feed in _Feeds - sometimes the feed we're given isn't a real feed (e.g. when
         * we're displaying a Flight Simulator X 'feed').
         */
        private _ActualFeedId: number = undefined;

        constructor(settings: AircraftListFetcher_Settings)
        {
            if(!settings.aircraftList) throw 'The aircraft list to fetch values for must be supplied';
            this._Settings = $.extend({
                name:               'default',
                aircraftList:       null,
                currentLocation:    null,
                mapJQ:              null,
                fetchFsxList:       false
            }, settings);
            this.setMapJQ(settings.mapJQ);

            this._ServerConfigChangedHook = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.serverConfigChanged, this);
            this._SiteTimedOutHook = VRS.timeoutManager ? VRS.timeoutManager.hookSiteTimedOut(this.siteTimedOut, this) : null;
            this.applyServerConfiguration();
        }

        /**
         * Gets the name of the object for the purposes of saving state.
         */
        getName = () =>
        {
            return this._Settings.name;
        }

        /**
         * Gets an array of feeds that the server is currently listening to.
         */
        getFeeds = () : IReceiver[] =>
        {
            return this._Feeds;
        }

        /**
         * Gets a copy of the array of feeds, sorted into alphabetical order.
         * @params {boolean} [includeDefaultFeed]       True if the default feed (undefined ID and a name of Default) should be included in the list. This is always sorted to appear at the start of the list.
         */
        getSortedFeeds = (includeDefaultFeed: boolean = false) : IReceiver[] =>
        {
            var feeds = this._Feeds.slice();
            if(includeDefaultFeed) feeds.push({ name: VRS.$$.DefaultSetting });
            feeds.sort(function(lhs, rhs) {
                if(lhs.id !== undefined && rhs.id !== undefined)        return lhs.name.localeCompare(rhs.name);
                else if(lhs.id === undefined && rhs.id === undefined)   return 0;
                else if(lhs.id === undefined)                           return -1;
                else                                                    return 1;
            });
            return feeds;
        }

        /**
         * Gets the feed for the ID passed across or null if no such feed exists.
         */
        getFeed = (id: number) : IReceiver =>
        {
            var result = null;
            var length = this._Feeds.length;
            for(var i = 0;i < length;++i) {
                var feed = this._Feeds[i];
                if(feed.id === id) {
                    result = feed;
                    break;
                }
            }

            return result;
        }

        /**
         * Gets the feed to ask the server for. Undefined if the default feed is to be used.
         */
        getRequestFeedId = () : number =>
        {
            return this._RequestFeedId;
        }

        /**
         * Sets the feed to ask the server for.
         */
        setRequestFeedId = (value?: number) =>
        {
            this._RequestFeedId = value === null ? undefined : value;
            if(this._RequestFeedId) {
                this._RequestFeedId = Number(this._RequestFeedId);
                if(isNaN(this._RequestFeedId)) this._RequestFeedId = undefined;
            }
        }

        /**
         * Gets the ID of the feed last sent by the server.
         */
        getActualFeedId = () =>
        {
            return this._ActualFeedId;
        }

        /**
         * Gets the feed object from _Feeds that we're reporting on, or null if none could be ascertained.
         */
        getActualFeed = () : IReceiver =>
        {
            var result: IReceiver = null;

            var feedId = this._ActualFeedId;
            if(feedId !== undefined) {
                $.each(this._Feeds, function(idx, feed) {
                    if(feed.id === feedId) result = feed;
                    return result === null;
                });
            }

            return result;
        }

        /**
         * Gets the interval in milliseconds between updates.
         */
        getInterval = () : number =>
        {
            var result = this._IntervalMilliseconds;
            if(result === -1) {
                result = this._ServerConfigDefaultRefreshInterval;
                if(result === -1) result = 1000;
            }
            if(result < this._MinimumRefreshInterval) result = this._MinimumRefreshInterval;
            return result;
        }

        /**
         * Sets the interval between updates.
         */
        setInterval = (value: number) =>
        {
            if(this._IntervalMilliseconds != value) {
                if(this._TimeoutHandle) {
                    clearTimeout(this._TimeoutHandle);
                    this._TimeoutHandle = undefined;
                }
                this._IntervalMilliseconds = value;
                if(!this._Paused) this.fetch();
            }
        }

        /**
         * Gets a value indicating whether the object is active.
         */
        getPaused = () : boolean =>
        {
            return this._Paused;
        }

        /**
         * Sets a value indicating whether the object is active.
         */
        setPaused = (value: boolean) =>
        {
            if(value !== this._Paused) {
                this._Paused = value;

                if(!this._Paused && VRS.timeoutManager) {
                    VRS.timeoutManager.restartTimedOutSite();
                    VRS.timeoutManager.resetTimer();
                }

                if(!this._Paused && !this._TimeoutHandle) this.fetch();
                else if(this._Paused && this._TimeoutHandle) {
                    clearTimeout(this._TimeoutHandle);
                    this._TimeoutHandle = undefined;
                }
                this._Dispatcher.raise(this._Events.pausedChanged);
            }
        }

        /**
         * Gets a value indicating that we should ask the server not to send us details of aircraft that aren't visible
         * on the map.
         */
        getHideAircraftNotOnMap = () : boolean =>
        {
            return this._HideAircraftNotOnMap;
        }
        setHideAircraftNotOnMap = (value: boolean) =>
        {
            if(this._HideAircraftNotOnMap !== value) {
                this._HideAircraftNotOnMap = value;
                this._Dispatcher.raise(this._Events.hideAircraftNotOnMapChanged);
            }
        }

        /**
         * Gets the map that the HideAircraftNotOnMap setting will use.
         */
        getMapJQ = () : JQuery =>
        {
            return this._MapJQ;
        }
        setMapJQ = (value: JQuery) =>
        {
            this._MapJQ = value;
            this._MapPlugin = this._MapJQ ? VRS.jQueryUIHelper.getMapPlugin(this._MapJQ) : null;
        };

        /**
         * Raised whenever the paused state is changed.
         */
        hookPausedChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.pausedChanged, callback, forceThis);
        }

        /**
         * Raised when the setting to hide aircraft that aren't on the map gets changed.
         */
        hookHideAircraftNotOnMapChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.hideAircraftNotOnMapChanged, callback, forceThis);
        }

        /**
         * Raised directly after the aircraft list has been fetched but before anything is done with it.
         * Gets passed two parameters - the aircraft list that was fetched and a bool that is true if the
         * fetch was for FSX data.
         */
        hookListFetched = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.listFetched, callback, forceThis);
        }

        /**
         * Unhooks an event handler.
         */
        unhook = (hookResult: IEventHandle) =>
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Releases resources allocated by the object.
         */
        dispose = () =>
        {
            if(this._ServerConfigChangedHook) {
                VRS.globalDispatch.unhook(this._ServerConfigChangedHook);
                this._ServerConfigChangedHook = null;
            }
            if(this._SiteTimedOutHook) {
                VRS.timeoutManager.unhook(this._SiteTimedOutHook);
                this._SiteTimedOutHook = null;
            }
        }

        /**
         * Saves the current state of the object.
         */
        saveState = () =>
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         */
        loadState = () : AircraftListFetcher_SaveState =>
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);

            if(VRS.globalOptions.aircraftListFixedRefreshInterval !== -1) result.interval = VRS.globalOptions.aircraftListFixedRefreshInterval;
            var minimumInterval = Math.max(1000, this._MinimumRefreshInterval);
            if(result.interval < minimumInterval) result.interval = minimumInterval;

            return result;
        }

        /**
         * Applies a previously saved state to the object.
         */
        applyState = (settings: AircraftListFetcher_SaveState) =>
        {
            this.setInterval(settings.interval);
            this.setHideAircraftNotOnMap(settings.hideAircraftNotOnMap);
            this.setRequestFeedId(settings.requestFeedId);
        }

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        loadAndApplyState = () : void =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the state will be saved.
         */
        private persistenceKey = () : string =>
        {
            return 'vrsAircraftListFetcher-' + this.getName();
        }

        /**
         * Creates the saved state object.
         */
        private createSettings = () : AircraftListFetcher_SaveState =>
        {
            return {
                interval:               this.getInterval(),
                requestFeedId:          this.getRequestFeedId(),
                hideAircraftNotOnMap:   this.getHideAircraftNotOnMap()
            };
        }

        /**
         * Creates the description of the options pane for the object.
         */
        createOptionPane = (displayOrder: number) : OptionPane =>
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAircraftListFetcherPane',
                titleKey:       'PaneDataFeed',
                displayOrder:   displayOrder
            });

            if(this.getFeeds().length > 1 && VRS.globalOptions.aircraftListUserCanChangeFeeds && !this._Settings.fetchFsxList) {
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
                    getValue:       this.getRequestFeedId,
                    setValue:       this.setRequestFeedId,
                    saveState:      this.saveState,
                    values:         values
                }));
            }

            if(VRS.globalOptions.aircraftListFixedRefreshInterval === -1) {
                pane.addField(new VRS.OptionFieldNumeric({
                    name:           'intervalMilliseconds',
                    labelKey:       'IntervalSeconds',
                    getValue:       this.getIntervalSeconds,
                    setValue:       this.setIntervalSeconds,
                    saveState:      this.saveState,
                    inputWidth:     VRS.InputWidth.ThreeChar,
                    min:            Math.floor(this._MinimumRefreshInterval / 1000),
                    max:            60,
                    decimals:       0
                }));
            }

            if(!this._Settings.fetchFsxList) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'hideAircraftNotOnMap',
                    labelKey:       'HideAircraftNotOnMap',
                    getValue:       this.getHideAircraftNotOnMap,
                    setValue:       this.setHideAircraftNotOnMap,
                    saveState:      this.saveState
                }));
            }

            return pane;
        }

        private getIntervalSeconds = () : number =>
        {
            return Math.floor(this.getInterval() / 1000);
        }

        private setIntervalSeconds = (value: number) =>
        {
            this.setInterval(value * 1000);
        }

        private applyServerConfiguration = () =>
        {
            if(VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    this._MinimumRefreshInterval = config.MinimumRefreshSeconds * 1000;
                    this._ServerConfigDefaultRefreshInterval = config.RefreshSeconds * 1000;
                }
            }
        }

        /**
         * Periodically fetches a new copy of the aircraft list from the server and applies it to the internal aircraft list.
         */
        private fetch = () =>
        {
            this._TimeoutHandle = undefined;

            var getFreshList = !this._Settings.aircraftList.getDataVersion() || this._RequestFeedId !== this._LastRequestFeedId;
            this._LastRequestFeedId = this._RequestFeedId;

            var params: IAircraftListRequestQueryString = { };
            if(!getFreshList) {
                params.ldv = this._Settings.aircraftList.getDataVersion();
                params.stm = this._Settings.aircraftList.getServerTicks();
            }
            if(this._RequestFeedId !== undefined && this._RequestFeedId !== null) {
                params.feed = this._RequestFeedId;
            }

            if(this._Settings.currentLocation) {
                var location = this._Settings.currentLocation.getCurrentLocation();
                if(location) {
                    params.lat = location.lat;
                    params.lng = location.lng;
                }
            }

            if(this._Settings.aircraftList) {
                var selectedAircraft = this._Settings.aircraftList.getSelectedAircraft();
                if(selectedAircraft) params.selAc = selectedAircraft.id;
            }

            if(this._HideAircraftNotOnMap && this._MapPlugin) {
                var bounds = this._MapPlugin.getBounds();
                if(!bounds) {
                    // This can happen when the map hasn't finished loading - wait a bit and try again
                    setTimeout($.proxy(this.fetch, this), 500);
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

            var postBody: IAircraftListRequestBody = { };
            if(!getFreshList) {
                postBody.ids = this._Settings.aircraftList.getAllAircraftIdsHexHyphenString();
            }

            this._Settings.aircraftList.raiseFetchingList(params, headers, postBody);

            var url = VRS.browserHelper.formUrl(
                this._Settings.fetchFsxList ? VRS.globalOptions.aircraftListFlightSimUrl : VRS.globalOptions.aircraftListUrl,
                params,
                false
            );
            $.ajax({
                url:      url,
                data:     postBody,
                method:   'POST',
                dataType: VRS.globalOptions.aircraftListDataType,
                headers:  headers,
                timeout:  VRS.globalOptions.aircraftListTimeout,
                success:  $.proxy(this.fetchSuccess, this),
                error:    $.proxy(this.fetchError, this)
            });
        }

        /**
         * Called when the fetch succeeds. Applies the JSON from the server to the aircraft list and then starts another fetch.
         */
        private fetchSuccess = (data) =>
        {
            this._TimeoutHandle = undefined;

            if(!this._Paused) {
                this._Dispatcher.raise(this._Events.listFetched, [ data, this._Settings.fetchFsxList ]);

                if(data.feeds) {
                    this._Feeds = data.feeds;
                    this._ActualFeedId = data.srcFeed;
                }

                if(data.configChanged && VRS.serverConfig) {
                    VRS.serverConfig.fetch(function() {});
                }

                this._Settings.aircraftList.applyJson(data, this);

                this._TimeoutHandle = setTimeout($.proxy(this.fetch, this), this._IntervalMilliseconds);
            }
        }

        /**
         * Called when the fetch fails. Starts another fetch but at a longer interval to avoid hammering the server.
         */
        private fetchError = () =>
        {
            this._TimeoutHandle = setTimeout($.proxy(this.fetch, this), VRS.globalOptions.aircraftListRetryInterval);
        }

        /**
         * Called when the server's configuration has changed.
         */
        private serverConfigChanged = () =>
        {
            this.applyServerConfiguration();
        }

        /**
         * Called when the site has timed out.
         */
        private siteTimedOut = () =>
        {
            this.setPaused(true);
        }
    }
}
