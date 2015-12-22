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

namespace VRS
{
    /**
     * The settings used to create a new OptionField.
     */
    export interface OptionField_Settings
    {
        /**
         * The unique name of the field.
         */
        name: string;

        /**
         * The name to use for the event dispatcher. For internal use.
         */
        dispatcherName?: string;

        /**
         * The VRS.optionsControlType enum value that determines which control will be used to display and/or edit this field.
         */
        controlType?: OptionControlTypesEnum;

        /**
         * The index into VRS.$$ of the label to use when displaying the field or a function that returns the translated text.
         */
        labelKey?: string | VoidFuncReturning<string>;

        /**
         * A callback that returns the value to display / edit from the object being configured.
         */
        getValue?: () => any;

        /**
         * A callback that is passed the edited value. It is expected to copy the value into the object being configured.
         */
        setValue?: (value: any) => void;

        /**
         * A callback that can save the current state of the object being configured.
         */
        saveState?: () => void;

        /**
         * If true then the field is to be displayed next to the field that follows it.
         */
        keepWithNext?: boolean;

        /**
         * A hook function that can be used to pick up changes to the value that is returned by getValue - for fields whose value may change while the configuration UI is on display.
         */
        hookChanged?: (callback: Function, forceThis?: Object) => any;

        /**
         * An unhook function that can unhook the event hooked by hookChanged.
         */
        unhookChanged?: (eventHandle: IEventHandle | IEventHandleJQueryUI) => void;

        /**
         * An indication of how wide the input field needs to be.
         */
        inputWidth?: InputWidthEnum;

        /**
         * Indicates whether the field should be visible to the user.
         */
        visible?: boolean | VoidFuncReturning<boolean>;
    }

    /**
     * Describes a single editable option - there is usually one of these for each configurable property on an object.
     */
    export class OptionField
    {
        // Events
        protected _Dispatcher: EventHandler;
        private _Events = {
            refreshFieldContent:            'refreshFieldContent',
            refreshFieldState:              'refreshFieldState',
            refreshFieldVisibility:         'refreshFieldVisibility'
        };

        protected _Settings: OptionField_Settings;

        constructor(settings: OptionField_Settings)
        {
            if(!settings) throw 'You must supply settings';
            if(!settings.name) throw 'You must supply a name for the field';
            if(!settings.controlType) throw 'You must supply the field\'s control type';
            if(!VRS.optionControlTypeBroker.controlTypeHasHandler(settings.controlType)) throw 'There is no control type handler for ' + settings.controlType;

            settings = $.extend({
                name: null,
                dispatcherName: 'VRS.OptionField',          // Filled in by derivees to get a better name for the dispatcher
                controlType: null,
                labelKey: '',
                getValue: $.noop,
                setValue: $.noop,
                saveState: $.noop,
                keepWithNext: false,
                hookChanged: null,
                unhookChanged: null,
                inputWidth: VRS.InputWidth.Auto,
                visible: true
            }, settings);

            this._Settings = settings;
            this._Dispatcher = new VRS.EventHandler({ name: settings.dispatcherName });
        }

        private _ChangedHookResult: IEventHandle | IEventHandleJQueryUI = null;

        getName() : string
        {
            return this._Settings.name;
        }

        getControlType() : OptionControlTypesEnum
        {
            return this._Settings.controlType;
        }

        getKeepWithNext() : boolean
        {
            return this._Settings.keepWithNext;
        }

        setKeepWithNext(value: boolean)
        {
            this._Settings.keepWithNext = value;
        }

        getLabelKey() : string | VoidFuncReturning<string>
        {
            return this._Settings.labelKey;
        }

        getLabelText() : string
        {
            return VRS.globalisation.getText(this._Settings.labelKey);
        }

        getInputWidth() : InputWidthEnum
        {
            return this._Settings.inputWidth;
        }

        getVisible() : boolean
        {
            return VRS.Utility.ValueOrFuncReturningValue(this._Settings.visible, true);
        }

        /**
         * Hooks an event that is raised when something wants the content of the field to be refreshed. Not all fields respond to this event.
         */
        hookRefreshFieldContent(callback: () => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.refreshFieldContent, callback, forceThis);
        }
        raiseRefreshFieldContent()
        {
            this._Dispatcher.raise(this._Events.refreshFieldContent);
        }

