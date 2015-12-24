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
/**
 * @fileoverview Code that manages the fetching and display of polar plot slices.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
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

    /**
     * The settings to pass when creating a new instance of a PolarPlotter.
     */
    export interface PolarPlotter_Settings
    {
        name?:                  string;
        map:                    IMap;
        aircraftListFetcher:    AircraftListFetcher
        autoSaveState?:         boolean;
        unitDisplayPreferences: UnitDisplayPreferences;
    }

    /**
     * Describes a polar plot slice - a range of altitudes and the entire polygon path of the plot for that range.
     */
    export interface PolarPlot_Slice
    {
         lowAlt:      number;
         highAlt:     number;
         plots:       ILatLng[];
    }

    /**
     * Associates a slice with a feed.
     */
    interface PolarPlot_FeedSlice
    {
        feedId:  number;
        slice:   PolarPlot_Slice;
    }

    /**
     * An external description of a feed's slice. This is a part of the polar plotter's saved state.
     */
    export interface PolarPlot_FeedSliceAbstract
    {
        feedName:    string;
        low:         number;
        high:        number;
    }

    /**
     * Carries all of the slices for a feed. This is what the server sends to us.
     */
    export interface PolarPlot_AllFeedSlices
    {
        feedId:      number;
        slices:      PolarPlot_Slice[];
    }

    /**
     * Describes the display configuration for a single slice.
     */
    export interface PolarPlot_AltitudeRangeConfig
    {
        low:         number;
        high:        number;
        colour:      string;
        zIndex:      number;
    }

    /**
     * The settings that are saved between sessions by PolarPlotter.
     */
    export interface PolarPlotter_SaveState
    {
        altitudeRangeConfigs:    PolarPlot_AltitudeRangeConfig[];
        plotsOnDisplay:          PolarPlot_FeedSliceAbstract[];
        strokeOpacity:           number;
        fillOpacity:             number;
    }

    /**
     * An object whose properties can identify a polar plot slice.
     */
    export interface PolarPlot_Id
    {
        feedId:      number;
        lowAlt:      number;
        highAlt:     number;
        colour?:     string;
    }

    /**
     * Creates a new polar plotter object.
     */
    export class PolarPlotter implements ISelfPersist<PolarPlotter_SaveState>
    {
        private _Settings: PolarPlotter_Settings;
        private _AutoRefreshTimerId: number;

        private _PlotsOnDisplay: PolarPlot_FeedSlice[] = [];      // The slices being shown on display
        private _PolarPlot: PolarPlot_AllFeedSlices = null;       // The last set of polar plots fetched from the server
        private  _AltitudeRangeConfigs: PolarPlot_AltitudeRangeConfig[] = VRS.globalOptions.polarPlotAltitudeConfigs.slice();
        private _StrokeOpacity: number = VRS.globalOptions.polarPlotStrokeOpacity;
        private _FillOpacity: number = VRS.globalOptions.polarPlotFillOpacity;

        constructor(settings: PolarPlotter_Settings)
        {
            if(!settings)                           throw 'You must supply a settings object';
            if(!settings.aircraftListFetcher)       throw 'You must supply an aircraftListFetcher object';
            if(!settings.map)                       throw 'You must supply a map';
            if(!settings.unitDisplayPreferences)    throw 'You must supply the unit display references';

            this._Settings = $.extend({
                name: 'default',
                autoSaveState: true
            }, settings);
        }

        /**
         * Gets the index number of a plot that is being shown for a feed.
         */
        private getPlotsOnDisplayIndex = (feedId: number, slice: PolarPlot_Slice) : number =>
        {
            return VRS.arrayHelper.indexOfMatch(this._PlotsOnDisplay, (item) => {
                return item.feedId === feedId && item.slice.lowAlt === slice.lowAlt && item.slice.highAlt === slice.highAlt;
            });
        }

        /**
         * Records a polar plot slice against a feed. If the feed already has a slice assigned to it then this
         * function does nothing.
         */
        private addToPlotsOnDisplay = (feedId: number, slice: PolarPlot_Slice) =>
        {
            if(this.getPlotsOnDisplayIndex(feedId, slice) === -1) {
                this._PlotsOnDisplay.push({ feedId: feedId, slice: slice });
            }
        }

        /**
         * Removes a slice from the plots recorded as being shown for a feed. If the feed does not already have
         * the slice's altitudes recorded as being on display then this function does nothing.
         */
        private removeFromPlotsOnDisplay = (feedId: number, slice: PolarPlot_Slice) =>
        {
            var index = this.getPlotsOnDisplayIndex(feedId, slice);
            if(index !== -1) {
                this._PlotsOnDisplay.splice(index, 1);
            }
        }

        /**
         * Returns an array of every slice being shown for a feed.
         */
        private getPlotsOnDisplayForFeed = (feedId: number) : PolarPlot_FeedSlice[] =>
        {
            return VRS.arrayHelper.filter(this._PlotsOnDisplay, function(plotOnDisplay) {
                return plotOnDisplay.feedId === feedId;
            })
        }

        /**
         * Returns the name of the object for the purposes of state persistence.
         */
        getName = () : string =>
        {
            return this._Settings.name;
        }

        /**
         * Returns the last polar plot fetched or null if there is none.
         */
        getPolarPlot = () : PolarPlot_AllFeedSlices =>
        {
            return this._PolarPlot;
        }

        /**
         * Returns an array of feeds that have polar plots.
         */
        getPolarPlotterFeeds = () : IReceiver[] =>
        {
            var result: IReceiver[] = [];

            if(!VRS.serverConfig || VRS.serverConfig.polarPlotsEnabled()) {
                result = VRS.arrayHelper.filter(this._Settings.aircraftListFetcher.getFeeds(), function(feed) {
                    return feed.polarPlot;
                });
            }

            return result;
        }

        /**
         * Returns an array of feeds that have polar plots in order of feed name.
         */
        getSortedPolarPlotterFeeds = () : IReceiver[] =>
        {
            var result = this.getPolarPlotterFeeds();
            result.sort(function(lhs, rhs) {
                if(lhs.id !== undefined && rhs.id !== undefined)        return lhs.name.localeCompare(rhs.name);
                else if(lhs.id === undefined && rhs.id === undefined)   return 0;
                else if(lhs.id === undefined)                           return -1;
                else                                                    return 1;
            });

            return result;
        }

        /**
         * Returns an array of plots on display.
         */
        getPlotsOnDisplay = () : PolarPlot_FeedSliceAbstract[] =>
        {
            var result: PolarPlot_FeedSliceAbstract[] = [];
            var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;
            if(serverConfig) {
                $.each(this._PlotsOnDisplay, (idx, plotOnDisplay) => {
                    var receiver = VRS.arrayHelper.findFirst(serverConfig.Receivers, function(serverReceiver) {
                        return serverReceiver.UniqueId === plotOnDisplay.feedId;
                    });
                    if(receiver) {
                        var normalisedRange = this.getNormalisedSliceRange(plotOnDisplay.slice, -1, -1);
                        result.push({
                            feedName: receiver.Name,
                            low: normalisedRange.lowAlt,
                            high: normalisedRange.highAlt
                        });
                    }
                });
            }

            return result;
        }

        getAltitudeRangeConfigs = () : PolarPlot_AltitudeRangeConfig[] =>
        {
            return this._AltitudeRangeConfigs.slice();
        }
        setAltitudeRangeConfigs = (value: PolarPlot_AltitudeRangeConfig[]) =>
        {
            if(value && this._AltitudeRangeConfigs && value.length === this._AltitudeRangeConfigs.length) {
                var changed = false;
                var length = value.length;
                for(var i = 0;i < length;++i) {
                    var current = this._AltitudeRangeConfigs[i];
                    var revised = value[i];
                    if(current.low === revised.low && current.high === revised.high && current.colour !== revised.colour) {
                        current.colour = revised.colour;
                        changed = true;
                    }
                }

                if(changed) {
                    this.refreshAllDisplayed();
                }
            }
        }

        getStrokeOpacity = () : number =>
        {
            return this._StrokeOpacity;
        }
        setStrokeOpacity = (value: number) =>
        {
            if(value && value !== this._StrokeOpacity && value >= 0 && value <= 1) {
                this._StrokeOpacity = value;
                this.refreshAllDisplayed();
            }
        }

        getFillOpacity = () : number =>
        {
            return this._FillOpacity;
        }
        setFillOpacity = (value: number) =>
        {
            if(value && value !== this._FillOpacity && value >= 0 && value <= 1) {
                this._FillOpacity = value;
                this.refreshAllDisplayed();
            }
        }

        /**
         * Saves the current state of the object.
         */
        saveState = () =>
        {
            var settings = this.createSettings();

            // Remove the plotsOnDisplay if the site has a VRS.globalOptions.polarPlotDisplayOnStartup declared and it's
            // exactly the same as the VRS.globalOptions.polarPlotDisplayOnStartup.
            if(VRS.globalOptions.polarPlotDisplayOnStartup && VRS.globalOptions.polarPlotDisplayOnStartup.length) {
                if(settings.plotsOnDisplay.length === VRS.globalOptions.polarPlotDisplayOnStartup.length) {
                    if(VRS.arrayHelper.except(settings.plotsOnDisplay, VRS.globalOptions.polarPlotDisplayOnStartup, function(lhs, rhs) {
                        return lhs.feedName === rhs.feedName &&
                               lhs.high === rhs.high &&
                               lhs.low === rhs.low;
                    }).length === 0) {
                        settings.plotsOnDisplay = undefined;
                    }
                }
            }

            VRS.configStorage.save(this.persistenceKey(), settings);
        }

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         */
        loadState = () : PolarPlotter_SaveState =>
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            if((!savedSettings || !savedSettings.plotsOnDisplay) && VRS.globalOptions.polarPlotDisplayOnStartup) {
                savedSettings = $.extend({ plotsOnDisplay: VRS.globalOptions.polarPlotDisplayOnStartup }, savedSettings);
            }
            var result = $.extend(this.createSettings(), savedSettings);

            var altitudeRangeConfigsBad = result.altitudeRangeConfigs.length !== this._AltitudeRangeConfigs.length;
            if(!altitudeRangeConfigsBad) {
                for(var i = 0;i < result.altitudeRangeConfigs.length;++i) {
                    var current = this._AltitudeRangeConfigs[i];
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
                $.each(result.plotsOnDisplay, function(idx, abstractReceiver) {
                    var plotReceiver = VRS.arrayHelper.findFirst(receivers, function(serverReceiver) {
                        return VRS.stringUtility.equals(serverReceiver.Name, abstractReceiver.feedName, true);
                    });
                    if(plotReceiver) {
                        usePlotsOnDisplay.push(abstractReceiver);
                    }
                });
            }
            result.plotsOnDisplay = usePlotsOnDisplay;

            return result;
        }

        /**
         * Applies a previously saved state to the object.
         */
        applyState = (settings: PolarPlotter_SaveState) =>
        {
            var polarPlotIdentifiers = [];
            var serverConfig = VRS.serverConfig ? VRS.serverConfig.get() : null;

            this.setAltitudeRangeConfigs(settings.altitudeRangeConfigs);
            this.setStrokeOpacity(settings.strokeOpacity);
            this.setFillOpacity(settings.fillOpacity);

            if(serverConfig && serverConfig.Receivers) {
                $.each(settings.plotsOnDisplay, function(idx, abstractReceiver) {
                    var colourMaxAltitude = VRS.arrayHelper.findFirst(<PolarPlot_AltitudeRangeConfig[]>VRS.globalOptions.polarPlotAltitudeConfigs, function(obj) {
                        return obj.low === abstractReceiver.low && obj.high === abstractReceiver.high;
                    });
                    var receiver = VRS.arrayHelper.findFirst(serverConfig.Receivers, function(feed) {
                        return VRS.stringUtility.equals(feed.Name, abstractReceiver.feedName, true);
                    });
                    if(colourMaxAltitude && receiver) polarPlotIdentifiers.push({
                        feedId: receiver.UniqueId,
                        lowAlt: colourMaxAltitude.low,
                        highAlt: colourMaxAltitude.high,
                        colour: colourMaxAltitude.colour
                    });
                });
                this.fetchAndDisplayByIdentifiers(polarPlotIdentifiers);
            }
        }

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        loadAndApplyState = () =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the state will be saved.
         */
        private persistenceKey() : string
        {
            return 'vrsPolarPlotter-' + this.getName();
        }

        /**
         * Creates the saved state object.
         */
        private createSettings() : PolarPlotter_SaveState
        {
            return {
                altitudeRangeConfigs:   this.getAltitudeRangeConfigs(),
                plotsOnDisplay:         this.getPlotsOnDisplay(),
                strokeOpacity:          this.getStrokeOpacity(),
                fillOpacity:            this.getFillOpacity()
            };
        }

        /**
         * Creates the configuration pane for the polar plotter.
         */
        createOptionPane = (displayOrder: number) : OptionPane =>
        {
            var result = new VRS.OptionPane({
                name:           'polarPlotColours',
                titleKey:       'PaneReceiverRange',
                displayOrder:   displayOrder
            });

            if((VRS.serverConfig && VRS.serverConfig.polarPlotsEnabled()) && VRS.globalOptions.polarPlotEnabled && VRS.globalOptions.polarPlotUserConfigurable) {
                var configs = this.getAltitudeRangeConfigs();
                $.each(configs, (idx, polarPlotConfig) => {
                    var colourField = new VRS.OptionFieldColour({
                        name:           'polarPlotColour' + idx,
                        labelKey:       () => this.getSliceRangeDescription(polarPlotConfig.low, polarPlotConfig.high),
                        getValue:       () => polarPlotConfig.colour,
                        setValue:       (value) => polarPlotConfig.colour = value,
                        saveState:      () => {
                            this.saveState();
                            this.refreshAllDisplayed();
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
                    getValue:       this.getFillOpacity,
                    setValue:       this.setFillOpacity,
                    saveState:      this.saveState
                }, commonOpacityOptions)));

                result.addField(new VRS.OptionFieldNumeric($.extend({
                    name:           'polarPlotterStrokeOpacity',
                    labelKey:       'StrokeOpacity',
                    getValue:       this.getStrokeOpacity,
                    setValue:       this.setStrokeOpacity,
                    saveState:      this.saveState
                }, commonOpacityOptions)));
            }

            return result;
        }

        /**
         * Fetches a polar plot from the server.
         */
        fetchPolarPlot = (feedId: number, callback?: () => void) =>
        {
            this._PolarPlot = null;

            $.ajax({
                url:        VRS.globalOptions.polarPlotFetchUrl,
                dataType:   'json',
                data:       { feedId: feedId },
                success:    (data: PolarPlot_AllFeedSlices) => {
                    this._PolarPlot = data;
                    if(callback) callback();
                },
                timeout:    VRS.globalOptions.polarPlotFetchTimeout
            });
        }

        /**
         * Returns the slice that matches the altitude range (where -1 represents an open end) or null if no slice matches.
         */
        findSliceForAltitudeRange = (plots: PolarPlot_AllFeedSlices, lowAltitude: number, highAltitude: number) : PolarPlot_Slice =>
        {
            var result = null;
            if(plots) {
                var length = plots.slices.length;
                for(var i = 0;i < length;++i) {
                    var slice = plots.slices[i];
                    if(this.isSliceForAltitudeRange(slice, lowAltitude, highAltitude)) {
                        result = slice;
                        break;
                    }
                }
            }

            return result;
        }

        /**
         * Returns true if the slice corresponds with the altitude range passed across. The altitude range can indicate
         * an open end with a value of -1.
         */
        isSliceForAltitudeRange = (slice: PolarPlot_Slice, lowAltitude: number, highAltitude: number) : boolean =>
        {
            return !!slice &&
                ((lowAltitude === -1 && slice.lowAlt < -20000000) || (lowAltitude !== -1 && slice.lowAlt === lowAltitude)) &&
                ((highAltitude === -1 && slice.highAlt > 20000000) || (highAltitude !== -1 && slice.highAlt === highAltitude));
        }

        /**
         * Returns an object that normalises the 'open-ended' low and high altitudes to the undefined value (or to any
         * value that the caller passes across).
         */
        getNormalisedSliceRange = (slice: PolarPlot_Slice, lowOpenEnd?: number, highOpenEnd?: number) : IAltitudeRange =>
        {
            return !slice ? null : {
                lowAlt:  slice.lowAlt < -20000000 ? lowOpenEnd : slice.lowAlt,
                highAlt: slice.highAlt > 20000000 ? highOpenEnd : slice.highAlt
            };
        }

        /**
         * Returns an object that normalises the 'open-ended' low and high altitudes to the values passed across.
         */
        getNormalisedRange = (lowAltitude?: number, highAltitude?: number, lowOpenEnd?: number, highOpenEnd?: number) : IAltitudeRange =>
        {
            if(lowAltitude === undefined || lowAltitude < -20000000) lowAltitude = lowOpenEnd;
            if(highAltitude === undefined || highAltitude > 20000000) highAltitude = highOpenEnd;

            return {
                lowAlt: lowAltitude,
                highAlt: highAltitude
            };
        }

        /**
         * Returns a description of the altitude range passed across.
         */
        getSliceRangeDescription = (lowAltitude: number, highAltitude: number) : string =>
        {
            var range = this.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
            lowAltitude = range.lowAlt;
            highAltitude = range.highAlt;

            var lowAlt = VRS.format.altitude(lowAltitude, VRS.AltitudeType.Barometric, false, this._Settings.unitDisplayPreferences.getHeightUnit(), false, true, false);
            var highAlt = VRS.format.altitude(highAltitude, VRS.AltitudeType.Barometric, false, this._Settings.unitDisplayPreferences.getHeightUnit(), false, true, false);

            return lowAltitude === -1 && highAltitude === -1 ? VRS.$$.AllAltitudes
                 : lowAltitude === -1                        ? VRS.stringUtility.format(VRS.$$.ToAltitude, highAlt)
                 : highAltitude === -1                       ? VRS.stringUtility.format(VRS.$$.FromAltitude, lowAlt)
                                                             : VRS.stringUtility.format(VRS.$$.FromToAltitude, lowAlt, highAlt);
        }

        /**
         * Returns true if the altitude range passed across represents all altitudes.
         */
        isAllAltitudes = (lowAltitude: number, highAltitude: number) : boolean =>
        {
            var range = this.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
            lowAltitude = range.lowAlt;
            highAltitude = range.highAlt;

            return lowAltitude === -1 && highAltitude === -1;
        }

        /**
         * Returns the AltitudeRangeColour object for the low and high altitude passed across. The altitudes are normalised
         * to -1 before they're used in the search.
         */
        getAltitudeRangeConfigRecord = (lowAltitude: number, highAltitude: number) : PolarPlot_AltitudeRangeConfig =>
        {
            var altRange = this.getNormalisedRange(lowAltitude, highAltitude, -1, -1);
            lowAltitude = altRange.lowAlt;
            highAltitude = altRange.highAlt;

            var result = null;
            var length = this._AltitudeRangeConfigs.length;
            for(var i = 0;i < length;++i) {
                var range = this._AltitudeRangeConfigs[i];
                if(range.low === lowAltitude && range.high === highAltitude) {
                    result = range;
                    break;
                }
            }

            return result;
        }

        /**
         * Gets the colour to use for the range or undefined if no colour has been declared for the range.
         */
        getSliceRangeColour = (lowAltitude: number, highAltitude: number) : string =>
        {
            var altitudeRangeConfig = this.getAltitudeRangeConfigRecord(lowAltitude, highAltitude);
            return altitudeRangeConfig ? altitudeRangeConfig.colour : null;
        }

        /**
         * Gets the z-index to use for the range or -1 if there is no AltitudeRangeColour object for the altitudes.
         */
        getSliceRangeZIndex = (lowAltitude: number, highAltitude: number) : number =>
        {
            var record = this.getAltitudeRangeConfigRecord(lowAltitude, highAltitude);
            return record ? record.zIndex : -1;
        }

        /**
         * Displays a polar plot slice. Does not remove any existing displays of slices.
         */
        displayPolarPlotSlice = (feedId: number, slice: PolarPlot_Slice, colour?: string) =>
        {
            if(slice) {
                var polygonId = this.getPolygonId(feedId, slice);
                var existingPolygon = this._Settings.map.getPolygon(polygonId);
                if(!slice.plots.length) {
                    if(existingPolygon) {
                        this._Settings.map.destroyPolygon(existingPolygon);
                    }
                } else {
                    if(colour === undefined) {
                        colour = this.getSliceRangeColour(slice.lowAlt, slice.highAlt);
                    }
                    var fillColour = VRS.globalOptions.polarPlotFillColourCallback ? VRS.globalOptions.polarPlotFillColourCallback(feedId, slice.lowAlt, slice.highAlt) : colour || this.getPolygonColour(slice);
                    var strokeColour = VRS.globalOptions.polarPlotStrokeColourCallback ? VRS.globalOptions.polarPlotStrokeColourCallback(feedId, slice.lowAlt, slice.highAlt) : VRS.globalOptions.polarPlotStrokeColour || fillColour;
                    var fillOpacity = this.getFillOpacity();
                    var strokeOpacity = this.getStrokeOpacity();
                    var zIndex = this.getSliceRangeZIndex(slice.lowAlt, slice.highAlt);

                    if(existingPolygon) {
                        existingPolygon.setPaths([ slice.plots ]);
                        existingPolygon.setFillColour(fillColour);
                        existingPolygon.setStrokeColour(strokeColour);
                        existingPolygon.setFillOpacity(fillOpacity);
                        existingPolygon.setStrokeOpacity(strokeOpacity);
                        existingPolygon.setZIndex(zIndex);
                    } else {
                        this._Settings.map.addPolygon(this.getPolygonId(feedId, slice), {
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
                this.addToPlotsOnDisplay(feedId, slice);

                if(this._Settings.autoSaveState) {
                    this.saveState();
                }
            }
        }

        /**
         * Returns true if the polar plot for the feed and altitude range (where -1 can denote an open end in the range)
         * is currently on display.
         */
        isOnDisplay = (feedId: number, lowAltitude: number, highAltitude: number) : boolean =>
        {
            var result = false;
            var length = this._PlotsOnDisplay.length;
            for(var i = 0;i < length;++i) {
                var plotOnDisplay = this._PlotsOnDisplay[i];
                if(plotOnDisplay.feedId === feedId && this.isSliceForAltitudeRange(plotOnDisplay.slice, lowAltitude, highAltitude)) {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /**
         * Removes a polar plot slice.
         */
        removePolarPlotSlice = (feedId: number, slice: PolarPlot_Slice) =>
        {
            if(slice) {
                var polygonId = this.getPolygonId(feedId, slice);
                var existingPolygon = this._Settings.map.getPolygon(polygonId);
                if(existingPolygon) {
                    this._Settings.map.destroyPolygon(existingPolygon);
                }
                this.removeFromPlotsOnDisplay(feedId, slice);
                if(this._Settings.autoSaveState) {
                    this.saveState();
                }
            }
        }

        /**
         * Removes all plots for the given feed.
         */
        removeAllSlicesForFeed = (feedId: number) =>
        {
            var slices = [];
            $.each(this._PlotsOnDisplay, (idx, displayInfo) => {
                if(displayInfo.feedId === feedId) {
                    slices.push(displayInfo.slice);
                }
            });
            $.each(slices, (idx, slice) => {
                this.removePolarPlotSlice(feedId, slice);
            });
        }

        /**
         * Removes all plots for all feeds on display.
         */
        removeAllSlicesForAllFeeds = () =>
        {
            var plotsOnDisplay = this._PlotsOnDisplay.slice();
            $.each(plotsOnDisplay, (idx, plot) => {
                this.removePolarPlotSlice(plot.feedId, plot.slice);
            });
        }

        /**
         * Toggles the display of a polar plot slice.
         */
        togglePolarPlotSlice = (feedId: number, slice: PolarPlot_Slice, colour?: string) : boolean =>
        {
            var result = false;

            if(slice) {
                var exists = this.getPlotsOnDisplayIndex(feedId, slice) !== -1;
                if(exists) this.removePolarPlotSlice(feedId, slice);
                else       this.displayPolarPlotSlice(feedId, slice, colour);
                result = !exists;
            }

            return result;
        }

        /**
         * Fetches the feeds and toggles the display of the array of polar plot identifiers passed across.
         */
        fetchAndToggleByIdentifiers = (plotIdentifiers: PolarPlot_Id[]) : boolean =>
        {
            var notOnDisplay = this.removeByIdentifiers(plotIdentifiers);
            this.fetchAndDisplayByIdentifiers(notOnDisplay);

            return notOnDisplay.length > 0;
        }

        /**
         * Fetches and displays all the identifiers specified. If the feed is already on display then it is refreshed.
         */
        fetchAndDisplayByIdentifiers = (plotIdentifiers: PolarPlot_Id[]) =>
        {
            var fetchFeeds = this.getDistinctFeedIds(plotIdentifiers);

            $.each(fetchFeeds, (idx, feedId) => {
                this.fetchPolarPlot(feedId, () => {
                    var plots = this.getPolarPlot();
                    $.each(plotIdentifiers, (innerIdx, plotIdentifier) => {
                        if(plotIdentifier.feedId === feedId) {
                            var slice = this.findSliceForAltitudeRange(plots, plotIdentifier.lowAlt, plotIdentifier.highAlt);
                            if(slice) this.displayPolarPlotSlice(feedId, slice);
                        }
                    });
                });
            });
        }

        /**
         * Removes all plots identified. Returns an array of plot identifiers that could not be removed because they
         * are not on display.
         */
        removeByIdentifiers = (plotIdentifiers: PolarPlot_Id[]) : PolarPlot_Id[] =>
        {
            var result: PolarPlot_Id[] = [];

            $.each(plotIdentifiers, (idx, identifier) => {
                var removedSlice = false;

                var feedPlotsOnDisplay = this.getPlotsOnDisplayForFeed(identifier.feedId);
                $.each(feedPlotsOnDisplay, (innerIdx, plotOnDisplay) => {
                    if(this.isSliceForAltitudeRange(plotOnDisplay.slice, identifier.lowAlt, identifier.highAlt)) {
                        this.removePolarPlotSlice(identifier.feedId, plotOnDisplay.slice);
                        removedSlice = true;
                    }
                    return !removedSlice;
                });

                if(!removedSlice) result.push(identifier);
            });

            return result;
        }

        /**
         * Generates the identifier for the polygon that represents a slice retrieved for a specific feed.
         */
        private getPolygonId = (feedId: number, slice: PolarPlot_Slice) : string =>
        {
            return 'polar$' + feedId + '$' + (slice.lowAlt === undefined ? 'min' : slice.lowAlt) + '-' + (slice.highAlt === undefined ? 'max' : slice.highAlt);
        }

        /**
         * Returns the colour to use for a particular slice, based on the minimum or maximum altitude.
         */
        private getPolygonColour = (slice: PolarPlot_Slice) : string =>
        {
            var sliceRange = this.getNormalisedSliceRange(slice);
            return sliceRange.lowAlt === undefined && sliceRange.highAlt === undefined ? '#000000' :
                         VRS.colourHelper.colourToCssString(
                             VRS.colourHelper.getColourWheelScale(sliceRange.lowAlt < -20000000 ? 0 : sliceRange.lowAlt, VRS.globalOptions.aircraftMarkerAltitudeTrailLow, VRS.globalOptions.aircraftMarkerAltitudeTrailHigh, true, true)
                         );
        }

        /**
         * Returns the list of distinct feed identifiers from the array passed across.
         */
        private getDistinctFeedIds = (feedIdArray: PolarPlot_Id[] | PolarPlot_FeedSlice[]) : number[] =>
        {
            var result: number[] = [];

            var length = feedIdArray.length;
            for(var i = 0;i < length;++i) {
                var feedId = feedIdArray[i].feedId;
                if(VRS.arrayHelper.indexOf(result, feedId) === -1) {
                    result.push(feedId);
                }
            }

            return result;
        }

        /**
         * Fetches and redisplays all displayed plots.
         */
        refetchAllDisplayed = () =>
        {
            var fetchFeeds = this.getDistinctFeedIds(this._PlotsOnDisplay);

            $.each(fetchFeeds, (idx, feedId) => {
                this.fetchPolarPlot(feedId, () => {
                    var plots = this.getPolarPlot();
                    var feedPlotsOnDisplay = this.getPlotsOnDisplayForFeed(feedId);
                    $.each(feedPlotsOnDisplay, (innerIdx, plotOnDisplay) => {
                        var plottedSlice = plotOnDisplay.slice;
                        var slice = plottedSlice ? this.findSliceForAltitudeRange(plots, plottedSlice.lowAlt, plottedSlice.highAlt) : null;
                        if(slice) {
                            this.displayPolarPlotSlice(feedId, slice);
                        } else if(plottedSlice) {
                            this.removePolarPlotSlice(feedId, plottedSlice);
                        }
                    });
                });
            });
        }

        /**
         * Repaints all displayed plots without fetching them from the server.
         */
        refreshAllDisplayed = () =>
        {
            var feedsCopy = this._PlotsOnDisplay.slice();
            $.each(feedsCopy, (idx, plotOnDisplay) => {
                this.displayPolarPlotSlice(plotOnDisplay.feedId, plotOnDisplay.slice);
            });
        }

        /**
         * Starts a timer that periodically refetches and repaints all polar plots currently on display.
         */
        startAutoRefresh = () =>
        {
            if(VRS.globalOptions.polarPlotAutoRefreshSeconds > 0) {
                if(this._AutoRefreshTimerId) {
                    clearTimeout(this._AutoRefreshTimerId);
                }
                this._AutoRefreshTimerId = setTimeout(() => {
                    var timedOut = VRS.timeoutManager && VRS.timeoutManager.getExpired();
                    if(!this._Settings.aircraftListFetcher.getPaused() && !timedOut) {
                        this.refetchAllDisplayed();
                    }
                    this.startAutoRefresh();
                }, VRS.globalOptions.polarPlotAutoRefreshSeconds * 1000);
            }
        }
    }
} 