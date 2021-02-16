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
 * @fileoverview Code that handles formatting links for aircraft.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.linkSeparator = VRS.globalOptions.linkSeparator || ' : : ';   // The separator to place between aircraft links.
    VRS.globalOptions.linkClass = VRS.globalOptions.linkClass || 'aircraftLink';    // The class to give aircraft links.

    /**
     * The settings to pass when creating a new LinkRenderHandler.
     */
    export interface LinkRenderHandler_Settings
    {
        /**
         * The link site that this object handles.
         */
        linkSite: LinkSiteEnum;

        /**
         * The relative order in which the link is displayed. Lower values are displayed before higher values. -ve values are not displayed as 'normal' aircraft links in detail panels etc.
         */
        displayOrder: number;

        /**
         * A method that returns true if a link can be produced for the aircraft.
         */
        canLinkAircraft: (aircraft: Aircraft) => boolean;

        /**
         * A method that returns true if the values that the link relies upon have changed in the last update.
         */
        hasChanged: (aircraft: Aircraft) => boolean;

        /**
         * Either a string title for the link or a method that takes an aircraft and returns the title.
         */
        title: string | AircraftFuncReturningString;

        /**
         * A method that takes an aircraft and returns the URL.
         */
        buildUrl: (aircraft: Aircraft) => string;

        /**
         * An optional target string or a callback that takes an aircraft and returns the name of the target page for HTML links.
         */
        target?: string | AircraftFuncReturningString;

        /**
         * An optional callback that is called when the URL is clicked.
         */
        onClick?: (event: Event) => void;
    }

    /**
     * Describes a handler for a VRS.LinkSite.
     */
    export class LinkRenderHandler
    {
        // Kept as public fields for backwards compatibility
        linkSite:           LinkSiteEnum;
        displayOrder:       number;
        canLinkAircraft:    (aircraft: Aircraft) => boolean;
        hasChanged:         (aircraft: Aircraft) => boolean;
        title:              string | AircraftFuncReturningString;
        buildUrl:           (aircraft: Aircraft) => string;
        target:             string | AircraftFuncReturningString;
        onClick:            (event: Event) => void;

        constructor(settings: LinkRenderHandler_Settings)
        {
            if(!settings) throw 'You must supply settings';
            if(!settings.linkSite || !VRS.enumHelper.getEnumName(VRS.LinkSite, settings.linkSite)) throw 'There is no LinkSite called ' + VRS.LinkSite;
            if(settings.displayOrder === undefined) throw 'You must provide a display order';
            if(!settings.canLinkAircraft) throw 'You must supply a canLinkAircraft callback';
            if(!settings.hasChanged) throw 'You must supply a hasChanged';
            if(!settings.title) throw 'You must supply a title';
            if(!settings.buildUrl) throw 'You must supply the buildUrl callback';

            this.linkSite = settings.linkSite;
            this.displayOrder = settings.displayOrder;
            this.canLinkAircraft = settings.canLinkAircraft;
            this.hasChanged = settings.hasChanged;
            this.title = settings.title;
            this.buildUrl = settings.buildUrl;
            this.target = settings.target || ((aircraft: Aircraft) => this.linkSite + '-' + aircraft.formatIcao());
            this.onClick = settings.onClick;
        }

        getTitle(aircraft: Aircraft) : string
        {
            if($.isFunction(this.title)) {
                return (<(aircraft: Aircraft) => string>(this.title))(aircraft);
            } else {
                return <string>(this.title);
            }
        }

        getTarget(aircraft: Aircraft) : string
        {
            if($.isFunction(this.target)) {
                return (<(aircraft: Aircraft) => string>(this.target))(aircraft);
            } else {
                return <string>(this.target);
            }
        }
    }

    /**
     * The collection of VRS.LinkRenderHandler objects.
     */
    export var linkRenderHandlers = [
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.AirlinersDotNet,
            displayOrder:       200,
            canLinkAircraft:    function(aircraft) { return aircraft && !!aircraft.registration.val; },
            hasChanged:         function(aircraft) { return aircraft.registration.chg; },
            title:              'www.airliners.net',
            buildUrl:           function(aircraft) { return 'http://www.airliners.net/search?registrationActual=' + VRS.stringUtility.htmlEscape(aircraft.formatRegistration()); },
            target:             'airliners'
        }),
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.JetPhotosDotCom,
            displayOrder:       300,
            canLinkAircraft:    function(aircraft) { return aircraft && !!aircraft.registration.val; },
            hasChanged:         function(aircraft) { return aircraft.registration.chg; },
            title:              'www.jetphotos.com',
            buildUrl:           function(aircraft) { return 'https://www.jetphotos.com/photo/keyword/' + VRS.stringUtility.htmlEscape(aircraft.formatRegistration(false)); },
            target:             'jetphotos'
        }),
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.StandingDataMaintenance,
            displayOrder:       -1,
            canLinkAircraft:    function(aircraft) { return (!VRS.serverConfig || VRS.serverConfig.routeSubmissionEnabled()) && aircraft && !!aircraft.callsign.val && aircraft.canSubmitRoute(); },
            hasChanged:         function(aircraft) { return aircraft.callsign.chg; },
            title:              function(aircraft) { return aircraft.hasRoute() ? VRS.$$.SubmitRouteCorrection : VRS.$$.SubmitRoute; },
            buildUrl:           function(aircraft) { return 'http://sdm.virtualradarserver.co.uk/Edit/AddCallsigns.aspx?callsigns=' + VRS.stringUtility.htmlEscape(aircraft.formatCallsign(false)); },
            target:             'vrs-sdm'
        })
    ];

    /**
     * The base class for link render handlers that need to be able to refresh the links plugin after they have been clicked.
     */
    export abstract class LinkRenderHandler_AutoRefreshPluginBase extends LinkRenderHandler
    {
        protected _LinksRendererPlugin: AircraftLinksPlugin[] = [];

        constructor(settings: LinkRenderHandler_Settings)
        {
            super(settings);
        }

        protected disposeBase()
        {
            this._LinksRendererPlugin = [];
        }

        addLinksRendererPlugin(value: AircraftLinksPlugin)
        {
            this._LinksRendererPlugin.push(value);
        }

        protected refreshAircraftLinksPlugin()
        {
            $.each(this._LinksRendererPlugin, function(idx, linksRendererPlugin) {
                linksRendererPlugin.reRender(true);
            });
        }
    }

    /**
     * Describes a handler for a link renderer that can control the enable / disable setting of an AircraftAutoSelect.
     * @param {VRS.AircraftAutoSelect} aircraftAutoSelect
     * @constructor
     */
    export class AutoSelectLinkRenderHelper extends LinkRenderHandler_AutoRefreshPluginBase
    {
        private _AutoSelectEnabledChangedHook : IEventHandle;
        private _AircraftAutoSelect: AircraftAutoSelect;

        constructor(aircraftAutoSelect: AircraftAutoSelect)
        {
            super({
                linkSite:           VRS.LinkSite.None,
                displayOrder:       -1,
                canLinkAircraft:    function() { return true; },
                hasChanged:         function() { return false; },
                title:              function() { return aircraftAutoSelect.getEnabled() ? VRS.$$.DisableAutoSelect : VRS.$$.EnableAutoSelect; },
                buildUrl:           function() { return "#"; },
                target:             function() { return null; },
                onClick:            (event: Event) => {
                    aircraftAutoSelect.setEnabled(!aircraftAutoSelect.getEnabled());
                    aircraftAutoSelect.saveState();
                    event.stopPropagation();
                    return false;
                }
            });

            this._AircraftAutoSelect = aircraftAutoSelect;
            this._AutoSelectEnabledChangedHook = aircraftAutoSelect.hookEnabledChanged(this.autoSelectEnabledChanged, this);
        }


        /**
         * Releases any resources held by the object.
         */
        dispose()
        {
            if(this._AutoSelectEnabledChangedHook) {
                this._AircraftAutoSelect.unhook(this._AutoSelectEnabledChangedHook);
                this._AutoSelectEnabledChangedHook = null;
            }
            this._AircraftAutoSelect = null;
            super.disposeBase();
        }

        /**
         * Called when the auto-select option value changes.
         */
        private autoSelectEnabledChanged()
        {
            super.refreshAircraftLinksPlugin();
        }
    }

    /**
     * The LinkRenderHandler that centres the map on the selected aircraft when you click the link.
     */
    export class CentreOnSelectedAircraftLinkRenderHandler extends LinkRenderHandler
    {
        constructor(aircraftList: AircraftList, mapPlugin: IMap)
        {
            super({
                linkSite:           VRS.LinkSite.None,
                displayOrder:       -1,
                canLinkAircraft:    function(aircraft) { return aircraft && mapPlugin && aircraftList && aircraft.hasPosition() && !aircraft.positionStale.val; },
                hasChanged:         function() { return false; },
                title:              function() { return VRS.$$.CentreOnSelectedAircraft; },
                buildUrl:           function() { return "#"; },
                target:             function() { return null; },
                onClick:            (event: Event) => {
                    var selectedAircraft = aircraftList.getSelectedAircraft();
                    mapPlugin.panTo(selectedAircraft.getPosition());
                    event.stopPropagation();
                    return false;
                }
            });
        }
    }

    /**
     * The LinkRenderHandler that suppresses aircraft that are not visible on the map from the aircraft list.
     */
    export class HideAircraftNotOnMapLinkRenderHandler extends LinkRenderHandler_AutoRefreshPluginBase
    {
        private _HideAircraftNotOnMapHook: IEventHandle;
        private _AircraftListFetcher: AircraftListFetcher;

        constructor(aircraftListFetcher: AircraftListFetcher)
        {
            super({
                linkSite:           VRS.LinkSite.None,
                displayOrder:       -1,
                canLinkAircraft:    function() { return true; },
                hasChanged:         function() { return false; },
                title:              function() { return aircraftListFetcher.getHideAircraftNotOnMap() ? VRS.$$.AllAircraft : VRS.$$.OnlyAircraftOnMap; },
                buildUrl:           function() { return '#';},
                target:             function() { return null; },
                onClick:            (event: Event ) => {
                    aircraftListFetcher.setHideAircraftNotOnMap(!aircraftListFetcher.getHideAircraftNotOnMap());
                    aircraftListFetcher.saveState();
                    event.stopPropagation();
                    return false;
                }
            });

            this._AircraftListFetcher = aircraftListFetcher;
            this._HideAircraftNotOnMapHook = aircraftListFetcher.hookHideAircraftNotOnMapChanged(this.hideAircraftChanged, this);
        }

        dispose()
        {
            if(this._HideAircraftNotOnMapHook) {
                this._AircraftListFetcher.unhook(this._HideAircraftNotOnMapHook);
                this._HideAircraftNotOnMapHook = null;
            }
            this._AircraftListFetcher = null;
            super.disposeBase();
        }

        private hideAircraftChanged()
        {
            super.refreshAircraftLinksPlugin();
        }
    }

    /**
     * The link renderer that jumps to the mobile site's aircraft detail page when clicked for an aircraft.
     */
    export class JumpToAircraftDetailPageRenderHandler extends LinkRenderHandler
    {
        constructor()
        {
            super({
                linkSite:           VRS.LinkSite.None,
                displayOrder:       -1,
                canLinkAircraft:    function() { return true; },
                hasChanged:         function() { return false; },
                title:              function() { return VRS.$$.ShowDetail; },
                buildUrl:           function() { return '#'; },
                target:             function() { return null; },
                onClick:            (event: Event) => {
                    if(VRS.pageManager) {
                        VRS.pageManager.show(VRS.MobilePageName.AircraftDetail);
                    }
                    event.stopPropagation();
                    return false;
                }
            });
        }
    }

    /**
     * The link handler that pauses aircraft list fetches when clicked.
     */
    export class PauseLinkRenderHandler extends LinkRenderHandler_AutoRefreshPluginBase
    {
        private _PausedChangedHook: IEventHandle;
        private _AircraftListFetcher: AircraftListFetcher;

        constructor(aircraftListFetcher: AircraftListFetcher)
        {
            super({
                linkSite:           VRS.LinkSite.None,
                displayOrder:       -1,
                canLinkAircraft:    function() { return true; },
                hasChanged:         function() { return false; },
                title:              function() { return aircraftListFetcher.getPaused() ? VRS.$$.Resume : VRS.$$.Pause; },
                buildUrl:           function() { return '#';},
                target:             function() { return null; },
                onClick:            (event: Event ) => {
                    aircraftListFetcher.setPaused(!aircraftListFetcher.getPaused());
                    event.stopPropagation();
                    return false;
                }
            });

            this._AircraftListFetcher = aircraftListFetcher;
            this._PausedChangedHook = aircraftListFetcher.hookPausedChanged(this.pausedChanged, this);
        }

        dispose()
        {
            if(this._PausedChangedHook) {
                this._AircraftListFetcher.unhook(this._PausedChangedHook);
                this._PausedChangedHook = null;
            }
            this._AircraftListFetcher = null;
            super.disposeBase();
        }

        private pausedChanged()
        {
            super.refreshAircraftLinksPlugin();
        }
    }

    /**
     * An object that can render links for aircraft.
     */
    export class LinksRenderer
    {
        /**
         * Returns the array of link sites to show for an aircraft, in the correct order.
         */
        getDefaultAircraftLinkSites() : LinkSiteEnum[]
        {
            var result: LinkSiteEnum[] = [];
            VRS.arrayHelper.select(this.getAircraftLinkHandlers(), function(handler) {
                result.push(handler.linkSite);
            });

            return result;
        }

        /**
         * Returns the link handler for the site passed across. The function also accepts a handler as a parameter, in
         * which case the handler is just returned.
         */
        findLinkHandler(linkSite: LinkSiteEnum | LinkRenderHandler) : LinkRenderHandler
        {
            var result: LinkRenderHandler = null;

            if(linkSite instanceof VRS.LinkRenderHandler) {
                result = linkSite;
            } else {
                var length = VRS.linkRenderHandlers.length;
                for(var i = 0;i < length;++i) {
                    var handler = VRS.linkRenderHandlers[i];
                    if(handler.linkSite === linkSite) {
                        result = handler;
                        break;
                    }
                }
            }

            return result;
        }

        /**
         * Returns an array of handlers that should be used to render standard links for aircraft. They will have been
         * sorted into the correct display order.
         * @returns {Array.<VRS.LinkRenderHandler>}
         */
        private getAircraftLinkHandlers() : LinkRenderHandler[]
        {
            var result = VRS.arrayHelper.filter(VRS.linkRenderHandlers, function(handler) {
                return handler.displayOrder > 0;
            });
            result.sort(function(lhs, rhs) {
                return lhs.displayOrder - rhs.displayOrder;
            });

            return result;
        }
    }

    /*
     * Pre-builts
     */
    export var linksRenderer = new VRS.LinksRenderer();
}
