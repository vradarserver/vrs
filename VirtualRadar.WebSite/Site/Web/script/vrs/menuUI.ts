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
 * @fileoverview Menu UI abstractions.
 */

namespace VRS
{
    /**
     * The settings that can be passed when creating a new instance of a MenuItem.
     */
    export interface MenuItem_Settings
    {
        /**
         * The unique name of the menu item.
         */
        name: string;

        /**
         * The name of a VRS.$$ item to show for the label, or a function that returns the label text.
         */
        labelKey?: string | VoidFuncReturning<string>;

        /**
         * The name of the jQuery UI icon (without the leading 'ui-icon-') to show against the item if labelKey has been supplied.
         */
        jqIcon?: string | VoidFuncReturning<string>;

        /**
         * The name of the vrsIcon icon (without the leading 'vrsIcon-') to show against the item if labelKey has been supplied.
         */
        vrsIcon?: string | VoidFuncReturning<string>;

        /**
         * A value indicating that the menu entry should be shown with a checkmark against it, or a function that returns a bool indicating same.
         */
        checked?: boolean | VoidFuncReturning<boolean>;

        /**
         * The URL of the image to use as the label icon.
         */
        labelImageUrl?: string | VoidFuncReturning<string>;

        /**
         * The classes to apply to the image used as a label icon.
         */
        labelImageClasses?: string | VoidFuncReturning<string>;

        /**
         * The function that is called when the menu item is clicked. Optional if subItems are supplied.
         */
        clickCallback?: (menuItem?: MenuItem) => void;

        /**
         * A value indicating that the menu entry should be shown with a slider against it, or a function that returns a bool indicating the same.
         */
        showSlider?: boolean | VoidFuncReturning<boolean>;

        /**
         * The minimum value for the slider's range or a function that returns the minimum value.
         */
        sliderMinimum?: number | VoidFuncReturning<number>;

        /**
         * The maximum value for the slider's range or a function that returns the maximum value.
         */
        sliderMaximum?: number | VoidFuncReturning<number>;

        /**
         * The interval to jump when sliding the slider.
         */
        sliderStep?: number | VoidFuncReturning<number>;

        /**
         * The initial value for the slider. Defaults to sliderMinimum if not supplied.
         */
        sliderInitialValue?: number | VoidFuncReturning<number>;

        /**
         * The function that is called when the slider value changes.
         */
        sliderCallback?: (value: number) => void;

        /**
         * An optional method called just before the menu item is used.
         */
        initialise?: () => void;

        /**
         * Either a bool indicating whether the menu item is disabled or a method that returns a bool indicating whether the menu item is disabled. Defaults to false.
         */
        disabled?: boolean | VoidFuncReturning<boolean>;

        /**
         * An optional array of sub-items.
         */
        subItems?: MenuItem[];

        /**
         * An optional object attached to the menu item.
         */
        tag?: any;

        /**
         * An optional flag that prevents the menu from closing when the item is clicked.
         */
        noAutoClose?: boolean;

        /**
         * An optional callback that can return true to prevent the menu option from appearing. Defaults to function that returns false.
         */
        suppress?: () => boolean;
    }

    /**
     * Describes a single item within a menu.
     */
    export class MenuItem
    {
        private _Initialise: () => void;
        private _Disabled: boolean | VoidFuncReturning<boolean>;
        private _LabelKey: string | VoidFuncReturning<string>;
        private _JqIcon: string | VoidFuncReturning<string>;
        private _VrsIcon: string | VoidFuncReturning<string>;
        private _Checked: boolean | VoidFuncReturning<boolean>;
        private _ShowSlider: boolean | VoidFuncReturning<boolean>;
        private _SliderMinimum: number | VoidFuncReturning<number>;
        private _SliderMaximum: number | VoidFuncReturning<number>;
        private _SliderStep: number | VoidFuncReturning<number>;
        private _SliderInitialValue: number | VoidFuncReturning<number>;
        private _SliderCallback: (value: number) => void;
        private _LabelImageUrl: string | VoidFuncReturning<string>;
        private _LabelImageClasses: string | VoidFuncReturning<string>;

        // Kept as public fields for backwards compatibility
        name:               string;
        clickCallback:      (menuItem?: MenuItem) => void;
        subItems:           MenuItem[];
        subItemsNormalised: MenuItem[];
        noAutoClose:        boolean;
        tag:                any;
        suppress:           () => boolean;

