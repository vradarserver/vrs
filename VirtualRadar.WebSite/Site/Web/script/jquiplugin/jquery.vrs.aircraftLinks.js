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
 * @fileoverview A jQueryUI plugin that can display a list of links for an aircraft.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    //endregion

    //region VRS.AircraftLinksPluginState
    /**
     * The object that carries state for the aircraft links plugin.
     * @constructor
     */
    VRS.AircraftLinksPluginState = function()
    {
        /**
         * The aircraft that the plugin is displaying links for.
         * @type {VRS.Aircraft=}
         */
        this.aircraft = undefined;

        /**
         * An array of jQuery elements for each link rendered.
         * @type {Array.<jQuery>}
         */
        this.linkElements = [];

        /**
         * An array of bools indicating that the corresponding linkElement item is visible.
         * @type {Array.<boolean>}
         */
        this.linkVisible = [];

        /**
         * An array of elements that contain separators between the links.
         * @type {Array.<jQuery>}
         */
        this.separatorElements = [];
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    /**
     * Returns the aircraft link plugin attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsAircraftLinks}
     */
    VRS.jQueryUIHelper.getAircraftLinksPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsAircraftLinks'); };

    /**
     * Returns a set of default options for the aircraft links widget, with optional overrides.
     * @param {VRS_OPTIONS_AIRCRAFTLINKS=} overrides
     * @returns {VRS_OPTIONS_AIRCRAFTLINKS}
     */
    VRS.jQueryUIHelper.getAircraftLinksOptions = function(overrides)
    {
        return $.extend({
            linkSites:  null,               // An array of the links to display. If null or undefined then all of the default links for an aircraft are displayed.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsAircraftLinks
    /**
     * A jQuery widget that can display links for an aircraft
     * @namespace VRS.vrsAircraftLinks
     */
    $.widget('vrs.vrsAircraftLinks', {
        //region -- options
        /** @type {VRS_OPTIONS_AIRCRAFTLINKS} */
        options: VRS.jQueryUIHelper.getAircraftLinksOptions(),
        //endregion

        //region -- _getState, _create etc.
        /**
         * Returns the state object for the plugin, creating it if it's not already there.
         * @returns {VRS.AircraftLinksPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('aircraftLinksPluginState');
            if(result === undefined) {
                result = new VRS.AircraftLinksPluginState();
                this.element.data('aircraftLinksPluginState', result);
            }

            return result;
        },

        _create: function()
        {
            var state = this._getState();

            if(!this.options.linkSites) this.options.linkSites = VRS.linksRenderer.getDefaultAircraftLinkSites();

            this.element.addClass('aircraftLinks');
        },

        _destroy: function()
        {
            var state = this._getState();

            this._removeLinkElements(state);
            state.aircraft = undefined;
        },
        //endregion

        //region -- renderForAircraft
        /**
         * Renders the links for the aircraft passed across. The aircraft can be null, in which case the links are removed.
         * @param {VRS.Aircraft}    aircraft        The aircraft to render links for.
         * @param {bool}            forceRefresh    True if links are to be rendered even if the values they depend upon have not changed.
         */
        renderForAircraft: function(aircraft, forceRefresh)
        {
            var state = this._getState();

            if(state.aircraft !== aircraft) forceRefresh = true;
            state.aircraft = aircraft;

            this.reRender(forceRefresh, state);
        },

        /**
         * Re-renders the links using the last aircraft passed to renderForAircraft.
         * @param {bool}                            forceRefresh    True if links are to be rendered even if the values they depend upon have not changed.
         * @param {VRS.AircraftLinksPluginState}   [state]          The optional state object - for internal use only, omit when calling from outside the plugin.
         */
        reRender: function(forceRefresh, state)
        {
            if(!state) state = this._getState();
            var options = this.options;

            var aircraft = state.aircraft;

            if(options.linkSites.length < 1) this._removeLinkElements(state);
            else {
                if(state.linkElements.length !== options.linkSites.length) {
                    this._removeLinkElements(state);
                    var self = this;
                    $.each(options.linkSites, function(/** number */idx, /** VRS.LinkSite|VRS.LinkRenderHandler */ siteOrHandler) {
                        var handler = VRS.linksRenderer.findLinkHandler(siteOrHandler);

                        if(idx !== 0) {
                            state.separatorElements.push(
                                $('<span/>')
                                    .text(VRS.globalOptions.linkSeparator)
                                    .hide()
                                    .appendTo(self.element)
                            );
                        }

                        var linkElement = $('<a/>')
                            .attr('href', '#')
                            .attr('target', '_self')
                            .addClass(VRS.globalOptions.linkClass)
                            .text('')
                            .hide()
                            .appendTo(self.element);
                        if(handler && handler.onClick) linkElement.on('click', function(/** Event */ event) {
                            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
                            handler.onClick(event);
                        });
                        state.linkElements.push(linkElement);
                        state.linkVisible.push(false);
                    });
                }

                $.each(options.linkSites, function(/** number */idx, /** VRS.LinkSite */linkSite) {
                    var linkElement = state.linkElements[idx];
                    var handler = VRS.linksRenderer.findLinkHandler(linkSite);
                    var canShowLink = !!(handler && handler.canLinkAircraft(aircraft));

                    if(!canShowLink) {
                        linkElement.hide();
                        state.linkVisible[idx] = false;
                    } else {
                        if(forceRefresh || !state.linkVisible[idx] || handler.hasChanged(aircraft)) {
                            linkElement
                                .attr('href', handler.buildUrl(aircraft))
                                .attr('target', handler.getTarget(aircraft))
                                .text(handler.getTitle(aircraft))
                                .show();
                            state.linkVisible[idx] = true;
                        }
                    }
                });

                var atLeastOneLinkVisible = false;
                $.each(state.linkVisible, function(/** number */idx, /** boolean */linkVisible) {
                    if(idx > 0) {
                        var showSeparator = linkVisible && atLeastOneLinkVisible;
                        var separatorElement = state.separatorElements[idx - 1];
                        if(showSeparator) separatorElement.show();
                        else              separatorElement.hide();
                    }
                    if(linkVisible) atLeastOneLinkVisible = true;
                });
            }
        },
        //endregion

        //region -- _removeLinkElements
        /**
         * Removes the elements for the links.
         * @param {VRS.AircraftLinksPluginState} state
         * @private
         */
        _removeLinkElements: function(state)
        {
            $.each(state.linkElements, function(idx, /** jQuery */element) {
                if(element && element instanceof jQuery) {
                    element.off();
                    element.remove();
                }
            });
            state.linkElements = [];
            state.linkVisible = [];

            $.each(state.separatorElements, function(idx, /** jQuery */element) {
                element.remove();
            });
            state.separatorElements = [];
        },
        //endregion

        __nop: null
    });
}(window.VRS = window.VRS || {}, jQuery));