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
            refreshPeriod: 1000,
        }, settings);
        //endregion

        //region Fields
        var that = this;
        /** @type {number|object} */                var _RefreshTimer = undefined;
        /** @type {jqXHR} */                        var _RefreshXHR = undefined;
        /** @type {VRS.WebAdmin.SiteNavigation} */  var _SiteNavigation = undefined;
        //endregion

        //region initialise, hookPageEvent, hookPageContainerEvent
        /**
         * Initialises the page.
         */
        this.initialise = function()
        {
            addStandardElements();
            that.addElements();

            $(document).on('ready', function() {
                FastClick.attach(document.body);
                that.hookEvents();
                that.initialiseContent();
                that.refreshContent();
            });
        };
        //endregion

        //region addStandardElements, addElements, hookEvents
        /**
         * Adds elements that are common across all views.
         */
        function addStandardElements()
        {
            _SiteNavigation = new VRS.WebAdmin.SiteNavigation();
            _SiteNavigation.injectIntoPage(settings.pageUrl);
        }

        /**
         * The derivee can override this to add their own dynamic elements to the page on first load.
         */
        that.addElements = function()
        {
        };

        /**
         * The derivee can override this to hook events after the page has loaded.
         */
        that.hookEvents = function()
        {
        };
        //endregion

        //region initialiseContent, refreshContent, performAction, showContent, startRefreshTimer, cancelRefreshTimer
        /**
         * The derivee can override this to perform first-run initialisation of the view.
         */
        this.initialiseContent = function()
        {
        };

        /**
         * Fetches new content from the server.
         */
        this.refreshContent = function()
        {
            that.fetchViewState();
        };

        /**
         * Calls the refresh content JSON entry point but passes an extra parameter, an action. The server should
         * pick this up and perform the action before returning the view state to us.
         * @param {string} action
         * @param {object} [params]
         *
         */
        this.performAction = function(action, params)
        {
            params = $.extend({
                action: action
            }, params || {});
            that.fetchViewState(params);
        };

        this.fetchViewState = function(params)
        {
            that.cancelRefreshTimer();
            that.cancelRefreshContent();
            var ajax = {
                url: settings.jsonUrl,
                timeout: 10000,
                success: contentFetched,
                error: function() { that.startRefreshTimer(10000); },
                data: params
            };
            _RefreshXHR = $.ajax(ajax);
        };

        /**
         * Calls the raiseEvent entry point and asks the view to raise the event named here. The server should return
         * immediately without waiting for the event handlers to finish.
         * @param {string} eventName
         * @param {object} [params]
         */
        this.raiseViewEventInBackground = function(eventName, params)
        {
            params = $.extend({
                eventName: eventName
            }, params || {});

            $.ajax({
                url: settings.jsonUrl,
                timeout: 10000,
                data: params
            });
        };

        /**
         * Cancels any running AJAX call to fetch content.
         */
        this.cancelRefreshContent = function()
        {
            if(_RefreshXHR) {
                _RefreshXHR.abort();
                _RefreshXHR = undefined;
            }
        };

        /**
         * Displays the data fetched from the server.
         * @param {VRS_WEBADMIN_VIEWDATA_BASEVIEW} data
         */
        function contentFetched(data)
        {
            _RefreshTimer = undefined;
            _RefreshXHR = undefined;
            var refreshPeriod = 10000;

            if(data && data.Running) {
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
         * new content from the server. A refreshPeriod of less than 1 disables the refresh timer.
         * @param {number} refreshPeriod
         */
        this.startRefreshTimer = function(refreshPeriod)
        {
            if(isNaN(refreshPeriod)) refreshPeriod = settings.refreshPeriod;

            that.cancelRefreshTimer();
            that.cancelRefreshContent();
            if(refreshPeriod > 0) {
                _RefreshTimer = setTimeout(that.refreshContent, refreshPeriod);
            }
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