        /**
         * Hooks an event that is raised when something wants the enabled / disabled state of the field to be refreshed. Not all fields response to this event.
         */
        hookRefreshFieldState(callback: () => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.refreshFieldState, callback, forceThis);
        }
        raiseRefreshFieldState()
        {
            this._Dispatcher.raise(this._Events.refreshFieldState);
        }

        /**
         * Hooks an event that is raised when the visibility of the field should be refreshed.
         */
        hookRefreshFieldVisibility(callback: () => void, forceThis?: Object)
        {
            return this._Dispatcher.hook(this._Events.refreshFieldVisibility, callback, forceThis);
        }
        raiseRefreshFieldVisibility()
        {
            this._Dispatcher.raise(this._Events.refreshFieldVisibility);
        }

        /**
         * Unhooks a previously hooked event on the field.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Calls the getValue method and returns the result.
         */
        getValue() : any
        {
            return this._Settings.getValue();
        }

        /**
         * Calls the setValue method with the value passed across.
         */
        setValue(value: any)
        {
            this._Settings.setValue(value);
        }

        /**
         * Calls the saveState method.
         */
        saveState()
        {
            this._Settings.saveState();
        }

        /**
         * Returns the input class based on the setting for inputWidth.
         */
        getInputClass()
        {
            return this.getInputWidth();
        }

        /**
         * Applies an input class to a jQuery element based on the setting for inputWidth.
         */
        applyInputClass(jqElement: JQuery)
        {
            var inputClass = this.getInputClass();
            if(inputClass && jqElement) {
                jqElement.addClass(inputClass);
            }
        }

        /**
         * Calls the hookChanged event hooker and records the result.
         */
        hookEvents(callback: () => void, forceThis?: Object)
        {
            if(this._Settings.hookChanged) {
                this._ChangedHookResult = this._Settings.hookChanged(callback, forceThis);
            }
        }

        /**
         * Calls the unhookChanged event hooker with the result from the previous hookChanged call.
         */
        unhookEvents()
        {
            if(this._Settings.unhookChanged && this._ChangedHookResult) {
                this._Settings.unhookChanged(this._ChangedHookResult);
                this._ChangedHookResult = null;
            }
        }
    }


    /**
     * The settings to pass when creating an OptionFieldButton.
     */
    export interface OptionFieldButton_Settings extends OptionField_Settings
    {
        /**
         * The name of the jQuery UI icon to display on the button without the leading 'ui-icon-'.
         */
        icon?:  string;

        /**
         * The name of the jQuery UI icon to display on the button when it isn't pressed, without the leading 'ui-icon-'.
         */
        primaryIcon?: string;

        /**
         * The name of the jQuery UI icon to display on the button when it is pressed, without the leading 'ui-icon'.
         */
        secondaryIcon?: string;

        /**
         * True if the text is to be displayed, false if only the icon is to be displayed.
         */
        showText?: boolean;
    }

    /**
     * Describes an option field that shows a button to the user.
     */
    export class OptionFieldButton extends OptionField
    {
        protected _Settings: OptionFieldButton_Settings;
        private _Enabled: boolean;

        constructor(settings: OptionFieldButton_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldButton',
                controlType: VRS.optionControlTypes.button,
                icon: null,
                primaryIcon: null,
                secondaryIcon: null,
                showText: true
            }, settings);
            this._Enabled = true;

            super(settings);
        }

        getPrimaryIcon() : string
        {
            return this._Settings.primaryIcon || this._Settings.icon;
        }

        getSecondaryIcon() : string
        {
            return this._Settings.secondaryIcon;
        }

        getShowText() : boolean
        {
            return this._Settings.showText;
        }

        getEnabled() : boolean
        {
            return this._Enabled;
        }
        setEnabled(value: boolean)
        {
            if(value !== this._Enabled) {
                this._Enabled = value;
                this.raiseRefreshFieldState();
            }
        }
    }

    /**
     * The settings that need to be passed when creating new OptionFieldCheckBox objects.
     */
    export interface OptionFieldCheckBox_Settings extends OptionField_Settings
    {
    }

    /**
     * The options for a checkbox field.
     */
    export class OptionFieldCheckBox extends OptionField
    {
        constructor(settings: OptionFieldCheckBox_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldCheckBox',
                controlType: VRS.optionControlTypes.checkBox
            }, settings);
            super(settings);
        }
    }

    /**
     * The settings used to create an OptionsFieldColour.
     */
    export interface OptionFieldColour_Settings extends OptionField_Settings
    {
    }

    /**
     * Describes the options for a colour picker field.
     */
    export class OptionFieldColour extends OptionField
    {
        constructor(settings: OptionFieldColour_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldColour',
                controlType: VRS.optionControlTypes.colour,
                inputWidth: VRS.InputWidth.NineChar
            }, settings);
            super(settings);
        }
    }

    /**
     * The settings used when creating an OptionsFieldComboBox.
     */
    export interface OptionFieldComboBox_Settings extends OptionField_Settings
    {
        /**
         * The values to display in the drop-down. text values are displayed as-is, textKey is an index into VRS.$$ or a function that returns the translated text.
         */
        values?: ValueText[];

        /**
         * Called whenever the combo box value changes, gets passed the current value of the combo box.
         */
        changed?: (any) => void;
    }

    /**
     * The settings that can be associated with a combo-box control.
     */
    export class OptionFieldComboBox extends OptionField
    {
        protected _Settings: OptionFieldComboBox_Settings;

        constructor(settings: OptionFieldComboBox_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldComboBox',
                controlType: VRS.optionControlTypes.comboBox,
                values: [],
                changed: $.noop
            }, settings);
            super(settings);
        }

        /**
         * Gets the values and associated identifiers to show in the dropdown list.
         */
        getValues() : ValueText[]
        {
            return this._Settings.values;
        }

        /**
         * Calls the changed value callback, if supplied.
         */
        callChangedCallback(selectedValue: any)
        {
            this._Settings.changed(selectedValue);
        }
    }

    /**
     * The settings to pass when creating a new instance of OptionFieldDate.
     */
    export interface OptionFieldDate_Settings extends OptionField_Settings
    {
        /**
         * The default date to display in the control. Defaults to today.
         */
        defaultDate?: Date;

        /**
         * The earliest date that can be selected by the user. Leave undefined when there is no minimum date.
         */
        minDate?: Date;

        /**
         * The latest date that can be selected by the user. Leave undefined when there is no maximum date.
         */
        maxDate?: Date;
    }

    /**
     * Describes the options for a date field.
     */
    export class OptionFieldDate extends OptionField
    {
        protected _Settings: OptionFieldDate_Settings;

        constructor(settings: OptionFieldDate_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldDate',
                controlType: VRS.optionControlTypes.date,
                defaultDate: null,
                minDate: null,
                maxDate: null
            }, settings);
            super(settings);
        }

        getDefaultDate() : Date
        {
            return this._Settings.defaultDate;
        }

        getMinDate() : Date
        {
            return this._Settings.minDate;
        }

        getMaxDate() : Date
        {
            return this._Settings.maxDate;
        }
    }

    /**
     * The settings that need to be passed when creating a new instance of OptionFieldLabel.
     */
    export interface OptionFieldLabel_Settings extends OptionField_Settings
    {
        /**
         * The width to assign to the label.
         */
        labelWidth?: LabelWidthEnum;
    }

    /**
     * The settings that can be associated with a label control.
     */
    export class OptionFieldLabel extends OptionField
    {
        protected _Settings: OptionFieldLabel_Settings;

        constructor(settings: OptionFieldLabel_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldLabel',
                controlType: VRS.optionControlTypes.label,
                labelWidth: VRS.LabelWidth.Auto
            }, settings);
            super(settings);
        }

        getLabelWidth() : LabelWidthEnum
        {
            return this._Settings.labelWidth;
        }
    }

    /**
     * The settings to pass when creating a new OptionFieldLinkLabel.
     */
    export interface OptionFieldLinkLabel_Settings extends OptionField_Settings
    {
        /**
         * A callback that returns the href for the link label. getValue() returns the text to display for the href.
         */
        getHref?: () => string;

        /**
         * A callback that returns the target for the link. If missing or if the function returns null/undefined then no target is used.
         */
        getTarget?: () => string;
    }

    /**
     * The settings that can be associated with a link label control.
     */
    export class OptionFieldLinkLabel extends OptionFieldLabel
    {
        protected _Settings: OptionFieldLinkLabel_Settings;

        constructor(settings: OptionFieldLinkLabel_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldLinkLabel',
                controlType: VRS.optionControlTypes.linkLabel,
                getHref: $.noop,
                getTarget: $.noop
            }, settings);
            super(settings);
        }

        getHref() : string
        {
            return this._Settings.getHref() || '#';
        }

        getTarget() : string
        {
            return this._Settings.getTarget() || null;
        }
    }

    /**
     * The settings to pass when creating new instances of OptionFieldNumeric.
     */
    export interface OptionFieldNumeric_Settings extends OptionField_Settings
    {
        /**
         * The smallest value that the user can enter.
         */
        min?: number;

        /**
         * The largest value that the user can enter.
         */
        max?: number;

        /**
         * The number of decimal places that will be shown to the user.
         */
        decimals?: number;

        /**
         * The increment to use when automatically stepping values up or down.
         */
        step?: number;

        /**
         * True if a slider control should be shown rather than a numeric control.
         */
        showSlider?: boolean;

        /**
         * The increment to use when paging on the slider control. If undefined then step is used instead.
         */
        sliderStep?: number;
    }

    /**
     * The settings that can be associated with a number editor control.
     */
    export class OptionFieldNumeric extends OptionField
    {
        protected _Settings: OptionFieldNumeric_Settings;

        constructor(settings: OptionFieldNumeric_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldNumeric',
                controlType: VRS.optionControlTypes.numeric,
                min: undefined,
                max: undefined,
                decimals: undefined,
                step: 1,
                showSlider: false,
                sliderStep: undefined
            }, settings);
            super(settings);
        }

        getMin() : number
        {
            return this._Settings.min;
        }

        getMax() : number
        {
            return this._Settings.max;
        }

        getDecimals() : number
        {
            return this._Settings.decimals;
        }

        getStep() : number
        {
            return this._Settings.step;
        }

        showSlider() : boolean
        {
            return this._Settings.showSlider;
        }

        getSliderStep() : number
        {
            return this._Settings.sliderStep === undefined ? this._Settings.step : this._Settings.sliderStep;
        }
    }

    /**
     * The settings to pass when creating new instances of OptionFieldOrderedSubset.
     */
    export interface OptionFieldOrderedSubset_Settings extends OptionField_Settings
    {
        /**
         * The set of values that the user can choose from.
         */
        values?: ValueText[];

        /**
         * True if the set of options that the user can choose from should be kept in alphabetical order.
         */
        keepValuesSorted?: boolean;
    }

    /**
     * The settings that can be associated with an editor that lets users choose a subset from a set of values.
     */
    export class OptionFieldOrderedSubset extends OptionField
    {
        protected _Settings: OptionFieldOrderedSubset_Settings;

        constructor(settings: OptionFieldOrderedSubset_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldOrderedSubset',
                controlType: VRS.optionControlTypes.orderedSubset,
                values: [],
                keepValuesSorted: false
            }, settings);
            super(settings);
        }

        getValues() : ValueText[]
        {
            return this._Settings.values;
        }

        getKeepValuesSorted() : boolean
        {
            return this._Settings.keepValuesSorted;
        }
    }

    /**
     * The settings to use when creating a new instance of OptionFieldPaneList.
     */
    export interface OptionFieldPaneList_Settings extends OptionField_Settings
    {
        /**
         * The option panes to show to the user.
         */
        panes?: OptionPane[];

        /**
         * The maximum number of panes (and by extension, items in the array being edited) that can be displayed / edited.
         * Undefined or -1 indicates there is no maximum.
         */
        maxPanes?: number;

        /**
         * An option pane containing the controls that can be used to add a new pane (and therefore a new array item).
         */
        addPane?: OptionPane;

        /**
         * True if the UI to remove individual items is not shown to the user. Defaults to false.
         */
        suppressRemoveButton?: boolean;

        /**
         * An optional method that it called to disable the add controls.
         */
        refreshAddControls?: (disabled: boolean, addPanel: JQuery) => void;
    }

    interface IOptionFieldPaneList_Events
    {
        paneAdded: string;
        paneRemoved: string;
        maxPanesChanged: string;
    }

    //region OptionFieldPaneList
    /**
     * The settings that can be associated with an editor for an array of objects, where each object is edited via its own option pane.
     */
    export class OptionFieldPaneList extends OptionField
    {
        protected _Settings: OptionFieldPaneList_Settings;
        private _PaneListEvents: IOptionFieldPaneList_Events;       // Need to do it this way round, have to initialise the list in the ctor

        constructor(settings: OptionFieldPaneList_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldPaneList',
                controlType: VRS.optionControlTypes.paneList,
                panes: [],
                maxPanes: -1,
                addPane: null,
                suppressRemoveButton: false,
                refreshAddControls: function(disabled: boolean, addParentJQ: JQuery) {
                    $(':input', addParentJQ).prop('disabled', disabled);
                    $(':button', addParentJQ).button('option', 'disabled', disabled);
                }
            }, settings);

            this._PaneListEvents = {
                paneAdded:          'paneAdded',
                paneRemoved:        'paneRemoved',
                maxPanesChanged:    'maxPanesChanged'
            };
            
            super(settings);
        }

        getMaxPanes() : number
        {
            return this._Settings.maxPanes;
        }
        setMaxPanes(value: number)
        {
            if(value !== this._Settings.maxPanes) {
                this._Settings.maxPanes = value;
                this.trimExcessPanes();
                this.onMaxPanesChanged();
            }
        }

        getPanes() : OptionPane[]
        {
            return this._Settings.panes;
        }

        getAddPane() : OptionPane
        {
            return this._Settings.addPane;
        }
        setAddPane(value: OptionPane)
        {
            this._Settings.addPane = value;
        }

        getSuppressRemoveButton() : boolean
        {
            return this._Settings.suppressRemoveButton;
        }
        setSuppressRemoveButton(value: boolean)
        {
            this._Settings.suppressRemoveButton = value;
        }

        getRefreshAddControls() : (disabled: boolean, addParentJQ: JQuery) => void
        {
            return this._Settings.refreshAddControls;
        }
        setRefreshAddControls(value: (disabled: boolean, addParentJQ: JQuery) => void)
        {
            this._Settings.refreshAddControls = value;
        }

        /**
         * Raised when the user adds a new pane. The listener is expected to add the associated array item.
         */
        hookPaneAdded(callback: (newPane: OptionPane, index: number) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._PaneListEvents.paneAdded, callback, forceThis);
        }
        private onPaneAdded(pane: OptionPane, index: number)
        {
            this._Dispatcher.raise(this._PaneListEvents.paneAdded, [ pane, index ]);
        }

        /**
         * Raised when the user removes an existing pane.
         */
        hookPaneRemoved(callback: (removedPane: OptionPane, index: number) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._PaneListEvents.paneRemoved, callback, forceThis);
        }
        private onPaneRemoved(pane: OptionPane, index: number)
        {
            this._Dispatcher.raise(this._PaneListEvents.paneRemoved, [ pane, index]);
        }

        /**
         * Raised after the maximum number of panes has changed.
         * @param {function()}  callback
         * @param {object}      forceThis
         * @returns {{}}
         */
        hookMaxPanesChanged(callback: () => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._PaneListEvents.maxPanesChanged, callback, forceThis);
        }
        private onMaxPanesChanged()
        {
            this._Dispatcher.raise(this._PaneListEvents.maxPanesChanged);
        }

        /**
         * Adds a new pane to the array at the index specified.
         */
        addPane(pane: OptionPane, index?: number)
        {
            if(index !== undefined) {
                this._Settings.panes.splice(index, 0, pane);
                this.onPaneAdded(pane, index);
            } else {
                this._Settings.panes.push(pane);
                this.onPaneAdded(pane, this._Settings.panes.length - 1);
            }
        }

        /**
         * Removes the pane from the array.
         */
        removePane(pane: OptionPane)
        {
            var index = this.findPaneIndex(pane);
            if(index === -1) throw 'Cannot find the pane to remove';
            this.removePaneAt(index);
        }

        /**
         * Removes panes until the count of panes no longer exceeds the maximum allowed.
         */
        trimExcessPanes()
        {
            if(this._Settings.maxPanes !== -1) {
                while(this._Settings.maxPanes > this._Settings.panes.length) {
                    this.removePane(this._Settings.panes[this._Settings.panes.length - 1]);
                }
            }
        }

        /**
         * Returns the index of the pane in the array or -1 if the pane is not in the arrary.
         */
        private findPaneIndex(pane: OptionPane) : number
        {
            var result = -1;
            var length = this._Settings.panes.length;
            for(var i = 0;i < length;++i) {
                if(this._Settings.panes[i] === pane) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Removes the pane at the index specified.
         */
        removePaneAt(index: number)
        {
            var pane = this._Settings.panes[index];
            this._Settings.panes.splice(index, 1);
            pane.dispose(null);                             // <-- this was not passed in the original JavaScript, need to look at existing dispose methods once I've converted everything to TypeScript
            this.onPaneRemoved(pane, index);
        }
    }

    /**
     * The settings to pass when creating a new instance of OptionFieldRadioButton.
     */
    export interface OptionFieldRadioButton_Settings extends OptionField_Settings
    {
        /**
         * The values for each of the radio buttons to show to the user.
         */
        values?: ValueText[];
    }

    export class OptionFieldRadioButton extends OptionField
    {
        protected _Settings: OptionFieldRadioButton_Settings;

        constructor(settings: OptionFieldRadioButton_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldRadioButton',
                controlType: VRS.optionControlTypes.radioButton,
                values: []
            }, settings);
            super(settings);
        }

        getValues(): ValueText[]
        {
            return this._Settings.values;
        }
    }

    /**
     * The settings to pass when creating a new instance of OptionFieldTextBox.
     */
    export interface OptionFieldTextBox_Settings extends OptionField_Settings
    {
        /**
         * True if the text is to be forced into upper-case.
         */
        upperCase?: boolean;

        /**
         * True if the text is to be forced into lower-case.
         */
        lowerCase?: boolean;

        /**
         * The maximum number of characters allowed in the box.
         */
        maxLength?: number;
    }

    /**
     * The settings that can be associated with an editor that can display and edit strings.
     */
    export class OptionFieldTextBox extends OptionField
    {
        protected _Settings: OptionFieldTextBox_Settings;

        constructor(settings: OptionFieldTextBox_Settings)
        {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldTextBox',
                controlType: VRS.optionControlTypes.textBox,
                upperCase: false,
                lowerCase: false,
                maxLength: undefined
            }, settings);
            super(settings);
        }

        getUpperCase() : boolean
        {
            return this._Settings.upperCase;
        }

        getLowerCase() : boolean
        {
            return this._Settings.lowerCase;
        }

        getMaxLength() : number
        {
            return this._Settings.maxLength;
        }
    }

    /**
     * The settings to pass when creating a new instance of an OptionPane.
     */
    export interface OptionPane_Settings
    {
        /**
         * The name of the pane.
         */
        name: string;

        /**
         * The index into VRS.$$ or a method that returns a string. Used as the title for the pane on the page.
         */
        titleKey?: string | VoidFuncReturning<string>;

        /**
         * The relative display order of the pane. Lower display orders appear before higher display orders.
         */
        displayOrder?: number;

        /**
         * The initial set of fields on the pane.
         */
        fields?: OptionField[];

        /**
         * A method to call when the pane is disposed of. Passed an object carrying the OptionPane and the OptionPageParent.
         */
        dispose?: (objects: OptionPane_DisposeObjects) => void;

        /**
         * A method to call when a page parent is created.
         */
        pageParentCreated?: (parent: OptionPageParent) => void;
    }

    /**
     * The object passed to an OptionPane's dispose callback.
     */
    export interface OptionPane_DisposeObjects
    {
        optionPane:          OptionPane;
        optionPageParent:    OptionPageParent;
    }

    /**
     * Describes a pane of related options in the options UI.
     */
    export class OptionPane
    {
        private _Settings: OptionPane_Settings;

        /**
         * The array of fields that the pane contains.
         */
        private _OptionFields: OptionField[] = [];

        /**
         * A value that is incremented any time a field is added to or removed from the pane.
         */
        private _Generation = 0;

        constructor(settings: OptionPane_Settings)
        {
            if(!settings) throw 'You must supply settings';
            if(!settings.name) throw 'You must supply a name for the pane';
            settings = $.extend({
                name: null,
                titleKey: null,
                displayOrder: 0,
                fields: [],
                dispose: $.noop,
                pageParentCreated: $.noop
            }, settings);
            this._Settings = settings;

            if(settings.fields) {
                for(var i = 0;i < settings.fields.length;++i) {
                    this.addField(settings.fields[i]);
                }
            }
        }

        getName() : string
        {
            return this._Settings.name;
        }

        getTitleKey() : string | VoidFuncReturning<string>
        {
            return this._Settings.titleKey;
        }
        getTitleText() : string
        {
            return VRS.globalisation.getText(this._Settings.titleKey);
        }
        setTitleKey(value: string | VoidFuncReturning<string>)
        {
            this._Settings.titleKey = value;
        }

        getDisplayOrder() : number
        {
            return this._Settings.displayOrder;
        }
        setDisplayOrder(value : number)
        {
            this._Settings.displayOrder = value;
        }

        getFieldCount() : number
        {
            return this._OptionFields.length;
        }

        getField(idx: number) : OptionField
        {
            return this._OptionFields[idx];
        }

        getFieldByName(optionFieldName: string) : OptionField
        {
            var index = this.findIndexByName(optionFieldName);
            return index === -1 ? null : this.getField(index);
        }

        dispose(options: OptionPane_DisposeObjects)
        {
            this._Settings.dispose(options);
        }

        /**
         * Invokes a callback, registered when the pane was created, that the pane is being rendered into a page and
         * passes the callback the VRS.OptionPageParent that will live for the duration of the render.
         */
        pageParentCreated(optionPageParent: OptionPageParent)
        {
            this._Settings.pageParentCreated(optionPageParent);
        }

        /**
         * Adds a field to the pane. Throws if the field already exists.
         */
        addField(optionField: OptionField)
        {
            var existingIndex = this.findIndexByName(optionField.getName());
            if(existingIndex !== -1) throw 'There is already a field in this pane called ' + optionField.getName();
            this._OptionFields.push(optionField);
            ++this._Generation;
        }

        /**
         * Removes a field from the pane by its name. Throws if the field does not exist.
         */
        removeFieldByName(optionFieldName: string)
        {
            var index = this.findIndexByName(optionFieldName);
            if(index === -1) throw 'Cannot remove option field ' + optionFieldName + ', it does not exist.';
            this._OptionFields.splice(index, 1);
            ++this._Generation;
        }

        /**
         * Passes every field on the pane to a callback method. The callback is not allowed to add or remove fields from the pane.
         */
        foreachField(callback: (field: OptionField) => void)
        {
            var length = this._OptionFields.length;
            var generation = this._Generation;
            for(var i = 0;i < length;++i) {
                callback(this._OptionFields[i]);
                if(this._Generation !== generation) {
                    throw 'Cannot continue to iterate through the fields after the collection has been modified';
                }
            }
        }

        /**
         * Returns the index of the named field or -1 if there is no field with that name.
         */
        private findIndexByName(optionFieldName: string) : number
        {
            var result = -1;
            $.each(this._OptionFields, function(idx, val) {
                var breakLoop = val.getName() === optionFieldName;
                if(breakLoop) result = idx;
                return !breakLoop;
            });

            return result;
        }
    }

    /**
     * The settings to pass when creating a new instance of OptionPage.
     */
    export interface OptionPage_Settings
    {
        /**
         * The unique name of the page.
         */
        name: string;

        /**
         * The VRS.$$ label for the page's title, or a void function that returns the page's title.
         */
        titleKey?: string | VoidFuncReturning<string>;

        /**
         * The relative order of the page within a group of pages. Pages with a lower display order come before those with a higher order.
         */
        displayOrder?: number;

        /**
         * The initial set of panes.
         */
        panes?: OptionPane[];
    }

    /**
     * Describes a page of panes in the configuration UI.
     */
    export class OptionPage
    {
        private _Settings: OptionPage_Settings;

        /**
         * The panes held by the page.
         */
        private _OptionPanes: OptionPane[] = [];

        /**
         * A value that is incremented every time a pane is added to or removed from the page.
         */
        private _Generation = 0;

        /**
         * The generation number as-at the last time the page was sorted. Used to optimise the sort method.
         */
        private _SortGeneration = -1;

        constructor(settings: OptionPage_Settings)
        {
            if(!settings) throw 'You must supply settings';
            if(!settings.name) throw 'You must give the page a name';
            this._Settings = settings;

            if(settings.panes) {
                for(var i = 0;i < settings.panes.length;++i) {
                    this.addPane(settings.panes[i]);
                }
            }
        }

        getName() : string
        {
            return this._Settings.name;
        }
        setName(value: string)
        {
            this._Settings.name = value;
        }

        getTitleKey() : string | VoidFuncReturning<string>
        {
            return this._Settings.titleKey;
        }
        setTitleKey(value: string | VoidFuncReturning<string>)
        {
            this._Settings.titleKey = value;
        }

        getDisplayOrder() : number
        {
            return this._Settings.displayOrder;
        }
        setDisplayOrder(value: number)
        {
            if(!isNaN(value)) {
                this._Settings.displayOrder = value;
            }
        }

        /**
         * Adds one or more panes to the page. If there is already a pane on the page with the same name then an exception is thrown.
         */
        addPane(optionPane: OptionPane | OptionPane[])
        {
            if(!(optionPane instanceof VRS.OptionPane)) {
                var length = (<OptionPane[]>optionPane).length;
                for(var i = 0;i < length;++i) {
                    this.addPane(optionPane[i]);
                }
            } else {
                var index = this.findIndexByName(optionPane.getName());
                if(index !== -1) throw 'There is already a pane on this page called ' + optionPane.getName();
                this._OptionPanes.push(optionPane);
                ++this._Generation;
            }
        }

        /**
         * Removes the pane with the name passed across. If there is no pane on the page with this name then an exception is thrown.
         */
        removePaneByName(optionPaneName: string)
        {
            var index = this.findIndexByName(optionPaneName);
            if(index === -1) throw 'There is no pane called ' + optionPaneName;
            this._OptionPanes.splice(index, 1);
            ++this._Generation;
        }

        /**
         * Passes each pane on the page to a callback method, one at a time. The callback must not add or remove panes on the page.
         */
        foreachPane(callback: (pane: OptionPane) => void)
        {
            this.sortPanes();
            var generation = this._Generation;
            var length = this._OptionPanes.length;
            for(var i = 0;i < length;++i) {
                callback(this._OptionPanes[i]);
                if(this._Generation != generation) throw 'Cannot continue to iterate through the panes, they have been changed';
            }
        }

        /**
         * Returns the index of a pane or -1 if there is no pane with the name passed across.
         */
        private findIndexByName(optionPaneName: string) : number
        {
            var result = -1;
            $.each(this._OptionPanes, function(idx, val) {
                var foundMatch = val.getName() === optionPaneName;
                if(foundMatch) result = idx;
                return !foundMatch;
            });

            return result;
        }

        /**
         * Sorts panes into display order.
         */
        private sortPanes()
        {
            if(this._SortGeneration !== this._Generation) {
                this._OptionPanes.sort(function(lhs, rhs) {
                    return lhs.getDisplayOrder() - rhs.getDisplayOrder();
                });
                this._SortGeneration = this._Generation;
            }
        }
    }

    /**
     * An object that exposes events across all of the options in a group of pages.
     */
    export class OptionPageParent
    {
        private _Dispatcher = new EventHandler({ name: 'VRS.OptionPageParent' });
        private _Events = {
            fieldChanged: 'fieldChanged'
        }

        /**
         * Hooks an event that is raised whenever a field changes on the object.
         */
        hookFieldChanged(callback: () => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.fieldChanged, callback, forceThis);
        }
        raiseFieldChanged()
        {
            this._Dispatcher.raise(this._Events.fieldChanged);
        }

        /**
         * Unhooks an event previously hooked.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }
    }

    /**
     * The base options that are passed to all plugins that represent the different kinds of OptionField.
     * All plugins should extend their options from this interface. In particular they should override
     * field with the OptionField type that they are handling.
     */
    export interface OptionControl_BaseOptions
    {
        field:              OptionField;
        fieldParentJQ:      JQuery;
        optionPageParent:   OptionPageParent;
    }

    /**
     * An object that can translate between control types and the jQuery UI plugin that handles the control.
     */
    export class OptionControlTypeBroker
    {
        /**
         * The associative array of control types and the method to use to create the appropriate jQuery UI plugin.
         */
        private _ControlTypes: { [index: /*OptionControlTypesEnum - TS1.7 does not allow types as index, even if they resolve to string... */string]: (settings: OptionControl_BaseOptions) => JQuery } = {};

        /**
         * Adds a handler for a control type handler. Throws an exception if there is already a handler for the control type.
         */
        addControlTypeHandler(controlType: OptionControlTypesEnum, creatorCallback: (options: OptionControl_BaseOptions) => JQuery)
        {
            if(this._ControlTypes[controlType]) throw 'There is already a handler registered for ' + controlType + ' control types';
            this._ControlTypes[controlType] = creatorCallback;
        }

        /**
         * Adds a handler for a control type, but only if one is not already registered. Does nothing if one is already registered.
         */
        addControlTypeHandlerIfNotRegistered(controlType: OptionControlTypesEnum, creatorCallback: (options: OptionControl_BaseOptions) => JQuery)
        {
            if(!this.controlTypeHasHandler(controlType)) {
                this.addControlTypeHandler(controlType, creatorCallback);
            }
        }

        /**
         * Removes the handler for a control type. Throws an exception if the control type does not have a handler registered.
         */
        removeControlTypeHandler(controlType: OptionControlTypesEnum)
        {
            if(!this._ControlTypes[controlType]) throw 'There is no handler registered for ' + controlType + ' control types';
            delete this._ControlTypes[controlType];
        }

        /**
         * Returns true if the control type has a handler registered for it, false if it does not.
         */
        controlTypeHasHandler(controlType: OptionControlTypesEnum) : boolean
        {
            return !!this._ControlTypes[controlType];
        }

        /**
         * Creates the jQuery UI plugin that will manage the display and editing of an option field.
         */
        createControlTypeHandler(options: OptionControl_BaseOptions) : JQuery
        {
            var controlType = options.field.getControlType();
            var creator = this._ControlTypes[controlType];
            if(!creator) throw 'There is no handler registered for ' + controlType + ' control types';

            return creator(options);
        }
    }

    /*
     * Pre-builts
     */
    export var optionControlTypeBroker = new VRS.OptionControlTypeBroker();

    type OptionControlTypesEnum = string;
    /*
     * A modifiable enumeration of the different control types. 3rd parties can add their own control types to this.
     */
    export var optionControlTypes = VRS.optionControlTypes || {};
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
}
 