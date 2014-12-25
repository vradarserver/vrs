/**
 * @license Copyright © 2014 onwards, Andrew Whewell
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
 * @fileoverview The base for objects that handle basic views.
 */

(function(VRS, $, undefined)
{
    VRS.WebAdmin = VRS.WebAdmin || {};

    /**
     * The base class for all view objects.
     * @param {VRS_WEBADMIN_VIEW_SETTINGS} settings
     * @constructor
     */
    VRS.WebAdmin.View = function(settings)
    {
        //region Default settings
        settings = $.extend({
            refreshPeriod: 1000
        }, settings);
        //endregion

        //region Fields
        var that = this;
        /** @type {number|object} */ var _RefreshTimer = undefined;
        //endregion

        //region initialise
        /**
         * Initialises the page.
         */
        this.initialise = function()
        {
            $(document).on('pageinit', '#' + settings.pageId, function() {
                that.refreshContent();
            });
        };
        //endregion

        //region refreshContent, showContent, startRefreshTimer, cancelRefreshTimer
        /**
         * Fetches new content from the server.
         */
        this.refreshContent = function()
        {
            $.ajax({
                url: settings.jsonUrl,
                timeout: 10000,
                success: contentFetched,
                error: that.startRefreshTimer(10000)
            });
        };

        /**
         * Displays the data fetched from the server.
         * @param {VRS_WEBADMIN_VIEWDATA_BASEVIEW} data
         */
        function contentFetched(data)
        {
            _RefreshTimer = undefined;
            var refreshPeriod = 10000;

            if(data && data.IsRunning) {
                refreshPeriod = settings.refreshPeriod;
                that.showContent(data);
            }

            that.startRefreshTimer(refreshPeriod);
        }

        /**
         * The derivee should override this and populate it with code to fill their object.
         * @param {VRS_WEBADMIN_VIEWDATA_BASEVIEW} data
         */
        that.showContent = function(data)
        {
        };

        /**
         * Pauses for the period passed in (defaults to settings.refreshPeriod) and then fetches
         * new content from the server.
         * @param {number} refreshPeriod
         */
        this.startRefreshTimer = function(refreshPeriod)
        {
            if(!refreshPeriod) refreshPeriod = settings.refreshPeriod;

            that.cancelRefreshTimer();
            _RefreshTimer = setTimeout(that.refreshContent, refreshPeriod);
        };

        /**
         * Cancels the refresh timer, preventing any further updates from the server.
         */
        this.cancelRefreshTimer = function()
        {
            if(_RefreshTimer !== undefined) {
                clearTimeout(_RefreshTimer);
                _RefreshTimer = undefined;
            }
        };
        //endregion
    };
}(window.VRS = window.VRS || {}, jQuery));