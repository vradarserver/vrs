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

namespace VRS
{
    export type LinkSiteEnumOrRenderHandler = LinkSiteEnum | LinkRenderHandler;

    /**
     * The options that can be passed when creating a new AircraftLinksPlugin.
     */
    export interface AircraftLinksPlugin_Options
    {
        /**
         * An array of the links to display. If null or undefined then all of the default links for an aircraft are displayed.
         */
        linkSites: LinkSiteEnumOrRenderHandler[];
    }

    /**
     * The object that carries state for the aircraft links plugin.
     */
    class AircraftLinksPlugin_State
    {
        /**
         * The aircraft that the plugin is displaying links for.
         */
        aircraft: Aircraft = undefined;

        /**
         * An array of jQuery elements for each link rendered.
         */
        linkElements: JQuery[] = [];

        /**
         * An array of bools indicating that the corresponding linkElement item is visible.
         */
        linkVisible: boolean[] = [];

        /**
         * An array of elements that contain separators between the links.
         */
        separatorElements: JQuery[] = [];
    }

    /*
     * jQueryUIHelper
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    jQueryUIHelper.getAircraftLinksPlugin = (jQueryElement: JQuery) : AircraftLinksPlugin =>
    {
        return <AircraftLinksPlugin>jQueryElement.data('vrsVrsAircraftLinks');
    }
    jQueryUIHelper.getAircraftLinksOptions = (overrides: AircraftLinksPlugin_Options) : AircraftLinksPlugin_Options =>
    {
        return $.extend({
            linkSites:  null
        }, overrides);
    }

    /**
     * A jQuery widget that can display links for an aircraft
     */
    export class AircraftLinksPlugin extends JQueryUICustomWidget
    {
        options: AircraftLinksPlugin_Options;

        constructor()
        {
            super();
            this.options = jQueryUIHelper.getAircraftLinksOptions();
        }

        _getState()
        {
            var result = this.element.data('aircraftLinksPluginState');
            if(result === undefined) {
                result = new AircraftLinksPlugin_State();
                this.element.data('aircraftLinksPluginState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();

            if(!this.options.linkSites) {
                this.options.linkSites = VRS.linksRenderer.getDefaultAircraftLinkSites();
            }

            this.element.addClass('aircraftLinks');
        }

        _destroy()
        {
            var state = this._getState();

            this._removeLinkElements(state);
            state.aircraft = undefined;
        }

        /**
         * Renders the links for the aircraft passed across. The aircraft can be null, in which case the links are removed.
         */
        renderForAircraft(aircraft: Aircraft, forceRefresh: boolean)
        {
            var state = this._getState();

            if(state.aircraft !== aircraft) {
                forceRefresh = true;
            }
            state.aircraft = aircraft;

            this.doReRender(forceRefresh, state);
        }

        /**
         * Re-renders the links using the last aircraft passed to renderForAircraft.
         */
        reRender(forceRefresh: boolean)
        {
            this.doReRender(forceRefresh, this._getState());
        }

        private doReRender(forceRefresh: boolean, state: AircraftLinksPlugin_State)
        {
            if(!state) state = this._getState();
            var options = this.options;

            var aircraft = state.aircraft;

            if(options.linkSites.length < 1) {
                this._removeLinkElements(state);
            } else {
                if(state.linkElements.length !== options.linkSites.length) {
                    this._removeLinkElements(state);
                    $.each(options.linkSites, (idx, siteOrHandler) => {
                        var handler = VRS.linksRenderer.findLinkHandler(siteOrHandler);

                        if(idx !== 0) {
                            state.separatorElements.push(
                                $('<span/>')
                                    .text(VRS.globalOptions.linkSeparator)
                                    .hide()
                                    .appendTo(this.element)
                            );
                        }

                        var linkElement = $('<a/>')
                            .attr('href', '#')
                            .attr('target', '_self')
                            .addClass(VRS.globalOptions.linkClass)
                            .text('')
                            .hide()
                            .appendTo(this.element);
                        if(handler && handler.onClick) {
                            linkElement.on('click', (event: Event) => {
                                if(VRS.timeoutManager) {
                                    VRS.timeoutManager.resetTimer();
                                }
                                handler.onClick(event);
                            });
                        }
                        state.linkElements.push(linkElement);
                        state.linkVisible.push(false);
                    });
                }

                $.each(options.linkSites, (idx, linkSite) => {
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
                $.each(state.linkVisible, (idx, linkVisible) => {
                    if(idx > 0) {
                        var showSeparator = linkVisible && atLeastOneLinkVisible;
                        var separatorElement = state.separatorElements[idx - 1];
                        if(showSeparator) separatorElement.show();
                        else              separatorElement.hide();
                    }
                    if(linkVisible) atLeastOneLinkVisible = true;
                });
            }
        }

        /**
         * Removes the elements for the links.
         */
        private _removeLinkElements(state: AircraftLinksPlugin_State)
        {
            $.each(state.linkElements, (idx, element) => {
                if(element && element instanceof jQuery) {
                    element.off();
                    element.remove();
                }
            });
            state.linkElements = [];
            state.linkVisible = [];

            $.each(state.separatorElements, (idx, element) => {
                element.remove();
            });
            state.separatorElements = [];
        }
    }

    $.widget('vrs.vrsAircraftLinks', new AircraftLinksPlugin());
}

declare interface JQuery
{
    vrsAircraftLinks();
    vrsAircraftLinks(options: VRS.AircraftLinksPlugin_Options);
    vrsAircraftLinks(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
