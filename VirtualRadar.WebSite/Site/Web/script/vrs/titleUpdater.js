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
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview Code to manage the updating of a web page's title.
 */

(function(VRS, $, /** object= */ undefined)
{
    /**
     * Handles the updating of the web page's title.
     * @constructor
     */
    VRS.TitleUpdater = function()
    {
        //region Fields
        var _LocaleChangedHookResult = VRS.globalisation.hookLocaleChanged(localeChanged, this);

        /**
         * The function to call when the page title needs to be refreshed.
         * @type {function(bool)}       Passed a bool indicating whether the title should be refreshed even if it doesn't seem to need to be.
         * @private
         */
        var _RefreshFunction = null;

        //region -- showAircraftListCount fields
        /**
         * The aircraft list supplied to showAircraftListCount.
         * @type {VRS.AircraftList}
         * @private
         */
        var _AircraftList = null;
        /**
         * The hook result from the aircraft list updated event.
         * @type {object}
         * @private
         */
        var _AircraftListUpdatedHookResult = null;
        /**
         * The number of aircraft currently showing in the title.
         * @type {number}
         * @private
         */
        var _AircraftListPreviousCount = -1;
        //endregion
        //endregion

        //region dispose
        /**
         * Releases all of the events and resources allocated to the object.
         */
        this.dispose = function()
        {
            if(_LocaleChangedHookResult && VRS.globalisation) {
                VRS.globalisation.unhook(_LocaleChangedHookResult);
                _LocaleChangedHookResult = null;
            }

            if(_AircraftListUpdatedHookResult && _AircraftList) {
                _AircraftList.unhook(_AircraftListUpdatedHookResult);
                _AircraftListUpdatedHookResult = null;
            }
            _AircraftList = null;
        };
        //endregion

        //region showAircraftListCount
        /**
         * Configures the object to automatically refresh the page title to show the number of aircraft being tracked.
         * @param {VRS.AircraftList} aircraftList
         */
        this.showAircraftListCount = function(aircraftList)
        {
            if(!_AircraftList) {
                _AircraftList = aircraftList;
                _AircraftListUpdatedHookResult = _AircraftList.hookUpdated(refreshAircraftCount, this);
                _RefreshFunction = refreshAircraftCount;
                refreshAircraftCount(true);
            }
        };

        /**
         * The refresh function for showAircraftListCount.
         * @param forceRefresh
         */
        function refreshAircraftCount(forceRefresh)
        {
            forceRefresh = !!forceRefresh;
            if(_AircraftList) {
                var count = _AircraftList.getCountTrackedAircraft();
                if(forceRefresh || count !== _AircraftListPreviousCount) {
                    document.title = VRS.$$.VirtualRadar + ' (' + count + ')';
                    _AircraftListPreviousCount = count;
                }
            }
        }
        //endregion

        //region Events subscribed
        /**
         * Called when the user changes the locale.
         */
        function localeChanged()
        {
            if(_RefreshFunction) _RefreshFunction.call(this, true);
        }
        //endregion
    }
}(window.VRS = window.VRS || {}, jQuery));
