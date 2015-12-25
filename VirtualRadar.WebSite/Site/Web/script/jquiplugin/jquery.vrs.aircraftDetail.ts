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
 * @fileoverview A jQueryUI plugin that can manage the detail panel for an aircraft.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.detailPanelDefaultShowUnits = VRS.globalOptions.detailPanelDefaultShowUnits !== undefined ? VRS.globalOptions.detailPanelDefaultShowUnits : true;     // True if the aircraft details panel should default to showing distance / speed / height units.
    if(VRS.globalOptions.isMobile) {
        VRS.globalOptions.detailPanelDefaultItems = VRS.globalOptions.detailPanelDefaultItems || [              // The default set of items to display on the detail panel.
            VRS.RenderProperty.Altitude,
            VRS.RenderProperty.VerticalSpeed,
            VRS.RenderProperty.Speed,
            VRS.RenderProperty.Heading,
            VRS.RenderProperty.Distance,
            VRS.RenderProperty.Squawk,
            VRS.RenderProperty.Engines,
            VRS.RenderProperty.Species,
            VRS.RenderProperty.Wtc,
            VRS.RenderProperty.RouteFull,
            VRS.RenderProperty.PictureOrThumbnails,
            VRS.RenderProperty.PositionOnMap
        ];
    } else {
        VRS.globalOptions.detailPanelDefaultItems = VRS.globalOptions.detailPanelDefaultItems || [              // The default set of items to display on the detail panel.
            VRS.RenderProperty.Altitude,
            VRS.RenderProperty.VerticalSpeed,
            VRS.RenderProperty.Speed,
            VRS.RenderProperty.Heading,
            VRS.RenderProperty.Distance,
            VRS.RenderProperty.Squawk,
            VRS.RenderProperty.Engines,
            VRS.RenderProperty.Species,
            VRS.RenderProperty.Wtc,
            VRS.RenderProperty.RouteFull,
            VRS.RenderProperty.PictureOrThumbnails
        ];
    }
    VRS.globalOptions.detailPanelUserCanConfigureItems = VRS.globalOptions.detailPanelUserCanConfigureItems !== undefined ? VRS.globalOptions.detailPanelUserCanConfigureItems : true;    // True if the user can change the items shown for an aircraft in the details panel.
    VRS.globalOptions.detailPanelShowSeparateRouteLink = VRS.globalOptions.detailPanelShowSeparateRouteLink !== undefined ? VRS.globalOptions.detailPanelShowSeparateRouteLink : true;  // Show a separate link to add or correct routes if the detail panel is showing routes.
    VRS.globalOptions.detailPanelShowAircraftLinks = VRS.globalOptions.detailPanelShowAircraftLinks !== undefined ? VRS.globalOptions.detailPanelShowAircraftLinks : true;              // True to show the links for an aircraft, false to suppress them.
    VRS.globalOptions.detailPanelShowEnableAutoSelect = VRS.globalOptions.detailPanelShowEnableAutoSelect !== undefined ? VRS.globalOptions.detailPanelShowEnableAutoSelect : true;     // True to show a link to enable and disable auto-select, false to suppress the link.
    VRS.globalOptions.detailPanelShowCentreOnAircraft = VRS.globalOptions.detailPanelShowCentreOnAircraft !== undefined ? VRS.globalOptions.detailPanelShowCentreOnAircraft : true;     // True to show a link to centre the map on the selected aircraft.
    VRS.globalOptions.detailPanelFlagUncertainCallsigns = VRS.globalOptions.detailPanelFlagUncertainCallsigns !== undefined ? VRS.globalOptions.detailPanelFlagUncertainCallsigns : true; // True if uncertain callsigns are to be flagged up on the detail panel.
    VRS.globalOptions.detailPanelDistinguishOnGround = VRS.globalOptions.detailPanelDistinguishOnGround !== undefined ? VRS.globalOptions.detailPanelDistinguishOnGround : true;        // True if altitudes should show 'GND' when the aircraft is on the ground.
    VRS.globalOptions.detailPanelAirportDataThumbnails = VRS.globalOptions.detailPanelAirportDataThumbnails || 2;                                                                       // The number of thumbnails to fetch from www.airport-data.com.

    /**
     * The options that AircraftDetailPlugin honours.
     */
    export interface AircraftDetailPlugin_Options
    {
        /**
         * The name to use when saving and loading state.
         */
        name?: string;

        /**
         * The VRS.AircraftList to display aircraft details from.
         */
        aircraftList?: AircraftList;

        /**
         * The VRS.UnitDisplayPreferences that dictates how values are to be displayed.
         */
        unitDisplayPreferences: UnitDisplayPreferences;

        /**
         * The VRS.AircraftAutoSelect that, if supplied, can be configured via controls in the detail panel.
         */
        aircraftAutoSelect?: AircraftAutoSelect;

        /**
         * The map to centre when the 'Centre selected aircraft on map' link is clicked. If it isn't supplied then the option isn't shown.
         */
        mapPlugin?: IMap;

        /**
         * True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
         */
        useSavedState?: boolean;

        /**
         * True if heights, distances and speeds should indicate their units.
         */
        showUnits?: boolean;

        /**
         * The items to show to the user aside from those that are always shown in the detail header.
         */
        items?: RenderPropertyEnum[];

        /**
         * True if a separate link to add or correct links is to be shown if the user is displaying routes. Always hidden if there are no routes on display.
         */
        showSeparateRouteLink?: boolean;

        /**
         * True if uncertain callsigns are to show be flagged up as such on display.
         */
        flagUncertainCallsigns?: boolean;

        /**
         * True if aircraft on the ground should be shown with altitudes of 'GND'.
         */
        distinguishOnGround?: boolean;

        /**
         * The map to pass to property renderers that display mirrored maps.
         */
        mirrorMapJQ?: JQuery;

        /**
         * The plotter options to pass to property renderers that employ aircraft plotters.
         */
        plotterOptions?: AircraftPlotterOptions;

        /**
         * The number of thumbnails to fetch from www.airport-data.com.
         */
        airportDataThumbnails?: number;
    }

    /**
     * Carries the state for the VRS.AircraftDetail plugin.
     */
    class AircraftDetailPlugin_State
    {
        /**
         * Indicates whether the detail panel responds to updates or not.
         */
        suspended = false;

        /**
         * A jQuery element for the container for the panel when an aircraft is selected.
         */
        container: JQuery = undefined;

        /**
         * A jQuery element for the container for the panel when no aircraft is selected.
         */
        noAircraftContainer: JQuery = undefined;

        /**
         * A jQuery element for the header's container.
         */
        headerContainer: JQuery = undefined;

        /**
         * A jQuery element for the body's container.
         */
        bodyContainer: JQuery = undefined;

        /**
         * A jQuery element for the links.
         */
        linksContainer: JQuery = undefined;

        /**
         * A direct reference to the plugin for standard aircraft links.
         */
        aircraftLinksPlugin: AircraftLinksPlugin = null;

        /**
         * A direct reference to the plugin for links to the routes submission site.
         */
        routeLinksPlugin: AircraftLinksPlugin = null;

        /**
         * A direct reference to the plugin for links that are shown when no aircraft is selected.
         */
        noAircraftLinksPlugin: AircraftLinksPlugin = null;

        /**
         * An instance of a link render helper that can manage a link to enable and disable auto-select.
         */
        autoSelectLinkRenderHelper: AutoSelectLinkRenderHelper = null;

        /**
         * An instance of a link render helper that can centre the map on the selected aircraft.
         */
        centreOnSelectedAircraftLinkRenderHandler: CentreOnSelectedAircraftLinkRenderHandler = null;

        /**
         * The hook result for the VRS.AircraftList.updated event.
         */
        aircraftListUpdatedHook: IEventHandle = undefined;

        /**
         * The hook result for the VRS.AircraftList.selectedAircraftChanged event.
         */
        selectedAircraftChangedHook: IEventHandle = undefined;

        /**
         * The hook result for the VRS.Localise.localeChanged event.
         */
        localeChangedHook: IEventHandle = undefined;

        /**
         * The hook result for the VRS.UnitDisplayPreferences.unitChanged event.
         */
        unitChangedHook: IEventHandle = undefined;

        /**
         * An array of VRS.AircraftDetailProperty objects that have been rendered into the detail panel's header.
         */
        headerProperties: AircraftDetailProperty[] = [];

        /**
         * An array of VRS.AircraftDetailProperty objects that have been rendered into the detail panel's body.
         */
        bodyProperties: AircraftDetailProperty[] = [];

        /**
         * The aircraft last rendered.
         */
        aircraftLastRendered: Aircraft = undefined;
    }

    /**
     * Describes a property that has been rendered into the aircraft detail panel.
     */
    class AircraftDetailProperty
    {
        handler: RenderPropertyHandler;
        contentElement: JQuery;

        constructor(property: RenderPropertyEnum, isHeader: boolean, public element: JQuery, options: AircraftRenderOptions)
        {
            if(!property) throw 'You must supply a RenderProperty';
            var handler = VRS.renderPropertyHandlers[property];
            if(!handler) throw 'The render property ' + property + ' has no handler declared for it';
            if(!handler.isSurfaceSupported(isHeader ? VRS.RenderSurface.DetailHead : VRS.RenderSurface.DetailBody)) throw 'The render property ' + property + ' does not support rendering on detail ' + (isHeader ? 'headers' : 'bodies');
            if(!element) throw 'You must supply a jQuery element';

            this.handler = handler;
            this.contentElement = isHeader ? element : element.children().first();

            handler.createWidgetInJQuery(this.contentElement, isHeader ? VRS.RenderSurface.DetailHead : VRS.RenderSurface.DetailBody, options);
        }
    }

    /**
     * The settings saved by AircraftDetailPlugin objects.
     */
    export interface AircraftDetailPlugin_SaveState
    {
        showUnits:  boolean;
        items:      RenderPropertyEnum[];
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftDetailPlugin = function(jQueryElement: JQuery) : AircraftDetailPlugin
    {
        return <AircraftDetailPlugin>jQueryElement.data('vrsVrsAircraftDetail');
    }
    VRS.jQueryUIHelper.getAircraftDetailOptions = function(overrides?: AircraftDetailPlugin_Options) : AircraftDetailPlugin_Options
    {
        return $.extend({
            name:                       'default',
            useSavedState:              true,
            showUnits:                  VRS.globalOptions.detailPanelDefaultShowUnits,
            items:                      VRS.globalOptions.detailPanelDefaultItems.slice(),
            showSeparateRouteLink:      VRS.globalOptions.detailPanelShowSeparateRouteLink,
            flagUncertainCallsigns:     VRS.globalOptions.detailPanelFlagUncertainCallsigns,
            distinguishOnGround:        VRS.globalOptions.detailPanelDistinguishOnGround,
            airportDataThumbnails:      VRS.globalOptions.detailPanelAirportDataThumbnails
        }, overrides);
    }

    /**
     * A plugin that can show the user some details about the currently selected aircraft.
     */
    export class AircraftDetailPlugin extends JQueryUICustomWidget implements ISelfPersist<AircraftDetailPlugin_SaveState>
    {
        options: AircraftDetailPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getAircraftDetailOptions();
        }

        private _getState() : AircraftDetailPlugin_State
        {
            var result = this.element.data('aircraftDetailPluginState');
            if(result === undefined) {
                result = new AircraftDetailPlugin_State();
                this.element.data('aircraftDetailPluginState', result);
            }

            return result;
        }

        _create()
        {
            var options = this.options;
            if(!options.aircraftList) throw 'An aircraft list must be supplied';
            if(!options.unitDisplayPreferences) throw 'A unit display preferences object must be supplied';

            if(options.useSavedState) {
                this.loadAndApplyState();
            }

            var state = this._getState();
            state.container =
                $('<div/>')
                    .addClass('vrsAircraftDetail aircraft')
                    .appendTo(this.element);
            state.headerContainer =
                $('<div/>')
                    .addClass('header')
                    .appendTo(state.container);
            state.bodyContainer =
                $('<div/>')
                    .addClass('body')
                    .appendTo(state.container);
            state.linksContainer =
                $('<div/>')
                    .addClass('links')
                    .appendTo(state.container);
            state.noAircraftContainer =
                $('<div/>')
                    .addClass('vrsAircraftDetail noAircraft')
                    .appendTo(this.element);

            this._buildContent(state);

            state.aircraftListUpdatedHook =     options.aircraftList.hookUpdated(this._aircraftListUpdated, this);
            state.selectedAircraftChangedHook = options.aircraftList.hookSelectedAircraftChanged(this._selectedAircraftChanged, this);
            state.localeChangedHook =           VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.unitChangedHook =             options.unitDisplayPreferences.hookUnitChanged(this._displayUnitChanged, this);

            this._renderContent(state, options.aircraftList.getSelectedAircraft(), true);
        }

        _destroy()
        {
            var state = this._getState();
            var options = this.options;

            state.headerProperties = this._destroyProperties(VRS.RenderSurface.DetailHead, state.headerProperties);
            state.bodyProperties =   this._destroyProperties(VRS.RenderSurface.DetailBody, state.bodyProperties);

            if(state.container) {
                this.element.empty();
                state.container = undefined;
            }
            if(state.aircraftLinksPlugin) {
                state.aircraftLinksPlugin.destroy();
                state.aircraftLinksPlugin = null;
            }
            if(state.routeLinksPlugin) {
                state.routeLinksPlugin.destroy();
                state.routeLinksPlugin = null;
            }
            if(state.autoSelectLinkRenderHelper) {
                state.autoSelectLinkRenderHelper.dispose();
                state.autoSelectLinkRenderHelper = null;
            }
            if(state.aircraftListUpdatedHook) {
                if(options && options.aircraftList) options.aircraftList.unhook(state.aircraftListUpdatedHook);
                state.aircraftListUpdatedHook = undefined;
            }
            if(state.selectedAircraftChangedHook) {
                if(options && options.aircraftList) options.aircraftList.unhook(state.selectedAircraftChangedHook);
                state.selectedAircraftChangedHook = undefined;
            }
            if(state.localeChangedHook) {
                if(VRS.globalisation) VRS.globalisation.unhook(state.localeChangedHook);
                state.localeChangedHook = undefined;
            }
            if(state.unitChangedHook) {
                if(options && options.unitDisplayPreferences) options.unitDisplayPreferences.unhook(state.unitChangedHook);
                state.unitChangedHook = undefined;
            }
        }

        /**
         * Destroys the elements and releases any resources held by a collection of properties.
         */
        private _destroyProperties(surface: RenderSurfaceBitFlags, properties: AircraftDetailProperty[])
        {
            var length = properties.length;
            for(var i = 0;i < length;++i) {
                var property = properties[i];
                if(property.element) {
                    property.handler.destroyWidgetInJQuery(property.contentElement, surface);
                    property.element.remove();
                }
            }

            return [];
        }

        /**
         * Suspends or resumes updates.
         */
        suspend(onOff: boolean)
        {
            onOff = !!onOff;
            var state = this._getState();
            if(state.suspended !== onOff) {
                state.suspended = onOff;
                this._suspendWidgets(state, onOff);

                if(!state.suspended) {
                    var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                    this._renderContent(state, selectedAircraft, true);
                }
            }
        }

        /**
         * Suspends or resumes widgets that have been used to render items.
         */
        private _suspendWidgets(state: AircraftDetailPlugin_State, onOff: boolean)
        {
            this._suspendWidgetProperties(state, onOff, state.headerProperties, VRS.RenderSurface.DetailHead);
            this._suspendWidgetProperties(state, onOff, state.bodyProperties, VRS.RenderSurface.DetailBody);
        }

        /**
         * Suspends or resumes widgets used to render an array of properties.
         */
        private _suspendWidgetProperties(state: AircraftDetailPlugin_State, onOff: boolean, properties: AircraftDetailProperty[], surface: RenderSurfaceBitFlags)
        {
            var length = properties.length;
            for(var i = 0;i < length;++i) {
                var property = properties[i];
                property.handler.suspendWidget(property.contentElement, surface, onOff);
            }
        }

        /**
         * Saves the current state.
         */
        saveState()
        {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        }

        /**
         * Returns the saved state or, if there is no saved state, the current state.
         */
        loadState() : AircraftDetailPlugin_SaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.items = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.items, [ VRS.RenderSurface.DetailBody ]);

            return result;
        }

        /**
         * Applies the saved state to the object.
         */
        applyState(settings: AircraftDetailPlugin_SaveState)
        {
            this.options.showUnits = settings.showUnits;
            this.options.items = settings.items;
        }

        /**
         * Loads and then applies the saved state.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key against which the state is saved.
         */
        private _persistenceKey() : string
        {
            return 'vrsAircraftDetailPlugin-' + this.options.name;
        }

        /**
         * Returns the current state.
         */
        private _createSettings() : AircraftDetailPlugin_SaveState
        {
            return {
                showUnits: this.options.showUnits,
                items: this.options.items
            };
        }

        /**
         * Returns the option panes that can be used to configure the panel.
         */
        createOptionPane(displayOrder: number) : OptionPane[]
        {
            var result: OptionPane[] = [];
            var state = this._getState();
            var options = this.options;

            var settingsPane = new VRS.OptionPane({
                name:           'vrsAircraftDetailPlugin_' + options.name + '_Settings',
                titleKey:       'PaneDetailSettings',
                displayOrder:   displayOrder,
                fields:         [
                    new VRS.OptionFieldCheckBox({
                        name:           'showUnits',
                        labelKey:       'ShowUnits',
                        getValue:       () => options.showUnits,
                        setValue:       (value) => {
                            options.showUnits = value;
                            this._buildContent(this._getState());
                            this._reRenderAircraft(this._getState())
                        },
                        saveState:      () => this.saveState()
                    })
                ]
            });
            result.push(settingsPane);

            if(VRS.globalOptions.detailPanelUserCanConfigureItems) {
                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane:       settingsPane,
                    fieldLabel: 'DetailItems',
                    surface:    VRS.RenderSurface.DetailBody,
                    getList:    () => options.items,
                    setList:    (items) => {
                        options.items = items;
                        this._buildBody(state);
                        this._reRenderAircraft(state);
                    },
                    saveState:  () => this.saveState()
                });
            }

            return result;
        }

        /**
         * Fills both the header and body with content.
         */
        private _buildContent(state: AircraftDetailPlugin_State)
        {
            this._buildHeader(state);
            this._buildBody(state);
            this._buildLinks(state);
        }

        /**
         * Empties the header and then prepares it with elements to render into.
         */
        private _buildHeader(state: AircraftDetailPlugin_State)
        {
            var options = this.options;

            state.headerProperties = this._destroyProperties(VRS.RenderSurface.DetailHead, state.headerProperties);
            state.headerContainer.empty();

            var table = $('<table/>')
                .appendTo(state.headerContainer),

                row1 = $('<tr/>')
                    .appendTo(table),
                regElement = $('<td/>')
                    .addClass('reg')
                    .appendTo(row1),
                icaoElement = $('<td/>')
                    .addClass('icao')
                    .appendTo(row1),
                bearingElement = $('<td/>')
                    .addClass('bearing')
                    .appendTo(row1),
                opFlagElement = $('<td/>')
                    .addClass('flag')
                    .appendTo(row1),

                row2 = $('<tr/>')
                    .appendTo(table),
                operatorElement = $('<td/>')
                    .addClass('op')
                    .attr('colspan', '3')
                    .appendTo(row2),
                callsignElement = $('<td/>')
                    .addClass('callsign')
                    .appendTo(row2),

                row3 = $('<tr/>')
                    .appendTo(table),
                countryElement = $('<td/>')
                    .addClass('country')
                    .attr('colspan', '3')
                    .appendTo(row3),
                militaryElement = $('<td/>')
                    .addClass('military')
                    .appendTo(row3),

                row4 = $('<tr/>')
                    .appendTo(table),
                modelElement = $('<td/>')
                    .addClass('model')
                    .attr('colspan', '3')
                    .appendTo(row4),
                modelTypeElement = $('<td/>')
                    .addClass('modelType')
                    .appendTo(row4);

            state.headerProperties = [
                new AircraftDetailProperty(VRS.RenderProperty.Registration, true, regElement,       options),
                new AircraftDetailProperty(VRS.RenderProperty.Icao,         true, icaoElement,      options),
                new AircraftDetailProperty(VRS.RenderProperty.Bearing,      true, bearingElement,   options),
                new AircraftDetailProperty(VRS.RenderProperty.OperatorFlag, true, opFlagElement,    options),
                new AircraftDetailProperty(VRS.RenderProperty.Operator,     true, operatorElement,  options),
                new AircraftDetailProperty(VRS.RenderProperty.Callsign,     true, callsignElement,  options),
                new AircraftDetailProperty(VRS.RenderProperty.Country,      true, countryElement,   options),
                new AircraftDetailProperty(VRS.RenderProperty.CivOrMil,     true, militaryElement,  options),
                new AircraftDetailProperty(VRS.RenderProperty.Model,        true, modelElement,     options),
                new AircraftDetailProperty(VRS.RenderProperty.ModelIcao,    true, modelTypeElement, options)
            ];
        }

        /**
         * Empties the body and then prepares it with elements to render into.
         */
        private _buildBody(state: AircraftDetailPlugin_State)
        {
            state.bodyProperties = this._destroyProperties(VRS.RenderSurface.DetailBody, state.bodyProperties);
            state.bodyContainer.empty();
            var options = this.options;

            var list =
                $('<ul/>')
                    .appendTo(state.bodyContainer);

            state.bodyProperties = [];
            var countProperties = options.items.length;
            for(var i = 0;i < countProperties;++i) {
                var property = options.items[i];
                var handler = VRS.renderPropertyHandlers[property];
                var suppressLabel = handler.suppressLabelCallback(VRS.RenderSurface.DetailBody);

                var listItem =
                    $('<li/>')
                        .appendTo(list);
                if(!suppressLabel) {
                    $('<div/>')
                        .addClass('label')
                        .append($('<span/>')
                            .text(VRS.globalisation.getText(handler.labelKey) + ':')
                        )
                        .appendTo(listItem);
                }
                var content =
                    $('<div/>')
                        .addClass('content')
                        .append($('<span/>'))
                        .appendTo(listItem);
                if(suppressLabel) {
                    listItem.addClass('wide');
                    content.addClass('wide');
                }
                if(handler.isMultiLine) {
                    listItem.addClass('multiline');
                    content.addClass('multiline');
                }
                state.bodyProperties.push(new AircraftDetailProperty(property, false, content, options));
            }
        }

        /**
         * Adds the UI for the aircraft links.
         */
        private _buildLinks(state: AircraftDetailPlugin_State)
        {
            var options = this.options;

            if(state.aircraftLinksPlugin) {
                state.aircraftLinksPlugin.destroy();
            }
            if(state.noAircraftLinksPlugin) {
                state.noAircraftLinksPlugin.destroy();
            }
            state.linksContainer.empty();
            state.noAircraftContainer.empty();

            var aircraftLinksElement = $('<div/>')
                .appendTo(state.linksContainer)
                .vrsAircraftLinks();
            state.aircraftLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(aircraftLinksElement);

            var routeLinks: (LinkSiteEnum | LinkRenderHandler)[] = [];

            if(options.mapPlugin && options.aircraftList && VRS.globalOptions.detailPanelShowCentreOnAircraft) {
                state.centreOnSelectedAircraftLinkRenderHandler = new VRS.CentreOnSelectedAircraftLinkRenderHandler(options.aircraftList, options.mapPlugin);
                routeLinks.push(state.centreOnSelectedAircraftLinkRenderHandler);
            }

            if(options.aircraftAutoSelect && VRS.globalOptions.detailPanelShowEnableAutoSelect) {
                state.autoSelectLinkRenderHelper = new VRS.AutoSelectLinkRenderHelper(options.aircraftAutoSelect);
                routeLinks.push(state.autoSelectLinkRenderHelper);
            }
            if(VRS.globalOptions.detailPanelShowSeparateRouteLink) {
                routeLinks.push(VRS.LinkSite.StandingDataMaintenance);
            }

            if(routeLinks.length > 0) {
                var routeLinksElement = $('<div/>')
                    .appendTo(state.linksContainer)
                    .vrsAircraftLinks({
                        linkSites: routeLinks
                    });
                state.routeLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(routeLinksElement);

                if(state.autoSelectLinkRenderHelper) state.autoSelectLinkRenderHelper.addLinksRendererPlugin(state.routeLinksPlugin);
            }

            if(options.aircraftAutoSelect && VRS.globalOptions.detailPanelShowEnableAutoSelect) {
                var noAircraftLinksElement = $('<div/>')
                    .appendTo(state.noAircraftContainer)
                    .vrsAircraftLinks({
                        linkSites: [ state.autoSelectLinkRenderHelper ]
                    });
                state.noAircraftLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(noAircraftLinksElement);
                state.autoSelectLinkRenderHelper.addLinksRendererPlugin(state.noAircraftLinksPlugin);
            }
        }

        /**
         * Updates the header and body with any values on the aircraft that have changed. If displayUnit is provided then only properties that
         * depend on the unit are refreshed.
         */
        private _renderContent(state: AircraftDetailPlugin_State, aircraft: Aircraft, refreshAll: boolean, displayUnit?: DisplayUnitDependencyEnum)
        {
            state.aircraftLastRendered = aircraft;
            if(!aircraft) {
                $(state.container, ':visible').hide();
                $(state.noAircraftContainer, ':hidden').show();
                this._renderLinks(state, null, refreshAll);
            } else {
                $(state.container, ':hidden').show();
                $(state.noAircraftContainer, ':visible').hide();

                this._renderProperties(state, aircraft, refreshAll, state.headerProperties, VRS.RenderSurface.DetailHead, displayUnit);
                this._renderProperties(state, aircraft, refreshAll, state.bodyProperties, VRS.RenderSurface.DetailBody, displayUnit);
                this._renderLinks(state, aircraft, refreshAll);
            }
        }

        /**
         * Renders the values into a set of existing elements on a surface. If displayUnit is provided then only properties that
         * depend on the unit are refreshed.
         */
        private _renderProperties(state: AircraftDetailPlugin_State, aircraft: Aircraft, refreshAll: boolean, properties: AircraftDetailProperty[], surface: RenderSurfaceBitFlags, displayUnit?: DisplayUnitDependencyEnum)
        {
            var options = this.options;
            var countProperties = properties.length;

            var refreshedContent = false;
            for(var i = 0;i < countProperties;++i) {
                var property = properties[i];
                var handler = property.handler;
                var element = property.contentElement;

                var forceRefresh = refreshAll;
                if(!forceRefresh) forceRefresh = displayUnit && handler.usesDisplayUnit(displayUnit);

                if(forceRefresh || handler.hasChangedCallback(aircraft)) {
                    handler.renderToJQuery(element, surface, aircraft, options);
                    refreshedContent = true;
                }
                if(forceRefresh || handler.tooltipChangedCallback(aircraft)) {
                    handler.renderTooltipToJQuery(element, surface, aircraft, options);
                }
            }
        }

        /**
         * Renders the links for the aircraft.
         */
        private _renderLinks(state: AircraftDetailPlugin_State, aircraft: Aircraft, refreshAll: boolean)
        {
            if(aircraft === null) {
                if(state.noAircraftLinksPlugin) {
                    state.noAircraftLinksPlugin.renderForAircraft(null, refreshAll);
                }
            } else {
                if(VRS.globalOptions.detailPanelShowAircraftLinks) {
                    state.aircraftLinksPlugin.renderForAircraft(aircraft, refreshAll);
                }
                if(state.routeLinksPlugin) {
                    state.routeLinksPlugin.renderForAircraft(aircraft, refreshAll);
                }
            }
        }

        /**
         * Forces the drawing of every item in both the header and body.
         */
        private _reRenderAircraft(state: AircraftDetailPlugin_State)
        {
            this._renderContent(state, state.aircraftLastRendered, true);
        }

        /**
         * Called when the aircraft list has been updated.
         */
        private _aircraftListUpdated()
        {
            var state = this._getState();
            if(!state.suspended) {
                var options = this.options;
                var selectedAircraft = options.aircraftList.getSelectedAircraft();
                if(selectedAircraft) this._renderContent(state, selectedAircraft, false);
            }
        }

        /**
         * Called when the user changes the display units.
         */
        private _displayUnitChanged(displayUnit: DisplayUnitDependencyEnum)
        {
            var state = this._getState();
            if(!state.suspended) {
                var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                if(selectedAircraft) this._renderContent(state, selectedAircraft, false, displayUnit);
            }
        }

        /**
         * Called when the selected aircraft has changed.
         */
        private _selectedAircraftChanged()
        {
            var state = this._getState();
            if(!state.suspended) {
                var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                this._renderContent(state, selectedAircraft, true);
            }
        }

        /**
         * Called when the language has changed.
         */
        private _localeChanged()
        {
            var state = this._getState();
            this._buildContent(state);
            if(!state.suspended) this._reRenderAircraft(state);
        }
    }

    $.widget('vrs.vrsAircraftDetail', new AircraftDetailPlugin());
}

declare interface JQuery
{
    vrsAircraftDetail();
    vrsAircraftDetail(options: VRS.AircraftDetailPlugin_Options);
    vrsAircraftDetail(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
