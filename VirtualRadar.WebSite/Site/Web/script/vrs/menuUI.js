var VRS;
(function (VRS) {
    var MenuItem = (function () {
        function MenuItem(settings) {
            if (!settings)
                throw 'You must supply a settings object';
            if (!settings.name)
                throw 'You must supply a unique name for the menu item';
            if (!settings.labelKey)
                throw 'You must supply a label key';
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
            this._SliderDefaultValue = settings.sliderDefaultValue;
            this._SliderCallback = settings.sliderCallback;
            this.name = settings.name;
            this.clickCallback = settings.clickCallback;
            this.subItems = settings.subItems || [];
            this.subItemsNormalised = [];
            this.noAutoClose = !!settings.noAutoClose;
            this.tag = settings.tag;
            this.suppress = settings.suppress || function () { return false; };
        }
        MenuItem.prototype.initialise = function () {
            if (this._Initialise) {
                this._Initialise();
            }
        };
        MenuItem.prototype.isDisabled = function () {
            return VRS.Utility.ValueOrFuncReturningValue(this._Disabled, false);
        };
        MenuItem.prototype.getLabelText = function () {
            return VRS.globalisation.getText(this._LabelKey);
        };
        MenuItem.prototype.getJQueryIcon = function () {
            return VRS.Utility.ValueOrFuncReturningValue(this._JqIcon, null);
        };
        MenuItem.prototype.getVrsIcon = function () {
            var result = VRS.Utility.ValueOrFuncReturningValue(this._VrsIcon, null);
            if (!this._VrsIcon && this._Checked !== undefined) {
                var isChecked = VRS.Utility.ValueOrFuncReturningValue(this._Checked, false);
                if (isChecked) {
                    result = 'checkmark';
                }
            }
            return result;
        };
        MenuItem.prototype.getLabelImageUrl = function () {
            return VRS.Utility.ValueOrFuncReturningValue(this._LabelImageUrl, null);
        };
        MenuItem.prototype.getLabelImageClasses = function () {
            return VRS.Utility.ValueOrFuncReturningValue(this._LabelImageClasses, null);
        };
        MenuItem.prototype.getLabelImageElement = function () {
            var result = null;
            var url = this.getLabelImageUrl();
            if (url) {
                result = $('<img/>').attr('src', url);
                var classes = this.getLabelImageClasses();
                if (classes) {
                    result.addClass(classes);
                }
            }
            return result;
        };
        MenuItem.prototype.showSlider = function () {
            return this._ShowSlider !== undefined && VRS.Utility.ValueOrFuncReturningValue(this._ShowSlider, false);
        };
        MenuItem.prototype.getSliderMinimum = function () {
            return this._SliderMinimum !== undefined ? VRS.Utility.ValueOrFuncReturningValue(this._SliderMinimum, 0) : 0;
        };
        MenuItem.prototype.getSliderMaximum = function () {
            return this._SliderMaximum !== undefined ? VRS.Utility.ValueOrFuncReturningValue(this._SliderMaximum, 100) : 100;
        };
        MenuItem.prototype.getSliderStep = function () {
            return this._SliderStep !== undefined ? VRS.Utility.ValueOrFuncReturningValue(this._SliderStep, 1) : 1;
        };
        MenuItem.prototype.getSliderInitialValue = function () {
            return this._SliderInitialValue !== undefined ? VRS.Utility.ValueOrFuncReturningValue(this._SliderInitialValue, this.getSliderMinimum()) : this.getSliderMinimum();
        };
        MenuItem.prototype.getSliderDefaultValue = function () {
            return this._SliderDefaultValue !== undefined ? VRS.Utility.ValueOrFuncReturningValue(this._SliderDefaultValue, this.getSliderMinimum()) : null;
        };
        MenuItem.prototype.callSliderCallback = function (value) {
            if (this._SliderCallback) {
                this._SliderCallback(value);
            }
        };
        return MenuItem;
    }());
    VRS.MenuItem = MenuItem;
    var Menu = (function () {
        function Menu(settings) {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.Menu'
            });
            this._Events = {
                beforeAddingFixedMenuItems: 'beforeAddingFixedMenuItems',
                afterAddingFixedMenuItems: 'afterAddingFixedMenuItems'
            };
            this._TopLevelMenuItems = [];
            this._Settings = $.extend({
                items: []
            }, settings);
            var self = this;
            $.each(this._Settings.items, function (idx, menuItem) {
                self._TopLevelMenuItems.push(menuItem);
            });
        }
        Menu.prototype.getTopLevelMenuItems = function () {
            return this._TopLevelMenuItems;
        };
        Menu.prototype.hookBeforeAddingFixedMenuItems = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.beforeAddingFixedMenuItems, callback, forceThis);
        };
        Menu.prototype.hookAfterAddingFixedMenuItems = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.afterAddingFixedMenuItems, callback, forceThis);
        };
        Menu.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        Menu.prototype.buildMenuItems = function () {
            var rawItems = [];
            this._Dispatcher.raise(this._Events.beforeAddingFixedMenuItems, [this, rawItems]);
            rawItems = rawItems.concat(this._TopLevelMenuItems);
            this._Dispatcher.raise(this._Events.afterAddingFixedMenuItems, [this, rawItems]);
            return this.normaliseMenu(rawItems);
        };
        Menu.prototype.normaliseMenu = function (rawItems) {
            var result = [];
            var previousIsSeparator = true;
            var length = rawItems.length;
            for (var i = 0; i < length; ++i) {
                var menuItem = rawItems[i];
                var suppress = menuItem ? menuItem.suppress() : false;
                if (!suppress) {
                    if (menuItem) {
                        previousIsSeparator = false;
                        menuItem.subItemsNormalised = this.normaliseMenu(menuItem.subItems);
                    }
                    else {
                        if (previousIsSeparator)
                            continue;
                        previousIsSeparator = true;
                    }
                    result.push(menuItem);
                }
            }
            if (result.length && previousIsSeparator)
                result.splice(result.length - 1, 1);
            return result;
        };
        Menu.prototype.findMenuItemForName = function (name, menuItems, useNormalisedSubItems) {
            var result = null;
            if (!menuItems)
                menuItems = this.getTopLevelMenuItems();
            var length = menuItems.length;
            for (var i = 0; i < length; ++i) {
                var menuItem = menuItems[i];
                if (!menuItem)
                    continue;
                if (menuItem.name === name) {
                    result = menuItem;
                    break;
                }
                var subItems = useNormalisedSubItems ? menuItem.subItemsNormalised : menuItem.subItems;
                if (subItems.length) {
                    result = this.findMenuItemForName(name, subItems, useNormalisedSubItems);
                    if (result)
                        break;
                }
            }
            return result;
        };
        return Menu;
    }());
    VRS.Menu = Menu;
})(VRS || (VRS = {}));
//# sourceMappingURL=menuUI.js.map