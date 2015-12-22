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

namespace VRS
{
    /**
     * The options for the OptionFieldPaneListPlugin.
     */
    export interface OptionFieldPaneListPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldPaneList;
    }

    /**
     * The object that carries state for the OptionFieldPaneListPlugin.
     */
    class OptionFieldPaneListPlugin_State
    {
        /**
         * A jQuery object describing the container for the list.
         */
        container: JQuery;

        /**
         * A jQuery object describing the container for the panes.
         */
        panesParent: JQuery;

        /**
         * A jQuery object describing the container for the add panel.
         */
        addParent: JQuery;

        /**
         * An array of containers, one for each pane in index order.
         */
        paneContainers: JQuery[] = [];

        /**
         * The option pane for the "add a pane" panel.
         */
        addPaneContainer: JQuery = null;

        /**
         * The hook result from the pane added event we're hooking on OptionPaneListTypeSettings.
         */
        paneAddedHookResult: IEventHandle;

        /**
         * The hook result from the pane removed event we're hooking on OptionPaneListTypeSettings.
         */
        paneRemovedHookResult: IEventHandle;

        /**
         * The hook result from the max panes changed event we're hooking on OptionPaneListTypeSettings.
         */
        maxPanesChangedHookResult: IEventHandle;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldPaneListPlugin = function(jQueryElement: JQuery) : OptionFieldPaneListPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldPaneList');
    }
    VRS.jQueryUIHelper.getOptionFieldPaneListOptions = function(overrides: OptionFieldPaneListPlugin_Options) : OptionFieldPaneListPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A jQuery UI widget that allows an array to be edited via a list of panes.
     */
    export class OptionFieldPaneListPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldPaneListPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldPaneListOptions();
        }

        private _getState() : OptionFieldPaneListPlugin_State
        {
            var result = this.element.data('optionFieldPanelListState');
            if(result === undefined) {
                result = new OptionFieldPaneListPlugin_State();
                this.element.data('optionFieldPanelListState', result);
            }

            return result;
        }

        _create()
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
        }

        _destroy()
        {
            var state = this._getState();
            var field = this.options.field;

            if(state.paneAddedHookResult) {
                field.unhook(state.paneAddedHookResult);
                state.paneAddedHookResult = undefined;
            }

            if(state.paneRemovedHookResult) {
                field.unhook(state.paneRemovedHookResult);
                state.paneRemovedHookResult = undefined;
            }

            if(state.maxPanesChangedHookResult) {
                field.unhook(state.maxPanesChangedHookResult);
                state.maxPanesChangedHookResult = undefined;
            }

            if(state.addPaneContainer) {
                var widget = VRS.jQueryUIHelper.getOptionPanePlugin(state.addPaneContainer);
                widget.destroy();
            }

            $.each(state.paneContainers, function(idx, optionPane) {
                var widget = VRS.jQueryUIHelper.getOptionPanePlugin(optionPane);
                widget.destroy();
            });

            this.element.empty();
        }

        /**
         * Adds a new pane that represents an item in the array to the list of panes.
         */
        private _addPane(pane: OptionPane, state: OptionFieldPaneListPlugin_State)
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
        }

        /**
         * Adds a new pane containing controls that can add item panes to the UI. It always appears after the list of
         * item panes.
         */
        private _addOptionsPane(pane: OptionPane, parent: JQuery) : JQuery
        {
            return $('<div/>')
                .vrsOptionPane(VRS.jQueryUIHelper.getOptionPaneOptions({
                    optionPane: pane,
                    optionPageParent: this.options.optionPageParent,
                    isInStack: true
                }))
                .appendTo(parent);
        }

        /**
         * Updates the enabled and disabled states of all of the panes and their controls.
         */
        private _refreshControlStates()
        {
            var state = this._getState();
            var field = this.options.field;

            if(state.addParent) {
                var maxPanes = field.getMaxPanes();
                var disabled = maxPanes !== -1 && state.paneContainers.length >= maxPanes;
                field.getRefreshAddControls()(disabled, state.addParent);
            }
        }

        /**
         * Called when the user adds a new item and pane to the list.
         */
        private _paneAdded(pane: OptionPane)
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var state = this._getState();
            this._addPane(pane, state);
            this._refreshControlStates();
        }

        /**
         * Called when the user removes an existing item and pane from the list.
         */
        _paneRemoved(pane: OptionPane, index: number)
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }

            var state = this._getState();
            var paneContainer = state.paneContainers[index];
            state.paneContainers.splice(index, 1);
            paneContainer.remove();
            this._refreshControlStates();
        }

        /**
         * Called when something has changed the maximum number of panes (and thereby items) that the user can have in the list.
         */
        private _maxPanesChanged()
        {
            this._refreshControlStates();
        }
    }

    $.widget('vrs.vrsOptionFieldPaneList', new OptionFieldPaneListPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.paneList,
            function(settings: OptionFieldPaneListPlugin_Options) {
                return $('<div/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldPaneList(VRS.jQueryUIHelper.getOptionFieldPaneListOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldPaneList();
    vrsOptionFieldPaneList(options: VRS.OptionFieldPaneListPlugin_Options);
    vrsOptionFieldPaneList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
