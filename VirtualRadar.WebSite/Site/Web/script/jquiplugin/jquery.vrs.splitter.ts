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
 * @fileoverview A jQuery UI widget that displays exactly two elements with a splitter between them.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.splitterBorderWidth = VRS.globalOptions.splitterBorderWidth !== undefined ? VRS.globalOptions.splitterBorderWidth : 1;    // The width in pixels of the borders around splitters.

    /**
     * Carries the state of a VRS splitter.
     */
    class Splitter_State
    {
        /**
         * A single instance of a SplitterPaneDetail.
         */
        bar: SplitterPaneDetail = null;

        /**
         * An array of 2 SplitterPaneDetail objects.
         */
        panes: SplitterPaneDetail[] = [];

        /**
         * The pixel offset of the left/top edge of the splitter bar.
         */
        posn: number = -1;

        /**
         * The collapse button jQuery element.
         */
        collapseButton: JQuery = null;

        /**
         * 1 if the first pane has been collapsed, 2 if the second pane has been collapsed. 0 if neither are collapsed.
         */
        collapsedPane: number = 0;

        /**
         * True if the position has been set by the user.
         */
        userSetPosn: boolean = false;

        /**
         * The pixel position from which the splitter is dragged. -1 if the splitter is not being dragged.
         */
        startMovePosn: number = -1;

        /**
         * The previous pixel position of the splitter bar.
         */
        previousPosn: number = -1;

        /**
         * The previous pixel length of pane 0.
         */
        previousPane0Length: number = -1;

        /**
         * The previous pixel length of pane 1.
         */
        previousPane1Length: number = -1;

        /**
         * The namespace that all DOM events that use $.proxy are suffixed with to ensure that unbinds only unbind
         * our splitter and nothing else.
         */
        eventNamespace: string = '';
    }

    /**
     * Carries the state of a single pane within a VRS splitter.
     */
    class SplitterPane_State
    {
        /**
         * The container element that wraps the content in a splitter pane.
         */
        container: JQuery = null;

        /**
         * The direct-access Splitter jQuery UI object - only set if the content is itself a splitter.
         */
        splitterContent: Splitter = null;

        /**
         * The CSS that needs to be applied to the content to bring it back to the state it was in before it was placed in a pane.
         */
        originalContentCss: Object = null;
    }

    /**
     * Groups together things that a splitter knows about one of its panes, or about its bar.
     */
    class SplitterPaneDetail
    {
        constructor(public element: JQuery)
        {
        }

        /**
         * The direct-access object to the splitterPane plugin for the pane. Note that the detail for the bar doesn't have one of these.
         */
        splitterPane: SplitterPane = undefined;

        /**
         * The length, in pixels, of the pane in the relevant dimension (width for vertical splitters, height for horizontal splitters)
         */
        length: number = 0;
    }

    /**
     * The details recorded in SplitterGroupPersistence for a splitter.
     */
    export interface Splitter_Detail
    {
        splitter:           Splitter;
        barMovedHookResult: IEventHandleJQueryUI;
        pane1Length?:       number;
        pane2Length?:       number;
    }

    /**
     * The state object saved by SplitterGroupPersistence for a group of (potentially) nested splitters.
     */
    export interface SplitterGroupPersistence_SaveState
    {
        lengths: SplitterGroupPersistence_SplitterSaveState[];
    }

    /**
     * The state object recording persisted state for a single splitter.
     */
    export interface SplitterGroupPersistence_SplitterSaveState
    {
        name:        string;
        pane:        number;
        vertical:    boolean;
        length:      number;
    }

    /**
     * The object responsible for saving and loading the state of a group of nested splitters.
     */
    export class SplitterGroupPersistence implements ISelfPersist<SplitterGroupPersistence_SaveState>
    {
        private _Name: string;
        private _SplitterDetails: { [index: string]: Splitter_Detail } = {};    // An associative array of splitter details indexed by splitter name.
        private _AutoSaveEnabled = false;

        constructor(name: string)
        {
            if(!name || name === '') throw 'You must supply the name that the group of splitters will be saved under';
            this._Name = name;
        }

        getAutoSaveEnabled() : boolean
        {
            return this._AutoSaveEnabled;
        }
        setAutoSaveEnabled(value: boolean)
        {
            this._AutoSaveEnabled = value;
        }

        /**
         * Releases the resources attached to the splitter group persistence object.
         */
        dispose()
        {
            $.each(this._SplitterDetails, function() {
                var details = this;
                if(details.splitter && details.barMovedHookResult) {
                    details.splitter.unhook(details.barMovedHookResult);
                }
            });

            this._SplitterDetails = {};
        }

        /**
         * Saves the current state of the group of splitters.
         */
        saveState()
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Returns the saved state for the group of splitters or the current state if no state has been saved.
         */
        loadState() : SplitterGroupPersistence_SaveState
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        }

        /**
         * Returns the saved state for a single splitter within the group.
         */
        getSplitterSavedState(splitterName: string)
        {
            var result = null;

            var savedSettings = VRS.configStorage.load(this.persistenceKey(), null);
            if(savedSettings) {
                $.each(savedSettings.lengths, (idx, savedSplitter) => {
                    if(savedSplitter.name === splitterName) {
                        result = savedSplitter;
                    }
                    return result === null;
                });
            }

            return result;
        }

        /**
         * Applies the saved state to the splitters held by the object.
         */
        applyState(settings: SplitterGroupPersistence_SaveState)
        {
            var autoSaveState = this.getAutoSaveEnabled();
            this.setAutoSaveEnabled(false);

            var length = settings.lengths ? settings.lengths.length : -1;
            for(var i = 0;i < length;++i) {
                var details = settings.lengths[i];
                var splitterDetails = this._SplitterDetails[details.name];
                if(splitterDetails) {
                    splitterDetails.splitter.applySavedLength(details);
                }
            }

            this.setAutoSaveEnabled(autoSaveState);
        }

        /**
         * Loads and applies the saved state.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key to save the splitter positions against.
         */
        private persistenceKey() : string
        {
            return 'vrsSplitterPosition-' + this._Name;
        }

        /**
         * Returns the current state.
         */
        private createSettings() : SplitterGroupPersistence_SaveState
        {
            var lengths = [];
            for(var splitterName in this._SplitterDetails) {
                var splitterDetails = this._SplitterDetails[splitterName];
                var splitter = splitterDetails.splitter;

                var pane = splitter.getSavePane();
                lengths.push({
                    name: splitter.getName(),
                    pane: pane,
                    vertical: splitter.getIsVertical(),
                    length: pane === 1 ? splitterDetails.pane1Length : splitterDetails.pane2Length
                });
            }

            return {
                lengths: lengths
            };
        }

        /**
         * Adds a splitter to the group of splitters whose positions will be saved.
         */
        registerSplitter(splitterElement: JQuery) : Splitter_Detail
        {
            var splitter = VRS.jQueryUIHelper.getSplitterPlugin(splitterElement);
            var splitterName = splitter.getName();
            var existingSplitter = this._SplitterDetails[splitterName];
            if(existingSplitter) throw 'The ' + splitterName + ' splitter has already been registered';

            this._SplitterDetails[splitterName] = {
                splitter: splitter,
                barMovedHookResult: splitter.hookBarMoved(this.onBarMoved, this)
            };

            return this.getSplitterSavedState(splitterName);
        }

        /**
         * Called when the user moves a splitter that this object is saving state for.
         */
        private onBarMoved(event: Event, data: Splitter_BarMovedEventArgs)
        {
            var splitter = VRS.jQueryUIHelper.getSplitterPlugin(data.splitterElement);
            var splitterDetails = splitter ? this._SplitterDetails[splitter.getName()] : null;
            if(splitterDetails) {
                splitterDetails.pane1Length = data.pane1Length;
                splitterDetails.pane2Length = data.pane2Length;

                if(this.getAutoSaveEnabled()) {
                    this.saveState();
                }
            }
        }
    }

    /*
     * JQueryUIHelper
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    jQueryUIHelper.getSplitterPlugin = (jQueryElement: JQuery) : Splitter =>
    {
        return <Splitter>jQueryElement.data('vrsVrsSplitter');
    }
    jQueryUIHelper.getSplitterPanePlugin = (jQueryElement: JQuery) : SplitterPane =>
    {
        return <SplitterPane>jQueryElement.data('vrsVrsSplitterPane');
    }

    /**
     * The options supported by a SplitterPane.
     */
    export interface SplitterPane_Options
    {
        /**
         * True if the parent splitter is vertical.
         */
        isVertical?: boolean;

        /**
         * The smallest allowable number of pixels.
         */
        minPixels?: number;

        /**
         * Either a pixel value (can be a %age) or a method that takes the width of the splitter without the bar and returns a number of pixels.
         */
        max?: PercentValue | ((splitterWidth: number) => number);

        /**
         * Either a pixel value (can be a %age) or a method that takes the width of the splitter without the bar and returns a number of pixels.
         */
        startSize?: PercentValue | ((splitterWidth: number) => number);
    }

    /**
     * A jQuery UI widget that encapsulates a single pane within a splitter.
     */
    export class SplitterPane extends JQueryUICustomWidget
    {
        options: SplitterPane_Options = {
            isVertical: false,
            minPixels: 1,
        };

        _getState()
        {
            var result = this.element.data('splitterPaneState');
            if(result === undefined) {
                result = new SplitterPane_State();
                this.element.data('splitterPaneState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();

            state.originalContentCss = this.element.css([
                'width',
                'height'
            ]);

            this.element.css({
                width: '100%',
                height: '100%'
            });

            state.container = $('<div/>')
                .css({
                    position: 'absolute',
                    left: '0',
                    top: '0',
                    'z-index': '1',
                    '-moz-outline-style': 'none',
                    width: '100%',
                    height: '100%'
                })
                .addClass('splitterPane')
                .insertBefore(this.element)  // Don't use wrap, it uses a copy
                .append(this.element);

            state.splitterContent = VRS.jQueryUIHelper.getSplitterPlugin(this.element);
            if(!state.splitterContent) {
                state.container
                    .css({
                        overflow: 'auto'
                    })
                    .addClass('border');
            }

            if(VRS.refreshManager) {
                VRS.refreshManager.registerOwner(state.container);
            }
        }

        _destroy()
        {
            var state = this._getState();
            if(state.container) {
                if(VRS.refreshManager) {
                    VRS.refreshManager.unregisterOwner(state.container);
                }

                this.element.unwrap();
                state.container = null;

                if(state.originalContentCss) {
                    this.element.css(state.originalContentCss);
                }
                state.originalContentCss = null;
            }
        }

        /**
         * Returns the pane's container object. This object is at the top level of elements, it is the one that the splitter controls.
         */
        getContainer() : JQuery
        {
            return this._getState().container;
        }

        /**
         * Returns the pane's content element. This object is below the container, it's the UI that the user sees within the splitter.
         */
        getContent() : JQuery
        {
            return this.element;
        }

        /**
         * Returns true if the content of the pane is another splitter.
         */
        getContentIsSplitter() : boolean
        {
            return !!this._getState().splitterContent;
        }

        /**
         * Returns the minimum number of pixels that this pane can be sized to.
         */
        getMinPixels() : number
        {
            return this.options.minPixels;
        }

        /**
         * Returns the maximum number of pixels that this pane can be sized to.
         * @param {number} availableLengthWithoutBar    The pixel width of the parent splitter after the splitter bar has been removed.
         * @returns {number=}
         */
        getMaxPixels(availableLengthWithoutBar: number) : number
        {
            return this._getPixelsFromMaxOrStartSize(this.options.max, availableLengthWithoutBar);
        }

        /**
         * Returns the initial size of the splitter in pixels.
         * @param {number} availableLengthWithoutBar    The pixel width of the parent splitter after the splitter bar has been removed.
         */
        getStartSize(availableLengthWithoutBar: number) : number
        {
            return this._getPixelsFromMaxOrStartSize(this.options.startSize, availableLengthWithoutBar);
        }

        /**
         * Returns either the maxmimum number of pixels allowed or the starting size
         * @param {(VRS_VALUE_PERCENT|function(number):number)=}    maxOrStartSize              The max or startSize value to resolve.
         * @param {number}                                          availableLengthWithoutBar   The width of the parent splitter after the bar has been removed.
         * @returns {number=}                                                                   The resolved pixel width or null if there is no maximum / start width.
         */
        _getPixelsFromMaxOrStartSize(maxOrStartSize: PercentValue | ((size: number) => number), availableLengthWithoutBar: number) : number
        {
            var result: number = null;

            if(maxOrStartSize) {
                if(maxOrStartSize instanceof Function) {
                    result = maxOrStartSize(availableLengthWithoutBar);
                } else {
                    var valuePercent = <PercentValue>maxOrStartSize;
                    result = valuePercent.value;
                    if(valuePercent.isPercent) {
                        result = Math.max(1, Math.ceil(availableLengthWithoutBar * result));
                    }
                }
            }

            return result;
        }
    }

    $.widget('vrs.vrsSplitterPane', new SplitterPane());

    /**
     * The options that a Splitter supports.
     */
    export interface Splitter_Options
    {
        /**
         * The mandatory name for the splitter.
         */
        name: string;

        /**
         * True if the splitter is vertical, false if it is horizontal.
         */
        vertical?: boolean;

        /**
         * 1 if the first pane's location should be preserved across page refreshes, 2 if the 2nd pane's location should be preserved. Not used with fixed splitters.
         */
        savePane?: number;

        /**
         * 1 if the first pane should be collapsible, 2 if the second is collapsible.
         */
        collapsePane?: number;

        /**
         * A two element array holding the minimum number of pixels for each pane. Use undefined / null if a particular pane has no minimum (in which case a minimum of 1 is applied).
         */
        minPixels?: number[];

        /**
         * 1 if the max value applies to the first pane, 2 if it applies to the second pane.
         */
        maxPane?: number;

        /**
         * Either an integer number of pixels or a string ending in % for a percentage. Can also be a function that is passed the available length and returns the number of pixels.
         */
        max?: number | string | PercentValue | ((availableLength: number) => number);

        /**
         * 1 if the start size applies to the first pane, 2 if it applies to the second pane.
         */
        startSizePane?: number;

        /**
         * An integer pixels, %age of available length or function returning number of pixels when passed the available length.
         */
        startSize?: number | string | PercentValue | ((availableLength: number) => number);

        /**
         * The persistence object that this splitter will have its bar positions saved through.
         */
        splitterGroupPersistence?: SplitterGroupPersistence;

        /**
         * True if the splitter is not nested within another splitter.
         */
        isTopLevelSplitter?: boolean;

        /**
         * The element to force the first pane to reattach to if the splitter is destroyed.
         */
        leftTopParent?: JQuery;

        /**
         * The element to force the second pane to reattach to if the splitter is destroyed.
         */
        rightBottomParent?: JQuery;
    }

    /**
     * The event args for the Splitter BarMoved event.
     */
    export interface Splitter_BarMovedEventArgs
    {
        splitterElement:     JQuery;
        pane1Length:         number;
        pane2Length:         number;
        barLength:           number;
    }

    /**
     * A JQueryUI plugin that adds a splitter with two panes. Splitters can be nested within other splitters.
     */
    export class Splitter extends JQueryUICustomWidget
    {
        options: Splitter_Options = {
            name:                       undefined,
            vertical:                   true,
            savePane:                   1,
            collapsePane:               undefined,
            minPixels:                  [ 10, 10 ],
            maxPane:                    0,
            max:                        null,
            startSizePane:              0,
            startSize:                  null,
            splitterGroupPersistence:   null,
            isTopLevelSplitter:         false,
        };

        _getState()
        {
            var result = this.element.data('splitterState');
            if(result === undefined) {
                result = new Splitter_State();
                this.element.data('splitterState', result);
            }

            return result;
        }

        _create()
        {
            var options = this.options;
            var state = this._getState();
            var i;

            if(!options.name) throw 'You must supply a name for the splitter';
            state.eventNamespace = 'vrsSplitter-' + options.name;

            options.max = this.convertMaxOrStartSize(options.maxPane, options.max, 'max');
            options.startSize = this.convertMaxOrStartSize(options.startSizePane, options.startSize, 'start size');
            if(options.minPixels.length !== 2) throw 'You must pass two integers for minPixels';

            if(!options.minPixels[0] || options.minPixels[0] < 1) options.minPixels[0] = 1;
            if(!options.minPixels[1] || options.minPixels[1] < 1) options.minPixels[1] = 1;

            this.element
                .attr('id', 'vrsSplitter-' + options.name)
                .css({
                    position: 'absolute',
                    width: '100%',
                    height: '100%',
                    overflow: 'hidden'
                });
            this.element.addClass('vrsSplitter');

            var children = this.element.children();
            if(children.length !== 2) throw 'A splitter control must have two children';
            for(i = 0;i < 2;++i) {
                var pane = $(children[i]).vrsSplitterPane(<SplitterPane_Options>{
                    isVertical: options.vertical,
                    minPixels:  options.minPixels[i],
                    max:        options.maxPane === i + 1 ? <PercentValue>options.max : undefined,
                    startSize:  options.startSizePane === i + 1 ? <PercentValue>options.startSize : undefined
                });
                var splitterPane = VRS.jQueryUIHelper.getSplitterPanePlugin(pane);
                var detail = new SplitterPaneDetail(splitterPane.getContainer());
                detail.splitterPane = splitterPane;
                state.panes.push(detail);
            }

            if(VRS.refreshManager) VRS.refreshManager.rebuildRelationships();

            state.bar = new SplitterPaneDetail($('<div/>')
                .insertAfter(state.panes[0].element)
                .addClass('bar')
                .addClass(options.vertical ? 'vertical' : 'horizontal')
                .addClass('movable')
                .css({
                    'z-index': '100',
                    position: 'absolute',
                    'user-select': 'none',
                    '-webkit-user-select': 'none',
                    '-khtml-user-select': 'none',
                    '-moz-user-select': 'none'
                })
            );
            state.bar.element.mousedown($.proxy(this._barMouseDown, this));
            state.bar.element.on('touchstart', $.proxy(this._touchStart, this));
            state.bar.length = options.vertical ? state.bar.element.outerWidth() : state.bar.element.outerHeight();

            if(options.collapsePane) {
                state.collapseButton = $('<div/>')
                    .addClass('collapse')
                    .addClass(options.vertical ? 'vertical' : 'horizontal')
                    .click($.proxy(this._collapseClicked, this))
                    .appendTo(state.bar.element);
                state.bar.element
                    .dblclick($.proxy(this._collapseClicked, this));
                this._syncCollapseButtonState(state);
            }

            var savedState = null;
            if(options.splitterGroupPersistence) {
                savedState = options.splitterGroupPersistence.registerSplitter(this.element);
            }

            var availableLength = this._determineAvailableLength(state);
            if(savedState) {
                this.applySavedLength(savedState);
            } else if(options.startSizePane) {
                var availableLengthWithoutBar = this._determineAvailableLengthWithoutBar(state, availableLength);
                var paneDetail = state.panes[options.startSizePane - 1];
                paneDetail.length = paneDetail.splitterPane.getStartSize(availableLengthWithoutBar);
                this._moveSplitterToFitPaneLength(state, options.startSizePane - 1, availableLength);
                this._setPositions(state, availableLength);
            } else {
                state.posn = Math.floor(availableLength / 2);
                this._sizePanesToSplitter(state, availableLength);
                this._setPositions(state, availableLength);
            }

            this._raiseBarMoved(state);
            if(VRS.refreshManager && state.panes.length >= 2) {
                VRS.refreshManager.refreshTargets(state.panes[0].element);
                VRS.refreshManager.refreshTargets(state.panes[1].element);
            }

            $(window).on('resize.' + state.eventNamespace, $.proxy(this._windowResized, this));
        }

        _destroy()
        {
            var options = this.options;
            var state = this._getState();
            if(state.panes && state.panes.length === 2) {
                for(var i = 0;i < state.panes.length;++i) {
                    var details = state.panes[i];
                    var element = details.splitterPane.getContent();
                    var originalParent = i === 0 ? options.leftTopParent : options.rightBottomParent;
                    var elementIsSplitter = details.splitterPane.getContentIsSplitter();

                    details.splitterPane.destroy();

                    if(elementIsSplitter) {
                        element.vrsSplitter('destroy');
                    } else if(originalParent) {
                        element.appendTo(originalParent);
                    }
                }

                state.panes = [];
            }

            if(state.bar !== null) {
                state.bar.element.off();
                state.bar.element.remove();
                state.bar = null;
            }

            $(window).off('resize.' + state.eventNamespace, this._windowResized);

            this.element.remove();

            if(VRS.refreshManager) {
                VRS.refreshManager.rebuildRelationships();
            }
        }

        /**
         * Converts a string / number / function into either a VRS_VALUE_PERCENT or a function that takes a width and returns a number of pixels.
         */
        private convertMaxOrStartSize(paneNumber: number, maxOrStartSize: string | number | PercentValue | ((availableLength: number) => number), description: string) : PercentValue | ((availableLength: number) => number)
        {
            var result: PercentValue | ((availableLength: number) => number) = <any>maxOrStartSize;
            if(paneNumber) {
                if(!(maxOrStartSize instanceof Function) && (<PercentValue>maxOrStartSize).isPercent === undefined) {
                    var valuePercent = VRS.unitConverter.getPixelsOrPercent(<string | number>maxOrStartSize);
                    if(valuePercent.isPercent && (valuePercent.value < 0.01 || valuePercent.value > 0.99)) throw description + ' percent must be between 1% and 99% inclusive';
                    result = valuePercent;
                }
            }

            return result;
        }

        /**
         * Gets the name of the splitter.
         */
        getName() : string
        {
            return this.options.name;
        }

        /**
         * Gets a value indicating whether the bar is vertical or horizontal.
         */
        getIsVertical() : boolean
        {
            return this.options.vertical;
        }

        /**
         * Gets the number of the pane whose dimensions are to be saved. Only one pane is ever saved.
         */
        getSavePane() : number
        {
            return this.options.savePane;
        }

        /**
         * Raised when the splitter bar is moved.
         */
        hookBarMoved(callback: (event: Event, data: Splitter_BarMovedEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsSplitter', 'barMoved', callback, forceThis);
        }
        private _raiseBarMoved(state: Splitter_State)
        {
            var panesCount = state.panes.length;
            this._trigger('barMoved', null, <Splitter_BarMovedEventArgs>{
                splitterElement: this.element,
                pane1Length:     panesCount > 0 ? state.panes[0].length : -1,
                pane2Length:     panesCount > 1 ? state.panes[1].length : -1,
                barLength:       state.bar ? state.bar.length : -1
            });
        }

        /**
         * Unhooks an event hooked on the object.
         */
        unhook(hookResult: IEventHandleJQueryUI)
        {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        }

        /**
         * Returns the current length available to the splitter, within which the panes and bar have to fit.
         */
        private _determineAvailableLength(state: Splitter_State) : number
        {
            var result = this.options.vertical ? this.element.width() : this.element.height();

            // Each pane has a border applied to it unless that pane contains a splitter. We need to knock the widths
            // of the border off the available length.
            if(!state) state = this._getState();
            for(var i = 0;i < 2;++i) {
                if(!state.panes[0].splitterPane.getContentIsSplitter()) result -= 2 * VRS.globalOptions.splitterBorderWidth;
            }

            return result;
        }

        /**
         * Returns the length available to the panes, i.e. the overall length minus the bar length.
         */
        private _determineAvailableLengthWithoutBar(state: Splitter_State, availableLength: number) : number
        {
            if(!state) state = this._getState();
            if(availableLength === undefined) availableLength = this._determineAvailableLength(state);

            return Math.max(0, availableLength - state.bar.length);
        }

        /**
         * Changes the size of both panes to make them fit flush with the current position of the splitter bar.
         */
        private _sizePanesToSplitter(state: Splitter_State, availableLength: number)
        {
            if(availableLength === undefined) availableLength = this._determineAvailableLength(state);
            var bar = state.bar;
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];

            pane0.length = state.posn;
            pane1.length = availableLength - (pane0.length + bar.length);
        }

        /**
         * Constrains the lengths of the panes to their minimums and maximums.
         */
        _applyMinMax(state: Splitter_State, availableLength: number)
        {
            if(!state) state = this._getState();
            if(state.panes.length > 1) {
                if(availableLength === undefined) availableLength = this._determineAvailableLength(state);
                var availableWithoutBar = this._determineAvailableLengthWithoutBar(state, availableLength);

                var moveSplitter = (offsetPixels: number, subtract: boolean) => {
                    offsetPixels = Math.max(0, offsetPixels);
                    if(offsetPixels) {
                        if(subtract) offsetPixels = -offsetPixels;
                        state.posn += offsetPixels;
                        this._sizePanesToSplitter(state, availableLength);
                    }
                };
                for(var i = 1;i >= 0;--i) {
                    var paneDetail = state.panes[i];
                    var minPixels = paneDetail.splitterPane.getMinPixels();
                    var maxPixels = paneDetail.splitterPane.getMaxPixels(availableWithoutBar);
                    if(maxPixels) moveSplitter(paneDetail.length - maxPixels, i === 0);
                    if(minPixels) moveSplitter(minPixels - paneDetail.length, i === 1);
                }
            }
        }

        /**
         * Sizes the panes to match the saved state passed across.
         */
        applySavedLength(savedState: SplitterGroupPersistence_SplitterSaveState)
        {
            var options = this.options;
            var pane = savedState.pane;
            var vertical = savedState.vertical;
            var length = savedState.length;

            if(options.savePane === pane && options.vertical === vertical) {
                var state = this._getState();
                if(state.panes.length > 1) {
                    state.userSetPosn = true;
                    var paneIndex = pane - 1;
                    state.panes[paneIndex].length = length;
                    var availableLength = this._determineAvailableLength(state);
                    this._moveSplitterToFitPaneLength(state, paneIndex, availableLength);
                    this._setPositions(state, availableLength);
                }
            }
        }

        /**
         * Updates the CSS on the panes and bar to move them into position on screen and raises events to let observers
         * know that the panes have been resized or moved. Returns true if the panes had to be resized, false if there was
         * no work to do.
         */
        private _setPositions(state: Splitter_State, availableLength: number) : boolean
        {
            if(!state) state = this._getState();
            if(availableLength === undefined) availableLength = this._determineAvailableLength(state);

            var collapsed = !!state.collapsedPane;
            if(!collapsed) this._applyMinMax(state, availableLength);

            var options = this.options;
            var bar = state.bar;
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];

            var pane0Length = pane0.length;
            var pane1Length = pane1.length;
            var barLength = bar.length;
            var posn = state.posn;

            if(collapsed) {
                switch(state.collapsedPane) {
                    case 1:
                        pane0Length = 0;
                        posn = 0;
                        pane1Length += pane0.length;
                        break;
                    case 2:
                        pane0Length += pane1.length;
                        posn = pane0Length;
                        pane1Length = 0;
                        break;
                }
            }

            var pane1Posn = posn + barLength;

            var result = state.previousPosn !== posn || state.previousPane0Length !== pane0Length || state.previousPane1Length !== pane1Length;
            if(result) {
                state.previousPosn = posn;
                state.previousPane0Length = pane0Length;
                state.previousPane1Length = pane1Length;
                var barOffset = pane0Length === 0 ? 0 : pane0.splitterPane.getContentIsSplitter() ? 0 : 2 * VRS.globalOptions.splitterBorderWidth;
                pane1Posn += barOffset;

                if(options.vertical) {
                    pane1.element.css({ left: pane1Posn, width: pane1Length });
                    bar.element.css({ left: posn + barOffset });
                    pane0.element.css({ width: pane0Length });
                } else {
                    pane1.element.css({ top: pane1Posn, height: pane1Length });
                    bar.element.css({ top: posn + barOffset });
                    pane0.element.css({ height: pane0Length });
                }

                if(VRS.refreshManager) {
                    if(pane0Length) VRS.refreshManager.refreshTargets(pane0.element);
                    if(pane1Length) VRS.refreshManager.refreshTargets(pane1.element);
                }

                this._raiseBarMoved(state);
            }

            return result;
        }

        /**
         * Assuming that one pane has the correct length this method adjusts the length of the other pane and the position
         * of the splitter bar to fill all available room.
         */
        private _moveSplitterToFitPaneLength(state: Splitter_State, fitPaneIndex: number, availableLength?: number)
        {
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];
            var bar = state.bar;
            if(availableLength === undefined) {
                availableLength = this._determineAvailableLength(state);
            }

            switch(fitPaneIndex) {
                case 0:
                    state.posn = pane0.length;
                    pane1.length = availableLength - (pane0.length + bar.length);
                    break;
                case 1:
                    state.posn = availableLength - (bar.length + pane1.length);
                    pane0.length = state.posn;
                    break;
                default:
                    throw 'Not implemented';
            }
        }

        /**
         * Assuming that the elements are in the correct position but the available room has changed, this changes the
         * lengths of the panes and the splitter position to try to keep both panes occupying the same proportion of
         * the new available length.
         */
        private _moveSplitterToKeepPaneProportions(state: Splitter_State, availableLength: number)
        {
            var options = this.options;
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];
            var bar = state.bar;
            if(availableLength === undefined) availableLength = this._determineAvailableLength(state);
            var oldAvailableLength = pane0.length + pane1.length + bar.length;

            if(oldAvailableLength !== availableLength) {
                var sized = false;
                if(options.maxPane) {
                    var maxPane = state.panes[options.maxPane - 1];
                    var availableLengthWithoutBar = this._determineAvailableLengthWithoutBar(state, availableLength);
                    var maxPaneLimit = maxPane.splitterPane.getMaxPixels(availableLengthWithoutBar);
                    if(maxPaneLimit <= maxPane.length) {
                        var otherPane = state.panes[options.maxPane === 1 ? 1 : 0];
                        maxPane.length = maxPaneLimit;
                        otherPane.length = availableLengthWithoutBar - maxPaneLimit;
                        state.posn = pane0.length;
                        sized = true;
                    }
                }

                if(!sized) {
                    var pane0Proportion = pane0.length / oldAvailableLength;
                    pane0.length = oldAvailableLength === 0 ? 1 : Math.floor(0.5 + (availableLength * pane0Proportion));
                    this._moveSplitterToFitPaneLength(state, 0, availableLength);
                }
            }
        }

        /**
         * Modifies the size and positions of the panes to fit into the available length. Returns true if panes
         * had to be moved as a result.
         */
        private adjustToNewSize(state: Splitter_State) : boolean
        {
            if(state === undefined) state = this._getState();
            var options = this.options;

            var availableLength = this._determineAvailableLength(state);

            var lockedPane = state.userSetPosn ? options.savePane : 0;

            if(lockedPane) this._moveSplitterToFitPaneLength(state, lockedPane - 1, availableLength);
            else           this._moveSplitterToKeepPaneProportions(state, availableLength);

            return this._setPositions(state, availableLength);
        }

        /**
         * Updates the collapse button to reflect the collapsed state.
         */
        private _syncCollapseButtonState(state: Splitter_State)
        {
            if(!state) state = this._getState();
            var options = this.options;
            var button = state.collapseButton;
            var collapsed = !!state.collapsedPane;

            button.removeClass('left right up down');
            switch(options.collapsePane) {
                case 1:
                    if(options.vertical) button.addClass(collapsed ? 'right' : 'left');
                    else                 button.addClass(collapsed ? 'down' : 'up');
                    break;
                case 2:
                    if(options.vertical) button.addClass(collapsed ? 'left' : 'right');
                    else                 button.addClass(collapsed ? 'up' : 'down');
                    break;
            }
        }

        /**
         * Called when the user clicks on the splitter bar.
         */
        private _barMouseDown(event: JQueryEventObject) : boolean
        {
            return this._startMove(event, true, event.pageX, event.pageY, (state: Splitter_State) => {
                $(document)
                    .on('mousemove.' + state.eventNamespace, $.proxy(this._documentMouseMove, this))
                    .on('mouseup.' + state.eventNamespace, $.proxy(this._documentMouseUp, this));
            });
        }

        /**
         * Called when the user touches the splitter bar
         */
        private _touchStart(event: JQueryEventObject) : boolean
        {
            event.preventDefault();
            var touch = (<TouchEvent>event.originalEvent).touches[0];
            return this._startMove(event, false, touch.pageX, touch.pageY, (state: Splitter_State) => {
                state.bar.element
                    .on('touchmove.' + state.eventNamespace, $.proxy(this._touchMove, this))
                    .on('touchend.' + state.eventNamespace, $.proxy(this._touchEnd, this));
            });
        }

        /**
         * Does the work for the start move event handlers.
         */
        private _startMove(event: JQueryEventObject, testForLeftButton: boolean, pageX: number, pageY: number, hookMoveAndUp: (state: Splitter_State) => void) : boolean
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var isLeftButton = !testForLeftButton || event.which === 1;
            var result = !isLeftButton;
            if(!result) {
                var options = this.options;
                var state = this._getState();
                state.bar.element.addClass('moving');
                state.startMovePosn = state.posn - (options.vertical ? pageX : pageY);
                hookMoveAndUp.call(this, state);
                event.stopPropagation();
            }

            return result;
        }

        /**
         * Called when the user moves the mouse after having clicked on the splitter bar.
         */
        private _documentMouseMove(event: JQueryEventObject) : boolean
        {
            return this._continueMove(event, event.pageX, event.pageY);
        }

        /**
         * Called when the user moves the mouse after having touched on the splitter bar.
         */
        private _touchMove(event: JQueryEventObject) : boolean
        {
            var touch = (<TouchEvent>event.originalEvent).touches[0];
            return this._continueMove(event, touch.pageX, touch.pageY);
        }

        /**
         * Does the work for the move events.
         */
        private _continueMove(event: JQueryEventObject, pageX: number, pageY: number) : boolean
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var options = this.options;
            var state = this._getState();
            var availableLength = this._determineAvailableLength(state);

            state.posn = Math.max(0, Math.min(availableLength, state.startMovePosn + (options.vertical ? pageX : pageY)));
            this._sizePanesToSplitter(state, availableLength);
            state.userSetPosn = true;
            this._setPositions(state, availableLength);

            event.stopPropagation();
            return false;
        }

        /**
         * Called when the user releases the mouse after having clicked on the splitter bar.
         */
        private _documentMouseUp(event: JQueryEventObject) : boolean
        {
            return this._stopMove(event, (state: Splitter_State) => {
                $(document)
                    .off('mousemove.' + state.eventNamespace, this._documentMouseMove)
                    .off('mouseup.' + state.eventNamespace, this._documentMouseUp);
            });
        }

        /**
         * Called when the user lifts their finger off a splitter bar.
         */
        private _touchEnd(event: JQueryEventObject) : boolean
        {
            return this._stopMove(event, (state: Splitter_State) => {
                state.bar.element
                    .off('touchmove.' + state.eventNamespace, this._touchMove)
                    .off('touchend.' + state.eventNamespace, this._touchEnd);
            });
        }

        /**
         * Does the work for the stop move event handlers.
         */
        private _stopMove(event: JQueryEventObject, unhookMoveAndUp: (state: Splitter_State) => void) : boolean
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var state = this._getState();
            state.bar.element.removeClass('moving');
            unhookMoveAndUp.call(this, state);
            event.stopPropagation();

            return false;
        }

        /**
         * Called when the browser window is resized.
         */
        private _windowResized()
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var state = this._getState();
            this.adjustToNewSize(state);
        }

        /**
         * Called when the collapse button on the bar is clicked.
         */
        private _collapseClicked(event: Event)
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var state = this._getState();
            state.collapsedPane = state.collapsedPane ? 0 : this.options.collapsePane;
            this._syncCollapseButtonState(state);
            this._setPositions(state, this._determineAvailableLength(state));

            event.stopPropagation();
            return false;
        }
    }

    $.widget('vrs.vrsSplitter', new Splitter());
}

declare interface JQuery
{
    vrsSplitterPane();
    vrsSplitterPane(options: VRS.SplitterPane_Options);
    vrsSplitterPane(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);

    vrsSplitter();
    vrsSplitter(options: VRS.Splitter_Options);
    vrsSplitter(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
