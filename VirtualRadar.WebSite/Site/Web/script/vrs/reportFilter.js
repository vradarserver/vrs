var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportMaximumCriteria = VRS.globalOptions.reportMaximumCriteria !== undefined ? VRS.globalOptions.reportMaximumCriteria : 15;
    VRS.globalOptions.reportFindAllPermutationsOfCallsign = VRS.globalOptions.reportFindAllPermutationsOfCallsign !== undefined ? VRS.globalOptions.reportFindAllPermutationsOfCallsign : false;
    var ReportFilterPropertyHandler = (function (_super) {
        __extends(ReportFilterPropertyHandler, _super);
        function ReportFilterPropertyHandler(settings) {
            _super.call(this, $.extend({
                propertyEnumObject: VRS.ReportFilterProperty
            }, settings));
        }
        return ReportFilterPropertyHandler;
    })(VRS.FilterPropertyHandler);
    VRS.ReportFilterPropertyHandler = ReportFilterPropertyHandler;
    VRS.reportFilterPropertyHandlers = VRS.reportFilterPropertyHandlers || {};
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Callsign] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.Callsign,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Callsign',
        serverFilterName: 'call-',
        inputWidth: VRS.InputWidth.NineChar,
        defaultCondition: VRS.FilterCondition.Equals
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Country] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.Country,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Country',
        serverFilterName: 'cou-',
        inputWidth: VRS.InputWidth.Long,
        defaultCondition: VRS.FilterCondition.Equals
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Date] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.Date,
        type: VRS.FilterPropertyType.DateRange,
        labelKey: 'Date',
        serverFilterName: 'date-',
        inputWidth: VRS.InputWidth.EightChar
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.HadEmergency] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.HadEmergency,
        type: VRS.FilterPropertyType.OnOff,
        labelKey: 'HadEmergency',
        serverFilterName: 'emg-'
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.FirstAltitude] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.FirstAltitude,
        type: VRS.FilterPropertyType.NumberRange,
        labelKey: 'FirstAltitude',
        serverFilterName: 'falt-'
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Icao] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.Icao,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Icao',
        serverFilterName: 'icao-',
        inputWidth: VRS.InputWidth.SixChar,
        defaultCondition: VRS.FilterCondition.Equals
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.IsMilitary] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.IsMilitary,
        type: VRS.FilterPropertyType.OnOff,
        labelKey: 'IsMilitary',
        serverFilterName: 'mil-'
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.LastAltitude] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.LastAltitude,
        type: VRS.FilterPropertyType.NumberRange,
        labelKey: 'LastAltitude',
        serverFilterName: 'lalt-'
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.ModelIcao] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.ModelIcao,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'ModelIcao',
        serverFilterName: 'typ-',
        inputWidth: VRS.InputWidth.SixChar,
        defaultCondition: VRS.FilterCondition.Equals
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Operator] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.Operator,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Operator',
        serverFilterName: 'op-',
        inputWidth: VRS.InputWidth.Long,
        defaultCondition: VRS.FilterCondition.Contains
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Registration] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.Registration,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Registration',
        serverFilterName: 'reg-',
        inputWidth: VRS.InputWidth.SixChar,
        defaultCondition: VRS.FilterCondition.Equals
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Species] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.Species,
        type: VRS.FilterPropertyType.EnumMatch,
        getEnumValues: function () {
            return [
                new VRS.ValueText({ value: VRS.Species.Amphibian, textKey: 'Amphibian' }),
                new VRS.ValueText({ value: VRS.Species.Gyrocopter, textKey: 'Gyrocopter' }),
                new VRS.ValueText({ value: VRS.Species.Helicopter, textKey: 'Helicopter' }),
                new VRS.ValueText({ value: VRS.Species.LandPlane, textKey: 'LandPlane' }),
                new VRS.ValueText({ value: VRS.Species.SeaPlane, textKey: 'SeaPlane' }),
                new VRS.ValueText({ value: VRS.Species.Tiltwing, textKey: 'Tiltwing' }),
                new VRS.ValueText({ value: VRS.Species.GroundVehicle, textKey: 'GroundVehicle' }),
                new VRS.ValueText({ value: VRS.Species.Tower, textKey: 'RadioMast' })
            ];
        },
        labelKey: 'Species',
        serverFilterName: 'spc-'
    });
    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.WakeTurbulenceCategory] = new VRS.ReportFilterPropertyHandler({
        property: VRS.ReportFilterProperty.WakeTurbulenceCategory,
        type: VRS.FilterPropertyType.EnumMatch,
        getEnumValues: function () {
            return [
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Light, textKey: 'WtcLight' }),
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Medium, textKey: 'WtcMedium' }),
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Heavy, textKey: 'WtcHeavy' })
            ];
        },
        labelKey: 'WakeTurbulenceCategory',
        serverFilterName: 'wtc-'
    });
    var ReportFilter = (function (_super) {
        __extends(ReportFilter, _super);
        function ReportFilter(reportFilterProperty, valueCondition) {
            _super.call(this, {
                property: reportFilterProperty,
                valueCondition: valueCondition,
                propertyEnumObject: VRS.ReportFilterProperty,
                filterPropertyHandlers: VRS.reportFilterPropertyHandlers,
                cloneCallback: function (property, valueCondition) {
                    return new VRS.ReportFilter(property, valueCondition);
                }
            });
        }
        return ReportFilter;
    })(VRS.Filter);
    VRS.ReportFilter = ReportFilter;
    var ReportFilterHelper = (function (_super) {
        __extends(ReportFilterHelper, _super);
        function ReportFilterHelper() {
            _super.call(this, {
                propertyEnumObject: VRS.ReportFilterProperty,
                filterPropertyHandlers: VRS.reportFilterPropertyHandlers,
                createFilterCallback: function (propertyHandler, valueCondition) {
                    return new VRS.ReportFilter(propertyHandler.property, valueCondition);
                }
            });
        }
        return ReportFilterHelper;
    })(VRS.FilterHelper);
    VRS.ReportFilterHelper = ReportFilterHelper;
    VRS.reportFilterHelper = new VRS.ReportFilterHelper();
    var ReportCriteria = (function () {
        function ReportCriteria(settings) {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.ReportCriteria'
            });
            this._Events = {
                criteriaChanged: 'criteriaChanged'
            };
            this._Filters = [];
            this._FindAllPermutationsOfCallsign = VRS.globalOptions.reportFindAllPermutationsOfCallsign;
            if (!settings)
                throw 'You must supply a settings object';
            if (!settings.name)
                throw 'You must supply a name';
            this._Settings = settings;
        }
        ReportCriteria.prototype.getName = function () {
            return this._Settings.name;
        };
        ReportCriteria.prototype.getFindAllPermutationsOfCallsign = function () {
            return this._FindAllPermutationsOfCallsign;
        };
        ReportCriteria.prototype.setFindAllPermutationsOfCallsign = function (value) {
            this._FindAllPermutationsOfCallsign = value;
        };
        ReportCriteria.prototype.hasCriteria = function () {
            return !!this._Filters.length;
        };
        ReportCriteria.prototype.isForSingleAircraft = function () {
            var result = false;
            var length = this._Filters.length;
            for (var i = 0; !result && i < length; ++i) {
                var filter = this._Filters[i];
                switch (filter.getProperty()) {
                    case VRS.ReportFilterProperty.Registration:
                    case VRS.ReportFilterProperty.Icao:
                        result = filter.getValueCondition().getCondition() === VRS.FilterCondition.Equals;
                        if (result) {
                            result = !!filter.getValueCondition().getValue();
                        }
                        break;
                }
            }
            return result;
        };
        ReportCriteria.prototype.hookCriteriaChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.criteriaChanged, callback, forceThis);
        };
        ReportCriteria.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        ReportCriteria.prototype.saveState = function () {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        };
        ReportCriteria.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        };
        ReportCriteria.prototype.applyState = function (settings) {
            this.setFindAllPermutationsOfCallsign(settings.findAllPermutationsOfCallsign);
        };
        ReportCriteria.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        ReportCriteria.prototype.persistenceKey = function () {
            return 'vrsReportCriteria-' + this.getName();
        };
        ReportCriteria.prototype.createSettings = function () {
            return {
                findAllPermutationsOfCallsign: this.getFindAllPermutationsOfCallsign()
            };
        };
        ReportCriteria.prototype.createOptionPane = function (displayOrder) {
            var _this = this;
            var pane = new VRS.OptionPane({
                name: 'reportFilterOptionPane',
                titleKey: 'Criteria',
                displayOrder: displayOrder
            });
            pane.addField(new VRS.OptionFieldCheckBox({
                name: 'findAllPermutations',
                labelKey: 'FindAllPermutationsOfCallsign',
                getValue: function () { return _this._FindAllPermutationsOfCallsign; },
                setValue: function (value) { return _this._FindAllPermutationsOfCallsign = value; },
                saveState: function () { return _this.saveState(); }
            }));
            var panesTypeSettings = VRS.reportFilterHelper.addConfigureFiltersListToPane({
                pane: pane,
                filters: this._Filters,
                saveState: $.noop,
                maxFilters: VRS.globalOptions.reportMaximumCriteria,
                allowableProperties: VRS.enumHelper.getEnumValues(VRS.ReportFilterProperty),
                addFilter: function (newFilter, paneField) {
                    var filter = _this.addFilter(newFilter);
                    if (filter) {
                        paneField.addPane(filter.createOptionPane($.noop));
                    }
                },
                addFilterButtonLabel: 'AddCriteria',
                onlyUniqueFilters: true,
                isAlreadyInUse: function (reportFilterProperty) {
                    return VRS.reportFilterHelper.isFilterInUse(_this._Filters, reportFilterProperty);
                }
            });
            panesTypeSettings.hookPaneRemoved(this.filterPaneRemoved, this);
            return pane;
        };
        ReportCriteria.prototype.addFilter = function (filterOrPropertyId) {
            var filter = filterOrPropertyId;
            if (!(filter instanceof VRS.ReportFilter)) {
                filter = VRS.reportFilterHelper.createFilter(filterOrPropertyId);
            }
            if (VRS.reportFilterHelper.isFilterInUse(this._Filters, filter.getProperty())) {
                filter = null;
            }
            else {
                this._Filters.push(filter);
                this._Dispatcher.raise(this._Events.criteriaChanged);
            }
            return filter;
        };
        ReportCriteria.prototype.filterPaneRemoved = function (pane, index) {
            this.removeFilterAt(index);
        };
        ReportCriteria.prototype.removeFilterAt = function (index) {
            this._Filters.splice(index, 1);
            this._Dispatcher.raise(this._Events.criteriaChanged);
        };
        ReportCriteria.prototype.populateFromQueryString = function () {
            var _this = this;
            this._Filters = [];
            var pageUrl = $.url();
            $.each(VRS.reportFilterPropertyHandlers, function (propertyIdx, propertyHandler) {
                var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
                if (typeHandler && propertyHandler.serverFilterName) {
                    $.each(typeHandler.getConditions(), function (typeIdx, condition) {
                        _this.extractFilterFromQueryString(pageUrl, propertyHandler, typeHandler, condition.condition, condition.reverseCondition);
                    });
                }
            });
            var allCallsignPermutations = pageUrl.param('callPerms');
            if (allCallsignPermutations) {
                this.setFindAllPermutationsOfCallsign(allCallsignPermutations !== '0');
            }
        };
        ReportCriteria.prototype.createQueryString = function (useRelativeDates) {
            var _this = this;
            var result = {};
            var now = new Date();
            var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
            var getRelativeDate = function (dateValue) {
                return !dateValue ? undefined : Math.floor((dateValue.getTime() - today.getTime()) / 86400000);
            };
            if (this._Filters.length) {
                $.each(this._Filters, function (idx, filter) {
                    var valueCondition = filter.getValueCondition();
                    var oneValueCondition = valueCondition;
                    var twoValueCondition = valueCondition;
                    switch (valueCondition.getCondition()) {
                        case VRS.FilterCondition.Contains:
                            _this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'C');
                            break;
                        case VRS.FilterCondition.Ends:
                            _this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'E');
                            break;
                        case VRS.FilterCondition.Equals:
                            _this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'Q');
                            break;
                        case VRS.FilterCondition.Starts:
                            _this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'S');
                            break;
                        case VRS.FilterCondition.Between:
                            var low = twoValueCondition.getValue1();
                            var high = twoValueCondition.getValue2();
                            var valueIsString = filter.getProperty() === VRS.ReportFilterProperty.Date && useRelativeDates;
                            if (valueIsString) {
                                low = getRelativeDate(low);
                                high = getRelativeDate(high);
                            }
                            if (low !== undefined)
                                _this.doAddQueryStringFromFilter(result, filter, low, 'L', valueIsString);
                            if (high !== undefined)
                                _this.doAddQueryStringFromFilter(result, filter, high, 'U', valueIsString);
                            break;
                        default:
                            throw 'Not implemented ' + valueCondition.getCondition();
                    }
                });
            }
            return result;
        };
        ReportCriteria.prototype.extractFilterFromQueryString = function (pageUrl, propertyHandler, typeHandler, condition, reverseCondition) {
            switch (condition) {
                case VRS.FilterCondition.Contains:
                    this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'C');
                    break;
                case VRS.FilterCondition.Ends:
                    this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'E');
                    break;
                case VRS.FilterCondition.Equals:
                    this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'Q');
                    break;
                case VRS.FilterCondition.Starts:
                    this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'S');
                    break;
                case VRS.FilterCondition.Between:
                    var filter = this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'L');
                    this.doExtractFilterFromQueryString(filter, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'U');
                    break;
                default:
                    throw 'Not implemented ' + condition;
            }
        };
        ReportCriteria.prototype.doExtractFilterFromQueryString = function (filter, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, nameSuffix) {
            var result = filter;
            var name = this.getFilterName(propertyHandler, nameSuffix, reverseCondition);
            var text = pageUrl.param(name);
            var value = typeHandler.parseString(text);
            if (value !== undefined) {
                if (!result) {
                    result = VRS.reportFilterHelper.createFilter(propertyHandler.property);
                    this._Filters.push(result);
                    this._Dispatcher.raise(this._Events.criteriaChanged);
                }
                var valueCondition = result.getValueCondition();
                valueCondition.setCondition(condition);
                valueCondition.setReverseCondition(reverseCondition);
                switch (condition) {
                    case VRS.FilterCondition.Between:
                        switch (nameSuffix) {
                            case 'L':
                                valueCondition.setValue1(value);
                                break;
                            case 'U':
                                valueCondition.setValue2(value);
                                break;
                            default: throw 'Not implemented ' + nameSuffix;
                        }
                        break;
                    default:
                        valueCondition.setValue(value);
                        break;
                }
            }
            return result;
        };
        ReportCriteria.prototype.doAddQueryStringFromFilter = function (queryStringParams, filter, value, nameSuffix, valueIsString) {
            var condition = filter.getValueCondition();
            var propertyHandler = filter.getPropertyHandler();
            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            var reverseCondition = condition.getReverseCondition();
            var name = this.getFilterName(propertyHandler, nameSuffix, reverseCondition);
            var stringValue = valueIsString ? value : typeHandler.toQueryString(value);
            queryStringParams[name] = stringValue;
        };
        ReportCriteria.prototype.getFilterName = function (propertyHandler, nameSuffix, reverseCondition) {
            return propertyHandler.serverFilterName + nameSuffix + (reverseCondition ? 'N' : '');
        };
        ReportCriteria.prototype.addToQueryParameters = function (params) {
            VRS.reportFilterHelper.addToQueryParameters(this._Filters, params, this._Settings.unitDisplayPreferences);
            if (this.getFindAllPermutationsOfCallsign()) {
                params.altCall = 1;
            }
        };
        return ReportCriteria;
    })();
    VRS.ReportCriteria = ReportCriteria;
})(VRS || (VRS = {}));
//# sourceMappingURL=reportFilter.js.map