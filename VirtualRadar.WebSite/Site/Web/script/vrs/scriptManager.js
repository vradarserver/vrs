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

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.scriptManagerTimeout = VRS.globalOptions.scriptManagerTimeout || 30000;           // The timeout in milliseconds on script loads.
    //endregion

    //region scriptKey
    /**
     * Holds unique identifiers for all of the scripts known to the VRS.ScriptManager class.
     * @enum {string}
     */
    VRS.scriptKey = {
        GoogleMaps: 'googleMap'
    };
    //endregion

    //region ScriptManager
    /**
     * The class that manages the loading of remote scripts at run-time.
     * @constructor
     */
    VRS.ScriptManager = function()
    {
        //region -- Fields
        /**
         * An associative array of loaded scripts indexed by a unique key, with the value being true if the script is loading / has loaded or false if it has not.
         * @type {Object.<string, bool>}
         * @private
         */
        var _LoadedScripts = {};

        /**
         * The queue of scripts to load in sequence.
         * @type {VRS_LOADSCRIPT_OPTIONS[]}
         * @private
         */
        var _Queue = [];
        //endregion

        //region -- loadScript
        /**
         * Loads a script if it has not already been loaded.
         * @param {Object}                                      options
         * @param {string}                                     [options.key]        The key associated with the URL. Used to control duplicate loads - pass null if duplicate load protection not required.
         * @param {string}                                      options.url         The URL of the script to load.
         * @param {Object}                                     [options.params]     The parameters to pass with the url.
         * @param {bool}                                       [options.async]      True if the script is to be loaded asynchronously.
         * @param {bool}                                       [options.queue]      True if the script is to loaded in a queue. Queued loads are always performed in order.
         * @param {function}                                   [options.success]    The function to call after the script has loaded (or to be called immediately if it has already been loaded).
         * @param {function(jQuery.jqXHR, string, string)=}    [options.error]      The function to call if the script cannot be loaded.
         */
        this.loadScript = function(/** VRS_LOADSCRIPT_OPTIONS */ options)
        {
            /** @type {VRS_LOADSCRIPT_OPTIONS} */
            options = $.extend({
                key:        null,
                params:     {},
                async:      false,
                queue:      false,
                success:    $.noop,
                error:      null,
                timeout:    VRS.globalOptions.scriptManagerTimeout
            }, options);

            if(!options.queue) {
                doLoadScript(options);
            } else {
                _Queue.push(options);
                if(_Queue.length === 1) doProcessQueue();
            }
        };

        /**
         * Processes outstanding queue entries.
         */
        function doProcessQueue()
        {
            if(_Queue.length) {
                var queueEntry = _Queue[0];
                doLoadScript(queueEntry, function() {
                    _Queue.splice(0, 1);
                    doProcessQueue();
                });
            }
        }

        /**
         * Loads a script.
         * @param {VRS_LOADSCRIPT_OPTIONS}  options         The options to use when loading the script.
         * @param {function()}             [onCompletion]   A method to call once the script has been successfully called.
         */
        function doLoadScript(options, onCompletion)
        {
            var callSuccess = function() {
                options.success();
                if(onCompletion) onCompletion();
            };

            if(options.key && _LoadedScripts[options.key]) {
                callSuccess();
            } else {
                if(options.key !== VRS.scriptKey.GoogleMaps) {
                    $.ajax({
                        url: options.url,
                        data: options.params,
                        success: function() {
                            if(options.key) _LoadedScripts[options.key] = true;
                            callSuccess();
                        },
                        error: function(jqXHR, textStatus, errorThrown) {
                            if(!options.error) throw 'Could not load the script "' + (options.key || '') + '" from "' + options.url + '". Status: ' + (textStatus || '') + '. Error: ' + (errorThrown || '');
                            options.error(jqXHR, textStatus, errorThrown);
                            if(onCompletion) onCompletion();
                        },
                        async: options.async,
                        dataType: 'script',
                        timeout: VRS.globalOptions.scriptManagerTimeout
                    });
                } else {
                    if(!options.async) throw 'Cannot load Google Maps synchronously';

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
        //endregion
    };
    //endregion

    //region Pre-builts
    VRS.scriptManager = new VRS.ScriptManager();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));