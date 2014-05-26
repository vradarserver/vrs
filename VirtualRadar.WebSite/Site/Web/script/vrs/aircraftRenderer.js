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

(function(VRS, $, undefined)
{
    //region globalOptions
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftRendererEnableDebugProperties = VRS.globalOptions.aircraftRendererEnableDebugProperties !== undefined ? VRS.globalOptions.aircraftRendererEnableDebugProperties : false;  // True if the debug aircraft properties that expose some internal values are switched on. This needs to be changed BEFORE aircraftRenderer.js is loaded by the browser, it has no effect after.
    //endregion

    //region RenderPropertyHandler
    /**
     * A class that brings together everything needed to present a piece of information about an aircraft.
     * @param {Object}                                                   settings
     * @param {VRS.RenderProperty}                                       settings.property                  The VRS.RenderProperty enum value for the aircraft property that this handler can deal with.
     * @param {VRS.RenderSurface}                                        settings.surfaces                  A combination of VRS.RenderSurface bitflags that describe which surfaces this handler can render the property onto.
     * @param {string}                                                  [settings.headingKey]               The key into VRS.$$ for the text to display for column headings. Uses the labelKey if not supplied.
     * @param {string}                                                  [settings.labelKey]                 The key into VRS.$$ for the text to display for full display labels. Uses the headingKey if not supplied.
     * @param {string}                                                  [settings.optionsLabelKey]          The key into VRS.$$ for the text to display for configuration UI labels. Uses either headingKey or labelKey if not supplied.
     * @param {VRS.Alignment}                                           [settings.headingAlignment]         The VRS.Alignment enum describing the alignment for column headings - uses alignment if not supplied.
     * @param {VRS.Alignment}                                           [settings.contentAlignment]         The VRS.Alignment enum describing the alignment for values in columns - uses alignment if not supplied.
     * @param {VRS.Alignment}                                           [settings.alignment]                The VRS.Alignment enum describing the alignment for headings and column values.
     * @param {function(VRS.RenderSurface):string}                      [settings.fixedWidth]               An optional fixed width as a CSS width string (e.g. '20px' or '6em') when rendering within columns.
     * @param {function(VRS.Aircraft):bool}                              settings.hasChangedCallback        Returns true if the content of the property changed in the last update.
     * @param {function(VRS.Aircraft, *, VRS.RenderSurface):string}     [settings.contentCallback]          Takes an aircraft, options object and surface and returns the text content that represents the property.
     * @param {function(VRS.Aircraft, *, VRS.RenderSurface):string}     [settings.renderCallback]           Takes an aircraft, options object and surface and returns HTML that represents the property.
     * @param {function(VRS.Aircraft, *, VRS.RenderSurface):bool}       [settings.useHtmlRendering]         Takes an aircraft, options object and surface and returns true if the renderCallback should be used or false if the contentCallback should be used. Only supply this if you supply BOTH contentCallback and renderCallback.
     * @param {function(VRS.DisplayUnitDependency):bool}                [settings.usesDisplayUnit]          Takes a VRS.DisplayUnitDependency enum and returns true if the property display depends upon it. Defaults to method that returns false.
     * @param {function(VRS.Aircraft):bool}                             [settings.tooltipChangedCallback]   Returns true if the tooltip has changed in the last update. Defaults to hasChangedCallback if not supplied.
     * @param {function(VRS.Aircraft, *, VRS.RenderSurface):string}     [settings.tooltipCallback]          Takes an aircraft, options object and render surface and returns the tooltip text for the property. If not supplied then the property has no tooltip.
     * @param {function(VRS.RenderSurface):bool}                        [settings.suppressLabelCallback]    Takes a VRS.RenderSurface and returns true if the label should not be rendered on the surface. Defaults to a method that returns false.
     * @param {bool}                                                    [settings.isMultiLine]              True if the content takes more than one line of text to display, false if it does not. Defaults to false.
     * @param {VRS.AircraftListSortableField}                           [settings.sortableField]            An optional reference to the sortable field for the property.
     * @param {function(jQuery, *, VRS.RenderSurface)}                  [settings.createWidget]             Widget support - called after an element has been created for the render property, allows the renderer to add a widget to the element.
     * @param {function(jQuery, VRS.Aircraft, *, VRS.RenderSurface)}    [settings.renderWidget]             Widget support - renders the property into a widget.
     * @param {function(jQuery, VRS.RenderSurface)}                     [settings.destroyWidget]            Destroys the widget attached to the element.
     * @param {function(jQuery, VRS.RenderSurface, bool)}               [settings.suspendWidget]            Passed true if updates are being suspended and false if updates are being resumed.
     * @constructor
     */
    VRS.RenderPropertyHandler = function(settings)
    {
        //region -- Fields and Properties
        var that = this;

        if(!settings) throw 'You must supply a settings object';
        if(!settings.property) throw 'The settings must specify the property';
        if(!settings.surfaces) throw 'The settings must specify the surfaces flags';
        if(!settings.hasChangedCallback) throw 'The settings must specify a hasChanged callback';
        if(!settings.headingKey && !settings.labelKey) throw 'The settings must specify either a heading or a label key';
        if(!settings.alignment) settings.alignment = VRS.Alignment.Left;
        if(!settings.tooltipChangedCallback) settings.tooltipChangedCallback = settings.hasChangedCallback;

        /** @type {VRS.RenderProperty} */                                   this.property = settings.property;
        /** @type {VRS.RenderSurface} */                                    this.surfaces = settings.surfaces;
        /** @type {string} */                                               this.headingKey = settings.headingKey || settings.labelKey;
        /** @type {string} */                                               this.labelKey = settings.labelKey || settings.headingKey;
        /** @type {string} */                                               this.optionsLabelKey = settings.optionsLabelKey || this.labelKey;
        /** @type {VRS.Alignment} */                                        this.headingAlignment = settings.headingAlignment || settings.alignment;
        /** @type {VRS.Alignment} */                                        this.contentAlignment = settings.contentAlignment || settings.alignment;
        /** @type {function(VRS.RenderSurface):string} */                   this.fixedWidth = settings.fixedWidth || function() { return null; };
        /** @type {function(VRS.Aircraft):bool} */                          this.hasChangedCallback = settings.hasChangedCallback;
        /** @type {function(VRS.Aircraft, *, VRS.RenderSurface):string} */  this.contentCallback = settings.contentCallback;
        /** @type {function(VRS.Aircraft, *, VRS.RenderSurface):string} */  this.renderCallback = settings.renderCallback;
        /** @type {function(VRS.Aircraft, *, VRS.RenderSurface):bool} */    this.useHtmlRendering = settings.useHtmlRendering || function() { return !!that.renderCallback; };
        /** @type {function(VRS.DisplayUnitDependency):bool} */             this.usesDisplayUnit = settings.usesDisplayUnit || function() { return false; };
        /** @type {function(VRS.Aircraft):bool} */                          this.tooltipChangedCallback = settings.tooltipChangedCallback;
        /** @type {function(VRS.Aircraft, *, VRS.RenderSurface):string} */  this.tooltipCallback = settings.tooltipCallback;
        /** @type {function(VRS.RenderSurface):bool} */                     this.suppressLabelCallback = settings.suppressLabelCallback || function() { return false; };
        /** @type {bool} */                                                 this.isMultiLine = !!settings.isMultiLine;
        /** @type {VRS.AircraftListSortableField} */                        this.sortableField = settings.sortableField || VRS.AircraftListSortableField.None;
        /** @type {function(jQuery, *, VRS.RenderSurface)} */               this.createWidget = settings.createWidget;
        /** @type {function(jQuery, VRS.Aircraft, *, VRS.RenderSurface)} */ this.renderWidget = settings.renderWidget;
        /** @type {function(jQuery, VRS.RenderSurface)} */                  this.destroyWidget = settings.destroyWidget;

        /**
         * Returns true if the property can be rendered onto the VRS.RenderSurface passed across.
         * @param {number} surface The VRS.RenderSurface being tested.
         * @returns {boolean}
         */
        this.isSurfaceSupported = function(surface) { return (that.surfaces & surface) !== 0; };

        /**
         * Returns true if the property is rendered using a jQuery UI widget.
         * @returns {boolean}
         */
        this.isWidgetProperty = function() { return !!that.createWidget; };
        //endregion

        //region -- suspendWidget
        /**
         * Called when a widget is being suspended or resumed.
         * @param {jQuery}              jQueryElement       The jQuery element for the widget.
         * @param {VRS.RenderSurface}   surface             The surface that the widget is being drawn onto.
         * @param {boolean}             onOff               True if updates are being suspended, false if they are being resumed.
         */
        this.suspendWidget = function(jQueryElement, surface, onOff)
        {
            if(settings.suspendWidget) settings.suspendWidget(jQueryElement, surface, onOff);
        };
        //endregion

        //region -- createWidgetInDom, createWidgetInJQuery, destroyWidgetInDom, destroyWidgetInJQuery
        /**
         * Call after creating an element for a property - this lets renderers that use a jQuery widget create their widget.
         * @param {HTMLElement}                 domElement          The DOM element to render into.
         * @param {VRS.RenderSurface}           surface             The VRS.RenderSurface that indicates which surface the DOM element is a part of.
         * @param {*}                           options             The options to pass to the create method.
         */
        this.createWidgetInDom = function(domElement, surface, options)
        {
            if(that.isWidgetProperty()) {
                that.createWidget($(domElement), options, surface);
            }
        };

        /**
         * Call after creating an element for a property - this lets renderers that use a jQuery widget create their widget.
         * @param {jQuery}                      jQueryElement       The jQuery element to render into.
         * @param {VRS.RenderSurface}           surface             The VRS.RenderSurface that indicates which surface the DOM element is a part of.
         * @param {*}                          [options]            The options to pass to the create method.
         */
        this.createWidgetInJQuery = function(jQueryElement, surface, options)
        {
            if(that.isWidgetProperty()) {
                that.createWidget(jQueryElement, options, surface);
            }
        };

        /**
         * Call before destroying an element for a property - this lets renderers that use a jQuery widget destroy their widget.
         * @param {HTMLElement}                 domElement          The DOM element to render into.
         * @param {VRS.RenderSurface}           surface             The VRS.RenderSurface that indicates which surface the DOM element is a part of.
         */
        this.destroyWidgetInDom = function(domElement, surface)
        {
            if(that.isWidgetProperty()) {
                that.destroyWidget($(domElement), surface);
            }
        };

        /**
         * Call before destroying an element for a property - this lets renderers that use a jQuery widget destroy their widget.
         * @param {jQuery}                      jQueryElement       The jQuery element to render into.
         * @param {VRS.RenderSurface}           surface             The VRS.RenderSurface that indicates which surface the DOM element is a part of.
         */
        this.destroyWidgetInJQuery = function(jQueryElement, surface)
        {
            if(that.isWidgetProperty()) {
                that.destroyWidget(jQueryElement, surface);
            }
        };
        //endregion

        //region -- renderToDom, renderToJQuery
        /**
         * Renders a property to a native DOM element.
         * @param {HTMLElement}                 domElement          The DOM element to render into.
         * @param {VRS.RenderSurface}           surface             The VRS.RenderSurface that indicates which surface the DOM element is a part of.
         * @param {VRS.Aircraft}                aircraft            The aircraft to render.
         * @param {*}                           options             The options object to pass to the appropriate render callback.
         * @param {bool}                       [compareContent]     True if the render should be suppressed if the new content matches the existing content.
         * @param {?}                          [existingContent]    The existing content to compare.
         * @returns {?}                                             The new content of the element.
         */
        this.renderToDom = function(domElement, surface, aircraft, options, compareContent, existingContent)
        {
            var newContent;

            if(that.useHtmlRendering(aircraft, options, surface)) {
                newContent = that.renderCallback(aircraft, options, surface);
                if(!compareContent || newContent !== existingContent) domElement.innerHTML = newContent;
            } else if(that.isWidgetProperty()) {
                that.renderWidget($(domElement), aircraft, options, surface);
            } else {
                newContent = that.contentCallback(aircraft, options, surface);
                if(!compareContent || newContent !== existingContent) domElement.textContent = newContent;
            }

            return newContent;
        };

        /**
         * Renders a property to a jQuery element.
         * @param {jQuery}                      jQueryElement   The jQuery element to render into.
         * @param {VRS.RenderSurface}           surface         The VRS.RenderSurface that indicates which surface the jQuery element is a part of.
         * @param {VRS.Aircraft}                aircraft        The aircraft to render.
         * @param {*}                           options         The options object to pass to the appropriate render callback.
         */
        this.renderToJQuery = function(jQueryElement, surface, aircraft, options)
        {
            if(that.useHtmlRendering(aircraft, options, surface)) jQueryElement.html(that.renderCallback(aircraft, options, surface));
            else if(that.isWidgetProperty()) that.renderWidget(jQueryElement, aircraft, options, surface);
            else jQueryElement.text(that.contentCallback(aircraft, options, surface));
        };
        //endregion

        //region -- renderTooltipToDom, renderTooltipToJQuery
        /**
         * Renders the tooltip for the aircraft to the DOM element.
         * @param {HTMLElement}                 domElement      The DOM element to render the tooltip into.
         * @param {VRS.RenderSurface}           surface         The VRS.RenderSurface that indicates which surface the DOM element is a part of.
         * @param {VRS.Aircraft}                aircraft        The aircraft to render.
         * @param {*}                           options         The options object to pass to the appropriate render callback.
         * @returns {boolean}                                   True if the property has a tooltip, false if it does not.
         */
        this.renderTooltipToDom = function(domElement, surface, aircraft, options)
        {
            var tooltipText = that.tooltipCallback ? that.tooltipCallback(aircraft, options, surface) || '' : '';
            if(tooltipText) domElement.setAttribute('title', tooltipText);
            else domElement.removeAttribute('title');

            return !!tooltipText;
        };

        /**
         * Renders the tooltip for the aircraft to the DOM element.
         * @param {jQuery}                      jQueryElement   The jQuery element to render the tooltip into.
         * @param {VRS.RenderSurface}           surface         The VRS.RenderSurface that indicates which surface the jQuery element is a part of.
         * @param {VRS.Aircraft}                aircraft        The aircraft to render.
         * @param {*}                           options         The options object to pass to the appropriate render callback.
         * @returns {boolean}                                   True if the property has a tooltip, false if it does not.
         */
        this.renderTooltipToJQuery = function(jQueryElement, surface, aircraft, options)
        {
            var tooltipText = that.tooltipCallback ? that.tooltipCallback(aircraft, options, surface) || '' : '';
            jQueryElement.prop('title', tooltipText);

            return !!tooltipText;
        };
        //endregion
    };
    //endregion

    //region VRS.renderPropertyHandlers - built-in render property handlers
    /**
     * The associative array of VRS.RenderPropertyHandler objects indexed by VRS.RenderProperty.
     * @type {Object.<VRS.RenderProperty, VRS.RenderPropertyHandler>}
     */
    VRS.renderPropertyHandlers = VRS.renderPropertyHandlers || {};

    VRS.renderPropertyHandlers[VRS.RenderProperty.AirportDataThumbnails] = new VRS.RenderPropertyHandler({
        property:               VRS.RenderProperty.AirportDataThumbnails,
        surfaces:               VRS.RenderSurface.DetailBody,
        labelKey:               'AirportDataThumbnails',
        alignment:              VRS.Alignment.Centre,
        hasChangedCallback:     function(/** VRS.Aircraft */ aircraft) { return aircraft.icao.chg || aircraft.airportDataThumbnails.chg; },
        suppressLabelCallback:  function(/** VRS.RenderSurface */ surface) { return true; },
        renderCallback:         function(/** VRS.Aircraft */ aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) {
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
        contentCallback:        function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) {
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
        renderCallback:         function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) {
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
        contentCallback:        function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) { return aircraft.formatAltitudeType(); }
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
        contentCallback:        function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) { return aircraft.formatAverageSignalLevel(); }
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
        renderCallback:         function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) {
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
        contentCallback:        function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) { return aircraft.formatHeadingType(); }
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
        hasChangedCallback:     function(aircraft) { return aircraft.userInterested; },
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
        createWidget:           function(/** jQuery */ element, /** Object */ options, /** VRS.RenderSurface */ surface) {
            if(surface === VRS.RenderSurface.DetailBody && !VRS.jQueryUIHelper.getAircraftPositionMapPlugin(element)) {
                element.vrsAircraftPositonMap(VRS.jQueryUIHelper.getAircraftPositionMapOptions({
                    plotterOptions:         options.plotterOptions,
                    mirrorMapJQ:            options.mirrorMapJQ,
                    mapOptionOverrides:     {
                        draggable:          false,
                        showMapTypeControl: false
                    },
                    unitDisplayPreferences: options.unitDisplayPreferences,
                    autoHideNoPosition:     true
                }));
            }
        },
        destroyWidget:          function(/** jQuery */ element, /** VRS.RenderSurface */ surface) {
            var aircraftPositionMap = VRS.jQueryUIHelper.getAircraftPositionMapPlugin(element);
            if(surface === VRS.RenderSurface.DetailBody && aircraftPositionMap) {
                aircraftPositionMap.destroy();
            }
        },
        renderWidget:           function(/** jQuery */ element, /** VRS.Aircraft */ aircraft, /** * */ options, /** VRS.RenderSurface */ surface) {
            var aircraftPositionMap = VRS.jQueryUIHelper.getAircraftPositionMapPlugin(element);
            if(surface === VRS.RenderSurface.DetailBody && aircraftPositionMap) {
                aircraftPositionMap.renderAircraft(aircraft, options.aircraftList ? options.aircraftList.getSelectedAircraft() === aircraft : false);
            }
        },
        suspendWidget:          function(/** jQuery */ element, /** VRS.RenderSurface */ surface, /** boolean */ onOff)
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
        hasChangedCallback:     function(aircraft) { return aircraft.hasRouteChanged(); },
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
        hasChangedCallback:     function(aircraft) { return aircraft.hasRouteChanged(); },
        renderCallback:         function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) { return aircraft.formatRouteMultiLine(); }
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
        contentCallback:        function(aircraft) { return aircraft.formatSquawk(); }
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
        contentCallback:        function(aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options) { return aircraft.formatVerticalSpeedType(); }
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
    //endregion

    //region VRS.renderPropertyHandlers - those that depend on other property handlers being registered
    var pictureHandler =                VRS.renderPropertyHandlers[VRS.RenderProperty.Picture];
    var airportDataThumbnailsHandler =  VRS.renderPropertyHandlers[VRS.RenderProperty.AirportDataThumbnails];

    if(pictureHandler && airportDataThumbnailsHandler) {
        VRS.renderPropertyHandlers[VRS.RenderProperty.PictureOrThumbnails] = new VRS.RenderPropertyHandler({
            property:               VRS.RenderProperty.PictureOrThumbnails,
            surfaces:               VRS.RenderSurface.DetailBody,
            labelKey:               'PictureOrThumbnails',
            alignment:              VRS.Alignment.Centre,
            hasChangedCallback:     function(/** VRS.Aircraft */ aircraft) { return aircraft.icao.chg || aircraft.hasPicture.chg || aircraft.airportDataThumbnails.chg; },
            suppressLabelCallback:  function(/** VRS.RenderSurface */ surface) { return true; },
            renderCallback:         function(/** VRS.Aircraft */ aircraft, /** VRS_OPTIONS_AIRCRAFTRENDER */ options, /** VRS.RenderSurface */ surface) {
                return aircraft.hasPicture.val ? pictureHandler.renderCallback(aircraft, options, surface)
                                               : airportDataThumbnailsHandler.renderCallback(aircraft, options, surface);
            }
        });
    }
    //endregion

    //region - debug VRS.renderPropertyHandlers
    if(VRS.globalOptions.aircraftRendererEnableDebugProperties) {
        VRS.RenderProperty.FullCoordCount =     '!fc';
        VRS.RenderProperty.ShortCoordCount =    '!sc';
        VRS.RenderProperty.PlottedDetail =      '!pd';
        var addTranslations = function() {
            VRS.$$['!DEBUG-FullCoords'] = '!Full Coords';
            VRS.$$['!DEBUG-ShortCoords'] = '!Short Coords';
            VRS.$$['!DEBUG-PlottedDetail'] = '!Plotted Detail';
        };
        VRS.globalisation.hookLocaleChanged(function() {
            addTranslations();
        });

        VRS.renderPropertyHandlers[VRS.RenderProperty.FullCoordCount] = new VRS.RenderPropertyHandler({
            property:           /** @type {VRS.RenderProperty} */ VRS.RenderProperty.FullCoordCount,
            surfaces:           VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker,
            headingKey:         '!DEBUG-FullCoords',
            labelKey:           '!DEBUG-FullCoords',
            alignment:          VRS.Alignment.Right,
            hasChangedCallback: function(aircraft) { return true; },
            useHtmlRendering:   function(aircraft, options, surface) { return surface === VRS.RenderSurface.DetailBody; },
            contentCallback:    function(aircraft) { return aircraft.fullTrail.arr.length; },
            isMultiLine:        true,
            renderCallback:     function(aircraft) {
                var result = $('<div/>');
                $('<span/>').text('chg: ' + aircraft.fullTrail.chg).appendTo(result);
                $('<span/>').text(', chgIdx: ' + aircraft.fullTrail.chgIdx).appendTo(result);
                var table = $('<table/>').appendTo(result);
                var length = aircraft.fullTrail.arr.length;
                for(var i = 0;i < length;++i) {
                    var trail = aircraft.fullTrail.arr[i];
                    var row = $('<tr/>').appendTo(table);
                    $('<td/>')                            .text(trail.lat).appendTo(row);
                    $('<td/>').attr('style', 'padding-left: 1em;').text(trail.lng).appendTo(row);
                    $('<td/>').attr('style', 'padding-left: 1em;').text(trail.heading).appendTo(row);
                    $('<td/>').attr('style', 'padding-left: 1em;').text(trail.altitude).appendTo(row);
                    $('<td/>').attr('style', 'padding-left: 1em;').text(trail.speed).appendTo(row);
                    $('<td/>').attr('style', 'padding-left: 1em;').text(trail.chg).appendTo(row);
                }
                return result.html();
            }
        });

        VRS.renderPropertyHandlers[VRS.RenderProperty.ShortCoordCount] = new VRS.RenderPropertyHandler({
            property:           /** @type {VRS.RenderProperty} */ VRS.RenderProperty.ShortCoordCount,
            surfaces:           VRS.RenderSurface.List + VRS.RenderSurface.DetailBody + VRS.RenderSurface.Marker,
            headingKey:         '!DEBUG-ShortCoords',
            labelKey:           '!DEBUG-ShortCoords',
            alignment:          VRS.Alignment.Right,
            hasChangedCallback: function(aircraft) { return aircraft.shortTrail.chg; },
            contentCallback:    function(aircraft) { return aircraft.shortTrail.arr.length; }
        });

        VRS.diagnosticsAircraftPlotter = null;
        VRS.globalDispatch.hook(VRS.globalEvent.bootstrapCreated, function(bootStrap) {
            bootStrap.hookInitialised(function(bs, pageSettings) {
                VRS.diagnosticsAircraftPlotter = pageSettings.aircraftPlotter;
            });
        });
        VRS.renderPropertyHandlers[VRS.RenderProperty.PlottedDetail] = new VRS.RenderPropertyHandler({
            property:           /** @type {VRS.RenderProperty} */ VRS.RenderProperty.PlottedDetail,
            surfaces:           VRS.RenderSurface.DetailBody,
            headingKey:         '!DEBUG-PlottedDetail',
            labelKey:           '!DEBUG-PlottedDetail',
            isMultiLine:        true,
            hasChangedCallback: function() { return true; },
            renderCallback:     function(aircraft) {
                var detail = VRS.diagnosticsAircraftPlotter ? VRS.diagnosticsAircraftPlotter.diagnosticsGetPlottedDetail(aircraft) : null;
                var result = $('<div/>');
                if(detail) {
                    result.append($('<span/>').text('polylinePathUpdateCounter: ' + detail.polylinePathUpdateCounter));
                    result.append($('<span/>').text(', polylineTrailType: ' + detail.polylineTrailType));
                    var table = $('<table/>').appendTo(result);
                    var length = detail.mapPolylines.length;
                    for(var i = 0;i < length;++i) {
                        var polyline = detail.mapPolylines[i];
                        var path = polyline.getPath();
                        var pathText = '';
                        $.each(path, function(idx, latLng) {
                            pathText += pathText.length ? '<br />' : '';
                            pathText += latLng ? latLng.lat.toString() + ' / ' + latLng.lng.toString() : 'UNDEFINED';
                        });
                        var row = $('<tr/>').attr('style', 'vertical-align:top').appendTo(table);
                        $('<td/>').text(polyline.id).appendTo(row);
                        $('<td/>').attr('style', 'padding-left: 1em').text(polyline.getStrokeColour()).appendTo(row);
                        $('<td/>').attr('style', 'padding-left: 1em').text(polyline.getStrokeWeight()).appendTo(row);
                        $('<td/>').attr('style', 'padding-left: 1em').html(pathText).appendTo(row);
                    }
                }
                return result.html();
            }
    });
    }
    //endregion

    //region RenderPropertyHelper
    /**
     * A helper object that can deal with the mundane tasks when working with VRS.RenderPropertyHandler objects.
     * @constructor
     */
    VRS.RenderPropertyHelper = function()
    {
        //region -- State save and load helpers
        /**
         * Removes invalid render properties from a list, usually called as part of loading a previous session's state.
         * @param {VRS.RenderProperty[]}    renderPropertiesList    An array of VRS.RenderProperty values.
         * @param {VRS.RenderSurface[]}     surfaces                An array of VRS.RenderSurface values that describe the allowable surfaces.
         * @param {number}                 [maximumProperties]      The maximum number of entries allowed in the list. Undefined or -1 places no limit on the length of the list.
         * @returns {VRS.RenderProperty[]}                          The sanitised list of VRS.RenderProperty values.
         */
        this.buildValidRenderPropertiesList = function(renderPropertiesList, surfaces, maximumProperties)
        {
            maximumProperties = maximumProperties === undefined ? -1 : maximumProperties;
            surfaces = surfaces || [];
            if(surfaces.length === 0) {
                for(var surfaceName in VRS.RenderSurface) {
                    //noinspection JSUnfilteredForInLoop
                    surfaces.push(VRS.RenderSurface[surfaceName]);
                }
            }
            var countSurfaces = surfaces.length;

            var validProperties = [];
            $.each(renderPropertiesList, function(idx, property) {
                if(maximumProperties === -1 || validProperties.length <= maximumProperties) {
                    var handler = VRS.renderPropertyHandlers[property];
                    if(handler) {
                        var surfaceSupported = false;
                        for(var i = 0;i < countSurfaces;++i) {
                            surfaceSupported = handler.isSurfaceSupported(surfaces[i]);
                            if(surfaceSupported) break;
                        }
                        if(surfaceSupported) validProperties.push(property);
                    }
                }
            });

            return validProperties;
        };
        //endregion

        //region -- getHandlersForSurface
        /**
         * Returns an array of render property handlers that support a surface.
         * @param {VRS.RenderSurface} surface
         * @returns {Array.<VRS.RenderPropertyHandler>}
         */
        this.getHandlersForSurface = function(surface)
        {
            var result = [];

            $.each(VRS.renderPropertyHandlers, function(/** Number */ idx, /** VRS.RenderPropertyHandler */ handler) {
                if(handler instanceof VRS.RenderPropertyHandler && handler.isSurfaceSupported(surface)) result.push(handler);
            });

            return result;
        };
        //endregion

        //region -- sortHandlers
        /**
         * Sorts the handlers array into labelKey order. Modifies the array passed in.
         * @param {Array.<VRS.RenderPropertyHandler>}   handlers
         * @param {bool}                                putNoneFirst    Always puts the handler for VRS.RenderProperty.None first in the sorted list (if present).
         */
        this.sortHandlers = function(handlers, putNoneFirst)
        {
            putNoneFirst = !!putNoneFirst;
            handlers.sort(function(lhs, rhs) {
                if(putNoneFirst && lhs.property === VRS.RenderProperty.None) return rhs.property === VRS.RenderProperty.None ? 0 : -1;
                if(putNoneFirst && rhs.property === VRS.RenderProperty.None) return 1;
                var lhsText = VRS.globalisation.getText(lhs.labelKey) || '';
                var rhsText = VRS.globalisation.getText(rhs.labelKey) || '';
                return lhsText.localeCompare(rhsText);
            });
        };
        //endregion

        //region -- addConfigureFiltersListToPane
        /**
         * Adds an orderedSubset field to an options pane that allows the user to configure a list of VRS.RenderProperty
         * strings for a given VRS.RenderSurface.
         * @param {Object}              settings
         * @param {VRS.OptionPane}      settings.pane           The pane to add the field to.
         * @param {VRS.RenderSurface}   settings.surface        The VRS.RenderSurface to show properties for. The user will be able to select any property that can be rendered to this surface.
         * @param {string}              settings.fieldLabel     The index into VRS.$$ for the field's label.
         * @param {function():string[]} settings.getList        A method that returns a list of VRS.RenderProperty strings representing the properties selected by the user.
         * @param {function(string[])}  settings.setList        A method that takes a list of VRS.RenderProperty strings selected by the user and copies them to the object being configured.
         * @param {function()}          settings.saveState      A method that can save the state of the object being configured.
         * @returns {VRS.OptionFieldOrderedSubset}              The option field that has been created.
         */
        this.addRenderPropertiesListOptionsToPane = function(settings)
        {
            var pane = settings.pane;
            var surface = settings.surface;
            var fieldLabel = settings.fieldLabel;
            var getList = settings.getList;
            var setList = settings.setList;
            var saveState = settings.saveState;

            var values = [];
            for(var propertyName in VRS.RenderProperty) {
                //noinspection JSUnfilteredForInLoop
                var property = VRS.RenderProperty[propertyName];
                var handler = VRS.renderPropertyHandlers[property];
                if(!handler) throw 'Cannot find the handler for property ' + property;
                if(!handler.isSurfaceSupported(surface)) continue;
                values.push(new VRS.ValueText({ value: property, textKey: handler.optionsLabelKey }));
            }
            values.sort(function(/** VRS.ValueText */ lhs, /** VRS.ValueText */ rhs) {
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
        };
        //endregion

        //region -- createValueTextListForHandlers
        /**
         * Returns a list of VRS.ValueText objects with the value set to the render property and the textKey set to the labelKey
         * for every handler in the list passed across.
         * @param {Array.<VRS.RenderPropertyHandler>} handlers
         * @returns {Array.<VRS.ValueText>}
         */
        this.createValueTextListForHandlers = function(handlers)
        {
            var result = [];
            $.each(handlers, function(/** Number */ idx, /** VRS.RenderPropertyHandler */ handler) {
                result.push(new VRS.ValueText({
                    value: handler.property,
                    textKey: handler.labelKey
                }));
            });

            return result;
        };
        //endregion
    };
    //endregion

    //region VRS.renderPropertyHelper
    /**
     * The singleton instance of VRS.RenderPropertyHelper.
     * @type {VRS.RenderPropertyHelper}
     */
    VRS.renderPropertyHelper = new VRS.RenderPropertyHelper();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
