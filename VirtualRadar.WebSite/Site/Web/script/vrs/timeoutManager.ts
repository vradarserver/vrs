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

namespace VRS
{
    /**
     * The object that manages timing out the site. All it does is raise an event when the site has timed out.
     */
    export class TimeoutManager
    {
        // Events
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.TimeoutManager'
        });
        private _Events = {
            siteTimedOut:       'timedOut'
        };

        /**
         * The interval in milliseconds between checks for user inactivity.
         */
        private _TimerInterval = 10000;

        /**
         * The ID of the timer that's going to periodically check whether the site has gone idle.
         */
        private _Timer = 0;

        /**
         * True if the server configuration indicates that we should be timing out the site.
         */
        private _Enabled = false;

        /**
         * The number of milliseconds of idle time that are allowed to expire before we timeout the site.
         */
        private _Threshold = 0;

        /**
         * The hook result from the server configuration update event.
         */
        private _ServerConfigurationChangedHookResult: IEventHandle;

        /**
         * The date and time of the last activity on the site.
         */
        private _LastActivity: Date;

        /**
         * True if the timeout has been exceeded and the site has expired.
         */
        private _Expired = false;
        getExpired() : boolean
        {
            return this._Expired;
        }

        /**
         * Raised when the site has timed out.
         */
        hookSiteTimedOut(callback: Function, forceThis: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.siteTimedOut, callback, forceThis);
        }

        /**
         * Unhooks an event handler from the object.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Initialises the object. Needs to be called after the server configuration has been loaded.
         */
        initialise()
        {
            this._ServerConfigurationChangedHookResult = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.serverConfigChanged, this);
            this.loadConfiguration();

            this.resetTimer();
            this.restartTimedOutSite();
        }

        /**
         * Releases any resources held by the object.
         */
        dispose()
        {
            if(this._ServerConfigurationChangedHookResult) VRS.globalDispatch.unhook(this._ServerConfigurationChangedHookResult);
            this._ServerConfigurationChangedHookResult = null;

            if(this._Timer) {
                clearTimeout(this._Timer);
                this._Timer = 0;
            }
        }

        /**
         * Called by various parts of the system to tell the manager that the site is no longer idle.
         */
        resetTimer()
        {
            if(!this._Expired) this._LastActivity = new Date();
        }

        /**
         * Restarts a timed-out site.
         */
        restartTimedOutSite()
        {
            if(!this._Timer) {
                this._Expired = false;
                this._Timer = setTimeout($.proxy(this.timeoutExpired, this), this._TimerInterval);
                this.resetTimer();
            }
        }

        /**
         * Loads the server configuration.
         */
        private loadConfiguration()
        {
            var config = VRS.serverConfig ? VRS.serverConfig.get() : null;
            if(config) {
                this._Enabled = !config.IsLocalAddress && config.InternetClientTimeoutMinutes > 0;
                this._Threshold = config.InternetClientTimeoutMinutes * 60000;
            }
        }

        /**
         * Called when the server's configuration has changed.
         */
        private serverConfigChanged()
        {
            this.loadConfiguration();
        }

        /**
         * Called when the timeout has expired and we need to see if the site has gone idle.
         */
        private timeoutExpired()
        {
            this._Timer = 0;

            if(this._Enabled && !this._Expired) {
                if(this._LastActivity) {
                    var msSinceLastActivity = new Date().getTime() - this._LastActivity.getTime();
                    this._Expired = msSinceLastActivity >= this._Threshold;
                    if(this._Expired) this._Dispatcher.raise(this._Events.siteTimedOut);
                }
            }

            if(!this._Expired) {
                this._Timer = setTimeout($.proxy(this.timeoutExpired, this), this._TimerInterval);
            }
        }
    }

    /*
     * Prebuilts
     */
    export var timeoutManager = new VRS.TimeoutManager();
}
