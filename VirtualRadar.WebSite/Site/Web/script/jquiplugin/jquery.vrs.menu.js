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

(function(VRS, $, /** object= */ undefined)
{
    //region Global Options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.menuClass = VRS.globalOptions.menuClass || 'vrsMenu';                 // The class to use on menu placeholders.
    //endregion

    //region MenuPluginState
    VRS.MenuPluginState = function()
    {
        /**
         * The menu items that the menu has rendered.
         * @type {Array.<VRS.MenuItem>}
         */
        this.menuItems = [];

        /**
         * A map of menu item names to the jQuery elements created for them.
         * @type {Object.<string, VRS_MENU_ELEMENTS>}
         */
        this.menuItemElements = {};

        /**
         * The menu trigger element.
         * @type {jQuery}
         */
        this.trigger = null;

        /**
         * The 100% width & height transparent div that is applied just behind the menu which, when clicked, causes
         * the menu to close. We use this approach rather than catching document clicks or mouse downs because it doesn't
         * get fooled by elements that stop propagation of clicks to the document / body / whatever.
         * @type {jQuery}
         */
        this.clickCatcher = null;

        /**
         * The container for the menu. If this exists then the menu is open.
         * @type {jQuery}
         */
        this.menuContainer = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionDialog attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsMenu}
     */
    VRS.jQueryUIHelper.getMenuPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsMenu'); };

    /**
     * Returns the default options for a menu widget with optional overrides.
     * @param {VRS_OPTIONS_MENU=} overrides
     * @returns {VRS_OPTIONS_MENU}
     */
    VRS.jQueryUIHelper.getMenuOptions = function(overrides)
    {
        return $.extend({
            menu:                   null,                   // The menu to render.
            showButtonTrigger:      true,                   // Show a trigger button with no text. Mutually exclusive with triggerElement.
            triggerElement:         null,                   // The jQuery element to use as a trigger. Mutually exclusive with showButtonTrigger.
            menuContainerClasses:   null,                   // Classes to add to the menu container.
            offsetX:                0,                      // The offset to add to the X axis when placing the menu.
            offsetY:                5,                      // The offset to add to the Y axis when placing the menu.
            alignment:              VRS.Alignment.Centre,   // How to align the menu relative to the trigger element.
            cssMenuWidth:           300,                    // The width, in pixels, of the menu as specified by the CSS.
            zIndex:                 99999,                  // The z-index at which the menu will be displayed. Must be higher than all other z-indexes in use.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsMenu jQueryUI widget
    /**
     * @namespace VRS.vrsMenu
     */
    $.widget('vrs.vrsMenu', {
        //region -- options
        /** @type {VRS_OPTIONS_MENU} */
        options: VRS.jQueryUIHelper.getMenuOptions(),
        //endregion

        //region -- _getState, _create
        /**
         * Returns the state associated with the plugin, creating it if it doesn't already exist.
         * @returns {VRS.MenuPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('vrsMenuState');
            if(result === undefined) {
                result = new VRS.MenuPluginState();
                this.element.data('vrsMenuState', result);
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

            this.element.addClass(VRS.globalOptions.menuClass);

            var trigger = options.triggerElement;
            if(options.showButtonTrigger) trigger = $('<span/>').addClass('vrsIcon vrsIcon-cog colourButton vrsNoHighlight');

            state.trigger = trigger
                .on('click', $.proxy(this._triggerPressed, this))
                .appendTo(this.element);
        },

        /**
         * Releases any resource allocated by the menu.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();

            this._destroyMenu(state);

            if(state.trigger) state.trigger.off();
            state.trigger = null;

            state.menuItemElements = {};
            state.menuItems = [];
        },
        //endregion

        //region -- _createMenu, _destroyMenu
        /**
         * Hierarchically creates elements for the menu items passed across and attaches them to the parent jQuery object.
         * Creates a dlmenu object from the top-level of the menu.
         * @param {VRS.MenuPluginState}     state
         * @private
         */
        _createMenu: function(state)
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
        },

        /**
         * Destroys the menu and removes it from the UI.
         * @param {VRS.MenuPluginState} state
         * @private
         */
        _destroyMenu: function(state)
        {
            if(state.menuContainer) {
                var options = this.options;

                $.each(state.menuItemElements, function(/** Number */ idx, /** VRS_MENU_ELEMENTS */ elements) {
                    if(elements.link) elements.link.off();
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
        },

        /**
         * Calculates the top-left position of the menu.
         * @param {VRS.MenuPluginState} state
         * @returns {{ top: Number=, bottom: Number=, left: Number= }}
         * @private
         */
        _determineTopLeft: function(state)
        {
            var options = this.options;
            var trigger = state.trigger;
            /** @type {{left: number, top: number}} */
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
        },

        /**
         * Recursively creates the menu structure.
         * @param {VRS.MenuPluginState} state
         * @param {jQuery}              parentJQ
         * @param {VRS.MenuItem[]}      menuItems
         * @private
         */
        _createMenuItemElements: function(state, parentJQ, menuItems)
        {
            var self = this;

            var list = $('<ul/>')
                .addClass(parentJQ === state.menuContainer ? 'dl-menu' : 'dl-submenu')
                .appendTo(parentJQ);

            var previousListItem = null;
            $.each(menuItems, function(idx, /** @type {VRS.MenuItem} */ menuItem) {
                if(!menuItem) {
                    if(previousListItem) previousListItem.addClass('dl-separator');
                } else {
                    menuItem.initialise();
                    var isDisabled = menuItem.isDisabled();

                    if(state.menuItemElements[menuItem.name]) throw 'There are at least two menu items called ' + menuItem.name + ' - menu item names must be unique';
                    var listItem = $('<li/>')
                        .appendTo(list);

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

                    var link = $('<a/>')
                        .attr('href', '#')
                        .append(imageElement)
                        .appendTo(listItem),
                    text = $('<span/>')
                        .text(menuItem.getLabelText())
                        .appendTo(link);

                    if(isDisabled) listItem.addClass('dl-disabled');
                    if(menuItem.clickCallback || menuItem.subItemsNormalised.length) link.click($.proxy(function(event) { self._menuItemClicked(event, menuItem); }, self));
                    if(menuItem.subItemsNormalised.length) self._createMenuItemElements(state, listItem, menuItem.subItemsNormalised);

                    state.menuItemElements[menuItem.name] = {
                        listItem:   listItem,
                        image:      imageElement,
                        link:       link,
                        text:       text
                    };
                    previousListItem = listItem;
                }
            });
        },

        /**
         * Refreshes the labels for the child items of the menu item.
         * @param {VRS.MenuPluginState} state
         * @param {VRS.MenuItem}        menuItem
         * @private
         */
        _refreshChildItems: function(state, menuItem)
        {
            $.each(menuItem.subItemsNormalised, function(/** Number */ idx, /** VRS.MenuItem */ subItem) {
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
        },
        //endregion

        //region -- getIsOpen, toggleMenu, openMenu, closeMenu, _triggerPressed
        /**
         * Returns true if the menu is open.
         * @param {VRS.MenuPluginState} [state]     Leave undefined. For internal use.
         * @returns {boolean}
         */
        getIsOpen: function(state)
        {
            state = state || this._getState();
            return !!state.menuContainer;
        },

        /**
         * Opens the menu unless it's already open, in which case it closes it.
         * @param {VRS.MenuPluginState} [state]     Leave undefined. For internal use.
         */
        toggleMenu: function(state)
        {
            state = state || this._getState();

            if(this.getIsOpen(state)) {
                this.closeMenu(state);
            } else {
                this.openMenu(state);
            }
        },

        /**
         * Opens the menu. Does nothing if it's already open.
         * @param {VRS.MenuPluginState} [state]     Leave undefined. For internal use.
         */
        openMenu: function(state)
        {
            state = state || this._getState();

            if(!state.menuContainer) {
                this._createMenu(state);
                state.menuContainer.dlmenu('openMenu');
            }
        },

        /**
         * Closes the menu. Does nothing if it's already closed.
         * @param {VRS.MenuPluginState} [state]     Leave undefined. For internal use.
         */
        closeMenu: function(state)
        {
            state = state || this._getState();

            if(state.menuContainer) {
                this._destroyMenu(state);
            }
        },

        /**
         * Called when the trigger element is pressed.
         * @params {Event} event
         * @private
         */
        _triggerPressed: function(event)
        {
            this.toggleMenu();
            event.stopPropagation();
        },
        //endregion

        //region -- _menuItemClicked, _clickCatcherClicked, _windowResized
        /**
         * Called when the user clicks an item in the menu. Also called automatically when the user clicks on a menu
         * item that has child items but has no click callback.
         * @param {Event}           event
         * @param {VRS.MenuItem}    menuItem
         * @private
         */
        _menuItemClicked: function(event, menuItem)
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
            }
        },

        /**
         * Called when the click catcher element is clicked while the menu is active.
         * @param {Event} event
         * @private
         */
        _clickCatcherClicked: function(event)
        {
            event.stopPropagation();
            this._destroyMenu(this._getState());
        },

        /**
         * Called when the window gets resized.
         * @param {Event} event
         * @private
         */
        _windowResized: function(event)
        {
            this._destroyMenu(this._getState());
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
