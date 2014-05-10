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
 * @fileoverview Declares everything about the event handling mechanism used in VRS.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global event names
    /**
     * Global event names.
     */
    VRS.globalEvent = $.extend(VRS.globalEvent || {}, {
        /**
         * Raised after a bootstrap object has been created but before it does any work. param1 = the bootstrap object.
         */
        bootstrapCreated: 'bootstrapCreated',

        /**
         * Raised after the display has been updated - typically after the aircraft list has raised its 'updated' event
         * and the handlers have redrawn themselves.
         */
        displayUpdated: 'displayUpdated',

        /**
         * Raised when the server configuration has changed. param1 = the new configuration.
         */
        serverConfigChanged: 'serverConfigChanged'
    });
    //endregion

    //region EventHandler
    /**
     * The event dispatcher object for VRS.
     * @constructor
     * @param {Object}   settings               This is actually optional
     * @param {string}  [settings.name]         The name of the event handler - used for trace messages.
     * @param {boolean} [settings.logLevel]     The level of logging performed. 0 = no logging, 1 = overall time spent in event handlers.
     */
    VRS.EventHandler = function(settings)
    {
        // Expand settings
        settings = $.extend({
            name:       'anon',
            logLevel:   0
        }, settings);

        //region -- Fields
        /**
         * An associative array of event names to event objects.
         * @type {Object.<string, VRS_EVENT_HANDLE[]>}
         * @private
         */
        var _Events = {};
        //endregion

        //region -- hook
        /**
         * Hooks a listener to an event
         * @param {String} eventName    The name of the event to hook.
         * @param {Function} callback   The method to call when then event is raised.
         * @param {object=} forceThis   The object to use as 'this' when calling the callback. If not passed then window is the value of this.
         * @returns {object}            The handle to the hook instance, can be used to unhook the event. Do not make any assumptions about the content of this object.
         */
        this.hook = function(eventName, callback, forceThis)
        {
            if(!_Events[eventName]) _Events[eventName] = [];
            _Events[eventName].push({ callback: callback, forceThis: forceThis || window });

            return /** @type {VRS_EVENT_HANDLE} */ { eventName: eventName, callback: callback };
        };
        //endregion

        //region -- unhook
        /**
         * Unhooks a listener from an event.
         * @param {{}} hookResult       The result of a previous call to hook.
         */
        this.unhook = function(hookResult)
        {
            if(hookResult) {
                var listeners = _Events[hookResult.eventName];
                if(listeners) $.each(listeners, function(idx, handle) {
                    if(handle.callback === hookResult.callback) {
                        _Events[hookResult.eventName].splice(idx, 1);
                        return false;
                    }
                    return true;
                });
            }
        };
        //endregion

        //region -- raise
        /**
         * Calls all listeners to an event.
         * @param {String} eventName    The name of the event to raise.
         * @param [args][]              The arguments to the event, passed as an array. These are expanded out to individual parameters when the callback is called.
         */
        this.raise = function(eventName, args)
        {
            var startTime = settings.logLevel ? new Date() : null;

            var listeners = _Events[eventName];
            if(listeners) {
                var length = listeners.length;
                for(var i = 0;i < length;++i) {
                    var handler = listeners[i];
                    if(handler) handler.callback.apply(handler.forceThis, args || []);      // handler can be undefined if an event handler causes other event handlers to get removed...
                }
            }

            if(startTime) {
                var elapsed = new Date() - startTime;
                console.log('EVT ' + settings.name + '.' + eventName + ' took ' + elapsed + 'ms with ' + (listeners ? listeners.length : 0) + ' listeners');
            }
        };
        //endregion

        //region -- hookJQueryUIPluginEvent, unhookJQueryUIPluginEvent
        /**
         * Hooks a jQuery UI event using a similar mechanism to hook/unhook.
         * @param {jQuery} pluginElement        The jQueryUI element to hook.
         * @param {String} pluginName           The name of the plugin (e.g. vrs.Map would be vrsMap).
         * @param {String} eventName            The name of the jQuery UI event to hook.
         * @param {Function} callback           The function to call when the jQuery event is raised.
         * @param {{}} [forceThis]              The object to use as 'this' when calling the callback.
         * @returns {object}                    A handle describing the hooked function for subsequent unhooking.
         */
        this.hookJQueryUIPluginEvent = function(pluginElement, pluginName, eventName, callback, forceThis)
        {
            var fullEventName = (pluginName + eventName).toLowerCase();
            if(forceThis) callback = $.proxy(callback, forceThis);
            pluginElement.bind(fullEventName, callback);

            return { eventName: fullEventName };
        };

        /**
         * Unhooks a jQueryUI event.
         * @param {jQuery} pluginElement        The jQueryUI element that was hooked.
         * @param {object} hookResult           The result of a previous call to hookJQueryUIPluginEvent.
         */
        this.unhookJQueryUIPluginEvent = function(pluginElement, hookResult)
        {
            if(pluginElement && hookResult && hookResult.eventName) {
                pluginElement.unbind(hookResult.eventName);
            }
        };
        //endregion
    };
    //endregion

    //region Pre-builts
    /**
     * The dispatcher for global events that are not associated with a single object.
     * @type {VRS.EventHandler}
     */
    VRS.globalDispatch = new VRS.EventHandler({
        name: 'GlobalDispatch'
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));