var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.polarPlotEnabled = VRS.globalOptions.polarPlotEnabled !== undefined ? VRS.globalOptions.polarPlotEnabled : true;
    VRS.globalOptions.polarPlotFetchUrl = VRS.globalOptions.polarPlotFetchUrl || 'PolarPlot.json';
    VRS.globalOptions.polarPlotFetchTimeout = VRS.globalOptions.polarPlotFetchTimeout || 10000;
    VRS.globalOptions.polarPlotAutoRefreshSeconds = VRS.globalOptions.polarPlotAutoRefreshSeconds !== undefined ? VRS.globalOptions.polarPlotAutoRefreshSeconds : 5;
    VRS.globalOptions.polarPlotAltitudeConfigs = VRS.globalOptions.polarPlotAltitudeConfigs || [
        { low: -1, high: -1, colour: '#000000', zIndex: -5 },
        { low: -1, high: 9999, colour: '#FFFFFF', zIndex: -1 },
        { low: 10000, high: 19999, colour: '#00FF00', zIndex: -2 },
        { low: 20000, high: 29999, colour: '#0000FF', zIndex: -3 },
        { low: 30000, high: -1, colour: '#FF0000', zIndex: -4 }
    ];
    VRS.globalOptions.polarPlotUserConfigurable = VRS.globalOptions.polarPlotUserConfigurable !== undefined ? VRS.globalOptions.polarPlotUserConfigurable : true;
    VRS.globalOptions.polarPlotStrokeWeight = VRS.globalOptions.polarPlotStrokeWeight !== undefined ? VRS.globalOptions.polarPlotStrokeWeight : 2;
    VRS.globalOptions.polarPlotStrokeColour = VRS.globalOptions.polarPlotStrokeColour !== undefined ? VRS.globalOptions.polarPlotStrokeColour : '#000000';
    VRS.globalOptions.polarPlotStrokeOpacity = VRS.globalOptions.polarPlotStrokeOpacity !== undefined ? VRS.globalOptions.polarPlotStrokeOpacity : 1.0;
    VRS.globalOptions.polarPlotFillOpacity = VRS.globalOptions.polarPlotFillOpacity !== undefined ? VRS.globalOptions.polarPlotFillOpacity : 0.50;
    VRS.globalOptions.polarPlotStrokeColourCallback = VRS.globalOptions.polarPlotStrokeColourCallback || undefined;
    VRS.globalOptions.polarPlotFillColourCallback = VRS.globalOptions.polarPlotFillColourCallback || undefined;
    VRS.globalOptions.polarPlotDisplayOnStartup = VRS.globalOptions.polarPlotDisplayOnStartup || [];
    var PolarPlotter = (function () {
        function PolarPlotter(settings) {
            var _this = this;
            this._PlotsOnDisplay = [];
            this._PolarPlot = null;
            this._AltitudeRangeConfigs = VRS.globalOptions.polarPlotAltitudeConfigs.slice();
            this._StrokeOpacity = VRS.globalOptions.polarPlotStrokeOpacity;
            this._FillOpacity = VRS.globalOptions.polarPlotFillOpacity;
            this.getPlotsOnDisplayIndex = function (feedId, slice) {
                return VRS.arrayHelper.indexOfMatch(_this._PlotsOnDisplay, function (item) {
                    return item.feedId === feedId && item.slice.lowAlt === slice.lowAlt && item.slice.highAlt === slice.highAlt;
                });
            };
            this.addToPlotsOnDisplay = function (feedId, slice) {
                if (_this.getPlotsOnDisplayIndex(feedId, slice) === -1) {
                    _this._PlotsOnDisplay.push({ feedId: feedId, slice: slice });
                }
            };
            this.removeFromPlotsOnDisplay = function (feedId, slice) {
                var index = _this.getPlotsOnDisplayIndex(feedId, slice);
                if (index !== -1) {
                    _this._PlotsOnDisplay.splice(index, 1);
                }
            };
            this.getPlotsOnDisplayForFeed = function (feedId) {
                return VRS.arrayHelper.filter(_this._PlotsOnDisplay, function (plotOnDisplay) {
                    return plotOnDisplay.feedId === feedId;
                });
            };
            this.getName = function () {
                return _this._Settings.name;
            };
            this.getPolarPlot = function () {
                return _this._PolarPlot;
            };
            this.getPolarPlotterFeeds = function () {
                var result = [];
                if (!VRS.serverConfig || VRS.serverConfig.polarPlotsEnabled()) {
                    result = VRS.arrayHelper.filter(_this._Settings.aircraftListFetcher.getFeeds(), function (feed) {
                        return feed.polarPlot;
                    });
                }
                return result;
            };
            this.getSortedPolarPlotterFeeds = function () {
                var result = _this.getPolarPlotterFeeds();
                result.sort(function (lhs, rhs) {
                    if (lhs.id !== undefined && rhs.id !== undefined)
                        return lhs.name.localeCompare(rhs.name);
                    else if (lhs.id === undefined && rhs.id === undefined)
                        return 0;
                    else if (lhs.id === undefined)
                        return -1;
                    else
                        return 1;
                });
                return result;
            };
            this.getPlotsOnDisplay = function () {
                var result = [];
                var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;
                if (serverConfig) {
                    $.each(_this._PlotsOnDisplay, function (idx, plotOnDisplay) {
                        var receiver = VRS.arrayHelper.findFirst(serverConfig.Receivers, function (serverReceiver) {
                            return serverReceiver.UniqueId === plotOnDisplay.feedId;
                        });
                        if (receiver) {
                            var normalisedRange = _this.getNormalisedSliceRange(plotOnDisplay.slice, -1, -1);
                            result.push({
                                feedName: receiver.Name,
                                low: normalisedRange.lowAlt,
                                high: normalisedRange.highAlt
                            });
                        }
                    });
                }
                return result;
            };
            this.getAltitudeRangeConfigs = function () {
                return _this._AltitudeRangeConfigs.slice();
            };
            this.setAltitudeRangeConfigs = function (value) {
                if (value && _this._AltitudeRangeConfigs && value.length === _this._AltitudeRangeConfigs.length) {
                    var changed = false;
                    var length = value.length;
                    for (var i = 0; i < length; ++i) {
                        var current = _this._AltitudeRangeConfigs[i];
                        var revised = value[i];
                        if (current.low === revised.low && current.high === revised.high && current.colour !== revised.colour) {
                            current.colour = revised.colour;
                            changed = true;
                        }
                    }
                    if (changed) {
                        _this.refreshAllDisplayed();
                    }
                }
            };
            this.getStrokeOpacity = function () {
                return _this._StrokeOpacity;
            };
            this.setStrokeOpacity = function (value) {
                if (value && value !== _this._StrokeOpacity && value >= 0 && value <= 1) {
                    _this._StrokeOpacity = value;
                    _this.refreshAllDisplayed();
                }
            };
            this.getFillOpacity = function () {
                return _this._FillOpacity;
            };
            this.setFillOpacity = function (value) {
                if (value && value !== _this._FillOpacity && value >= 0 && value <= 1) {
                    _this._FillOpacity = value;
                    _this.refreshAllDisplayed();
                }
            };
            this.saveState = function () {
                var settings = _this.createSettings();
                if (VRS.globalOptions.polarPlotDisplayOnStartup && VRS.globalOptions.polarPlotDisplayOnStartup.length) {
                    if (settings.plotsOnDisplay.length === VRS.globalOptions.polarPlotDisplayOnStartup.length) {
                        if (VRS.arrayHelper.except(settings.plotsOnDisplay, VRS.globalOptions.polarPlotDisplayOnStartup, function (lhs, rhs) {
                            return lhs.feedName === rhs.feedName &&
                                lhs.high === rhs.high &&
                                lhs.low === rhs.low;
                        }).length === 0) {
                            settings.plotsOnDisplay = undefined;
                        }
                    }
                }
                VRS.configStorage.save(_this.persistenceKey(), settings);
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.load(_this.persistenceKey(), {});
                if ((!savedSettings || !savedSettings.plotsOnDisplay) && VRS.globalOptions.polarPlotDisplayOnStartup) {
                    savedSettings = $.extend({ plotsOnDisplay: VRS.globalOptions.polarPlotDisplayOnStartup }, savedSettings);
                }
                var result = $.extend(_this.createSettings(), savedSettings);
                var altitudeRangeConfigsBad = result.altitudeRangeConfigs.length !== _this._AltitudeRangeConfigs.length;
                if (!altitudeRangeConfigsBad) {
                    for (var i = 0; i < result.altitudeRangeConfigs.length; ++i) {
                        var current = _this._AltitudeRangeConfigs[i];
                        var saved = result.altitudeRangeConfigs[i];
                        altitudeRangeConfigsBad = current.low !== saved.low || current.high !== saved.high;
                        if (altitudeRangeConfigsBad)
                            break;
                    }
                }
                if (altitudeRangeConfigsBad)
                    result.altitudeRangeConfigs = _this.getAltitudeRangeConfigs();
                var usePlotsOnDisplay = [];
                var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;
                var receivers = serverConfig ? serverConfig.Receivers : null;
                if (receivers) {
                    $.each(result.plotsOnDisplay, function (idx, abstractReceiver) {
                        var plotReceiver = VRS.arrayHelper.findFirst(receivers, function (serverReceiver) {
                            return VRS.stringUtility.equals(serverReceiver.Name, abstractReceiver.feedName, true);
                        });
                        if (plotReceiver) {
                            usePlotsOnDisplay.push(abstractReceiver);
                        }
                    });
                }
                result.plotsOnDisplay = usePlotsOnDisplay;
                return result;
            };
            this.applyState = function (settings) {
                var polarPlotIdentifiers = [];
                var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;
                _this.setAltitudeRangeConfigs(settings.altitudeRangeConfigs);
                _this.setStrokeOpacity(settings.strokeOpacity);
                _this.setFillOpacity(settings.fillOpacity);
                if (serverConfig && serverConfig.Receivers) {
                    $.each(settings.plotsOnDisplay, function (idx, abstractReceiver) {
                        var colourMaxAltitude = VRS.arrayHelper.findFirst(VRS.globalOptions.polarPlotAltitudeConfigs, function (obj) {
                            return obj.low === abstractReceiver.low && obj.high === abstractReceiver.high;
                        });
                        var receiver = VRS.arrayHelper.findFirst(serverConfig.Receivers, function (feed) {
                            return VRS.stringUtility.equals(feed.Name, abstractReceiver.feedName, true);
                        });
                        if (colourMaxAltitude && receiver)
                            polarPlotIdentifiers.push({
                                feedId: receiver.UniqueId,
                                lowAlt: colourMaxAltitude.low,
                                highAlt: colourMaxAltitude.high,
                                colour: colourMaxAltitude.colour
                            });
                    });
                    _this.fetchAndDisplayByIdentifiers(polarPlotIdentifiers);
                }
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.createOptionPane = function (displayOrder) {
                var result = new VRS.OptionPane({
                    name: 'polarPlotColours',
                    titleKey: 'PaneReceiverRange',
                    displayOrder: displayOrder
                });
                if ((VRS.serverConfig && VRS.serverConfig.polarPlotsEnabled()) && VRS.globalOptions.polarPlotEnabled && VRS.globalOptions.polarPlotUserConfigurable) {
                    var configs = _this.getAltitudeRangeConfigs();
                    $.each(configs, function (idx, polarPlotConfig) {
                        var colourField = new VRS.OptionFieldColour({
                            name: 'polarPlotColour' + idx,
                            labelKey: function () { return _this.getSliceRangeDescription(polarPlotConfig.low, polarPlotConfig.high); },
                            getValue: function () { return polarPlotConfig.colour; },
                            setValue: function (value) { return polarPlotConfig.colour = value; },
                            saveState: function () {
                                _this.saveState();
                                _this.refreshAllDisplayed();
                            }
                        });
                        result.addField(colourField);
                    });
                    var commonOpacityOptions = {
                        showSlider: true,
                        min: 0.0,
                        max: 1.0,
                        step: 0.01,
                        decimals: 2,
                        inputWidth: VRS.InputWidth.ThreeChar
                    };
                    result.addField(new VRS.OptionFieldNumeric($.extend({
                        name: 'polarPlotterFillOpacity',
                        labelKey: 'FillOpacity',
                        getValue: _this.getFillOpacity,
                        setValue: _this.setFillOpacity,
                        saveState: _this.saveState
                    }, commonOpacityOptions)));
                    result.addField(new VRS.OptionFieldNumeric($.extend({
                        name: 'polarPlotterStrokeOpacity',
                        labelKey: 'StrokeOpacity',
                        getValue: _this.getStrokeOpacity,
                        setValue: _this.setStrokeOpacity,
                        saveState: _this.saveState
                    }, commonOpacityOptions)));
                }
                return result;
            };
            this.fetchPolarPlot = function (feedId, callback) {
                _this._PolarPlot = null;
                $.ajax({
                    url: VRS.globalOptions.polarPlotFetchUrl,
                    dataType: 'json',
                    data: { feedId: feedId },
                    success: function (data) {
                        _this._PolarPlot = data;
                        if (callback)
                            callback();
                    },
                    timeout: VRS.globalOptions.polarPlotFetchTimeout
                });
            };
            this.findSliceForAltitudeRange = function (plots, lowAltitude, highAltitude) {
                var result = null;
                if (plots) {
                    var length = plots.slices.length;
                    for (var i = 0; i < length; ++i) {
                        var slice = plots.slices[i];
                        if (_this.isSliceForAltitudeRange(slice, lowAltitude, highAltitude)) {
                            result = slice;
                            break;
                        }
                    }
                }
                return result;
            };
            this.isSliceForAltitudeRange = function (slice, lowAltitude, highAltitude) {
                return !!slice &&
                    ((lowAltitude === -1 && slice.lowAlt < -20000000) || (lowAltitude !== -1 && slice.lowAlt === lowAltitude)) &&
                    ((highAltitude === -1 && slice.highAlt > 20000000) || (highAltitude !== -1 && slice.highAlt === highAltitude));
            };
            this.getNormalisedSliceRange = function (slice, lowOpenEnd, highOpenEnd) {
                return !slice ? null : {
                    lowAlt: slice.lowAlt < -20000000 ? lowOpenEnd : slice.lowAlt,
                    highAlt: slice.highAlt > 20000000 ? highOpenEnd : slice.highAlt
                };
            };
            this.getNormalisedRange = function (lowAltitude, highAltitude, lowOpenEnd, highOpenEnd) {
                if (lowAltitude === undefined || lowAltitude < -20000000)
                    lowAltitude = lowOpenEnd;
                if (highAltitude === undefined || highAltitude > 20000000)
                    highAltitude = highOpenEnd;
                return {
                    lowAlt: lowAltitude,
                    highAlt: highAltitude
                };
            };
            this.getSliceRangeDescription = function (lowAltitude, highAltitude) {
                var range = _this.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
                lowAltitude = range.lowAlt;
                highAltitude = range.highAlt;
                var lowAlt = VRS.format.altitude(lowAltitude, VRS.AltitudeType.Barometric, false, _this._Settings.unitDisplayPreferences.getHeightUnit(), false, true, false);
                var highAlt = VRS.format.altitude(highAltitude, VRS.AltitudeType.Barometric, false, _this._Settings.unitDisplayPreferences.getHeightUnit(), false, true, false);
                return lowAltitude === -1 && highAltitude === -1 ? VRS.$$.AllAltitudes
                    : lowAltitude === -1 ? VRS.stringUtility.format(VRS.$$.ToAltitude, highAlt)
                        : highAltitude === -1 ? VRS.stringUtility.format(VRS.$$.FromAltitude, lowAlt)
                            : VRS.stringUtility.format(VRS.$$.FromToAltitude, lowAlt, highAlt);
            };
            this.isAllAltitudes = function (lowAltitude, highAltitude) {
                var range = _this.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
                lowAltitude = range.lowAlt;
                highAltitude = range.highAlt;
                return lowAltitude === -1 && highAltitude === -1;
            };
            this.getAltitudeRangeConfigRecord = function (lowAltitude, highAltitude) {
                var altRange = _this.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
                lowAltitude = altRange.lowAlt;
                highAltitude = altRange.highAlt;
                var result = null;
                var length = _this._AltitudeRangeConfigs.length;
                for (var i = 0; i < length; ++i) {
                    var range = _this._AltitudeRangeConfigs[i];
                    if (range.low === lowAltitude && range.high === highAltitude) {
                        result = range;
                        break;
                    }
                }
                return result;
            };
            this.getSliceRangeColour = function (lowAltitude, highAltitude) {
                var altitudeRangeConfig = _this.getAltitudeRangeConfigRecord(lowAltitude, highAltitude);
                return altitudeRangeConfig ? altitudeRangeConfig.colour : null;
            };
            this.getSliceRangeZIndex = function (lowAltitude, highAltitude) {
                var record = _this.getAltitudeRangeConfigRecord(lowAltitude, highAltitude);
                return record ? record.zIndex : -1;
            };
            this.displayPolarPlotSlice = function (feedId, slice, colour) {
                if (slice) {
                    var polygonId = _this.getPolygonId(feedId, slice);
                    var existingPolygon = _this._Settings.map.getPolygon(polygonId);
                    if (!slice.plots.length) {
                        if (existingPolygon) {
                            _this._Settings.map.destroyPolygon(existingPolygon);
                        }
                    }
                    else {
                        if (colour === undefined) {
                            colour = _this.getSliceRangeColour(slice.lowAlt, slice.highAlt);
                        }
                        var fillColour = VRS.globalOptions.polarPlotFillColourCallback ? VRS.globalOptions.polarPlotFillColourCallback(feedId, slice.lowAlt, slice.highAlt) : colour || _this.getPolygonColour(slice);
                        var strokeColour = VRS.globalOptions.polarPlotStrokeColourCallback ? VRS.globalOptions.polarPlotStrokeColourCallback(feedId, slice.lowAlt, slice.highAlt) : VRS.globalOptions.polarPlotStrokeColour || fillColour;
                        var fillOpacity = _this.getFillOpacity();
                        var strokeOpacity = _this.getStrokeOpacity();
                        var zIndex = _this.getSliceRangeZIndex(slice.lowAlt, slice.highAlt);
                        if (existingPolygon) {
                            existingPolygon.setPaths([slice.plots]);
                            existingPolygon.setFillColour(fillColour);
                            existingPolygon.setStrokeColour(strokeColour);
                            existingPolygon.setFillOpacity(fillOpacity);
                            existingPolygon.setStrokeOpacity(strokeOpacity);
                            existingPolygon.setZIndex(zIndex);
                        }
                        else {
                            _this._Settings.map.addPolygon(_this.getPolygonId(feedId, slice), {
                                strokeColour: strokeColour,
                                strokeWeight: VRS.globalOptions.polarPlotStrokeWeight,
                                strokeOpacity: strokeOpacity,
                                fillColour: fillColour,
                                fillOpacity: fillOpacity,
                                paths: [slice.plots],
                                zIndex: zIndex
                            });
                        }
                    }
                    _this.addToPlotsOnDisplay(feedId, slice);
                    if (_this._Settings.autoSaveState) {
                        _this.saveState();
                    }
                }
            };
            this.isOnDisplay = function (feedId, lowAltitude, highAltitude) {
                var result = false;
                var length = _this._PlotsOnDisplay.length;
                for (var i = 0; i < length; ++i) {
                    var plotOnDisplay = _this._PlotsOnDisplay[i];
                    if (plotOnDisplay.feedId === feedId && _this.isSliceForAltitudeRange(plotOnDisplay.slice, lowAltitude, highAltitude)) {
                        result = true;
                        break;
                    }
                }
                return result;
            };
            this.removePolarPlotSlice = function (feedId, slice) {
                if (slice) {
                    var polygonId = _this.getPolygonId(feedId, slice);
                    var existingPolygon = _this._Settings.map.getPolygon(polygonId);
                    if (existingPolygon) {
                        _this._Settings.map.destroyPolygon(existingPolygon);
                    }
                    _this.removeFromPlotsOnDisplay(feedId, slice);
                    if (_this._Settings.autoSaveState) {
                        _this.saveState();
                    }
                }
            };
            this.removeAllSlicesForFeed = function (feedId) {
                var slices = [];
                $.each(_this._PlotsOnDisplay, function (idx, displayInfo) {
                    if (displayInfo.feedId === feedId) {
                        slices.push(displayInfo.slice);
                    }
                });
                $.each(slices, function (idx, slice) {
                    _this.removePolarPlotSlice(feedId, slice);
                });
            };
            this.removeAllSlicesForAllFeeds = function () {
                var plotsOnDisplay = _this._PlotsOnDisplay.slice();
                $.each(plotsOnDisplay, function (idx, plot) {
                    _this.removePolarPlotSlice(plot.feedId, plot.slice);
                });
            };
            this.togglePolarPlotSlice = function (feedId, slice, colour) {
                var result = false;
                if (slice) {
                    var exists = _this.getPlotsOnDisplayIndex(feedId, slice) !== -1;
                    if (exists)
                        _this.removePolarPlotSlice(feedId, slice);
                    else
                        _this.displayPolarPlotSlice(feedId, slice, colour);
                    result = !exists;
                }
                return result;
            };
            this.fetchAndToggleByIdentifiers = function (plotIdentifiers) {
                var notOnDisplay = _this.removeByIdentifiers(plotIdentifiers);
                _this.fetchAndDisplayByIdentifiers(notOnDisplay);
                return notOnDisplay.length > 0;
            };
            this.fetchAndDisplayByIdentifiers = function (plotIdentifiers) {
                var fetchFeeds = _this.getDistinctFeedIds(plotIdentifiers);
                $.each(fetchFeeds, function (idx, feedId) {
                    _this.fetchPolarPlot(feedId, function () {
                        var plots = _this.getPolarPlot();
                        $.each(plotIdentifiers, function (innerIdx, plotIdentifier) {
                            if (plotIdentifier.feedId === feedId) {
                                var slice = _this.findSliceForAltitudeRange(plots, plotIdentifier.lowAlt, plotIdentifier.highAlt);
                                if (slice)
                                    _this.displayPolarPlotSlice(feedId, slice);
                            }
                        });
                    });
                });
            };
            this.removeByIdentifiers = function (plotIdentifiers) {
                var result = [];
                $.each(plotIdentifiers, function (idx, identifier) {
                    var removedSlice = false;
                    var feedPlotsOnDisplay = _this.getPlotsOnDisplayForFeed(identifier.feedId);
                    $.each(feedPlotsOnDisplay, function (innerIdx, plotOnDisplay) {
                        if (_this.isSliceForAltitudeRange(plotOnDisplay.slice, identifier.lowAlt, identifier.highAlt)) {
                            _this.removePolarPlotSlice(identifier.feedId, plotOnDisplay.slice);
                            removedSlice = true;
                        }
                        return !removedSlice;
                    });
                    if (!removedSlice)
                        result.push(identifier);
                });
                return result;
            };
            this.getPolygonId = function (feedId, slice) {
                return 'polar$' + feedId + '$' + (slice.lowAlt === undefined ? 'min' : slice.lowAlt) + '-' + (slice.highAlt === undefined ? 'max' : slice.highAlt);
            };
            this.getPolygonColour = function (slice) {
                var sliceRange = _this.getNormalisedSliceRange(slice);
                return sliceRange.lowAlt === undefined && sliceRange.highAlt === undefined ? '#000000' :
                    VRS.colourHelper.colourToCssString(VRS.colourHelper.getColourWheelScale(sliceRange.lowAlt < -20000000 ? 0 : sliceRange.lowAlt, VRS.globalOptions.aircraftMarkerAltitudeTrailLow, VRS.globalOptions.aircraftMarkerAltitudeTrailHigh, true, true));
            };
            this.getDistinctFeedIds = function (feedIdArray) {
                var result = [];
                var length = feedIdArray.length;
                for (var i = 0; i < length; ++i) {
                    var feedId = feedIdArray[i].feedId;
                    if (VRS.arrayHelper.indexOf(result, feedId) === -1) {
                        result.push(feedId);
                    }
                }
                return result;
            };
            this.refetchAllDisplayed = function () {
                var fetchFeeds = _this.getDistinctFeedIds(_this._PlotsOnDisplay);
                $.each(fetchFeeds, function (idx, feedId) {
                    _this.fetchPolarPlot(feedId, function () {
                        var plots = _this.getPolarPlot();
                        var feedPlotsOnDisplay = _this.getPlotsOnDisplayForFeed(feedId);
                        $.each(feedPlotsOnDisplay, function (innerIdx, plotOnDisplay) {
                            var plottedSlice = plotOnDisplay.slice;
                            var slice = plottedSlice ? _this.findSliceForAltitudeRange(plots, plottedSlice.lowAlt, plottedSlice.highAlt) : null;
                            if (slice) {
                                _this.displayPolarPlotSlice(feedId, slice);
                            }
                            else if (plottedSlice) {
                                _this.removePolarPlotSlice(feedId, plottedSlice);
                            }
                        });
                    });
                });
            };
            this.refreshAllDisplayed = function () {
                var feedsCopy = _this._PlotsOnDisplay.slice();
                $.each(feedsCopy, function (idx, plotOnDisplay) {
                    _this.displayPolarPlotSlice(plotOnDisplay.feedId, plotOnDisplay.slice);
                });
            };
            this.startAutoRefresh = function () {
                if (VRS.globalOptions.polarPlotAutoRefreshSeconds > 0) {
                    if (_this._AutoRefreshTimerId) {
                        clearTimeout(_this._AutoRefreshTimerId);
                    }
                    _this._AutoRefreshTimerId = setTimeout(function () {
                        var timedOut = VRS.timeoutManager && VRS.timeoutManager.getExpired();
                        if (!_this._Settings.aircraftListFetcher.getPaused() && !timedOut) {
                            _this.refetchAllDisplayed();
                        }
                        _this.startAutoRefresh();
                    }, VRS.globalOptions.polarPlotAutoRefreshSeconds * 1000);
                }
            };
            if (!settings)
                throw 'You must supply a settings object';
            if (!settings.aircraftListFetcher)
                throw 'You must supply an aircraftListFetcher object';
            if (!settings.map)
                throw 'You must supply a map';
            if (!settings.unitDisplayPreferences)
                throw 'You must supply the unit display references';
            this._Settings = $.extend({
                name: 'default',
                autoSaveState: true
            }, settings);
        }
        PolarPlotter.prototype.persistenceKey = function () {
            return 'vrsPolarPlotter-' + this.getName();
        };
        PolarPlotter.prototype.createSettings = function () {
            return {
                altitudeRangeConfigs: this.getAltitudeRangeConfigs(),
                plotsOnDisplay: this.getPlotsOnDisplay(),
                strokeOpacity: this.getStrokeOpacity(),
                fillOpacity: this.getFillOpacity()
            };
        };
        return PolarPlotter;
    })();
    VRS.PolarPlotter = PolarPlotter;
})(VRS || (VRS = {}));
//# sourceMappingURL=polarPlotter.js.map