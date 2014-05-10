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
 * @fileoverview Handles timing out the site when the user's been inactive for a while.
 */

(function(VRS, $, /** object= */ undefined)
{
    /**
     * The object that manages timing out the site. All it does is raise an event when the site has timed out.
     * @constructor
     */
    VRS.TimeoutManager = function()
    {
        //region -- Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.TimeoutManager'
        });
        var _Events = {
            siteTimedOut:       'timedOut'
        };
        var _TimerInterval = 10000;

        /**
         * The ID of the timer that's going to periodically check whether the site has gone idle.
         * @type {number}
         * @private
         */
        var _Timer = 0;

        /**
         * True if the server configuration indicates that we should be timing out the site.
         * @type {boolean}
         * @private
         */
        var _Enabled = false;

        /**
         * The number of milliseconds of idle time that are allowed to expire before we timeout the site.
         * @type {number}
         * @private
         */
        var _Threshold = 0;

        /**
         * The hook result from the server configuration update event.
         * @type {Object}
         */
        var _ServerConfigurationChangedHookResult;

        /**
         * The date and time of the last activity on the site.
         * @type {Date=}
         */
        var _LastActivity;
        //endregion

        //region Properties
        /**
         * True if the timeout has been exceeded and the site has expired.
         * @type {boolean}
         */
        var _Expired = false;
        this.getExpired = function() { return _Expired; };
        //endregion

        //region Events
        /**
         * Raised when the site has timed out.
         * @param {function()} callback
         * @param {Object} forceThis
         * @returns {Object}
         */
        this.hookSiteTimedOut = function(callback, forceThis) { return _Dispatcher.hook(_Events.siteTimedOut, callback, forceThis); };

        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion

        //region initialise, dispose
        /**
         * Initialises the object. Needs to be called after the server configuration has been loaded.
         */
        this.initialise = function()
        {
            _ServerConfigurationChangedHookResult = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, serverConfigChanged, this);
            loadConfiguration();

            that.resetTimer();
            that.restartTimedOutSite();
        };

        /**
         * Releases any resources held by the object.
         */
        this.dispose = function()
        {
            if(_ServerConfigurationChangedHookResult) VRS.globalDispatch.unhook(_ServerConfigurationChangedHookResult);
            _ServerConfigurationChangedHookResult = null;

            if(_Timer) {
                clearTimeout(_Timer);
                _Timer = 0;
            }
        };
        //endregion

        //region resetTimer, restartTimedOutSite, loadConfiguration
        /**
         * Called by various parts of the system to tell the manager that the site is no longer idle.
         */
        this.resetTimer = function()
        {
            if(!_Expired) _LastActivity = new Date();
        };

        /**
         * Restarts a timed-out site.
         */
        this.restartTimedOutSite = function()
        {
            if(!_Timer) {
                _Expired = false;
                _Timer = setTimeout(timeoutExpired, _TimerInterval);
                that.resetTimer();
            }
        };

        /**
         * Loads the server configuration.
         */
        function loadConfiguration()
        {
            var config = VRS.serverConfig ? VRS.serverConfig.get() : null;
            if(config) {
                _Enabled = !config.IsLocalAddress && config.InternetClientTimeoutMinutes > 0;
                _Threshold = config.InternetClientTimeoutMinutes * 60000;
            }
        }
        //endregion

        //region Events subscribed
        /**
         * Called when the server's configuration has changed.
         */
        function serverConfigChanged()
        {
            loadConfiguration();
        }

        /**
         * Called when the timeout has expired and we need to see if the site has gone idle.
         */
        function timeoutExpired()
        {
            _Timer = 0;

            if(_Enabled && !_Expired) {
                if(_LastActivity) {
                    var msSinceLastActivity = new Date().getTime() - _LastActivity.getTime();
                    _Expired = msSinceLastActivity >= _Threshold;
                    if(_Expired) _Dispatcher.raise(_Events.siteTimedOut);
                }
            }

            if(!_Expired) setTimeout(timeoutExpired, _TimerInterval);
        }
        //endregion
    };

    VRS.timeoutManager = new VRS.TimeoutManager();
})(window.VRS = window.VRS || {}, jQuery);