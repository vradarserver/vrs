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
 * @fileoverview Configuration UI abstractions.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region OptionField
    /**
     * Describes a single editable option - there is usually one of these for each configurable property on an object.
     * @param {object}                          settings
     * @param {string}                          settings.name           The unique name of the field.
     * @param {string}                          settings.controlType    The VRS.optionsControlType enum value that determines which control will be used to display and/or edit this field.
     * @param {string|function():string}       [settings.labelKey]      The index into VRS.$$ of the label to use when displaying the field or a function that returns the translated text.
     * @param {function():*}                   [settings.getValue]      A callback that returns the value to display / edit from the object being configured.
     * @param {function(*)}                    [settings.setValue]      A callback that is passed the edited value. It is expected to copy the value into the object being configured.
     * @param {function()}                     [settings.saveState]     A callback that can save the current state of the object being configured.
     * @param {bool}                           [settings.keepWithNext]  If true then the field is to be displayed next to the field that follows it.
     * @param {function(function, *):*}        [settings.hookChanged]   A hook function that can be used to pick up changes to the value that is returned by getValue - for fields whose value may change while the configuration UI is on display.
     * @param {function(*)}                    [settings.unhookChanged] An unhook function that can unhook the event hooked by hookChanged.
     * @param {VRS.InputWidth=}                [settings.inputWidth]    An indication of how wide the input field needs to be.
     * @param {boolean|(function():boolean)}   [settings.visible]       Indicates whether the field should be visible to the user.
     * @constructor
     */
    VRS.OptionField = function(settings)
    {
        if(!settings) throw 'You must supply settings';
        if(!settings.name) throw 'You must supply a name for the field';
        if(!settings.controlType) throw 'You must supply the field\'s control type';
        if(!VRS.optionControlTypeBroker.controlTypeHasHandler(settings.controlType)) throw 'There is no control type handler for ' + settings.controlType;

        settings = $.extend({
            /** @type {string} */                       name: null,
            /** @type {string} */                       dispatcherName: 'VRS.OptionField',          // Filled in by derivees to get a better name for the dispatcher
            /** @type {string} */                       controlType: null,
            /** @type {string} */                       labelKey: '',
            /** @type {function():*} */                 getValue: $.noop,
            /** @type {function(*)} */                  setValue: $.noop,
            /** @type {function()} */                   saveState: $.noop,
            /** @type {boolean} */                      keepWithNext: false,
            /** @type {function(function, *):*} */      hookChanged: null,
            /** @type {function(*)} */                  unhookChanged: null,
            /** @type {VRS.InputWidth} */               inputWidth: VRS.InputWidth.Auto,
            /** @type {boolean|function():boolean} */   visible: true
        }, settings);

        //region -- Fields
        var that = this;

        var _Dispatcher = new VRS.EventHandler({ name: settings.dispatcherName });
        var _Events = {
            refreshFieldContent:            'refreshFieldContent',
            refreshFieldState:              'refreshFieldState',
            refreshFieldVisibility:         'refreshFieldVisibility'
        };

        /** @type {Object} @private */
        var _ChangedHookResult = null;
        //endregion

        //region -- Properties
        this.getName = function()                               { return settings.name; };
        this.getControlType = function()                        { return settings.controlType; };
        this.getDispatcher = function()                         { return _Dispatcher; };
        this.getKeepWithNext = function()                       { return settings.keepWithNext; };
        this.setKeepWithNext = function(/** boolean */ value)   { settings.keepWithNext = value; };
        this.getLabelKey = function()                           { return settings.labelKey; };
        this.getLabelText = function()                          { return VRS.globalisation.getText(settings.labelKey); };
        this.getInputWidth = function()                         { return settings.inputWidth; };
        this.getVisible = function()                            { return $.isFunction(settings.visible) ? settings.visible() : !!settings.visible; };
        //endregion

        //region Events exposed
        /**
         * Hooks an event that is raised when something wants the content of the field to be refreshed. Not all fields respond to this event.
         * @param {function()}  callback
         * @param {Object}     [forceThis]
         * @returns {Object}
         */
        this.hookRefreshFieldContent = function(callback, forceThis) { return _Dispatcher.hook(_Events.refreshFieldContent, callback, forceThis); };

        /**
         * Raises the refreshFieldContent event. Not all fields respond to this.
         */
        this.raiseRefreshFieldContent = function() { _Dispatcher.raise(_Events.refreshFieldContent); };

        /**
         * Hooks an event that is raised when something wants the enabled / disabled state of the field to be refreshed. Not all fields response to this event.
         * @param {function()}  callback
         * @param {Object}     [forceThis]
         * @returns {Object}
         */
        this.hookRefreshFieldState = function(callback, forceThis)   { return _Dispatcher.hook(_Events.refreshFieldState, callback, forceThis); };

        /**
         * Raises the refreshFieldState event. Not all fields respond to this.
         */
        this.raiseRefreshFieldState = function() { _Dispatcher.raise(_Events.refreshFieldState); };

        /**
         * Hooks an event that is raised when the visibility of the field should be refreshed.
         * @param callback
         * @param forceThis
         * @returns {Object}
         */
        this.hookRefreshFieldVisibility = function(callback, forceThis) { return _Dispatcher.hook(_Events.refreshFieldVisibility, callback, forceThis); };

        /**
         * Raises the refreshFieldVisibility event. Not all fields respond to this.
         */
        this.raiseRefreshFieldVisibility = function() { _Dispatcher.raise(_Events.refreshFieldVisibility); };

        /**
         * Unhooks a previously hooked event on the field.
         * @param {Object} hookResult
         */
        this.unhook = function(hookResult)
        {
            _Dispatcher.unhook(hookResult);
        };
        //endregion

        //region -- getValue, setValue, saveState
        /**
         * Calls the getValue method and returns the result.
         * @returns {*}
         */
        this.getValue = function()
        {
            return settings.getValue();
        };

        /**
         * Calls the setValue method with the value passed across.
         * @param {*} value
         */
        this.setValue = function(value)
        {
            settings.setValue(value);
        };

        /**
         * Calls the saveState method.
         */
        this.saveState = function()
        {
            settings.saveState();
        };
        //endregion

        //region -- getInputClass, applyInputClass
        /**
         * Returns the input class based on the setting for inputWidth.
         * @returns {string}
         */
        this.getInputClass = function()
        {
            return that.getInputWidth();
        };

        /**
         * Applies an input class to a jQuery element based on the setting for inputWidth.
         * @param jqElement
         */
        this.applyInputClass = function(jqElement)
        {
            var inputClass = that.getInputClass();
            if(inputClass && jqElement) jqElement.addClass(inputClass);
        };
        //endregion

        //region -- hookEvents, unhookEvents
        /**
         * Calls the hookChanged event hooker and records the result.
         * @param callback
         * @param forceThis
         */
        this.hookEvents = function(callback, forceThis)
        {
            if(settings.hookChanged) _ChangedHookResult = settings.hookChanged(callback, forceThis);
        };

        /**
         * Calls the unhookChanged event hooker with the result from the previous hookChanged call.
         */
        this.unhookEvents = function()
        {
            if(settings.unhookChanged && _ChangedHookResult) {
                settings.unhookChanged(_ChangedHookResult);
                _ChangedHookResult = null;
            }
        };
        //endregion
    };
    //endregion

    //region OptionField subclasses
    //region OptionFieldButton
    /**
     * The settings that can be associated with a button control.
     * @param {object}  settings               Is actually optional but if you declare it as optional then WebStorm7 gets its knickers in a twist.
     * @param {string} [settings.icon]          The name of the jQuery UI icon to display on the button without the leading 'ui-icon-'.
     * @param {string} [settings.primaryIcon]   The name of the jQuery UI icon to display on the button when it isn't pressed, without the leading 'ui-icon-'.
     * @param {string} [settings.secondaryIcon] The name of the jQuery UI icon to display on the button when it is pressed, without the leading 'ui-icon'.
     * @param {bool}   [settings.showText]      True if the text is to be displayed, false if only the icon is to be displayed.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldButton = function(settings)
    {
        settings = $.extend({
            /** @type {string} */   dispatcherName: 'VRS.OptionFieldButton',
            /** @type {string} */   controlType: VRS.optionControlTypes.button,
            /** @type {string} */   icon: null,
            /** @type {string} */   primaryIcon: null,
            /** @type {string} */   secondaryIcon: null,
            /** @type {boolean} */  showText: true
        }, settings);

        VRS.OptionField.call(this, settings);
        var that = this;

        this.getPrimaryIcon = function() { return settings.primaryIcon || settings.icon; };
        this.getSecondaryIcon = function() { return settings.secondaryIcon; };
        this.getShowText = function() { return settings.showText; };

        var _Enabled = true;
        this.getEnabled = function() { return _Enabled; };
        this.setEnabled = function(/** bool */ value) {
            if(value !== _Enabled) {
                _Enabled = value;
                that.raiseRefreshFieldState();
            }
        };
    };
    VRS.OptionFieldButton.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldCheckBox
    /**
     * The options for a checkbox field.
     * @param {Object} settings
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldCheckBox = function(settings)
    {
        settings = $.extend({
            /** @type {string} */   dispatcherName: 'VRS.OptionFieldCheckBox',
            /** @type {string} */   controlType: VRS.optionControlTypes.checkBox
        }, settings);

        VRS.OptionField.call(this, settings);
    };
    VRS.OptionFieldCheckBox.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldColour
    /**
     * Describes the options for a colour field.
     * @param {Object} settings
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldColour = function(settings)
    {
        settings = $.extend({
            /** @type {string} */           dispatcherName: 'VRS.OptionFieldColour',
            /** @type {string} */           controlType: VRS.optionControlTypes.colour,
            /** @type {VRS.InputWidth} */   inputWidth: VRS.InputWidth.NineChar
        }, settings);
        VRS.OptionField.call(this, settings);
    };
    VRS.OptionFieldColour.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldComboBox
    /**
     * The settings that can be associated with a combo-box control.
     * @param {object}               settings           Is actually optional but if you declare it as optional then WebStorm7 gets its knickers in a twist.
     * @param {VRS.ValueText[]}     [settings.values]   The values to display in the drop-down. text values are displayed as-is, textKey is an index into VRS.$$ or a function that returns the translated text.
     * @param {function(?)}         [settings.changed]  Called whenever the combo box value changes. Gets passed the current value of the combo box.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldComboBox = function(settings)
    {
        settings = $.extend({
            /** @type {string} */           dispatcherName: 'VRS.OptionFieldComboBox',
            /** @type {string} */           controlType: VRS.optionControlTypes.comboBox,
            /** @type {VRS.ValueText[]} */  values: [],
            /** @type {function(*)} */      changed: $.noop
        }, settings);

        VRS.OptionField.call(this, settings);

        this.getValues = function() { return settings.values; };

        this.callChangedCallback = function(selectedValue)
        {
            settings.changed(selectedValue);
        };
    };
    VRS.OptionFieldComboBox.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldDate
    /**
     * The settings that can be associated with a date control.
     * @param {Object}  settings                        Is actually optional but if you declare it as optional then WebStorm7 gets in a huff.
     * @param {Date=}   settings.defaultDate            The default date to display in the control. Defaults to today.
     * @param {Date=}   settings.minDate                The earliest date that can be selected by the user. Leave undefined when there is no minimum date.
     * @param {Date=}   settings.maxDate                The latest date that can be selected by the user. Leave undefined when there is no maximum date.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldDate = function(settings)
    {
        settings = $.extend({
            /** @type {string} */           dispatcherName: 'VRS.OptionFieldDate',
            /** @type {string} */           controlType: VRS.optionControlTypes.date,
            /** @type {Date} */             defaultDate: null,
            /** @type {Date} */             minDate: null,
            /** @type {Date} */             maxDate: null
        }, settings);
        VRS.OptionField.call(this, settings);

        this.getDefaultDate = function() { return settings.defaultDate; };
        this.getMinDate = function() { return settings.minDate; };
        this.getMaxDate = function() { return settings.maxDate; };
    };
    VRS.OptionFieldDate.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldLabel
    /**
     * The settings that can be associated with a label control.
     * @param {object}           settings                Is actually optional but if you declare it as optional then WebStorm7 gets its knickers in a twist.
     * @param {VRS.LabelWidth}  [settings.labelWidth]    The width to assign to the label.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldLabel = function(settings)
    {
        settings = $.extend({
            /** @type {string} */           dispatcherName: 'VRS.OptionFieldLabel',
            /** @type {string} */           controlType: VRS.optionControlTypes.label,
            /** @type {VRS.LabelWidth} */   labelWidth: VRS.LabelWidth.Auto
        }, settings);
        VRS.OptionField.call(this, settings);

        this.getLabelWidth = function() { return settings.labelWidth; };
    };
    VRS.OptionFieldLabel.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldLinkLabel
    /**
     * The settings that can be associated with a link label control.
     * @param {object}              settings                Is actually optional but if you declare it as optional then WebStorm7 doesn't like it.
     * @param {function():string}   settings.getHref        A callback that returns the href for the link label. getValue returns the text to display for the href.
     * @param {function():string}  [settings.getTarget]     A callback that returns the target for the link. If missing or if the function returns null/undefined then no target is used.
     * @constructor
     * @extends {VRS.OptionFieldLabel}
     */
    VRS.OptionFieldLinkLabel = function(settings)
    {
        settings = $.extend({
            /** @type {string} */               dispatcherName: 'VRS.OptionFieldLinkLabel',
            /** @type {string} */               controlType: VRS.optionControlTypes.linkLabel,
            /** @type {function():string} */    getHref: $.noop,
            /** @type {function():string} */    getTarget: $.noop
        }, settings);
        VRS.OptionFieldLabel.call(this, settings);

        this.getHref = function() { return settings.getHref() || '#'; };
        this.getTarget = function() { return settings.getTarget() || null; };
    };
    VRS.OptionFieldLinkLabel.prototype = VRS.objectHelper.subclassOf(VRS.OptionFieldLabel);
    //endregion

    //region OptionFieldNumeric
    /**
     * The settings that can be associated with a number editor control.
     * @param {object}   settings               Is actually optional but if you declare it as optional then WebStorm7 gets its knickers in a twist.
     * @param {number}  [settings.min]          The smallest value that the user can enter.
     * @param {number}  [settings.max]          The largest value that the user can enter.
     * @param {number}  [settings.decimals]     The number of decimal places that will be shown to the user.
     * @param {number}  [settings.step]         The increment to use when automatically stepping values up or down.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldNumeric = function(settings)
    {
        settings = $.extend({
            /** @type {string} */       dispatcherName: 'VRS.OptionFieldNumeric',
            /** @type {string} */       controlType: VRS.optionControlTypes.numeric,
            /** @type {number} */       min: undefined,
            /** @type {number} */       max: undefined,
            /** @type {number} */       decimals: undefined,
            /** @type {number} */       step: 1,
            /** @type {boolean} */      showSlider: false,
            /** @type {number} */       sliderStep: undefined
        }, settings);
        VRS.OptionField.call(this, settings);

        this.getMin = function() { return settings.min; };
        this.getMax = function() { return settings.max; };
        this.getDecimals = function() { return settings.decimals; };
        this.getStep = function() { return settings.step; };
        this.showSlider = function() { return settings.showSlider; };
        this.getSliderStep = function() { return settings.sliderStep === undefined ? settings.step : settings.sliderStep; };
    };
    VRS.OptionFieldNumeric.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldOrderedSubset
    /**
     * The settings that can be associated with an editor that lets users choose a subset from a set of values.
     * @param {object}              settings                       Is actually optional but if you declare it as such then WebStorm7 gets its knickers in a twist.
     * @param {VRS.ValueText[]}    [settings.values]               The full set of values that the user can choose from.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldOrderedSubset = function(settings)
    {
        settings = $.extend({
            /** @type {string} */           dispatcherName: 'VRS.OptionFieldOrderedSubset',
            /** @type {string} */           controlType: VRS.optionControlTypes.orderedSubset,
            /** @type {VRS.ValueText[]} */  values: [],
            /** @type {boolean} */          keepValuesSorted: false
        }, settings);
        VRS.OptionField.call(this, settings);

        this.getValues = function() { return settings.values; };
        this.getKeepValuesSorted = function() { return settings.keepValuesSorted; };
    };
    VRS.OptionFieldOrderedSubset.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldPaneList
    /**
     * The settings that can be associated with an editor for an array of objects, where each object is edited via its own option pane.
     * @param {object}                               settings                           Is actually optional but if you declare it as optional then WebStorm7 gets its knickers in a twist.
     * @param {VRS.OptionPane[]}                    [settings.panes]                    The option panes to show to the user.
     * @param {number}                              [settings.maxPanes]                 The maximum number of panes (and by extension, items in the array being edited) that can be displayed / edited. Undefined or -1 indicates there is no maximum.
     * @param {VRS.OptionPane}                      [settings.addPane]                  An option pane containing the controls that can be used to add a new pane (and therefore a new array item).
     * @param {bool}                                [settings.suppressRemoveButton]     True if the UI to remove individual items is not shown to the user. Defaults to false.
     * @param {function(bool, jQuery)}              [settings.refreshAddControls]       An optional method that it called to disable the add controls. Passed true if the user has reached maximum number of panes. Also passed the jQuery element of the parent of the add pane. If not supplied then all input and button controls in the add pane are disabled or enabled.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldPaneList = function(settings)
    {
        settings = $.extend({
            /** @type {string} */                   dispatcherName: 'VRS.OptionFieldPaneList',
            /** @type {string} */                   controlType: VRS.optionControlTypes.paneList,
            /** @type {VRS.OptionPane[]} */         panes: [],
            /** @type {number} */                   maxPanes: -1,
            /** @type {VRS.OptionPane} */           addPane: null,
            /** @type {boolean} */                  suppressRemoveButton: false,
            /** @type {function(bool, jQuery)} */   refreshAddControls: function(/** bool */ disabled, /** jQuery */ addParentJQ) {
                $(':input', addParentJQ).prop('disabled', disabled);
                $(':button', addParentJQ).button('option', 'disabled', disabled);
            }
        }, settings);
        VRS.OptionField.call(this, settings);

        //region -- Fields
        var that = this;
        var _Dispatcher = this.getDispatcher();
        var _Events = {
            paneAdded: 'paneAdded',
            paneRemoved: 'paneRemoved',
            maxPanesChanged: 'maxPanesChanged'
        };
        //endregion

        //region -- Properties
        this.getMaxPanes = function() { return settings.maxPanes; };
        this.setMaxPanes = function(/** number */ value) {
            if(value !== settings.maxPanes) {
                settings.maxPanes = value;
                trimExcessPanes();
                onMaxPanesChanged();
            }
        };

        this.getPanes = function() { return settings.panes; };

        this.getAddPane = function() { return settings.addPane; };
        this.setAddPane = function(/** VRS.OptionPane */ value) { settings.addPane = value; };

        this.getSuppressRemoveButton = function() { return settings.suppressRemoveButton; };
        this.setSuppressRemoveButton = function(/** boolean */ value) { settings.suppressRemoveButton = value; };

        this.getRefreshAddControls = function() { return settings.refreshAddControls; };
        this.setRefreshAddControls = function(/** function(bool, jQuery) */ value) { settings.refreshAddControls = value; };
        //endregion

        //region -- Events
        /**
         * Raised when the user adds a new pane.
         * @param {function(VRS.OptionPane, number)}    callback    Passed the new pane and its index in the array. The listener is expected to add the associated array item.
         * @param {object}                              forceThis   The object to use as 'this' when calling the callback.
         * @returns {object}
         */
        this.hookPaneAdded = function(callback, forceThis) { return _Dispatcher.hook(_Events.paneAdded, callback, forceThis); };

        /**
         * Raised when the user removes an existing pane.
         * @param {function(VRS.OptionPane, number)}    callback    Passed the pane that was removed and its index. The listener is expected to remove the associated array item.
         * @param {object}                              forceThis   The object to use as 'this' when calling the callback.
         * @returns {object}
         */
        this.hookPaneRemoved = function(callback, forceThis) { return _Dispatcher.hook(_Events.paneRemoved, callback, forceThis); };

        /**
         * Raised after the maximum number of panes has changed.
         * @param {function()}  callback
         * @param {object}      forceThis
         * @returns {{}}
         */
        this.hookMaxPanesChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.maxPanesChanged, callback, forceThis); };

        function onPaneAdded(/** VRS.OptionPane */ pane, /** number */ index)   { _Dispatcher.raise(_Events.paneAdded, [ pane, index ]); }
        function onPaneRemoved(/** VRS.OptionPane */ pane, /** number */ index) { _Dispatcher.raise(_Events.paneRemoved, [ pane, index]); }
        function onMaxPanesChanged()                                            { _Dispatcher.raise(_Events.maxPanesChanged); }
        //endregion

        //region -- addPane, removePane, trimExcessPanes, findPaneIndex, removePaneAt
        /**
         * Adds a new pane to the array at the index specified.
         * @param {VRS.OptionPane} pane
         * @param {number} [index]
         */
        this.addPane = function(pane, index)
        {
            if(index !== undefined) {
                settings.panes.splice(index, 0, pane);
                onPaneAdded(pane, index);
            } else {
                settings.panes.push(pane);
                onPaneAdded(pane, settings.panes.length - 1);
            }
        };

        /**
         * Removes the pane from the array.
         * @param {VRS.OptionPane} pane
         */
        this.removePane = function(pane)
        {
            var index = findPaneIndex(pane);
            if(index === -1) throw 'Cannot find the pane to remove';
            this.removePaneAt(index);
        };

        /**
         * Removes panes until the count of panes no longer exceeds the maximum allowed.
         */
        function trimExcessPanes()
        {
            if(settings.maxPanes !== -1) {
                while(settings.maxPanes > settings.panes.length) {
                    that.removePane(settings.panes[settings.panes.length - 1]);
                }
            }
        }

        /**
         * Returns the index of the pane in the array or -1 if the pane is not in the arrary.
         * @param {VRS.OptionPane} pane
         * @returns {number}
         */
        function findPaneIndex(pane)
        {
            var result = -1;
            var length = settings.panes.length;
            for(var i = 0;i < length;++i) {
                if(settings.panes[i] === pane) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Removes the pane at the index specified.
         * @param {number} index
         */
        this.removePaneAt = function(index)
        {
            var pane = settings.panes[index];
            settings.panes.splice(index, 1);
            pane.dispose();
            onPaneRemoved(pane, index);
        };
        //endregion
    };
    VRS.OptionFieldPaneList.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldRadioButton
    /**
     * The settings that can be associated with a radio button group control.
     * @param {object}               settings           Is actually optional but if you declare it as optional then WebStorm7 gets its knickers in a twist.
     * @param {VRS.ValueText[]}     [settings.values]   The values for each of the radio buttons to show to the user.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldRadioButton = function(settings)
    {
        settings = $.extend({
            /** @type {string} */                   dispatcherName: 'VRS.OptionFieldRadioButton',
            /** @type {string} */                   controlType: VRS.optionControlTypes.radioButton,
            /** @type {VRS.ValueText[]} */          values: []
        }, settings);
        VRS.OptionField.call(this, settings);

        this.getValues = function() { return settings.values; };
    };
    VRS.OptionFieldRadioButton.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion

    //region OptionFieldTextBox
    /**
     * The settings that can be associated with an editor that can display and edit strings.
     * @param {object}   settings                   Is actually optional but if you declare it as optional then WebStorm7 gets its knickers in a twist.
     * @param {bool}    [settings.upperCase]        True if the text is to be forced into upper-case.
     * @param {bool}    [settings.lowerCase]        True if the text is to be forced into lower-case.
     * @param {number}  [settings.maxLength]        The maximum number of characters allowed in the box.
     * @constructor
     * @extends VRS.OptionField
     */
    VRS.OptionFieldTextBox = function(settings)
    {
        settings = $.extend({
            /** @type {string} */   dispatcherName: 'VRS.OptionFieldTextBox',
            /** @type {string} */   controlType: VRS.optionControlTypes.textBox,
            /** @type {boolean} */  upperCase: false,
            /** @type {boolean} */  lowerCase: false,
            /** @type {number} */   maxLength: undefined
        }, settings);
        VRS.OptionField.call(this, settings);

        this.getUpperCase = function() { return settings.upperCase; };
        this.getLowerCase = function() { return settings.lowerCase; };
        this.getMaxLength = function() { return settings.maxLength; };
    };
    VRS.OptionFieldTextBox.prototype = VRS.objectHelper.subclassOf(VRS.OptionField);
    //endregion
    //endregion

    //region OptionPane
    /**
     * Describes a collection of fields to present together on a page in the configuration UI.
     * @param {object}                          settings
     * @param {string}                          settings.name               The name of the pane.
     * @param {string|function():string}       [settings.titleKey]          The index into VRS.$$ or a method that returns a string. Used as the title for the pane on the page.
     * @param {number}                         [settings.displayOrder]      The relative display order of the pane. Lower display orders appear before higher display orders.
     * @param {VRS.OptionField[]}              [settings.fields]            The initial set of fields on the pane.
     * @param {function(Object)}               [settings.dispose]           A method to call when the pane is disposed of. Passed an object carrying the OptionPane and the OptionPageParent.
     * @param {function(VRS.OptionPageParent)} [settings.pageParentCreated] A method to call when a page parent is created.
     * @constructor
     */
    VRS.OptionPane = function(settings)
    {
        var that = this;

        if(!settings) throw 'You must supply settings';
        if(!settings.name) throw 'You must supply a name for the pane';
        settings = $.extend({
            /** @type {string} */                           name: null,
            /** @type {string|function():string} */         titleKey: null,
            /** @type {number} */                           displayOrder: 0,
            /** @type {VRS.OptionField[]} */                fields: [],
            /** @type {function(Object)} */                 dispose: $.noop,
            /** @type {function(VRS.OptionPageParent)} */   pageParentCreated: $.noop
        }, settings);

        //region -- Fields
        /**
         * The array of fields that the pane contains.
         * @type {Array.<VRS.OptionField>}
         * @private
         */
        var _OptionFields = [];

        /**
         * A value that is incremented any time a field is added to or removed from the pane.
         * @type {number}
         * @private
         */
        var _Generation = 0;
        //endregion

        //region -- Properties
        this.getName = function() { return settings.name; };

        this.getTitleKey = function() { return settings.titleKey; };
        this.getTitleText = function() { return VRS.globalisation.getText(settings.titleKey); };
        this.setTitleKey = function(/** string|function():string */ value)  { settings.titleKey = value; };

        this.getDisplayOrder = function() { return settings.displayOrder; };
        this.setDisplayOrder = function(/** number */ value)  { settings.displayOrder = value; };

        this.getFieldCount = function() { return _OptionFields.length; };
        /** @type {VRS.OptionField} */  this.getField = function(/** number */ idx) { return _OptionFields[idx]; };
        /** @type {VRS.OptionField} */
        this.getFieldByName = function(/** string */ optionFieldName) {
            var index = findIndexByName(optionFieldName);
            return index === -1 ? null : that.getField(index);
        };
        //endregion

        //region -- dispose, pageParentCreated
        /**
         * Disposes of the pane.
         * @param {{
         * optionPane:          VRS.OptionPane,
         * optionPageParent:    VRS.OptionPageParent
         * }} options
         */
        this.dispose = function(options)
        {
            settings.dispose(options);
        };

        /**
         * Invokes a callback, registered when the pane was created, that the pane is being rendered into a page and
         * passes the callback the VRS.OptionPageParent that will live for the duration of the render.
         * @param {VRS.OptionPageParent} optionPageParent
         */
        this.pageParentCreated = function(optionPageParent)
        {
            settings.pageParentCreated(optionPageParent);
        };
        //endregion

        //region -- addField, removeFieldByName, foreachField
        /**
         * Adds a field to the pane. Throws if the field already exists.
         * @param {VRS.OptionField} optionField
         */
        this.addField = function(optionField)
        {
            /// <summary>Adds the field to the list of option fields to display to the user.</summary>
            var existingIndex = findIndexByName(optionField.getName());
            if(existingIndex !== -1) throw 'There is already a field in this pane called ' + optionField.getName();
            _OptionFields.push(optionField);
            ++_Generation;
        };

        /**
         * Removes a field from the pane by its name. Throws if the field does not exist.
         * @param {string} optionFieldName
         */
        this.removeFieldByName = function(optionFieldName)
        {
            var index = findIndexByName(optionFieldName);
            if(index === -1) throw 'Cannot remove option field ' + optionFieldName + ', it does not exist.';
            _OptionFields.splice(index, 1);
            ++_Generation;
        };

        /**
         * Passes every field on the pane to a callback method. The callback is not allowed to add or remove fields from the pane.
         * @param {function(VRS.OptionField)} callback
         */
        this.foreachField = function(callback)
        {
            /// <summary>Loops through each field in the order that it was added to the pane and passes it to the callback.
            var length = _OptionFields.length;
            //noinspection UnnecessaryLocalVariableJS
            var generation = _Generation;       // <-- not redundant, need to ensure that callback doesn't mess with fields
            for(var i = 0;i < length;++i) {
                callback(_OptionFields[i]);
                if(_Generation !== generation) throw 'Cannot continue to iterate through the fields after the collection has been modified';
            }
        };

        /**
         * Returns the index of the named field or -1 if there is no field with that name.
         * @param {string} optionFieldName
         * @returns {number}
         */
        function findIndexByName(optionFieldName)
        {
            var result = -1;
            $.each(_OptionFields, function(idx, val) {
                var breakLoop = val.getName() === optionFieldName;
                if(breakLoop) result = idx;
                return !breakLoop;
            });

            return result;
        }
        //endregion

        //region -- Initialisation
        if(settings.fields) {
            for(var i = 0;i < settings.fields.length;++i) this.addField(settings.fields[i]);
        }
        //endregion
    };
    //endregion

    //region OptionPage
    /**
     * Describes a page of panes in the configuration UI.
     * @param {object}              settings
     * @param {string}              settings.name           The unique name of the page.
     * @param {number}             [settings.displayOrder]  The relative order of the page within a group of pages. Pages with a lower display order come before those with a higher order.
     * @param {VRS.OptionPane[]}   [settings.panes]         The initial set of panes.
     * @constructor
     */
    VRS.OptionPage = function(settings)
    {
        if(!settings) throw 'You must supply settings';
        if(!settings.name) throw 'You must give the page a name';

        //region -- Fields
        /**
         * The panes held by the page.
         * @type {Array.<VRS.OptionPane>}
         * @private
         */
        var _OptionPanes = [];

        /**
         * A value that is incremented every time a pane is added to or removed from the page.
         * @type {number}
         * @private
         */
        var _Generation = 0;

        /**
         * The generation number as-at the last time the page was sorted. Used to optimise the sort method.
         * @type {number}
         * @private
         */
        var _SortGeneration = -1;
        //endregion

        //region -- Properties
        /** @type {string} @private */
        var _Name = settings.name;
        this.getName = function()        { return _Name; };
        //noinspection JSUnusedGlobalSymbols
        this.setName = function(/** string */ value)   { _Name = value; };

        /** @type {string} @private */
        var _TitleKey = settings.titleKey;
        this.getTitleKey = function()       { return _TitleKey; };
        //noinspection JSUnusedGlobalSymbols
        this.setTitleKey = function(/** string */ value)  { _TitleKey = value; };

        /** @type {number} @private */
        var _DisplayOrder = settings.displayOrder;
        this.getDisplayOrder = function()       { return _DisplayOrder; };
        //noinspection JSUnusedGlobalSymbols
        this.setDisplayOrder = function(/** number */ value)  { if(!isNaN(value)) _DisplayOrder = value; };
        //endregion

        //region -- addPane, removePaneByName, foreachPane
        /**
         * Adds a pane to the page. If there is already a pane on the page with the same name then an exception is thrown.
         * @param {VRS.OptionPane|Array.<VRS.OptionPane>} optionPane
         */
        this.addPane = function(optionPane)
        {
            if(!(optionPane instanceof VRS.OptionPane)) {
                var length = optionPane.length;
                for(var i = 0;i < length;++i) {
                    this.addPane(optionPane[i]);
                }
            } else {
                var index = findIndexByName(optionPane.getName());
                if(index !== -1) throw 'There is already a pane on this page called ' + optionPane.getName();
                _OptionPanes.push(optionPane);
                ++_Generation;
            }
        };

        //noinspection JSUnusedGlobalSymbols
        /**
         * Removes the pane with the name passed across. If there is no pane on the page with this name then an exception is thrown.
         * @param {string} optionPaneName
         */
        this.removePaneByName = function(optionPaneName)
        {
            var index = findIndexByName(optionPaneName);
            if(index === -1) throw 'There is no pane called ' + optionPaneName;
            _OptionPanes.splice(index, 1);
            ++_Generation;
        };

        /**
         * Passes each pane on the page to a callback method, one at a time. The callback must not add or remove panes on the page.
         * @param {function(VRS.OptionPane)} callback
         */
        this.foreachPane = function(callback)
        {
            sortPanes();
            //noinspection UnnecessaryLocalVariableJS
            var generation = _Generation;           // <-- not redundant, need to ensure callback doesn't mess with panes
            var length = _OptionPanes.length;
            for(var i = 0;i < length;++i) {
                callback(_OptionPanes[i]);
                if(_Generation != generation) throw 'Cannot continue to iterate through the panes, they have been changed';
            }
        };

        /**
         * Returns the index of a pane or -1 if there is no pane with the name passed across.
         * @param {string} optionPaneName
         * @returns {number}
         */
        function findIndexByName(optionPaneName)
        {
            var result = -1;
            $.each(_OptionPanes, function(idx, val) {
                var foundMatch = val.getName() === optionPaneName;
                if(foundMatch) result = idx;
                return !foundMatch;
            });

            return result;
        }

        /**
         * Sorts panes into display order.
         */
        function sortPanes()
        {
            if(_SortGeneration !== _Generation) {
                _OptionPanes.sort(function(lhs, rhs) { return lhs.getDisplayOrder() - rhs.getDisplayOrder(); });
                _SortGeneration = _Generation;
            }
        }
        //endregion

        //region -- Initialisation
        if(settings.panes) {
            for(var i = 0;i < settings.panes.length;++i) this.addPane(settings.panes[i]);
        }
        //endregion
    };
    //endregion

    //region OptionPageParent
    /**
     * An object that exposes events across all of the options in a group of pages.
     * @constructor
     */
    VRS.OptionPageParent = function()
    {
        //region -- Fields
        var _Dispatcher = new VRS.EventHandler({ name: 'VRS.OptionPageParent' });
        var _Events = {
            fieldChanged: 'fieldChanged'
        };
        //endregion

        //region -- Events exposed
        /**
         * Hooks an event that is raised whenever a field changes on the object.
         * @param {function()}  callback
         * @param {Object}     [forceThis]
         * @returns {Object}
         */
        this.hookFieldChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.fieldChanged, callback, forceThis); };

        /**
         * Raises the fieldChanged event.
         */
        this.raiseFieldChanged = function() { _Dispatcher.raise(_Events.fieldChanged); };

        /**
         * Unhooks an event previously hooked.
         * @param hookResult
         */
        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion
    };
    //endregion

    //region OptionControlTypeBroker
    /**
     * An object that can translate between control types and the jQuery UI plugin that handles the control.
     * @constructor
     */
    VRS.OptionControlTypeBroker = function()
    {
        //region -- Fields
        /**
         * The array of control types and the method to use to create the appropriate jQuery UI plugin.
         * @type {Object.<VRS.optionControlTypes, function(VRS_OPTION_FIELD_SETTINGS):jQuery>}
         * @private
         */
        var _ControlTypes = {};
        //endregion

        //region -- addControlTypeHandler, removeControlTypeHandler, controlTypeHasHandler
        /**
         * Adds a handler for a control type handler. Throws an exception if there is already a handler for the control type.
         * @param {string}                                      controlType         The VRS.optionControlTypes control type to register.
         * @param {function(VRS_OPTION_FIELD_SETTINGS):jQuery}  creatorCallback     A method that takes a parent element and creates the control for it.
         */
        this.addControlTypeHandler = function(controlType, creatorCallback)
        {
            if(_ControlTypes[controlType]) throw 'There is already a handler registered for ' + controlType + ' control types';
            _ControlTypes[controlType] = creatorCallback;
        };

        /**
         * Adds a handler for a control type, but only if one is not already registered. Does nothing if one is already registered.
         * @param {string}                                      controlType         The VRS.optionControlTypes control type to register.
         * @param {function(VRS_OPTION_FIELD_SETTINGS):jQuery}  creatorCallback     A method that takes a parent element and creates the control for it.
         */
        this.addControlTypeHandlerIfNotRegistered = function(controlType, creatorCallback)
        {
            if(!this.controlTypeHasHandler(controlType)) this.addControlTypeHandler(controlType, creatorCallback);
        };

        //noinspection JSUnusedGlobalSymbols
        /**
         * Removes the handler for a control type. Throws an exception if the control type does not have a handler registered.
         * @param {string} controlType The VRS.optionControlTypes control type to remove.
         */
        this.removeControlTypeHandler = function(controlType)
        {
            if(!_ControlTypes[controlType]) throw 'There is no handler registered for ' + controlType + ' control types';
            delete _ControlTypes[controlType];
        };

        /**
         * Returns true if the control type has a handler registered for it, false if it does not.
         * @param {string} controlType The VRS.optionControlTypes control type to test for.
         * @returns {boolean}
         */
        this.controlTypeHasHandler = function(controlType)
        {
            return !!_ControlTypes[controlType];
        };

        /**
         * Creates the jQuery UI plugin that will manage the display and editing of an option field.
         * @param {VRS_OPTION_FIELD_SETTINGS}   settings        The settings to use when creating the field.
         * @returns {jQuery}
         */
        this.createControlTypeHandler = function(settings)
        {
            var controlType = settings.field.getControlType();
            var creator = _ControlTypes[controlType];
            if(!creator) throw 'There is no handler registered for ' + controlType + ' control types';

            return creator(settings);
        };
        //endregion
    };
    //endregion

    //region Pre-builts
    VRS.optionControlTypeBroker = new VRS.OptionControlTypeBroker();
    //endregion

    //region Standard VRS control types
    /**
     * An modifiable enumeration of the different control types. 3rd parties can add their own control types to this.
     * @enum {string}
     */
    VRS.optionControlTypes = VRS.optionControlTypes || {};
    VRS.optionControlTypes.button = 'vrsButton';
    VRS.optionControlTypes.checkBox = 'vrsCheckBox';
    VRS.optionControlTypes.colour = 'vrsColour';
    VRS.optionControlTypes.comboBox = 'vrsComboBox';
    VRS.optionControlTypes.date = 'vrsDate';
    VRS.optionControlTypes.label = 'vrsLabel';
    VRS.optionControlTypes.linkLabel = 'vrsLinkLabel';
    VRS.optionControlTypes.numeric = 'vrsNumeric';
    VRS.optionControlTypes.orderedSubset = 'vrsOrderedSubset';
    VRS.optionControlTypes.paneList = 'vrsPaneList';
    VRS.optionControlTypes.radioButton = 'vrsRadioButton';
    VRS.optionControlTypes.textBox = 'vrsTextBox';
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
