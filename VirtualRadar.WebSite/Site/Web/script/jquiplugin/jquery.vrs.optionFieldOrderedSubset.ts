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
 * @fileoverview A jQuery UI plugin that lets users select a subset of items from a larger set.
 */

namespace VRS
{
    /**
     * The options for the OptionFieldOrderedSubsetPlugin
     */
    export interface OptionFieldOrderedSubsetPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldOrderedSubset;
    }

    /**
     * The state held by the OptionFieldOrderedSubsetPlugin
     */
    class OptionFieldOrderedSubsetPlugin_State
    {
        /**
         * The jQuery element that holds all of the elements for the selected subset.
         */
        subsetElement: JQuery = null;

        /**
         * The jQuery UI sortable element;
         */
        sortableElement: JQuery = null;

        /**
         * The jQuery element that holds the edit controls toolstrip.
         */
        toolstripElement: JQuery = null;

        /**
         * The jQuery element that holds the list of values that can be added to the subset.
         */
        addSelectElement: JQuery = null;

        /**
         * The jQuery element for the add to subset button.
         */
        addButtonElement: JQuery = null;

        /**
         * The jQuery element for the lock buton.
         */
        lockElement: JQuery = null;

        /**
         * True if modifications to the list are locked, false if they are not.
         */
        lockEnabled = true;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldOrderedSubsetPlugin = function(jQueryElement: JQuery) : OptionFieldOrderedSubsetPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldOrderedSubset');
    }
    VRS.jQueryUIHelper.getOptionFieldOrderedSubsetOptions = function(overrides?: OptionFieldOrderedSubsetPlugin_Options) : OptionFieldOrderedSubsetPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A plugin that allows users to select a subset of a larger set of items.
     */
    export class OptionFieldOrderedSubsetPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldOrderedSubsetPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldOrderedSubsetOptions();
        }

        private _getState() : OptionFieldOrderedSubsetPlugin_State
        {
            var result = this.element.data('vrsOptionFieldOrderedSubsetState');
            if(result === undefined) {
                result = new OptionFieldOrderedSubsetPlugin_State();
                this.element.data('vrsOptionFieldOrderedSubsetState', result);
            }

            return result;
        }

        _create()
        {
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
        }

        _destroy()
        {
            var state = this._getState();

            if(state.sortableElement) {
                state.sortableElement.off();
                state.sortableElement.find('.button').off();
                state.sortableElement.sortable('destroy');
                state.subsetElement.remove();
                state.sortableElement = null;
            }

            if(state.subsetElement) {
                state.subsetElement.remove();
            }
            state.subsetElement = null;

            this._removeToolstripContent(state);

            if(state.toolstripElement) {
                state.toolstripElement.remove();
            }
            state.toolstripElement = null;

            this.element.empty();
        }

        /**
         * Populates the toolstrip element with controls.
         */
        private _buildToolstripContent(state: OptionFieldOrderedSubsetPlugin_State, values: ValueText[], subsetValues: ValueText[], autoSelect?: any)
        {
            this._removeToolstripContent(state);

            var allValues = values.slice();
            allValues.sort(function(lhs, rhs) {
                var lhsText = lhs.getText() || '';
                var rhsText = rhs.getText() || '';
                return lhsText.localeCompare(rhsText);
            });

            var availableValues = VRS.arrayHelper.except(allValues, subsetValues);
            var autoSelectValue = autoSelect === undefined ? undefined
                                                           : VRS.arrayHelper.findFirst(availableValues, function(value) {
                                                                return value.getValue() === autoSelect;
                                                             });

            if(autoSelect !== undefined && !autoSelectValue) {
                var autoSelectIndex = VRS.arrayHelper.indexOfMatch(allValues, function(value) {
                    return value.getValue() === autoSelect;
                });
                if(autoSelectIndex !== -1) {
                    for(var searchDirection = 0;!autoSelectValue && searchDirection < 2;++searchDirection) {
                        var startIndex = searchDirection ? autoSelectIndex - 1 : autoSelectIndex + 1;
                        var step = startIndex < autoSelectIndex ? -1 : 1;
                        var limit = step === -1 ? -1 : allValues.length;
                        for(var i = startIndex;i !== limit;i += step) {
                            var candidate = allValues[i];
                            if(VRS.arrayHelper.indexOf(availableValues, candidate) !== -1) {
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
            $.each(availableValues, function(idx, value) {
                $('<option/>')
                    .attr('value', value.getValue())
                    .text(value.getText())
                    .appendTo(state.addSelectElement);
            });
            if(autoSelectValue) {
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
        }

        /**
         * Modifies the display to reflect the state of the controls.
         */
        private _showEnabledDisabled(state: OptionFieldOrderedSubsetPlugin_State)
        {
            if(state.sortableElement && state.lockElement) {
                var listItems = state.sortableElement.children('li');
                var icons = state.sortableElement.find('.button');
                var firstSortUp = state.sortableElement.find('.vrsIcon-sort-up').first();
                var lastSortDown = state.sortableElement.find('.vrsIcon-sort-down').last();

                if(state.lockEnabled) {
                    state.lockElement.removeClass('vrsIcon-unlocked').addClass('locked vrsIcon-locked');
                    listItems.addClass('locked');
                    icons.addClass('locked');

                    state.addSelectElement.prop('disabled', true);
                    state.addButtonElement.button('disable');
                } else {
                    state.lockElement.removeClass('locked vrsIcon-locked').addClass('vrsIcon-unlocked');
                    listItems.removeClass('locked');
                    icons.removeClass('locked');
                    firstSortUp.addClass('locked');
                    lastSortDown.addClass('locked');

                    if(state.addSelectElement.children().length === 0) {
                        state.addSelectElement.prop('disabled', true);
                        state.addButtonElement.button('disable');
                    } else {
                        state.addSelectElement.prop('disabled', false);
                        state.addButtonElement.button('enable');
                    }
                }
            }
        }

        /**
         * Removes all of the controls in the toolstrip.
         */
        private _removeToolstripContent(state: OptionFieldOrderedSubsetPlugin_State)
        {
            if(state.addSelectElement) {
                state.addSelectElement.remove();
            }
            state.addSelectElement = null;

            if(state.addButtonElement) {
                state.addButtonElement.off();
                state.addButtonElement.button('destroy');
                state.addButtonElement.remove();
            }
            state.addButtonElement = null;

            if(state.lockElement) {
                state.lockElement.off();
                state.lockElement.remove();
            }
            state.lockElement = null;
        }

        /**
         * Creates the sortable element containing entries for the selected list.
         */
        private _buildSortableList(state: OptionFieldOrderedSubsetPlugin_State, parent: JQuery, values: ValueText[])
        {
            state.sortableElement = $('<ul/>')
                .uniqueId()
                .appendTo(parent);

            var length = values.length;
            for(var i = 0;i < length;++i) {
                var value = values[i];
                this._addValueToSortableList(state, value);
            }

            state.sortableElement.sortable({
                containment: 'parent',
                disabled: state.lockEnabled,
                cancel: '.button',
                stop: $.proxy(this._sortableSortStopped, this)
            });
        }

        /**
         * Adds a value to the sortable list.
         */
        private _addValueToSortableList(state: OptionFieldOrderedSubsetPlugin_State, value: ValueText)
        {
            $('<li/>')
                .data('vrs', { value: value.getValue() })
                .text(value.getText())
                .append($('<div/>')
                    .addClass('button vrsIcon vrsIcon-sort-up')
                    .on('click', $.proxy(this._sortIncrementClicked, this))
                )
                .append($('<div/>')
                    .addClass('button vrsIcon vrsIcon-sort-down')
                    .on('click', $.proxy(this._sortIncrementClicked, this))
                )
                .append($('<div/>')
                    .addClass('button vrsIcon vrsIcon-remove')
                    .on('click', $.proxy(this._removeIconClicked, this))
                )
                .appendTo(state.sortableElement);
        }

        /**
         * Destroys the element representing the sortable list value.
         */
        private _destroySortableListValue(valueElement: JQuery)
        {
            valueElement.hide({
                effect: 'Clip',
                duration: 300,
                complete: function() {
                    valueElement.find('div')
                        .off()
                        .remove();
                    valueElement
                        .data('vrs', null)
                        .off()
                        .remove();
                }
            });
        }

        /**
         * Returns a copy of the subset array filtered so that any elements not present in the values array are removed.
         */
        private _getSubsetValueObjects(values: ValueText[], subset: any[]) : ValueText[]
        {
            var result: ValueText[] = [];
            var subsetLength = subset.length;
            var valuesLength = values.length;
            for(var i = 0;i < subsetLength;++i) {
                var subsetValue = undefined;
                for(var c = 0;c < valuesLength;++c) {
                    var value = values[c];
                    if(value.getValue() === subset[i]) {
                        subsetValue = value;
                        break;
                    }
                }
                if(!subsetValue) throw 'Cannot find the value that corresponds with subset value ' + subset[i];
                result.push(subsetValue);
            }

            return result;
        }

        /**
         * Returns an array of values that are present in the subset control.
         */
        _getSelectedValues(state: OptionFieldOrderedSubsetPlugin_State) : any[]
        {
            var result: any[] = [];
            var items = state.sortableElement.find('> li');
            items.each(function(idx, item) {
                var data = $(item).data('vrs');
                if(data) result.push(data.value);
            });

            return result;
        }

        /**
         * Called when the user stops sorting the subset of selected values.
         */
        private _sortableSortStopped(event: JQueryEventObject, ui: JQueryUI.SortableUIParams)
        {
            if(VRS.timeoutManager) {
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
        }

        /**
         * Called when the user clicks one of the sort-up or sort-down buttons.
         */
        private _sortIncrementClicked(event: JQueryEventObject)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
            var state = this._getState();
            if(!state.lockEnabled) {
                var options = this.options;
                var field = options.field;

                var values = this._getSelectedValues(state);
                var clickedIcon = $(event.target);
                var moveUp = clickedIcon.hasClass('vrsIcon-sort-up');
                var clickedListItem = clickedIcon.parent();
                var clickedValue = clickedListItem.data('vrs').value;
                var clickedIndex = VRS.arrayHelper.indexOf(values, clickedValue);

                if((moveUp && clickedIndex !== 0) || (!moveUp && clickedIndex + 1 < values.length)) {
                    values.splice(clickedIndex, 1);
                    var index = moveUp ? clickedIndex - 1 : clickedIndex + 1;
                    values.splice(index, 0, clickedValue);

                    var topElement = moveUp ? clickedListItem.prev() : clickedListItem;
                    var bottomElement = moveUp ? clickedListItem : clickedListItem.next();
                    var topOffset = topElement.offset().top;
                    var bottomOffset = bottomElement.offset().top;

                    var animationsCompleted = 0;
                    var complete = () => {
                        if(++animationsCompleted == 2) {
                            topElement.css({ position: '', top: ''});
                            bottomElement.css({ position: '', top: ''});

                            if(moveUp) clickedListItem.insertBefore(topElement);
                            else       clickedListItem.insertAfter(bottomElement);

                            field.setValue(values);
                            field.saveState();
                            options.optionPageParent.raiseFieldChanged();

                            this._showEnabledDisabled(state);
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
        }

        /**
         * Called when the user clicks the ADD button for a new subset value.
         */
        private _addValueButtonClicked()
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var allValues = field.getValues();

            var newValue;
            var selectedValue = state.addSelectElement.val();
            if(selectedValue) {
                newValue = VRS.arrayHelper.findFirst(allValues, function(searchValue) {
                    return searchValue.getValue() === selectedValue;
                });
            }
            if(newValue) {
                var values = this._getSelectedValues(state);
                values.push(selectedValue);
                field.setValue(values);
                field.saveState();
                options.optionPageParent.raiseFieldChanged();

                var subsetValues = this._getSubsetValueObjects(allValues, values);

                this._addValueToSortableList(state, newValue);
                this._buildToolstripContent(state, allValues, subsetValues, selectedValue);
            }
        }

        /**
         * Called when the user clicks the toggle delete mode button.
         */
        private _lockButtonClicked()
        {
            var state = this._getState();
            state.lockEnabled = !state.lockEnabled;

            state.sortableElement.sortable('option', 'disabled', state.lockEnabled);

            this._showEnabledDisabled(state);
        }

        /**
         * Called when the user clicks a delete icon.
         */
        private _removeIconClicked(event: JQueryEventObject)
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var state = this._getState();
            if(!state.lockEnabled) {
                var options = this.options;
                var field = options.field;
                var allValues = field.getValues();
                var subsetValues = this._getSubsetValueObjects(allValues, this._getSelectedValues(state));

                var clickedIcon = $(event.target);
                var clickedListItem = clickedIcon.parent();
                var clickedValue = clickedListItem.data('vrs').value;
                var clickedIndex = VRS.arrayHelper.indexOfMatch(subsetValues, function(/** VRS.ValueText */ valueText) { return valueText.getValue() === clickedValue; });

                subsetValues.splice(clickedIndex, 1);
                var values = VRS.arrayHelper.select(subsetValues, function(/** VRS.ValueText */ valueText) { return valueText.getValue(); });
                field.setValue(values);
                field.saveState();
                options.optionPageParent.raiseFieldChanged();

                this._destroySortableListValue(clickedListItem);
                this._buildToolstripContent(state, allValues, subsetValues, clickedValue);
            }
        }
    }

    $.widget('vrs.vrsOptionFieldOrderedSubset', new OptionFieldOrderedSubsetPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.orderedSubset,
            function(settings: OptionFieldOrderedSubsetPlugin_Options) {
                return $('<div/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldOrderedSubset(VRS.jQueryUIHelper.getOptionFieldOrderedSubsetOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldOrderedSubset();
    vrsOptionFieldOrderedSubset(options: VRS.OptionFieldOrderedSubsetPlugin_Options);
    vrsOptionFieldOrderedSubset(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
