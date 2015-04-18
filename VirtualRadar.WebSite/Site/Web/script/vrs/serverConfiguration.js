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
 * @fileoverview Manages fetching and exposing the server's configuration.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.serverConfigUrl = VRS.globalOptions.serverConfigUrl || 'ServerConfig.json';       // The URL to fetch the server configuration from.
    VRS.globalOptions.serverConfigDataType = VRS.globalOptions.serverConfigDataType || 'json';          // The type of call to make when fetching the server configuration.
    VRS.globalOptions.serverConfigTimeout = VRS.globalOptions.serverConfigTimeout || 10000;             // The number of milliseconds to wait before timing out a fetch of server configuration.
    VRS.globalOptions.serverConfigRetryInterval = VRS.globalOptions.serverConfigRetryInterval || 5000;  // The number of milliseconds to wait before retrying a fetch of server configuration.
    VRS.globalOptions.serverConfigOverwrite = VRS.globalOptions.serverConfigOverwrite !== undefined ? VRS.globalOptions.serverConfigOverwrite : false;                              // Whether to overwrite the existing configuration with the configuration stored on the server.
    VRS.globalOptions.serverConfigResetBeforeImport = VRS.globalOptions.serverConfigResetBeforeImport !== undefined ? VRS.globalOptions.serverConfigResetBeforeImport : false;      // Whether to erase the existing configuration before importing the configuration stored on the server.
    VRS.globalOptions.serverConfigIgnoreSplitters = VRS.globalOptions.serverConfigIgnoreSplitters !== undefined ? VRS.globalOptions.serverConfigIgnoreSplitters : false;            // Whether to ignore the splitter settings when importing the configuration stored on the server.
    VRS.globalOptions.serverConfigIgnoreLanguage = VRS.globalOptions.serverConfigIgnoreLanguage !== undefined ? VRS.globalOptions.serverConfigIgnoreLanguage : true;                // Whether to ignore the language settings when importing the configuration stored on the server.
    VRS.globalOptions.serverConfigIgnoreRequestFeedId = VRS.globalOptions.serverConfigIgnoreRequestFeedId !== undefined ? VRS.globalOptions.serverConfigIgnoreRequestFeedId : true; // Whether to ignore the feed ID to fetch when importing the configuration stored on the server.
    //endregion

    //region ServerConfiguration
    VRS.ServerConfiguration = function()
    {
        //region -- Fields
        var that = this;
        //endregion

        //region -- Properties
        /** @type {VRS_SERVER_CONFIG} @private */
        var _ServerConfig = null;

        /**
         * Gets the current configuration on the server.
         * @returns {VRS_SERVER_CONFIG}
         */
        this.get = function() { return _ServerConfig; };

        /**
         * Returns true if the configuration allows for the playing of audio.
         * @returns {boolean}
         */
        this.audioEnabled = function() { return _ServerConfig && !_ServerConfig.IsMono && _ServerConfig.IsAudioEnabled && (_ServerConfig.IsLocalAddress || _ServerConfig.InternetClientsCanPlayAudio); };

        /**
         * Returns true if the configuration allows for the viewing of aircraft pictures.
         * @returns {boolean}
         */
        this.picturesEnabled = function() { return _ServerConfig && (_ServerConfig.IsLocalAddress || _ServerConfig.InternetClientsCanSeeAircraftPictures); };

        /**
         * Returns true if the configuration allows for the display of pin text on aircraft markers.
         * @returns {boolean}
         */
        this.pinTextEnabled = function() { return _ServerConfig && (_ServerConfig.IsLocalAddress || _ServerConfig.InternetClientCanShowPinText); };

        /**
         * Returns true if the configuration allows for the running of reports.
         * @returns {boolean}
         */
        this.reportsEnabled = function() { return _ServerConfig && (_ServerConfig.IsLocalAddress || _ServerConfig.InternetClientCanRunReports); };

        /**
         * Returns true if the configuration allows for the submission of routes.
         * @returns {boolean}
         */
        this.routeSubmissionEnabled = function() { return _ServerConfig && (_ServerConfig.IsLocalAddress || _ServerConfig.InternetClientsCanSubmitRoutes); };

        /**
         * Returns true if the configuration allows for the display of polar plots.
         * @returns {boolean}
         */
        this.polarPlotsEnabled = function() { return _ServerConfig && (_ServerConfig.IsLocalAddress || _ServerConfig.InternetClientsCanSeePolarPlots); };
        //endregion

        //region -- fetch
        /**
         * Fetches the configuration from the server. If this fails then it will retry perpetually. Once it finally
         * receives a reply it calls the callback passed across. The callback is mandatory.
         * @param {function()} successCallback
         */
        this.fetch = function(successCallback)
        {
            if(!successCallback) throw 'You must supply a method to call once the configuration has been fetched';

            $.ajax({
                url:        VRS.globalOptions.serverConfigUrl,
                dataType:   VRS.globalOptions.serverConfigDataType,
                timeout:    VRS.globalOptions.serverConfigTimeout,
                success:    function(data) {
                                _ServerConfig = data;

                                if(_ServerConfig.InitialSettings && VRS.configStorage && !VRS.configStorage.getHasSettings()) {
                                    VRS.configStorage.importSettings(_ServerConfig.InitialSettings, {
                                        overwrite:              VRS.globalOptions.serverConfigOverwrite,
                                        resetBeforeImport:      VRS.globalOptions.serverConfigResetBeforeImport,
                                        ignoreSplitters:        VRS.globalOptions.serverConfigIgnoreSplitters,
                                        ignoreLanguage:         VRS.globalOptions.serverConfigIgnoreLanguage,
                                        ignoreRequestFeedId:    VRS.globalOptions.serverConfigIgnoreRequestFeedId
                                    });
                                }

                                VRS.globalDispatch.raise(VRS.globalEvent.serverConfigChanged, [ _ServerConfig ]);
                                successCallback();
                            },
                failure:    function(jqXHR, textStatus, errorThrown) { fetchFailed(successCallback); }
            });
        };

        /**
         * Called when the fetch fails. Waits for a bit and then tries again.
         * @param {function()} successCallback
         */
        function fetchFailed(successCallback)
        {
            setTimeout(function() { that.fetch(successCallback); }, VRS.globalOptions.serverConfigRetryInterval);
        }
        //endregion
    };
    //endregion

    //region Pre-builts
    VRS.serverConfig = new VRS.ServerConfiguration();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));