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
 * @fileoverview Abstracts away details of a report's criteria.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportMaximumCriteria = VRS.globalOptions.reportMaximumCriteria !== undefined ? VRS.globalOptions.reportMaximumCriteria : 15;                             // The maximum number of criteria that can be passed to a report.
    VRS.globalOptions.reportFindAllPermutationsOfCallsign = VRS.globalOptions.reportFindAllPermutationsOfCallsign !== undefined ? VRS.globalOptions.reportFindAllPermutationsOfCallsign : false;  // True if all permutations of a callsign should be found.

    /**
     * The settings to use when creating new instances of ReportFilterPropertyHandler
     */
    export interface ReportFilterPropertyHandler_Settings extends FilterPropertyHandler_Settings
    {
        property: ReportFilterPropertyEnum;
    }

    /**
     * The filter property handler to use when requesting reports.
     */
    export class ReportFilterPropertyHandler extends FilterPropertyHandler
    {
        constructor(settings: ReportFilterPropertyHandler_Settings)
        {
            super($.extend({
                propertyEnumObject: VRS.ReportFilterProperty
            }, settings));
        }
    }

    /*
     * Report filters
     */
    export var reportFilterPropertyHandlers: { [index: string /* ReportFilterPropertyEnum */]: ReportFilterPropertyHandler } = VRS.reportFilterPropertyHandlers || {};

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Callsign] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.Callsign,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Callsign',
        serverFilterName:   'call-',
        inputWidth:         VRS.InputWidth.NineChar,
        defaultCondition:   VRS.FilterCondition.Equals
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Country] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.Country,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Country',
        serverFilterName:   'cou-',
        inputWidth:         VRS.InputWidth.Long,
        defaultCondition:   VRS.FilterCondition.Equals
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Date] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.Date,
        type:               VRS.FilterPropertyType.DateRange,
        labelKey:           'Date',
        serverFilterName:   'date-',
        inputWidth:         VRS.InputWidth.EightChar
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.HadEmergency] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.HadEmergency,
        type:               VRS.FilterPropertyType.OnOff,
        labelKey:           'HadEmergency',
        serverFilterName:   'emg-'
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.FirstAltitude] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.FirstAltitude,
        type:               VRS.FilterPropertyType.NumberRange,
        labelKey:           'FirstAltitude',
        serverFilterName:   'falt-'
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Icao] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.Icao,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Icao',
        serverFilterName:   'icao-',
        inputWidth:         VRS.InputWidth.SixChar,
        defaultCondition:   VRS.FilterCondition.Equals
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.IsMilitary] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.IsMilitary,
        type:               VRS.FilterPropertyType.OnOff,
        labelKey:           'IsMilitary',
        serverFilterName:   'mil-'
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.LastAltitude] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.LastAltitude,
        type:               VRS.FilterPropertyType.NumberRange,
        labelKey:           'LastAltitude',
        serverFilterName:   'lalt-'
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.ModelIcao] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.ModelIcao,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'ModelIcao',
        serverFilterName:   'typ-',
        inputWidth:         VRS.InputWidth.SixChar,
        defaultCondition:   VRS.FilterCondition.Equals
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Operator] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.Operator,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Operator',
        serverFilterName:   'op-',
        inputWidth:         VRS.InputWidth.Long,
        defaultCondition:   VRS.FilterCondition.Contains
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Registration] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.Registration,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Registration',
        serverFilterName:   'reg-',
        inputWidth:         VRS.InputWidth.SixChar,
        defaultCondition:   VRS.FilterCondition.Equals
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.Species] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.Species,
        type:               VRS.FilterPropertyType.EnumMatch,
        getEnumValues:      function() {
            return [
                new VRS.ValueText({ value: VRS.Species.Amphibian,       textKey: 'Amphibian' }),
                new VRS.ValueText({ value: VRS.Species.Gyrocopter,      textKey: 'Gyrocopter' }),
                new VRS.ValueText({ value: VRS.Species.Helicopter,      textKey: 'Helicopter' }),
                new VRS.ValueText({ value: VRS.Species.LandPlane,       textKey: 'LandPlane' }),
                new VRS.ValueText({ value: VRS.Species.SeaPlane,        textKey: 'SeaPlane' }),
                new VRS.ValueText({ value: VRS.Species.Tiltwing,        textKey: 'Tiltwing' }),
                new VRS.ValueText({ value: VRS.Species.GroundVehicle,   textKey: 'GroundVehicle' }),
                new VRS.ValueText({ value: VRS.Species.Tower,           textKey: 'RadioMast' })
            ];
        },
        labelKey:           'Species',
        serverFilterName:   'spc-'
    });

    VRS.reportFilterPropertyHandlers[VRS.ReportFilterProperty.WakeTurbulenceCategory] = new VRS.ReportFilterPropertyHandler({
        property:           VRS.ReportFilterProperty.WakeTurbulenceCategory,
        type:               VRS.FilterPropertyType.EnumMatch,
        getEnumValues:      function () {
            return [
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Light,  textKey: 'WtcLight' }),
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Medium, textKey: 'WtcMedium' }),
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Heavy,  textKey: 'WtcHeavy' })
            ];
        },
        labelKey:           'WakeTurbulenceCategory',
        serverFilterName:   'wtc-'
    });

    /**
     * Brings together a report filter property and a value/condition object to describe a criteria in the report.
     */
    export class ReportFilter extends Filter
    {
        constructor(reportFilterProperty: ReportFilterPropertyEnum, valueCondition: ValueCondition)
        {
            super({
                property:               reportFilterProperty,
                valueCondition:         valueCondition,
                propertyEnumObject:     VRS.ReportFilterProperty,
                filterPropertyHandlers: VRS.reportFilterPropertyHandlers,
                cloneCallback:          function(property, valueCondition) {
                    return new VRS.ReportFilter(property, valueCondition);
                }
            });
        }
    }

    /**
     * A helper class that can ease the tedium of creating report filter objects, serialising them etc.
     */
    export class ReportFilterHelper extends FilterHelper
    {
        constructor()
        {
            super({
                propertyEnumObject:     VRS.ReportFilterProperty,
                filterPropertyHandlers: VRS.reportFilterPropertyHandlers,
                createFilterCallback:   function(propertyHandler, valueCondition) {
                    return new VRS.ReportFilter(propertyHandler.property, valueCondition);
                }
            });
        }
    }
    export var reportFilterHelper = new VRS.ReportFilterHelper();

    /**
     * The settings to use when creating a new instance of ReportCriteria.
     */
    export interface ReportCriteria_Settings
    {
        /**
         * A unique name that distinguishes this object from other instances of the same class.
         */
        name: string; 

        /**
         * The unit display preferences to use when applying criteria.
         */
        unitDisplayPreferences?: UnitDisplayPreferences;
    }

    /**
     * The settings that a ReportCriteria object can persist between sessions.
     */
    export interface ReportCriteria_SaveState
    {
        findAllPermutationsOfCallsign: boolean;
    }

    /**
     * Collects together a bunch of report filters which together describe all of the criteria for a report.
     */
    export class ReportCriteria implements ISelfPersist<ReportCriteria_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.ReportCriteria'
        });
        private _Events = {
            criteriaChanged: 'criteriaChanged'
        };
        private _Settings: ReportCriteria_Settings;

        private _Filters: ReportFilter[] = [];          // The list of report filters that represent each of the criteria to apply to the report.
        private _FindAllPermutationsOfCallsign: boolean = VRS.globalOptions.reportFindAllPermutationsOfCallsign;

        constructor(settings: ReportCriteria_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(!settings.name) throw 'You must supply a name';

            this._Settings = settings;
        }

        /**
         * Returns the name of the criteria.
         */
        getName() : string
        {
            return this._Settings.name;
        }

        /**
         * Gets a value indicating that the server should search for all permutations of a callsign supplied as criteria.
         */
        getFindAllPermutationsOfCallsign() : boolean
        {
            return this._FindAllPermutationsOfCallsign;
        }
        setFindAllPermutationsOfCallsign(value: boolean)
        {
            this._FindAllPermutationsOfCallsign = value;
        }

        /**
         * Gets a value indicating that some criteria has been entered for the report.
         */
        hasCriteria() : boolean
        {
            return !!this._Filters.length;
        }

        /**
         * Returns true if the criteria constrains the report to displaying flights for a single aircraft.
         */
        isForSingleAircraft() : boolean
        {
            var result = false;
            var length = this._Filters.length;
            for(var i = 0;!result && i < length;++i) {
                var filter = this._Filters[i];
                switch(filter.getProperty()) {
                    case VRS.ReportFilterProperty.Registration:
                    case VRS.ReportFilterProperty.Icao:
                        // Neither of these are 100% guaranteed to identify a single aircraft - an aircraft can be
                        // reregistered so more than one aircraft can have the same registration or ICAO over their
                        // lifetimes. But for our purposes it'll be fine.
                        result = filter.getValueCondition().getCondition() === VRS.FilterCondition.Equals;
                        if(result) {
                            result = !!(<OneValueCondition>filter.getValueCondition()).getValue();
                        }
                        break;
                }
            }

            return result;
        }

        /**
         * Raised when new criteria are added or removed. Is **NOT** raised when the criteria values are changed. So basically,
         * gets raised when they click 'Add Criteria' or 'Remove Criteria', not when they change the value for an altitude
         * criteria (or any other kind).
         */
        hookCriteriaChanged(callback: () => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.criteriaChanged, callback, forceThis);
        }

        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Saves the current state of the object.
         */
        saveState()
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         */
        loadState() : ReportCriteria_SaveState
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        }

        /**
         * Applies a previously saved state to the object.
         */
        applyState(settings: ReportCriteria_SaveState)
        {
            this.setFindAllPermutationsOfCallsign(settings.findAllPermutationsOfCallsign);
        }

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the state will be saved.
         */
        private persistenceKey() : string
        {
            return 'vrsReportCriteria-' + this.getName();
        }

        /**
         * Creates the saved state object.
         */
        private createSettings() : ReportCriteria_SaveState
        {
            return {
                findAllPermutationsOfCallsign:  this.getFindAllPermutationsOfCallsign()
            };
        }

        /**
         * Creates the option pane that the user can employ to build up the criteria for the report.
         */
        createOptionPane(displayOrder: number) : OptionPane
        {
            var pane = new VRS.OptionPane({
                name:           'reportFilterOptionPane',
                titleKey:       'Criteria',
                displayOrder:   displayOrder
            });

            pane.addField(new VRS.OptionFieldCheckBox({
                name:           'findAllPermutations',
                labelKey:       'FindAllPermutationsOfCallsign',
                getValue:       () => this._FindAllPermutationsOfCallsign,
                setValue:       (value) => this._FindAllPermutationsOfCallsign = value,
                saveState:      () => this.saveState()
            }));

            var panesTypeSettings = VRS.reportFilterHelper.addConfigureFiltersListToPane({
                pane: pane,
                filters: this._Filters,
                saveState: $.noop,
                maxFilters: VRS.globalOptions.reportMaximumCriteria,
                allowableProperties: VRS.enumHelper.getEnumValues(VRS.ReportFilterProperty),
                addFilter: (newFilter, paneField) => {
                    var filter = this.addFilter(newFilter);
                    if(filter) {
                        paneField.addPane(filter.createOptionPane($.noop));
                    }
                },
                addFilterButtonLabel: 'AddCriteria',
                onlyUniqueFilters: true,
                isAlreadyInUse: (reportFilterProperty) => {
                    return VRS.reportFilterHelper.isFilterInUse(this._Filters, reportFilterProperty);
                }
            });
            panesTypeSettings.hookPaneRemoved(this.filterPaneRemoved, this);

            return pane;
        }

        /**
         * Adds a new filter to the object.
         * @param {VRS.ReportFilter|VRS.ReportFilterProperty} filterOrPropertyId Either a filter or a VRS.ReportFilterProperty property name.
         * @returns {VRS.ReportFilter} Either the filter passed in or the filter built from the property name.
         */
        addFilter(filterOrPropertyId: ReportFilter | ReportFilterPropertyEnum) : ReportFilter
        {
            var filter = <ReportFilter>filterOrPropertyId;
            if(!(filter instanceof VRS.ReportFilter)) {
                filter = VRS.reportFilterHelper.createFilter(<string>filterOrPropertyId);
            }

            if(VRS.reportFilterHelper.isFilterInUse(this._Filters, filter.getProperty())) {
                filter = null;
            } else {
                this._Filters.push(filter);
                this._Dispatcher.raise(this._Events.criteriaChanged);
            }

            return filter;
        }

        /**
         * Removes the filter at the index passed across.
         */
        private filterPaneRemoved(pane: OptionPane, index: number)
        {
            this.removeFilterAt(index);
        }

        /**
         * Removes the filter at the index passed across.
         */
        removeFilterAt(index: number)
        {
            this._Filters.splice(index, 1);
            this._Dispatcher.raise(this._Events.criteriaChanged);
        }

        /**
         * Constructs the list of filters held by the object from parameters passed across on the query string.
         */
        populateFromQueryString()
        {
            this._Filters = [];

            var pageUrl = $.url();
            $.each(VRS.reportFilterPropertyHandlers, (propertyIdx, propertyHandler) => {
                var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
                if(typeHandler && propertyHandler.serverFilterName) {
                    $.each(typeHandler.getConditions(), (typeIdx, condition) => {
                        this.extractFilterFromQueryString(pageUrl, propertyHandler, typeHandler, condition.condition, condition.reverseCondition);
                    });
                }
            });

            var allCallsignPermutations = pageUrl.param('callPerms');
            if(allCallsignPermutations) {
                this.setFindAllPermutationsOfCallsign(allCallsignPermutations !== '0');
            }
        }

        /**
         * Constructs a query string object for the list of filters held by the object. The query string can be used to
         * create a link to a report, it is the encoding of filters that can be decoded by populateFromQueryString.
         */
        createQueryString(useRelativeDates: boolean) : Object
        {
            var result = {};

            var now = new Date();
            var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
            var getRelativeDate = function(dateValue) {
                return !dateValue ? undefined : Math.floor((dateValue.getTime() - today.getTime()) / 86400000);
            };

            if(this._Filters.length) {
                $.each(this._Filters, (idx, filter) => {
                    var valueCondition = filter.getValueCondition();
                    var oneValueCondition = <OneValueCondition>valueCondition;
                    var twoValueCondition = <TwoValueCondition>valueCondition;

                    switch(valueCondition.getCondition()) {
                        case VRS.FilterCondition.Contains:  this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'C'); break;
                        case VRS.FilterCondition.Ends:      this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'E'); break;
                        case VRS.FilterCondition.Equals:    this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'Q'); break;
                        case VRS.FilterCondition.Starts:    this.doAddQueryStringFromFilter(result, filter, oneValueCondition.getValue(), 'S'); break;
                        case VRS.FilterCondition.Between:
                            var low = twoValueCondition.getValue1();
                            var high = twoValueCondition.getValue2();
                            var valueIsString = filter.getProperty() === VRS.ReportFilterProperty.Date && useRelativeDates;
                            if(valueIsString) {
                                low = getRelativeDate(low);
                                high = getRelativeDate(high);
                            }
                            if(low !== undefined)  this.doAddQueryStringFromFilter(result, filter, low, 'L', valueIsString);
                            if(high !== undefined) this.doAddQueryStringFromFilter(result, filter, high, 'U', valueIsString);
                            break;
                        default:
                            throw 'Not implemented ' + valueCondition.getCondition();
                    }
                });
            }

            return result;
        }


        /**
         * Searches for a query string for the property and condition passed across and, if one is present, parses the
         * criteria into a filter and returns it. If there is no query string parameter for the property and condition
         * then null is returned.
         */
        private extractFilterFromQueryString(pageUrl: purl.Url, propertyHandler: ReportFilterPropertyHandler, typeHandler: FilterPropertyTypeHandler, condition: FilterConditionEnum, reverseCondition: boolean)
        {
            switch(condition) {
                case VRS.FilterCondition.Contains:  this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'C'); break;
                case VRS.FilterCondition.Ends:      this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'E'); break;
                case VRS.FilterCondition.Equals:    this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'Q'); break;
                case VRS.FilterCondition.Starts:    this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'S'); break;
                case VRS.FilterCondition.Between:
                    var filter = this.doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'L');
                    this.doExtractFilterFromQueryString(filter, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'U');
                    break;
                default:
                    throw 'Not implemented ' + condition;
            }
        }

        /**
         * Builds the name of the query string parameter, fetches its value and parses it into a filter.
         */
        private doExtractFilterFromQueryString(
            filter: ReportFilter,
            pageUrl: purl.Url,
            propertyHandler: ReportFilterPropertyHandler,
            typeHandler: FilterPropertyTypeHandler,
            condition: FilterConditionEnum,
            reverseCondition: boolean,
            nameSuffix: string
        ) : ReportFilter
        {
            var result = filter;

            var name = this.getFilterName(propertyHandler, nameSuffix, reverseCondition);
            var text = pageUrl.param(name);
            var value = typeHandler.parseString(text);
            if(value !== undefined ) {
                if(!result) {
                    result = VRS.reportFilterHelper.createFilter(propertyHandler.property);
                    this._Filters.push(result);
                    this._Dispatcher.raise(this._Events.criteriaChanged);
                }

                var valueCondition = result.getValueCondition();
                valueCondition.setCondition(condition);
                valueCondition.setReverseCondition(reverseCondition);
                switch(condition) {
                    case VRS.FilterCondition.Between:
                        switch(nameSuffix) {
                            case 'L':   (<TwoValueCondition>valueCondition).setValue1(value); break;
                            case 'U':   (<TwoValueCondition>valueCondition).setValue2(value); break;
                            default:    throw 'Not implemented ' + nameSuffix;
                        }
                        break;
                    default:
                        (<OneValueCondition>valueCondition).setValue(value);
                        break;
                }
            }

            return result;
        }

        /**
         * Adds the value for a filter to the queryStringParams object passed across.
         */
        private doAddQueryStringFromFilter(queryStringParams: Object, filter: ReportFilter, value: any, nameSuffix: string, valueIsString?: boolean)
        {
            var condition = filter.getValueCondition();
            var propertyHandler = filter.getPropertyHandler();
            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            var reverseCondition = condition.getReverseCondition();

            var name = this.getFilterName(propertyHandler, nameSuffix, reverseCondition);
            var stringValue = valueIsString ? value : typeHandler.toQueryString(value);

            queryStringParams[name] = stringValue;
        }

        /**
         * Returns the name of a query string field.
         * @param {VRS.ReportFilterPropertyHandler} propertyHandler
         * @param {string}                          nameSuffix
         * @param {boolean}                         reverseCondition
         * @returns {string}
         */
        private getFilterName(propertyHandler: ReportFilterPropertyHandler, nameSuffix: string, reverseCondition: boolean) : string
        {
            return propertyHandler.serverFilterName + nameSuffix + (reverseCondition ? 'N' : '');
        }

        /**
         * Extends the parameters object that is passed to the server when a page of the report is fetched.
         */
        addToQueryParameters(params: any)
        {
            VRS.reportFilterHelper.addToQueryParameters(this._Filters, params, this._Settings.unitDisplayPreferences);
            if(this.getFindAllPermutationsOfCallsign()) {
                params.altCall = 1;
            }
        }
    }
} 