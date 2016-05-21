var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.splitterBorderWidth = VRS.globalOptions.splitterBorderWidth !== undefined ? VRS.globalOptions.splitterBorderWidth : 1;
    var Splitter_State = (function () {
        function Splitter_State() {
            this.bar = null;
            this.panes = [];
            this.posn = -1;
            this.collapseButton = null;
            this.collapsedPane = 0;
            this.userSetPosn = false;
            this.startMovePosn = -1;
            this.previousPosn = -1;
            this.previousPane0Length = -1;
            this.previousPane1Length = -1;
            this.eventNamespace = '';
        }
        return Splitter_State;
    })();
    var SplitterPane_State = (function () {
        function SplitterPane_State() {
            this.container = null;
            this.splitterContent = null;
            this.originalContentCss = null;
        }
        return SplitterPane_State;
    })();
    var SplitterPaneDetail = (function () {
        function SplitterPaneDetail(element) {
            this.element = element;
            this.splitterPane = undefined;
            this.length = 0;
        }
        return SplitterPaneDetail;
    })();
    var SplitterGroupPersistence = (function () {
        function SplitterGroupPersistence(name) {
            this._SplitterDetails = {};
            this._AutoSaveEnabled = false;
            if (!name || name === '')
                throw 'You must supply the name that the group of splitters will be saved under';
            this._Name = name;
        }
        SplitterGroupPersistence.prototype.getAutoSaveEnabled = function () {
            return this._AutoSaveEnabled;
        };
        SplitterGroupPersistence.prototype.setAutoSaveEnabled = function (value) {
            this._AutoSaveEnabled = value;
        };
        SplitterGroupPersistence.prototype.dispose = function () {
            $.each(this._SplitterDetails, function () {
                var details = this;
                if (details.splitter && details.barMovedHookResult) {
                    details.splitter.unhook(details.barMovedHookResult);
                }
            });
            this._SplitterDetails = {};
        };
        SplitterGroupPersistence.prototype.saveState = function () {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        };
        SplitterGroupPersistence.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        };
        SplitterGroupPersistence.prototype.getSplitterSavedState = function (splitterName) {
            var result = null;
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), null);
            if (savedSettings) {
                $.each(savedSettings.lengths, function (idx, savedSplitter) {
                    if (savedSplitter.name === splitterName) {
                        result = savedSplitter;
                    }
                    return result === null;
                });
            }
            return result;
        };
        SplitterGroupPersistence.prototype.applyState = function (settings) {
            var autoSaveState = this.getAutoSaveEnabled();
            this.setAutoSaveEnabled(false);
            var length = settings.lengths ? settings.lengths.length : -1;
            for (var i = 0; i < length; ++i) {
                var details = settings.lengths[i];
                var splitterDetails = this._SplitterDetails[details.name];
                if (splitterDetails) {
                    splitterDetails.splitter.applySavedLength(details);
                }
            }
            this.setAutoSaveEnabled(autoSaveState);
        };
        SplitterGroupPersistence.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        SplitterGroupPersistence.prototype.persistenceKey = function () {
            return 'vrsSplitterPosition-' + this._Name;
        };
        SplitterGroupPersistence.prototype.createSettings = function () {
            var lengths = [];
            for (var splitterName in this._SplitterDetails) {
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
        };
        SplitterGroupPersistence.prototype.registerSplitter = function (splitterElement) {
            var splitter = VRS.jQueryUIHelper.getSplitterPlugin(splitterElement);
            var splitterName = splitter.getName();
            var existingSplitter = this._SplitterDetails[splitterName];
            if (existingSplitter)
                throw 'The ' + splitterName + ' splitter has already been registered';
            this._SplitterDetails[splitterName] = {
                splitter: splitter,
                barMovedHookResult: splitter.hookBarMoved(this.onBarMoved, this)
            };
            return this.getSplitterSavedState(splitterName);
        };
        SplitterGroupPersistence.prototype.onBarMoved = function (event, data) {
            var splitter = VRS.jQueryUIHelper.getSplitterPlugin(data.splitterElement);
            var splitterDetails = splitter ? this._SplitterDetails[splitter.getName()] : null;
            if (splitterDetails) {
                splitterDetails.pane1Length = data.pane1Length;
                splitterDetails.pane2Length = data.pane2Length;
                if (this.getAutoSaveEnabled()) {
                    this.saveState();
                }
            }
        };
        return SplitterGroupPersistence;
    })();
    VRS.SplitterGroupPersistence = SplitterGroupPersistence;
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getSplitterPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsSplitter');
    };
    VRS.jQueryUIHelper.getSplitterPanePlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsSplitterPane');
    };
    var SplitterPane = (function (_super) {
        __extends(SplitterPane, _super);
        function SplitterPane() {
            _super.apply(this, arguments);
            this.options = {
                isVertical: false,
                minPixels: 1,
            };
        }
        SplitterPane.prototype._getState = function () {
            var result = this.element.data('splitterPaneState');
            if (result === undefined) {
                result = new SplitterPane_State();
                this.element.data('splitterPaneState', result);
            }
            return result;
        };
        SplitterPane.prototype._create = function () {
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
                .insertBefore(this.element)
                .append(this.element);
            state.splitterContent = VRS.jQueryUIHelper.getSplitterPlugin(this.element);
            if (!state.splitterContent) {
                state.container
                    .css({
                    overflow: 'auto'
                })
                    .addClass('border');
            }
            if (VRS.refreshManager) {
                VRS.refreshManager.registerOwner(state.container);
            }
        };
        SplitterPane.prototype._destroy = function () {
            var state = this._getState();
            if (state.container) {
                if (VRS.refreshManager) {
                    VRS.refreshManager.unregisterOwner(state.container);
                }
                this.element.unwrap();
                state.container = null;
                if (state.originalContentCss) {
                    this.element.css(state.originalContentCss);
                }
                state.originalContentCss = null;
            }
        };
        SplitterPane.prototype.getContainer = function () {
            return this._getState().container;
        };
        SplitterPane.prototype.getContent = function () {
            return this.element;
        };
        SplitterPane.prototype.getContentIsSplitter = function () {
            return !!this._getState().splitterContent;
        };
        SplitterPane.prototype.getMinPixels = function () {
            return this.options.minPixels;
        };
        SplitterPane.prototype.getMaxPixels = function (availableLengthWithoutBar) {
            return this._getPixelsFromMaxOrStartSize(this.options.max, availableLengthWithoutBar);
        };
        SplitterPane.prototype.getStartSize = function (availableLengthWithoutBar) {
            return this._getPixelsFromMaxOrStartSize(this.options.startSize, availableLengthWithoutBar);
        };
        SplitterPane.prototype._getPixelsFromMaxOrStartSize = function (maxOrStartSize, availableLengthWithoutBar) {
            var result = null;
            if (maxOrStartSize) {
                if (maxOrStartSize instanceof Function) {
                    result = maxOrStartSize(availableLengthWithoutBar);
                }
                else {
                    var valuePercent = maxOrStartSize;
                    result = valuePercent.value;
                    if (valuePercent.isPercent) {
                        result = Math.max(1, Math.ceil(availableLengthWithoutBar * result));
                    }
                }
            }
            return result;
        };
        return SplitterPane;
    })(JQueryUICustomWidget);
    VRS.SplitterPane = SplitterPane;
    $.widget('vrs.vrsSplitterPane', new SplitterPane());
    var Splitter = (function (_super) {
        __extends(Splitter, _super);
        function Splitter() {
            _super.apply(this, arguments);
            this.options = {
                name: undefined,
                vertical: true,
                savePane: 1,
                collapsePane: undefined,
                minPixels: [10, 10],
                maxPane: 0,
                max: null,
                startSizePane: 0,
                startSize: null,
                splitterGroupPersistence: null,
                isTopLevelSplitter: false,
            };
        }
        Splitter.prototype._getState = function () {
            var result = this.element.data('splitterState');
            if (result === undefined) {
                result = new Splitter_State();
                this.element.data('splitterState', result);
            }
            return result;
        };
        Splitter.prototype._create = function () {
            var options = this.options;
            var state = this._getState();
            var i;
            if (!options.name)
                throw 'You must supply a name for the splitter';
            state.eventNamespace = 'vrsSplitter-' + options.name;
            options.max = this.convertMaxOrStartSize(options.maxPane, options.max, 'max');
            options.startSize = this.convertMaxOrStartSize(options.startSizePane, options.startSize, 'start size');
            if (options.minPixels.length !== 2)
                throw 'You must pass two integers for minPixels';
            if (!options.minPixels[0] || options.minPixels[0] < 1)
                options.minPixels[0] = 1;
            if (!options.minPixels[1] || options.minPixels[1] < 1)
                options.minPixels[1] = 1;
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
            if (children.length !== 2)
                throw 'A splitter control must have two children';
            for (i = 0; i < 2; ++i) {
                var pane = $(children[i]).vrsSplitterPane({
                    isVertical: options.vertical,
                    minPixels: options.minPixels[i],
                    max: options.maxPane === i + 1 ? options.max : undefined,
                    startSize: options.startSizePane === i + 1 ? options.startSize : undefined
                });
                var splitterPane = VRS.jQueryUIHelper.getSplitterPanePlugin(pane);
                var detail = new SplitterPaneDetail(splitterPane.getContainer());
                detail.splitterPane = splitterPane;
                state.panes.push(detail);
            }
            if (VRS.refreshManager)
                VRS.refreshManager.rebuildRelationships();
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
            }));
            state.bar.element.mousedown($.proxy(this._barMouseDown, this));
            state.bar.element.on('touchstart', $.proxy(this._touchStart, this));
            state.bar.length = options.vertical ? state.bar.element.outerWidth() : state.bar.element.outerHeight();
            if (options.collapsePane) {
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
            if (options.splitterGroupPersistence) {
                savedState = options.splitterGroupPersistence.registerSplitter(this.element);
            }
            var availableLength = this._determineAvailableLength(state);
            if (savedState) {
                this.applySavedLength(savedState);
            }
            else if (options.startSizePane) {
                var availableLengthWithoutBar = this._determineAvailableLengthWithoutBar(state, availableLength);
                var paneDetail = state.panes[options.startSizePane - 1];
                paneDetail.length = paneDetail.splitterPane.getStartSize(availableLengthWithoutBar);
                this._moveSplitterToFitPaneLength(state, options.startSizePane - 1, availableLength);
                this._setPositions(state, availableLength);
            }
            else {
                state.posn = Math.floor(availableLength / 2);
                this._sizePanesToSplitter(state, availableLength);
                this._setPositions(state, availableLength);
            }
            this._raiseBarMoved(state);
            if (VRS.refreshManager && state.panes.length >= 2) {
                VRS.refreshManager.refreshTargets(state.panes[0].element);
                VRS.refreshManager.refreshTargets(state.panes[1].element);
            }
            $(window).on('resize.' + state.eventNamespace, $.proxy(this._windowResized, this));
        };
        Splitter.prototype._destroy = function () {
            var options = this.options;
            var state = this._getState();
            if (state.panes && state.panes.length === 2) {
                for (var i = 0; i < state.panes.length; ++i) {
                    var details = state.panes[i];
                    var element = details.splitterPane.getContent();
                    var originalParent = i === 0 ? options.leftTopParent : options.rightBottomParent;
                    var elementIsSplitter = details.splitterPane.getContentIsSplitter();
                    details.splitterPane.destroy();
                    if (elementIsSplitter) {
                        element.vrsSplitter('destroy');
                    }
                    else if (originalParent) {
                        element.appendTo(originalParent);
                    }
                }
                state.panes = [];
            }
            if (state.bar !== null) {
                state.bar.element.off();
                state.bar.element.remove();
                state.bar = null;
            }
            $(window).off('resize.' + state.eventNamespace, this._windowResized);
            this.element.remove();
            if (VRS.refreshManager) {
                VRS.refreshManager.rebuildRelationships();
            }
        };
        Splitter.prototype.convertMaxOrStartSize = function (paneNumber, maxOrStartSize, description) {
            var result = maxOrStartSize;
            if (paneNumber) {
                if (!(maxOrStartSize instanceof Function) && maxOrStartSize.isPercent === undefined) {
                    var valuePercent = VRS.unitConverter.getPixelsOrPercent(maxOrStartSize);
                    if (valuePercent.isPercent && (valuePercent.value < 0.01 || valuePercent.value > 0.99))
                        throw description + ' percent must be between 1% and 99% inclusive';
                    result = valuePercent;
                }
            }
            return result;
        };
        Splitter.prototype.getName = function () {
            return this.options.name;
        };
        Splitter.prototype.getIsVertical = function () {
            return this.options.vertical;
        };
        Splitter.prototype.getSavePane = function () {
            return this.options.savePane;
        };
        Splitter.prototype.hookBarMoved = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsSplitter', 'barMoved', callback, forceThis);
        };
        Splitter.prototype._raiseBarMoved = function (state) {
            var panesCount = state.panes.length;
            this._trigger('barMoved', null, {
                splitterElement: this.element,
                pane1Length: panesCount > 0 ? state.panes[0].length : -1,
                pane2Length: panesCount > 1 ? state.panes[1].length : -1,
                barLength: state.bar ? state.bar.length : -1
            });
        };
        Splitter.prototype.unhook = function (hookResult) {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        };
        Splitter.prototype._determineAvailableLength = function (state) {
            var result = this.options.vertical ? this.element.width() : this.element.height();
            if (!state)
                state = this._getState();
            for (var i = 0; i < 2; ++i) {
                if (!state.panes[0].splitterPane.getContentIsSplitter())
                    result -= 2 * VRS.globalOptions.splitterBorderWidth;
            }
            return result;
        };
        Splitter.prototype._determineAvailableLengthWithoutBar = function (state, availableLength) {
            if (!state)
                state = this._getState();
            if (availableLength === undefined)
                availableLength = this._determineAvailableLength(state);
            return Math.max(0, availableLength - state.bar.length);
        };
        Splitter.prototype._sizePanesToSplitter = function (state, availableLength) {
            if (availableLength === undefined)
                availableLength = this._determineAvailableLength(state);
            var bar = state.bar;
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];
            pane0.length = state.posn;
            pane1.length = availableLength - (pane0.length + bar.length);
        };
        Splitter.prototype._applyMinMax = function (state, availableLength) {
            var _this = this;
            if (!state)
                state = this._getState();
            if (state.panes.length > 1) {
                if (availableLength === undefined)
                    availableLength = this._determineAvailableLength(state);
                var availableWithoutBar = this._determineAvailableLengthWithoutBar(state, availableLength);
                var moveSplitter = function (offsetPixels, subtract) {
                    offsetPixels = Math.max(0, offsetPixels);
                    if (offsetPixels) {
                        if (subtract)
                            offsetPixels = -offsetPixels;
                        state.posn += offsetPixels;
                        _this._sizePanesToSplitter(state, availableLength);
                    }
                };
                for (var i = 1; i >= 0; --i) {
                    var paneDetail = state.panes[i];
                    var minPixels = paneDetail.splitterPane.getMinPixels();
                    var maxPixels = paneDetail.splitterPane.getMaxPixels(availableWithoutBar);
                    if (maxPixels)
                        moveSplitter(paneDetail.length - maxPixels, i === 0);
                    if (minPixels)
                        moveSplitter(minPixels - paneDetail.length, i === 1);
                }
            }
        };
        Splitter.prototype.applySavedLength = function (savedState) {
            var options = this.options;
            var pane = savedState.pane;
            var vertical = savedState.vertical;
            var length = savedState.length;
            if (options.savePane === pane && options.vertical === vertical) {
                var state = this._getState();
                if (state.panes.length > 1) {
                    state.userSetPosn = true;
                    var paneIndex = pane - 1;
                    state.panes[paneIndex].length = length;
                    var availableLength = this._determineAvailableLength(state);
                    this._moveSplitterToFitPaneLength(state, paneIndex, availableLength);
                    this._setPositions(state, availableLength);
                }
            }
        };
        Splitter.prototype._setPositions = function (state, availableLength) {
            if (!state)
                state = this._getState();
            if (availableLength === undefined)
                availableLength = this._determineAvailableLength(state);
            var collapsed = !!state.collapsedPane;
            if (!collapsed)
                this._applyMinMax(state, availableLength);
            var options = this.options;
            var bar = state.bar;
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];
            var pane0Length = pane0.length;
            var pane1Length = pane1.length;
            var barLength = bar.length;
            var posn = state.posn;
            if (collapsed) {
                switch (state.collapsedPane) {
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
            if (result) {
                state.previousPosn = posn;
                state.previousPane0Length = pane0Length;
                state.previousPane1Length = pane1Length;
                var barOffset = pane0Length === 0 ? 0 : pane0.splitterPane.getContentIsSplitter() ? 0 : 2 * VRS.globalOptions.splitterBorderWidth;
                pane1Posn += barOffset;
                if (options.vertical) {
                    pane1.element.css({ left: pane1Posn, width: pane1Length });
                    bar.element.css({ left: posn + barOffset });
                    pane0.element.css({ width: pane0Length });
                }
                else {
                    pane1.element.css({ top: pane1Posn, height: pane1Length });
                    bar.element.css({ top: posn + barOffset });
                    pane0.element.css({ height: pane0Length });
                }
                if (VRS.refreshManager) {
                    if (pane0Length)
                        VRS.refreshManager.refreshTargets(pane0.element);
                    if (pane1Length)
                        VRS.refreshManager.refreshTargets(pane1.element);
                }
                this._raiseBarMoved(state);
            }
            return result;
        };
        Splitter.prototype._moveSplitterToFitPaneLength = function (state, fitPaneIndex, availableLength) {
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];
            var bar = state.bar;
            if (availableLength === undefined) {
                availableLength = this._determineAvailableLength(state);
            }
            switch (fitPaneIndex) {
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
        };
        Splitter.prototype._moveSplitterToKeepPaneProportions = function (state, availableLength) {
            var options = this.options;
            var pane0 = state.panes[0];
            var pane1 = state.panes[1];
            var bar = state.bar;
            if (availableLength === undefined)
                availableLength = this._determineAvailableLength(state);
            var oldAvailableLength = pane0.length + pane1.length + bar.length;
            if (oldAvailableLength !== availableLength) {
                var sized = false;
                if (options.maxPane) {
                    var maxPane = state.panes[options.maxPane - 1];
                    var availableLengthWithoutBar = this._determineAvailableLengthWithoutBar(state, availableLength);
                    var maxPaneLimit = maxPane.splitterPane.getMaxPixels(availableLengthWithoutBar);
                    if (maxPaneLimit <= maxPane.length) {
                        var otherPane = state.panes[options.maxPane === 1 ? 1 : 0];
                        maxPane.length = maxPaneLimit;
                        otherPane.length = availableLengthWithoutBar - maxPaneLimit;
                        state.posn = pane0.length;
                        sized = true;
                    }
                }
                if (!sized) {
                    var pane0Proportion = pane0.length / oldAvailableLength;
                    pane0.length = oldAvailableLength === 0 ? 1 : Math.floor(0.5 + (availableLength * pane0Proportion));
                    this._moveSplitterToFitPaneLength(state, 0, availableLength);
                }
            }
        };
        Splitter.prototype.adjustToNewSize = function (state) {
            if (state === undefined)
                state = this._getState();
            var options = this.options;
            var availableLength = this._determineAvailableLength(state);
            var lockedPane = state.userSetPosn ? options.savePane : 0;
            if (lockedPane)
                this._moveSplitterToFitPaneLength(state, lockedPane - 1, availableLength);
            else
                this._moveSplitterToKeepPaneProportions(state, availableLength);
            return this._setPositions(state, availableLength);
        };
        Splitter.prototype._syncCollapseButtonState = function (state) {
            if (!state)
                state = this._getState();
            var options = this.options;
            var button = state.collapseButton;
            var collapsed = !!state.collapsedPane;
            button.removeClass('left right up down');
            switch (options.collapsePane) {
                case 1:
                    if (options.vertical)
                        button.addClass(collapsed ? 'right' : 'left');
                    else
                        button.addClass(collapsed ? 'down' : 'up');
                    break;
                case 2:
                    if (options.vertical)
                        button.addClass(collapsed ? 'left' : 'right');
                    else
                        button.addClass(collapsed ? 'up' : 'down');
                    break;
            }
        };
        Splitter.prototype._barMouseDown = function (event) {
            var _this = this;
            return this._startMove(event, true, event.pageX, event.pageY, function (state) {
                $(document)
                    .on('mousemove.' + state.eventNamespace, $.proxy(_this._documentMouseMove, _this))
                    .on('mouseup.' + state.eventNamespace, $.proxy(_this._documentMouseUp, _this));
            });
        };
        Splitter.prototype._touchStart = function (event) {
            var _this = this;
            event.preventDefault();
            var touch = event.originalEvent.touches[0];
            return this._startMove(event, false, touch.pageX, touch.pageY, function (state) {
                state.bar.element
                    .on('touchmove.' + state.eventNamespace, $.proxy(_this._touchMove, _this))
                    .on('touchend.' + state.eventNamespace, $.proxy(_this._touchEnd, _this));
            });
        };
        Splitter.prototype._startMove = function (event, testForLeftButton, pageX, pageY, hookMoveAndUp) {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var isLeftButton = !testForLeftButton || event.which === 1;
            var result = !isLeftButton;
            if (!result) {
                var options = this.options;
                var state = this._getState();
                state.bar.element.addClass('moving');
                state.startMovePosn = state.posn - (options.vertical ? pageX : pageY);
                hookMoveAndUp.call(this, state);
                event.stopPropagation();
            }
            return result;
        };
        Splitter.prototype._documentMouseMove = function (event) {
            return this._continueMove(event, event.pageX, event.pageY);
        };
        Splitter.prototype._touchMove = function (event) {
            var touch = event.originalEvent.touches[0];
            return this._continueMove(event, touch.pageX, touch.pageY);
        };
        Splitter.prototype._continueMove = function (event, pageX, pageY) {
            if (VRS.timeoutManager) {
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
        };
        Splitter.prototype._documentMouseUp = function (event) {
            var _this = this;
            return this._stopMove(event, function (state) {
                $(document)
                    .off('mousemove.' + state.eventNamespace, _this._documentMouseMove)
                    .off('mouseup.' + state.eventNamespace, _this._documentMouseUp);
            });
        };
        Splitter.prototype._touchEnd = function (event) {
            var _this = this;
            return this._stopMove(event, function (state) {
                state.bar.element
                    .off('touchmove.' + state.eventNamespace, _this._touchMove)
                    .off('touchend.' + state.eventNamespace, _this._touchEnd);
            });
        };
        Splitter.prototype._stopMove = function (event, unhookMoveAndUp) {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            state.bar.element.removeClass('moving');
            unhookMoveAndUp.call(this, state);
            event.stopPropagation();
            return false;
        };
        Splitter.prototype._windowResized = function () {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            this.adjustToNewSize(state);
        };
        Splitter.prototype._collapseClicked = function (event) {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            state.collapsedPane = state.collapsedPane ? 0 : this.options.collapsePane;
            this._syncCollapseButtonState(state);
            this._setPositions(state, this._determineAvailableLength(state));
            event.stopPropagation();
            return false;
        };
        return Splitter;
    })(JQueryUICustomWidget);
    VRS.Splitter = Splitter;
    $.widget('vrs.vrsSplitter', new Splitter());
})(VRS || (VRS = {}));
