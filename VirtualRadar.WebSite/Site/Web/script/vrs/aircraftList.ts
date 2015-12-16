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

namespace VRS
{
    /**
     * An associative array of aircraft. The index is their unique ID number.
     */
    export class AircraftCollection
    {
        /**
         * Loops through every aircraft in the list passing each in turn to a callback.
         */
        foreachAircraft(callback: (aircraft: Aircraft) => void) : AircraftCollection
        {
            for(var id in this) {
                var aircraft = this[id];
                if(aircraft && aircraft instanceof VRS.Aircraft) callback(aircraft);
            }

            return this;
        }

        /**
         * Returns the aircraft with the specified ID or undefined if no such aircraft exists.
         */
        findAircraftById(id: number) : Aircraft
        {
            var aircraft = this[id];
            return aircraft && aircraft instanceof VRS.Aircraft ? this[id] : undefined;
        }

        /**
         * Returns the list of aircraft as an unordered array.
         */
        toList(filterCallback: (aircraft:Aircraft) => boolean) : Aircraft[]
        {
            var result: Aircraft[] = [];
            this.foreachAircraft(function(aircraft) {
                if(!filterCallback || filterCallback(aircraft)) result.push(aircraft);
            });

            return result;
        }
    }

    /**
     * A collection of aircraft being tracked by the server.
     */
    export class AircraftList
    {
        // Events
        private _Dispatcher = new EventHandler({
            name: 'VRS.AircraftList'
        });
        private _Events = {
            fetchingList:       'fetchingList',
            selectedChanged:    'selectedChanged',
            selectedReselected: 'selectedReselected',
            updated:            'updated'
        };

        private _Aircraft = new AircraftCollection();
        /**
         * Returns the aircraft being tracked by the object.
         */
        getAircraft() : AircraftCollection
        {
            return this._Aircraft;
        }

        private _CountTrackedAircraft = 0;
        /**
         * Gets the number of aircraft currently being tracked by the server. If filtering is in force then this may
         * be larger than the number of aircraft sent in the last update.
         */
        getCountTrackedAircraft() : number
        {
            return this._CountTrackedAircraft;
        }

        private _CountAvailableAircraft = 0;
        /**
         * Gets the number of aircraft sent in the last update. This may be smaller than the number of tracked aircraft
         * if filtering is in force.
         */
        getCountAvailableAircraft() : number
        {
            return this._CountAvailableAircraft;
        }

        private _AircraftListSource = VRS.AircraftListSource.Unknown;
        /**
         * Gets the VRS.AircraftListSource value indicating where the list came from.
         */
        getAircraftListSource() : AircraftListSourceEnum
        {
            return this._AircraftListSource;
        }

        private _ServerHasSilhouettes = false;
        /**
         * Gets a value indicating whether the server has silhouette images.
         */
        getServerHasSilhouettes() : boolean
        {
            return this._ServerHasSilhouettes;
        }

        private _ServerHasOperatorFlags = false;
        /**
         * Gets a value indicating whether the server has operator flag images.
         */
        getServerHasOperatorFlags()
        {
            return this._ServerHasOperatorFlags;
        }

        private _ServerHasPictures = false;
        /**
         * Gets a value indicating whether the server has aircraft pictures.
         */
        getServerHasPictures()
        {
            return this._ServerHasPictures;
        }

        private _FlagWidth = 85;
        /**
         * Gets the width of operator flag and silhouette images. No longer used.
         */
        getFlagWidth()
        {
            return this._FlagWidth;
        }

        private _FlagHeight = 20;
        /**
         * Gets the height of operator flag and silhouette images. No longer used.
         */
        getFlagHeight() : number
        {
            return this._FlagHeight;
        }

        private _DataVersion: number = undefined;
        /**
         * Gets the data version number from the last update. This goes up for each update.
         */
        getDataVersion() : number
        {
            return this._DataVersion;
        }

        private _ShortTrailSeconds = 0;
        /**
         * Gets the length of the short trails in seconds.
         */
        getShortTrailSeconds() : number
        {
            return this._ShortTrailSeconds;
        }

        private _ServerTicks = 0;
        /**
         * Gets the time at the server of the last update in ticks.
         */
        getServerTicks() : number
        {
            return this._ServerTicks;
        }

        private _WasAircraftSelectedByUser = false;
        /**
         * Gets a value indicating that the selected aircraft was selected manually by the user - false if it was selected by code.
         */
        getWasAircraftSelectedByUser() : boolean
        {
            return this._WasAircraftSelectedByUser;
        }

        private _SelectedAircraft: Aircraft = undefined;
        /**
         * Gets the selected aircraft or undefined if no aircraft is selected.
         */
        getSelectedAircraft() : Aircraft
        {
            return this._SelectedAircraft;
        }
        /**
         * Sets the selected aircraft.
         */
        setSelectedAircraft(value: Aircraft, wasSelectedByUser: boolean)
        {
            if(value === null) value = undefined;
            if(wasSelectedByUser === undefined) throw 'You must indicate whether the aircraft was selected by the user';

            if(wasSelectedByUser && VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            if(this._SelectedAircraft !== value) {
                var oldSelectedAircraft = this._SelectedAircraft;
                this._SelectedAircraft = value;
                this._WasAircraftSelectedByUser = wasSelectedByUser;
                this._Dispatcher.raise(this._Events.selectedChanged, [ oldSelectedAircraft ]);
            }
        }

        /**
         * Raised after the aircraft list has been updated.
         */
        hookUpdated(callback: (newAircraft: AircraftCollection, offRadar: AircraftCollection) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.updated, callback, forceThis);
        }

