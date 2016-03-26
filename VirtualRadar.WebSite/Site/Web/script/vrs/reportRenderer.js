var VRS;
(function (VRS) {
    var ReportPropertyHandler = (function () {
        function ReportPropertyHandler(settings) {
            this.property = settings.property;
            this.surfaces = settings.surfaces || (VRS.ReportSurface.List + VRS.ReportSurface.DetailBody);
            this.labelKey = settings.labelKey || settings.headingKey;
            this.headingKey = settings.headingKey || settings.labelKey;
            this.optionsLabelKey = settings.optionsLabelKey || settings.labelKey || settings.headingKey;
            this.headingAlignment = settings.headingAlignment || settings.alignment || settings.contentAlignment || VRS.Alignment.Left;
            this.contentAlignment = settings.contentAlignment || settings.alignment || settings.headingAlignment || VRS.Alignment.Left;
            this.isMultiLine = settings.isMultiLine || false;
            this.fixedWidth = settings.fixedWidth;
            this.suppressLabelCallback = settings.suppressLabelCallback || function () { return false; };
            this.sortColumn = settings.sortColumn;
            this.groupValue = settings.groupValue;
            this.isAircraftProperty = !!VRS.enumHelper.getEnumName(VRS.ReportAircraftProperty, settings.property);
            this.isFlightsProperty = !this.isAircraftProperty;
            this._HasValue = settings.hasValue;
            this._CreateWidget = settings.createWidget;
            this._DestroyWidget = settings.destroyWidget;
            this._RenderWidget = settings.renderWidget;
            this._ContentCallback = settings.contentCallback;
            this._RenderCallback = settings.renderCallback;
            this._TooltipCallback = settings.tooltipCallback;
        }
        ReportPropertyHandler.prototype.isSurfaceSupported = function (surface) {
            return (this.surfaces & surface) !== 0;
        };
        ReportPropertyHandler.prototype.hasValue = function (flightJson) {
            var json = this.isAircraftProperty ? flightJson.aircraft : flightJson;
            return this._HasValue(json);
        };
        ReportPropertyHandler.prototype.createWidgetInJQueryElement = function (jQueryElement, surface, options) {
            if (this._CreateWidget) {
                this._CreateWidget(jQueryElement, surface, options);
            }
        };
        ReportPropertyHandler.prototype.destroyWidgetInJQueryElement = function (jQueryElement, surface) {
            if (this._DestroyWidget) {
                this._DestroyWidget(jQueryElement, surface);
            }
        };
        ReportPropertyHandler.prototype.renderIntoJQueryElement = function (jqElement, json, options, surface) {
            if (this._ContentCallback) {
                jqElement.text(this._ContentCallback(json, options, surface));
            }
            else if (this._RenderWidget) {
                this._RenderWidget(jqElement, json, options, surface);
            }
            else {
                jqElement.html(this._RenderCallback(json, options, surface));
            }
        };
        ReportPropertyHandler.prototype.addTooltip = function (jqElement, json, options) {
            if (this._TooltipCallback) {
                var text = this._TooltipCallback(json, options);
                if (text) {
                    jqElement.attr('title', text);
                }
            }
        };
        return ReportPropertyHandler;
    })();
    VRS.ReportPropertyHandler = ReportPropertyHandler;
    VRS.reportPropertyHandlers = VRS.reportPropertyHandlers || {};
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.AircraftClass] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.AircraftClass,
        headingKey: 'ListAircraftClass',
        labelKey: 'AircraftClass',
        hasValue: function (json) { return !!json.acClass; },
        contentCallback: function (json) { return VRS.format.aircraftClass(json.acClass); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.CofACategory] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.CofACategory,
        headingKey: 'ListCofACategory',
        labelKey: 'CofACategory',
        hasValue: function (json) { return !!json.cofACategory; },
        contentCallback: function (json) { return VRS.format.certOfACategory(json.cofACategory); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.CofAExpiry] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.CofAExpiry,
        headingKey: 'ListCofAExpiry',
        labelKey: 'CofAExpiry',
        hasValue: function (json) { return !!json.cofAExpiry; },
        contentCallback: function (json) { return VRS.format.certOfAExpiry(json.cofAExpiry); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Country] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Country,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListCountry',
        labelKey: 'Country',
        hasValue: function (json) { return !!json.country; },
        contentCallback: function (json) { return VRS.format.country(json.country); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.CurrentRegDate] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.CurrentRegDate,
        headingKey: 'ListCurrentRegDate',
        labelKey: 'CurrentRegDate',
        hasValue: function (json) { return !!json.curRegDate; },
        contentCallback: function (json) { return VRS.format.currentRegistrationDate(json.curRegDate); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.DeRegDate] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.DeRegDate,
        headingKey: 'ListDeRegDate',
        labelKey: 'DeRegDate',
        hasValue: function (json) { return !!json.deregDate; },
        contentCallback: function (json) { return VRS.format.deregisteredDate(json.deregDate); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Engines] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Engines,
        headingKey: 'ListEngines',
        labelKey: 'Engines',
        hasValue: function (json) { return json.engType !== undefined && json.engines !== undefined; },
        contentCallback: function (json) { return VRS.format.engines(json.engines, json.engType); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.FirstRegDate] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.FirstRegDate,
        headingKey: 'ListFirstRegDate',
        labelKey: 'FirstRegDate',
        hasValue: function (json) { return !!json.firstRegDate; },
        contentCallback: function (json) { return VRS.format.firstRegistrationDate(json.firstRegDate); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.GenericName] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.GenericName,
        headingKey: 'ListGenericName',
        labelKey: 'GenericName',
        hasValue: function (json) { return json.genericName !== undefined; },
        contentCallback: function (json) { return VRS.format.genericName(json.genericName); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Icao] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Icao,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListIcao',
        labelKey: 'Icao',
        hasValue: function (json) { return !!json.icao; },
        contentCallback: function (json) { return VRS.format.icao(json.icao); },
        sortColumn: VRS.ReportSortColumn.Icao,
        groupValue: function (json) { return json.icao; }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Interesting] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Interesting,
        headingKey: 'ListInteresting',
        labelKey: 'Interesting',
        hasValue: function (json) { return json.interested !== undefined; },
        contentCallback: function (json) { return VRS.format.userInterested(json.interested); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Manufacturer] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Manufacturer,
        headingKey: 'ListManufacturer',
        labelKey: 'Manufacturer',
        hasValue: function (json) { return !!json.manufacturer; },
        contentCallback: function (json) { return VRS.format.manufacturer(json.manufacturer); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Military] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Military,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListCivOrMil',
        labelKey: 'CivilOrMilitary',
        hasValue: function (json) { return true; },
        contentCallback: function (json) { return VRS.format.isMilitary(!!json.military); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Model] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Model,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListModel',
        labelKey: 'Model',
        hasValue: function (json) { return !!json.typ; },
        contentCallback: function (json) { return VRS.format.model(json.typ); },
        sortColumn: VRS.ReportSortColumn.Model,
        groupValue: function (json) { return json.typ; }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.ModelIcao] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.ModelIcao,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListModelIcao',
        labelKey: 'ModelIcao',
        hasValue: function (json) { return !!json.icaoType; },
        contentCallback: function (json) { return VRS.format.modelIcao(json.icaoType); },
        sortColumn: VRS.ReportSortColumn.ModelIcao,
        groupValue: function (json) { return json.icaoType; }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.ModeSCountry] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.ModeSCountry,
        headingKey: 'ListModeSCountry',
        labelKey: 'ModeSCountry',
        hasValue: function (json) { return !!json.modeSCountry; },
        contentCallback: function (json) { return VRS.format.modeSCountry(json.modeSCountry); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.MTOW] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.MTOW,
        headingKey: 'ListMaxTakeoffWeight',
        labelKey: 'MaxTakeoffWeight',
        hasValue: function (json) { return !!json.mtow; },
        contentCallback: function (json) { return VRS.format.maxTakeoffWeight(json.mtow); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Notes] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Notes,
        headingKey: 'ListNotes',
        labelKey: 'Notes',
        hasValue: function (json) { return !!json.notes; },
        contentCallback: function (json) { return VRS.format.notes(json.notes); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Operator] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Operator,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListOperator',
        labelKey: 'Operator',
        hasValue: function (json) { return !!json.owner; },
        contentCallback: function (json) { return VRS.format.operator(json.owner); },
        sortColumn: VRS.ReportSortColumn.Operator,
        groupValue: function (json) { return json.owner; }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.OperatorFlag] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.OperatorFlag,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListOperatorFlag',
        labelKey: 'OperatorFlag',
        headingAlignment: VRS.Alignment.Centre,
        fixedWidth: function () { return VRS.globalOptions.aircraftOperatorFlagSize.width.toString() + 'px'; },
        hasValue: function (json) { return !!json.opFlag || !!json.icao || !!json.reg; },
        renderCallback: function (json) { return VRS.format.operatorIcaoImageHtml(json.owner, json.opFlag, json.icao, json.reg); },
        tooltipCallback: function (json) { return VRS.format.operatorIcaoAndName(json.owner, json.opFlag); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.OperatorIcao] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.OperatorIcao,
        headingKey: 'ListOperatorIcao',
        labelKey: 'OperatorCode',
        hasValue: function (json) { return !!json.opFlag; },
        contentCallback: function (json) { return VRS.format.operatorIcao(json.opFlag); },
        tooltipCallback: function (json) { return VRS.format.operatorIcaoAndName(json.owner, json.opFlag); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.OwnershipStatus] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.OwnershipStatus,
        headingKey: 'ListOwnershipStatus',
        labelKey: 'OwnershipStatus',
        hasValue: function (json) { return !!json.ownerStatus; },
        contentCallback: function (json) { return VRS.format.ownershipStatus(json.ownerStatus); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Picture] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Picture,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailBody,
        headingKey: 'ListPicture',
        labelKey: 'Picture',
        headingAlignment: VRS.Alignment.Centre,
        isMultiLine: true,
        fixedWidth: function (surface) {
            switch (surface) {
                case VRS.ReportSurface.List:
                    return VRS.globalOptions.aircraftPictureSizeList.width.toString() + 'px';
                default:
                    return null;
            }
        },
        hasValue: function (json) { return json.hasPic; },
        suppressLabelCallback: function (surface) { return surface === VRS.ReportSurface.DetailBody; },
        renderCallback: function (json, options, surface) {
            switch (surface) {
                case VRS.ReportSurface.List:
                    return VRS.format.pictureHtml(json.reg, json.icao, json.picX, json.picY, VRS.globalOptions.aircraftPictureSizeList);
                case VRS.ReportSurface.DetailBody:
                    return VRS.format.pictureHtml(json.reg, json.icao, json.picX, json.picY, !VRS.globalOptions.isMobile ? VRS.globalOptions.aircraftPictureSizeDesktopDetail
                        : VRS.browserHelper.isProbablyPhone() ? VRS.globalOptions.aircraftPictureSizeIPhoneDetail
                            : VRS.globalOptions.aircraftPictureSizeIPadDetail, false, true);
                default:
                    throw 'Unexpected surface ' + surface;
            }
        }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.PopularName] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.PopularName,
        headingKey: 'ListPopularName',
        labelKey: 'PopularName',
        hasValue: function (json) { return !!json.popularName; },
        contentCallback: function (json) { return VRS.format.popularName(json.popularName); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.PreviousId] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.PreviousId,
        headingKey: 'ListPreviousId',
        labelKey: 'PreviousId',
        hasValue: function (json) { return !!json.previousId; },
        contentCallback: function (json) { return VRS.format.previousId(json.previousId); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Registration] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Registration,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListRegistration',
        labelKey: 'Registration',
        hasValue: function (json) { return !!json.reg; },
        contentCallback: function (json) { return VRS.format.registration(json.reg); },
        sortColumn: VRS.ReportSortColumn.Registration,
        groupValue: function (json) { return json.reg; }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.SerialNumber] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.SerialNumber,
        headingKey: 'ListSerialNumber',
        labelKey: 'SerialNumber',
        hasValue: function (json) { return !!json.serial; },
        contentCallback: function (json) { return VRS.format.serial(json.serial); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Silhouette] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Silhouette,
        headingKey: 'ListModelSilhouette',
        labelKey: 'Silhouette',
        surfaces: VRS.ReportSurface.List,
        headingAlignment: VRS.Alignment.Centre,
        fixedWidth: function () { return VRS.globalOptions.aircraftSilhouetteSize.width.toString() + 'px'; },
        hasValue: function (json) { return !!json.icaoType || !!json.icao || !!json.reg; },
        renderCallback: function (json) { return VRS.format.modelIcaoImageHtml(json.icaoType, json.icao, json.reg); },
        tooltipCallback: function (json) { return VRS.format.modelIcaoNameAndDetail(json.icaoType, json.typ, json.engines, json.engType, json.species, json.wtc); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Species] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Species,
        headingKey: 'ListSpecies',
        labelKey: 'Species',
        hasValue: function (json) { return json.species !== undefined; },
        contentCallback: function (json) { return VRS.format.species(json.species); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.Status] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.Status,
        headingKey: 'ListStatus',
        labelKey: 'Status',
        hasValue: function (json) { return !!json.status; },
        contentCallback: function (json) { return VRS.format.status(json.status); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.TotalHours] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.TotalHours,
        headingKey: 'ListTotalHours',
        labelKey: 'TotalHours',
        hasValue: function (json) { return !!json.totalHours; },
        contentCallback: function (json) { return VRS.format.totalHours(json.totalHours); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.YearBuilt] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.YearBuilt,
        headingKey: 'ListYearBuilt',
        labelKey: 'YearBuilt',
        hasValue: function (json) { return !!json.yearBuilt; },
        contentCallback: function (json) { return VRS.format.yearBuilt(json.yearBuilt); }
    });
    VRS.reportPropertyHandlers[VRS.ReportAircraftProperty.WakeTurbulenceCategory] = new VRS.ReportPropertyHandler({
        property: VRS.ReportAircraftProperty.WakeTurbulenceCategory,
        headingKey: 'ListWtc',
        labelKey: 'WakeTurbulenceCategory',
        hasValue: function (json) { return json.wtc !== undefined; },
        contentCallback: function (json) { return VRS.format.wakeTurbulenceCat(json.wtc, true, false); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Altitude] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.Altitude,
        headingKey: 'ListAltitude',
        labelKey: 'Altitude',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fAlt !== undefined || json.lAlt !== undefined; },
        contentCallback: function (json, options) { return VRS.format.altitudeFromTo(json.fAlt, json.fOnGnd, json.lAlt, json.lOnGnd, options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Callsign] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.Callsign,
        surfaces: VRS.ReportSurface.List + VRS.ReportSurface.DetailHead,
        headingKey: 'ListCallsign',
        labelKey: 'Callsign',
        hasValue: function (json) { return !!json.call; },
        contentCallback: function (json) { return VRS.format.callsign(json.call, false, false); },
        tooltipCallback: function (json) { return VRS.format.reportRouteFull(json.call, json.route); },
        sortColumn: VRS.ReportSortColumn.Callsign,
        groupValue: function (json) { return json.call; }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.CountAdsb] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.CountAdsb,
        headingKey: 'ListCountAdsb',
        labelKey: 'CountAdsb',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.cADSB !== undefined; },
        contentCallback: function (json) { return VRS.format.countMessages(json.cADSB); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.CountModeS] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.CountModeS,
        headingKey: 'ListCountModeS',
        labelKey: 'CountModeS',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.cMDS !== undefined; },
        contentCallback: function (json) { return VRS.format.countMessages(json.cMDS); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.CountPositions] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.CountPositions,
        headingKey: 'ListCountPositions',
        labelKey: 'CountPositions',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.cPOS !== undefined; },
        contentCallback: function (json) { return VRS.format.countMessages(json.cPOS); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Duration] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.Duration,
        headingKey: 'ListDuration',
        labelKey: 'Duration',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.start !== undefined && json.end !== undefined; },
        contentCallback: function (json) { return VRS.format.duration((json.end) - (json.start), false); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.EndTime] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.EndTime,
        headingKey: 'ListEndTime',
        labelKey: 'EndTime',
        hasValue: function (json) { return json.end !== undefined; },
        contentCallback: function (json, options) { return VRS.format.endDateTime(json.start, json.end, false, options.alwaysShowEndDate); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstAltitude] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstAltitude,
        headingKey: 'ListFirstAltitude',
        labelKey: 'FirstAltitude',
        alignment: VRS.Alignment.Right,
        sortColumn: VRS.ReportSortColumn.FirstAltitude,
        hasValue: function (json) { return json.fAlt !== undefined; },
        contentCallback: function (json, options) { return VRS.format.altitude(json.fAlt, VRS.AltitudeType.Barometric, json.fOnGnd, options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits, false); },
        groupValue: function (json) { return json.fAlt; }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstFlightLevel] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstFlightLevel,
        headingKey: 'ListFirstFlightLevel',
        labelKey: 'FirstFlightLevel',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fAlt !== undefined; },
        contentCallback: function (json, options) {
            return VRS.format.flightLevel(json.fAlt, json.fAlt, VRS.AltitudeType.Barometric, json.fOnGnd, options.unitDisplayPreferences.getFlightLevelTransitionAltitude(), options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(), options.unitDisplayPreferences.getFlightLevelHeightUnit(), options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits, false);
        }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstHeading] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstHeading,
        headingKey: 'ListFirstHeading',
        labelKey: 'FirstHeading',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fTrk !== undefined; },
        contentCallback: function (json, options) { return VRS.format.heading(json.fTrk, false, options.showUnits, false); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstLatitude] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstLatitude,
        headingKey: 'ListFirstLatitude',
        labelKey: 'FirstLatitude',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fLat !== undefined; },
        contentCallback: function (json, options) { return VRS.format.latitude(json.fLat, options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstLongitude] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstLongitude,
        headingKey: 'ListFirstLongitude',
        labelKey: 'FirstLongitude',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fLng !== undefined; },
        contentCallback: function (json, options) { return VRS.format.longitude(json.fLng, options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstOnGround] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstOnGround,
        headingKey: 'ListFirstOnGround',
        labelKey: 'FirstOnGround',
        hasValue: function (json) { return json.fOnGnd !== undefined; },
        contentCallback: function (json) { return VRS.format.isOnGround(json.fOnGnd); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstSpeed] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstSpeed,
        headingKey: 'ListFirstSpeed',
        labelKey: 'FirstSpeed',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fSpd !== undefined; },
        contentCallback: function (json, options) {
            return VRS.format.speed(json.fSpd, VRS.SpeedType.Ground, options.unitDisplayPreferences.getSpeedUnit(), options.showUnits, false);
        }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstSquawk] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstSquawk,
        headingKey: 'ListFirstSquawk',
        labelKey: 'FirstSquawk',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fSqk !== undefined; },
        contentCallback: function (json) { return VRS.format.squawk(json.fSqk); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FirstVerticalSpeed] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FirstVerticalSpeed,
        headingKey: 'ListFirstVerticalSpeed',
        labelKey: 'FirstVerticalSpeed',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fVsi !== undefined; },
        contentCallback: function (json, options) { return VRS.format.verticalSpeed(json.fVsi, VRS.AltitudeType.Barometric, options.unitDisplayPreferences.getHeightUnit(), options.unitDisplayPreferences.getShowVerticalSpeedPerSecond(), options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.FlightLevel] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.FlightLevel,
        headingKey: 'ListFlightLevel',
        labelKey: 'FlightLevel',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fAlt !== undefined || json.lAlt !== undefined; },
        contentCallback: function (json, options) {
            return VRS.format.flightLevelFromTo(json.fAlt, json.fOnGnd, json.lAlt, json.lOnGnd, options.unitDisplayPreferences.getFlightLevelTransitionAltitude(), options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(), options.unitDisplayPreferences.getFlightLevelHeightUnit(), options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits);
        }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.HadAlert] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.HadAlert,
        headingKey: 'ListHadAlert',
        labelKey: 'HadAlert',
        hasValue: function (json) { return json.hAlrt !== undefined; },
        contentCallback: function (json) { return VRS.format.hadAlert(json.hAlrt); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.HadEmergency] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.HadEmergency,
        headingKey: 'ListHadEmergency',
        labelKey: 'HadEmergency',
        hasValue: function (json) { return json.hEmg !== undefined; },
        contentCallback: function (json) { return VRS.format.hadEmergency(json.hEmg); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.HadSPI] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.HadSPI,
        headingKey: 'ListHadSPI',
        labelKey: 'HadSPI',
        hasValue: function (json) { return json.hSpi !== undefined; },
        contentCallback: function (json) { return VRS.format.hadSPI(json.hSpi); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastAltitude] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastAltitude,
        headingKey: 'ListLastAltitude',
        labelKey: 'LastAltitude',
        alignment: VRS.Alignment.Right,
        sortColumn: VRS.ReportSortColumn.LastAltitude,
        hasValue: function (json) { return json.lAlt !== undefined; },
        contentCallback: function (json, options) { return VRS.format.altitude(json.lAlt, VRS.AltitudeType.Barometric, json.lOnGnd, options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits, false); },
        groupValue: function (json) { return json.lAlt; }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastFlightLevel] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastFlightLevel,
        headingKey: 'ListLastFlightLevel',
        labelKey: 'LastFlightLevel',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.lAlt !== undefined; },
        contentCallback: function (json, options) {
            return VRS.format.flightLevel(json.lAlt, json.lAlt, VRS.AltitudeType.Barometric, json.fOnGnd, options.unitDisplayPreferences.getFlightLevelTransitionAltitude(), options.unitDisplayPreferences.getFlightLevelTransitionHeightUnit(), options.unitDisplayPreferences.getFlightLevelHeightUnit(), options.unitDisplayPreferences.getHeightUnit(), options.distinguishOnGround, options.showUnits, false);
        }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastHeading] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastHeading,
        headingKey: 'ListLastHeading',
        labelKey: 'LastHeading',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.lTrk !== undefined; },
        contentCallback: function (json, options) { return VRS.format.heading(json.lTrk, false, options.showUnits, false); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastLatitude] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastLatitude,
        headingKey: 'ListLastLatitude',
        labelKey: 'LastLatitude',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.lLat !== undefined; },
        contentCallback: function (json, options) { return VRS.format.latitude(json.lLat, options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastLongitude] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastLongitude,
        headingKey: 'ListLastLongitude',
        labelKey: 'LastLongitude',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.lLng !== undefined; },
        contentCallback: function (json, options) { return VRS.format.longitude(json.lLng, options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastOnGround] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastOnGround,
        headingKey: 'ListLastOnGround',
        labelKey: 'LastOnGround',
        hasValue: function (json) { return json.lOnGnd !== undefined; },
        contentCallback: function (json) { return VRS.format.isOnGround(json.lOnGnd); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastSpeed] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastSpeed,
        headingKey: 'ListLastSpeed',
        labelKey: 'LastSpeed',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.lSpd !== undefined; },
        contentCallback: function (json, options) {
            return VRS.format.speed(json.lSpd, VRS.SpeedType.Ground, options.unitDisplayPreferences.getSpeedUnit(), options.showUnits, false);
        }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastSquawk] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastSquawk,
        headingKey: 'ListLastSquawk',
        labelKey: 'LastSquawk',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.lSqk !== undefined; },
        contentCallback: function (json) { return VRS.format.squawk(json.lSqk); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.LastVerticalSpeed] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.LastVerticalSpeed,
        headingKey: 'ListLastVerticalSpeed',
        labelKey: 'LastVerticalSpeed',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.lVsi !== undefined; },
        contentCallback: function (json, options) { return VRS.format.verticalSpeed(json.lVsi, VRS.AltitudeType.Barometric, options.unitDisplayPreferences.getHeightUnit(), options.unitDisplayPreferences.getShowVerticalSpeedPerSecond(), options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.PositionsOnMap] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.PositionsOnMap,
        surfaces: VRS.ReportSurface.DetailBody,
        headingKey: 'Map',
        labelKey: 'Map',
        hasValue: function (json) { return !!json.fLat || !!json.fLng || !!json.lLat || !!json.lLng; },
        suppressLabelCallback: function () { return true; },
        createWidget: function (element, surface, options) {
            if (surface === VRS.ReportSurface.DetailBody && !VRS.jQueryUIHelper.getReportMapPlugin(element)) {
                element.vrsReportMap(VRS.jQueryUIHelper.getReportMapOptions({
                    plotterOptions: options.plotterOptions,
                    unitDisplayPreferences: options.unitDisplayPreferences,
                    elementClasses: 'aircraftPosnMap'
                }));
            }
        },
        destroyWidget: function (element, surface) {
            var reportMap = VRS.jQueryUIHelper.getReportMapPlugin(element);
            if (surface === VRS.ReportSurface.DetailBody && reportMap) {
                reportMap.destroy();
            }
        },
        renderWidget: function (element, flight, options, surface) {
            var reportMap = VRS.jQueryUIHelper.getReportMapPlugin(element);
            if (surface === VRS.ReportSurface.DetailBody && reportMap) {
                reportMap.showFlight(flight);
            }
        }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.RouteShort] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.RouteShort,
        headingKey: 'ListRoute',
        labelKey: 'RouteShort',
        hasValue: function (json) { return !!json.route.from; },
        contentCallback: function (json) { return VRS.format.reportRouteShort(json.call, json.route, true, false); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.RouteFull] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.RouteFull,
        surfaces: VRS.ReportSurface.DetailBody,
        headingKey: 'ListRoute',
        labelKey: 'Route',
        isMultiLine: true,
        hasValue: function (json) { return !!json.route.from; },
        renderCallback: function (json, options) { return VRS.format.reportRouteFull(json.call, json.route); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.RowNumber] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.RowNumber,
        headingKey: 'ListRowNumber',
        labelKey: 'RowNumber',
        surfaces: VRS.ReportSurface.List,
        hasValue: function (json) { return true; },
        contentCallback: function (json) { return VRS.stringUtility.format('{0:N0}', json.row); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Speed] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.Speed,
        headingKey: 'ListSpeed',
        labelKey: 'Speed',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fSpd !== undefined || json.lSpd !== undefined; },
        contentCallback: function (json, options) { return VRS.format.speedFromTo(json.fSpd, json.lSpd, options.unitDisplayPreferences.getSpeedUnit(), options.showUnits); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.Squawk] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.Squawk,
        headingKey: 'ListSquawk',
        labelKey: 'Squawk',
        alignment: VRS.Alignment.Right,
        hasValue: function (json) { return json.fSqk !== undefined || json.lSqk !== undefined; },
        contentCallback: function (json) { return VRS.format.squawkFromTo(json.fSqk, json.lSqk); }
    });
    VRS.reportPropertyHandlers[VRS.ReportFlightProperty.StartTime] = new VRS.ReportPropertyHandler({
        property: VRS.ReportFlightProperty.StartTime,
        headingKey: 'ListStartTime',
        labelKey: 'StartTime',
        hasValue: function (json) { return json.start !== undefined; },
        contentCallback: function (json, options) { return VRS.format.startDateTime(json.start, false, options.justShowStartTime); },
        sortColumn: VRS.ReportSortColumn.Date,
        groupValue: function (json) { return json.start ? Globalize.format(json.start, 'dddd d MMM yyyy') : null; }
    });
    var ReportPropertyHandlerHelper = (function () {
        function ReportPropertyHandlerHelper() {
        }
        ReportPropertyHandlerHelper.prototype.buildValidReportPropertiesList = function (reportPropertyList, surfaces, maximumProperties) {
            maximumProperties = maximumProperties === undefined ? -1 : maximumProperties;
            surfaces = surfaces || [];
            if (surfaces.length === 0) {
                for (var surfaceName in VRS.ReportSurface) {
                    surfaces.push(VRS.ReportSurface[surfaceName]);
                }
            }
            var countSurfaces = surfaces.length;
            var validProperties = [];
            $.each(reportPropertyList, function (idx, property) {
                if (maximumProperties === -1 || validProperties.length <= maximumProperties) {
                    var handler = VRS.reportPropertyHandlers[property];
                    if (handler) {
                        var surfaceSupported = false;
                        for (var i = 0; i < countSurfaces; ++i) {
                            surfaceSupported = handler.isSurfaceSupported(surfaces[i]);
                            if (surfaceSupported)
                                break;
                        }
                        if (surfaceSupported)
                            validProperties.push(property);
                    }
                }
            });
            return validProperties;
        };
        ReportPropertyHandlerHelper.prototype.addReportPropertyListOptionsToPane = function (settings) {
            var pane = settings.pane;
            var surface = settings.surface;
            var fieldLabel = settings.fieldLabel;
            var getList = settings.getList;
            var setList = settings.setList;
            var saveState = settings.saveState;
            var values = [];
            for (var property in VRS.reportPropertyHandlers) {
                var handler = VRS.reportPropertyHandlers[property];
                if (!handler || !handler.isSurfaceSupported(surface))
                    continue;
                values.push(new VRS.ValueText({ value: handler.property, textKey: handler.optionsLabelKey }));
            }
            values.sort(function (lhs, rhs) {
                var lhsText = lhs.getText();
                var rhsText = rhs.getText();
                return lhsText.localeCompare(rhsText);
            });
            var field = new VRS.OptionFieldOrderedSubset({
                name: 'renderProperties',
                controlType: VRS.optionControlTypes.orderedSubset,
                labelKey: fieldLabel,
                getValue: getList,
                setValue: setList,
                saveState: saveState,
                values: values
            });
            pane.addField(field);
            return field;
        };
        ReportPropertyHandlerHelper.prototype.findPropertyHandlerForSortColumn = function (sortColumn) {
            var result = null;
            if (sortColumn !== VRS.ReportSortColumn.None) {
                for (var property in VRS.reportPropertyHandlers) {
                    var handler = VRS.reportPropertyHandlers[property];
                    if (handler.sortColumn && handler.sortColumn === sortColumn) {
                        result = handler;
                        break;
                    }
                }
            }
            return result;
        };
        return ReportPropertyHandlerHelper;
    })();
    VRS.ReportPropertyHandlerHelper = ReportPropertyHandlerHelper;
    VRS.reportPropertyHandlerHelper = new VRS.ReportPropertyHandlerHelper();
})(VRS || (VRS = {}));
//# sourceMappingURL=reportRenderer.js.map