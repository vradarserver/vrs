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

(function(VRS, $, /** object= */ undefined)
{
    //region Global Options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.splitterBorderWidth = VRS.globalOptions.splitterBorderWidth !== undefined ? VRS.globalOptions.splitterBorderWidth : 1;    // The width in pixels of the borders around splitters.
    //endregion

    //region SplitterState
    /**
     * Carries the state of a VRS splitter.
     * @constructor
     */
    VRS.SplitterState = function()
    {
        /**
         * A single instance of a SplitterPaneDetail.
         * @type {VRS.SplitterPaneDetail}
         */
        this.bar = null;

        /**
         * An array of 2 SplitterPaneDetail objects.
         * @type {Array.<VRS.SplitterPaneDetail>}
         */
        this.panes = [];

        /**
         * The pixel offset of the left/top edge of the splitter bar.
         * @type {number}
         */
        this.posn = -1;

        /**
         * The collapse button jQuery element.
         * @type {jQuery}
         */
        this.collapseButton = null;

        /**
         * 1 if the first pane has been collapsed, 2 if the second pane has been collapsed.
         * @type {number}
         */
        this.collapsedPane = 0;

        /**
         * True if the position has been set by the user.
         * @type {boolean}
         */
        this.userSetPosn = false;

        /**
         * The pixel position from which the splitter is dragged.
         * @type {number}
         */
        this.startMovePosn = -1;

        /**
         * The previous pixel position of the splitter bar.
         * @type {number}
         */
        this.previousPosn = -1;

        /**
         * The previous pixel length of pane 0.
         * @type {number}
         */
        this.previousPane0Length = -1;

        /**
         * The previous pixel length of pane 1.
         * @type {number}
         */
        this.previousPane1Length = -1;

        /**
         * The namespace that all DOM events that use $.proxy are suffixed with to ensure that unbinds only unbind
         * our splitter and nothing else.
         * @type {string}
         */
        this.eventNamespace = '';
    };
    //endregion

    //region SplitterPaneState
    /**
     * Carries the state of a single pane within a VRS splitter.
     * @constructor
     */
    VRS.SplitterPaneState = function()
    {
        /**
         * The container element that wraps the content in a splitter pane.
         * @type {jQuery=}
         */
        this.container = null;

        /**
         * The direct-access Splitter jQuery UI object - only set if the content is itself a splitter.
         * @type {jQuery=}
         */
        this.splitterContent = null;

        /**
         * The CSS that needs to be applied to the content to bring it back to the state it was in before it was placed in a pane.
         * @type {object}
         */
        this.originalContentCss = null;
    };
    //endregion

    //region SplitterPaneDetail
    /**
     * Groups together things that a splitter knows about one of its panes, or about its bar.
     * @param {jQuery} element
     * @constructor
     */
    VRS.SplitterPaneDetail = function(element)
    {
        /**
         * The jQuery element for the pane.
         * @type {jQuery}
         */
        this.element = element;

        /**
         * The direct-access object to the splitterPane plugin for the pane. Note that the detail for the bar doesn't have one of these.
         * @type {VRS.vrsSplitterPane=}
         */
        this.splitterPane = undefined;

        /**
         * The length, in pixels, of the pane in the relevant dimension (width for vertical splitters, height for horizontal splitters)
         * @type {number}
         */
        this.length = 0;
    };
    //endregion

    //region SplitterGroupPersistence
    /**
     * The object responsible for saving and loading the state of a group of nested splitters.
     * @param {string} name
     * @constructor
     */
    VRS.SplitterGroupPersistence = function(name)
    {
        if(!name) throw 'You must supply the name that the group of splitters will be saved under';

        //region -- Fields
        var that = this;

        /**
         * An associative array of splitter details indexed by splitter name.
         * @type {Object.<string, VRS_SPLITTER_PERSIST_DETAIL>}
         * @private
         */
        var _SplitterDetails = {};
        //endregion

        //region -- Properties
        var _AutoSaveEnabled = false;
        this.getAutoSaveEnabled = function() { return _AutoSaveEnabled; };
        this.setAutoSaveEnabled = function(/** bool */ value) { _AutoSaveEnabled = value; };
        //endregion

        //region -- dispose
        /**
         * Releases the resources attached to the splitter group persistence object.
         */
        this.dispose = function()
        {
            $.each(_SplitterDetails, function() {
                var details = this;
                if(details.splitter && details.barMovedHookResult) details.splitter.unhook(details.barMovedHookResult);
            });

            _SplitterDetails = {};
        };
        //endregion

        //region -- saveState, loadState, getSplitterSavedState, applyState, loadAndApplyState
        /**
         * Saves the current state of the group of splitters.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Returns the saved state for the group of splitters or the current state if no state has been saved.
         * @returns {VRS_STATE_SPLITTER_GROUP}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            return $.extend(createSettings(), savedSettings);
        };

        /**
         * Returns the saved state for a single splitter within the group.
         * @param {string} splitterName
         * @returns {VRS_STATE_SPLITTER}
         */
        this.getSplitterSavedState = function(splitterName)
        {
            var result = null;

            var savedSettings = VRS.configStorage.load(persistenceKey(), null);
            if(savedSettings) {
                $.each(savedSettings.lengths, function(idx, savedSplitter) {
                    if(savedSplitter.name === splitterName) result = savedSplitter;
                    return result === null;
                });
            }

            return result;
        };

        /**
         * Applies the saved state to the splitters held by the object.
         * @param {VRS_STATE_SPLITTER_GROUP} settings
         */
        this.applyState = function(settings)
        {
            var autoSaveState = this.getAutoSaveEnabled();
            this.setAutoSaveEnabled(false);

            var length = settings.lengths ? settings.lengths.length : -1;
            for(var i = 0;i < length;++i) {
                var details = settings.lengths[i];
                var splitter = _SplitterDetails[details.name];
                if(splitter) splitter.applySavedLength(details);
            }

            this.setAutoSaveEnabled(autoSaveState);
        };

        /**
         * Loads and applies the saved state.
         */
        this.loadAndApplyState = function()
        {
            this.applyState(this.loadState());
        };

        /**
         * Returns the key to save the splitter positions against.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsSplitterPosition-' + name;
        }

        /**
         * Returns the current state.
         * @returns {VRS_STATE_SPLITTER_GROUP}
         */
        function createSettings()
        {
            var lengths = [];
            for(var splitterName in _SplitterDetails) {
                //noinspection JSUnfilteredForInLoop
                var splitterDetails = _SplitterDetails[splitterName];
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
        //endregion

        //region -- registerSplitter
        /**
         * Adds a splitter to the group of splitters whose positions will be saved.
         * @param {jQuery} splitterElement
         * @returns {VRS_STATE_SPLITTER}
         */
        this.registerSplitter = function(splitterElement)
        {
            var splitter = VRS.jQueryUIHelper.getSplitterPlugin(splitterElement);
            var splitterName = splitter.getName();
            var existingSplitter = _SplitterDetails[splitterName];
            if(existingSplitter) throw 'The ' + splitterName + ' splitter has already been registered';

            _SplitterDetails[splitterName] = {
                splitter: splitter,
                barMovedHookResult: splitter.hookBarMoved(onBarMoved, this)
            };

            return this.getSplitterSavedState(splitterName);
        };
        //endregion

        //region -- events consumed - onBarMoved
        //noinspection JSUnusedLocalSymbols
        /**
         * Called when the user moves a splitter that this object is saving state for.
         * @param event
         * @param data
         */
        function onBarMoved(event, data)
        {
            var splitter = VRS.jQueryUIHelper.getSplitterPlugin(data.splitterElement);
            var splitterDetails = splitter ? _SplitterDetails[splitter.getName()] : null;
            if(splitterDetails) {
                splitterDetails.pane1Length = data.pane1Length;
                splitterDetails.pane2Length = data.pane2Length;

                if(that.getAutoSaveEnabled()) that.saveState();
            }
        }
        //endregion
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsSplitter attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsSplitter}
     */
    VRS.jQueryUIHelper.getSplitterPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsSplitter'); };

    /**
     * Returns the VRS.vrsSplitterPane attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsSplitterPane}
     */
    VRS.jQueryUIHelper.getSplitterPanePlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsSplitterPane'); };
    //endregion

    //region vrsSplitterPane
    /**
     * A jQuery UI widget that encapsulates a single pane within a splitter.
     * @namespace VRS.vrsSplitterPane
     */
    $.widget('vrs.vrsSplitterPane', {
        //region -- options
        options: {
            /** @type {bool} */                                         isVertical: false,      // True if the parent splitter is vertical
            /** @type {number} */                                       minPixels: 1,           // The smallest allowable number of pixels
            /** @type {(VRS_VALUE_PERCENT|function(number):number)=} */ max: undefined,         // Either a pixel value (can be a %age) or a method that takes the width of the splitter without the bar and returns a number of pixels.
            /** @type {(VRS_VALUE_PERCENT|function(number):number)=} */ startSize: undefined,   // Either a pixel value (can be a %age) or a method that takes the width of the splitter without the bar and returns a number of pixels.

            __nop: null
        },
        //endregion

        //region -- Property methods
        /**
         * Returns the pane's container object. This object is at the top level of elements, it is the one that the
         * splitter controls.
         * @returns {jQuery=}
         */
        getContainer: function() { return this._getState().container; },

        /**
         * Returns the pane's content element. This object is below the container, it's the UI that the user sees within
         * the splitter.
         * @returns {jQuery}
         */
        getContent: function() { return this.element; },

        /**
         * Returns true if the content of the pane is another splitter.
         * @returns {boolean}
         */
        getContentIsSplitter: function() { return !!this._getState().splitterContent; },
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object held by the pane plugin, creating it if it doesn't already exist.
         * @returns {VRS.SplitterPaneState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('splitterPaneState');
            if(result === undefined) {
                result = new VRS.SplitterPaneState();
                this.element.data('splitterPaneState', result);
            }

            return result;
        },

        /**
         * Creates the splitter pane.
         * @private
         */
        _create: function()
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

            if(VRS.refreshManager) VRS.refreshManager.registerOwner(state.container);
        },

        /**
         * Releases the resources allocated to the splitter pane.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            if(state.container) {
                if(VRS.refreshManager) VRS.refreshManager.unregisterOwner(state.container);

                this.element.unwrap();
                state.container = null;

                if(state.originalContentCss) this.element.css(state.originalContentCss);
                state.originalContentCss = null;
            }
        },
        //endregion

        //region -- getMinPixels, getMaxPixels, getStartSize
        /**
         * Returns the minimum number of pixels that this pane can be sized to.
         * @returns {number}
         */
        getMinPixels: function()
        {
            return this.options.minPixels;
        },

        /**
         * Returns the maximum number of pixels that this pane can be sized to.
         * @param {number} availableLengthWithoutBar    The pixel width of the parent splitter after the splitter bar has been removed.
         * @returns {number=}
         */
        getMaxPixels: function(availableLengthWithoutBar)
        {
            return this._getPixelsFromMaxOrStartSize(this.options.max, availableLengthWithoutBar);
        },

        /**
         * Returns the initial size of the splitter in pixels.
         * @param {number} availableLengthWithoutBar    The pixel width of the parent splitter after the splitter bar has been removed.
         * @returns {number=}
         */
        getStartSize: function(availableLengthWithoutBar)
        {
            return this._getPixelsFromMaxOrStartSize(this.options.startSize, availableLengthWithoutBar);
        },

        /**
         * Returns either the maxmimum number of pixels allowed or the starting size
         * @param {(VRS_VALUE_PERCENT|function(number):number)=}    maxOrStartSize              The max or startSize value to resolve.
         * @param {number}                                          availableLengthWithoutBar   The width of the parent splitter after the bar has been removed.
         * @returns {number=}                                                                   The resolved pixel width or null if there is no maximum / start width.
         * @private
         */
        _getPixelsFromMaxOrStartSize: function(maxOrStartSize, availableLengthWithoutBar)
        {
            var result = null;

            if(maxOrStartSize) {
                if(maxOrStartSize instanceof Function) result = maxOrStartSize(availableLengthWithoutBar);
                else {
                    //noinspection UnnecessaryLocalVariableJS
                    /** @type {VRS_VALUE_PERCENT} */ var valuePercent = maxOrStartSize;
                    result = valuePercent.value;
                    if(valuePercent.isPercent) result = Math.max(1, Math.ceil(availableLengthWithoutBar * result));
                }
            }

            return result;
        },
        //endregion
        __nop: null
    });
    //endregion

    //region vrsSplitter
    /**
     * A jQuery UI widget that places a splitter bar between two elements to control the size of those elements.
     * @namespace VRS.vrsSplitter
     */
    $.widget('vrs.vrsSplitter', {
        //region -- options
        options: {
            /** @type {string} */                                   name:                       undefined,          // The mandatory name for the splitter.
            /** @type {bool} */                                     vertical:                   true,               // True if the splitter is vertical, false if it is horizontal.
            /** @type {number} */                                   savePane:                   1,                  // 1 if the first pane's location should be preserved across page refreshes, 2 if the 2nd pane's location should be preserved. Not used with fixed splitters.
            /** @type {number=} */                                  collapsePane:               undefined,          // 1 if the first pane should be collapsible, 2 if the second is collapsible.
            /** @type {Array.<number>} */                           minPixels:                  [ 10, 10 ],         // The minimum number of pixels for each pane. Use undefined / null if a particular pane has no minimum (in which case a minimum of 1 is applied).
            /** @type {number} */                                   maxPane:                    0,                  // 1 if the max value applies to the first pane, 2 if it applies to the second pane.
            /** @type {number|string|function(number):number} */    max:                        null,               // Either an integer number of pixels or a string ending in % for a percentage. Can also be a function that is passed the available length and returns the number of pixels.
            /** @type {number} */                                   startSizePane:              0,                  // 1 if the start size applies to the first pane, 2 if it applies to the second pane.
            /** @type {number|string|function(number):number} */    startSize:                  null,               // An integer pixels, %age of available length or function returning number of pixels when passed the available length.
            /** @type {VRS.SplitterGroupPersistence} */             splitterGroupPersistence:   null,               // The persistence object that this splitter will have its bar positions saved through
            /** @type {bool} */                                     isTopLevelSplitter:         false,              // True if the splitter is not nested within another splitter.

            __nop: null
        },
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state attached to the splitter, creating it if necessary.
         * @returns {VRS.SplitterState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('splitterState');
            if(result === undefined) {
                result = new VRS.SplitterState();
                this.element.data('splitterState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the splitter.
         * @private
         */
        _create: function()
        {
            var options = this.options;
            var state = this._getState();
            var i;

            if(!options.name) throw 'You must supply a name for the splitter';
            state.eventNamespace = 'vrsSplitter-' + options.name;

            /**
             * Converts a string / number / function into either a VRS_VALUE_PERCENT or a function that takes a width and returns a number of pixels.
             * @param {number}                                  paneNumber      The number of the pane being converted.
             * @param {string|number|function(number):number}   maxOrStartSize  The max or startSize from the pane.
             * @param {string}                                  description     A description of the value and pane being converted (used in exception descriptions).
             * @returns {(VRS_VALUE_PERCENT|function(number):number)=}
             */
            var convertMaxOrStartSize = function(paneNumber, maxOrStartSize, description) {
                /** @type {(VRS_VALUE_PERCENT|function(number):number)=} */
                var innerResult = maxOrStartSize;
                if(paneNumber) {
                    if(!(maxOrStartSize instanceof Function)) {
                        var valuePercent = VRS.unitConverter.getPixelsOrPercent(maxOrStartSize);
                        if(valuePercent.isPercent && (valuePercent.value < 0.01 || valuePercent.value > 0.99)) throw description + ' percent must be between 1% and 99% inclusive';
                        innerResult = valuePercent;
                    }
                }
                return innerResult;
            };
            options.max = convertMaxOrStartSize(options.maxPane, options.max, 'max');
            options.startSize = convertMaxOrStartSize(options.startSizePane, options.startSize, 'start size');
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
                var pane = $(children[i]).vrsSplitterPane({
                    isVertical: options.vertical,
                    minPixels: options.minPixels[i],
                    max: options.maxPane === i + 1 ? options.max : undefined,
                    startSize: options.startSizePane === i + 1 ? options.startSize : undefined
                });
                var splitterPane = VRS.jQueryUIHelper.getSplitterPanePlugin(pane);
                var detail = new VRS.SplitterPaneDetail(splitterPane.getContainer());
                detail.splitterPane = splitterPane;
                state.panes.push(detail);
            }

            if(VRS.refreshManager) VRS.refreshManager.rebuildRelationships();

            state.bar = new VRS.SplitterPaneDetail($('<div/>')
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
        },

        /**
         * Releases any resources held by the splitter.
         * @private
         */
        _destroy: function()
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

                    if(elementIsSplitter) element.vrsSplitter('destroy');
                    else if(originalParent) element.appendTo(originalParent);
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

            if(VRS.refreshManager) VRS.refreshManager.rebuildRelationships();
        },
        //endregion

        //region -- properties
        /**
         * Gets the name of the splitter.
         * @returns {string}
         */
        getName:            function() { return this.options.name; },

        /**
         * Gets a value indicating whether the bar is vertical or horizontal.
         * @returns {boolean}
         */
        getIsVertical:      function() { return this.options.vertical; },

        /**
         * Gets the number of the pane whose dimensions are to be saved. Only one pane is ever saved.
         * @returns {number}
         */
        getSavePane:        function() { return this.options.savePane; },
        //endregion

        //region -- events exposed
        /**
         * Raised when the splitter bar is moved.
         * @param {function(VRS_SPLITTER_BAR_MOVED_ARGS)}   callback
         * @param {object}                                  forceThis
         * @returns {object}
         */
        hookBarMoved: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsSplitter', 'barMoved', callback, forceThis); },
        _raiseBarMoved: function(state)
        {
            var panesCount = state.panes.length;
            this._trigger('barMoved', null, {
                splitterElement: this.element,
                pane1Length: panesCount > 0 ? state.panes[0].length : -1,
                pane2Length: panesCount > 1 ? state.panes[1].length : -1,
                barLength: state.bar ? state.bar.length : -1
            });
        },

        /**
         * Unhooks an event hooked on the object.
         * @param hookResult
         */
        unhook: function(hookResult)
        {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        },
        //endregion

        //region -- Dimension handling: _determineAvailableLength, _sizePanesToSplitter, _applyMinMax
        /**
         * Returns the current length available to the splitter, within which the panes and bar have to fit.
         * @param {VRS.SplitterState} [state]
         * @returns {Number} The length of the splitter.
         * @private
         */
        _determineAvailableLength: function(state)
        {
            var result = this.options.vertical ? this.element.width() : this.element.height();

            // Each pane has a border applied to it unless that pane contains a splitter. We need to knock the widths
            // of the border off the available length.
            if(!state) state = this._getState();
            for(var i = 0;i < 2;++i) {
                if(!state.panes[0].splitterPane.getContentIsSplitter()) result -= 2 * VRS.globalOptions.splitterBorderWidth;
            }

            return result;
        },

        /**
         * Returns the length available to the panes, i.e. the overall length minus the bar length.
         * @param {VRS.SplitterState}   [state]
         * @param {number}              [availableLength]
         * @returns {number}
         * @private
         */
        _determineAvailableLengthWithoutBar: function(state, availableLength)
        {
            if(!state) state = this._getState();
            if(availableLength === undefined) availableLength = this._determineAvailableLength(state);

            return Math.max(0, availableLength - state.bar.length);
        },

        /**
         * Changes the size of both panes to make them fit flush with the current position of the splitter bar.
         * @param {VRS.SplitterState}    state
         * @param {number}              [availableLength]
         * @private
         */
        _sizePanesToSplitter: function(state, availableLength)
        {
            if(availableLength === undefined) availableLength = this._determineAvailableLength(state);
            var bar = state.bar;
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];

            pane0.length = state.posn;
            pane1.length = availableLength - (pane0.length + bar.length);
        },

        /**
         * Constrains the lengths of the panes to their minimums and maximums.
         * @param {VRS.SplitterState}   [state]
         * @param {number}              [availableLength]
         * @private
         */
        _applyMinMax: function(state, availableLength)
        {
            if(!state) state = this._getState();
            if(state.panes.length > 1) {
                if(availableLength === undefined) availableLength = this._determineAvailableLength(state);
                var availableWithoutBar = this._determineAvailableLengthWithoutBar(state, availableLength);

                var self = this;
                var moveSplitter = function(offsetPixels, subtract) {
                    offsetPixels = Math.max(0, offsetPixels);
                    if(offsetPixels) {
                        if(subtract) offsetPixels = -offsetPixels;
                        state.posn += offsetPixels;
                        self._sizePanesToSplitter(state, availableLength);
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
        },
        //endregion

        //region -- Location handling: applySavedLength, _setPositions, _moveSplitterToFitPaneLength, _moveSplitterToKeepPaneProportions, adjustToNewSize
        /**
         * Sizes the panes to match the saved state passed across.
         * @param {VRS_STATE_SPLITTER} savedState
         */
        applySavedLength: function(savedState)
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
        },

        /**
         * Updates the CSS on the panes and bar to move them into position on screen and raises events to let observers
         * know that the panes have been resized or moved.
         * @param {VRS.SplitterState}   [state]
         * @param {number}              [availableLength]
         * @returns {boolean}                                   True if the panes had to be resized, false if there was no work to do.
         * @private
         */
        _setPositions: function(state, availableLength)
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
        },

        /**
         * Assuming that one pane has the correct length this method adjusts the length of the other pane and the position
         * of the splitter bar to fill all available room.
         * @param {VRS.SplitterState}    state              The splitter's state.
         * @param {number}               fitPaneIndex       The pane that has the correct width (0 or 1).
         * @param {number}              [availableLength]   The available width including the bar's width.
         * @private
         */
        _moveSplitterToFitPaneLength: function(state, fitPaneIndex, availableLength)
        {
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];
            var bar = state.bar;
            if(availableLength === undefined) availableLength = this._determineAvailableLength(state);

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
        },

        /**
         * Assuming that the elements are in the correct position but the available room has changed, this changes the
         * lengths of the panes and the splitter position to try to keep both panes occupying the same proportion of
         * the new available length.
         * @param {VRS.SplitterState}    state              The splitter's state.
         * @param {number}              [availableLength]   The available width including the bar's width.
         * @private
         */
        _moveSplitterToKeepPaneProportions: function(state, availableLength)
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
        },

        /**
         * Modifies the size and positions of the panes to fit into the available length.
         * @param {VRS.SplitterState} [state]
         * @returns {boolean}                   True if the panes had to be moved to adjust to new size, false if they did not.
         */
        adjustToNewSize: function(state)
        {
            if(state === undefined) state = this._getState();
            var options = this.options;

            var availableLength = this._determineAvailableLength(state);

            var lockedPane = state.userSetPosn ? options.savePane : 0;

            if(lockedPane) this._moveSplitterToFitPaneLength(state, lockedPane - 1, availableLength);
            else           this._moveSplitterToKeepPaneProportions(state, availableLength);

            return this._setPositions(state, availableLength);
        },
        //endregion

        //region -- Collapse state handling - _syncCollapseButtonState
        /**
         * Updates the collapse button to reflect the collapsed state.
         * @param {VRS.SplitterState} [state]
         * @private
         */
        _syncCollapseButtonState: function(state)
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
        },
        //endregion

        //region -- Event handling
        /**
         * Called when the user clicks on the splitter bar.
         * @param {Event} event
         * @returns {boolean}
         * @private
         */
        _barMouseDown: function(event)
        {
            return this._startMove(event, true, event.pageX, event.pageY, function(/** VRS.SplitterState */ state) {
                $(document)
                    .on('mousemove.' + state.eventNamespace, $.proxy(this._documentMouseMove, this))
                    .on('mouseup.' + state.eventNamespace, $.proxy(this._documentMouseUp, this));
            });
        },

        /**
         * Called when the user touches the splitter bar
         * @param event
         * @returns {boolean}
         * @private
         */
        _touchStart: function(event)
        {
            event.preventDefault();
            var touch = event.originalEvent.touches[0];
            return this._startMove(event, false, touch.pageX, touch.pageY, function(/** VRS.SplitterState */ state) {
                state.bar.element
                    .on('touchmove.' + state.eventNamespace, $.proxy(this._touchMove, this))
                    .on('touchend.' + state.eventNamespace, $.proxy(this._touchEnd, this));
            });
        },

        /**
         * Does the work for the start move event handlers.
         * @param {Event}                       event
         * @param {boolean}                     testForLeftButton
         * @param {Number}                      pageX
         * @param {Number}                      pageY
         * @param {function(VRS.SplitterState)} hookMoveAndUp
         * @returns {boolean}
         * @private
         */
        _startMove: function(event, testForLeftButton, pageX, pageY, hookMoveAndUp)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

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
        },

        /**
         * Called when the user moves the mouse after having clicked on the splitter bar.
         * @param {Event} event
         * @returns {boolean}
         * @private
         */
        _documentMouseMove: function(event)
        {
            return this._continueMove(event, event.pageX, event.pageY);
        },

        /**
         * Called when the user moves the mouse after having touched on the splitter bar.
         * @param event
         * @returns {*}
         * @private
         */
        _touchMove: function(event)
        {
            var touch = event.originalEvent.touches[0];
            return this._continueMove(event, touch.pageX, touch.pageY);
        },

        /**
         * Does the work for the move events.
         * @param {Event}       event
         * @param {Number}      pageX
         * @param {Number}      pageY
         * @returns {boolean}
         * @private
         */
        _continueMove: function(event, pageX, pageY)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var options = this.options;
            var state = this._getState();
            var availableLength = this._determineAvailableLength(state);

            state.posn = Math.max(0, Math.min(availableLength, state.startMovePosn + (options.vertical ? pageX : pageY)));
            this._sizePanesToSplitter(state, availableLength);
            state.userSetPosn = true;
            this._setPositions(state, availableLength);

            event.stopPropagation();
            return false;
        },

        /**
         * Called when the user releases the mouse after having clicked on the splitter bar.
         * @param {Event} event
         * @returns {boolean}
         * @private
         */
        _documentMouseUp: function(event)
        {
            return this._stopMove(event, function(/** VRS.SplitterState */ state) {
                $(document)
                    .off('mousemove.' + state.eventNamespace, this._documentMouseMove)
                    .off('mouseup.' + state.eventNamespace, this._documentMouseUp);
            });
        },

        /**
         * Called when the user lifts their finger off a splitter bar.
         * @param {Event} event
         * @returns {boolean}
         * @private
         */
        _touchEnd: function(event)
        {
            return this._stopMove(event, function(/** VRS.SplitterState */ state) {
                state.bar.element
                    .off('touchmove.' + state.eventNamespace, this._touchMove)
                    .off('touchend.' + state.eventNamespace, this._touchEnd);
            });
        },

        /**
         * Does the work for the stop move event handlers.
         * @param {Event}                       event
         * @param {function(VRS.SplitterState)} unhookMoveAndUp
         * @returns {boolean}
         * @private
         */
        _stopMove: function(event, unhookMoveAndUp)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var state = this._getState();
            state.bar.element.removeClass('moving');
            unhookMoveAndUp.call(this, state);
            event.stopPropagation();

            return false;
        },

        /**
         * Called when the browser window is resized.
         * @private
         */
        _windowResized: function()
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var state = this._getState();
            this.adjustToNewSize(state);
        },

        /**
         * Called when the collapse button on the bar is clicked.
         * @param {Event} event
         * @private
         */
        _collapseClicked: function(event)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var state = this._getState();
            state.collapsedPane = state.collapsedPane ? 0 : this.options.collapsePane;
            this._syncCollapseButtonState(state);
            this._setPositions(state, this._determineAvailableLength(state));

            event.stopPropagation();
            return false;
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