        constructor(settings: MenuItem_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(!settings.name) throw 'You must supply a unique name for the menu item';
            if(!settings.labelKey) throw 'You must supply a label key';

            this._Initialise = settings.initialise;
            this._Disabled = settings.disabled;
            this._LabelKey = settings.labelKey;
            this._JqIcon = settings.jqIcon;
            this._VrsIcon = settings.vrsIcon;
            this._Checked = settings.checked;
            this._LabelImageUrl = settings.labelImageUrl;
            this._LabelImageClasses = settings.labelImageClasses;

            this._ShowSlider = settings.showSlider;
            this._SliderMinimum = settings.sliderMinimum;
            this._SliderMaximum = settings.sliderMaximum;
            this._SliderStep = settings.sliderStep;
            this._SliderInitialValue = settings.sliderInitialValue;
            this._SliderCallback = settings.sliderCallback;

            this.name = settings.name;
            this.clickCallback = settings.clickCallback;
            this.subItems = settings.subItems || [];
            this.subItemsNormalised = [];
            this.noAutoClose = !!settings.noAutoClose;
            this.tag = settings.tag;
            this.suppress = settings.suppress || function() { return false; };
        }

        /**
         * Initialises the menu item.
         */
        initialise()
        {
            if(this._Initialise) {
                this._Initialise();
            }
        }

        /**
         * Returns true if the menu item is disabled.
         */
        isDisabled() : boolean
        {
            return Utility.ValueOrFuncReturningValue(this._Disabled, false);
        }

        /**
         * Returns the label text to use.
         */
        getLabelText()
        {
            return VRS.globalisation.getText(this._LabelKey);
        }

        /**
         * Returns the jQueryUI icon or null if there is no jQueryUI icon.
         */
        getJQueryIcon() : string
        {
            return Utility.ValueOrFuncReturningValue(this._JqIcon, null);
        }

        /**
         * Returns the VRS icon or null if there is no VRS icon.
         */
        getVrsIcon() : string
        {
            var result = Utility.ValueOrFuncReturningValue(this._VrsIcon, null);
            if(!this._VrsIcon && this._Checked !== undefined) {
                var isChecked = Utility.ValueOrFuncReturningValue(this._Checked, false);
                if(isChecked) {
                    result = 'checkmark';
                }
            }

            return result;
        }

        /**
         * Returns the label image URL or null if there is no label image URL.
         */
        getLabelImageUrl() : string
        {
            return Utility.ValueOrFuncReturningValue(this._LabelImageUrl, null);
        }

        /**
         * Returns the label image classes or null if there are no label image classes.
         */
        getLabelImageClasses() : string
        {
            return Utility.ValueOrFuncReturningValue(this._LabelImageClasses, null);
        }

        /**
         * Returns a jQuery object wrapping a newly created img element for the label image or null if the menu has no
         * label image URL. Note that the image has no explicit width or height.
         */
        getLabelImageElement() : JQuery
        {
            var result = null;

            var url = this.getLabelImageUrl();
            if(url) {
                result = $('<img/>').attr('src', url);
                var classes = this.getLabelImageClasses();
                if(classes) {
                    result.addClass(classes);
                }
            }

            return result;
        }

        showSlider() : boolean
        {
            return this._ShowSlider !== undefined && Utility.ValueOrFuncReturningValue(this._ShowSlider, false);
        }

        getSliderMinimum() : number
        {
            return this._SliderMinimum !== undefined ? Utility.ValueOrFuncReturningValue(this._SliderMinimum, 0) : 0;
        }

        getSliderMaximum() : number
        {
            return this._SliderMaximum !== undefined ? Utility.ValueOrFuncReturningValue(this._SliderMaximum, 100) : 100;
        }

        getSliderStep() : number
        {
            return this._SliderStep !== undefined ? Utility.ValueOrFuncReturningValue(this._SliderStep, 1) : 1;
        }

        getSliderInitialValue() : number
        {
            return this._SliderInitialValue !== undefined ? Utility.ValueOrFuncReturningValue(this._SliderInitialValue, this.getSliderMinimum()) : this.getSliderMinimum();
        }

        callSliderCallback(value: number)
        {
            if(this._SliderCallback) {
                this._SliderCallback(value);
            }
        }
    }

    /**
     * The settings to use when creating a new instance of Menu.
     */
    export interface Menu_Settings
    {
        items:      MenuItem[];
    }

