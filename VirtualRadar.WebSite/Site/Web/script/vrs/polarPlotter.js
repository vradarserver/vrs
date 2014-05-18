/**
 * @license Copyright © 2014 onwards, Andrew Whewell
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
 * @fileoverview Code that manages the fetching and display of polar plot slices.
 */

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.polarPlotEnabled = VRS.globalOptions.polarPlotEnabled !== undefined ? VRS.globalOptions.polarPlotEnabled : true;          // True if the polar plotter is enabled, false if it is not.
    VRS.globalOptions.polarPlotFetchUrl = VRS.globalOptions.polarPlotFetchUrl || 'PolarPlot.json';                                              // The URL to fetch polar plots from.
    VRS.globalOptions.polarPlotFetchTimeout = VRS.globalOptions.polarPlotFetchTimeout || 10000;                                                 // The timeout when fetching polar plots.
    VRS.globalOptions.polarPlotAutoRefreshSeconds = VRS.globalOptions.polarPlotAutoRefreshSeconds !== undefined ? VRS.globalOptions.polarPlotAutoRefreshSeconds : 5;    // The number of seconds between automatic refreshes of displayed polar plots. Set to a value less than 1 to disable automatic refreshes.
    VRS.globalOptions.polarPlotAltitudeConfigs = VRS.globalOptions.polarPlotAltitudeConfigs || [                                                      // An array of the altitudes ranges to show in the polar plot menu and the colours for each range. Note that the ranges are defined on the server, if you change the upper or lower bounds of a range you need to modify the server to match and recompile it.
        { low: -1, high: -1, colour: '#000000', zIndex: -5 },
        { low: -1, high: 9999, colour: '#FFFFFF', zIndex: -1 },
        { low: 10000, high: 19999, colour: '#00FF00', zIndex: -2 },
        { low: 20000, high: 29999, colour: '#0000FF', zIndex: -3 },
        { low: 30000, high: -1, colour: '#FF0000', zIndex: -4 }
    ];
    VRS.globalOptions.polarPlotUserConfigurable = VRS.globalOptions.polarPlotUserConfigurable !== undefined ? VRS.globalOptions.polarPlotUserConfigurable : true;   // True if the user can change the colours, false if they can't.
    VRS.globalOptions.polarPlotStrokeWeight = VRS.globalOptions.polarPlotStrokeWeight !== undefined ? VRS.globalOptions.polarPlotStrokeWeight : 2;          // The weight in pixels of the line to draw around the edge of a polar plot.
    VRS.globalOptions.polarPlotStrokeColour = VRS.globalOptions.polarPlotStrokeColour !== undefined ? VRS.globalOptions.polarPlotStrokeColour : '#000000';  // The colour of the polar plot stroke.
    VRS.globalOptions.polarPlotStrokeOpacity = VRS.globalOptions.polarPlotStrokeOpacity !== undefined ? VRS.globalOptions.polarPlotStrokeOpacity : 1.0;     // The opacity of the polar plot stroke.
    VRS.globalOptions.polarPlotFillOpacity = VRS.globalOptions.polarPlotFillOpacity !== undefined ? VRS.globalOptions.polarPlotFillOpacity : 0.50;          // The transparency of the fill area for a polar plot.
    VRS.globalOptions.polarPlotStrokeColourCallback = VRS.globalOptions.polarPlotStrokeColourCallback || undefined;                                         // A function that is passed the feed id, low altitude and high altitude, and returns a CSS colour for the stroke.
    VRS.globalOptions.polarPlotFillColourCallback = VRS.globalOptions.polarPlotFillColourCallback || undefined;                                             // A function that is passed the feed id, low altitude and high altitude, and returns a CSS colour for the fill.
    VRS.globalOptions.polarPlotDisplayOnStartup = VRS.globalOptions.polarPlotDisplayOnStartup || [];  // An array of polar plots to show when the site is loaded. The array of objects is { feedName: string, low: number, high: number }.
    //endregion

    //region PolarPlotter
    /**
     * Creates a new polar plotter object.
     * @param {VRS_SETTINGS_POLAR_PLOTTER} settings
     * @constructor
     */
    VRS.PolarPlotter = function(settings)
    {
        //region -- initialisation
        if(!settings)                           throw 'You must supply a settings object';
        if(!settings.aircraftListFetcher)       throw 'You must supply an aircraftListFetcher object';
        if(!settings.map)                       throw 'You must supply a map';
        if(!settings.unitDisplayPreferences)    throw 'You must supply the unit display references';

        settings = $.extend({
            name: 'default',
            autoSaveState: true
        }, settings);
        //endregion

        //region -- Fields
        var that = this;
        var _AutoRefreshTimerId;

        /**
         * An array of plots that are being displayed.
         * @type {VRS_POLAR_PLOT_DISPLAY[]}
         * @private
         */
        var _PlotsOnDisplay = [];
        function getPlotsOnDisplayIndex(/** number */ feedId, /** VRS_POLAR_PLOTS_SLICE **/ slice) {
            return VRS.arrayHelper.indexOfMatch(_PlotsOnDisplay, function(/** { feedId: number, slice: VRS_POLAR_PLOTS_SLICE } */ item) {
                return item.feedId === feedId && item.slice.lowAlt === slice.lowAlt && item.slice.highAlt === slice.highAlt;
            });
        }
        function addToPlotsOnDisplay(/** number */ feedId, /** VRS_POLAR_PLOTS_SLICE **/ slice) {
            if(getPlotsOnDisplayIndex(feedId, slice) === -1) _PlotsOnDisplay.push({ feedId: feedId, slice: slice });
        }
        function removeFromPlotsOnDisplay(/** number */ feedId, /** VRS_POLAR_PLOTS_SLICE **/ slice) {
            var index = getPlotsOnDisplayIndex(feedId, slice);
            if(index !== -1) _PlotsOnDisplay.splice(index, 1);
        }
        function getPlotsOnDisplayForFeed(/** number */ feedId) {
            return VRS.arrayHelper.filter(_PlotsOnDisplay, function(plotOnDisplay) { return plotOnDisplay.feedId === feedId; })
        }
        //endregion

        //region -- Properties
        /**
         * Returns the name of the object for the purposes of state persistence.
         * @returns {string}
         */
        this.getName = function() { return settings.name; };

        /**
         * The last polar plot fetched from the server.
         * @type {VRS_POLAR_PLOTS}
         * @private
         */
        var _PolarPlot = null;
        /**
         * Returns the last polar plot fetched or null if there is no polar plot.
         * @returns {VRS_POLAR_PLOTS}
         */
        this.getPolarPlot = function() { return _PolarPlot; };

        /**
         * Returns an array of feeds that have polar plots.
         * @returns {VRS_RECEIVER[]}
         */
        this.getPolarPlotterFeeds = function()
        {
            var result = [];
            if(!VRS.serverConfig || VRS.serverConfig.polarPlotsEnabled()) {
                result = VRS.arrayHelper.filter(settings.aircraftListFetcher.getFeeds(), function(feed) {
                    return feed.polarPlot;
                });
            }

            return result;
        };

        /**
         * Returns an array of plots on display.
         * @returns {VRS_POLAR_PLOT_SLICE_ABSTRACT[]}
         */
        this.getPlotsOnDisplay = function()
        {
            var result = [];
            var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;
            if(serverConfig) {
                $.each(_PlotsOnDisplay, function(/** number */ idx, /** VRS_POLAR_PLOT_DISPLAY */ plotOnDisplay) {
                    var receiver = VRS.arrayHelper.findFirst(serverConfig.Receivers, function(/** VRS_SERVER_CONFIG_RECEIVER */ serverReceiver) { return serverReceiver.UniqueId === plotOnDisplay.feedId; });
                    if(receiver) {
                        var normalisedRange = that.getNormalisedSliceRange(plotOnDisplay.slice, -1, -1);
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

        /** @type {VRS_POLAR_PLOT_CONFIG[]} */
        var _AltitudeRangeConfigs = VRS.globalOptions.polarPlotAltitudeConfigs.slice();
        this.getAltitudeRangeConfigs = function() { return _AltitudeRangeConfigs.slice(); };
        this.setAltitudeRangeConfigs = function(/** VRS_POLAR_PLOT_CONFIG[] */ value)
        {
            if(value && _AltitudeRangeConfigs && value.length === _AltitudeRangeConfigs.length) {
                var changed = false;
                var length = value.length;
                for(var i = 0;i < length;++i) {
                    var current = _AltitudeRangeConfigs[i];
                    var revised = value[i];
                    if(current.low === revised.low && current.high === revised.high && current.colour !== revised.colour) {
                        current.colour = revised.colour;
                        changed = true;
                    }
                }

                if(changed) that.refreshAllDisplayed();
            }
        };

        var _StrokeOpacity = VRS.globalOptions.polarPlotStrokeOpacity;
        this.getStrokeOpacity = function() { return _StrokeOpacity; };
        this.setStrokeOpacity = function(/** number */ value) {
            if(value && value !== _StrokeOpacity && value >= 0 && value <= 1) {
                _StrokeOpacity = value;
                that.refreshAllDisplayed();
            }
        };

        var _FillOpacity = VRS.globalOptions.polarPlotFillOpacity;
        this.getFillOpacity = function() { return _FillOpacity; };
        this.setFillOpacity = function(/** number */ value) {
            if(value && value !== _FillOpacity && value >= 0 && value <= 1) {
                _FillOpacity = value;
                that.refreshAllDisplayed();
            }
        };
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            var settings = createSettings();

            // Remove the plotsOnDisplay if the site has a VRS.globalOptions.polarPlotDisplayOnStartup declared and it's
            // exactly the same as the VRS.globalOptions.polarPlotDisplayOnStartup.
            if(VRS.globalOptions.polarPlotDisplayOnStartup && VRS.globalOptions.polarPlotDisplayOnStartup.length) {
                if(settings.plotsOnDisplay.length === VRS.globalOptions.polarPlotDisplayOnStartup.length) {
                    if(VRS.arrayHelper.except(settings.plotsOnDisplay, VRS.globalOptions.polarPlotDisplayOnStartup, function(/** VRS_POLAR_PLOT_SLICE_ABSTRACT */ lhs, /** VRS_POLAR_PLOT_SLICE_ABSTRACT */ rhs) {
                        return lhs.feedName === rhs.feedName &&
                               lhs.high === rhs.high &&
                               lhs.low === rhs.low;
                    }).length === 0) {
                        settings.plotsOnDisplay = undefined;
                    }
                }
            }

            VRS.configStorage.save(persistenceKey(), settings);
        };

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         * @returns {VRS_STATE_POLARPLOTTER}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            if((!savedSettings || !savedSettings.plotsOnDisplay) && VRS.globalOptions.polarPlotDisplayOnStartup) {
                savedSettings = $.extend({ plotsOnDisplay: VRS.globalOptions.polarPlotDisplayOnStartup }, savedSettings);
            }
            var result = $.extend(createSettings(), savedSettings);

            var altitudeRangeConfigsBad = result.altitudeRangeConfigs.length !== _AltitudeRangeConfigs.length;
            if(!altitudeRangeConfigsBad) {
                for(var i = 0;i < result.altitudeRangeConfigs.length;++i) {
                    var current = _AltitudeRangeConfigs[i];
                    var saved = result.altitudeRangeConfigs[i];
                    altitudeRangeConfigsBad = current.low !== saved.low || current.high !== saved.high;
                    if(altitudeRangeConfigsBad) break;
                }
            }
            if(altitudeRangeConfigsBad) result.altitudeRangeConfigs = this.getAltitudeRangeConfigs();

            var usePlotsOnDisplay = [];
            var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;
            var receivers = serverConfig ? serverConfig.Receivers : null;
            if(receivers) {
                $.each(result.plotsOnDisplay, function(/** number */ idx, /** VRS_POLAR_PLOT_SLICE_ABSTRACT */ abstract) {
                    var plotReceiver = VRS.arrayHelper.findFirst(receivers, function(serverReceiver) { return VRS.stringUtility.equals(serverReceiver.Name, abstract.feedName, true); });
                    if(plotReceiver) usePlotsOnDisplay.push(abstract);
                });
            }
            result.plotsOnDisplay = usePlotsOnDisplay;

            return result;
        };

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_POLARPLOTTER} settings
         */
        this.applyState = function(settings)
        {
            var polarPlotIdentifiers = [];
            var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;

            that.setAltitudeRangeConfigs(settings.altitudeRangeConfigs);
            that.setStrokeOpacity(settings.strokeOpacity);
            that.setFillOpacity(settings.fillOpacity);

            if(serverConfig && serverConfig.Receivers) {
                $.each(settings.plotsOnDisplay, function(/** number */ idx, /** { VRS_POLAR_PLOT_SLICE_ABSTRACT } */ abstract) {
                    var colourMaxAltitude = VRS.arrayHelper.findFirst(VRS.globalOptions.polarPlotAltitudeConfigs, function(obj) {
                        return obj.low === abstract.low && obj.high === abstract.high;
                    });
                    var receiver = VRS.arrayHelper.findFirst(serverConfig.Receivers, function(/** VRS_SERVER_CONFIG_RECEIVER */ feed) {
                        return VRS.stringUtility.equals(feed.Name, abstract.feedName, true);
                    });
                    if(colourMaxAltitude && receiver) polarPlotIdentifiers.push({
                        feedId: receiver.UniqueId,
                        lowAlt: colourMaxAltitude.low,
                        highAlt: colourMaxAltitude.high,
                        colour: colourMaxAltitude.colour
                    });
                });
                that.fetchAndDisplayByIdentifiers(polarPlotIdentifiers);
            }
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
            return 'vrsPolarPlotter-' + that.getName();
        }

        /**
         * Creates the saved state object.
         * @returns {VRS_STATE_POLARPLOTTER}
         */
        function createSettings()
        {
            return {
                altitudeRangeConfigs:   that.getAltitudeRangeConfigs(),
                plotsOnDisplay:         that.getPlotsOnDisplay(),
                strokeOpacity:          that.getStrokeOpacity(),
                fillOpacity:            that.getFillOpacity()
            };
        }
        //endregion

        //region -- Configuration - createOptionPane
        /**
         * Creates the configuration pane for the polar plotter.
         * @param {number} displayOrder
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder)
        {
            var result = new VRS.OptionPane({
                name:           'polarPlotColours',
                titleKey:       'PaneReceiverRange',
                displayOrder:   displayOrder
            });

            if((VRS.serverConfig && VRS.serverConfig.polarPlotsEnabled()) && VRS.globalOptions.polarPlotEnabled && VRS.globalOptions.polarPlotUserConfigurable) {
                var configs = that.getAltitudeRangeConfigs();
                $.each(configs, function(/** number */ idx, /** VRS_POLAR_PLOT_CONFIG */ polarPlotConfig) {
                    var colourField = new VRS.OptionFieldColour({
                        name:           'polarPlotColour' + idx,
                        labelKey:       function() { return that.getSliceRangeDescription(polarPlotConfig.low, polarPlotConfig.high); },
                        getValue:       function() { return polarPlotConfig.colour; },
                        setValue:       function(value) { polarPlotConfig.colour = value; },
                        saveState:      function() {
                            that.saveState();
                            that.refreshAllDisplayed();
                        }
                    });
                    result.addField(colourField);
                });

                var commonOpacityOptions = {
                    showSlider: true,
                    min:            0.0,
                    max:            1.0,
                    step:           0.01,
                    decimals:       2,
                    inputWidth:     VRS.InputWidth.ThreeChar
                };

                result.addField(new VRS.OptionFieldNumeric($.extend({
                    name:           'polarPlotterFillOpacity',
                    labelKey:       'FillOpacity',
                    getValue:       that.getFillOpacity,
                    setValue:       that.setFillOpacity,
                    saveState:      that.saveState
                }, commonOpacityOptions)));

                result.addField(new VRS.OptionFieldNumeric($.extend({
                    name:           'polarPlotterStrokeOpacity',
                    labelKey:       'StrokeOpacity',
                    getValue:       that.getStrokeOpacity,
                    setValue:       that.setStrokeOpacity,
                    saveState:      that.saveState
                }, commonOpacityOptions)));
            }

            return result;
        };
        //endregion

        //region -- fetchPolarPlot
        /**
         * Fetches a polar plot from the server.
         * @param {number}      feedId      The identifier of the feed to fetch the polar plot for.
         * @param {function()=} callback    The optional callback method to call when the plot has been fetched.
         */
        this.fetchPolarPlot = function(feedId, callback)
        {
            _PolarPlot = null;

            $.ajax({
                url:        VRS.globalOptions.polarPlotFetchUrl,
                dataType:   'json',
                data:       { feedId: feedId },
                success:    function(/** VRS_POLAR_PLOTS */ data) {
                    _PolarPlot = data;
                    if(callback) callback();
                },
                timeout:    VRS.globalOptions.polarPlotFetchTimeout
            });
        };
        //endregion

        //region -- findSliceForAltitudeRange, isSliceForAltitudeRange
        /**
         * Returns the slice that matches the altitude range (where -1 represents an open end) or null if no slice matches.
         * @param {VRS_POLAR_PLOTS} plots
         * @param {number}          lowAltitude
         * @param {number}          highAltitude
         * @returns {VRS_POLAR_PLOTS_SLICE}
         */
        this.findSliceForAltitudeRange = function(plots, lowAltitude, highAltitude)
        {
            var result = null;
            if(plots) {
                var length = plots.slices.length;
                for(var i = 0;i < length;++i) {
                    var slice = plots.slices[i];
                    if(that.isSliceForAltitudeRange(slice, lowAltitude, highAltitude)) {
                        result = slice;
                        break;
                    }
                }
            }

            return result;
        };

        /**
         * Returns true if the slice corresponds with the altitude range passed across. The altitude range can indicate
         * an open end with a value of -1.
         * @param {VRS_POLAR_PLOTS_SLICE}   slice
         * @param {number}                  lowAltitude
         * @param {number}                  highAltitude
         * @returns {boolean}
         */
        this.isSliceForAltitudeRange = function(slice, lowAltitude, highAltitude)
        {
            return !!slice &&
                ((lowAltitude === -1 && slice.lowAlt < -20000000) || (lowAltitude !== -1 && slice.lowAlt === lowAltitude)) &&
                ((highAltitude === -1 && slice.highAlt > 20000000) || (highAltitude !== -1 && slice.highAlt === highAltitude));
        };
        //endregion

        //region -- getNormalisedSliceRange, getNormalisedRange, getSliceRangeDescription, getAltitudeRangeConfigRecord, getSliceRangeColor, getSliceRangeZIndex
        /**
         * Returns an object that normalises the 'open-ended' low and high altitudes to the undefined value (or to any
         * value that the caller passes across).
         * @param {VRS_POLAR_PLOTS_SLICE}   slice       The slice to display.
         * @param {number=}                 lowOpenEnd  The value that represents the low open end. Defaults to undefined.
         * @param {number=}                 highOpenEnd The value that represents the high open end. Defaults to undefined.
         * @returns {{ lowAlt: number, highAlt: number }}   An object with the low and high altitudes (either or both of which could be undefined if the slice is open-ended).
         */
        this.getNormalisedSliceRange = function(slice, lowOpenEnd, highOpenEnd)
        {
            return !slice ? null : {
                lowAlt:  slice.lowAlt < -20000000 ? lowOpenEnd : slice.lowAlt,
                highAlt: slice.highAlt > 20000000 ? highOpenEnd : slice.highAlt
            };
        };

        /**
         * Returns an object that normalises the 'open-ended' low and high altitudes to the values passed across.
         * @param {number}  lowAltitude
         * @param {number}  highAltitude
         * @param {number}  lowOpenEnd
         * @param {number}  highOpenEnd
         * @returns {{ lowAlt: number, highAlt: number }}
         */
        this.getNormalisedRange = function(lowAltitude, highAltitude, lowOpenEnd, highOpenEnd)
        {
            if(lowAltitude === undefined || lowAltitude < -20000000) lowAltitude = lowOpenEnd;
            if(highAltitude === undefined || highAltitude > 20000000) highAltitude = highOpenEnd;

            return {
                lowAlt: lowAltitude,
                highAlt: highAltitude
            };
        };

        /**
         * Returns a description of the altitude range passed across.
         * @param {number} lowAltitude
         * @param {number} highAltitude
         * @returns {string}
         */
        this.getSliceRangeDescription = function(lowAltitude, highAltitude)
        {
            var range = that.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
            lowAltitude = range.lowAlt;
            highAltitude = range.highAlt;

            var lowAlt = VRS.format.altitude(lowAltitude, false, settings.unitDisplayPreferences.getHeightUnit(), false, true);
            var highAlt = VRS.format.altitude(highAltitude, false, settings.unitDisplayPreferences.getHeightUnit(), false, true);

            return lowAltitude === -1 && highAltitude === -1 ? VRS.$$.AllAltitudes
                 : lowAltitude === -1                        ? VRS.stringUtility.format(VRS.$$.ToAltitude, highAlt)
                 : highAltitude === -1                       ? VRS.stringUtility.format(VRS.$$.FromAltitude, lowAlt)
                                                             : VRS.stringUtility.format(VRS.$$.FromToAltitude, lowAlt, highAlt);
        };

        /**
         * Returns the AltitudeRangeColour object for the low and high altitude passed across. The altitudes are normalised
         * to -1 before they're used in the search.
         * @param {number} lowAltitude
         * @param {number} highAltitude
         * @returns {VRS_POLAR_PLOT_CONFIG}
         */
        this.getAltitudeRangeConfigRecord = function(lowAltitude, highAltitude)
        {
            var altRange = that.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
            lowAltitude = altRange.lowAlt;
            highAltitude = altRange.highAlt;

            var result = null;
            var length = _AltitudeRangeConfigs.length;
            for(var i = 0;i < length;++i) {
                var range = _AltitudeRangeConfigs[i];
                if(range.low === lowAltitude && range.high === highAltitude) {
                    result = range;
                    break;
                }
            }

            return result;
        };

        /**
         * Gets the colour to use for the range or undefined if no colour has been declared for the range.
         * @param {number} lowAltitude
         * @param {number} highAltitude
         * @returns {string}
         */
        this.getSliceRangeColour = function(lowAltitude, highAltitude)
        {
            var altitudeRangeConfig = that.getAltitudeRangeConfigRecord(lowAltitude, highAltitude);
            return altitudeRangeConfig ? altitudeRangeConfig.colour : null;
        };

        /**
         * Gets the z-index to use for the range or -1 if there is no AltitudeRangeColour object for the altitudes.
         * @param lowAltitude
         * @param highAltitude
         */
        this.getSliceRangeZIndex = function(lowAltitude, highAltitude)
        {
            var record = that.getAltitudeRangeConfigRecord(lowAltitude, highAltitude);
            return record ? record.zIndex : -1;
        };
        //endregion

        //region -- displayPolarPlotSlice, removePolarPlotSlice, removeAllSlicesForFeed, togglePolarPlotSlice
        /**
         * Displays a polar plot slice. Does not remove any existing displays of slices.
         * @param {number}                  feedId  The identifier of the feed whose slice is being displayed. Used to generate a unique name for the slice.
         * @param {VRS_POLAR_PLOTS_SLICE}   slice   The slice to display.
         * @param {string}                 [colour] The optional colour to use when displaying the slice. If this is not provided then the default altitude colour is used.
         */
        this.displayPolarPlotSlice = function(feedId, slice, colour)
        {
            if(slice) {
                var polygonId = getPolygonId(feedId, slice);
                var existingPolygon = settings.map.getPolygon(polygonId);
                if(!slice.plots.length) {
                    if(existingPolygon) settings.map.destroyPolygon(existingPolygon);
                } else {
                    if(colour === undefined) colour = that.getSliceRangeColour(slice.lowAlt, slice.highAlt);
                    var fillColour = VRS.globalOptions.polarPlotFillColourCallback ? VRS.globalOptions.polarPlotFillColourCallback(feedId, slice.lowAlt, slice.highAlt) : colour || getPolygonColour(slice);
                    var strokeColour = VRS.globalOptions.polarPlotStrokeColourCallback ? VRS.globalOptions.polarPlotStrokeColourCallback(feedId, slice.lowAlt, slice.highAlt) : VRS.globalOptions.polarPlotStrokeColour || fillColour;
                    var fillOpacity = that.getFillOpacity();
                    var strokeOpacity = that.getStrokeOpacity();
                    var zIndex = that.getSliceRangeZIndex(slice.lowAlt, slice.highAlt);

                    if(existingPolygon) {
                        existingPolygon.setPaths([ slice.plots ]);
                        existingPolygon.setFillColour(fillColour);
                        existingPolygon.setStrokeColour(strokeColour);
                        existingPolygon.setFillOpacity(fillOpacity);
                        existingPolygon.setStrokeOpacity(strokeOpacity);
                        existingPolygon.setZIndex(zIndex);
                    } else {
                        settings.map.addPolygon(getPolygonId(feedId, slice), {
                            strokeColour: strokeColour,
                            strokeWeight: VRS.globalOptions.polarPlotStrokeWeight,
                            strokeOpacity: strokeOpacity,
                            fillColour: fillColour,
                            fillOpacity: fillOpacity,
                            paths: [ slice.plots ],
                            zIndex: zIndex
                        });
                    }
                }
                addToPlotsOnDisplay(feedId, slice);

                if(settings.autoSaveState) that.saveState();
            }
        };

        /**
         * Returns true if the polar plot for the feed and altitude range (where -1 can denote an open end in the range)
         * is currently on display.
         * @param {number}      feedId
         * @param {number}      lowAltitude
         * @param {number}      highAltitude
         * @returns {boolean}
         */
        this.isOnDisplay = function(feedId, lowAltitude, highAltitude)
        {
            var result = false;
            var length = _PlotsOnDisplay.length;
            for(var i = 0;i < length;++i) {
                var plotOnDisplay = _PlotsOnDisplay[i];
                if(plotOnDisplay.feedId === feedId && that.isSliceForAltitudeRange(plotOnDisplay.slice, lowAltitude, highAltitude)) {
                    result = true;
                    break;
                }
            }

            return result;
        };

        /**
         * Removes a polar plot slice.
         * @param {number}                  feedId  The identifier of the feed whose slice is being taken off screen. Used to generate a unique name for the slice.
         * @param {VRS_POLAR_PLOTS_SLICE}   slice   The slice to stop displaying.
         */
        this.removePolarPlotSlice = function(feedId, slice)
        {
            if(slice) {
                var polygonId = getPolygonId(feedId, slice);
                var existingPolygon = settings.map.getPolygon(polygonId);
                if(existingPolygon) settings.map.destroyPolygon(existingPolygon);
                removeFromPlotsOnDisplay(feedId, slice);
                if(settings.autoSaveState) that.saveState();
            }
        };

        /**
         * Removes all plots for the given feed.
         * @param {number} feedId   The identifier of the feed whose slices are to be removed.
         */
        this.removeAllSlicesForFeed = function(feedId)
        {
            var slices = [];
            $.each(_PlotsOnDisplay, function(/** number */ idx, /** Object */ displayInfo) {
                if(displayInfo.feedId === feedId) slices.push(displayInfo.slice);
            });
            $.each(slices, function(/** number */ idx, /** VRS_POLAR_PLOT_DISPLAY */ slice) {
                that.removePolarPlotSlice(feedId, slice);
            });
        };

        /**
         * Removes all plots for all feeds on display.
         */
        this.removeAllSlicesForAllFeeds = function()
        {
            var plotsOnDisplay = _PlotsOnDisplay.slice();
            $.each(plotsOnDisplay, function(/** number */ idx, /** VRS_POLAR_PLOT_DISPLAY */ plot) {
                that.removePolarPlotSlice(plot.feedId, plot.slice);
            });
        };

        /**
         * Toggles the display of a polar plot slice.
         * @param {number}                  feedId  The identifier of the feed whose slice is being taken off screen. Used to generate a unique name for the slice.
         * @param {VRS_POLAR_PLOTS_SLICE}   slice   The slice to stop displaying.
         * @param {string}                 [colour] The optional colour to use when displaying the slice. If this is not provided then the default altitude colour is used.
         * @returns {boolean} True if the slice was displayed by the call, false if it was hidden by the call.
         */
        this.togglePolarPlotSlice = function(feedId, slice, colour)
        {
            var result = false;

            if(slice) {
                var exists = getPlotsOnDisplayIndex(feedId, slice) !== -1;
                if(exists) that.removePolarPlotSlice(feedId, slice);
                else       that.displayPolarPlotSlice(feedId, slice, colour);
                result = !exists;
            }

            return result;
        };
        //endregion

        //region -- fetchAndToggleByIdentifiers, fetchAndDisplayByIdentifiers, removeByIdentifiers
        /**
         * Fetches the feeds and toggles the display of the array of polar plot identifiers passed across.
         * @param {VRS_POLAR_PLOT_ID[]} plotIdentifiers
         */
        this.fetchAndToggleByIdentifiers = function(plotIdentifiers)
        {
            var notOnDisplay = that.removeByIdentifiers(plotIdentifiers);
            that.fetchAndDisplayByIdentifiers(notOnDisplay);
        };

        /**
         * Fetches and displays all the identifiers specified. If the feed is already on display then it is refreshed.
         * @param {VRS_POLAR_PLOT_ID[]} plotIdentifiers
         */
        this.fetchAndDisplayByIdentifiers = function(plotIdentifiers)
        {
            var fetchFeeds = getDistinctFeedIds(plotIdentifiers);

            $.each(fetchFeeds, function(/** number */ idx, /** number */ feedId) {
                that.fetchPolarPlot(feedId, function() {
                    var plots = that.getPolarPlot();
                    $.each(plotIdentifiers, function(/** number */ innerIdx, /** VRS_POLAR_PLOT_ID */ plotIdentifier) {
                        if(plotIdentifier.feedId === feedId) {
                            var slice = that.findSliceForAltitudeRange(plots, plotIdentifier.lowAlt, plotIdentifier.highAlt);
                            if(slice) that.displayPolarPlotSlice(feedId, slice);
                        }
                    });
                });
            });
        };

        /**
         * Removes all plots identified. Returns an array of plot identifiers that could not be removed because they
         * are not on display.
         * @param {VRS_POLAR_PLOT_ID[]} plotIdentifiers
         * @returns {VRS_POLAR_PLOT_ID[]}
         */
        this.removeByIdentifiers = function(plotIdentifiers)
        {
            /** @type {VRS_POLAR_PLOT_ID[]} */
            var result = [];

            $.each(plotIdentifiers, function(/** number */ idx, /** VRS_POLAR_PLOT_ID */ identifier) {
                var removedSlice = false;

                var feedPlotsOnDisplay = getPlotsOnDisplayForFeed(identifier.feedId);
                $.each(feedPlotsOnDisplay, function(/** number */ innerIdx, /** VRS_POLAR_PLOT_DISPLAY */ plotOnDisplay) {
                    if(that.isSliceForAltitudeRange(plotOnDisplay.slice, identifier.lowAlt, identifier.highAlt)) {
                        that.removePolarPlotSlice(identifier.feedId, plotOnDisplay.slice);
                        removedSlice = true;
                    }
                    return !removedSlice;
                });

                if(!removedSlice) result.push(identifier);
            });

            return result;
        };
        //endregion

        //region -- getPolygonId, getPolygonColour, getDistinctFeedIds
        /**
         * Generates the identifier for the polygon that represents a slice retrieved for a specific feed.
         * @param {number}                  feedId  The identifier of the feed whose slice is being displayed.
         * @param {VRS_POLAR_PLOTS_SLICE}   slice   The slice to display.
         * @returns {string}
         */
        function getPolygonId(feedId, slice)
        {
            return 'polar$' + feedId + '$' + (slice.lowAlt === undefined ? 'min' : slice.lowAlt) + '-' + (slice.highAlt === undefined ? 'max' : slice.highAlt);
        }

        /**
         * Returns the colour to use for a particular slice, based on the minimum or maximum altitude.
         * @param {VRS_POLAR_PLOTS_SLICE}   slice
         * @returns {string}
         */
        function getPolygonColour(slice)
        {
            var sliceRange = that.getNormalisedSliceRange(slice);
            return sliceRange.lowAlt === undefined && sliceRange.highAlt === undefined ? '#000000' :
                         VRS.colourHelper.colourToCssString(
                             VRS.colourHelper.getColourWheelScale(sliceRange.lowAlt < -20000000 ? 0 : sliceRange.lowAlt, VRS.globalOptions.aircraftMarkerAltitudeTrailLow, VRS.globalOptions.aircraftMarkerAltitudeTrailHigh, true, true)
                         );
        }

        /**
         * Returns the list of distinct feed identifiers from the array passed across.
         * @param {VRS_POLAR_PLOT_ID[]|VRS_POLAR_PLOT_DISPLAY[]} feedIdArray
         * @returns {number[]}
         */
        function getDistinctFeedIds(feedIdArray)
        {
            var result = [];

            var length = feedIdArray.length;
            for(var i = 0;i < length;++i) {
                var feedId = feedIdArray[i].feedId;
                if(VRS.arrayHelper.indexOf(result, feedId) === -1) result.push(feedId);
            }

            return result;
        }
        //endregion

        //region -- refetchAllDisplayed, refreshAllDisplayed, startAutoRefresh
        /**
         * Fetches and redisplays all displayed plots.
         */
        this.refetchAllDisplayed = function()
        {
            var fetchFeeds = getDistinctFeedIds(_PlotsOnDisplay);

            $.each(fetchFeeds, function(/** number */ idx, /** number */ feedId) {
                that.fetchPolarPlot(feedId, function() {
                    var plots = that.getPolarPlot();
                    var feedPlotsOnDisplay = getPlotsOnDisplayForFeed(feedId);
                    $.each(feedPlotsOnDisplay, function(/** number */ innerIdx, /** VRS_POLAR_PLOT_DISPLAY */ plotOnDisplay) {
                        var plottedSlice = plotOnDisplay.slice;
                        var slice = plottedSlice ? that.findSliceForAltitudeRange(plots, plottedSlice.lowAlt, plottedSlice.highAlt) : null;
                        if(slice)               that.displayPolarPlotSlice(feedId, slice);
                        else if(plottedSlice)   that.removePolarPlotSlice(feedId, plottedSlice);
                    });
                });
            });
        };

        /**
         * Repaints all displayed plots without fetching them from the server.
         */
        this.refreshAllDisplayed = function()
        {
            var feedsCopy = _PlotsOnDisplay.slice();
            $.each(feedsCopy, function(/** number */ idx, /** VRS_POLAR_PLOT_DISPLAY */ plotOnDisplay) {
                that.displayPolarPlotSlice(plotOnDisplay.feedId, plotOnDisplay.slice);
            });
        };

        /**
         * Starts a timer that periodically refetches and repaints all polar plots currently on display.
         */
        this.startAutoRefresh = function()
        {
            if(VRS.globalOptions.polarPlotAutoRefreshSeconds > 0) {
                if(_AutoRefreshTimerId) clearTimeout(_AutoRefreshTimerId);
                _AutoRefreshTimerId = setTimeout(function() {
                    var timedOut = VRS.timeoutManager && VRS.timeoutManager.getExpired();
                    if(!settings.aircraftListFetcher.getPaused() && !timedOut) {
                        that.refetchAllDisplayed();
                    }
                    that.startAutoRefresh();
                }, VRS.globalOptions.polarPlotAutoRefreshSeconds * 1000);
            }
        };
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
