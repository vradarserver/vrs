var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldPaneListPlugin_State = (function () {
        function OptionFieldPaneListPlugin_State() {
            this.paneContainers = [];
            this.addPaneContainer = null;
        }
        return OptionFieldPaneListPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldPaneListPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldPaneList');
    };
    VRS.jQueryUIHelper.getOptionFieldPaneListOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldPaneListPlugin = (function (_super) {
        __extends(OptionFieldPaneListPlugin, _super);
        function OptionFieldPaneListPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldPaneListOptions();
            return _this;
        }
        OptionFieldPaneListPlugin.prototype._getState = function () {
            var result = this.element.data('optionFieldPanelListState');
            if (result === undefined) {
                result = new OptionFieldPaneListPlugin_State();
                this.element.data('optionFieldPanelListState', result);
            }
            return result;
        };
        OptionFieldPaneListPlugin.prototype._create = function () {
            var state = this._getState();
            var field = this.options.field;
            var panes = field.getPanes();
            state.paneAddedHookResult = field.hookPaneAdded(this._paneAdded, this);
            state.paneRemovedHookResult = field.hookPaneRemoved(this._paneRemoved, this);
            state.maxPanesChangedHookResult = field.hookMaxPanesChanged(this._maxPanesChanged, this);
            state.container = this.element;
            state.panesParent =
                $('<div/>')
                    .appendTo(state.container);
            var length = panes.length;
            for (var i = 0; i < length; ++i) {
                var pane = panes[i];
                this._addPane(pane, state);
            }
            var addPanel = field.getAddPane();
            if (addPanel) {
                state.addParent =
                    $('<div/>')
                        .appendTo(state.container);
                state.addPaneContainer = this._addOptionsPane(addPanel, state.addParent);
            }
            this._refreshControlStates();
        };
        OptionFieldPaneListPlugin.prototype._destroy = function () {
            var state = this._getState();
            var field = this.options.field;
            if (state.paneAddedHookResult) {
                field.unhook(state.paneAddedHookResult);
                state.paneAddedHookResult = undefined;
            }
            if (state.paneRemovedHookResult) {
                field.unhook(state.paneRemovedHookResult);
                state.paneRemovedHookResult = undefined;
            }
            if (state.maxPanesChangedHookResult) {
                field.unhook(state.maxPanesChangedHookResult);
                state.maxPanesChangedHookResult = undefined;
            }
            if (state.addPaneContainer) {
                var widget = VRS.jQueryUIHelper.getOptionPanePlugin(state.addPaneContainer);
                widget.destroy();
            }
            $.each(state.paneContainers, function (idx, optionPane) {
                var widget = VRS.jQueryUIHelper.getOptionPanePlugin(optionPane);
                widget.destroy();
            });
            this.element.empty();
        };
        OptionFieldPaneListPlugin.prototype._addPane = function (pane, state) {
            var field = this.options.field;
            var showRemoveButton = !field.getSuppressRemoveButton();
            if (pane.getFieldCount()) {
                var lastField = pane.getField(pane.getFieldCount() - 1);
                var lastFieldKeepWithNext = lastField.getKeepWithNext();
                var removeButtonFieldName = 'removePane';
                if (showRemoveButton) {
                    lastField.setKeepWithNext(true);
                    pane.addField(new VRS.OptionFieldButton({
                        name: removeButtonFieldName,
                        labelKey: 'Remove',
                        saveState: function () { field.removePane(pane); },
                        icon: 'trash',
                        showText: false
                    }));
                }
                var paneContainer = this._addOptionsPane(pane, state.panesParent);
                state.paneContainers.push(paneContainer);
                if (showRemoveButton) {
                    pane.removeFieldByName(removeButtonFieldName);
                    lastField.setKeepWithNext(lastFieldKeepWithNext);
                }
            }
        };
        OptionFieldPaneListPlugin.prototype._addOptionsPane = function (pane, parent) {
            return $('<div/>')
                .vrsOptionPane(VRS.jQueryUIHelper.getOptionPaneOptions({
                optionPane: pane,
                optionPageParent: this.options.optionPageParent,
                isInStack: true
            }))
                .appendTo(parent);
        };
        OptionFieldPaneListPlugin.prototype._refreshControlStates = function () {
            var state = this._getState();
            var field = this.options.field;
            if (state.addParent) {
                var maxPanes = field.getMaxPanes();
                var disabled = maxPanes !== -1 && state.paneContainers.length >= maxPanes;
                field.getRefreshAddControls()(disabled, state.addParent);
            }
        };
        OptionFieldPaneListPlugin.prototype._paneAdded = function (pane) {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            this._addPane(pane, state);
            this._refreshControlStates();
        };
        OptionFieldPaneListPlugin.prototype._paneRemoved = function (pane, index) {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            var paneContainer = state.paneContainers[index];
            state.paneContainers.splice(index, 1);
            paneContainer.remove();
            this._refreshControlStates();
        };
        OptionFieldPaneListPlugin.prototype._maxPanesChanged = function () {
            this._refreshControlStates();
        };
        return OptionFieldPaneListPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldPaneListPlugin = OptionFieldPaneListPlugin;
    $.widget('vrs.vrsOptionFieldPaneList', new OptionFieldPaneListPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.paneList, function (settings) {
            return $('<div/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldPaneList(VRS.jQueryUIHelper.getOptionFieldPaneListOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldPaneList.js.map