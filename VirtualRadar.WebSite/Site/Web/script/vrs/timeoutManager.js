var VRS;
(function (VRS) {
    var TimeoutManager = (function () {
        function TimeoutManager() {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.TimeoutManager'
            });
            this._Events = {
                siteTimedOut: 'timedOut'
            };
            this._TimerInterval = 10000;
            this._Timer = 0;
            this._Enabled = false;
            this._Threshold = 0;
            this._Expired = false;
        }
        TimeoutManager.prototype.getExpired = function () {
            return this._Expired;
        };
        TimeoutManager.prototype.hookSiteTimedOut = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.siteTimedOut, callback, forceThis);
        };
        TimeoutManager.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        TimeoutManager.prototype.initialise = function () {
            this._ServerConfigurationChangedHookResult = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.serverConfigChanged, this);
            this.loadConfiguration();
            this.resetTimer();
            this.restartTimedOutSite();
        };
        TimeoutManager.prototype.dispose = function () {
            if (this._ServerConfigurationChangedHookResult)
                VRS.globalDispatch.unhook(this._ServerConfigurationChangedHookResult);
            this._ServerConfigurationChangedHookResult = null;
            if (this._Timer) {
                clearTimeout(this._Timer);
                this._Timer = 0;
            }
        };
        TimeoutManager.prototype.resetTimer = function () {
            if (!this._Expired)
                this._LastActivity = new Date();
        };
        TimeoutManager.prototype.restartTimedOutSite = function () {
            if (!this._Timer) {
                this._Expired = false;
                this._Timer = setTimeout($.proxy(this.timeoutExpired, this), this._TimerInterval);
                this.resetTimer();
            }
        };
        TimeoutManager.prototype.loadConfiguration = function () {
            var config = VRS.serverConfig ? VRS.serverConfig.get() : null;
            if (config) {
                this._Enabled = !config.IsLocalAddress && config.InternetClientTimeoutMinutes > 0;
                this._Threshold = config.InternetClientTimeoutMinutes * 60000;
            }
        };
        TimeoutManager.prototype.serverConfigChanged = function () {
            this.loadConfiguration();
        };
        TimeoutManager.prototype.timeoutExpired = function () {
            this._Timer = 0;
            if (this._Enabled && !this._Expired) {
                if (this._LastActivity) {
                    var msSinceLastActivity = new Date().getTime() - this._LastActivity.getTime();
                    this._Expired = msSinceLastActivity >= this._Threshold;
                    if (this._Expired)
                        this._Dispatcher.raise(this._Events.siteTimedOut);
                }
            }
            if (!this._Expired) {
                this._Timer = setTimeout($.proxy(this.timeoutExpired, this), this._TimerInterval);
            }
        };
        return TimeoutManager;
    })();
    VRS.TimeoutManager = TimeoutManager;
    VRS.timeoutManager = new VRS.TimeoutManager();
})(VRS || (VRS = {}));
