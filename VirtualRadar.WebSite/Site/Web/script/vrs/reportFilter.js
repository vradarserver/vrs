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
 * @fileoverview Abstracts away details of a report's criteria.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportMaximumCriteria = VRS.globalOptions.reportMaximumCriteria !== undefined ? VRS.globalOptions.reportMaximumCriteria : 15;                             // The maximum number of criteria that can be passed to a report.
    VRS.globalOptions.reportFindAllPermutationsOfCallsign = VRS.globalOptions.reportFindAllPermutationsOfCallsign !== undefined ? VRS.globalOptions.reportFindAllPermutationsOfCallsign : false;  // True if all permutations of a callsign should be found.
        //endregion

    //region ReportFilterPropertyHandler
    /**
     * Describes a report criteria property. Report criteria are based on filter properties, hence the name.
     * @param {Object}                      settings
     * @param {VRS.ReportFilterProperty}    settings.property   The criteria property that is being described by the handler.
     * @constructor
     * @augments VRS.FilterPropertyHandler
     */
    VRS.ReportFilterPropertyHandler = function(settings)
    {
        VRS.FilterPropertyHandler.call(this, $.extend({
            propertyEnumObject: VRS.ReportFilterProperty
        }, settings));
    };
    VRS.ReportFilterPropertyHandler.prototype = VRS.objectHelper.subclassOf(VRS.FilterPropertyHandler);
    //endregion

    //region VRS.reportFilterPropertyHandlers - pre-built report filter property handlers
    VRS.reportFilterPropertyHandlers = VRS.reportFilterPropertyHandlers || {};

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
    //endregion

    //region ReportFilter
    /**
     * Brings together a report filter property and a value/condition object to describe a criteria in the report.
     * @param {VRS.ReportFilterProperty}    reportFilterProperty
     * @param {VRS_ANY_VALUECONDITION}      valueCondition
     * @constructor
     * @augments VRS.Filter
     */
    VRS.ReportFilter = function(reportFilterProperty, valueCondition)
    {
        VRS.Filter.call(this, {
            property:               reportFilterProperty,
            valueCondition:         valueCondition,
            propertyEnumObject:     VRS.ReportFilterProperty,
            filterPropertyHandlers: VRS.reportFilterPropertyHandlers,
            cloneCallback:          function(/** VRS.ReportFilterProperty */ property, /** VRS_ANY_VALUECONDITION */ valueCondition) {
                return new VRS.ReportFilter(property, valueCondition);
            }
        });
        var that = this;
    };
    VRS.ReportFilter.prototype = VRS.objectHelper.subclassOf(VRS.Filter);
    //endregion

    //region ReportFilterHelper
    /**
     * A helper class that can ease the tedium of creating report filter objects, serialising them etc.
     * @constructor
     */
    VRS.ReportFilterHelper = function()
    {
        VRS.FilterHelper.call(this, {
            propertyEnumObject:     VRS.ReportFilterProperty,
            filterPropertyHandlers: VRS.reportFilterPropertyHandlers,
            createFilterCallback:   function(/** VRS.ReportFilterPropertyHandler */ propertyHandler, /** VRS_ANY_VALUECONDITION */ valueCondition) {
                return new VRS.ReportFilter(propertyHandler.property, valueCondition);
            }
        });
    };
    VRS.ReportFilterHelper.prototype = VRS.objectHelper.subclassOf(VRS.FilterHelper);

    VRS.reportFilterHelper = new VRS.ReportFilterHelper();
    //endregion

    //region ReportCriteria
    /**
     * Collects together a bunch of report filters which together describe all of the criteria for a report.
     * @param {Object}                      settings
     * @param {string}                      settings.name                       A unique name that distinguishes this object from other instances of the same class.
     * @param {VRS.UnitDisplayPreferences}  settings.unitDisplayPreferences     The unit display preferences to use when applying criteria.
     * @constructor
     */
    VRS.ReportCriteria = function(settings)
    {
        if(!settings) throw 'You must supply a settings object';
        if(!settings.name) throw 'You must supply a name';

        //region -- Fields
        var that = this;

        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.ReportCriteria'
        });
        var _Events = {
            criteriaChanged: 'criteriaChanged'
        };

        /**
         * The list of report filters that represent each of the criteria to apply to the report.
         * @type {Array.<VRS.ReportFilter>}
         * @private
         */
        var _Filters = [];
        //endregion

        //region -- Properties
        /**
         * Returns the name of the criteria.
         * @returns {string}
         */
        this.getName = function() { return settings.name; };

        /** @type {boolean} @private */
        var _FindAllPermutationsOfCallsign = VRS.globalOptions.reportFindAllPermutationsOfCallsign;
        /**
         * Gets a value indicating that the server should search for all permutations of a callsign supplied as criteria.
         * @returns {boolean}
         */
        this.getFindAllPermutationsOfCallsign = function() { return _FindAllPermutationsOfCallsign; };
        this.setFindAllPermutationsOfCallsign = function(/** boolean */ value) {
            _FindAllPermutationsOfCallsign = value;
        };

        /**
         * Gets a value indicating that some criteria has been entered for the report.
         * @returns {boolean}
         */
        this.hasCriteria = function() { return !!_Filters.length; };

        /**
         * Returns true if the criteria constrains the report to displaying flights for a single aircraft.
         */
        this.isForSingleAircraft = function()
        {
            var result = false;
            var length = _Filters.length;
            for(var i = 0;!result && i < length;++i) {
                var filter = _Filters[i];
                switch(filter.getProperty()) {
                    case VRS.ReportFilterProperty.Registration:
                    case VRS.ReportFilterProperty.Icao:
                        // Neither of these are 100% guaranteed to identify a single aircraft - an aircraft can be
                        // reregistered so more than one aircraft can have the same registration or ICAO over their
                        // lifetimes. But for our purposes it'll be fine.
                        result = filter.getValueCondition().getCondition() === VRS.FilterCondition.Equals;
                        if(result) result = !!filter.getValueCondition().getValue();
                        break;
                }
            }

            return result;
        };
        //endregion

        //region -- Events exposed
        /**
         * Raised when new criteria are added or removed. Is **NOT** raised when the criteria values are changed. So basically,
         * gets raised when they click 'Add Criteria' or 'Remove Criteria', not when they change the value for an altitude
         * criteria (or any other kind).
         * @param {function()}  callback
         * @param {Object}      forceThis
         * @returns {Object}
         */
        this.hookCriteriaChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.criteriaChanged, callback, forceThis); };

        /**
         * Unhooks an event handler from the object.
         * @param hookResult
         */
        this.unhook = function(hookResult)
        {
            _Dispatcher.unhook(hookResult);
        };
        //endregion

        //region -- saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         * @returns {VRS_STATE_REPORTCRITERIA}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            return $.extend(createSettings(), savedSettings);
        };

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_REPORTCRITERIA} settings
         */
        this.applyState = function(settings)
        {
            that.setFindAllPermutationsOfCallsign(settings.findAllPermutationsOfCallsign);
        };

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key under which the state will be saved.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsReportCriteria-' + that.getName();
        }

        /**
         * Creates the saved state object.
         * @returns {VRS_STATE_REPORTCRITERIA}
         */
        function createSettings()
        {
            return {
                findAllPermutationsOfCallsign:  that.getFindAllPermutationsOfCallsign()
            };
        }
        //endregion

        //region -- createOptionPane
        /**
         * Creates the option pane that the user can employ to build up the criteria for the report.
         * @param {number} displayOrder The relative display order for the pane amongst other panes.
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder)
        {
            var pane = new VRS.OptionPane({
                name:           'reportFilterOptionPane',
                titleKey:       'Criteria',
                displayOrder:   displayOrder
            });

            pane.addField(new VRS.OptionFieldCheckBox({
                name:           'findAllPermutations',
                labelKey:       'FindAllPermutationsOfCallsign',
                getValue:       function() { return _FindAllPermutationsOfCallsign; },
                setValue:       function(value) { _FindAllPermutationsOfCallsign = value; },
                saveState:      function() { that.saveState(); }
            }));

            var panesTypeSettings = VRS.reportFilterHelper.addConfigureFiltersListToPane({
                pane: pane,
                filters: _Filters,
                saveState: $.noop,
                maxFilters: VRS.globalOptions.reportMaximumCriteria,
                allowableProperties: VRS.enumHelper.getEnumValues(VRS.ReportFilterProperty),
                addFilter: function(newFilter, paneField) {
                    var filter = that.addFilter(newFilter);
                    if(filter) paneField.addPane(filter.createOptionPane($.noop));
                },
                addFilterButtonLabel: 'AddCriteria',
                onlyUniqueFilters: true,
                isAlreadyInUse: function(/** VRS.ReportFilterProperty */ reportFilterProperty) {
                    return VRS.reportFilterHelper.isFilterInUse(_Filters, reportFilterProperty);
                }
            });
            panesTypeSettings.hookPaneRemoved(filterPaneRemoved, this);

            return pane;
        };
        //endregion

        //region -- addFilter, filterPaneRemoved, removeFilterAt
        /**
         * Adds a new filter to the object.
         * @param {VRS.ReportFilter|VRS.ReportFilterProperty} filterOrPropertyId Either a filter or a VRS.ReportFilterProperty property name.
         * @returns {VRS.ReportFilter} Either the filter passed in or the filter built from the property name.
         */
        this.addFilter = function(filterOrPropertyId)
        {
            var filter = filterOrPropertyId;
            if(!(filter instanceof VRS.ReportFilter)) filter = VRS.reportFilterHelper.createFilter(filterOrPropertyId);

            if(VRS.reportFilterHelper.isFilterInUse(_Filters, filter.getProperty())) filter = null;
            else {
                _Filters.push(filter);
                _Dispatcher.raise(_Events.criteriaChanged);
            }

            return filter;
        };

        //noinspection JSUnusedLocalSymbols
        /**
         * Removes the filter at the index passed across.
         * @param {VRS.OptionPane=} pane Unused.
         * @param {number} index
         */
        function filterPaneRemoved(pane, index)
        {
            that.removeFilterAt(index);
        }

        /**
         * Removes the filter at the index passed across.
         * @param {number} index
         */
        this.removeFilterAt = function(index)
        {
            _Filters.splice(index, 1);
            _Dispatcher.raise(_Events.criteriaChanged);
        };
        //endregion

        //region -- populateFromQueryString, createQueryString
        /**
         * Constructs the list of filters held by the object from parameters passed across on the query string.
         */
        this.populateFromQueryString = function()
        {
            _Filters = [];

            var pageUrl = $.url();
            $.each(VRS.reportFilterPropertyHandlers, function(/** Number */ propertyIdx, /** VRS.ReportFilterPropertyHandler */ propertyHandler) {
                var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
                if(typeHandler && propertyHandler.serverFilterName) {
                    $.each(typeHandler.getConditions(), function(/** Number */ typeIdx, /** VRS_CONDITION */ condition) {
                        extractFilterFromQueryString(pageUrl, propertyHandler, typeHandler, condition.condition, condition.reverseCondition);
                    });
                }
            });

            var allCallsignPermutations = pageUrl.param('callPerms');
            if(allCallsignPermutations) that.setFindAllPermutationsOfCallsign(allCallsignPermutations !== '0');
        };

        /**
         * Constructs a query string object for the list of filters held by the object. The query string can be used to
         * create a link to a report, it is the encoding of filters that can be decoded by populateFromQueryString.
         * @param {boolean} useRelativeDates        True if dates should be relative to today, false if they should be fixed dates.
         * @returns {Object}
         */
        this.createQueryString = function(useRelativeDates)
        {
            var result = {};

            var now = new Date();
            var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
            var getRelativeDate = function(dateValue) {
                return !dateValue ? undefined : Math.floor((dateValue - today) / 86400000);
            };

            if(_Filters.length) {
                $.each(_Filters, function(/** number */ idx, /** VRS.ReportFilter */ filter) {
                    var valueCondition = filter.getValueCondition();
                    switch(valueCondition.getCondition()) {
                        case VRS.FilterCondition.Contains:  doAddQueryStringFromFilter(result, filter, valueCondition.getValue(), 'C'); break;
                        case VRS.FilterCondition.Ends:      doAddQueryStringFromFilter(result, filter, valueCondition.getValue(), 'E'); break;
                        case VRS.FilterCondition.Equals:    doAddQueryStringFromFilter(result, filter, valueCondition.getValue(), 'Q'); break;
                        case VRS.FilterCondition.Starts:    doAddQueryStringFromFilter(result, filter, valueCondition.getValue(), 'S'); break;
                        case VRS.FilterCondition.Between:
                            var low = valueCondition.getValue1();
                            var high = valueCondition.getValue2();
                            var valueIsString = filter.getProperty() === VRS.ReportFilterProperty.Date && useRelativeDates;
                            if(valueIsString) {
                                low = getRelativeDate(low);
                                high = getRelativeDate(high);
                            }
                            if(low !== undefined)  doAddQueryStringFromFilter(result, filter, low, 'L', valueIsString);
                            if(high !== undefined) doAddQueryStringFromFilter(result, filter, high, 'U', valueIsString);
                            break;
                        default:
                            throw 'Not implemented ' + valueCondition.getCondition();
                    }
                });
            }

            return result;
        };


        /**
         * Searches for a query string for the property and condition passed across and, if one is present, parses the
         * criteria into a filter and returns it. If there is no query string parameter for the property and condition
         * then null is returned.
         * @param {purl}                            pageUrl
         * @param {VRS.ReportFilterPropertyHandler} propertyHandler
         * @param {VRS.FilterPropertyTypeHandler}   typeHandler
         * @param {VRS.FilterCondition}             condition
         * @param {boolean}                         reverseCondition
         */
        function extractFilterFromQueryString(pageUrl, propertyHandler, typeHandler, condition, reverseCondition)
        {
            switch(condition) {
                case VRS.FilterCondition.Contains:  doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'C'); break;
                case VRS.FilterCondition.Ends:      doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'E'); break;
                case VRS.FilterCondition.Equals:    doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'Q'); break;
                case VRS.FilterCondition.Starts:    doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'S'); break;
                case VRS.FilterCondition.Between:
                    var filter = doExtractFilterFromQueryString(null, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'L');
                    doExtractFilterFromQueryString(filter, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, 'U');
                    break;
                default:
                    throw 'Not implemented ' + condition;
            }
        }

        /**
         * Builds the name of the query string parameter, fetches its value and parses it into a filter.
         * @param {VRS.ReportFilter}                filter
         * @param {purl}                            pageUrl
         * @param {VRS.ReportFilterPropertyHandler} propertyHandler
         * @param {VRS.FilterPropertyTypeHandler}   typeHandler
         * @param {VRS.FilterCondition}             condition
         * @param {boolean}                         reverseCondition
         * @param {string}                          nameSuffix
         * @returns {VRS.ReportFilter}
         */
        function doExtractFilterFromQueryString(filter, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, nameSuffix)
        {
            var result = filter;

            var name = getFilterName(propertyHandler, nameSuffix, reverseCondition);
            var text = pageUrl.param(name);
            var value = typeHandler.parseString(text);
            if(value !== undefined ) {
                if(!result) {
                    result = VRS.reportFilterHelper.createFilter(propertyHandler.property);
                    _Filters.push(result);
                    _Dispatcher.raise(_Events.criteriaChanged);
                }
                var valueCondition = result.getValueCondition();
                valueCondition.setCondition(condition);
                valueCondition.setReverseCondition(reverseCondition);
                switch(condition) {
                    case VRS.FilterCondition.Between:
                        switch(nameSuffix) {
                            case 'L':   valueCondition.setValue1(value); break;
                            case 'U':   valueCondition.setValue2(value); break;
                            default:    throw 'Not implemented ' + nameSuffix;
                        }
                        break;
                    default:
                        valueCondition.setValue(value);
                        break;
                }
            }

            return result;
        }

        /**
         * Adds the value for a filter to the queryStringParams object passed across.
         * @param {Object}              queryStringParams       The object to add the parameter to.
         * @param {VRS.ReportFilter}    filter                  The filter to add.
         * @param {*}                   value                   The value to add.
         * @param {string}              nameSuffix              The condition suffix to use on the name.
         * @param {boolean}            [valueIsString]          True if the value should be used as-is, false (the default) if it should be passed through toQueryString.
         */
        function doAddQueryStringFromFilter(queryStringParams, filter, value, nameSuffix, valueIsString)
        {
            var condition = filter.getValueCondition();
            /** @type {VRS.ReportFilterPropertyHandler} */ var propertyHandler = filter.getPropertyHandler();
            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            var reverseCondition = condition.getReverseCondition();
            var name = getFilterName(propertyHandler, nameSuffix, reverseCondition);
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
        function getFilterName(propertyHandler, nameSuffix, reverseCondition)
        {
            return propertyHandler.serverFilterName + nameSuffix + (reverseCondition ? 'N' : '');
        }
        //endregion

        //region -- addToQueryParameters
        /**
         * Extends the parameters object that is passed to the server when a page of the report is fetched.
         * @param {Object} params   The parameters object. Every property on this is converted into a query string parameter in the request for a page.
         */
        this.addToQueryParameters = function(params)
        {
            VRS.reportFilterHelper.addToQueryParameters(_Filters, params, settings.unitDisplayPreferences);
            if(that.getFindAllPermutationsOfCallsign()) params.altCall = 1;
        };
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