    /**
     * A class that collects together a bunch of menu items to form a menu structure.
     */
    export class Menu
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.Menu'
        });
        private _Events = {
            beforeAddingFixedMenuItems: 'beforeAddingFixedMenuItems',
            afterAddingFixedMenuItems:  'afterAddingFixedMenuItems'
        };

        /**
         * The settings used to initialise the class.
         */
        private _Settings: Menu_Settings;

        /**
         * The top-level items to show when the user asks for the menu. Null entries in the list represent separators.
         */
        private _TopLevelMenuItems: MenuItem[] = [];
        getTopLevelMenuItems() : MenuItem[]
        {
            return this._TopLevelMenuItems;
        }

        /**
         * Raised before buildMenuItems adds the fixed top-level menu items to the menu structure that it returns. Gets
         * passed the array of menu items as currently constructed - you can modify this array but you must not call
         * buildMenuItems in the handler. Note that multiple consecutive separators will be removed before buildMenuItems
         * returns, you don't need to remove them yourself.
         */
        hookBeforeAddingFixedMenuItems(callback: (menu: Menu, menuItems: MenuItem[]) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.beforeAddingFixedMenuItems, callback, forceThis);
        }

        /**
         * Raised after buildMenuItems adds the fixed top-level menu items to the menu structure that it returns. Gets
         * passed the array of menu items as currently constructed - you can modify this array but you must not call
         * buildMenuItems in the handler. Note that multiple consecutive separators will be removed before buildMenuItems
         * returns, you don't need to remove them yourself.
         */
        hookAfterAddingFixedMenuItems(callback: (menu: Menu, menuItems: MenuItem[]) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.afterAddingFixedMenuItems, callback, forceThis);
        }

        /**
         * Unhooks a previously hooked event on the object.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Creates a new object.
         */
        constructor(settings?: Menu_Settings)
        {
            this._Settings = $.extend({
                items:      []
            }, settings);

            var self = this;
            $.each(this._Settings.items, function(idx, menuItem) {
                self._TopLevelMenuItems.push(menuItem);
            });
        }

        /**
         * Constructs the set of menu items to display to the user.
         */
        buildMenuItems() : MenuItem[]
        {
            var rawItems: MenuItem[] = [];

            this._Dispatcher.raise(this._Events.beforeAddingFixedMenuItems, [ this, rawItems ]);
            rawItems = rawItems.concat(this._TopLevelMenuItems);
            this._Dispatcher.raise(this._Events.afterAddingFixedMenuItems, [ this, rawItems ]);

            return this.normaliseMenu(rawItems);
        }

        /**
         * Returns a copy of the array passed across with unwanted duplicates etc. filtered out.
         */
        private normaliseMenu(rawItems: MenuItem[]) : MenuItem[]
        {
            var result: MenuItem[] = [];

            var previousIsSeparator = true;
            var length = rawItems.length;
            for(var i = 0;i < length;++i) {
                var menuItem = rawItems[i];
                var suppress = menuItem ? menuItem.suppress() : false;
                if(!suppress) {
                    if(menuItem) {
                        previousIsSeparator = false;
                        menuItem.subItemsNormalised = this.normaliseMenu(menuItem.subItems);
                    }
                    else {
                        if(previousIsSeparator) continue;
                        previousIsSeparator = true;
                    }

                    result.push(menuItem);
                }
            }

            if(result.length && previousIsSeparator) result.splice(result.length - 1, 1);

            return result;
        }

        /**
         * Returns the menu item with the given name. If a menuItems array is supplied then the search is constrained
         * to that array (including sub-items in the array), otherwise the top-level menu items are searched. If the
         * menu cannot be found then null is returned.
         */
        findMenuItemForName(name: string, menuItems?: MenuItem[], useNormalisedSubItems?: boolean) : MenuItem
        {
            var result: MenuItem = null;

            if(!menuItems) menuItems = this.getTopLevelMenuItems();
            var length = menuItems.length;
            for(var i = 0;i < length;++i) {
                var menuItem = menuItems[i];
                if(!menuItem) continue;

                if(menuItem.name === name) {
                    result = menuItem;
                    break;
                }

                var subItems = useNormalisedSubItems ? menuItem.subItemsNormalised : menuItem.subItems;
                if(subItems.length) {
                    result = this.findMenuItemForName(name, subItems, useNormalisedSubItems);
                    if(result) break;
                }
            }

            return result;
        }
    }
} 
