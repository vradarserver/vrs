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
 * @fileoverview A jQuery UI plugin that renders a VRS.Menu.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.menuClass = VRS.globalOptions.menuClass || 'vrsMenu';                 // The class to use on menu placeholders.

    /**
     * Collects together the elements created for a menu item.
     */
    interface IMenuItemElements
    {
        listItem:    JQuery;
        image?:      JQuery;
        link:        JQuery;
        text:        JQuery;
        slider?:     JQuery;
    }

    /**
     * Describes the location and height of a menu panel.
     */
    interface ITopBottomLeft extends JQueryCoordinates
    {
        top:        number;
        bottom:     number;
        left:       number;
    }

    /**
     * Describes an HTML container to add to something and a JQuery UI element contained within.
     */
    interface IContainerAndElement
    {
        container:  JQuery;
        element:    JQuery;
    }

    /**
     * Plugin state
     */
    class MenuPlugin_State
    {
        /**
         * The menu items that the menu has rendered.
         */
        menuItems: MenuItem[] = [];

        /**
         * A map of menu item names to the jQuery elements created for them.
         */
        menuItemElements: { [name: string]: IMenuItemElements } = {};

        /**
         * The menu trigger element.
         */
        trigger: JQuery = null;

        /**
         * The 100% width & height transparent div that is applied just behind the menu which, when clicked, causes
         * the menu to close. We use this approach rather than catching document clicks or mouse downs because it doesn't
         * get fooled by elements that stop propagation of clicks to the document / body / whatever.
         */
        clickCatcher: JQuery = null;

        /**
         * The container for the menu. If this is not null then the menu is open.
         */
        menuContainer: JQuery = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};

    VRS.jQueryUIHelper.getMenuPlugin = function(jQueryElement: JQuery) : MenuPlugin
    {
        return <MenuPlugin>jQueryElement.data('vrsVrsMenu');
    }

    VRS.jQueryUIHelper.getMenuOptions = function(overrides?: MenuPlugin_Options) : MenuPlugin_Options
    {
        return $.extend({
            menu:                   null,
            showButtonTrigger:      true,
            triggerElement:         null,
            menuContainerClasses:   null,
            offsetX:                0,
            offsetY:                5,
            alignment:              VRS.Alignment.Centre,
            cssMenuWidth:           300,
            zIndex:                 99999,

            __nop: null
        }, overrides);
    }

    /**
     * The options that can be passed when creating a MenuPlugin.
     */
    export interface MenuPlugin_Options extends JQueryUICustomWidget_Options
    {
        /**
         * The menu to render.
         */
        menu: Menu;

        /**
         * True to show a trigger button with no text. Mutually exclusive with triggerElement.
         */
        showButtonTrigger?: boolean;

        /**
         * The jQuery element to use as a trigger. Mutually exclusive with showButtonTrigger.
         */
        triggerElement?: JQuery;

        /**
         * Classes to add to the menu container.
         */
        menuContainerClasses?: string;

        /**
         * The offset to add to the X axis when placing the menu.
         */
        offsetX?: number;

        /**
         * The offset to add to the Y axis when placing the menu.
         */
        offsetY?: number;

        /**
         * How to align the menu relative to the trigger element.
         * (VRS.Alignment)
         */
        alignment?: string;

        /**
         * The width, in pixels, of the menu as specified by the CSS.
         */
        cssMenuWidth?: number;

        /**
         * The z-index at which the menu will be displayed. Must be higher than all other z-indexes in use.
         */
        zIndex?: number;
    }

    /**
     * A widget that can render a menu structure.
     */
    export class MenuPlugin extends JQueryUICustomWidget
    {
        options: MenuPlugin_Options;

        constructor()
        {
            super();
            this.options = jQueryUIHelper.getMenuOptions();
        }

        private _getState() : MenuPlugin_State
        {
            var result = this.element.data('vrsMenuState');
            if(result === undefined) {
                result = new MenuPlugin_State();
                this.element.data('vrsMenuState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.menuClass);

            var trigger = options.triggerElement;
            if(options.showButtonTrigger) trigger = $('<span/>').addClass('vrsIcon vrsIcon-cog colourButton vrsNoHighlight');

            state.trigger = trigger
                .on('click', $.proxy(this._triggerPressed, this))
                .appendTo(this.element);
        }

        _destroy()
        {
            var state = this._getState();

            this._destroyMenu(state);

            if(state.trigger) state.trigger.off();
            state.trigger = null;

            state.menuItemElements = {};
            state.menuItems = [];
        }

        /**
         * Hierarchically creates elements for the menu items passed across and attaches them to the parent jQuery object.
         * Creates a dlmenu object from the top-level of the menu.
         */
        private _createMenu(state: MenuPlugin_State)
        {
            var options = this.options;

            state.menuItems = this.options.menu.buildMenuItems();
            state.menuContainer = $('<div/>')
                .addClass('dl-menuwrapper')
                .css({
                    'z-index': options.zIndex
                });
            if(options.menuContainerClasses) state.menuContainer.addClass(options.menuContainerClasses);
            if(options.showButtonTrigger) state.trigger.removeClass('colourButton').addClass('colourButtonActive');

            this._createMenuItemElements(state, state.menuContainer, state.menuItems);

            var body = $('body');
            var position = this._determineTopLeft(state);
            state.menuContainer
                .css(position)
                .prependTo(body);

            state.clickCatcher = $('<div/>')
                .css({
                    'width':    '100%',
                    'height':   '100%',
                    'position': 'fixed',
                    'top':      0,
                    'left':     0,
                    'z-index':  options.zIndex - 1
                })
                .on('click.vrsMenu', $.proxy(this._clickCatcherClicked, this))
                .prependTo(body);

            state.menuContainer.dlmenu({
                backLinkText: VRS.$$.MenuBack
            });

            $(window).on('resize.vrsMenu', $.proxy(this._windowResized, this));
            if(VRS.refreshManager) VRS.refreshManager.registerTarget(this.element, $.proxy(function() {
                this._destroyMenu(state);
            }, this));
        }

        /**
         * Destroys the menu and removes it from the UI.
         */
        private _destroyMenu(state: MenuPlugin_State)
        {
            if(state.menuContainer) {
                var options = this.options;

                $.each(state.menuItemElements, function(idx: number, element: IMenuItemElements) {
                    if(element.slider) element.slider.slider('destroy');
                    if(element.link) element.link.off();
                });
                state.menuItemElements = {};

                if(state.clickCatcher) {
                    state.clickCatcher.off('click.vrsMenu');
                    state.clickCatcher.remove();
                }
                state.clickCatcher = null;

                state.menuContainer.dlmenu('dispose');
                state.menuContainer.remove();
                state.menuContainer = null;

                if(options.showButtonTrigger) state.trigger.removeClass('colourButtonActive').addClass('colourButton');

                $(window).off('resize.vrsMenu');
                if(VRS.refreshManager) VRS.refreshManager.unregisterTarget(this.element);
            }
        }

        /**
         * Calculates the top-left position of the menu.
         */
        private _determineTopLeft(state: MenuPlugin_State) : ITopBottomLeft
        {
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
            if(pixelsAboveTarget > pixelsBelowTarget) bottom = windowSize.height - pixelsAboveTarget;
            else                                      top = windowSize.height - pixelsBelowTarget;

            var left = offset.left + options.offsetX;
            var width = options.cssMenuWidth;
            switch(options.alignment) {
                case VRS.Alignment.Right:   left -= (width - triggerSize.width); break;
                case VRS.Alignment.Centre:  left -= (width - triggerSize.width) / 2; break;
            }
            left = Math.min(left, windowSize.width - width);
            left = Math.max(left, 0);

            return {
                top: top,
                bottom: bottom,
                left: left
            };
        }

        /**
         * Recursively creates the menu structure.
         */
        private _createMenuItemElements(state: MenuPlugin_State, parentJQ: JQuery, menuItems: MenuItem[])
        {
            var self = this;

            var list = $('<ul/>')
                .addClass(parentJQ === state.menuContainer ? 'dl-menu' : 'dl-submenu')
                .appendTo(parentJQ);

            var previousListItem = null;
            $.each(menuItems, function(idx, menuItem) {
                if(!menuItem) {
                    if(previousListItem) previousListItem.addClass('dl-separator');
                } else {
                    menuItem.initialise();
                    var isDisabled = menuItem.isDisabled();

                    if(state.menuItemElements[menuItem.name]) throw 'There are at least two menu items called ' + menuItem.name + ' - menu item names must be unique';
                    var listItem = $('<li/>')
                        .appendTo(list);

                    var imageElement = self._buildMenuItemImageElement(state, menuItem);
                    var link = $('<a/>')
                        .attr('href', '#')
                        .append(imageElement)
                        .appendTo(listItem);
                    var text = self._buildMenuItemTextElement(state, menuItem)
                        .appendTo(link);

                    var sliderContainerAndElement = self._buildMenuItemSliderElement(state, menuItem);
                    if(sliderContainerAndElement) {
                        sliderContainerAndElement.container.appendTo(link);
                    }

                    if(isDisabled) listItem.addClass('dl-disabled');
                    if(menuItem.clickCallback || menuItem.subItemsNormalised.length) link.click($.proxy(function(event) { self._menuItemClicked(event, menuItem); }, self));
                    if(menuItem.subItemsNormalised.length) self._createMenuItemElements(state, listItem, menuItem.subItemsNormalised);

                    state.menuItemElements[menuItem.name] = {
                        listItem:   listItem,
                        image:      imageElement,
                        link:       link,
                        text:       text,
                        slider:     sliderContainerAndElement ? sliderContainerAndElement.element : null
                    };
                    previousListItem = listItem;
                }
            });
        }

        private _buildMenuItemImageElement(state: MenuPlugin_State, menuItem: MenuItem) : JQuery
        {
            var isDisabled = menuItem.isDisabled();
            var jqIcon = menuItem.getJQueryIcon();
            var vrsIcon = menuItem.getVrsIcon();
            var iconImage = menuItem.getLabelImageUrl();
            var showIcon = jqIcon || vrsIcon || !iconImage;
            var imageElement;
            if(showIcon) {
                imageElement = $('<span/>').addClass(jqIcon  ? 'dl-icon ui-icon ui-icon-' + jqIcon :
                                                     vrsIcon ? 'dl-icon colourButton vrsIcon vrsIcon-' + vrsIcon
                                                             : 'dl-noicon');
            } else {
                imageElement = menuItem.getLabelImageElement().addClass('dl-iconImage');
                var labelImageClasses = menuItem.getLabelImageClasses();
                if(labelImageClasses) imageElement.addClass(labelImageClasses);
            }
            if(isDisabled) imageElement.addClass('dl-disabled');

            return imageElement;
        }

        private _buildMenuItemTextElement(state: MenuPlugin_State, menuItem: MenuItem) : JQuery
        {
            var textElement = $('<span/>')
                .text(menuItem.getLabelText());

            return textElement;
        }

        private _buildMenuItemSliderElement(state: MenuPlugin_State, menuItem: MenuItem) : IContainerAndElement
        {
            var result: IContainerAndElement = null;

            if(menuItem.showSlider()) {
                var valueSpan = $('<span></span>')
                    .text(menuItem.getSliderInitialValue());

                var sliderElement = $('<div></div>').slider({
                    min: menuItem.getSliderMinimum(),
                    max: menuItem.getSliderMaximum(),
                    step: menuItem.getSliderStep(),
                    value: menuItem.getSliderInitialValue(),
                    change: (event, ui) => {
                        valueSpan.text(ui.value);
                        menuItem.callSliderCallback(ui.value);
                    }
                });

                var resetAndText = $('<div class="dl-menu-slider-value"></div>')
                    .append(valueSpan);

                if(menuItem.getSliderDefaultValue() !== null) {
                    resetAndText.append(
                        $('<span class="vrsIcon vrsIconButton vrsIcon-close "></span>')
                        .on('click', (e: JQueryEventObject) => {
                            sliderElement.slider('value', menuItem.getSliderDefaultValue());
                            e.stopPropagation();
                        })
                    );
                }

                result = {
                    container: $('<div></div>')
                        .append(sliderElement)
                        .append(resetAndText),

                    element: sliderElement
                };
            }

            return result;
        }

        private _refreshMenuItem(state: MenuPlugin_State, menuItem: MenuItem)
        {
            var newImage = this._buildMenuItemImageElement(state, menuItem);
            var newText  = this._buildMenuItemTextElement(state, menuItem);

            var elements = state.menuItemElements[menuItem.name];
            elements.image.replaceWith(newImage);
            elements.text.replaceWith(newText);

            elements.image = newImage;
            elements.text = newText;
        }

        /**
         * Refreshes the labels for the child items of the menu item.
         */
        private _refreshChildItems(state: MenuPlugin_State, menuItem: MenuItem)
        {
            $.each(menuItem.subItemsNormalised, function(idx, subItem) {
                if(subItem) {
                    var elements = state.menuItemElements[subItem.name];
                    if(elements) {
                        if(elements.listItem) {
                            var wasDisabled = elements.listItem.hasClass('dl-disabled');
                            var nowDisabled = subItem.isDisabled();
                            if(wasDisabled !== nowDisabled) {
                                if(wasDisabled) elements.listItem.removeClass('dl-disabled');
                                else            elements.listItem.addClass('dl-disabled');
                            }
                        }
                        if(elements.text) elements.text.text(subItem.getLabelText());
                    }
                }
            });
        }

        /**
         * Returns true if the menu is open.
         */
        getIsOpen() : boolean
        {
            return this.doGetIsOpen();
        }
        private doGetIsOpen(state?: MenuPlugin_State) : boolean
        {
            state = state || this._getState();
            return !!state.menuContainer;
        }

        /**
         * Opens the menu unless it's already open, in which case it closes it.
         */
        toggleMenu()
        {
            this.doToggleMenu();
        }
        private doToggleMenu(state?: MenuPlugin_State)
        {
            state = state || this._getState();

            if(this.doGetIsOpen(state)) {
                this.doCloseMenu(state);
            } else {
                this.doOpenMenu(state);
            }
        }

        /**
         * Opens the menu. Does nothing if it's already open.
         */
        openMenu()
        {
            this.doOpenMenu();
        }
        private doOpenMenu(state?: MenuPlugin_State)
        {
            state = state || this._getState();

            if(!state.menuContainer) {
                this._createMenu(state);
                state.menuContainer.dlmenu('openMenu');
            }
        }

        /**
         * Closes the menu. Does nothing if it's already closed.
         */
        closeMenu()
        {
            this.doCloseMenu();
        }
        private doCloseMenu(state?: MenuPlugin_State)
        {
            state = state || this._getState();

            if(state.menuContainer) {
                this._destroyMenu(state);
            }
        }

        /**
         * Called when the trigger element is pressed.
         * @params {Event} event
         * @private
         */
        private _triggerPressed(event: Event)
        {
            this.toggleMenu();
            event.stopPropagation();
        }

        /**
         * Called when the user clicks an item in the menu. Also called automatically when the user clicks on a menu
         * item that has child items but has no click callback.
         */
        private _menuItemClicked(event: Event, menuItem: MenuItem)
        {
            var state = this._getState();

            var elements = state.menuItemElements[menuItem.name];
            var disabled = elements.listItem && elements.listItem.hasClass('dl-disabled');
            var stopPropagation = disabled || (!menuItem.noAutoClose && menuItem.clickCallback);
            if(stopPropagation) event.stopPropagation();

            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

            if(!disabled) {
                if(stopPropagation) this._destroyMenu(state);
                if(menuItem.clickCallback) menuItem.clickCallback(menuItem);
                if(menuItem.subItemsNormalised.length > 0) this._refreshChildItems(state, menuItem);
                if(!stopPropagation) this._refreshMenuItem(state, menuItem);
            }
        }

        /**
         * Called when the click catcher element is clicked while the menu is active.
         */
        private _clickCatcherClicked(event: Event)
        {
            event.stopPropagation();
            this._destroyMenu(this._getState());
        }

        /**
         * Called when the window gets resized.
         */
        private _windowResized(event: Event)
        {
            this._destroyMenu(this._getState());
        }
    }

    $.widget('vrs.vrsMenu', new MenuPlugin());
}

declare interface JQuery
{
    vrsMenu();
    vrsMenu(options: VRS.MenuPlugin_Options);
    vrsMenu(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
