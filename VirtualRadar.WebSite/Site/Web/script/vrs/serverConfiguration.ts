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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.serverConfigUrl = VRS.globalOptions.serverConfigUrl || 'ServerConfig.json';       // The URL to fetch the server configuration from.
    VRS.globalOptions.serverConfigDataType = VRS.globalOptions.serverConfigDataType || 'json';          // The type of call to make when fetching the server configuration.
    VRS.globalOptions.serverConfigTimeout = VRS.globalOptions.serverConfigTimeout || 10000;             // The number of milliseconds to wait before timing out a fetch of server configuration.
    VRS.globalOptions.serverConfigRetryInterval = VRS.globalOptions.serverConfigRetryInterval || 5000;  // The number of milliseconds to wait before retrying a fetch of server configuration.
    VRS.globalOptions.serverConfigOverwrite = VRS.globalOptions.serverConfigOverwrite !== undefined ? VRS.globalOptions.serverConfigOverwrite : false;                              // Whether to overwrite the existing configuration with the configuration stored on the server.
    VRS.globalOptions.serverConfigResetBeforeImport = VRS.globalOptions.serverConfigResetBeforeImport !== undefined ? VRS.globalOptions.serverConfigResetBeforeImport : false;      // Whether to erase the existing configuration before importing the configuration stored on the server.
    VRS.globalOptions.serverConfigIgnoreSplitters = VRS.globalOptions.serverConfigIgnoreSplitters !== undefined ? VRS.globalOptions.serverConfigIgnoreSplitters : false;            // Whether to ignore the splitter settings when importing the configuration stored on the server.
    VRS.globalOptions.serverConfigIgnoreLanguage = VRS.globalOptions.serverConfigIgnoreLanguage !== undefined ? VRS.globalOptions.serverConfigIgnoreLanguage : true;                // Whether to ignore the language settings when importing the configuration stored on the server.
    VRS.globalOptions.serverConfigIgnoreRequestFeedId = VRS.globalOptions.serverConfigIgnoreRequestFeedId !== undefined ? VRS.globalOptions.serverConfigIgnoreRequestFeedId : true; // Whether to ignore the feed ID to fetch when importing the configuration stored on the server.

    export interface IServerConfig
    {
        VrsVersion:                             string;
        IsMono:                                 boolean;
        UseMarkerLabels:                        boolean;
        Receivers:                              IServerConfigReceiver[];
        IsLocalAddress:                         boolean;
        IsAudioEnabled:                         boolean;
        MinimumRefreshSeconds:                  number;
        RefreshSeconds:                         number;
        GoogleMapsApiKey:                       string;
        InitialSettings:                        string;
        InitialLatitude:                        number;
        InitialLongitude:                       number;
        InitialMapType:                         string;    // VRS.MapType
        InitialZoom:                            number;
        InitialDistanceUnit:                    string;    // VRS.Distance
        InitialHeightUnit:                      string;    // VRS.Height
        InitialSpeedUnit:                       string;    // VRS.Speed
        InternetClientCanRunReports:            boolean;
        InternetClientCanShowPinText:           boolean;
        InternetClientTimeoutMinutes:           number;
        InternetClientsCanPlayAudio:            boolean;
        InternetClientsCanSubmitRoutes:         boolean;
        InternetClientsCanSeeAircraftPictures:  boolean;
        InternetClientsCanSeePolarPlots:        boolean;
        OpenStreetMapTileServerUrl:             string;
    }

    export interface IServerConfigReceiver
    {
        UniqueId: number;
        Name:     string;
    }

    export class ServerConfiguration
    {
        private _ServerConfig: IServerConfig = null;

        /**
         * Gets the current configuration on the server.
         */
        get() : IServerConfig
        {
            return this._ServerConfig;
        }

        /**
         * Returns true if the configuration allows for the playing of audio.
         */
        audioEnabled() : boolean
        {
            return this._ServerConfig &&
                   !this._ServerConfig.IsMono &&
                   this._ServerConfig.IsAudioEnabled &&
                   (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanPlayAudio);
        }

        /**
         * Returns true if the configuration allows for the viewing of aircraft pictures.
         */
        picturesEnabled() : boolean
        {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanSeeAircraftPictures);
        }

        /**
         * Returns true if the configuration allows for the display of pin text on aircraft markers.
         */
        pinTextEnabled() : boolean
        {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientCanShowPinText);
        }

        /**
         * Returns true if the configuration allows for the running of reports.
         */
        reportsEnabled() : boolean
        {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientCanRunReports);
        }

        /**
         * Returns true if the configuration allows for the submission of routes.
         */
        routeSubmissionEnabled() : boolean
        {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanSubmitRoutes);
        }

        /**
         * Returns true if the configuration allows for the display of polar plots.
         */
        polarPlotsEnabled() : boolean
        {
            return this._ServerConfig && (this._ServerConfig.IsLocalAddress || this._ServerConfig.InternetClientsCanSeePolarPlots);
        }

        /**
         * Fetches the configuration from the server. If this fails then it will retry perpetually. Once it finally
         * receives a reply it calls the callback passed across. The callback is mandatory.
         */
        fetch(successCallback: () => void)
        {
            var self = this;
            if(!successCallback) throw 'You must supply a method to call once the configuration has been fetched';

            $.ajax({
                url:        VRS.globalOptions.serverConfigUrl,
                dataType:   VRS.globalOptions.serverConfigDataType,
                timeout:    VRS.globalOptions.serverConfigTimeout,
                success:    function(data) {
                                self._ServerConfig = data;

                                if(self._ServerConfig.InitialSettings && VRS.configStorage && !VRS.configStorage.getHasSettings()) {
                                    VRS.configStorage.importSettings(self._ServerConfig.InitialSettings, {
                                        overwrite:              VRS.globalOptions.serverConfigOverwrite,
                                        resetBeforeImport:      VRS.globalOptions.serverConfigResetBeforeImport,
                                        ignoreSplitters:        VRS.globalOptions.serverConfigIgnoreSplitters,
                                        ignoreLanguage:         VRS.globalOptions.serverConfigIgnoreLanguage,
                                        ignoreRequestFeedId:    VRS.globalOptions.serverConfigIgnoreRequestFeedId
                                    });
                                }

                                VRS.globalDispatch.raise(VRS.globalEvent.serverConfigChanged, [ self._ServerConfig ]);
                                successCallback();
                            },
                error:      function(jqXHR, textStatus, errorThrown) { self.fetchFailed(successCallback); }
            });
        }

        /**
         * Called when the fetch fails. Waits for a bit and then tries again.
         */
        private fetchFailed(successCallback: () => void)
        {
            var self = this;
            setTimeout(function() {
                self.fetch(successCallback);
            }, VRS.globalOptions.serverConfigRetryInterval);
        }
    }

    /*
     * Prebuilts
     */
    export var serverConfig = new VRS.ServerConfiguration();
}
