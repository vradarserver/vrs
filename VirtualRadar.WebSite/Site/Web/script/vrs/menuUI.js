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

(function(VRS, $, /** object= */ undefined)
{
    //region MenuItem
    /**
     * Describes a menu item.
     * @param {object}                      settings
     * @param {string}                      settings.name               The unique name of the menu item.
     * @param {string|function():string}   [settings.labelKey]          The index into VRS.$$ to show for the menu (mutually exclusive with renderCallback).
     * @param {string|function():string}   [settings.jqIcon]            The name of the jQuery UI icon (without the leading 'ui-icon-') to show against the item if labelKey has been supplied.
     * @param {string|function():string}   [settings.vrsIcon]           The name of the vrsIcon icon (without the leading 'vrsIcon-') to show against the item if labelKey has been supplied.
     * @param {boolean|function():boolean} [settings.checked]           A value indicating that the menu entry should be shown with a checkmark against it, or a function that returns a bool indicating same.
     * @param {string|function():string}   [settings.labelImageUrl]     The URL of the image to use as the label icon.
     * @param {string|function():string}   [settings.labelImageClasses] The classes to apply to the image used as a label icon.
     * @param {function(VRS.MenuItem)}     [settings.clickCallback]     The function that is called when the menu item is clicked. Optional if subItems are supplied.
     * @param {function()}                 [settings.initialise]        An optional method called just before the menu item is used.
     * @param {bool|function():bool}       [settings.disabled]          Either a bool indicating whether the menu item is disabled or a method that returns a bool indicating whether the menu item is disabled. Defaults to false.
     * @param {Array.<VRS.MenuItem>}       [settings.subItems]          An optional array of sub-items.
     * @param {object}                     [settings.tag]               An optional object attached to the menu item.
     * @param {bool}                       [settings.noAutoClose]       An optional flag that prevents the menu from closing when the item is clicked.
     * @param {function():bool}            [settings.suppress]          An optional callback that can return true to prevent the menu option from appearing. Defaults to function that returns false.
     * @constructor
     */
    VRS.MenuItem = function(settings)
    {
        var that = this;

        if(!settings) throw 'You must supply a settings object';
        if(!settings.name) throw 'You must supply a unique name for the menu item';
        if(!settings.labelKey && !settings.renderCallback) throw 'You must supply a label key or a render callback';

        /**
         * The unique name of the menu item.
         * @type {string}
         */
        this.name = settings.name;

        /**
         * The function that is called when the menu item is clicked. Optional if renderCallback is supplied (in which case the render should set up its own click handling).
         * @type {function(VRS.MenuItem)=}
         */
        this.clickCallback = settings.clickCallback;

        /**
         * An array of sub-items. This should not be used by objects that build menus - use subItemsNormalised instead.
         * @type {Array.<VRS.MenuItem>}
         */
        this.subItems = settings.subItems || [];

        /**
         * An array of normalised sub-items. VRS.Menu builds this, you can't supply it. Consumers of menus should use
         * this instead of the subItems array.
         * @type {Array.<VRS.MenuItem>}
         */
        this.subItemsNormalised = [];

        /**
         * Do not close the menu when the item is clicked.
         * @type {boolean}
         */
        this.noAutoClose = !!settings.noAutoClose;

        /**
         * An optional object that is attached to the menu item.
         * @type {Object}
         */
        this.tag = settings.tag;

        /**
         * Returns true if the menu item should not be shown.
         * @type {function(): boolean}
         */
        this.suppress = settings.suppress || function() { return false; };

        /**
         * Gives the menu item the opportunity to initialise itself directly before it is used to generate UI elements.
         */
        this.initialise = function()
        {
            if(settings.initialise) settings.initialise();
        };

        /**
         * Returns true if the menu item is disabled.
         * @returns {boolean}
         */
        this.isDisabled = function()
        {
            return (settings.disabled && $.isFunction(settings.disabled)) ? settings.disabled() : !!(settings.disabled);
        };

        /**
         * Returns the label text to use.
         * @returns {string}
         */
        this.getLabelText = function()
        {
            return VRS.globalisation.getText(settings.labelKey);
        };

        /**
         * Returns the jQueryUI icon or null if there is no jQueryUI icon.
         * @returns {string}
         */
        this.getJQueryIcon = function()
        {
            return settings.jqIcon ? $.isFunction(settings.jqIcon) ? settings.jqIcon() : settings.jqIcon : null;
        };

        /**
         * Returns the VRS icon or null if there is no VRS icon.
         * @returns {}
         */
        this.getVrsIcon = function()
        {
            var result = settings.vrsIcon ? $.isFunction(settings.vrsIcon) ? settings.vrsIcon() : settings.vrsIcon : null;
            if(!settings.vrsIcon && settings.checked !== undefined) {
                var isChecked = $.isFunction(settings.checked) ? settings.checked() : !!settings.checked;
                if(isChecked) result = 'checkmark';
            }

            return result;
        };

        /**
         * Returns the label image URL or null if there is no label image URL.
         * @returns {string}
         */
        this.getLabelImageUrl = function()
        {
            return settings.labelImageUrl ? $.isFunction(settings.labelImageUrl) ? settings.labelImageUrl() : settings.labelImageUrl : null;
        };

        /**
         * Returns the label image classes or null if there are no label image classes.
         * @returns {string}
         */
        this.getLabelImageClasses = function()
        {
            return settings.labelImageClasses ? $.isFunction(settings.labelImageClasses) ? settings.labelImageClasses() : settings.labelImageClasses : null;
        };

        /**
         * Returns a jQuery object wrapping a newly created img element for the label image or null if the menu has no
         * label image URL. Note that the image has no explicit width or height.
         * @returns {jQuery}
         */
        this.getLabelImageElement = function()
        {
            var result = null;

            var url = that.getLabelImageUrl();
            if(url) {
                result = $('<img/>').attr('src', url);
                var classes = that.getLabelImageClasses();
                if(classes) result.addClass(classes);
            }

            return result;
        };
    };
    //endregion

    //region Menu
    /**
     * A class that collects together a bunch of menu items to form a menu structure.
     * @params {Object}                 [settings]
     * @params {Array.<VRS.MenuItem>}   [settings.items]    The menu items to initialise the menu with.
     * @constructor
     */
    VRS.Menu = function(settings)
    {
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.Menu'
        });
        var _Events = {
            beforeAddingFixedMenuItems: 'beforeAddingFixedMenuItems',
            afterAddingFixedMenuItems:  'afterAddingFixedMenuItems'
        };

        // Initialise the settings
        settings = $.extend({
            items:      []
        }, settings);

        /**
         * The top-level items to show when the user asks for the menu. Null entries in the list represent separators.
         * @type {Array.<VRS.MenuItem>}
         * @private
         */
        var _TopLevelMenuItems = [];
        this.getTopLevelMenuItems = function() { return _TopLevelMenuItems; };

        /**
         * Raised before buildMenuItems adds the fixed top-level menu items to the menu structure that it returns. Gets
         * passed the array of menu items as currently constructed - you can modify this array but you must not call
         * buildMenuItems in the handler. Note that multiple consecutive separators will be removed before buildMenuItems
         * returns, you don't need to remove them yourself.
         * @param {function(VRS.Menu, Array.<VRS.MenuItem>)}    callback    The method to call when the event is raised.
         * @param {Object}                                      forceThis   The object to use for 'this' when the method is called.
         * @returns {Object}                                    Hook result.
         */
        this.hookBeforeAddingFixedMenuItems = function(callback, forceThis) { return _Dispatcher.hook(_Events.beforeAddingFixedMenuItems, callback, forceThis); };

        /**
         * Raised after buildMenuItems adds the fixed top-level menu items to the menu structure that it returns. Gets
         * passed the array of menu items as currently constructed - you can modify this array but you must not call
         * buildMenuItems in the handler. Note that multiple consecutive separators will be removed before buildMenuItems
         * returns, you don't need to remove them yourself.
         * @param {function(VRS.Menu, Array.<VRS.MenuItem>)}    callback    The method to call when the event is raised.
         * @param {Object}                                      forceThis   The object to use for 'this' when the method is called.
         * @returns {Object}                                    Hook result.
         */
        this.hookAfterAddingFixedMenuItems = function(callback, forceThis)  { return _Dispatcher.hook(_Events.afterAddingFixedMenuItems, callback, forceThis); };

        /**
         * Unhooks a previously hooked event on the object.
         * @param hookResult
         */
        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };

        /**
         * Constructs the set of menu items to display to the user.
         * @returns {Array.<MenuItem>}
         */
        this.buildMenuItems = function()
        {
            /** @type {Array.<VRS.MenuItem>} */
            var rawItems = [];

            _Dispatcher.raise(_Events.beforeAddingFixedMenuItems, [ this, rawItems ]);
            rawItems = rawItems.concat(_TopLevelMenuItems);
            _Dispatcher.raise(_Events.afterAddingFixedMenuItems, [ this, rawItems ]);

            return normaliseMenu(rawItems);
        };

        /**
         * Returns a copy of the array passed across with unwanted duplicates etc. filtered out.
         * @param {Array.<VRS.MenuItem>} rawItems
         * @return {Array.<VRS.MenuItem>}
         */
        function normaliseMenu(rawItems)
        {
            /** @type {Array.<VRS.MenuItem>} */
            var result = [];

            var previousIsSeparator = true;
            var length = rawItems.length;
            for(var i = 0;i < length;++i) {
                var menuItem = rawItems[i];
                var suppress = menuItem ? menuItem.suppress() : false;
                if(!suppress) {
                    if(menuItem) {
                        previousIsSeparator = false;
                        menuItem.subItemsNormalised = normaliseMenu(menuItem.subItems);
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
         *
         * @param {string}                  name                    The name to search for. Case sensitive.
         * @param {Array.<VRS.MenuItem>}   [menuItems]              The menu items to search through.
         * @param {bool}                   [useNormalisedSubItems]  True to recurse through subItemsNormalised, false to recurse through subItems.
         * @returns {VRS.MenuItem}
         */
        this.findMenuItemForName = function(name, menuItems, useNormalisedSubItems)
        {
            /** type {VRS.MenuItem} */
            var result = null;

            if(!menuItems) menuItems = that.getTopLevelMenuItems();
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
                    result = that.findMenuItemForName(subItems, name, useNormalisedSubItems);
                    if(result) break;
                }
            }

            return result;
        };

        // Construct the object.
        $.each(settings.items, function(/** Number */ idx, /** VRS.MenuItem */ menuItem) {
            _TopLevelMenuItems.push(menuItem);
        });
    };
    //endregion
})(window.VRS = window.VRS || {}, jQuery);