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
 * @fileoverview Code that handles formatting links for aircraft.
 */

(function(VRS, $, /** object= */undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.linkSeparator = VRS.globalOptions.linkSeparator || ' : : ';            // The separator to place between aircraft links.
    VRS.globalOptions.linkClass = VRS.globalOptions.linkClass || 'aircraftLink';            // The class to give aircraft links.
    //endregion

    //region LinkRenderHandler
    /**
     * Describes a handler for a VRS.LinkSite.
     * @param {object}                                  settings
     * @param {VRS.LinkSite}                            settings.linkSite           The link site that this object handles.
     * @param {number}                                  settings.displayOrder       The relative order in which the link is displayed. Lower values are displayed before higher values. -ve values are not displayed as 'normal' aircraft links in detail panels etc.
     * @param {function(VRS.Aircraft):bool}             settings.canLinkAircraft    A method that returns true if a link can be produced for the aircraft.
     * @param {function(VRS.Aircraft):bool}             settings.hasChanged         A method that returns true if the values that the link relies upon have changed in the last update.
     * @param {string|function(VRS.Aircraft):string}    settings.title              Either a string title for the link or a method that takes an aircraft and returns the title.
     * @param {function(VRS.Aircraft):string}           settings.buildUrl           A method that takes an aircraft and returns the URL.
     * @param {string|function(VRS.Aircraft):string}   [settings.target]            An optional target string or a callback that takes an aircraft and returns the name of the target page for HTML links.
     * @param {function()}                             [settings.onClick]           An optional callback that is called when the URL is clicked.
     * @constructor
     */
    VRS.LinkRenderHandler = function(settings)
    {
        if(!settings) throw 'You must supply settings';
        if(!settings.linkSite || !VRS.enumHelper.getEnumName(VRS.LinkSite, settings.linkSite)) throw 'There is no LinkSite called ' + VRS.LinkSite;
        if(settings.displayOrder === undefined) throw 'You must provide a display order';
        if(!settings.canLinkAircraft) throw 'You must supply a canLinkAircraft callback';
        if(!settings.hasChanged) throw 'You must supply a hasChanged';
        if(!settings.title) throw 'You must supply a title';
        if(!settings.buildUrl) throw 'You must supply the buildUrl callback';

        var that = this;
        this.linkSite = settings.linkSite;
        this.displayOrder = settings.displayOrder;
        this.canLinkAircraft = settings.canLinkAircraft;
        this.hasChanged = settings.hasChanged;
        this.title = settings.title;
        this.buildUrl = settings.buildUrl;
        this.target = settings.target || function(/** VRS.Aircraft */ aircraft) { return that.linkSite + '-' + aircraft.formatIcao(); };
        this.onClick = settings.onClick;

        /**
         * Returns the title of the link for the aircraft.
         * @param {VRS.Aircraft} aircraft The aircraft to fetch the title for.
         * @returns {string} The title of the link.
         */
        this.getTitle = function(aircraft)
        {
            return that.title instanceof Function ? that.title(aircraft) : that.title;
        };

        /**
         * Returns the target of the link for the aircraft.
         * @param {VRS.Aircraft} aircraft
         * @returns {string}
         */
        this.getTarget = function(aircraft)
        {
            return that.target instanceof Function ? that.target(aircraft) : that.target;
        };
    };
    //endregion

    //region linkRenderHandlers
    /**
     * The collection of VRS.LinkRenderHandler objects.
     * @type {Array.<VRS.LinkRenderHandler>}
     */
    VRS.linkRenderHandlers = [
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.AirportDataDotCom,
            displayOrder:       100,
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return aircraft && aircraft.registration.val; },
            hasChanged:         function(/** VRS.Aircraft */ aircraft) { return aircraft.registration.chg; },
            title:              'www.airport-data.com',
            buildUrl:           function(/** VRS.Aircraft */ aircraft) { return 'http://www.airport-data.com/aircraft/' + VRS.stringUtility.htmlEscape(aircraft.formatRegistration()) + '.html'; },
            target:             'airport-data'
        }),
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.AirlinersDotNet,
            displayOrder:       200,
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return aircraft && aircraft.registration.val; },
            hasChanged:         function(/** VRS.Aircraft */ aircraft) { return aircraft.registration.chg; },
            title:              'www.airliners.net',
            buildUrl:           function(/** VRS.Aircraft */ aircraft) { return 'http://www.airliners.net/search/photo.search?regsearch=' + VRS.stringUtility.htmlEscape(aircraft.formatRegistration()); },
            target:             'airliners'
        }),
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.AirframesDotOrg,
            displayOrder:       300,
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return aircraft && aircraft.registration.val; },
            hasChanged:         function(/** VRS.Aircraft */ aircraft) { return aircraft.registration.chg; },
            title:              'www.airframes.org',
            buildUrl:           function(/** VRS.Aircraft */ aircraft) { return 'http://www.airframes.org/reg/' + VRS.stringUtility.htmlEscape(aircraft.formatRegistration(true)); },
            target:             'airframes'
        }),
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.StandingDataMaintenance,
            displayOrder:       -1,
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return (!VRS.serverConfig || VRS.serverConfig.routeSubmissionEnabled()) && aircraft && aircraft.callsign.val; },
            hasChanged:         function(/** VRS.Aircraft */ aircraft) { return aircraft.callsign.chg; },
            title:              function(/** VRS.Aircraft */ aircraft) { return aircraft.hasRoute() ? VRS.$$.SubmitRouteCorrection : VRS.$$.SubmitRoute; },
            buildUrl:           function(/** VRS.Aircraft */ aircraft) { return 'http://sdm.virtualradarserver.co.uk/Edit/AddCallsigns.aspx?callsigns=' + VRS.stringUtility.htmlEscape(aircraft.formatCallsign(false)); },
            target:             'vrs-sdm'
        })
    ];
    //endregion

    //region Functionality render handlers - AutoSelectLinkRenderHelper, CentreOnSelectedAircraftLinkRenderHandler, PauseLinkRenderHandler
    //region -- AutoSelectLinkRenderHelper
    /**
     * Describes a handler for a link renderer that can control the enable / disable setting of an AircraftAutoSelect.
     * @param {VRS.AircraftAutoSelect} aircraftAutoSelect
     * @constructor
     */
    VRS.AutoSelectLinkRenderHelper = function(aircraftAutoSelect)
    {
        VRS.LinkRenderHandler.call(this, {
            linkSite:           VRS.LinkSite.None,
            displayOrder:       -1,
            canLinkAircraft:    function() { return true; },
            hasChanged:         function() { return false; },
            title:              function() { return aircraftAutoSelect.getEnabled() ? VRS.$$.DisableAutoSelect : VRS.$$.EnableAutoSelect; },
            buildUrl:           function() { return "#"; },
            target:             function() { return null; },
            onClick:            $.proxy(function(/** Event */ event) {
                aircraftAutoSelect.setEnabled(!aircraftAutoSelect.getEnabled());
                aircraftAutoSelect.saveState();
                event.stopPropagation();
                return false;
            }, this)
        });

        var _AutoSelectEnabledChangedHook = aircraftAutoSelect.hookEnabledChanged(autoSelectEnabledChanged, this);

        /** @type {Array.<VRS.vrsAircraftLinks>} */
        var _LinksRendererPlugin = [];
        this.addLinksRendererPlugin = function(/** VRS.vrsAircraftLinks */ value) { _LinksRendererPlugin.push(value); };

        /**
         * Releases any resources held by the object.
         */
        this.dispose = function()
        {
            if(_AutoSelectEnabledChangedHook) {
                aircraftAutoSelect.unhook(_AutoSelectEnabledChangedHook);
                _AutoSelectEnabledChangedHook = null;
            }
            _LinksRendererPlugin = [];
        };

        /**
         * Called when the auto-select option value changes.
         */
        function autoSelectEnabledChanged()
        {
            $.each(_LinksRendererPlugin, function(/** number */idx, /** VRS.vrsAircraftLinks */ linksRendererPlugin){
                linksRendererPlugin.reRender(true);
            });
        }
    };
    VRS.AutoSelectLinkRenderHelper.prototype = VRS.objectHelper.subclassOf(VRS.LinkRenderHandler);
    //endregion

    //region -- CentreOnSelectedAircraftLinkRenderHandler
    /**
     * The LinkRenderHandler that centres the map on the selected aircraft when you click the link.
     * @param {VRS.AircraftList}    aircraftList
     * @param {VRS.vrsMap}          mapPlugin
     * @constructor
     */
    VRS.CentreOnSelectedAircraftLinkRenderHandler = function(aircraftList, mapPlugin)
    {
        VRS.LinkRenderHandler.call(this, {
            linkSite:           VRS.LinkSite.None,
            displayOrder:       -1,
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return aircraft && mapPlugin && aircraftList && aircraft.hasPosition(); },
            hasChanged:         function() { return false; },
            title:              function() { return VRS.$$.CentreOnSelectedAircraft; },
            buildUrl:           function() { return "#"; },
            target:             function() { return null; },
            onClick:            $.proxy(function(/** Event */ event) {
                var selectedAircraft = aircraftList.getSelectedAircraft();
                mapPlugin.panTo(selectedAircraft.getPosition());
                event.stopPropagation();
                return false;
            }, this)
        });
    };
    VRS.CentreOnSelectedAircraftLinkRenderHandler.prototype = VRS.objectHelper.subclassOf(VRS.LinkRenderHandler);
    //endregion

    //region -- HideAircraftNotOnMapLinkRenderHandler
    VRS.HideAircraftNotOnMapLinkRenderHandler = function(aircraftListFetcher)
    {
        VRS.LinkRenderHandler.call(this, {
            linkSite:           VRS.LinkSite.None,
            displayOrder:       -1,
            canLinkAircraft:    function() { return true; },
            hasChanged:         function() { return false; },
            title:              function() { return aircraftListFetcher.getHideAircraftNotOnMap() ? VRS.$$.AllAircraft : VRS.$$.OnlyAircraftOnMap; },
            buildUrl:           function() { return '#';},
            target:             function() { return null; },
            onClick:            $.proxy(function(/** Event */ event) {
                aircraftListFetcher.setHideAircraftNotOnMap(!aircraftListFetcher.getHideAircraftNotOnMap());
                aircraftListFetcher.saveState();
                event.stopPropagation();
                return false;
            })
        });

        var _HideAircraftNotOnMapHook = aircraftListFetcher.hookHideAircraftNotOnMapChanged(hideAircraftChanged, this);

        /** @type {Array.<VRS.vrsAircraftLinks>} */
        var _LinksRendererPlugin = [];
        this.addLinksRendererPlugin = function(/** VRS.vrsAircraftLinks */ value) { _LinksRendererPlugin.push(value); };

        this.dispose = function()
        {
            if(_HideAircraftNotOnMapHook) {
                aircraftListFetcher.unhook(_HideAircraftNotOnMapHook);
                _HideAircraftNotOnMapHook = null;
            }
            _LinksRendererPlugin = [];
        };

        function hideAircraftChanged()
        {
            $.each(_LinksRendererPlugin, function(/** number */idx, /** VRS.vrsAircraftLinks */ linksRendererPlugin){
                linksRendererPlugin.reRender(true);
            });
        }
    };
    VRS.HideAircraftNotOnMapLinkRenderHandler.prototype = VRS.objectHelper.subclassOf(VRS.LinkRenderHandler);
    //endregion

    //region -- JumpToAircraftDetailPageRenderHandler
    VRS.JumpToAircraftDetailPageRenderHandler = function()
    {
        VRS.LinkRenderHandler.call(this, {
            linkSite:           VRS.LinkSite.None,
            displayOrder:       -1,
            canLinkAircraft:    function() { return true; },
            hasChanged:         function() { return false; },
            title:              function() { return VRS.$$.ShowDetail; },
            buildUrl:           function() { return '#'; },
            target:             function() { return null; },
            onClick:            $.proxy(function(/** Event */ event) {
                if(VRS.pageManager) VRS.pageManager.show(VRS.MobilePageName.AircraftDetail);
                event.stopPropagation();
                return false;
            })
        });
    };
    VRS.JumpToAircraftDetailPageRenderHandler.prototype = VRS.objectHelper.subclassOf(VRS.LinkRenderHandler);
    //endregion

    //region -- PauseLinkRenderHandler
    VRS.PauseLinkRenderHandler = function(aircraftListFetcher)
    {
        VRS.LinkRenderHandler.call(this, {
            linkSite:           VRS.LinkSite.None,
            displayOrder:       -1,
            canLinkAircraft:    function() { return true; },
            hasChanged:         function() { return false; },
            title:              function() { return aircraftListFetcher.getPaused() ? VRS.$$.Resume : VRS.$$.Pause; },
            buildUrl:           function() { return '#';},
            target:             function() { return null; },
            onClick:            $.proxy(function(/** Event */ event) {
                aircraftListFetcher.setPaused(!aircraftListFetcher.getPaused());
                event.stopPropagation();
                return false;
            })
        });

        var _PausedChangedHook = aircraftListFetcher.hookPausedChanged(pausedChanged, this);

        /** @type {Array.<VRS.vrsAircraftLinks>} */
        var _LinksRendererPlugin = [];
        this.addLinksRendererPlugin = function(/** VRS.vrsAircraftLinks */ value) { _LinksRendererPlugin.push(value); };

        this.dispose = function()
        {
            if(_PausedChangedHook) {
                aircraftListFetcher.unhook(_PausedChangedHook);
                _PausedChangedHook = null;
            }
            _LinksRendererPlugin = [];
        };

        function pausedChanged()
        {
            $.each(_LinksRendererPlugin, function(/** number */idx, /** VRS.vrsAircraftLinks */ linksRendererPlugin){
                linksRendererPlugin.reRender(true);
            });
        }
    };
    VRS.PauseLinkRenderHandler.prototype = VRS.objectHelper.subclassOf(VRS.LinkRenderHandler);
    //endregion
    //endregion

    //region LinksRenderer
    /**
     * An object that can render links for aircraft.
     * @constructor
     */
    VRS.LinksRenderer = function()
    {
        //region -- getDefaultAircraftLinkSites
        /**
         * Returns the array of link sites to show for an aircraft, in the correct order.
         * @returns {Array.<VRS.LinkSite>}
         */
        this.getDefaultAircraftLinkSites = function()
        {
            /** @type {Array.<VRS.LinkSite>} */
            var result = [];
            VRS.arrayHelper.select(getAircraftLinkHandlers(), function(/** VRS.LinkRenderHandler */ handler) {
                result.push(handler.linkSite);
            });

            return result;
        };
        //endregion

        //region -- findLinkHandler, getAircraftLinkHandlers
        /**
         * Returns the link handler for the site passed across. The function also accepts a handler as a parameter, in
         * which case the handler is just returned.
         * @param {VRS.LinkSite|VRS.LinkRenderHandler} linkSite
         * @returns {VRS.LinkRenderHandler}
         */
        this.findLinkHandler = function(linkSite)
        {
            /** @type {VRS.LinkRenderHandler} */
            var result = null;

            if(linkSite instanceof VRS.LinkRenderHandler) result = linkSite;
            else {
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
        };

        /**
         * Returns an array of handlers that should be used to render standard links for aircraft. They will have been
         * sorted into the correct display order.
         * @returns {Array.<VRS.LinkRenderHandler>}
         */
        function getAircraftLinkHandlers()
        {
            var result = VRS.arrayHelper.filter(VRS.linkRenderHandlers, function(/** VRS.LinkRenderHandler */ handler) {
                return handler.displayOrder > 0;
            });
            result.sort(function(/** VRS.LinkRenderHandler */ lhs, /** VRS.LinkRenderHandler */ rhs){
                return lhs.displayOrder - rhs.displayOrder;
            });

            return result;
        }
        //endregion
    };
    //endregion

    //region Pre-builts
    VRS.linksRenderer = new VRS.LinksRenderer();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));