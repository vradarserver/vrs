var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.menuClass = VRS.globalOptions.menuClass || 'vrsMenu';
    var MenuPlugin_State = (function () {
        function MenuPlugin_State() {
            this.menuItems = [];
            this.menuItemElements = {};
            this.trigger = null;
            this.clickCatcher = null;
            this.menuContainer = null;
        }
        return MenuPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getMenuPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsMenu');
    };
    VRS.jQueryUIHelper.getMenuOptions = function (overrides) {
        return $.extend({
            menu: null,
            showButtonTrigger: true,
            triggerElement: null,
            menuContainerClasses: null,
            offsetX: 0,
            offsetY: 5,
            alignment: VRS.Alignment.Centre,
            cssMenuWidth: 300,
            zIndex: 99999,
            __nop: null
        }, overrides);
    };
    var MenuPlugin = (function (_super) {
        __extends(MenuPlugin, _super);
        function MenuPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getMenuOptions();
            return _this;
        }
        MenuPlugin.prototype._getState = function () {
            var result = this.element.data('vrsMenuState');
            if (result === undefined) {
                result = new MenuPlugin_State();
                this.element.data('vrsMenuState', result);
            }
            return result;
        };
        MenuPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            this.element.addClass(VRS.globalOptions.menuClass);
            var trigger = options.triggerElement;
            if (options.showButtonTrigger)
                trigger = $('<span/>').addClass('vrsIcon vrsIcon-cog colourButton vrsNoHighlight');
            state.trigger = trigger
                .on('click', $.proxy(this._triggerPressed, this))
                .appendTo(this.element);
        };
        MenuPlugin.prototype._destroy = function () {
            var state = this._getState();
            this._destroyMenu(state);
            if (state.trigger)
                state.trigger.off();
            state.trigger = null;
            state.menuItemElements = {};
            state.menuItems = [];
        };
        MenuPlugin.prototype._createMenu = function (state) {
            var options = this.options;
            state.menuItems = this.options.menu.buildMenuItems();
            state.menuContainer = $('<div/>')
                .addClass('dl-menuwrapper')
                .css({
                'z-index': options.zIndex
            });
            if (options.menuContainerClasses)
                state.menuContainer.addClass(options.menuContainerClasses);
            if (options.showButtonTrigger)
                state.trigger.removeClass('colourButton').addClass('colourButtonActive');
            this._createMenuItemElements(state, state.menuContainer, state.menuItems);
            var body = $('body');
            var position = this._determineTopLeft(state);
            state.menuContainer
                .css(position)
                .prependTo(body);
            state.clickCatcher = $('<div/>')
                .css({
                'width': '100%',
                'height': '100%',
                'position': 'fixed',
                'top': 0,
                'left': 0,
                'z-index': options.zIndex - 1
            })
                .on('click.vrsMenu', $.proxy(this._clickCatcherClicked, this))
                .prependTo(body);
            state.menuContainer.dlmenu({
                backLinkText: VRS.$$.MenuBack
            });
            $(window).on('resize.vrsMenu', $.proxy(this._windowResized, this));
            if (VRS.refreshManager)
                VRS.refreshManager.registerTarget(this.element, $.proxy(function () {
                    this._destroyMenu(state);
                }, this));
        };
        MenuPlugin.prototype._destroyMenu = function (state) {
            if (state.menuContainer) {
                var options = this.options;
                $.each(state.menuItemElements, function (idx, elements) {
                    if (elements.link)
                        elements.link.off();
                });
                state.menuItemElements = {};
                if (state.clickCatcher) {
                    state.clickCatcher.off('click.vrsMenu');
                    state.clickCatcher.remove();
                }
                state.clickCatcher = null;
                state.menuContainer.dlmenu('dispose');
                state.menuContainer.remove();
                state.menuContainer = null;
                if (options.showButtonTrigger)
                    state.trigger.removeClass('colourButtonActive').addClass('colourButton');
                $(window).off('resize.vrsMenu');
                if (VRS.refreshManager)
                    VRS.refreshManager.unregisterTarget(this.element);
            }
        };
        MenuPlugin.prototype._determineTopLeft = function (state) {
            var options = this.options;
            var trigger = state.trigger;
            var offset = trigger.offset();
            var triggerSize = {
                width: trigger.outerWidth(),
                height: trigger.outerHeight()
            };
            var windowJQ = $(window);
            var windowSize = {
                width: windowJQ.width(),
                height: windowJQ.height()
            };
            var pixelsAboveTarget = offset.top - options.offsetY;
            var pixelsBelowTarget = windowSize.height - (offset.top + triggerSize.height + options.offsetY);
            var top, bottom;
            if (pixelsAboveTarget > pixelsBelowTarget)
                bottom = windowSize.height - pixelsAboveTarget;
            else
                top = windowSize.height - pixelsBelowTarget;
            var left = offset.left + options.offsetX;
            var width = options.cssMenuWidth;
            switch (options.alignment) {
                case VRS.Alignment.Right:
                    left -= (width - triggerSize.width);
                    break;
                case VRS.Alignment.Centre:
                    left -= (width - triggerSize.width) / 2;
                    break;
            }
            left = Math.min(left, windowSize.width - width);
            left = Math.max(left, 0);
            return {
                top: top,
                bottom: bottom,
                left: left
            };
        };
        MenuPlugin.prototype._createMenuItemElements = function (state, parentJQ, menuItems) {
            var self = this;
            var list = $('<ul/>')
                .addClass(parentJQ === state.menuContainer ? 'dl-menu' : 'dl-submenu')
                .appendTo(parentJQ);
            var previousListItem = null;
            $.each(menuItems, function (idx, menuItem) {
                if (!menuItem) {
                    if (previousListItem)
                        previousListItem.addClass('dl-separator');
                }
                else {
                    menuItem.initialise();
                    var isDisabled = menuItem.isDisabled();
                    if (state.menuItemElements[menuItem.name])
                        throw 'There are at least two menu items called ' + menuItem.name + ' - menu item names must be unique';
                    var listItem = $('<li/>')
                        .appendTo(list);
                    var imageElement = self._buildMenuItemImageElement(state, menuItem);
                    var link = $('<a/>')
                        .attr('href', '#')
                        .append(imageElement)
                        .appendTo(listItem);
                    var text = self._buildMenuItemTextElement(state, menuItem)
                        .appendTo(link);
                    if (isDisabled)
                        listItem.addClass('dl-disabled');
                    if (menuItem.clickCallback || menuItem.subItemsNormalised.length)
                        link.click($.proxy(function (event) { self._menuItemClicked(event, menuItem); }, self));
                    if (menuItem.subItemsNormalised.length)
                        self._createMenuItemElements(state, listItem, menuItem.subItemsNormalised);
                    state.menuItemElements[menuItem.name] = {
                        listItem: listItem,
                        image: imageElement,
                        link: link,
                        text: text
                    };
                    previousListItem = listItem;
                }
            });
        };
        MenuPlugin.prototype._buildMenuItemImageElement = function (state, menuItem) {
            var isDisabled = menuItem.isDisabled();
            var jqIcon = menuItem.getJQueryIcon();
            var vrsIcon = menuItem.getVrsIcon();
            var iconImage = menuItem.getLabelImageUrl();
            var showIcon = jqIcon || vrsIcon || !iconImage;
            var imageElement;
            if (showIcon) {
                imageElement = $('<span/>').addClass(jqIcon ? 'dl-icon ui-icon ui-icon-' + jqIcon :
                    vrsIcon ? 'dl-icon colourButton vrsIcon vrsIcon-' + vrsIcon
                        : 'dl-noicon');
            }
            else {
                imageElement = menuItem.getLabelImageElement().addClass('dl-iconImage');
                var labelImageClasses = menuItem.getLabelImageClasses();
                if (labelImageClasses)
                    imageElement.addClass(labelImageClasses);
            }
            if (isDisabled)
                imageElement.addClass('dl-disabled');
            return imageElement;
        };
        MenuPlugin.prototype._buildMenuItemTextElement = function (state, menuItem) {
            var textElement = $('<span/>')
                .text(menuItem.getLabelText());
            return textElement;
        };
        MenuPlugin.prototype._refreshMenuItem = function (state, menuItem) {
            var newImage = this._buildMenuItemImageElement(state, menuItem);
            var newText = this._buildMenuItemTextElement(state, menuItem);
            var elements = state.menuItemElements[menuItem.name];
            elements.image.replaceWith(newImage);
            elements.text.replaceWith(newText);
            elements.image = newImage;
            elements.text = newText;
        };
        MenuPlugin.prototype._refreshChildItems = function (state, menuItem) {
            $.each(menuItem.subItemsNormalised, function (idx, subItem) {
                if (subItem) {
                    var elements = state.menuItemElements[subItem.name];
                    if (elements) {
                        if (elements.listItem) {
                            var wasDisabled = elements.listItem.hasClass('dl-disabled');
                            var nowDisabled = subItem.isDisabled();
                            if (wasDisabled !== nowDisabled) {
                                if (wasDisabled)
                                    elements.listItem.removeClass('dl-disabled');
                                else
                                    elements.listItem.addClass('dl-disabled');
                            }
                        }
                        if (elements.text)
                            elements.text.text(subItem.getLabelText());
                    }
                }
            });
        };
        MenuPlugin.prototype.getIsOpen = function () {
            return this.doGetIsOpen();
        };
        MenuPlugin.prototype.doGetIsOpen = function (state) {
            state = state || this._getState();
            return !!state.menuContainer;
        };
        MenuPlugin.prototype.toggleMenu = function () {
            this.doToggleMenu();
        };
        MenuPlugin.prototype.doToggleMenu = function (state) {
            state = state || this._getState();
            if (this.doGetIsOpen(state)) {
                this.doCloseMenu(state);
            }
            else {
                this.doOpenMenu(state);
            }
        };
        MenuPlugin.prototype.openMenu = function () {
            this.doOpenMenu();
        };
        MenuPlugin.prototype.doOpenMenu = function (state) {
            state = state || this._getState();
            if (!state.menuContainer) {
                this._createMenu(state);
                state.menuContainer.dlmenu('openMenu');
            }
        };
        MenuPlugin.prototype.closeMenu = function () {
            this.doCloseMenu();
        };
        MenuPlugin.prototype.doCloseMenu = function (state) {
            state = state || this._getState();
            if (state.menuContainer) {
                this._destroyMenu(state);
            }
        };
        MenuPlugin.prototype._triggerPressed = function (event) {
            this.toggleMenu();
            event.stopPropagation();
        };
        MenuPlugin.prototype._menuItemClicked = function (event, menuItem) {
            var state = this._getState();
            var elements = state.menuItemElements[menuItem.name];
            var disabled = elements.listItem && elements.listItem.hasClass('dl-disabled');
            var stopPropagation = disabled || (!menuItem.noAutoClose && menuItem.clickCallback);
            if (stopPropagation)
                event.stopPropagation();
            if (VRS.timeoutManager)
                VRS.timeoutManager.resetTimer();
            if (!disabled) {
                if (stopPropagation)
                    this._destroyMenu(state);
                if (menuItem.clickCallback)
                    menuItem.clickCallback(menuItem);
                if (menuItem.subItemsNormalised.length > 0)
                    this._refreshChildItems(state, menuItem);
                if (!stopPropagation)
                    this._refreshMenuItem(state, menuItem);
            }
        };
        MenuPlugin.prototype._clickCatcherClicked = function (event) {
            event.stopPropagation();
            this._destroyMenu(this._getState());
        };
        MenuPlugin.prototype._windowResized = function (event) {
            this._destroyMenu(this._getState());
        };
        return MenuPlugin;
    }(JQueryUICustomWidget));
    VRS.MenuPlugin = MenuPlugin;
    $.widget('vrs.vrsMenu', new MenuPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.menu.js.map