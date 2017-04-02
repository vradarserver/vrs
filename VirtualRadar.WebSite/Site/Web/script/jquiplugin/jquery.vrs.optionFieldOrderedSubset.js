var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldOrderedSubsetPlugin_State = (function () {
        function OptionFieldOrderedSubsetPlugin_State() {
            this.subsetElement = null;
            this.sortableElement = null;
            this.toolstripElement = null;
            this.addSelectElement = null;
            this.addButtonElement = null;
            this.lockElement = null;
            this.lockEnabled = true;
        }
        return OptionFieldOrderedSubsetPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldOrderedSubsetPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldOrderedSubset');
    };
    VRS.jQueryUIHelper.getOptionFieldOrderedSubsetOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldOrderedSubsetPlugin = (function (_super) {
        __extends(OptionFieldOrderedSubsetPlugin, _super);
        function OptionFieldOrderedSubsetPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldOrderedSubsetOptions();
            return _this;
        }
        OptionFieldOrderedSubsetPlugin.prototype._getState = function () {
            var result = this.element.data('vrsOptionFieldOrderedSubsetState');
            if (result === undefined) {
                result = new OptionFieldOrderedSubsetPlugin_State();
                this.element.data('vrsOptionFieldOrderedSubsetState', result);
            }
            return result;
        };
        OptionFieldOrderedSubsetPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var values = field.getValues();
            var subset = field.getValue() || [];
            var subsetValues = this._getSubsetValueObjects(values, subset);
            this.element
                .addClass('vrsOptionPluginSubset');
            state.toolstripElement = $('<div/>')
                .addClass('toolstrip')
                .appendTo(this.element);
            state.subsetElement = $('<div/>')
                .addClass('subset')
                .appendTo(this.element);
            this._buildToolstripContent(state, values, subsetValues);
            this._buildSortableList(state, state.subsetElement, subsetValues);
            this._showEnabledDisabled(state);
        };
        OptionFieldOrderedSubsetPlugin.prototype._destroy = function () {
            var state = this._getState();
            if (state.sortableElement) {
                state.sortableElement.off();
                state.sortableElement.find('.button').off();
                state.sortableElement.sortable('destroy');
                state.subsetElement.remove();
                state.sortableElement = null;
            }
            if (state.subsetElement) {
                state.subsetElement.remove();
            }
            state.subsetElement = null;
            this._removeToolstripContent(state);
            if (state.toolstripElement) {
                state.toolstripElement.remove();
            }
            state.toolstripElement = null;
            this.element.empty();
        };
        OptionFieldOrderedSubsetPlugin.prototype._buildToolstripContent = function (state, values, subsetValues, autoSelect) {
            this._removeToolstripContent(state);
            var allValues = values.slice();
            allValues.sort(function (lhs, rhs) {
                var lhsText = lhs.getText() || '';
                var rhsText = rhs.getText() || '';
                return lhsText.localeCompare(rhsText);
            });
            var availableValues = VRS.arrayHelper.except(allValues, subsetValues);
            var autoSelectValue = autoSelect === undefined ? undefined
                : VRS.arrayHelper.findFirst(availableValues, function (value) {
                    return value.getValue() === autoSelect;
                });
            if (autoSelect !== undefined && !autoSelectValue) {
                var autoSelectIndex = VRS.arrayHelper.indexOfMatch(allValues, function (value) {
                    return value.getValue() === autoSelect;
                });
                if (autoSelectIndex !== -1) {
                    for (var searchDirection = 0; !autoSelectValue && searchDirection < 2; ++searchDirection) {
                        var startIndex = searchDirection ? autoSelectIndex - 1 : autoSelectIndex + 1;
                        var step = startIndex < autoSelectIndex ? -1 : 1;
                        var limit = step === -1 ? -1 : allValues.length;
                        for (var i = startIndex; i !== limit; i += step) {
                            var candidate = allValues[i];
                            if (VRS.arrayHelper.indexOf(availableValues, candidate) !== -1) {
                                autoSelectValue = candidate;
                                break;
                            }
                        }
                    }
                }
            }
            var parent = state.toolstripElement;
            state.addSelectElement = $('<select/>')
                .appendTo(parent);
            $.each(availableValues, function (idx, value) {
                $('<option/>')
                    .attr('value', value.getValue())
                    .text(value.getText())
                    .appendTo(state.addSelectElement);
            });
            if (autoSelectValue) {
                state.addSelectElement.val(autoSelectValue.getValue());
            }
            state.addButtonElement = $('<button/>')
                .on('click', $.proxy(this._addValueButtonClicked, this))
                .appendTo(parent)
                .button({
                label: VRS.$$.Add
            });
            state.lockElement = $('<div/>')
                .addClass('lockButton vrsIcon')
                .on('click', $.proxy(this._lockButtonClicked, this))
                .appendTo(parent);
            this._showEnabledDisabled(state);
        };
        OptionFieldOrderedSubsetPlugin.prototype._showEnabledDisabled = function (state) {
            if (state.sortableElement && state.lockElement) {
                var listItems = state.sortableElement.children('li');
                var icons = state.sortableElement.find('.button');
                var firstSortUp = state.sortableElement.find('.vrsIcon-sort-up').first();
                var lastSortDown = state.sortableElement.find('.vrsIcon-sort-down').last();
                if (state.lockEnabled) {
                    state.lockElement.removeClass('vrsIcon-unlocked').addClass('locked vrsIcon-locked');
                    listItems.addClass('locked');
                    icons.addClass('locked');
                    state.addSelectElement.prop('disabled', true);
                    state.addButtonElement.button('disable');
                }
                else {
                    state.lockElement.removeClass('locked vrsIcon-locked').addClass('vrsIcon-unlocked');
                    listItems.removeClass('locked');
                    icons.removeClass('locked');
                    firstSortUp.addClass('locked');
                    lastSortDown.addClass('locked');
                    if (state.addSelectElement.children().length === 0) {
                        state.addSelectElement.prop('disabled', true);
                        state.addButtonElement.button('disable');
                    }
                    else {
                        state.addSelectElement.prop('disabled', false);
                        state.addButtonElement.button('enable');
                    }
                }
            }
        };
        OptionFieldOrderedSubsetPlugin.prototype._removeToolstripContent = function (state) {
            if (state.addSelectElement) {
                state.addSelectElement.remove();
            }
            state.addSelectElement = null;
            if (state.addButtonElement) {
                state.addButtonElement.off();
                state.addButtonElement.button('destroy');
                state.addButtonElement.remove();
            }
            state.addButtonElement = null;
            if (state.lockElement) {
                state.lockElement.off();
                state.lockElement.remove();
            }
            state.lockElement = null;
        };
        OptionFieldOrderedSubsetPlugin.prototype._buildSortableList = function (state, parent, values) {
            state.sortableElement = $('<ul/>')
                .uniqueId()
                .appendTo(parent);
            var length = values.length;
            for (var i = 0; i < length; ++i) {
                var value = values[i];
                this._addValueToSortableList(state, value);
            }
            state.sortableElement.sortable({
                containment: 'parent',
                disabled: state.lockEnabled,
                cancel: '.button',
                stop: $.proxy(this._sortableSortStopped, this)
            });
        };
        OptionFieldOrderedSubsetPlugin.prototype._addValueToSortableList = function (state, value) {
            $('<li/>')
                .data('vrs', { value: value.getValue() })
                .text(value.getText())
                .append($('<div/>')
                .addClass('button vrsIcon vrsIcon-sort-up')
                .on('click', $.proxy(this._sortIncrementClicked, this)))
                .append($('<div/>')
                .addClass('button vrsIcon vrsIcon-sort-down')
                .on('click', $.proxy(this._sortIncrementClicked, this)))
                .append($('<div/>')
                .addClass('button vrsIcon vrsIcon-remove')
                .on('click', $.proxy(this._removeIconClicked, this)))
                .appendTo(state.sortableElement);
        };
        OptionFieldOrderedSubsetPlugin.prototype._destroySortableListValue = function (valueElement) {
            valueElement.hide({
                effect: 'Clip',
                duration: 300,
                complete: function () {
                    valueElement.find('div')
                        .off()
                        .remove();
                    valueElement
                        .data('vrs', null)
                        .off()
                        .remove();
                }
            });
        };
        OptionFieldOrderedSubsetPlugin.prototype._getSubsetValueObjects = function (values, subset) {
            var result = [];
            var subsetLength = subset.length;
            var valuesLength = values.length;
            for (var i = 0; i < subsetLength; ++i) {
                var subsetValue = undefined;
                for (var c = 0; c < valuesLength; ++c) {
                    var value = values[c];
                    if (value.getValue() === subset[i]) {
                        subsetValue = value;
                        break;
                    }
                }
                if (!subsetValue)
                    throw 'Cannot find the value that corresponds with subset value ' + subset[i];
                result.push(subsetValue);
            }
            return result;
        };
        OptionFieldOrderedSubsetPlugin.prototype._getSelectedValues = function (state) {
            var result = [];
            var items = state.sortableElement.find('> li');
            items.each(function (idx, item) {
                var data = $(item).data('vrs');
                if (data)
                    result.push(data.value);
            });
            return result;
        };
        OptionFieldOrderedSubsetPlugin.prototype._sortableSortStopped = function (event, ui) {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var values = this._getSelectedValues(state);
            field.setValue(values);
            field.saveState();
            options.optionPageParent.raiseFieldChanged();
            this._showEnabledDisabled(state);
        };
        OptionFieldOrderedSubsetPlugin.prototype._sortIncrementClicked = function (event) {
            var _this = this;
            if (VRS.timeoutManager)
                VRS.timeoutManager.resetTimer();
            var state = this._getState();
            if (!state.lockEnabled) {
                var options = this.options;
                var field = options.field;
                var values = this._getSelectedValues(state);
                var clickedIcon = $(event.target);
                var moveUp = clickedIcon.hasClass('vrsIcon-sort-up');
                var clickedListItem = clickedIcon.parent();
                var clickedValue = clickedListItem.data('vrs').value;
                var clickedIndex = VRS.arrayHelper.indexOf(values, clickedValue);
                if ((moveUp && clickedIndex !== 0) || (!moveUp && clickedIndex + 1 < values.length)) {
                    values.splice(clickedIndex, 1);
                    var index = moveUp ? clickedIndex - 1 : clickedIndex + 1;
                    values.splice(index, 0, clickedValue);
                    var topElement = moveUp ? clickedListItem.prev() : clickedListItem;
                    var bottomElement = moveUp ? clickedListItem : clickedListItem.next();
                    var topOffset = topElement.offset().top;
                    var bottomOffset = bottomElement.offset().top;
                    var animationsCompleted = 0;
                    var complete = function () {
                        if (++animationsCompleted == 2) {
                            topElement.css({ position: '', top: '' });
                            bottomElement.css({ position: '', top: '' });
                            if (moveUp)
                                clickedListItem.insertBefore(topElement);
                            else
                                clickedListItem.insertAfter(bottomElement);
                            field.setValue(values);
                            field.saveState();
                            options.optionPageParent.raiseFieldChanged();
                            _this._showEnabledDisabled(state);
                        }
                    };
                    topElement.css('position', 'relative');
                    bottomElement.css('position', 'relative');
                    var duration = 300;
                    topElement.animate({ top: bottomOffset - topOffset }, {
                        duration: duration,
                        queue: false,
                        complete: complete
                    });
                    bottomElement.animate({ top: topOffset - bottomOffset }, {
                        duration: duration,
                        queue: false,
                        complete: complete
                    });
                }
            }
        };
        OptionFieldOrderedSubsetPlugin.prototype._addValueButtonClicked = function () {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var allValues = field.getValues();
            var newValue;
            var selectedValue = state.addSelectElement.val();
            if (selectedValue) {
                newValue = VRS.arrayHelper.findFirst(allValues, function (searchValue) {
                    return searchValue.getValue() === selectedValue;
                });
            }
            if (newValue) {
                var values = this._getSelectedValues(state);
                values.push(selectedValue);
                field.setValue(values);
                field.saveState();
                options.optionPageParent.raiseFieldChanged();
                var subsetValues = this._getSubsetValueObjects(allValues, values);
                this._addValueToSortableList(state, newValue);
                this._buildToolstripContent(state, allValues, subsetValues, selectedValue);
            }
        };
        OptionFieldOrderedSubsetPlugin.prototype._lockButtonClicked = function () {
            var state = this._getState();
            state.lockEnabled = !state.lockEnabled;
            state.sortableElement.sortable('option', 'disabled', state.lockEnabled);
            this._showEnabledDisabled(state);
        };
        OptionFieldOrderedSubsetPlugin.prototype._removeIconClicked = function (event) {
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            if (!state.lockEnabled) {
                var options = this.options;
                var field = options.field;
                var allValues = field.getValues();
                var subsetValues = this._getSubsetValueObjects(allValues, this._getSelectedValues(state));
                var clickedIcon = $(event.target);
                var clickedListItem = clickedIcon.parent();
                var clickedValue = clickedListItem.data('vrs').value;
                var clickedIndex = VRS.arrayHelper.indexOfMatch(subsetValues, function (valueText) { return valueText.getValue() === clickedValue; });
                subsetValues.splice(clickedIndex, 1);
                var values = VRS.arrayHelper.select(subsetValues, function (valueText) { return valueText.getValue(); });
                field.setValue(values);
                field.saveState();
                options.optionPageParent.raiseFieldChanged();
                this._destroySortableListValue(clickedListItem);
                this._buildToolstripContent(state, allValues, subsetValues, clickedValue);
            }
        };
        return OptionFieldOrderedSubsetPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldOrderedSubsetPlugin = OptionFieldOrderedSubsetPlugin;
    $.widget('vrs.vrsOptionFieldOrderedSubset', new OptionFieldOrderedSubsetPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.orderedSubset, function (settings) {
            return $('<div/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldOrderedSubset(VRS.jQueryUIHelper.getOptionFieldOrderedSubsetOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldOrderedSubset.js.map