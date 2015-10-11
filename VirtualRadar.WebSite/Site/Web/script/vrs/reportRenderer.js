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
 * @fileoverview Describes how to render the data sent from the server in a request for a report
 */

(function(VRS, $, /** object= */ undefined)
{
    //region ReportPropertyHandler
    /**
     * An object that can handle the rendering of a property in the report.
     * @param {Object}                                                              settings
     * @param {VRS_REPORT_PROPERTY}                                                 settings.property               The property that this object handles.
     * @param {VRS.ReportSurface}                                                  [settings.surfaces]              The surfaces that the property can be rendered onto. If not supplied then it is assumed to be List and DetailBody.
     * @param {string}                                                             [settings.labelKey]              The key into VRS.$$ for the property's label. Uses headingKey if missing.
     * @param {string}                                                             [settings.headingKey]            The key into VRS.$$ for the property's list heading. Uses labelKey if missing.
     * @param {string}                                                             [settings.optionsLabelKey]       The key into VRS.$$ for the property's options label. Uses labelKey or headingKey if missing.
     * @param {VRS.Alignment}                                                      [settings.headingAlignment]      The VRS.Alignment enum describing the alignment for column headings - uses alignment if not supplied.
     * @param {VRS.Alignment}                                                      [settings.contentAlignment]      The VRS.Alignment enum describing the alignment for values in columns - uses alignment if not supplied.
     * @param {VRS.Alignment}                                                      [settings.alignment]             The VRS.Alignment enum describing the alignment for headings and column values.
     * @param {function():string}                                                  [settings.fixedWidth]            An optional fixed width as a CSS width string (e.g. '20px' or '6em') when rendering within columns.
     * @param {function(VRS_JSON_REPORT_TOPLEVEL):bool}                             settings.hasValue               A mandatory method that returns true if the JSON passed across has a value for the property.
     * @param {function(VRS_JSON_REPORT_TOPLEVEL, *):string}                       [settings.contentCallback]       Takes a JSON object and an options object and returns the text content that represents the property.
     * @param {function(VRS_JSON_REPORT_TOPLEVEL, *):string}                       [settings.renderCallback]        Takes a JSON object and options object and returns HTML that represents the property.
     * @param {function(VRS_JSON_REPORT_TOPLEVEL, *):string}                       [settings.tooltipCallback]       Takes a JSON object and options object and returns the tooltip text for the property. If not supplied then the property has no tooltip.
     * @param {bool}                                                               [settings.isMultiLine]           True if the content takes more than one line of text to display, false if it does not. Defaults to false.
     * @param {function(VRS.ReportSurface):bool}                                   [settings.suppressLabelCallback] An optional method that returns true if the label is not to be shown on this surface. Default returns false.
     * @param {VRS.ReportSortColumn}                                               [settings.sortColumn]            The sort column corresponding to this property, if any. Default is undefined.
     * @param {function(VRS_JSON_REPORT_TOPLEVEL):string}                          [settings.groupValue]            The grouping value to use when grouping report columns based on sort value. Default is undefined. Mandatory if sortColumn is supplied.
     * @param {function(jQuery, *, VRS.ReportSurface)}                             [settings.createWidget]          Widget support - called after an element has been created for the render property, allows the renderer to add a widget to the element.
     * @param {function(jQuery, VRS_JSON_REPORT_TOPLEVEL, *, VRS.ReportSurface)}   [settings.renderWidget]          Widget support - renders the property into a widget.
     * @param {function(jQuery, VRS.ReportSurface)}                                [settings.destroyWidget]         Destroys the widget attached to the element.
     * @constructor
     */
    VRS.ReportPropertyHandler = function(settings)
    {
        var that = this;

        this.property = settings.property;
        this.surfaces = settings.surfaces || (VRS.ReportSurface.List + VRS.ReportSurface.DetailBody);
        this.labelKey = settings.labelKey || settings.headingKey;
        this.headingKey = settings.headingKey || settings.labelKey;
        this.optionsLabelKey = settings.optionsLabelKey || settings.labelKey || settings.headingKey;
        this.headingAlignment = settings.headingAlignment || settings.alignment || settings.contentAlignment || VRS.Alignment.Left;
        this.contentAlignment = settings.contentAlignment || settings.alignment || settings.headingAlignment || VRS.Alignment.Left;
        this.isMultiLine = settings.isMultiLine || false;
        this.fixedWidth = settings.fixedWidth;
        this.suppressLabelCallback = settings.suppressLabelCallback || function() { return false; };
        this.sortColumn = settings.sortColumn;
        this.groupValue = settings.groupValue;

        this.isAircraftProperty = !!VRS.enumHelper.getEnumName(VRS.ReportAircraftProperty, this.property);
        this.isFlightsProperty = !this.isAircraftProperty;

        /**
         * Returns true if the property can be rendered onto the VRS.ReportSurface passed across.
         * @param {number} surface The VRS.ReportSurface being tested.
         * @returns {boolean}
         */
        this.isSurfaceSupported = function(surface)
        {
            return (that.surfaces & surface) !== 0;
        };

        /**
         * Returns true if the property has a value for the flight or aircraft passed across.
         * @param {VRS_JSON_REPORT_FLIGHT} flightJson
         * @returns {boolean}
         */
        this.hasValue = function(flightJson)
        {
            var json = that.isAircraftProperty ? flightJson.aircraft : flightJson;
            return settings.hasValue(json);
        };

        /**
         * If the renderer uses a widget then this creates the widget in the element passed across, otherwise it does nothing.
         * @param {jQuery}              jQueryElement
         * @param {VRS.ReportSurface}   surface
         * @param {*}                   options
         */
        this.createWidgetInJQueryElement = function(jQueryElement, surface, options)
        {
            if(settings.createWidget) settings.createWidget(jQueryElement, surface, options);
        };

        /**
         * If the renderer uses a widget then this destroys the widget in the element passed across, otherwise it does nothing.
         * @param {jQuery}              jQueryElement
         * @param {VRS.ReportSurface}   surface
         */
        this.destroyWidgetInJQueryElement = function(jQueryElement, surface)
        {
            if(settings.destroyWidget) settings.destroyWidget(jQueryElement, surface);
        };

        /**
         * Renders content into the jQuery element passed across.
         * @param {jQuery}                      jqElement
         * @param {VRS_JSON_REPORT_TOPLEVEL}    json
         * @param {Object}                      options
         * @param {VRS.ReportSurface}           surface
         */
        this.renderIntoJQueryElement = function(jqElement, json, options, surface)
        {
            if(settings.contentCallback)    jqElement.text(settings.contentCallback(json, options, surface));
            else if(settings.renderWidget)  settings.renderWidget(jqElement, json, options, surface);
            else                            jqElement.html(settings.renderCallback(json, options, surface));
        };

        /**
         * Adds a tooltip to the jQuery element passed acros.
         * @param {jQuery}                      jqElement
         * @param {VRS_JSON_REPORT_TOPLEVEVL}   json
         * @param {Object}                      options
         */
        this.addTooltip = function(jqElement, json, options)
        {
            if(settings.tooltipCallback) {
                var text = settings.tooltipCallback(json, options);
                if(text) jqElement.attr('title', text);
            }
        };
    };
    //endregion

    //region reportPropertyHandlers
    VRS.reportPropertyHandlers = VRS.reportPropertyHandlers || [];

    //region -- ReportAircraftProperty
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.AircraftClass] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.AircraftClass,
        headingKey:         'ListAircraftClass',
        labelKey:           'AircraftClass',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.acClass; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.aircraftClass(json.acClass); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.CofACategory] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.CofACategory,
        headingKey:         'ListCofACategory',
        labelKey:           'CofACategory',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.cofACategory; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.certOfACategory(json.cofACategory); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.CofAExpiry] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.CofAExpiry,
        headingKey:         'ListCofAExpiry',
        labelKey:           'CofAExpiry',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.cofAExpiry; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.certOfAExpiry(json.cofAExpiry); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Country] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Country,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListCountry',
        labelKey:           'Country',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.country; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.country(json.country); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.CurrentRegDate] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.CurrentRegDate,
        headingKey:         'ListCurrentRegDate',
        labelKey:           'CurrentRegDate',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.curRegDate; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.currentRegistrationDate(json.curRegDate); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.DeRegDate] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.DeRegDate,
        headingKey:         'ListDeRegDate',
        labelKey:           'DeRegDate',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.deregDate; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.deregisteredDate(json.deregDate); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Engines] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Engines,
        headingKey:         'ListEngines',
        labelKey:           'Engines',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.engType !== undefined && json.engines !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.engines(json.engines, json.engType); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.FirstRegDate] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.FirstRegDate,
        headingKey:         'ListFirstRegDate',
        labelKey:           'FirstRegDate',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.firstRegDate; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.firstRegistrationDate(json.firstRegDate); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.GenericName] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.GenericName,
        headingKey:         'ListGenericName',
        labelKey:           'GenericName',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.genericName !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.genericName(json.genericName); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Icao] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Icao,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListIcao',
        labelKey:           'Icao',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.icao; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.icao(json.icao); },
        sortColumn:         VRS.ReportSortColumn.Icao,
        groupValue:         function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.icao; }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Interesting] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Interesting,
        headingKey:         'ListInteresting',
        labelKey:           'Interesting',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.interested !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.userInterested(json.interested); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Manufacturer] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Manufacturer,
        headingKey:         'ListManufacturer',
        labelKey:           'Manufacturer',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.manufacturer; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.manufacturer(json.manufacturer); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Military] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Military,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListCivOrMil',
        labelKey:           'CivilOrMilitary',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return true; },        // The server doesn't emit the value if it's civilian
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.isMilitary(!!json.military); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Model] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Model,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListModel',
        labelKey:           'Model',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.typ; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.model(json.typ); },
        sortColumn:         VRS.ReportSortColumn.Model,
        groupValue:         function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.typ; }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.ModelIcao] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.ModelIcao,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListModelIcao',
        labelKey:           'ModelIcao',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.icaoType; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.modelIcao(json.icaoType); },
        sortColumn:         VRS.ReportSortColumn.ModelIcao,
        groupValue:         function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.icaoType; }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.ModeSCountry] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.ModeSCountry,
        headingKey:         'ListModeSCountry',
        labelKey:           'ModeSCountry',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.modeSCountry; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.modeSCountry(json.modeSCountry); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.MTOW] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.MTOW,
        headingKey:         'ListMaxTakeoffWeight',
        labelKey:           'MaxTakeoffWeight',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.mtow; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.maxTakeoffWeight(json.mtow); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Notes] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Notes,
        headingKey:         'ListNotes',
        labelKey:           'Notes',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.notes; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.notes(json.notes); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Operator] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Operator,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListOperator',
        labelKey:           'Operator',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.owner; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.operator(json.owner); },
        sortColumn:         VRS.ReportSortColumn.Operator,
        groupValue:         function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.owner; }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.OperatorFlag] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.OperatorFlag,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListOperatorFlag',
        labelKey:           'OperatorFlag',
        headingAlignment:   VRS.Alignment.Centre,
        fixedWidth:         function() { return VRS.globalOptions.aircraftOperatorFlagSize.width.toString() + 'px'; },
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.opFlag || !!json.icao || !!json.reg; },
        renderCallback:     function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.operatorIcaoImageHtml(json.owner, json.opFlag, json.icao, json.reg); },
        tooltipCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.operatorIcaoAndName(json.owner, json.opFlag); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.OperatorIcao] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.OperatorIcao,
        headingKey:         'ListOperatorIcao',
        labelKey:           'OperatorCode',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.opFlag; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.operatorIcao(json.opFlag); },
        tooltipCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.operatorIcaoAndName(json.owner, json.opFlag); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.OwnershipStatus] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.OwnershipStatus,
        headingKey:         'ListOwnershipStatus',
        labelKey:           'OwnershipStatus',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.ownerStatus; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.ownershipStatus(json.ownerStatus); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Picture] = new VRS.ReportPropertyHandler({
        property:               VRS.ReportAircraftProperty.Picture,
        surfaces:               VRS.ReportSurface.List + VRS.ReportSurface.DetailBody,
        headingKey:             'ListPicture',
        labelKey:               'Picture',
        headingAlignment:       VRS.Alignment.Centre,
        isMultiLine:            true,
        fixedWidth:             function(/** VRS.ReportSurface */ surface) {
            switch(surface) {
                case VRS.ReportSurface.List:
                    return VRS.globalOptions.aircraftPictureSizeList.width.toString() + 'px';
                default:
                    return null;
            }
        },
        hasValue:               function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.hasPic; },
        suppressLabelCallback:  function(/** VRS.ReportSurface */surface) { return surface === VRS.ReportSurface.DetailBody; },
        renderCallback:         function(/** VRS_JSON_REPORT_AIRCRAFT */ json, /** Object */ options, /** VRS.ReportSurface */ surface) {
            switch(surface) {
                case VRS.ReportSurface.List:
                    return VRS.format.pictureHtml(
                        json.reg,
                        json.icao,
                        json.picX,
                        json.picY,
                        VRS.globalOptions.aircraftPictureSizeList);
                case VRS.ReportSurface.DetailBody:
                    return VRS.format.pictureHtml(
                        json.reg,
                        json.icao,
                        json.picX,
                        json.picY,
                        !VRS.globalOptions.isMobile ?               VRS.globalOptions.aircraftPictureSizeDesktopDetail
                            : VRS.browserHelper.isProbablyPhone() ? VRS.globalOptions.aircraftPictureSizeIPhoneDetail
                            :                                       VRS.globalOptions.aircraftPictureSizeIPadDetail,
                        false, true);
                default:
                    throw 'Unexpected surface ' + surface;
            }
        }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.PopularName] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.PopularName,
        headingKey:         'ListPopularName',
        labelKey:           'PopularName',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.popularName; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.popularName(json.popularName); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.PreviousId] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.PreviousId,
        headingKey:         'ListPreviousId',
        labelKey:           'PreviousId',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.previousId; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.previousId(json.previousId); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Registration] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Registration,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListRegistration',
        labelKey:           'Registration',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.reg; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.registration(json.reg); },
        sortColumn:         VRS.ReportSortColumn.Registration,
        groupValue:         function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.reg; }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.SerialNumber] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.SerialNumber,
        headingKey:         'ListSerialNumber',
        labelKey:           'SerialNumber',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.serial; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.serial(json.serial); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Silhouette] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Silhouette,
        headingKey:         'ListModelSilhouette',
        labelKey:           'Silhouette',
        surfaces:           VRS.ReportSurface.List,
        headingAlignment:   VRS.Alignment.Centre,
        fixedWidth:         function() { return VRS.globalOptions.aircraftSilhouetteSize.width.toString() + 'px'; },
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.icaoType || !!json.icao || !!json.reg; },
        renderCallback:     function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.modelIcaoImageHtml(json.icaoType, json.icao, json.reg); },
        tooltipCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.modelIcaoNameAndDetail(json.icaoType, json.type, json.engines, json.engType, json.species, json.wtc); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Species] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Species,
        headingKey:         'ListSpecies',
        labelKey:           'Species',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.species !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.species(json.species); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Status] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.Status,
        headingKey:         'ListStatus',
        labelKey:           'Status',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.status; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.status(json.status); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.TotalHours] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.TotalHours,
        headingKey:         'ListTotalHours',
        labelKey:           'TotalHours',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.totalHours; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.totalHours(json.totalHours); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.YearBuilt] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.YearBuilt,
        headingKey:         'ListYearBuilt',
        labelKey:           'YearBuilt',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return !!json.yearBuilt; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.yearBuilt(json.yearBuilt); }
    });

    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.WakeTurbulenceCategory] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportAircraftProperty.WakeTurbulenceCategory,
        headingKey:         'ListWtc',
        labelKey:           'WakeTurbulenceCategory',
        hasValue:           function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return json.wtc !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_AIRCRAFT */ json) { return VRS.format.wakeTurbulenceCat(json.wtc, true, false); }
    });
    //endregion

    //region -- ReportFlightProperty
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Altitude] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.Altitude,
        headingKey:         'ListAltitude',
        labelKey:           'Altitude',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fAlt !== undefined || json.lAlt !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.altitudeFromTo(json.fAlt, json.fOnGnd, json.lAlt, json.lOnGnd, options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Callsign] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.Callsign,
        surfaces:           VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey:         'ListCallsign',
        labelKey:           'Callsign',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return !!json.call; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.callsign(json.call, false, false); },
        tooltipCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.reportRouteFull(json.call, json.route); },
        sortColumn:         VRS.ReportSortColumn.Callsign,
        groupValue:         function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.call; }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.CountAdsb] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.CountAdsb,
        headingKey:         'ListCountAdsb',
        labelKey:           'CountAdsb',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.cADSB !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.countMessages(json.cADSB); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.CountModeS] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.CountModeS,
        headingKey:         'ListCountModeS',
        labelKey:           'CountModeS',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.cMDS !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.countMessages(json.cMDS); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.CountPositions] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.CountPositions,
        headingKey:         'ListCountPositions',
        labelKey:           'CountPositions',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.cPOS !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.countMessages(json.cPOS); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Duration] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.Duration,
        headingKey:         'ListDuration',
        labelKey:           'Duration',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.start !== undefined && json.end !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.duration(json.end - json.start, false); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.EndTime] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.EndTime,
        headingKey:         'ListEndTime',
        labelKey:           'EndTime',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.end !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.endDateTime(json.start, json.end, false, options.alwaysShowEndDate); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstAltitude] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstAltitude,
        headingKey:         'ListFirstAltitude',
        labelKey:           'FirstAltitude',
        alignment:          VRS.Alignment.Right,
        sortColumn:         VRS.ReportSortColumn.FirstAltitude,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fAlt !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.altitude(json.fAlt, VRS.AltitudeType.Barometric, json.fOnGnd, options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits, false); },
        groupValue:         function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fAlt; }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstFlightLevel] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstFlightLevel,
        headingKey:         'ListFirstFlightLevel',
        labelKey:           'FirstFlightLevel',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fAlt !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.flightLevel(
            json.fAlt,
            VRS.AltitudeType.Barometric,
            json.fOnGnd,
            options.unitDisplayPreferences.getFlightLevelTransitionAltitude(),
            options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(),
            options.unitDisplayPreferences.getFlightLevelHeightUnit(),
            options.unitDisplayPreferences.getHeightUnit(),
            options.distinguishOnGround,
            options.showUnits,
            false
        ); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstHeading] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstHeading,
        headingKey:         'ListFirstHeading',
        labelKey:           'FirstHeading',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fTrk !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.heading(json.fTrk, false, options.showUnits, false); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstLatitude] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstLatitude,
        headingKey:         'ListFirstLatitude',
        labelKey:           'FirstLatitude',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fLat !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.latitude(json.fLat, options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstLongitude] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstLongitude,
        headingKey:         'ListFirstLongitude',
        labelKey:           'FirstLongitude',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fLng !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.longitude(json.fLng, options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstOnGround] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstOnGround,
        headingKey:         'ListFirstOnGround',
        labelKey:           'FirstOnGround',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fOnGnd !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.isOnGround(json.fOnGnd); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstSpeed] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstSpeed,
        headingKey:         'ListFirstSpeed',
        labelKey:           'FirstSpeed',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fSpd !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) {
            return VRS.format.speed(
                json.fSpd,
                VRS.SpeedType.Ground,
                options.unitDisplayPreferences.getSpeedUnit(),
                options.showUnits,
                false
            );
        }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstSquawk] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstSquawk,
        headingKey:         'ListFirstSquawk',
        labelKey:           'FirstSquawk',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fSqk !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.squawk(json.fSqk); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstVerticalSpeed] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FirstVerticalSpeed,
        headingKey:         'ListFirstVerticalSpeed',
        labelKey:           'FirstVerticalSpeed',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fVsi !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.verticalSpeed(json.fVsi, options.unitDisplayPreferences.getHeightUnit(), options.unitDisplayPreferences.getShowVerticalSpeedPerSecond(), options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FlightLevel] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.FlightLevel,
        headingKey:         'ListFlightLevel',
        labelKey:           'FlightLevel',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fAlt !== undefined || json.lAlt !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.flightLevelFromTo(
            json.fAlt,
            json.fOnGnd,
            json.lAlt,
            json.lOnGnd,
            options.unitDisplayPreferences.getFlightLevelTransitionAltitude(),
            options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(),
            options.unitDisplayPreferences.getFlightLevelHeightUnit(),
            options.unitDisplayPreferences.getHeightUnit(),
            options.distinguishOnGround,
            options.showUnits
        ); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.HadAlert] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.HadAlert,
        headingKey:         'ListHadAlert',
        labelKey:           'HadAlert',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.hAlrt !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.hadAlert(json.hAlrt); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.HadEmergency] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.HadEmergency,
        headingKey:         'ListHadEmergency',
        labelKey:           'HadEmergency',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.hEmg !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.hadEmergency(json.hEmg); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.HadSPI] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.HadSPI,
        headingKey:         'ListHadSPI',
        labelKey:           'HadSPI',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.hSpi !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.hadSPI(json.hSpi); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastAltitude] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastAltitude,
        headingKey:         'ListLastAltitude',
        labelKey:           'LastAltitude',
        alignment:          VRS.Alignment.Right,
        sortColumn:         VRS.ReportSortColumn.LastAltitude,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lAlt !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.altitude(json.lAlt, VRS.AltitudeType.Barometric, json.lOnGnd, options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits, false); },
        groupValue:         function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lAlt; }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastFlightLevel] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastFlightLevel,
        headingKey:         'ListLastFlightLevel',
        labelKey:           'LastFlightLevel',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lAlt !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.flightLevel(
            json.lAlt,
            VRS.AltitudeType.Barometric,
            json.fOnGnd,
            options.unitDisplayPreferences.getFlightLevelTransitionAltitude(),
            options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(),
            options.unitDisplayPreferences.getFlightLevelHeightUnit(),
            options.unitDisplayPreferences.getHeightUnit(),
            options.distinguishOnGround,
            options.showUnits,
            false
        ); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastHeading] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastHeading,
        headingKey:         'ListLastHeading',
        labelKey:           'LastHeading',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lTrk !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.heading(json.lTrk, false, options.showUnits, false); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastLatitude] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastLatitude,
        headingKey:         'ListLastLatitude',
        labelKey:           'LastLatitude',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lLat !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.latitude(json.lLat, options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastLongitude] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastLongitude,
        headingKey:         'ListLastLongitude',
        labelKey:           'LastLongitude',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lLng !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.longitude(json.lLng, options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastOnGround] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastOnGround,
        headingKey:         'ListLastOnGround',
        labelKey:           'LastOnGround',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lOnGnd !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.isOnGround(json.lOnGnd); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastSpeed] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastSpeed,
        headingKey:         'ListLastSpeed',
        labelKey:           'LastSpeed',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lSpd !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) {
            return VRS.format.speed(
                json.lSpd,
                VRS.SpeedType.Ground,
                options.unitDisplayPreferences.getSpeedUnit(),
                options.showUnits,
                false
            );
        }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastSquawk] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastSquawk,
        headingKey:         'ListLastSquawk',
        labelKey:           'LastSquawk',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lSqk !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.squawk(json.lSqk); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastVerticalSpeed] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.LastVerticalSpeed,
        headingKey:         'ListLastVerticalSpeed',
        labelKey:           'LastVerticalSpeed',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.lVsi !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.verticalSpeed(json.lVsi, options.unitDisplayPreferences.getHeightUnit(), options.unitDisplayPreferences.getShowVerticalSpeedPerSecond(), options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.PositionsOnMap] = new VRS.ReportPropertyHandler({
        property:               VRS.ReportFlightProperty.PositionsOnMap,
        surfaces:               VRS.ReportSurface.DetailBody,
        headingKey:             'Map',
        labelKey:               'Map',
        hasValue:               function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fLat || json.fLng || json.lLat || json.lLng; },
        suppressLabelCallback:  function() { return true; },
        createWidget:           function(/** jQuery */ element, /** Object */ options, /** VRS.ReportSurface */ surface) {
            if(surface === VRS.ReportSurface.DetailBody && !VRS.jQueryUIHelper.getReportMapPlugin(element)) {
                element.vrsReportMap(VRS.jQueryUIHelper.getReportMapOptions({
                    plotterOptions:         options.plotterOptions,
                    unitDisplayPreferences: options.unitDisplayPreferences,
                    elementClasses:         'aircraftPosnMap'
                }));
            }
        },
        destroyWidget:          function(/** jQuery */ element, /** VRS.ReportSurface */ surface) {
            var reportMap = VRS.jQueryUIHelper.getReportMapPlugin(element);
            if(surface === VRS.ReportSurface.DetailBody && reportMap) {
                reportMap.destroy();
            }
        },
        renderWidget:           function(/** jQuery */ element, /** VRS_JSON_REPORT_FLIGHT */ flight, /** * */ options, /** VRS.ReportSurface */ surface) {
            var reportMap = VRS.jQueryUIHelper.getReportMapPlugin(element);
            if(surface === VRS.ReportSurface.DetailBody && reportMap) {
                reportMap.showFlight(flight);
            }
        }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.RouteShort] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.RouteShort,
        headingKey:         'ListRoute',
        labelKey:           'RouteShort',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return !!json.route.from; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.reportRouteShort(json.call, json.route, true, false, false); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.RouteFull] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.RouteFull,
        surfaces:           VRS.ReportSurface.DetailBody,
        headingKey:         'ListRoute',
        labelKey:           'Route',
        isMultiLine:        true,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return !!json.route.from; },
        renderCallback:     function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.reportRouteFull(json.call, json.route); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.RowNumber] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.RowNumber,
        headingKey:         'ListRowNumber',
        labelKey:           'RowNumber',
        surfaces:           VRS.ReportSurface.List,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return true; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.stringUtility.format('{0:N0}', json.row); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Speed] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.Speed,
        headingKey:         'ListSpeed',
        labelKey:           'Speed',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fSpd !== undefined || json.lSpd !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.speedFromTo(json.fSpd, json.lSpd, options.unitDisplayPreferences.getSpeedUnit(), options.showUnits); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Squawk] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.Squawk,
        headingKey:         'ListSquawk',
        labelKey:           'Squawk',
        alignment:          VRS.Alignment.Right,
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.fSqk !== undefined || json.lSqk !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json) { return VRS.format.squawkFromTo(json.fSqk, json.lSqk); }
    });

    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.StartTime] = new VRS.ReportPropertyHandler({
        property:           VRS.ReportFlightProperty.StartTime,
        headingKey:         'ListStartTime',
        labelKey:           'StartTime',
        hasValue:           function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.start !== undefined; },
        contentCallback:    function(/** VRS_JSON_REPORT_FLIGHT */ json, /** Object */ options) { return VRS.format.startDateTime(json.start, false, options.justShowStartTime); },
        sortColumn:         VRS.ReportSortColumn.Date,
        groupValue:         function(/** VRS_JSON_REPORT_FLIGHT */ json) { return json.start ? Globalize.format(json.start, 'dddd d MMM yyyy') : null; }
    });
    //endregion

    //endregion

    //region ReportPropertyHandlerHelper
    VRS.ReportPropertyHandlerHelper = function()
    {
        //region -- State save and load helpers
        /**
         * Removes invalid report properties from a list, usually called as part of loading a previous session's state.
         * @param {VRS.ReportProperty[]}    reportPropertyList      An array of VRS.ReportProperty values.
         * @param {VRS.ReportSurface[]}     surfaces                An array of VRS.ReportSurface values that describe the allowable surfaces.
         * @param {number}                 [maximumProperties]      The maximum number of entries allowed in the list. Undefined or -1 places no limit on the length of the list.
         * @returns {VRS.RenderProperty[]}                          The sanitised list of VRS.ReportProperty values.
         */
        this.buildValidReportPropertiesList = function(reportPropertyList, surfaces, maximumProperties)
        {
            maximumProperties = maximumProperties === undefined ? -1 : maximumProperties;
            surfaces = surfaces || [];
            if(surfaces.length === 0) {
                for(var surfaceName in VRS.ReportSurface) {
                    //noinspection JSUnfilteredForInLoop
                    surfaces.push(VRS.ReportSurface[surfaceName]);
                }
            }
            var countSurfaces = surfaces.length;

            var validProperties = [];
            $.each(reportPropertyList, function(idx, property) {
                if(maximumProperties === -1 || validProperties.length <= maximumProperties) {
                    var handler = VRS.reportPropertyHandlers[property];
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

        //region -- addReportPropertyListOptionsToPane
        /**
         * Adds an orderedSubset field to an options pane that allows the user to configure a list of VRS.ReportProperty
         * strings.
         * @param {Object}              settings
         * @param {VRS.OptionPane}      settings.pane           The pane to add the field to.
         * @param {VRS.ReportSurface}   settings.surface        The VRS.ReportSurface to show properties for. The user will be able to select any property that can be rendered to this surface.
         * @param {string}              settings.fieldLabel     The index into VRS.$$ for the field's label.
         * @param {function():string[]} settings.getList        A method that returns a list of VRS.ReportProperty strings representing the properties selected by the user.
         * @param {function(string[])}  settings.setList        A method that takes a list of VRS.ReportProperty strings selected by the user and copies them to the object being configured.
         * @param {function()}          settings.saveState      A method that can save the state of the object being configured.
         * @returns {VRS.OptionFieldOrderedSubset}              The option field that has been created.
         */
        this.addReportPropertyListOptionsToPane = function(settings)
        {
            var pane = settings.pane;
            var surface = settings.surface;
            var fieldLabel = settings.fieldLabel;
            var getList = settings.getList;
            var setList = settings.setList;
            var saveState = settings.saveState;

            var values = [];
            for(var property in VRS.reportPropertyHandlers) {
                var handler = VRS.reportPropertyHandlers[property];
                if(!handler || !handler.isSurfaceSupported(surface)) continue;
                values.push(new VRS.ValueText({ value: handler.property, textKey: handler.optionsLabelKey }));
            }
            values.sort(function(/** VRS.ValueText */ lhs, /** VRS.ValueText */ rhs) {
                var lhsText = lhs.getText();
                var rhsText = rhs.getText();
                return lhsText.localeCompare(rhsText);
            });

            var field = new VRS.OptionFieldOrderedSubset({
                name:           'renderProperties',
                controlType:    VRS.optionControlTypes.orderedSubset,
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

        //region -- findPropertyHandlerForSortColumn
        /**
         * Returns the VRS.ReportPropertyHandler that declares itself as the handler for the sort column passed across.
         * @param {VRS.ReportSortColumn} sortColumn
         * @returns {VRS.ReportPropertyHandler}
         */
        this.findPropertyHandlerForSortColumn = function(sortColumn)
        {
            var result = null;

            if(sortColumn !== VRS.ReportSortColumn.None) {
                for(var property in VRS.reportPropertyHandlers) {
                    var handler = VRS.reportPropertyHandlers[property];
                    if(handler.sortColumn && handler.sortColumn === sortColumn) {
                        result = handler;
                        break;
                    }
                }
            }

            return result;
        };
        //endregion
    };

    VRS.reportPropertyHandlerHelper = new VRS.ReportPropertyHandlerHelper();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
