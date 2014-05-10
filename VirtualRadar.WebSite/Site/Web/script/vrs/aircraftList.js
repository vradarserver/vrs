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
 * @fileoverview Aircraft list handling.
 */

(function(VRS, $, undefined)
{
    //region AircraftCollection
    /**
     * An associative array of aircraft. The index is their unique ID.
     * @constructor
     */
    VRS.AircraftCollection = function()
    {
        /**
         * Loops through every aircraft in the list passing each in turn to a callback.
         * @param {function(VRS.Aircraft)} callback
         * @returns {VRS.AircraftCollection}
         */
        this.foreachAircraft = function(callback)
        {
            for(var id in this) {
                //noinspection JSUnfilteredForInLoop
                var aircraft = this[id];
                if(aircraft && aircraft instanceof VRS.Aircraft) callback(aircraft);
            }

            return this;
        };

        /**
         * Returns the aircraft with the specified ID or undefined if no such aircraft exists.
         * @param {number} id
         * @returns {VRS.Aircraft|undefined}
         */
        this.findAircraftById = function(id)
        {
            var aircraft = this[id];
            return aircraft && aircraft instanceof VRS.Aircraft ? this[id] : undefined;
        };

        /**
         * Returns the list of aircraft as an unordered array.
         * @param {function(VRS.Aircraft):bool=} filterCallback If supplied then only aircraft for which the callback returns true are included in the list.
         * @returns {VRS.Aircraft[]}
         */
        this.toList = function(filterCallback)
        {
            var result = [];
            this.foreachAircraft(function(aircraft) {
                if(!filterCallback || filterCallback(aircraft)) result.push(aircraft);
            });

            return result;
        };
    };
    //endregion

    //region AircraftList
    /**
     * Records a collection of aircraft being tracked by the server.
     * @constructor
     */
    VRS.AircraftList = function()
    {
        //region -- Fields
        //noinspection JSUnusedLocalSymbols
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftList'
        });
        var _Events = {
            fetchingList:       'fetchingList',
            selectedChanged:    'selectedChanged',
            selectedReselected: 'selectedReselected',
            updated:            'updated'
        };
        //endregion

        //region -- Properties
        var _Aircraft = new VRS.AircraftCollection();
        /**
         * Returns the aircraft being tracked by the object.
         * @returns {VRS.AircraftCollection}
         */
        this.getAircraft = function() { return _Aircraft; };

        var _CountTrackedAircraft = 0;
        /**
         * Gets the number of aircraft currently being tracked by the server. If filtering is in force then this may
         * be larger than the number of aircraft sent in the last update.
         * @returns {number}
         */
        this.getCountTrackedAircraft = function() { return _CountTrackedAircraft; };

        var _CountAvailableAircraft = 0;
        /**
         * Gets the number of aircraft sent in the last update. This may be smaller than the number of tracked aircraft
         * if filtering is in force.
         * @returns {number}
         */
        this.getCountAvailableAircraft = function() { return _CountAvailableAircraft; };

        var _AircraftListSource = VRS.AircraftListSource.Unknown;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the VRS.AircraftListSource value indicating where the list came from.
         * @returns {VRS.AircraftListSource}
         */
        this.getAircraftListSource = function() { return _AircraftListSource; };

        var _ServerHasSilhouettes = false;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets a value indicating whether the server has silhouette images.
         * @returns {boolean}
         */
        this.getServerHasSilhouettes = function() { return _ServerHasSilhouettes; };

        var _ServerHasOperatorFlags = false;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets a value indicating whether the server has operator flag images.
         * @returns {boolean}
         */
        this.getServerHasOperatorFlags = function() { return _ServerHasOperatorFlags; };

        var _ServerHasPictures = false;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets a value indicating whether the server has aircraft pictures.
         * @returns {boolean}
         */
        this.getServerHasPictures = function() { return _ServerHasPictures; };

        var _FlagWidth = 85;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the width of operator flag and silhouette images. No longer used.
         * @returns {number}
         */
        this.getFlagWidth = function() { return _FlagWidth; };

        var _FlagHeight = 20;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the height of operator flag and silhouette images. No longer used.
         * @returns {number}
         */
        this.getFlagHeight = function() { return _FlagHeight; };

        var _DataVersion = '';
        /**
         * Gets the data version number from the last update. This goes up for each update.
         * @returns {string}
         */
        this.getDataVersion = function() { return _DataVersion; };

        var _ShortTrailSeconds = 0;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the length of the short trails in seconds.
         * @returns {number}
         */
        this.getShortTrailSeconds = function() { return _ShortTrailSeconds; };

        var _ServerTicks = 0;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the time at the server of the last update in ticks.
         * @returns {number}
         */
        this.getServerTicks = function() { return _ServerTicks; };

        var _WasAircraftSelectedByUser = false;
        /**
         * Gets a value indicating that the selected aircraft was selected manually by the user - false if it was selected by code.
         * @returns {boolean}
         */
        this.getWasAircraftSelectedByUser = function() { return _WasAircraftSelectedByUser; };

        var _SelectedAircraft = undefined;
        /**
         * Gets the selected aircraft or undefined if no aircraft is selected.
         * @returns {VRS.Aircraft=}
         */
        this.getSelectedAircraft = function() { return _SelectedAircraft; };
        /**
         * Sets the selected aircraft.
         * @param {VRS.Aircraft|undefined|null} value
         * @param {bool} wasSelectedByUser
         */
        this.setSelectedAircraft = function(value, wasSelectedByUser) {
            if(value === null) value = undefined;
            if(wasSelectedByUser === undefined) throw 'You must indicate whether the aircraft was selected by the user';

            if(wasSelectedByUser && VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            if(_SelectedAircraft !== value) {
                var oldSelectedAircraft = _SelectedAircraft;
                _SelectedAircraft = value;
                _WasAircraftSelectedByUser = wasSelectedByUser;
                _Dispatcher.raise(_Events.selectedChanged, [ oldSelectedAircraft ]);
            }
        };
        //endregion

        //region -- Events exposed
        /**
         * Raised after the aircraft list has been updated.
         * @param {function(VRS.AircraftCollection, VRS.AircraftCollection)} callback Passed the aircraft first seen in this update followed by the aircraft that are no longer on radar.
         * @param {object=} forceThis The object to use for 'this' in the call.
         * @returns {object}
         */
        this.hookUpdated = function(callback, forceThis) { return _Dispatcher.hook(_Events.updated, callback, forceThis); };

        /**
         * Raised after the selected aircraft has been changed.
         * @param {function(VRS.Aircraft=)} callback Passed the previously selected aircraft.
         * @param {object=} forceThis The object to use for 'this' in the call.
         * @returns {object}
         */
        this.hookSelectedAircraftChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.selectedChanged, callback, forceThis); };

        /**
         * Raised after the selected aircraft has changed its object. This can happen if the selected aircraft goes off
         * the radar and comes back - when it returns to the radar it is given a new object.
         * @param {function()} callback     The function to call when the select aircraft's object is changed.
         * @param {object=} forceThis       The object to use for 'this' in the call.
         * @returns {object}
         */
        this.hookSelectedReselected = function(callback, forceThis) { return _Dispatcher.hook(_Events.selectedReselected, callback, forceThis); };

        /**
         * Raised by anything that causes an aircraft list to be fetched from the server.
         * @param {function(*, *)} callback Passed an object holding the URL query string parameters and an object holding the HTML headers that are passed to the server when fetching the list.
         * @param {object=} forceThis The object to use for 'this' in the callback.
         * @returns {object}
         */
        this.hookFetchingList = function(callback, forceThis) { return _Dispatcher.hook(_Events.fetchingList, callback, forceThis); };

        /**
         * Raises the FetchingList event.
         * @param {*} xhrParams  The object holding the URL query string parameters to pass to the server.
         * @param {*} xhrHeaders The object holding the HTML headers to pass to the server.
         */
        this.raiseFetchingList = function(xhrParams, xhrHeaders) { _Dispatcher.raise(_Events.fetchingList, [ xhrParams, xhrHeaders ]); };

        /**
         * Unhooks an event that was hooked on the object.
         * @param {object} hookResult
         */
        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- foreachAircraft, findAircraftById, getAllAircraftIdsString
        /**
         * Loops through every aircraft passing each in turn to a callback.
         * @param {function(VRS.Aircraft)} callback
         * @returns {VRS.AircraftCollection}
         */
        this.foreachAircraft = function(callback)
        {
            return _Aircraft.foreachAircraft(callback);
        };

        /**
         * Returns the aircraft as an unordered array.
         * @param {function(VRS.Aircraft):bool} [filterCallback] If supplied then only aircraft for which the callback returns true are returned.
         * @returns {VRS.Aircraft[]}
         */
        this.toList = function(filterCallback)
        {
            return _Aircraft.toList(filterCallback);
        };

        /**
         * Returns the aircraft with the specified ID.
         * @param {number} id The ID to search for. It must be in the same case as originally specified in the JSON (traditionally the ICAO expressed in decimal).
         * @returns {VRS.Aircraft|undefined}
         */
        this.findAircraftById = function(id)
        {
            return _Aircraft.findAircraftById(id);
        };

        /**
         * Returns the ID of every aircraft being tracked as a comma-delimited string.
         * @returns {string}
         */
        this.getAllAircraftIdsString = function()
        {
            var result = '';
            _Aircraft.foreachAircraft(function(aircraft) {
                if(result) result += ',';
                result += aircraft.id;
            });

            return result;
        };
        //endregion

        //region -- applyJson
        /**
         * Applies details about an aircraft's current state to the aircraft list.
         * @param {VRS_JSON_AIRCRAFTLIST} aircraftListJson
         * @param {VRS.AircraftListFetcher} aircraftListFetcher
         */
        this.applyJson = function(aircraftListJson, aircraftListFetcher)
        {
            aircraftListJson = aircraftListJson || {};
            _CountTrackedAircraft =   aircraftListJson.totalAc || 0;
            _AircraftListSource =       /** @type {VRS.AircraftListSource} */ aircraftListJson.src || 0;
            _ServerHasSilhouettes =     !!aircraftListJson.showSil;
            _ServerHasOperatorFlags =   !!aircraftListJson.showFlg;
            _ServerHasPictures =        !!aircraftListJson.showPic;
            _FlagWidth =                aircraftListJson.flgW || 0;
            _FlagHeight =               aircraftListJson.flgH || 0;
            _DataVersion =              aircraftListJson.lastDv || -1;
            _ShortTrailSeconds =        aircraftListJson.shtTrlSec || 0;
            _ServerTicks =              aircraftListJson.stm || 0;

            var aircraft = new VRS.AircraftCollection();
            var newAircraft = new VRS.AircraftCollection();
            var jsonList = aircraftListJson.acList || [];
            var length = jsonList.length;

            var aircraftApplyJsonSettings = {
                shortTrailTickThreshold:    _ServerTicks === 0 || _ShortTrailSeconds <= 0 ? -1 : (_ServerTicks - ((1000 * _ShortTrailSeconds) + 500)),
                picturesEnabled:            VRS.serverConfig ? VRS.serverConfig.picturesEnabled() :  false
            };

            var reselectedAircraft = null;
            for(var i = 0;i < length;++i) {
                var aircraftJson = jsonList[i];
                if(isNaN(aircraftJson.Id)) continue;

                var id = aircraftJson.Id;

                var existing = _Aircraft[id];
                var isNew = !existing;
                if(!isNew) delete _Aircraft[id];
                else {
                    existing = new VRS.Aircraft();
                    newAircraft[id] = existing;
                }

                existing.applyJson(aircraftJson, aircraftListFetcher, aircraftApplyJsonSettings);
                aircraft[id] = existing;

                if(isNew && _SelectedAircraft && _SelectedAircraft.id === id) reselectedAircraft = existing;
            }

            var offRadar = _Aircraft;
            _Aircraft = aircraft;
            _CountAvailableAircraft = length;

            if(reselectedAircraft) {
                _SelectedAircraft = reselectedAircraft;
                _Dispatcher.raise(_Events.selectedReselected);
            }

            _Dispatcher.raise(_Events.updated, [ newAircraft, offRadar ]);

            VRS.globalDispatch.raise(VRS.globalEvent.displayUpdated);
        };
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));