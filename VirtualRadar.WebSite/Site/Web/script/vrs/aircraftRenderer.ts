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
 * @fileoverview Code that can handle the translation of aircraft properties into text or HTML.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftRendererEnableDebugProperties = VRS.globalOptions.aircraftRendererEnableDebugProperties !== undefined ? VRS.globalOptions.aircraftRendererEnableDebugProperties : false;  // True if the debug aircraft properties that expose some internal values are switched on. This needs to be changed BEFORE aircraftRenderer.js is loaded by the browser, it has no effect after.

    /**
     * The options that can be passed into an aircraft property renderer.
     */
    export interface AircraftRenderOptions
    {
        unitDisplayPreferences:         UnitDisplayPreferences;
        distinguishOnGround?:           boolean;
        flagUncertainCallsigns?:        boolean;
        showUnits?:                     boolean;
        suppressRouteCorrectionLinks?:  boolean;
        airportDataThumbnails?:         number;
        plotterOptions?:                AircraftPlotterOptions;
        mirrorMapJQ?:                   JQuery;
        aircraftList?:                  AircraftList;
    }

    /**
     * The settings that need to be passed to new instances of RenderPropertyHandler.
     */
    export interface RenderPropertyHandler_Settings
    {
        //
        /**
         * The VRS.RenderProperty enum value for the aircraft property that this handler can deal with.
         */
        property: RenderPropertyEnum;

        /**
         * A combination of VRS.RenderSurface bitflags that describe which surfaces this handler can render the property onto.
         */
        surfaces: RenderSurfaceBitFlags;

        /**
         * The key into VRS.$$ for the text to display for column headings. Uses the labelKey if not supplied.
         */
        headingKey?: string;

        /**
         * The key into VRS.$$ for the text to display for full display labels. Uses the headingKey if not supplied.
         */
        labelKey?: string;

        /**
         * The key into VRS.$$ for the text to display for configuration UI labels. Uses either headingKey or labelKey if not supplied.
         */
        optionsLabelKey?: string;

        /**
         * The VRS.Alignment enum describing the alignment for column headings - uses alignment if not supplied.
         */
        headingAlignment?: AlignmentEnum;

        /**
         * The VRS.Alignment enum describing the alignment for values in columns - uses alignment if not supplied.
         */
        contentAlignment?: AlignmentEnum;

        /**
         * The VRS.Alignment enum describing the alignment for headings and column values.
         */
        alignment?: AlignmentEnum;

        /**
         * An optional fixed width as a CSS width string (e.g. '20px' or '6em') when rendering within columns.
         */
        fixedWidth?: (surface: RenderSurfaceBitFlags) => string;

        /**
         * Returns true if the content of the property changed in the last update.
         */
        hasChangedCallback?: (aircraft?: Aircraft) => boolean;
        
        /**
         * Takes an aircraft, options object and surface and returns the text content that represents the property.
         */
        contentCallback?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;

        /**
         * Takes an aircraft, options object and surface and returns HTML that represents the property.
         */
        renderCallback?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;

        /**
         * Takes an aircraft, options object and surface and returns true if the renderCallback should be used or false if the contentCallback should be used. Only supply this if you supply BOTH contentCallback and renderCallback.
         */
        useHtmlRendering?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => boolean;

        /**
         * Takes a VRS.DisplayUnitDependency enum and returns true if the property display depends upon it. Defaults to method that returns false.
         */
        usesDisplayUnit?: (displayUnitDependency: DisplayUnitDependencyEnum) => boolean;

        /**
         * Returns true if the tooltip has changed in the last update. Defaults to hasChangedCallback if not supplied.
         */
        tooltipChangedCallback?: (aircraft?: Aircraft) => boolean;

        /**
         * Takes an aircraft, options object and render surface and returns the tooltip text for the property. If not supplied then the property has no tooltip.
         */
        tooltipCallback?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;

        /**
         * Takes a VRS.RenderSurface and returns true if the label should not be rendered on the surface. Defaults to a method that returns false.
         */
        suppressLabelCallback?: (surface: RenderSurfaceBitFlags) => boolean;

        /**
         * True if the content takes more than one line of text to display, false if it does not. Defaults to false.
         */
        isMultiLine?: boolean;

        /**
         * An optional reference to the sortable field for the property.
         */
        sortableField?: AircraftListSortableFieldEnum;

        /**
         * Widget support - called after an element has been created for the render property, allows the renderer to add a widget to the element.
         */
        createWidget?: (element?: JQuery, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;

        /**
         * Widget support - renders the property into a widget.
         */
        renderWidget?: (element?: JQuery, aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;

        /**
         * Destroys the widget attached to the element.
         */
        destroyWidget?: (element?: JQuery, surface?: RenderSurfaceBitFlags) => void;

        /**
         * Passed true if updates are being suspended and false if updates are being resumed.
         */
        suspendWidget?: (element: JQuery, surface: RenderSurfaceBitFlags, suspend: boolean) => void;
     }

    /**
     * A class that brings together everything needed to present a piece of information about an aircraft.
     */
    export class RenderPropertyHandler
    {
        private _SuspendWidget: (element: JQuery, surface: RenderSurfaceBitFlags, suspend: boolean) => void;

        // Keeping these as public fields for backwards compatibility
        property:               RenderPropertyEnum;
        surfaces:               RenderSurfaceBitFlags;
        headingKey:             string;
        labelKey:               string;
        optionsLabelKey:        string;
        headingAlignment:       AlignmentEnum;
        contentAlignment:       AlignmentEnum;
        fixedWidth:             (surface: RenderSurfaceBitFlags) => string;
        hasChangedCallback:     (aircraft?: Aircraft) => boolean;
        contentCallback:        (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        renderCallback:         (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        useHtmlRendering:       (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => boolean;
        usesDisplayUnit:        (displayUnitDependency: DisplayUnitDependencyEnum) => boolean;
        tooltipChangedCallback: (aircraft?: Aircraft) => boolean;
        tooltipCallback:        (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        suppressLabelCallback:  (surface: RenderSurfaceBitFlags) => boolean;
        isMultiLine:            boolean;
        sortableField:          AircraftListSortableFieldEnum;
        createWidget:           (element?: JQuery, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;
        renderWidget:           (element?: JQuery, aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;
        destroyWidget:          (element?: JQuery, surface?: RenderSurfaceBitFlags) => void;

        constructor(settings: RenderPropertyHandler_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(!settings.property) throw 'The settings must specify the property';
            if(!settings.surfaces) throw 'The settings must specify the surfaces flags';
            if(!settings.hasChangedCallback) throw 'The settings must specify a hasChanged callback';
            if(!settings.headingKey && !settings.labelKey) throw 'The settings must specify either a heading or a label key';
            if(!settings.alignment) settings.alignment = VRS.Alignment.Left;
            if(!settings.tooltipChangedCallback) settings.tooltipChangedCallback = settings.hasChangedCallback;

            this.property = settings.property;
            this.surfaces = settings.surfaces;
            this.headingKey = settings.headingKey || settings.labelKey;
            this.labelKey = settings.labelKey || settings.headingKey;
            this.optionsLabelKey = settings.optionsLabelKey || settings.labelKey;
            this.headingAlignment = settings.headingAlignment || settings.alignment;
            this.contentAlignment = settings.contentAlignment || settings.alignment;
            this.fixedWidth = settings.fixedWidth || function() { return null; };
            this.hasChangedCallback = settings.hasChangedCallback;
            this.contentCallback = settings.contentCallback;
            this.renderCallback = settings.renderCallback;
            this.useHtmlRendering = settings.useHtmlRendering || function() { return !!settings.renderCallback; };
            this.usesDisplayUnit = settings.usesDisplayUnit || function() { return false; };
            this.tooltipChangedCallback = settings.tooltipChangedCallback;
            this.tooltipCallback = settings.tooltipCallback;
            this.suppressLabelCallback = settings.suppressLabelCallback || function() { return false; };
            this.isMultiLine = !!settings.isMultiLine;
            this.sortableField = settings.sortableField || VRS.AircraftListSortableField.None;
            this.createWidget = settings.createWidget;
            this.renderWidget = settings.renderWidget;
            this.destroyWidget = settings.destroyWidget;

            this._SuspendWidget = settings.suspendWidget;
        }

        /**
         * Returns true if the property can be rendered onto the VRS.RenderSurface passed across.
         */
        isSurfaceSupported(surface: RenderSurfaceBitFlags) : boolean
        {
            return (this.surfaces & surface) !== 0;
        }

        /**
         * Returns true if the property is rendered using a jQuery UI widget.
         */
        isWidgetProperty() : boolean
        {
            return !!this.createWidget;
        }

        /**
         * Called when a widget is being suspended or resumed.
         */
        suspendWidget(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, onOff: boolean)
        {
            if(this._SuspendWidget) {
                this._SuspendWidget(jQueryElement, surface, onOff);
            }
        }

        /**
         * Call after creating an element for a property - this lets renderers that use a jQuery widget create their widget.
         */
        createWidgetInDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags, options: AircraftRenderOptions)
        {
            if(this.isWidgetProperty()) {
                this.createWidget($(domElement), options, surface);
            }
        }

        /**
         * Call after creating an element for a property - this lets renderers that use a jQuery widget create their widget.
         */
        createWidgetInJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, options?: AircraftRenderOptions)
        {
            if(this.isWidgetProperty()) {
                this.createWidget(jQueryElement, options, surface);
            }
        }

        /**
         * Call before destroying an element for a property - this lets renderers that use a jQuery widget destroy their widget.
         */
        destroyWidgetInDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags)
        {
            if(this.isWidgetProperty()) {
                this.destroyWidget($(domElement), surface);
            }
        }

        /**
         * Call before destroying an element for a property - this lets renderers that use a jQuery widget destroy their widget.
         */
        destroyWidgetInJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags)
        {
            if(this.isWidgetProperty()) {
                this.destroyWidget(jQueryElement, surface);
            }
        }

        /**
         * Renders a property to a native DOM element and returns the new content of the element.
         */
        renderToDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions, compareContent?: boolean, existingContent?: string) : string
        {
            var newContent: string;

            if(this.useHtmlRendering(aircraft, options, surface)) {
                newContent = this.renderCallback(aircraft, options, surface);
                if(!compareContent || newContent !== existingContent) {
                    domElement.innerHTML = newContent;
                }
            } else if(this.isWidgetProperty()) {
                this.renderWidget($(domElement), aircraft, options, surface);
            } else {
                newContent = this.contentCallback(aircraft, options, surface);
                if(!compareContent || newContent !== existingContent) {
                    domElement.textContent = newContent;
                }
            }

            return newContent;
        }

        /**
         * Renders a property to a jQuery element.
         */
        renderToJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions)
        {
            if(this.useHtmlRendering(aircraft, options, surface)) {
                jQueryElement.html(this.renderCallback(aircraft, options, surface));
            } else if(this.isWidgetProperty()) {
                this.renderWidget(jQueryElement, aircraft, options, surface);
            } else {
                jQueryElement.text(this.contentCallback(aircraft, options, surface));
            }
        }

        /**
         * Renders the tooltip for the aircraft to the DOM element. Returns true if the element now has a tooltip.
         */
        renderTooltipToDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions) : boolean
        {
            var tooltipText = this.tooltipCallback ? this.tooltipCallback(aircraft, options, surface) || '' : '';
            if(tooltipText) {
                domElement.setAttribute('title', tooltipText);
            } else {
                domElement.removeAttribute('title');
            }

            return !!tooltipText;
        }

        /**
         * Renders the tooltip for the aircraft to the DOM element. Returns true if the element now has a tooltip.
         */
        renderTooltipToJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions)
        {
            var tooltipText = this.tooltipCallback ? this.tooltipCallback(aircraft, options, surface) || '' : '';
            jQueryElement.prop('title', tooltipText);

            return !!tooltipText;
        }
    }

    /**
     * The associative array of VRS.RenderPropertyHandler objects indexed by VRS.RenderProperty.
     * @type {Object.<VRS.RenderProperty, VRS.RenderPropertyHandler>}
     */
    export var renderPropertyHandlers: { [index: string /* RenderPropertyEnum */]: RenderPropertyHandler } = VRS.renderPropertyHandlers || {};


    /*
     * NORMAL PROPERTY HANDLERS
     */


    VRS.renderPropertyHandlers[VRS.RenderProperty.AirportDataThumbnails] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.AirportDataThumbnails,
        surfaces:               VRS.RenderSurface.DetailBody,
        labelKey:               'AirportDataThumbnails',
        alignment:              VRS.Alignment.Centre,
        hasChangedCallback:     function(aircraft) { return aircraft.icao.chg || aircraft.airportDataThumbnails.chg; },
        suppressLabelCallback:  function(surface) { return true; },
        renderCallback:         function(aircraft, options) {
            var result = '';
            if(!aircraft.airportDataThumbnails.chg) {
                aircraft.fetchAirportDataThumbnails(options.airportDataThumbnails);
            } else {
                result = aircraft.formatAirportDataThumbnails(true);
            }
            return result;
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Altitude] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Altitude,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListAltitude',
        labelKey:               'Altitude',
        sortableField:          VRS.AircraftListSortableField.Altitude,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.altitude.chg || aircraft.isOnGround.chg; },
        contentCallback:        function(aircraft, options) {
            return aircraft.formatAltitude(
                options.unitDisplayPreferences.getHeightUnit(),
                options.distinguishOnGround,
                options.showUnits,
                options.unitDisplayPreferences.getShowAltitudeType()
            );
        },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Height; }
    });

    /*
    VRS.renderPropertyHandlers[VRS.RenderProperty.AltitudeAndSpeedGraph] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.AltitudeAndSpeedGraph,
        surfaces:               VRS.RenderSurface.DetailBody,
        labelKey:               'AltitudeAndSpeedGraph',
        isMultiLine:            true,
        hasChangedCallback:     function(aircraft) { return aircraft.altitude.chg || aircraft.speed.chg; },
        contentCallback:        function() { return '[ Altitude and speed graph goes here - one day :) ]'}
    });
    */

    VRS.renderPropertyHandlers[VRS.RenderProperty.AltitudeAndVerticalSpeed] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.AltitudeAndVerticalSpeed,
        surfaces:               VRS.RenderSurface.List,
        headingKey:             'ListAltitudeAndVerticalSpeed',
        labelKey:               'AltitudeAndVerticalSpeed',
        sortableField:          VRS.AircraftListSortableField.Altitude,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.altitude.chg || aircraft.isOnGround.chg || aircraft.verticalSpeed.chg; },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Height || displayUnitDependency === VRS.DisplayUnitDependency.VsiSeconds; },
        renderCallback:         function(aircraft, options) {
            return VRS.format.stackedValues(
                aircraft.formatAltitude(options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits, options.unitDisplayPreferences.getShowAltitudeType()),
                aircraft.formatVerticalSpeed(options.unitDisplayPreferences.getHeightUnit(), options.unitDisplayPreferences.getShowVerticalSpeedPerSecond(), options.showUnits, options.unitDisplayPreferences.getShowVerticalSpeedType())
            );
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.AltitudeType] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.AltitudeType,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListAltitudeType',
        labelKey:               'AltitudeType',
        sortableField:          VRS.AircraftListSortableField.AltitudeType,
        hasChangedCallback:     function(aircraft) { return aircraft.altitudeType.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatAltitudeType(); }
    });

    /*
    VRS.renderPropertyHandlers[VRS.RenderProperty.AltitudeGraph] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.AltitudeGraph,
        surfaces:               VRS.RenderSurface.DetailBody,
        labelKey:               'AltitudeGraph',
        isMultiLine:            true,
        hasChangedCallback:     function(aircraft) { return aircraft.altitude.chg; },
        contentCallback:        function() { return '[ Altitude graph goes here - one day :) ]'}
    });
    */

    VRS.renderPropertyHandlers[VRS.RenderProperty.AverageSignalLevel] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.AverageSignalLevel,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListAverageSignalLevel',
        labelKey:               'AverageSignalLevel',
        alignment:              VRS.Alignment.Right,
        sortableField:          VRS.AircraftListSortableField.AverageSignalLevel,
        hasChangedCallback:     function(aircraft) { return aircraft.averageSignalLevel.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatAverageSignalLevel(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Bearing] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Bearing,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListBearing',
        labelKey:               'Bearing',
        sortableField:          VRS.AircraftListSortableField.Bearing,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.bearingFromHere.chg; },
        useHtmlRendering:       function(aircraft, options, surface) { return surface === VRS.RenderSurface.DetailHead; },
        contentCallback:        function(aircraft, options, surface) {
            switch(surface) {
                case VRS.RenderSurface.List:
                case VRS.RenderSurface.DetailBody:
                case VRS.RenderSurface.InfoWindow:
                    return aircraft.formatBearingFromHere(options.showUnits);
                default:
                    throw 'Unexpected surface ' + surface;
            }
        },
        renderCallback:         function(aircraft, options, surface) {
            switch(surface) {
                case VRS.RenderSurface.DetailHead:
                    return aircraft.formatBearingFromHereImage();
                default:
                    throw 'Unexpected surface ' + surface;
            }
        },
        tooltipCallback:        function(aircraft, options, surface) {
            switch(surface) {
                case VRS.RenderSurface.DetailHead:
                    return aircraft.formatBearingFromHere(options.showUnits);
                default:
                    return '';
            }
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Callsign] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Callsign,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListCallsign',
        labelKey:               'Callsign',
        sortableField:          VRS.AircraftListSortableField.Callsign,
        hasChangedCallback:     function(aircraft) { return aircraft.callsign.chg || aircraft.callsignSuspect.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatCallsign(options.flagUncertainCallsigns); },
        tooltipChangedCallback: function(aircraft) { return aircraft.hasRouteChanged() || aircraft.callsignSuspect.chg; },
        tooltipCallback:        function(aircraft, options) {
            var result = aircraft.formatRouteFull();
            if(options.flagUncertainCallsigns && aircraft.callsignSuspect.val) result += '. ' + VRS.$$.CallsignMayNotBeCorrect + '.';
            return result;
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.CallsignAndShortRoute] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.CallsignAndShortRoute,
        surfaces:               VRS.RenderSurface.List,
        headingKey:             'ListCallsign',
        labelKey:               'CallsignAndShortRoute',
        sortableField:          VRS.AircraftListSortableField.Callsign,
        hasChangedCallback:     function(aircraft) { return aircraft.callsign.chg || aircraft.callsignSuspect.chg || aircraft.hasRouteChanged(); },
        renderCallback:         function(aircraft, options) {
            return VRS.format.stackedValues(
                aircraft.formatCallsign(options.flagUncertainCallsigns),
                aircraft.formatRouteShort()
            );
        },
        tooltipChangedCallback: function(aircraft) { return aircraft.hasRouteChanged() || aircraft.callsignSuspect.chg; },
        tooltipCallback:        function(aircraft, options) {
            var result = aircraft.formatRouteFull();
            if(options.flagUncertainCallsigns && aircraft.callsignSuspect.val) result += '. ' + VRS.$$.CallsignMayNotBeCorrect + '.';
            return result;
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.CivOrMil] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.CivOrMil,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListCivOrMil',
        labelKey:               'CivilOrMilitary',
        sortableField:          VRS.AircraftListSortableField.CivOrMil,
        hasChangedCallback:     function(aircraft) { return aircraft.isMilitary.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatIsMilitary(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.CountMessages] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.CountMessages,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListCountMessages',
        labelKey:               'MessageCount',
        sortableField:          VRS.AircraftListSortableField.CountMessages,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.countMessages.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatCountMessages(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Country] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Country,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListCountry',
        labelKey:               'Country',
        sortableField:          VRS.AircraftListSortableField.Country,
        hasChangedCallback:     function(aircraft) { return aircraft.country.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatCountry(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Distance] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Distance,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListDistance',
        labelKey:               'Distance',
        sortableField:          VRS.AircraftListSortableField.Distance,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.distanceFromHereKm.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatDistanceFromHere(options.unitDisplayPreferences.getDistanceUnit(), options.showUnits); },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Distance; }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Engines] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Engines,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListEngines',
        labelKey:               'Engines',
        hasChangedCallback:     function(aircraft) { return aircraft.engineType.chg || aircraft.countEngines.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatEngines(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.FlightLevel] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.FlightLevel,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListFlightLevel',
        labelKey:               'FlightLevel',
        sortableField:          VRS.AircraftListSortableField.Altitude,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.altitude.chg || aircraft.isOnGround.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatFlightLevel(
            options.unitDisplayPreferences.getFlightLevelTransitionAltitude(),
            options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(),
            options.unitDisplayPreferences.getFlightLevelHeightUnit(),
            options.unitDisplayPreferences.getHeightUnit(),
            options.distinguishOnGround,
            options.showUnits,
            options.unitDisplayPreferences.getShowAltitudeType()
        ); },
        usesDisplayUnit:        function(displayUnitDependency) {
            switch(displayUnitDependency) {
                case VRS.DisplayUnitDependency.Height:
                case VRS.DisplayUnitDependency.FLHeightUnit:
                case VRS.DisplayUnitDependency.FLTransitionAltitude:
                case VRS.DisplayUnitDependency.FLTransitionHeightUnit:
                    return true;
            }
            return false;
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.FlightLevelAndVerticalSpeed] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.FlightLevelAndVerticalSpeed,
        surfaces:               VRS.RenderSurface.List,
        headingKey:             'ListFlightLevelAndVerticalSpeed',
        labelKey:               'FlightLevelAndVerticalSpeed',
        sortableField:          VRS.AircraftListSortableField.Altitude,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.altitude.chg || aircraft.isOnGround.chg || aircraft.verticalSpeed.chg; },
        renderCallback:         function(aircraft, options) {
            return VRS.format.stackedValues(
                aircraft.formatFlightLevel(
                    options.unitDisplayPreferences.getFlightLevelTransitionAltitude(),
                    options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(),
                    options.unitDisplayPreferences.getFlightLevelHeightUnit(),
                    options.unitDisplayPreferences.getHeightUnit(),
                    options.distinguishOnGround,
                    options.showUnits,
                    options.unitDisplayPreferences.getShowAltitudeType()
                ),
                aircraft.formatVerticalSpeed(
                    options.unitDisplayPreferences.getHeightUnit(),
                    options.unitDisplayPreferences.getShowVerticalSpeedPerSecond(),
                    options.showUnits,
                    options.unitDisplayPreferences.getShowVerticalSpeedType()
                )
            );
        },
        usesDisplayUnit:        function(displayUnitDependency) {
            switch(displayUnitDependency) {
                case VRS.DisplayUnitDependency.Height:
                case VRS.DisplayUnitDependency.FLHeightUnit:
                case VRS.DisplayUnitDependency.FLTransitionAltitude:
                case VRS.DisplayUnitDependency.FLTransitionHeightUnit:
                case VRS.DisplayUnitDependency.VsiSeconds:
                    return true;
            }
            return false;
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.FlightsCount] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.FlightsCount,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListFlightsCount',
        labelKey:               'FlightsCount',
        sortableField:          VRS.AircraftListSortableField.FlightsCount,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.countFlights.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatCountFlights(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Heading] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Heading,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListHeading',
        labelKey:               'Heading',
        sortableField:          VRS.AircraftListSortableField.Heading,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.heading.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatHeading(options.showUnits, options.unitDisplayPreferences.getShowTrackType()); },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Angle; }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.HeadingType] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.HeadingType,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListHeadingType',
        labelKey:               'HeadingType',
        sortableField:          VRS.AircraftListSortableField.HeadingType,
        hasChangedCallback:     function(aircraft) { return aircraft.headingIsTrue.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatHeadingType(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Icao] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Icao,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListIcao',
        labelKey:               'Icao',
        sortableField:          VRS.AircraftListSortableField.Icao,
        hasChangedCallback:     function(aircraft) { return aircraft.icao.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatIcao(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Interesting] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Interesting,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListInteresting',
        labelKey:               'Interesting',
        hasChangedCallback:     function(aircraft) { return aircraft.userInterested.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatUserInterested(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Latitude] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Latitude,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListLatitude',
        labelKey:               'Latitude',
        sortableField:          VRS.AircraftListSortableField.Latitude,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.latitude.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatLatitude(options.showUnits); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Longitude] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Longitude,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListLongitude',
        labelKey:               'Longitude',
        sortableField:          VRS.AircraftListSortableField.Longitude,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.longitude.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatLongitude(options.showUnits); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Manufacturer] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Manufacturer,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListManufacturer',
        labelKey:               'Manufacturer',
        sortableField:          VRS.AircraftListSortableField.Manufacturer,
        hasChangedCallback:     function(aircraft) { return aircraft.manufacturer.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatManufacturer(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Mlat] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Mlat,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListMlat',
        labelKey:               'Mlat',
        hasChangedCallback:     function(aircraft) { return aircraft.isMlat.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatIsMlat(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Model] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Model,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListModel',
        labelKey:               'Model',
        sortableField:          VRS.AircraftListSortableField.Model,
        hasChangedCallback:     function(aircraft) { return aircraft.model.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatModel(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.ModelIcao] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.ModelIcao,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListModelIcao',
        labelKey:               'ModelIcao',
        sortableField:          VRS.AircraftListSortableField.ModelIcao,
        hasChangedCallback:     function(aircraft) { return aircraft.modelIcao.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatModelIcao(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.None] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.None,
        surfaces:               VRS.RenderSurface.Marker,
        headingKey:             'None',
        labelKey:               'None',
        hasChangedCallback:     function() { return false; },
        contentCallback:        function() { return ''; }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Operator] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Operator,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListOperator',
        labelKey:               'Operator',
        sortableField:          VRS.AircraftListSortableField.Operator,
        hasChangedCallback:     function(aircraft) { return aircraft.operator.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatOperator(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.OperatorFlag] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.OperatorFlag,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListOperatorFlag',
        labelKey:               'OperatorFlag',
        sortableField:          VRS.AircraftListSortableField.OperatorIcao,
        headingAlignment:       VRS.Alignment.Centre,
        suppressLabelCallback:  function() { return true; },
        fixedWidth:             function() { return VRS.globalOptions.aircraftOperatorFlagSize.width.toString() + 'px'; },
        hasChangedCallback:     function(aircraft) { return aircraft.operatorIcao.chg || aircraft.icao.chg || aircraft.registration.chg; },
        renderCallback:         function(aircraft) { return aircraft.formatOperatorIcaoImageHtml(); },
        tooltipChangedCallback: function(aircraft) { return aircraft.operatorIcao.chg || aircraft.operator.chg; },
        tooltipCallback:        function(aircraft) { return aircraft.formatOperatorIcaoAndName(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.OperatorIcao] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.OperatorIcao,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListOperatorIcao',
        labelKey:               'OperatorCode',
        sortableField:          VRS.AircraftListSortableField.OperatorIcao,
        hasChangedCallback:     function(aircraft) { return aircraft.operatorIcao.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatOperatorIcao(); },
        tooltipChangedCallback: function(aircraft) { return aircraft.operator.chg; },
        tooltipCallback:        function(aircraft) { return aircraft.formatOperator(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Picture] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Picture,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListPicture',
        labelKey:               'Picture',
        headingAlignment:       VRS.Alignment.Centre,
        fixedWidth:             function(/** VRS.RenderSurface */renderSurface) {
            switch(renderSurface) {
                case VRS.RenderSurface.InfoWindow:
                    return VRS.globalOptions.aircraftPictureSizeInfoWindow.width.toString() + 'px';
                case VRS.RenderSurface.List:
                    return VRS.globalOptions.aircraftPictureSizeList.width.toString() + 'px';
                default:
                    return null;
            }
        },
        hasChangedCallback:     function(aircraft) { return aircraft.hasPicture.chg; },
        suppressLabelCallback:  function(surface) { return surface === VRS.RenderSurface.DetailBody; },
        renderCallback:         function(aircraft, options, surface) {
            switch(surface) {
                case VRS.RenderSurface.InfoWindow:
                    return aircraft.formatPictureHtml(VRS.globalOptions.aircraftPictureSizeInfoWindow);
                case VRS.RenderSurface.List:
                    return aircraft.formatPictureHtml(VRS.globalOptions.aircraftPictureSizeList, true, false, VRS.globalOptions.aircraftPictureSizeList);
                case VRS.RenderSurface.DetailBody:
                    return aircraft.formatPictureHtml(
                        !VRS.globalOptions.isMobile ?               VRS.globalOptions.aircraftPictureSizeDesktopDetail
                            : VRS.browserHelper.isProbablyPhone() ? VRS.globalOptions.aircraftPictureSizeIPhoneDetail
                            :                                       VRS.globalOptions.aircraftPictureSizeIPadDetail,
                        false, true);
                default:
                    throw 'Unexpected surface ' + surface;
            }
        }
    });
    VRS.renderPropertyHandlers[VRS.RenderProperty.PositionOnMap] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.PositionOnMap,
        surfaces:               VRS.RenderSurface.DetailBody,
        headingKey:             'Map',
        labelKey:               'Map',
        suppressLabelCallback:  function() { return true; },
        hasChangedCallback:     function(aircraft) { return aircraft.latitude.chg || aircraft.longitude.chg; },
        createWidget:           function(element, options, surface) {
            if(surface === VRS.RenderSurface.DetailBody && !VRS.jQueryUIHelper.getAircraftPositionMapPlugin(element)) {
                (<any>element).vrsAircraftPositonMap(VRS.jQueryUIHelper.getAircraftPositionMapOptions(<AircraftPositionMapPlugin_Options>{
                    plotterOptions:         options.plotterOptions,
                    mirrorMapJQ:            options.mirrorMapJQ,
                    mapOptionOverrides:     <IMapOptions>{
                        draggable:          false,
                        showMapTypeControl: false
                    },
                    unitDisplayPreferences: options.unitDisplayPreferences,
                    autoHideNoPosition:     true
                }));
            }
        },
        destroyWidget:          function(element, surface) {
            var aircraftPositionMap = VRS.jQueryUIHelper.getAircraftPositionMapPlugin(element);
            if(surface === VRS.RenderSurface.DetailBody && aircraftPositionMap) {
                aircraftPositionMap.destroy();
            }
        },
        renderWidget:           function(element, aircraft, options, surface) {
            var aircraftPositionMap = VRS.jQueryUIHelper.getAircraftPositionMapPlugin(element);
            if(surface === VRS.RenderSurface.DetailBody && aircraftPositionMap) {
                aircraftPositionMap.renderAircraft(aircraft, options.aircraftList ? options.aircraftList.getSelectedAircraft() === aircraft : false);
            }
        },
        suspendWidget:          function(element, surface, onOff)
        {
            var aircraftPositionMap = VRS.jQueryUIHelper.getAircraftPositionMapPlugin(element);
            aircraftPositionMap.suspend(onOff);
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Receiver] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Receiver,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListReceiver',
        labelKey:               'Receiver',
        sortableField:          VRS.AircraftListSortableField.Receiver,
        hasChangedCallback:     function(aircraft) { return aircraft.receiverId.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatReceiver(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Registration] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Registration,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListRegistration',
        labelKey:               'Registration',
        sortableField:          VRS.AircraftListSortableField.Registration,
        hasChangedCallback:     function(aircraft) { return aircraft.registration.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatRegistration(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.RegistrationAndIcao] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.RegistrationAndIcao,
        surfaces:               VRS.RenderSurface.List,
        headingKey:             'ListRegistration',
        labelKey:               'RegistrationAndIcao',
        sortableField:          VRS.AircraftListSortableField.Registration,
        hasChangedCallback:     function(aircraft) { return aircraft.registration.chg || aircraft.icao.chg; },
        renderCallback:         function(aircraft) {
            return VRS.format.stackedValues(
                aircraft.formatRegistration(),
                aircraft.formatIcao()
            );
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.RouteShort] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.RouteShort,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListRoute',
        labelKey:               'Route',
        optionsLabelKey:        'RouteShort',
        hasChangedCallback:     function(aircraft) { return aircraft.callsign.chg || aircraft.hasRouteChanged(); },
        useHtmlRendering:       function(aircraft, options, surface) { return surface === VRS.RenderSurface.DetailBody; },
        contentCallback:        function(aircraft, options, surface) {
            switch(surface) {
                case VRS.RenderSurface.InfoWindow:
                case VRS.RenderSurface.List:
                case VRS.RenderSurface.Marker:      return aircraft.formatRouteShort();
                default:                            throw 'Unexpected surface ' + surface;
            }
        },
        renderCallback:         function(aircraft, options, surface) {
            switch(surface) {
                case VRS.RenderSurface.DetailBody:  return aircraft.formatRouteShort(false, true);
                default:                            throw 'Unexpected surface ' + surface;
            }
        },
        tooltipCallback:        function(aircraft) { return aircraft.formatRouteFull(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.RouteFull] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.RouteFull,
        surfaces:               VRS.RenderSurface.DetailBody,
        headingKey:             'ListRoute',
        labelKey:               'Route',
        optionsLabelKey:        'RouteFull',
        isMultiLine:            true,
        hasChangedCallback:     function(aircraft) { return aircraft.callsign.chg || aircraft.hasRouteChanged(); },
        renderCallback:         function(aircraft, options) { return aircraft.formatRouteMultiLine(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Serial] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Serial,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListSerialNumber',
        labelKey:               'SerialNumber',
        sortableField:          VRS.AircraftListSortableField.Serial,
        hasChangedCallback:     function(aircraft) { return aircraft.serial.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatSerial(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.SignalLevel] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.SignalLevel,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListSignalLevel',
        labelKey:               'SignalLevel',
        optionsLabelKey:        'SignalLevel',
        alignment:              VRS.Alignment.Right,
        sortableField:          VRS.AircraftListSortableField.SignalLevel,
        hasChangedCallback:     function(aircraft) { return aircraft.signalLevel.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatSignalLevel(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Silhouette] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Silhouette,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailHead + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListModelSilhouette',
        labelKey:               'Silhouette',
        sortableField:          VRS.AircraftListSortableField.ModelIcao,
        headingAlignment:       VRS.Alignment.Centre,
        suppressLabelCallback:  function() { return true; },
        fixedWidth:             function() { return VRS.globalOptions.aircraftSilhouetteSize.width.toString() + 'px'; },
        hasChangedCallback:     function(aircraft) { return aircraft.modelIcao.chg || aircraft.icao.chg || aircraft.registration.chg; },
        renderCallback:         function(aircraft) { return aircraft.formatModelIcaoImageHtml(); },
        tooltipChangedCallback: function(aircraft) { return aircraft.model.chg || aircraft.modelIcao.chg || aircraft.countEngines.chg || aircraft.engineType.chg || aircraft.species.chg || aircraft.wakeTurbulenceCat.chg; },
        tooltipCallback:        function(aircraft) { return aircraft.formatModelIcaoNameAndDetail(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.SilhouetteAndOpFlag] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.SilhouetteAndOpFlag,
        surfaces:               VRS.RenderSurface.List,
        headingKey:             'ListModelSilhouetteAndOpFlag',
        labelKey:               'SilhouetteAndOpFlag',
        headingAlignment:       VRS.Alignment.Centre,
        sortableField:          VRS.AircraftListSortableField.OperatorIcao,
        fixedWidth:             function() { return Math.max(VRS.globalOptions.aircraftSilhouetteSize.width, VRS.globalOptions.aircraftOperatorFlagSize.width).toString() + 'px'; },
        hasChangedCallback:     function(aircraft) { return aircraft.modelIcao.chg || aircraft.operatorIcao.chg || aircraft.registration.chg; },
        renderCallback:         function(aircraft) { return aircraft.formatModelIcaoImageHtml() + aircraft.formatOperatorIcaoImageHtml(); },
        tooltipChangedCallback: function(aircraft) { return aircraft.model.chg || aircraft.modelIcao.chg || aircraft.countEngines.chg || aircraft.engineType.chg || aircraft.species.chg || aircraft.wakeTurbulenceCat.chg || aircraft.operatorIcao.chg || aircraft.operator.chg; },
        tooltipCallback:        function(aircraft) {
            var silhouetteTooltip = aircraft.formatModelIcaoNameAndDetail();
            var opFlagTooltip = aircraft.formatOperatorIcaoAndName();
            return silhouetteTooltip && opFlagTooltip ? silhouetteTooltip + '. ' + opFlagTooltip : silhouetteTooltip ? silhouetteTooltip : opFlagTooltip;
        }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Species] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Species,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListSpecies',
        labelKey:               'Species',
        hasChangedCallback:     function(aircraft) { return aircraft.species.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatSpecies(false); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Speed] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Speed,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListSpeed',
        labelKey:               'Speed',
        sortableField:          VRS.AircraftListSortableField.Speed,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.speed.chg; },
        contentCallback:        function(aircraft, options) {
            return aircraft.formatSpeed(
                options.unitDisplayPreferences.getSpeedUnit(),
                options.showUnits,
                options.unitDisplayPreferences.getShowSpeedType()
            );
        },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Speed; }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.SpeedType] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.SpeedType,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListSpeedType',
        labelKey:               'SpeedType',
        sortableField:          VRS.AircraftListSortableField.SpeedType,
        hasChangedCallback:     function(aircraft) { return aircraft.speedType.chg; },
        contentCallback:        function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) { return aircraft.formatSpeedType(); }
    });

    /*
    VRS.renderPropertyHandlers[VRS.RenderProperty.SpeedGraph] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.SpeedGraph,
        surfaces:               VRS.RenderSurface.DetailBody,
        labelKey:               'SpeedGraph',
        isMultiLine:            true,
        hasChangedCallback:     function(aircraft) { return aircraft.speed.chg; },
        contentCallback:        function() { return '[ Speed graph goes here - one day :) ]'}
    });
    */

    VRS.renderPropertyHandlers[VRS.RenderProperty.Squawk] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Squawk,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListSquawk',
        labelKey:               'Squawk',
        sortableField:          VRS.AircraftListSortableField.Squawk,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.squawk.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatSquawk(); },
        tooltipCallback:        function(aircraft, options, surface) { return aircraft.formatSquawkDescription(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.TargetAltitude] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.TargetAltitude,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListTargetAltitude',
        labelKey:               'TargetAltitude',
        sortableField:          VRS.AircraftListSortableField.TargetAltitude,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.targetAltitude.chg; },
        contentCallback:        function(aircraft, options) {
            return aircraft.formatTargetAltitude(
                options.unitDisplayPreferences.getHeightUnit(),
                options.showUnits,
                options.unitDisplayPreferences.getShowAltitudeType()
            );
        },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Height; }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.TargetHeading] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.TargetHeading,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListTargetHeading',
        labelKey:               'TargetHeading',
        sortableField:          VRS.AircraftListSortableField.TargetHeading,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.targetHeading.chg; },
        contentCallback:        function(aircraft, options) {
            return aircraft.formatTargetHeading(
                options.showUnits,
                options.unitDisplayPreferences.getShowTrackType()
            );
        },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Angle; }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.TimeTracked] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.TimeTracked,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListDuration',
        labelKey:               'TimeTracked',
        sortableField:          VRS.AircraftListSortableField.TimeTracked,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function() { return true; },
        contentCallback:        function(aircraft) { return aircraft.formatSecondsTracked(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Tisb] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Tisb,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListTisb',
        labelKey:               'Tisb',
        hasChangedCallback:     function(aircraft) { return aircraft.isTisb.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatIsTisb(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.TransponderType] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.TransponderType,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListTransponderType',
        labelKey:               'TransponderType',
        sortableField:          VRS.AircraftListSortableField.TransponderType,
        hasChangedCallback:     function(aircraft) { return aircraft.transponderType.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatTransponderType(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.TransponderTypeFlag] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.TransponderTypeFlag,
        surfaces:               VRS.RenderSurface.List,
        headingKey:             'ListTransponderTypeFlag',
        labelKey:               'TransponderTypeFlag',
        sortableField:          VRS.AircraftListSortableField.TransponderType,
        suppressLabelCallback:  function() { return true; },
        fixedWidth:             function() { return VRS.globalOptions.aircraftTransponderTypeSize.width.toString() + 'px'; },
        hasChangedCallback:     function(aircraft) { return aircraft.transponderType.chg; },
        renderCallback:         function(aircraft) { return aircraft.formatTransponderTypeImageHtml(); },
        tooltipChangedCallback: function(aircraft) { return aircraft.transponderType.chg; },
        tooltipCallback:        function(aircraft) { return aircraft.formatTransponderType(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.UserTag] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.UserTag,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListUserTag',
        labelKey:               'UserTag',
        sortableField:          VRS.AircraftListSortableField.UserTag,
        hasChangedCallback:     function(aircraft) { return aircraft.userTag.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatUserTag(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.VerticalSpeed] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.VerticalSpeed,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListVerticalSpeed',
        labelKey:               'VerticalSpeed',
        sortableField:          VRS.AircraftListSortableField.VerticalSpeed,
        alignment:              VRS.Alignment.Right,
        hasChangedCallback:     function(aircraft) { return aircraft.verticalSpeed.chg; },
        contentCallback:        function(aircraft, options) {
            return aircraft.formatVerticalSpeed(
                options.unitDisplayPreferences.getHeightUnit(),
                options.unitDisplayPreferences.getShowVerticalSpeedPerSecond(),
                options.showUnits,
                options.unitDisplayPreferences.getShowVerticalSpeedType()
            );
        },
        usesDisplayUnit:        function(displayUnitDependency) { return displayUnitDependency === VRS.DisplayUnitDependency.Height || displayUnitDependency === VRS.DisplayUnitDependency.VsiSeconds; }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.VerticalSpeedType] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.VerticalSpeedType,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListVerticalSpeedType',
        labelKey:               'VerticalSpeedType',
        sortableField:          VRS.AircraftListSortableField.VerticalSpeedType,
        hasChangedCallback:     function(aircraft) { return aircraft.verticalSpeedType.chg; },
        contentCallback:        function(aircraft, options) { return aircraft.formatVerticalSpeedType(); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.Wtc] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.Wtc,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListWtc',
        labelKey:               'WakeTurbulenceCategory',
        hasChangedCallback:     function(aircraft) { return aircraft.wakeTurbulenceCat.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatWakeTurbulenceCat(false, false); },
        tooltipCallback:        function(aircraft) { return aircraft.formatWakeTurbulenceCat(true, true); }
    });

    VRS.renderPropertyHandlers[VRS.RenderProperty.YearBuilt] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.YearBuilt,
        surfaces:               VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker + VRS.RenderSurface.InfoWindow,
        headingKey:             'ListYearBuilt',
        labelKey:               'YearBuilt',
        sortableField:          VRS.AircraftListSortableField.YearBuilt,
        hasChangedCallback:     function(aircraft) { return aircraft.yearBuilt.chg; },
        contentCallback:        function(aircraft) { return aircraft.formatYearBuilt(); }
    });


    /*
     * PROPERTY HANDLERS THAT DEPEND ON OTHER PROPERTY HANDLERS BEING REGISTERED BEFORE THEM
     */


    var pictureHandler =                VRS.renderPropertyHandlers[VRS.RenderProperty.Picture];
    var airportDataThumbnailsHandler =  VRS.renderPropertyHandlers[VRS.RenderProperty.AirportDataThumbnails];

    if(pictureHandler && airportDataThumbnailsHandler) {
        VRS.renderPropertyHandlers[VRS.RenderProperty.PictureOrThumbnails] = new VRS.RenderPropertyHandler({
            property:               VRS.RenderProperty.PictureOrThumbnails,
            surfaces:               VRS.RenderSurface.DetailBody,
            labelKey:               'PictureOrThumbnails',
            alignment:              VRS.Alignment.Centre,
            hasChangedCallback:     function(aircraft) { return aircraft.icao.chg || aircraft.hasPicture.chg || aircraft.airportDataThumbnails.chg; },
            suppressLabelCallback:  function(surface) { return true; },
            renderCallback:         function(aircraft, options, surface) {
                return aircraft.hasPicture.val ? pictureHandler.renderCallback(aircraft, options, surface)
                                               : airportDataThumbnailsHandler.renderCallback(aircraft, options, surface);
            }
        });
    }

    /**
        * The settings object to pass to RenderPropertyHelper.addRenderPropertiesListOptionsToPane
        */
    export interface RenderPropertyHelper_ListOptionsToPane
    {
        /**
            * The pane to add the field to.
            */
        pane: OptionPane;

        /**
            * The VRS.RenderSurface to show properties for. The user will be able to select any property that can be rendered to this surface.
            */
        surface: RenderSurfaceBitFlags;

        /**
            * The index into VRS.$$ for the field's label.
            */
        fieldLabel: string;

        /**
            * A method that returns a list of VRS.RenderProperty strings representing the properties selected by the user.
            */
        getList: () => RenderPropertyEnum[];

        /**
            * A method that takes a list of VRS.RenderProperty strings selected by the user and copies them to the object being configured.
            */
        setList: (properties: RenderPropertyEnum[]) => void;

        /**
            * A method that can save the state of the object being configured.
            */
        saveState: () => void;
    }

    /**
     * A helper object that can deal with the mundane tasks when working with VRS.RenderPropertyHandler objects.
     */
    export class RenderPropertyHelper
    {
        /**
         * Removes invalid render properties from a list, usually called as part of loading a previous session's state.
         */
        buildValidRenderPropertiesList(renderPropertiesList: RenderPropertyEnum[], surfaces: RenderSurfaceBitFlags[] = [], maximumProperties: number = -1) : RenderPropertyEnum[]
        {
            if(surfaces.length === 0) {
                for(var surfaceName in VRS.RenderSurface) {
                    surfaces.push(VRS.RenderSurface[surfaceName]);
                }
            }
            var countSurfaces = surfaces.length;

            var validProperties: RenderPropertyEnum[] = [];
            $.each(renderPropertiesList, function(idx, property) {
                if(maximumProperties === -1 || validProperties.length <= maximumProperties) {
                    var handler = VRS.renderPropertyHandlers[property];
                    if(handler) {
                        var surfaceSupported = false;
                        for(var i = 0;i < countSurfaces;++i) {
                            surfaceSupported = handler.isSurfaceSupported(surfaces[i]);
                            if(surfaceSupported) {
                                validProperties.push(property);
                                break;
                            }
                        }
                    }
                }
            });

            return validProperties;
        }

        /**
         * Returns an array of render property handlers that support a surface.
         */
        getHandlersForSurface(surface: RenderSurfaceBitFlags) : RenderPropertyHandler[]
        {
            var result: RenderPropertyHandler[] = [];

            $.each(VRS.renderPropertyHandlers, function(idx, handler) {
                if(handler instanceof VRS.RenderPropertyHandler && handler.isSurfaceSupported(surface)) {
                    result.push(handler);
                }
            });

            return result;
        }

        /**
         * Sorts the handlers array into labelKey order. Modifies the array passed in. Set putNoneFirst to true if the handler
         * for RenderProperty.None should always appear before every other handler.
         */
        sortHandlers(handlers: RenderPropertyHandler[], putNoneFirst: boolean)
        {
            putNoneFirst = !!putNoneFirst;
            handlers.sort(function(lhs, rhs) {
                if(putNoneFirst && lhs.property === VRS.RenderProperty.None) return rhs.property === VRS.RenderProperty.None ? 0 : -1;
                if(putNoneFirst && rhs.property === VRS.RenderProperty.None) return 1;
                var lhsText = VRS.globalisation.getText(lhs.labelKey) || '';
                var rhsText = VRS.globalisation.getText(rhs.labelKey) || '';
                return lhsText.localeCompare(rhsText);
            });
        }

        /**
         * Adds an orderedSubset field to an options pane that allows the user to configure a list of VRS.RenderProperty
         * strings for a given VRS.RenderSurface.
         */
        addRenderPropertiesListOptionsToPane(settings: RenderPropertyHelper_ListOptionsToPane) : OptionFieldOrderedSubset
        {
            var pane = settings.pane;
            var surface = settings.surface;
            var fieldLabel = settings.fieldLabel;
            var getList = settings.getList;
            var setList = settings.setList;
            var saveState = settings.saveState;

            var values: ValueText[] = [];
            for(var propertyName in VRS.RenderProperty) {
                var property = VRS.RenderProperty[propertyName];
                var handler = VRS.renderPropertyHandlers[property];
                if(!handler) throw 'Cannot find the handler for property ' + property;
                if(!handler.isSurfaceSupported(surface)) continue;
                values.push(new VRS.ValueText({ value: property, textKey: handler.optionsLabelKey }));
            }
            values.sort(function(lhs, rhs) {
                var lhsText = lhs.getText() || '';
                var rhsText = rhs.getText() || '';
                return lhsText.localeCompare(rhsText);
            });

            var field = new VRS.OptionFieldOrderedSubset({
                name:           'renderProperties',
                labelKey:       fieldLabel,
                getValue:       getList,
                setValue:       setList,
                saveState:      saveState,
                values:         values
            });
            pane.addField(field);

            return field;
        }

        /**
         * Returns a list of VRS.ValueText objects with the value set to the render property and the textKey set to the labelKey
         * for every handler in the list passed across.
         */
        createValueTextListForHandlers(handlers: RenderPropertyHandler[]) : ValueText[]
        {
            var result: ValueText[] = [];
            $.each(handlers, function(idx, handler) {
                result.push(new VRS.ValueText({
                    value: handler.property,
                    textKey: handler.labelKey
                }));
            });

            return result;
        }
    }

    /*
     * Pre-builts
     */
    export var renderPropertyHelper = new VRS.RenderPropertyHelper();
}
