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
 * @fileoverview A jQuery UI plugin that displays a message box when the site times out.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region TimeoutMessageBoxPluginState
    VRS.TimeoutMessageBoxPluginState = function()
    {
        /**
         * The hook result for the site timed-out event.
         * @type {Object}
         */
        this.SiteTimedOutHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionDialog attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionDialog}
     */
    VRS.jQueryUIHelper.getTimeoutMessageBox = function(jQueryElement) { return jQueryElement.data('vrsVrsTimeoutMessageBox'); };
    //endregion

    //region vrsTimeoutMessageBox
    $.widget('vrs.vrsTimeoutMessageBox', {
        //region -- options
        options:
        {
            /** @type {VRS.AircraftListFetcher} */  aircraftListFetcher:    null,               // The aircraft list fetcher that pauses when the site times out.

            __nop: null
        },
        //endregion

        //region -- _getState, _create
        /**
         * Returns the state associated with the plugin, creating it if it doesn't already exist.
         * @returns {VRS.TimeoutMessageBoxPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('vrsTimeoutMessageBoxState');
            if(result === undefined) {
                result = new VRS.TimeoutMessageBoxPluginState();
                this.element.data('vrsTimeoutMessageBoxState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the plugin.
         * @private
         */
        _create: function()
        {
            if(!this.options.aircraftListFetcher)   throw 'An aircraft list must be supplied';

            var state = this._getState();
            state.SiteTimedOutHookResult = VRS.timeoutManager.hookSiteTimedOut(this._siteTimedOut, this);
        },

        /**
         * Releases all resources allocated to the plugin.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            if(state.SiteTimedOutHookResult) {
                VRS.timeoutManager.unhook(state.SiteTimedOutHookResult);
                state.SiteTimedOutHookResult = null;
            }
        },
        //endregion

        //region Events subscribed
        /**
         * Called when the site has timed out.
         * @private
         */
        _siteTimedOut: function()
        {
            var options = this.options;

            var dialog = $('<div/>')
                .appendTo('body');
            $('<p/>')
                .text(VRS.$$.SiteTimedOut)
                .appendTo(dialog);

            dialog.dialog({
                modal: true,
                title: VRS.$$.TitleSiteTimedOut,
                close: function() {
                    options.aircraftListFetcher.setPaused(false);
                    dialog.dialog('destroy');
                    dialog.remove();
                }
            });
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
