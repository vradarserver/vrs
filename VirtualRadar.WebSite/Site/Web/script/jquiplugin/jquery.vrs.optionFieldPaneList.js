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
 * @fileoverview A jQuery UI plugin that lets users edit an array of values where each entry in the array is represented by a pane of fields.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region OptionFieldPaneListState
    /**
     * The object that carries state for the pane list control.
     * @constructor
     */
    VRS.OptionFieldPaneListState = function()
    {
        /**
         * A jQuery object describing the container for the list.
         * @type {jQuery=}
         */
        this.container = undefined;

        /**
         * A jQuery object describing the container for the panes.
         * @type {jQuery=}
         */
        this.panesParent = undefined;

        /**
         * A jQuery object describing the container for the add panel.
         * @type {jQuery=}
         */
        this.addParent = undefined;

        /**
         * An array of containers, one for each pane in index order.
         * @type {Array.<jQuery>}
         */
        this.paneContainers = [];

        /**
         * The option pane for the "add a pane" panel.
         * @type {jQuery}
         */
        this.addPaneContainer = null;

        /**
         * The hook result from the pane added event we're hooking on OptionPaneListTypeSettings.
         * @type {object=}
         */
        this.paneAddedHookResult = undefined;

        /**
         * The hook result from the pane removed event we're hooking on OptionPaneListTypeSettings.
         * @type {object=}
         */
        this.paneRemovedHookResult = undefined;

        /**
         * The hook result from the max panes changed event we're hooking on OptionPaneListTypeSettings.
         * @type {object=}
         */
        this.maxPanesChangedHookResult = undefined;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Gets the VRS.vrsOptionFieldPaneList widget attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldPaneList}
     */
    VRS.jQueryUIHelper.getOptionFieldPaneListPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldPaneList'); };

    /**
     * Returns the default options for the option field pane list widget, with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDPANELIST=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDPANELIST}
     */
    VRS.jQueryUIHelper.getOptionFieldPaneListOptions = function(overrides)
    {
        return $.extend({
            field: undefined,
            optionPageParent: null,

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionFieldPaneList
    /**
     * A jQuery UI widget that allows an array to be edited via a list of panes.
     * @namespace VRS.vrsOptionFieldPaneList
     */
    $.widget('vrs.vrsOptionFieldPaneList', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDPANELIST} */
        options: VRS.jQueryUIHelper.getOptionFieldPaneListOptions(),
        //endregion

        //region -- _getState, _create
        /**
         * Returns the state object attached to the plugin, creating it if it's not already there.
         * @returns {VRS.OptionFieldPaneListState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('optionFieldPanelListState');
            if(result === undefined) {
                result = new VRS.OptionFieldPaneListState();
                this.element.data('optionFieldPanelListState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
        {
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
            for(var i = 0;i < length;++i) {
                var pane = panes[i];
                this._addPane(pane, state);
            }

            var addPanel = field.getAddPane();
            if(addPanel) {
                state.addParent =
                    $('<div/>')
                        .appendTo(state.container);
                state.addPaneContainer = this._addOptionsPane(addPanel, state.addParent);
            }

            this._refreshControlStates();
        },

        /**
         * Called when jQuery UI destroys the widget - releases any resources allocated to it.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            var field = this.options.field;

            if(state.paneAddedHookResult) {
                field.unhook(state.paneAddedHookResult);
                state.paneAddedHookResult = undefined;
            }

            if(field.paneRemovedHookResult) {
                field.unhook(state.paneRemovedHookResult);
                state.paneRemovedHookResult = undefined;
            }

            if(field.maxPanesChangedHookResult) {
                field.unhook(state.maxPanesChangedHookResult);
                state.maxPanesChangedHookResult = undefined;
            }

            if(state.addPaneContainer) {
                var widget = VRS.jQueryUIHelper.getOptionPanePlugin(state.addPaneContainer);
                widget.destroy();
            }
            $.each(state.paneContainers, function(/** Number */ idx, /** jQuery */ optionPane) {
                var widget = VRS.jQueryUIHelper.getOptionPanePlugin(optionPane);
                widget.destroy();
            });

            this.element.empty();
        },

        /**
         * Adds a new pane that represents an item in the array to the list of panes.
         * @param {VRS.OptionPane}                  pane
         * @param {VRS.OptionFieldPaneListState}    state
         * @private
         */
        _addPane: function(pane, state)
        {
            var field = this.options.field;
            var showRemoveButton = !field.getSuppressRemoveButton();

            if(pane.getFieldCount()) {
                var lastField = pane.getField(pane.getFieldCount() - 1);
                var lastFieldKeepWithNext = lastField.getKeepWithNext();

                var removeButtonFieldName = 'removePane';
                if(showRemoveButton) {
                    lastField.setKeepWithNext(true);
                    pane.addField(new VRS.OptionFieldButton({
                        name:           removeButtonFieldName,
                        labelKey:       'Remove',
                        saveState:      function() { field.removePane(pane); },
                        icon:           'trash',
                        showText:       false
                    }));
                }

                var paneContainer = this._addOptionsPane(pane, state.panesParent);
                state.paneContainers.push(paneContainer);

                if(showRemoveButton) {
                    pane.removeFieldByName(removeButtonFieldName);
                    lastField.setKeepWithNext(lastFieldKeepWithNext);
                }
            }
        },

        /**
         * Adds a new pane containing controls that can add item panes to the UI. It always appears after the list of
         * item panes.
         * @param {VRS.OptionPane}      pane        The pane holding controls to create new panes.
         * @param {jQuery}              parent      The jQuery object representing the parent control.
         * @returns {jQuery}                        The jQuery object created by this method, to which the pane has been attached.
         * @private
         */
        _addOptionsPane: function(pane, parent)
        {
            return $('<div/>')
                .vrsOptionPane(VRS.jQueryUIHelper.getOptionPaneOptions({
                    optionPane: pane,
                    optionPageParent: this.options.optionPageParent,
                    isInStack: true
                }))
                .appendTo(parent);
        },

        /**
         * Updates the enabled and disabled states of all of the panes and their controls.
         * @private
         */
        _refreshControlStates: function()
        {
            var state = this._getState();
            var field = this.options.field;

            if(state.addParent) {
                var maxPanes = field.getMaxPanes();
                var disabled = maxPanes !== -1 && state.paneContainers.length >= maxPanes;
                field.getRefreshAddControls()(disabled, state.addParent);
            }
        },
        //endregion

        //region -- Events consumed
        /**
         * Called when the user adds a new item and pane to the list.
         * @param {VRS.OptionPane}  pane    The pane for the new item.
         //* @param {number}          index   The index of the new item in the list.
         * @private
         */
        _paneAdded: function(pane)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var state = this._getState();
            this._addPane(pane, state);
            this._refreshControlStates();
        },

        /**
         * Called when the user removes an existing item and pane from the list.
         * @param {VRS.OptionPane}  pane    The pane removed from the list.
         * @param {number}          index   The index of the item removed from the list.
         * @private
         */
        _paneRemoved: function(pane, index)
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            var state = this._getState();
            var paneContainer = state.paneContainers[index];
            state.paneContainers.splice(index, 1);
            paneContainer.remove();
            this._refreshControlStates();
        },

        /**
         * Called when something has changed the maximum number of panes (and thereby items) that the user can have in
         * the list.
         * @private
         */
        _maxPanesChanged: function()
        {
            this._refreshControlStates();
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control types
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.paneList,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<div/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldPaneList(VRS.jQueryUIHelper.getOptionFieldPaneListOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));