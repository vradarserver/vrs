var VRS;
(function (VRS) {
    var AircraftCollection = (function () {
        function AircraftCollection() {
        }
        AircraftCollection.prototype.foreachAircraft = function (callback) {
            for (var id in this) {
                var aircraft = this[id];
                if (aircraft && aircraft instanceof VRS.Aircraft)
                    callback(aircraft);
            }
            return this;
        };
        AircraftCollection.prototype.findAircraftById = function (id) {
            var aircraft = this[id];
            return aircraft && aircraft instanceof VRS.Aircraft ? this[id] : undefined;
        };
        AircraftCollection.prototype.toList = function (filterCallback) {
            var result = [];
            this.foreachAircraft(function (aircraft) {
                if (!filterCallback || filterCallback(aircraft))
                    result.push(aircraft);
            });
            return result;
        };
        return AircraftCollection;
    }());
    VRS.AircraftCollection = AircraftCollection;
    var AircraftList = (function () {
        function AircraftList() {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.AircraftList'
            });
            this._Events = {
                fetchingList: 'fetchingList',
                selectedChanged: 'selectedChanged',
                selectedReselected: 'selectedReselected',
                appliedJson: 'appliedJson',
                updated: 'updated'
            };
            this._Aircraft = new AircraftCollection();
            this._CountTrackedAircraft = 0;
            this._CountAvailableAircraft = 0;
            this._AircraftListSource = VRS.AircraftListSource.Unknown;
            this._ServerHasSilhouettes = false;
            this._ServerHasOperatorFlags = false;
            this._ServerHasPictures = false;
            this._FlagWidth = 85;
            this._FlagHeight = 20;
            this._DataVersion = undefined;
            this._ShortTrailSeconds = 0;
            this._ServerTicks = 0;
            this._WasAircraftSelectedByUser = false;
            this._SelectedAircraft = undefined;
            this.findAircraftById = function (id) {
                return this._Aircraft.findAircraftById(id);
            };
        }
        AircraftList.prototype.getAircraft = function () {
            return this._Aircraft;
        };
        AircraftList.prototype.getCountTrackedAircraft = function () {
            return this._CountTrackedAircraft;
        };
        AircraftList.prototype.getCountAvailableAircraft = function () {
            return this._CountAvailableAircraft;
        };
        AircraftList.prototype.getAircraftListSource = function () {
            return this._AircraftListSource;
        };
        AircraftList.prototype.getServerHasSilhouettes = function () {
            return this._ServerHasSilhouettes;
        };
        AircraftList.prototype.getServerHasOperatorFlags = function () {
            return this._ServerHasOperatorFlags;
        };
        AircraftList.prototype.getServerHasPictures = function () {
            return this._ServerHasPictures;
        };
        AircraftList.prototype.getFlagWidth = function () {
            return this._FlagWidth;
        };
        AircraftList.prototype.getFlagHeight = function () {
            return this._FlagHeight;
        };
        AircraftList.prototype.getDataVersion = function () {
            return this._DataVersion;
        };
        AircraftList.prototype.getShortTrailSeconds = function () {
            return this._ShortTrailSeconds;
        };
        AircraftList.prototype.getServerTicks = function () {
            return this._ServerTicks;
        };
        AircraftList.prototype.getWasAircraftSelectedByUser = function () {
            return this._WasAircraftSelectedByUser;
        };
        AircraftList.prototype.getSelectedAircraft = function () {
            return this._SelectedAircraft;
        };
        AircraftList.prototype.setSelectedAircraft = function (value, wasSelectedByUser) {
            if (value === null)
                value = undefined;
            if (wasSelectedByUser === undefined)
                throw 'You must indicate whether the aircraft was selected by the user';
            if (wasSelectedByUser && VRS.timeoutManager)
                VRS.timeoutManager.resetTimer();
            if (this._SelectedAircraft !== value) {
                var oldSelectedAircraft = this._SelectedAircraft;
                this._SelectedAircraft = value;
                this._WasAircraftSelectedByUser = wasSelectedByUser;
                this._Dispatcher.raise(this._Events.selectedChanged, [oldSelectedAircraft]);
            }
        };
        AircraftList.prototype.hookAppliedJson = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.appliedJson, callback, forceThis);
        };
        AircraftList.prototype.hookUpdated = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.updated, callback, forceThis);
        };
        AircraftList.prototype.hookSelectedAircraftChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.selectedChanged, callback, forceThis);
        };
        AircraftList.prototype.hookSelectedReselected = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.selectedReselected, callback, forceThis);
        };
        AircraftList.prototype.hookFetchingList = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.fetchingList, callback, forceThis);
        };
        AircraftList.prototype.raiseFetchingList = function (xhrParams, xhrHeaders, xhrPostBody) {
            this._Dispatcher.raise(this._Events.fetchingList, [xhrParams, xhrHeaders, xhrPostBody]);
        };
        AircraftList.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        AircraftList.prototype.foreachAircraft = function (callback) {
            return this._Aircraft.foreachAircraft(callback);
        };
        AircraftList.prototype.toList = function (filterCallback) {
            return this._Aircraft.toList(filterCallback);
        };
        AircraftList.prototype.getAllAircraftIdsString = function () {
            var result = '';
            this._Aircraft.foreachAircraft(function (aircraft) {
                if (result)
                    result += ',';
                result += aircraft.id;
            });
            return result;
        };
        AircraftList.prototype.getAllAircraftIcaosString = function () {
            var result = '';
            this._Aircraft.foreachAircraft(function (aircraft) {
                if (result)
                    result += '-';
                result += aircraft.icao.val;
            });
            return result;
        };
        AircraftList.prototype.applyJson = function (aircraftListJson, aircraftListFetcher) {
            if (aircraftListJson) {
                this._CountTrackedAircraft = aircraftListJson.totalAc || 0;
                this._AircraftListSource = aircraftListJson.src || 0;
                this._ServerHasSilhouettes = !!aircraftListJson.showSil;
                this._ServerHasOperatorFlags = !!aircraftListJson.showFlg;
                this._ServerHasPictures = !!aircraftListJson.showPic;
                this._FlagWidth = aircraftListJson.flgW || 0;
                this._FlagHeight = aircraftListJson.flgH || 0;
                this._DataVersion = aircraftListJson.lastDv || -1;
                this._ShortTrailSeconds = aircraftListJson.shtTrlSec || 0;
                this._ServerTicks = aircraftListJson.stm || 0;
                var aircraft = new AircraftCollection();
                var newAircraft = new AircraftCollection();
                var jsonList = aircraftListJson.acList || [];
                var length = jsonList.length;
                var aircraftApplyJsonSettings = {
                    shortTrailTickThreshold: this._ServerTicks === 0 || this._ShortTrailSeconds <= 0 ? -1 : (this._ServerTicks - ((1000 * this._ShortTrailSeconds) + 500)),
                    picturesEnabled: VRS.serverConfig ? VRS.serverConfig.picturesEnabled() : false
                };
                var reselectedAircraft = null;
                for (var i = 0; i < length; ++i) {
                    var aircraftJson = jsonList[i];
                    if (isNaN(aircraftJson.Id))
                        continue;
                    var id = aircraftJson.Id;
                    var existing = this._Aircraft[id];
                    var isNew = !existing;
                    if (!isNew)
                        delete this._Aircraft[id];
                    else {
                        existing = new VRS.Aircraft();
                        newAircraft[id] = existing;
                    }
                    existing.applyJson(aircraftJson, aircraftListFetcher, aircraftApplyJsonSettings);
                    aircraft[id] = existing;
                    if (isNew && this._SelectedAircraft && this._SelectedAircraft.id === id)
                        reselectedAircraft = existing;
                }
                var offRadar = this._Aircraft;
                this._Aircraft = aircraft;
                this._CountAvailableAircraft = length;
                this._Dispatcher.raise(this._Events.appliedJson, [newAircraft, offRadar]);
                if (reselectedAircraft) {
                    this._SelectedAircraft = reselectedAircraft;
                    this._Dispatcher.raise(this._Events.selectedReselected);
                }
                this._Dispatcher.raise(this._Events.updated, [newAircraft, offRadar]);
                VRS.globalDispatch.raise(VRS.globalEvent.displayUpdated);
            }
        };
        return AircraftList;
    }());
    VRS.AircraftList = AircraftList;
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraftList.js.map