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
 * @fileoverview Code to load script at run-time.
 */

namespace VRS
{
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.scriptManagerTimeout = VRS.globalOptions.scriptManagerTimeout || 30000;           // The timeout in milliseconds on script loads.

    /**
     * Holds unique identifiers for all of the scripts known to the VRS.ScriptManager class.
     */
    export var scriptKey = {
        GoogleMaps: 'googleMap'
    };

    /**
     * The definition of the options object that can be passed to ScriptManager.loadScript
     */
    export interface LoadScript_Options
    {
        key?:       string;
        url?:       string;
        params?:    any;
        queue?:     boolean;
        success?:   () => void;
        error?:     (xhr?: JQueryXHR, status?: string, error?: string) => void;
        timeout?:   number;
    }

    /**
     * The class that manages the loading of remote scripts at run-time.
     */
    export class ScriptManager
    {
        /**
         * An associative array of loaded scripts indexed by a unique key, with the value being true if the script is loading / has loaded or false if it has not.
         */
        private _LoadedScripts: { [scriptKey: string]: boolean } = {};

        /**
         * The queue of scripts to load in sequence.
         * @type {VRS_LOADSCRIPT_OPTIONS[]}
         * @private
         */
        private _Queue: LoadScript_Options[] = [];

        /**
         * Loads a script if it has not already been loaded.
         */
        loadScript(options: LoadScript_Options)
        {
            options = $.extend(<LoadScript_Options>{
                key:        null,
                params:     {},
                queue:      false,
                success:    $.noop,
                error:      null,
                timeout:    VRS.globalOptions.scriptManagerTimeout
            }, options);

            if(!options.queue) {
                this.doLoadScript(options);
            } else {
                this._Queue.push(options);
                if(this._Queue.length === 1) this.doProcessQueue();
            }
        }

        /**
         * Processes outstanding queue entries.
         */
        private doProcessQueue()
        {
            var self = this;
            if(this._Queue.length) {
                var queueEntry = this._Queue[0];
                this.doLoadScript(queueEntry, function() {
                    self._Queue.splice(0, 1);
                    self.doProcessQueue();
                });
            }
        }

        /**
         * Loads a script.
         */
        private doLoadScript(options: LoadScript_Options, onCompletion?: () => void)
        {
            var self = this;
            var callSuccess = function() {
                options.success();
                if(onCompletion) onCompletion();
            };

            if(options.key && this._LoadedScripts[options.key]) {
                callSuccess();
            } else {
                if(options.key !== VRS.scriptKey.GoogleMaps) {
                    $.ajax({
                        url: options.url,
                        data: options.params,
                        success: function() {
                            if(options.key) self._LoadedScripts[options.key] = true;
                            callSuccess();
                        },
                        error: function(jqXHR, textStatus, errorThrown) {
                            if(!options.error) throw 'Could not load the script "' + (options.key || '') + '" from "' + options.url + '". Status: ' + (textStatus || '') + '. Error: ' + (errorThrown || '');
                            options.error(jqXHR, textStatus, errorThrown);
                            if(onCompletion) onCompletion();
                        },
                        dataType: 'script',
                        timeout: VRS.globalOptions.scriptManagerTimeout
                    });
                } else {
                    var callbackName = 'googleMapCallback_' + $.now();
                    options.params = $.extend({}, options.params, { callback: callbackName });

                    var timeoutId = setTimeout(function() {
                        timeoutId = 0;
                        VRS.pageHelper.showMessageBox('Google Maps', 'Could not load Google Maps within ' + options.timeout + 'ms');
                        if(options.error) {
                            options.error(null, 'Timed out', 'Timed out');
                            if(onCompletion) onCompletion();
                        }
                    }, options.timeout);

                    window[callbackName] = function() {
                        if(timeoutId) clearTimeout(timeoutId);
                        callSuccess();
                    };

                    var fullUrl = options.url + '?' + $.param(options.params);
                    var script = $('<script/>')
                        .attr('type', 'text/javascript')
                        .attr('src', fullUrl);
                    $(document).find('head').last().append(script);
                }
            }
        }
    }

    /**
     * Pre-builts
     */
    export var scriptManager = new VRS.ScriptManager();
} 