        /**
         * Raised after the selected aircraft has been changed.
         * @param {function(VRS.Aircraft=)} callback Passed the previously selected aircraft.
         * @param {object=} forceThis The object to use for 'this' in the call.
         * @returns {object}
         */
        hookSelectedAircraftChanged(callback: (wasSelected: Aircraft) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.selectedChanged, callback, forceThis);
        }

        /**
         * Raised after the selected aircraft has changed its object. This can happen if the selected aircraft goes off
         * the radar and comes back - when it returns to the radar it is given a new object.
         */
        hookSelectedReselected(callback: () => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.selectedReselected, callback, forceThis);
        }

        /**
         * Raised by anything that causes an aircraft list to be fetched from the server.
         */
        hookFetchingList(callback: (parameters: Object, headers: Object, postBody: Object) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.fetchingList, callback, forceThis);
        }

        /**
         * Raises the FetchingList event.
         * @param {*} xhrParams     The object holding the URL query string parameters to pass to the server.
         * @param {*} xhrHeaders    The object holding the HTML headers to pass to the server.
         * @param {*} xhrPostBody   The object holding the post body to pass to the server.
         */
        raiseFetchingList(xhrParams: Object, xhrHeaders: Object, xhrPostBody: Object)
        {
            this._Dispatcher.raise(this._Events.fetchingList, [ xhrParams, xhrHeaders, xhrPostBody ]);
        }

        /**
         * Unhooks an event that was hooked on the object.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Loops through every aircraft passing each in turn to a callback.
         * @param {function(VRS.Aircraft)} callback
         * @returns {VRS.AircraftCollection}
         */
        foreachAircraft(callback: (aircraft:Aircraft) => void) : AircraftCollection
        {
            return this._Aircraft.foreachAircraft(callback);
        }

        /**
         * Returns the aircraft as an unordered array.
         */
        toList(filterCallback: (aircraft: Aircraft) => boolean) : Aircraft[]
        {
            return this._Aircraft.toList(filterCallback);
        }

        /**
         * Returns the aircraft with the specified ID.
         */
        findAircraftById = function(id: number) : Aircraft
        {
            return this._Aircraft.findAircraftById(id);
        }

        /**
         * Returns the ID of every aircraft being tracked as a comma-delimited string.
         */
        getAllAircraftIdsString() : string
        {
            var result = '';
            this._Aircraft.foreachAircraft(function(aircraft) {
                if(result) result += ',';
                result += aircraft.id;
            });

            return result;
        }

        /**
         * Returns the ICAO of every aircraft being tracked as a hyphen-delimited string.
         */
        getAllAircraftIcaosString() : string
        {
            var result = '';
            this._Aircraft.foreachAircraft(function(aircraft) {
                if(result) result += '-';
                result += aircraft.icao.val;
            });

            return result;
        }

        /**
         * Applies details about an aircraft's current state to the aircraft list.
         */
        applyJson(aircraftListJson: IAircraftList, aircraftListFetcher: AircraftListFetcher)
        {
            if(aircraftListJson) {
                this._CountTrackedAircraft =    aircraftListJson.totalAc || 0;
                this._AircraftListSource =      aircraftListJson.src || 0;
                this._ServerHasSilhouettes =    !!aircraftListJson.showSil;
                this._ServerHasOperatorFlags =  !!aircraftListJson.showFlg;
                this._ServerHasPictures =       !!aircraftListJson.showPic;
                this._FlagWidth =               aircraftListJson.flgW || 0;
                this._FlagHeight =              aircraftListJson.flgH || 0;
                this._DataVersion =             aircraftListJson.lastDv || -1;
                this._ShortTrailSeconds =       aircraftListJson.shtTrlSec || 0;
                this._ServerTicks =             aircraftListJson.stm || 0;

                var aircraft = new AircraftCollection();
                var newAircraft = new AircraftCollection();
                var jsonList = aircraftListJson.acList || [];
                var length = jsonList.length;

                var aircraftApplyJsonSettings = {
                    shortTrailTickThreshold:    this._ServerTicks === 0 || this._ShortTrailSeconds <= 0 ? -1 : (this._ServerTicks - ((1000 * this._ShortTrailSeconds) + 500)),
                    picturesEnabled:            VRS.serverConfig ? VRS.serverConfig.picturesEnabled() :  false
                };

                var reselectedAircraft = null;
                for(var i = 0;i < length;++i) {
                    var aircraftJson = jsonList[i];
                    if(isNaN(aircraftJson.Id)) continue;

                    var id = aircraftJson.Id;

                    var existing = this._Aircraft[id];
                    var isNew = !existing;
                    if(!isNew) delete this._Aircraft[id];
                    else {
                        existing = new VRS.Aircraft();
                        newAircraft[id] = existing;
                    }

                    existing.applyJson(aircraftJson, aircraftListFetcher, aircraftApplyJsonSettings);
                    aircraft[id] = existing;

                    if(isNew && this._SelectedAircraft && this._SelectedAircraft.id === id) reselectedAircraft = existing;
                }

                var offRadar = this._Aircraft;
                this._Aircraft = aircraft;
                this._CountAvailableAircraft = length;

                if(reselectedAircraft) {
                    this._SelectedAircraft = reselectedAircraft;
                    this._Dispatcher.raise(this._Events.selectedReselected);
                }

                this._Dispatcher.raise(this._Events.updated, [ newAircraft, offRadar ]);

                VRS.globalDispatch.raise(VRS.globalEvent.displayUpdated);
            }
        }
    }
}
 