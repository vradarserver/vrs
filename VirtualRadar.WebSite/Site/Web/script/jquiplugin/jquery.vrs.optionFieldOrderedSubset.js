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

(function(VRS, $, /** object= */ undefined)
{
    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldOrderedSubset attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldOrderedSubset}
     */
    VRS.jQueryUIHelper.getOptionFieldOrderedSubsetPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldOrderedSubset'); };

    /**
     * Returns the default options for an option ordered subset widget, with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDORDEREDSUBSET=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDORDEREDSUBSET}
     */
    VRS.jQueryUIHelper.getOptionFieldOrderedSubsetPlugin = function(overrides)
    {
        return $.extend({
            field:      undefined,      // The VRS.OptionField that describes the field being edited.
            optionPageParent: null,     // The object that carries events across the entire options form.

            __nop: null
        }, overrides);
    };
    //endregion

    //region OptionFieldOrderedSubsetState
    /**
     * The state for the ordered subset control.
     * @constructor
     */
    VRS.OptionFieldOrderedSubsetState = function()
    {
        /**
         * The jQuery element that holds all of the elements for the selected subset.
         * @type {jQuery}
         */
        this.subsetElement = null;

        /**
         * The jQuery UI sortable element;
         * @type {jQuery}
         */
        this.sortableElement = null;

        /**
         * The jQuery element that holds the edit controls toolstrip.
         * @type {jQuery}
         */
        this.toolstripElement = null;

        /**
         * The jQueryU element that holds the list of values that can be added to the subset.
         * @type {jQuery}
         */
        this.addSelectElement = null;

        /**
         * The jQuery element for the add to subset button.
         * @type {jQuery}
         */
        this.addButtonElement = null;

        /**
         * The jQuery element for the lock buton.
         * @type {jQuery}
         */
        this.lockElement = null;

        /**
         * True if modifications to the list are locked, false if they are not.
         * @type {boolean}
         */
        this.lockEnabled = true;
    };
    //endregion

    //region vrsOptionFieldOrderedSubset
    /**
     * A plugin that allows users to select a subset of a larger set of items.
     * @namespace VRS.vrsOptionFieldOrderedSubset
     */
    $.widget('vrs.vrsOptionFieldOrderedSubset', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDORDEREDSUBSET} */
        options: VRS.jQueryUIHelper.getOptionFieldOrderedSubsetPlugin(),
        //endregion

        //region -- _getState, _create
        /**
         * Returns the state associated with the plugin, creating it if it doesn't already exist.
         * @returns {VRS.OptionFieldOrderedSubsetState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('vrsOptionFieldOrderedSubsetState');
            if(result === undefined) {
                result = new VRS.OptionFieldOrderedSubsetState();
                this.element.data('vrsOptionFieldOrderedSubsetState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the plugin.
         * @private
         */
        _create: function()
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
        },

        _destroy: function()
        {
            var state = this._getState();

            if(state.sortableElement) {
                state.sortableElement.off();
                state.sortableElement.find('.button').off();
                state.sortableElement.sortable('destroy');
                state.subsetElement.remove();
                state.sortableElement = null;
            }

            if(state.subsetElement) state.subsetElement.remove();
            state.subsetElement = null;

            this._removeToolstripContent(state);

            if(state.toolstripElement) state.toolstripElement.remove();
            state.toolstripElement = null;

            this.element.empty();
        },

        /**
         * Populates the toolstrip element with controls.
         * @param {VRS.OptionFieldOrderedSubsetState}   state           The state object.
         * @param {VRS.ValueText[]}                     values          The full set of values that the user can choose from.
         * @param {VRS.ValueText[]}                     subsetValues    The set of values that the user has already chosen.
         * @param {*}                                  [autoSelect]     The value to automatically select in the add dropdown. If the value is no longer in the dropdown then the next nearest is selected.
         * @private
         */
        _buildToolstripContent: function(state, values, subsetValues, autoSelect)
        {
            this._removeToolstripContent(state);

            var allValues = values.slice();
            allValues.sort(function(/** VRS.ValueText */ lhs, /** VRS.ValueText */ rhs) {
                var lhsText = lhs.getText() || '';
                var rhsText = rhs.getText() || '';
                return lhsText.localeCompare(rhsText);
            });

            var availableValues = VRS.arrayHelper.except(allValues, subsetValues);

            var autoSelectValue = autoSelect === undefined ? undefined : VRS.arrayHelper.findFirst(availableValues, function(/** VRS.ValueText */ value) { return value.getValue() === autoSelect; });
            if(autoSelect !== undefined && !autoSelectValue) {
                var autoSelectIndex = VRS.arrayHelper.indexOfMatch(allValues, function(/** VRS.ValueText */ value) { return value.getValue() === autoSelect; });
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
            $.each(availableValues, function(/** number */idx, /** VRS.ValueText */ value) {
                $('<option/>')
                    .attr('value', value.getValue())
                    .text(value.getText())
                    .appendTo(state.addSelectElement);
            });
            if(autoSelectValue) state.addSelectElement.val(autoSelectValue.getValue());

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
        },

        /**
         * Modifies the display to reflect the state of the controls.
         * @param {VRS.OptionFieldOrderedSubsetState} state
         * @private
         */
        _showEnabledDisabled: function(state)
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
        },

        /**
         * Removes all of the controls in the toolstrip.
         * @param {VRS.OptionFieldOrderedSubsetState} state
         * @private
         */
        _removeToolstripContent: function(state)
        {
            if(state.addSelectElement) state.addSelectElement.remove();
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
        },

        /**
         * Creates the sortable element containing entries for the selected list.
         * @param {VRS.OptionFieldOrderedSubsetState}   state   The state object.
         * @param {jQuery}                              parent  The parent to attach the sortable to.
         * @param {VRS.ValueText[]}                     values  The values to display within the sortable.
         * @private
         */
        _buildSortableList: function(state, parent, values)
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
        },

        /**
         * Adds a value to the sortable list.
         * @param {VRS.OptionFieldOrderedSubsetState}   state
         * @param {VRS.ValueText}                       value
         * @private
         */
        _addValueToSortableList: function(state, value)
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
        },

        /**
         * Destroys the element representing the sortable list value.
         * @param {jQuery} valueElement The LI element representing a value in the sortable list.
         * @private
         */
        _destroySortableListValue: function(valueElement)
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
        },
        //endregion

        //region -- _getSubsetValueObjects, _getSelectedValues
        /**
         * Returns a copy of the subset array filtered so that any elements not present in the values array are removed.
         * @param {VRS.ValueText[]}     values  The full set of values.
         * @param {Array}               subset  The subset of values that the user has chosen.
         * @returns {VRS.ValueText[]}           All of the values in subset that are also in values.
         * @private
         */
        _getSubsetValueObjects: function(values, subset)
        {
            var result = [];
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
        },

        /**
         * Returns an array of values that are present in the subset control.
         * @param {VRS.OptionFieldOrderedSubsetState} state
         * @private {Array}
         */
        _getSelectedValues: function(state)
        {
            var result = [];
            var items = state.sortableElement.find('> li');
            items.each(function(idx, item) {
                var data = $(item).data('vrs');
                if(data) result.push(data.value);
            });

            return result;
        },
        //endregion

        //region -- Events consumed - _sortableSortStopped, _addValueButtonClicked
        /**
         * Called when the user stops sorting the subset of selected values.
         * @param {Event}                   event           The event object.
         * @param {JQUERYUI_SORTABLE_UI}    ui              The jQuery UI object.
         * @private
         */
        _sortableSortStopped: function(event, ui)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var state = this._getState();
            var options = this.options;
            var field = options.field;

            var values = this._getSelectedValues(state);
            field.setValue(values);
            field.saveState();
            options.optionPageParent.raiseFieldChanged();

            this._showEnabledDisabled(state);
        },

        /**
         * Called when the user clicks one of the sort-up or sort-down buttons.
         * @param {Event} event
         * @private
         */
        _sortIncrementClicked: function(event)
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
                    var self = this;
                    var complete = function() {
                        if(++animationsCompleted == 2) {
                            topElement.css({ position: '', top: ''});
                            bottomElement.css({ position: '', top: ''});

                            if(moveUp) clickedListItem.insertBefore(topElement);
                            else       clickedListItem.insertAfter(bottomElement);

                            field.setValue(values);
                            field.saveState();
                            options.optionPageParent.raiseFieldChanged();

                            self._showEnabledDisabled(state);
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
        },

        /**
         * Called when the user clicks the ADD button for a new subset value.
         * @private
         */
        _addValueButtonClicked: function()
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var allValues = field.getValues();
            var newValue;
            var selectedValue = state.addSelectElement.val();
            if(selectedValue) newValue = VRS.arrayHelper.findFirst(allValues, function(/** VRS.ValueText */ searchValue) { return searchValue.getValue() === selectedValue; });
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
        },

        /**
         * Called when the user clicks the toggle delete mode button.
         * @private
         */
        _lockButtonClicked: function()
        {
            var state = this._getState();
            state.lockEnabled = !state.lockEnabled;

            state.sortableElement.sortable('option', 'disabled', state.lockEnabled);

            this._showEnabledDisabled(state);
        },

        /**
         * Called when the user clicks a delete icon.
         * @params {Event} event
         * @private
         */
        _removeIconClicked: function(event)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

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
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.orderedSubset,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<div/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldOrderedSubset(VRS.jQueryUIHelper.getOptionFieldOrderedSubsetPlugin(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));