var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.serverConfigUrl = VRS.globalOptions.serverConfigUrl || 'ServerConfig.json';
    VRS.globalOptions.serverConfigDataType = VRS.globalOptions.serverConfigDataType || 'json';
    VRS.globalOptions.serverConfigTimeout = VRS.globalOptions.serverConfigTimeout || 10000;
    VRS.globalOptions.serverConfigRetryInterval = VRS.globalOptions.serverConfigRetryInterval || 5000;
    VRS.globalOptions.serverConfigOverwrite = VRS.globalOptions.serverConfigOverwrite !== undefined ? VRS.globalOptions.serverConfigOverwrite : false;
    VRS.globalOptions.serverConfigResetBeforeImport = VRS.globalOptions.serverConfigResetBeforeImport !== undefined ? VRS.globalOptions.serverConfigResetBeforeImport : false;
    VRS.globalOptions.serverConfigIgnoreSplitters = VRS.globalOptions.serverConfigIgnoreSplitters !== undefined ? VRS.globalOptions.serverConfigIgnoreSplitters : false;
    VRS.globalOptions.serverConfigIgnoreLanguage = VRS.globalOptions.serverConfigIgnoreLanguage !== undefined ? VRS.globalOptions.serverConfigIgnoreLanguage : true;
    VRS.globalOptions.serverConfigIgnoreRequestFeedId = VRS.globalOptions.serverConfigIgnoreRequestFeedId !== undefined ? VRS.globalOptions.serverConfigIgnoreRequestFeedId : true;
    var ServerConfiguration = (function () {
        function ServerConfiguration() {
            this._ServerConfig = null;
        }
        ServerConfiguration.prototype.get = function () {
            return this._ServerConfig;
        };
        ServerConfiguration.prototype.audioEnabled = function () {
            return this._ServerConfig &&
                !this._ServerConfig.IsMono &&
                this._ServerConfig.IsAudioEnabled &&
                (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanPlayAudio);
        };
        ServerConfiguration.prototype.picturesEnabled = function () {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanSeeAircraftPictures);
        };
        ServerConfiguration.prototype.pinTextEnabled = function () {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientCanShowPinText);
        };
        ServerConfiguration.prototype.reportsEnabled = function () {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientCanRunReports);
        };
        ServerConfiguration.prototype.routeSubmissionEnabled = function () {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanSubmitRoutes);
        };
        ServerConfiguration.prototype.polarPlotsEnabled = function () {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanSeePolarPlots);
        };
        ServerConfiguration.prototype.fetch = function (successCallback) {
            var self = this;
            if (!successCallback)
                throw 'You must supply a method to call once the configuration has been fetched';
            $.ajax({
                url: VRS.globalOptions.serverConfigUrl,
                dataType: VRS.globalOptions.serverConfigDataType,
                timeout: VRS.globalOptions.serverConfigTimeout,
                success: function (data) {
                    self._ServerConfig = data;
                    if (self._ServerConfig.InitialSettings && VRS.configStorage && !VRS.configStorage.getHasSettings()) {
                        VRS.configStorage.importSettings(self._ServerConfig.InitialSettings, {
                            overwrite: VRS.globalOptions.serverConfigOverwrite,
                            resetBeforeImport: VRS.globalOptions.serverConfigResetBeforeImport,
                            ignoreSplitters: VRS.globalOptions.serverConfigIgnoreSplitters,
                            ignoreLanguage: VRS.globalOptions.serverConfigIgnoreLanguage,
                            ignoreRequestFeedId: VRS.globalOptions.serverConfigIgnoreRequestFeedId
                        });
                    }
                    VRS.globalDispatch.raise(VRS.globalEvent.serverConfigChanged, [self._ServerConfig]);
                    successCallback();
                },
                error: function (jqXHR, textStatus, errorThrown) { self.fetchFailed(successCallback); }
            });
        };
        ServerConfiguration.prototype.fetchFailed = function (successCallback) {
            var self = this;
            setTimeout(function () {
                self.fetch(successCallback);
            }, VRS.globalOptions.serverConfigRetryInterval);
        };
        return ServerConfiguration;
    })();
    VRS.ServerConfiguration = ServerConfiguration;
    VRS.serverConfig = new VRS.ServerConfiguration();
})(VRS || (VRS = {}));
//# sourceMappingURL=serverConfiguration.js.map