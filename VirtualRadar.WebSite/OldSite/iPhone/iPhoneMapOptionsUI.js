function iPhoneMapOptionsUI(events, options, optionsStorage, aircraftListOptions, iPhoneMapPages)
{
    var that = this;
    var mEvents = events;
    var mOptions = options;
    var mOptionsStorage = optionsStorage;
    var mAircraftListOptions = aircraftListOptions;
    var mIPhoneMapPages = iPhoneMapPages;
    var mForm = null;
    var mDisplayOptionsHandlers = [];

    this.initialise = function()
    {
        mForm = createElement('form', null, null);
        mForm.setAttribute('name', 'iPhoneOptions');

        mEvents.addListener(EventId.optionsChanged, refreshOptionValues);

        // Toolbar
        var toolbar = createElement('div', 'toolbar', mForm);
        createElement('h1', null, toolbar).innerHTML = 'Settings';
        var toolbarBackButton = createLink('#', 'button back', toolbar);
        toolbarBackButton.innerHTML = 'List';
        toolbarBackButton.onclick = function() { mIPhoneMapPages.select(PageId.aircraftList, PageAnimation.none); };
        var toolbarMapButton = createLink('#', 'button', toolbar);
        toolbarMapButton.innerHTML = 'Map';
        toolbarMapButton.onclick = function() { mIPhoneMapPages.select(PageId.map, PageAnimation.none); };

        // Miscellaneous options
        var misc = createGroup('Miscellaneous', mForm);
        createSubpageAndLink(misc, 'Refresh period', 'Refresh Period', 'optionsRefreshPeriod',
            PageId.options, PageId.optionsRefreshPeriod,
            function() { return mOptions.refreshSeconds; },
            function(value) { mOptions.refreshSeconds = value; },
            [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 40, 50, 60 ],
            function(value) { return value.toString() + ' second' + (value === 1 ? '' : 's'); }
        );

        // Filter options
        if(_MapMode !== MapMode.flightSim) {
            var filter = createGroup('Filters', mForm);
            createToggle(filter, 'Hide A/C with no posn.', 'optionsFilterAircraftWithNoPosition',
                function() { return mOptions.filterAircraftWithNoPosition; },
                function(value) { mOptions.filterAircraftWithNoPosition = value; }
            );
            createToggle(filter, 'Only list A/C on map', 'optionsFilterAircraftNotOnMap',
                function() { return mOptions.filterAircraftNotOnMap; },
                function(value) { mOptions.filterAircraftNotOnMap = value; }
            );
            createToggle(filter, 'Use filters', 'optionsUseFilters',
                function() { return mOptions.filteringEnabled; },
                function(value) { mOptions.filteringEnabled = value; }
            );
            createTextBoxSingle(filter, 'Callsign', 'optionsFilterCallsign', 15,
                function() { return mOptions.filterCallsign; },
                function(value) { mOptions.filterCallsign = value; }
            ).setAttribute('style', 'text-transform:uppercase;');
            createTextBoxSingle(filter, 'Operator', 'optionsFilterOperator', 15,
                function() { return mOptions.filterOperator; },
                function(value) { mOptions.filterOperator = value; }
            ).setAttribute('style', 'text-transform:uppercase;');
            createTextBoxSingle(filter, 'Registration', 'optionsFilterRegistration', 12,
                function() { return mOptions.filterRegistration; },
                function(value) { mOptions.filterRegistration = value; }
            ).setAttribute('style', 'text-transform:uppercase;');
            createTextBoxSingle(filter, 'Type', 'optionsFilterType', 12,
                function() { return mOptions.filterType; },
                function(value) { mOptions.filterType = value; }
            ).setAttribute('style', 'text-transform:uppercase;');
            createTextBoxSingle(filter, 'Country', 'optionsFilterCountry', 12,
                function() { return mOptions.filterCountry; },
                function(value) { mOptions.filterCountry = value; }
            ).setAttribute('style', 'text-transform:uppercase;');
            createTextBoxPair(filter, 'Altitude', ' to ', 'optionsFilterAltitudeLower', 6, 'optionsFilterAltitudeUpper', 6,
                function() { return mOptions.filterAltitudeLower; },
                function(value) { mOptions.filterAltitudeLower = value; },
                function() { return mOptions.filterAltitudeUpper; },
                function(value) { mOptions.filterAltitudeUpper = value; }
            );
            createTextBoxPair(filter, 'Distance', ' to ', 'optionsFilterDistanceLower', 6, 'optionsFilterDistanceUpper', 6,
                function() { return mOptions.filterDistanceLower; },
                function(value) { mOptions.filterDistanceLower = value; },
                function() { return mOptions.filterDistanceUpper; },
                function(value) { mOptions.filterDistanceUpper = value; }
            );
            createTextBoxPair(filter, 'Squawk', ' to ', 'optionsFilterSquawkLower', 6, 'optionsFilterSquawkUpper', 6,
                function() { return mOptions.filterSquawkLower; },
                function(value) { mOptions.filterSquawkLower = value; },
                function() { return mOptions.filterSquawkUpper; },
                function(value) { mOptions.filterSquawkUpper = value; }
            );
            createSubpageAndLink(filter, 'Wake turbulence cat.', 'WTC', 'optionsFilterWtc',
                PageId.options, PageId.optionsFilterWtc,
                function() { return mOptions.filterWtc; },
                function(value) { mOptions.filterWtc = value; },
                [ '', '0', '1', '2', '3' ],
                function(value) {
                    switch(value) {
                        case '': return '(any)';
                        case '0': return 'None';
                        case '1': return 'Light';
                        case '2': return 'Medium';
                        case '3': return 'Heavy';
                    }
                    return 'Unknown';
                }
            );
            createSubpageAndLink(filter, 'Species', 'Species', 'optionsFilterSpecies',
                PageId.options, PageId.optionsFilterSpecies,
                function() { return mOptions.filterSpecies; },
                function(value) { mOptions.filterSpecies = value; },
                [ '', '0', '1', '2', '3', '4', '5', '6' ],
                function(value) {
                    switch(value) {
                        case '': return '(any)';
                        case '0': return 'None';
                        case '1': return 'Landplane';
                        case '2': return 'Seaplane';
                        case '3': return 'Amphibian';
                        case '4': return 'Helicopter';
                        case '5': return 'Gyrocopter';
                        case '6': return 'Tilt-wing';
                    }
                    return 'Unknown';
                }
            );
            createSubpageAndLink(filter, 'Engine type', 'Engine Type', 'optionsFilterEngType',
                PageId.options, PageId.optionsFilterEngType,
                function() { return mOptions.filterEngType; },
                function(value) { mOptions.filterEngType = value; },
                [ '', '0', '1', '2', '3', '4' ],
                function(value) {
                    switch(value) {
                        case '': return '(any)';
                        case '0': return 'None';
                        case '1': return 'Piston';
                        case '2': return 'Turboprop / Turboshaft';
                        case '3': return 'Jet';
                        case '4': return 'Electric';
                    }
                    return 'Unknown';
                }
            );
            createToggle(filter, 'Only show military', 'optionsFilterIsMilitary',
                function() { return mOptions.filterIsMilitary; },
                function(value) { mOptions.filterIsMilitary = value; }
            );
            createToggle(filter, 'Only show interesting', 'optionsFilterIsInteresting',
                function() { return mOptions.filterIsInteresting; },
                function(value) { mOptions.filterIsInteresting = value; }
            );
        }

        // Units
        var units = createGroup('Units', mForm);
        createSubpageAndLink(units, 'Distances', 'Distance Unit', 'optionsDistanceUnit',
            PageId.options, PageId.optionsDistanceUnit,
            function() { return mOptions.distanceUnit; },
            function(value) { mOptions.distanceUnit = value; },
            [ DistanceUnit.Miles, DistanceUnit.Kilometres, DistanceUnit.NauticalMiles ],
            function(value) {
                switch(value) {
                    case DistanceUnit.Miles: return 'Miles (statute)';
                    case DistanceUnit.Kilometres: return 'Kilometres';
                    case DistanceUnit.NauticalMiles: return 'Miles (nautical)';
                }
                return 'Unknown';
            }
        );
        createSubpageAndLink(units, 'Heights', 'Height Unit', 'optionsHeightUnit',
            PageId.options, PageId.optionsHeightUnit,
            function() { return mOptions.heightUnit; },
            function(value) { mOptions.heightUnit = value; },
            [ HeightUnit.Feet, HeightUnit.Metres ],
            function(value) {
                switch(value) {
                    case HeightUnit.Feet: return 'Feet';
                    case HeightUnit.Metres: return 'Metres';
                }
                return 'Unknown';
            }
        );
        createSubpageAndLink(units, 'Speeds', 'Speed Unit', 'optionsSpeedUnit',
            PageId.options, PageId.optionsSpeedUnit,
            function() { return mOptions.speedUnit; },
            function(value) { mOptions.speedUnit = value; },
            [ SpeedUnit.Knots, SpeedUnit.MilesPerHour, SpeedUnit.KilometresPerHour ],
            function(value) {
                switch(value) {
                    case SpeedUnit.Knots: return 'Knots';
                    case SpeedUnit.MilesPerHour: return 'Miles per hour';
                    case SpeedUnit.KilometresPerHour: return 'Kilometres per hour';
                }
                return 'Unknown';
            }
        );
        createToggle(units, 'Vertical speed / sec', 'optionsVerticalSpeedPerSecond',
            function() { return mOptions.verticalSpeedPerSecond; },
            function(value) { mOptions.verticalSpeedPerSecond = value; }
        );
        createTextBoxSingle(units, 'FL transition alt.', 'optionsFlightLevelTransitionAltitude', 12,
            function() { return mOptions.flightLevelTransitionAltitude; },
            function(value) { mOptions.flightLevelTransitionAltitude = value; }
        );
        createSubpageAndLink(units, 'in', 'Height Unit', 'optionsFlightLevelTransitionAltitudeUnit',
            PageId.options, PageId.optionsFlightLevelTransitionAltitudeUnit,
            function() { return mOptions.flightLevelTransitionAltitudeUnit; },
            function(value) { mOptions.flightLevelTransitionAltitudeUnit = value; },
            [ HeightUnit.Feet, HeightUnit.Metres ],
            function(value) {
                switch(value) {
                    case HeightUnit.Feet: return 'Feet';
                    case HeightUnit.Metres: return 'Metres';
                }
                return 'Unknown';
            }
        );
        createSubpageAndLink(units, 'FL heights', 'FL Heights', 'optionsFlightLevelHeightUnit',
            PageId.options, PageId.optionsFlightLevelHeightUnit,
            function() { return mOptions.flightLevelHeightUnit; },
            function(value) { mOptions.flightLevelHeightUnit = value; },
            [ HeightUnit.Feet, HeightUnit.Metres ],
            function(value) {
                switch(value) {
                    case HeightUnit.Feet: return 'Feet';
                    case HeightUnit.Metres: return 'Metres';
                }
                return 'Unknown';
            }
        );

        // Aircraft display
        var aircraftDisplay = createGroup('Aircraft display', mForm);
        createToggle(aircraftDisplay, 'Simple when zoomed out', 'optionsSimplePinsWhenZoomedOut',
            function() { return mOptions.simplePinsWhenZoomedOut; },
            function(value) { mOptions.simplePinsWhenZoomedOut = value; }
        );
        createToggle(aircraftDisplay, 'Show altitude stalks', 'optionsShowAltitudeStalk',
            function() { return mOptions.showAltitudeStalk; },
            function(value) { mOptions.showAltitudeStalk = value; }
        );
        var pinTextValues = [ PinText.None, PinText.Registration, PinText.Callsign, PinText.Type, PinText.ICAO, PinText.Squawk, PinText.OperatorCode, PinText.Altitude, PinText.FlightLevel, PinText.Speed, PinText.Route ]
        var pinTextDescriptions = [ 'None', 'Registration', 'Callsign', 'Type', 'ICAO', 'Squawk', 'Operator Code', 'Altitude', 'Flight Level', 'Speed', 'Route' ];
        createSubpageAndLink(aircraftDisplay, 'Label line 1', 'Label Line', 'optionsPinText1',
            PageId.options, PageId.optionsPinText1,
            function() { return mOptions.pinTextLines[0]; },
            function(value) { mOptions.pinTextLines[0] = value; },
            pinTextValues,
            function(value) { return pinTextDescriptions[value]; }
        );
        createSubpageAndLink(aircraftDisplay, 'Label line 2', 'Label Line', 'optionsPinText2',
            PageId.options, PageId.optionsPinText2,
            function() { return mOptions.pinTextLines[1]; },
            function(value) { mOptions.pinTextLines[1] = value; },
            pinTextValues,
            function(value) { return pinTextDescriptions[value]; }
        );
        createSubpageAndLink(aircraftDisplay, 'Label line 3', 'Label Line', 'optionsPinText3',
            PageId.options, PageId.optionsPinText3,
            function() { return mOptions.pinTextLines[2]; },
            function(value) { mOptions.pinTextLines[2] = value; },
            pinTextValues,
            function(value) { return pinTextDescriptions[value]; }
        );

        // Aircraft trails
        var aircraftTrails = createGroup('Aircraft trails', mForm);
        createToggle(aircraftTrails, 'Show short trails', 'optionsShowShortTrails',
            function() { return mOptions.showShortTrail; },
            function(value) {
                if(mOptions.showShortTrail !== value) {
                    mOptions.tempRefreshTrails = true;
                    mOptions.showShortTrail = value;
                }
            }
        );
        createRadioButtonSet(aircraftTrails, 'optionsTraceType',
            function() { return mOptions.trace; },
            function(value) { mOptions.trace = value; },
            [ TraceType.None, TraceType.JustSelected, TraceType.All ],
            function(value) {
                switch(value) {
                    case TraceType.None: return 'Do not show for any aircraft';
                    case TraceType.JustSelected: return 'Show for selected aircraft';
                    case TraceType.All: return 'Show for all aircraft';
                }
                return 'Unknown';
            }
        );

        // Sort aircraft list
        var sortListValues = [ '', 'timeSeen', 'type', 'model', 'operator', 'operatorIcao', 'reg', 'icao', 'alt', 'call', 'spd', 'from', 'to', 'fcnt', 'sqk', 'vsi', 'dist' ];
        var sortListDescriptions = [ 'None', 'Time First Seen', 'Aircraft Type', 'Aircraft Model', 'Operator Name', 'Operator Code', 'Registration', 'ICAO', 'Altitude', 'Callsign', 'Speed', 'From', 'To', 'Seen Count', 'Squawk', 'Vertical Speed', 'Distance' ];
        var sortDirectionValues = [ 'asc', 'desc' ];
        var sortDirectionDescriptions = [ 'Ascending', 'Descending' ];
        var sortList = createGroup('Sort aircraft list', mForm);
        createSubpageAndLink(sortList, 'Sort by', 'Sort Column', 'optionsSortListColumn1',
            PageId.options, PageId.optionsSortListColumn1,
            function() { return mAircraftListOptions.getSortField1(); },
            function(value) { mAircraftListOptions.setSortField1(value); },
            sortListValues,
            function(value) {
                for(var i = 0;i < sortListValues.length;++i) {
                    if(sortListValues[i] == value) return sortListDescriptions[i];
                }
                return "Unknown";
            }
        );
        createSubpageAndLink(sortList, '&nbsp;', 'Sort Direction', 'optionsSortListDir1',
            PageId.options, PageId.optionsSortListDir1,
            function() { return mAircraftListOptions.getSortDirection1(); },
            function(value) { mAircraftListOptions.setSortDirection1(value); },
            sortDirectionValues,
            function(value) {
                for(var i = 0;i < sortDirectionValues.length;++i) {
                    if(sortDirectionValues[i] == value) return sortDirectionDescriptions[i];
                }
                return "Unknown";
            }
        );
        createSubpageAndLink(sortList, 'and then by', 'Sort Column', 'optionsSortListColumn2',
            PageId.options, PageId.optionsSortListColumn2,
            function() { return mAircraftListOptions.getSortField2(); },
            function(value) { mAircraftListOptions.setSortField2(value); },
            sortListValues,
            function(value) {
                for(var i = 0;i < sortListValues.length;++i) {
                    if(sortListValues[i] == value) return sortListDescriptions[i];
                }
                return "Unknown";
            }
        );
        createSubpageAndLink(sortList, '&nbsp;', 'Sort Direction', 'optionsSortListDir2',
            PageId.options, PageId.optionsSortListDir2,
            function() { return mAircraftListOptions.getSortDirection2(); },
            function(value) { mAircraftListOptions.setSortDirection2(value); },
            sortDirectionValues,
            function(value) {
                for(var i = 0;i < sortDirectionValues.length;++i) {
                    if(sortDirectionValues[i] == value) return sortDirectionDescriptions[i];
                }
                return "Unknown";
            }
        );

        // Auto-selection
        var autoSelect = createGroup('Auto-selection', mForm);
        createToggle(autoSelect, 'Enabled', 'optionsAutoSelectEnabled',
            function() { return mOptions.autoSelectEnabled; },
            function(value) { mOptions.autoSelectEnabled = value; }
        );
        createRadioButtonSet(autoSelect, 'optionsAutoSelectClosest',
            function() { return mOptions.autoSelect.useClosest; },
            function(value) { mOptions.autoSelect.useClosest = value; },
            [ true, false ],
            function(value) {
                return value ? 'Select closest to here' : 'Select furthest from here';
            }
        );

        // Finish off the page
        createElement('p', null, mForm).innerHTML = '&nbsp;';
        document.getElementById('options').appendChild(mForm);

        refreshOptionValues(null, null);
    };

    function createSubpageAndLink(linkParent, linkLabel, subPageTitle, idPrefix, parentPageId, subPageId, getOptionValueFunction, setFunction, values, getDescriptionFunction)
    {
        // Create the link on the main page showing the label and the current value, with a func to refresh the value
        var linkValueElementId = idPrefix + 'Value';
        createLinkToSubpage(linkParent, linkLabel, linkValueElementId, subPageId);
        mDisplayOptionsHandlers.push(function() { setLinkToSubpageValue(linkValueElementId, getDescriptionFunction(getOptionValueFunction())); });

        // Create the subpage itself - this is always attached to the top-level iPhones pages div
        buildComboBoxSubpage(document.getElementById('iPhonePages'), parentPageId, subPageTitle, subPageId, idPrefix + 'Choice', values, getDescriptionFunction, setFunction, function(value) { return getOptionValueFunction() === value; }, linkValueElementId);
    };

    function createTextBoxSingle(parentElement, label, idPrefix, length, getFunction, setFunction)
    {
        var line = createElement('li', null, parentElement);

        createElement('div', 'optionsLabel', line).innerHTML = label;

        var inputContainer = createElement('div', 'optionsInput', line);
        return createTextBoxInput(inputContainer, null, idPrefix, length, getFunction, setFunction);
    };

    function createTextBoxPair(parentElement, leftLabel, middleLabel, firstIdPrefix, firstLength, secondIdPrefix, secondLength, firstGetFunction, firstSetFunction, secondGetFunction, secondSetFunction)
    {
        var line = createElement('li', null, parentElement);

        createElement('div', 'optionsLabel', line).innerHTML = leftLabel;
        var inputContainer = createElement('div', 'optionsInput', line);

        createTextBoxInput(inputContainer, 'optionsLeftTextBox', firstIdPrefix, firstLength, firstGetFunction, firstSetFunction);
        createElement('div', 'optionsLabel', inputContainer).innerHTML = middleLabel;
        createTextBoxInput(inputContainer, 'optionsRightTextBox', secondIdPrefix, secondLength, secondGetFunction, secondSetFunction);
    };

    function createTextBoxInput(parentElement, extraClasses, idPrefix, length, getFunction, setFunction)
    {
        var inputId = idPrefix + 'Input';
        var classes = 'optionsTextBox' + (extraClasses ? ' ' + extraClasses : '');

        var input = createInput('textbox', idPrefix + 'Name', classes, parentElement);
        input.setAttribute('size', length);
        input.setAttribute('id', inputId);

        input.onchange = function() { setFunction(input.value); save(); };
        mDisplayOptionsHandlers.push(function() { input.value = getFunction(); });

        return input;
    };

    function createToggle(toggleParent, toggleLabel, idPrefix, getOptionValueFunction, setFunction)
    {
        var inputId = idPrefix + 'Input';
        var line = createElement('li', null, toggleParent);
        createElement('div', 'optionsLabel', line).innerHTML = toggleLabel;
        var toggleOuter = createElement('span', 'toggle', line);
        var input = createInput('checkbox', idPrefix + 'Name', null, toggleOuter);
        input.setAttribute('id', inputId);
        input.onclick = function() { setFunction(input.checked); save(); };

        mDisplayOptionsHandlers.push(function() { input.checked = getOptionValueFunction(); });
    };

    function createRadioButtonSet(parentElement, idPrefix, getOptionValueFunction, setFunction, values, getDescriptionFunction)
    {
        for(var i = 0;i < values.length;++i) {
            var value = values[i];
            var id = idPrefix + value;
            createRadioButtonElement(parentElement, value, getDescriptionFunction(value), id, setFunction);
        }

        mDisplayOptionsHandlers.push(function() {
            var optionValue = getOptionValueFunction();
            for(var i = 0;i < values.length;++i) {
                var value = values[i];
                var element = document.getElementById(idPrefix + value);
                setClass(element, optionValue ===  value ? 'tick' : 'null');
            }
        });
    };

    function createRadioButtonElement(parentElement, value, description, id, setFunction)
    {
        var element = createElement('li', null, parentElement);
        element.setAttribute('id', id);
        element.onclick = function() {
            setFunction(value);
            save();
        };
        createElement('div', 'optionsLabel', element).innerHTML = description;
    };

    function createLinkToSubpage(parent, description, settingsElementId, pageId)
    {
        var result = createElement('li', 'arrow', parent);
        createElement('div', 'optionsLabel', result).innerHTML = description;
        createElement('div', 'optionsSetting', result).setAttribute('id', settingsElementId);
        result.onclick = function() { mIPhoneMapPages.select(pageId, PageAnimation.none); };

        return result;
    };

    function buildComboBoxSubpage(subpageParent, parentPageId, title, pageId, elementPrefix, values, getDescriptionFunction, setFunction, compareValueFunction, linkValueElementId)
    {
        var screen = createSubpage(subpageParent, parentPageId, title, pageId, linkValueElementId);
        var group = createGroup(null, screen);

        for(var i = 0;i < values.length;++i) {
            var value = values[i];
            var description = getDescriptionFunction(value);
            var elementId = elementPrefix + value;
            createSetValueLinkLine(group, parentPageId, description, elementId, setFunction, value, linkValueElementId);
        }

        mDisplayOptionsHandlers.push(function() {
            for(var i = 0;i < values.length;++i) {
                var value = values[i];
                var elementId = elementPrefix + value;
                var element = document.getElementById(elementId);
                setClass(element, compareValueFunction(value) ? 'tick' : null);
            }
        });
    };

    function refreshOptionValues(sender, args)
    {
        var length = mDisplayOptionsHandlers.length;
        for(var i = 0;i < length;++i) {
            mDisplayOptionsHandlers[i]();
        }
    };

    this.subscreenCancelClicked = function()
    {
        mIPhoneMapPages.select(PageId.options, PageAnimation.none);
    };

    function createGroup(name, parent)
    {
        if(name !== null) createElement('h4', null, parent).innerHTML = name;
        return createElement('ul', 'rounded', parent);
    };

    function setLinkToSubpageValue(id, description)
    {
        document.getElementById(id).innerHTML = description;
    };

    function createSubpage(parent, parentPageId, heading, pageId, scrollToId)
    {
        var result = createElement('div', null, parent);
        result.setAttribute('id', mIPhoneMapPages.getPageHtmlId(pageId));

        var toolbar = createElement('div', 'toolbar', result);
        createElement('h1', null, toolbar).innerHTML = heading;
        var cancelButton = createLink('#', 'button cancel', toolbar);
        cancelButton.innerHTML = 'Cancel';
        cancelButton.onclick = function() {
            mIPhoneMapPages.select(parentPageId, PageAnimation.none);
            var scrollToElement = document.getElementById(scrollToId);
            scrollToElement.scrollIntoView();
            return false;
        };

        return result;
    };

    function createSetValueLinkLine(owner, ownerParentPageId, description, elementId, setValueFunction, value, scrollToId)
    {
        var result = createElement('li', null, owner);
        if(result._VirtualRadarServerOptionValue !== undefined) throw new "li elements already use the name _VirtualRadarServerOptionValue";
        result._VirtualRadarServerOptionValue = value;

        result.setAttribute('id', elementId);
        result.onclick = function() {
            setValueFunction(result._VirtualRadarServerOptionValue);
            save();
            mIPhoneMapPages.select(ownerParentPageId, PageAnimation.none);
            var scrollToElement = document.getElementById(scrollToId);
            scrollToElement.scrollIntoView();
        };
        createElement('div', 'optionsLabel', result).innerHTML = description;

        return result;
    };

    function save()
    {
        mOptions.recalculateTempValues();
        mOptionsStorage.save(mOptions);
        mAircraftListOptions.save();
        mEvents.raise(EventId.resetTimeOut, null, null);
        mEvents.raise(EventId.optionsChanged, null, null);
    };
}