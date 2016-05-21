var VRS;
(function (VRS) {
    var TitleUpdater = (function () {
        function TitleUpdater() {
            this._AircraftList = null;
            this._AircraftListUpdatedHookResult = null;
            this._AircraftListPreviousCount = -1;
            this._LocaleChangedHookResult = VRS.globalisation.hookLocaleChanged(this.localeChanged, this);
        }
        TitleUpdater.prototype.dispose = function () {
            if (this._LocaleChangedHookResult && VRS.globalisation) {
                VRS.globalisation.unhook(this._LocaleChangedHookResult);
                this._LocaleChangedHookResult = null;
            }
            if (this._AircraftListUpdatedHookResult && this._AircraftList) {
                this._AircraftList.unhook(this._AircraftListUpdatedHookResult);
                this._AircraftListUpdatedHookResult = null;
            }
            this._AircraftList = null;
        };
        TitleUpdater.prototype.showAircraftListCount = function (aircraftList) {
            if (!this._AircraftList) {
                this._AircraftList = aircraftList;
                this._AircraftListUpdatedHookResult = this._AircraftList.hookUpdated(this.aircraftListUpdated, this);
                this.refreshAircraftCount(true);
            }
        };
        TitleUpdater.prototype.refreshAircraftCount = function (forceRefresh) {
            if (this._AircraftList) {
                var count = this._AircraftList.getCountTrackedAircraft();
                if (forceRefresh || count !== this._AircraftListPreviousCount) {
                    document.title = VRS.$$.VirtualRadar + ' (' + count + ')';
                    this._AircraftListPreviousCount = count;
                }
            }
        };
        TitleUpdater.prototype.aircraftListUpdated = function () {
            this.refreshAircraftCount(false);
        };
        TitleUpdater.prototype.localeChanged = function () {
            this.refreshAircraftCount(true);
        };
        return TitleUpdater;
    })();
    VRS.TitleUpdater = TitleUpdater;
})(VRS || (VRS = {}));
