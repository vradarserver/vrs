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
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview Code that manages multiple full-width pages, of which only one is shown at a time. Intended for use on
 * the mobile site but could be used on desktop sites too.
 */

(function(VRS, $, /** Object= */ undefined)
{
    //region Page
    /**
     * Describes a page for the page manager.
     * @param {Object}              settings
     * @param {string}              settings.name                   The unique name of the page.
     * @param {jQuery}              settings.element                The top-level element of the page.
     * @param {function(bool)=}     settings.visibleCallback        A method that is called whenever the page is hidden or made visible. Passed true if the page is about to be made visible and false if it has just been hidden.
     * @param {function()=}         settings.afterVisibleCallback   A method that is called after the page has been made visible.
     * @constructor
     */
    VRS.Page = function(settings)
    {
        if(!settings) throw 'You must supply a settings object';
        if(!settings.name) throw 'You must supply a unique name';
        if(!settings.element) throw 'You must supply the top-level element';

        var that = this;

        /** @type {string} */
        this.name = settings.name;

        /** @type {jQuery} */
        this.element = settings.element;

        /** @type {function(bool)} */
        this.visibleCallback = settings.visibleCallback || function() { };

        /** @type {function()} */
        this.afterVisibleCallback = settings.afterVisibleCallback || function() { };

        /**
         * The container that the element is wrapped with.
         * @type {jQuery}
         */
        this.container = null;

        /**
         * True if the page is visible, false if it is not.
         * @type {boolean}
         */
        this.isVisible = true;

        /**
         * The original parent of the element.
         * @type {Object}
         */
        this.originalParent = null;

        /**
         * For internal use. Holds the CSS on element before any modifications by the page manager.
         * @type {Object}
         */
        this.originalCss = null;

        /**
         * Releases any resources held by the page. Moves the page back to its original parent.
         */
        this.dispose = function()
        {
            if(that.originalCss)    that.element.css(that.originalCss);
            if(that.originalParent) that.originalParent.append(that.element);
            else                    that.element.remove();
            if(that.container) that.container.remove();
            that.container = null;
            that.originalCss = null;
            that.originalParent = null;
        };
    };
    //endregion

    //region PageManager
    VRS.PageManager = function()
    {
        //region -- Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.PageManager'
        });
        var _Events = {
            hidingPage:     'hidingPage',
            hiddenPage:     'hiddenPage',
            showingPage:    'showingPage',
            shownPage:      'shownPage'
        };

        /**
         * The element that the page manager attaches all pages to.
         * @type {jQuery}
         * @private
         */
        var _Element = null;

        /**
         * The CSS that needs to be applied to the element to return it to the pre-initialised state.
         * @type {Object}
         * @private
         */
        var _ElementOriginalCss = null;

        /**
         * The collection of pages that have been added to the manager.
         * @type {Array.<VRS.Page>}
         * @private
         */
        var _Pages = [];

        /**
         * The page that is currently visible.
         * @type {VRS.Page}
         * @private
         */
        var _VisiblePage = null;
        //endregion

        //region -- dispose
        /**
         * Releases all of the resources held by the object.
         */
        this.dispose = function()
        {
            var length = _Pages.length;
            for(var i = length - 1;i >= 0;--i) {
                var page = _Pages[i];
                page.dispose();
            }
            _Pages = [];
            if(_Element && _ElementOriginalCss) _Element.css(_ElementOriginalCss);
            _Element = null;
            _ElementOriginalCss = null;

            $(window).off('resize.vrspagemanager', windowResized);
        };
        //endregion

        //region -- Events exposed
        /**
         * Called before a page is hidden.
         * @param {function(VRS.Page)}  callback
         * @param {Object}              forceThis
         * @returns {Object}
         */
        this.hookHidingPage = function(callback, forceThis) { return _Dispatcher.hook(_Events.hidingPage, callback, forceThis); };

        /**
         * Called after a page has been hidden.
         * @param {function(VRS.Page)}  callback
         * @param {Object}              forceThis
         * @returns {Object}
         */
        this.hookHiddenPage = function(callback, forceThis) { return _Dispatcher.hook(_Events.hiddenPage, callback, forceThis); };

        /**
         * Called before a page is shown.
         * @param {function(VRS.Page)}  callback
         * @param {Object}              forceThis
         * @returns {Object}
         */
        this.hookShowingPage = function(callback, forceThis) { return _Dispatcher.hook(_Events.showingPage, callback, forceThis); };

        /**
         * Called after a page has been shown.
         * @param {function(VRS.Page)}  callback
         * @param {Object}              forceThis
         * @returns {Object}
         */
        this.hookShownPage = function(callback, forceThis) { return _Dispatcher.hook(_Events.shownPage, callback, forceThis); };

        /**
         * Raises an event.
         * @param {string}      event   The name of the event to raise.
         * @param {VRS.Page}    page    The page to pass as a parameter to the event.
         */
        function raiseEvent(event, page)
        {
            _Dispatcher.raise(event, [ page ]);
        }

        /**
         * Unhooks a hooked event.
         * @param hookResult
         */
        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- initialise
        /**
         * Initialises the page manager.
         * @param {jQuery} element The element to which all pages will be attached. The CSS for this element gets modified.
         */
        this.initialise = function(element)
        {
            if(_Element) throw 'The page manager cannot be initialised more than once';
            if(!element) throw 'You must supply an element for the page manager to use';

            _Element = element;
            _ElementOriginalCss = _Element.css([
                'width',
                'height',
                'position'
            ]);
            _Element.css({
                width:  '100%',
                height: '100%',
                position: 'relative'
            });

            $(window).on('resize.vrspagemanager', $.proxy(windowResized, this));
        };
        //endregion

        //region -- addPage, removePage
        /**
         * Creates a page from a jQuery element. If this is the first page that's been added then it remains on screen,
         * otherwise it is hidden.
         * @param {VRS.Page} page The page to add.
         */
        this.addPage = function(page)
        {
            if(!_Element) throw 'You must initialise the page manager before calling addPage';
            if(!page) throw 'You must supply a page';
            if(findPage(page.name)) throw 'A page already exists with the name ' + page.name;

            _Pages.push(page);

            page.originalParent = page.element.parent();
            page.originalCss = page.element.css([
                'width',
                'height'
            ]);

            page.container = $('<div/>')
                .css({
                    width:  '100%',
                    height: '100%'
                })
                .appendTo(_Element)
                .append(page.element);

            page.element.css({
                width:  '100%',
                height: '100%'
            });

            if(VRS.refreshManager) {
                VRS.refreshManager.registerOwner(page.container);
                VRS.refreshManager.rebuildRelationships();
            }

            if(_Pages.length === 1) showPage(page);
            else                    hidePage(page);
        };

        //noinspection JSUnusedGlobalSymbols
        /**
         * Removes a page from the manager. The page is either returned to its original parent or, if it had no parent,
         * it is just removed from the window.
         * @param {string|VRS.Page} pageOrName  The page or name of a page to remove.
         */
        this.removePage = function(pageOrName)
        {
            var index = findPageIndex(pageOrName);
            var page = index === -1 ? null : _Pages[index];
            if(page) {
                if(VRS.refreshManager) VRS.refreshManager.unregisterOwner(page.container);

                if(page === _VisiblePage) {
                    if(index + 1 < _Pages.length) that.show(_Pages[index + 1]);
                    else if(index > 0)            that.show(_Pages[index - 1]);
                    else                          _VisiblePage = null;
                }
                _Pages.splice(index, 1);
                page.dispose();

                if(VRS.refreshManager) VRS.refreshManager.rebuildRelationships();
            }
        };
        //endregion

        //region -- show
        /**
         * Hides or shows a page.
         * @param {string|VRS.Page} pageOrName  Either the name of the page to show or the page itself.
         */
        this.show = function(pageOrName)
        {
            var page = findPage(pageOrName);
            if(!page.isVisible) {
                if(_VisiblePage) hidePage(_VisiblePage);
                showPage(page);
            }
        };
        //endregion

        //region hidePage, showPage
        /**
         * Hides a page from view.
         * @param {VRS.Page} page
         */
        function hidePage(page)
        {
            if(page.isVisible) {
                page.isVisible = false;
                raiseEvent(_Events.hidingPage, page);
                page.container.hide();
                raiseEvent(_Events.hiddenPage, page);
                page.visibleCallback(false);
            }
            if(page === _VisiblePage) _VisiblePage = null;
        }

        /**
         * Brings a page into view.
         * @param {VRS.Page} page
         */
        function showPage(page)
        {
            if(!page.isVisible) {
                page.isVisible = true;
                page.visibleCallback(true);
                raiseEvent(_Events.showingPage, page);
                page.container.show();
                raiseEvent(_Events.shownPage, page);
                page.afterVisibleCallback();

                if(VRS.refreshManager) VRS.refreshManager.refreshTargets(page.container);
            }
            _VisiblePage = page;
        }
        //endregion

        //region -- findPage, findPageIndex
        /**
         * If passes the name of a page then this returns the registered VRS.Page with that name, otherwise if passed a
         * page then it simply gets returned. Returns null if no page exists for the name passed across.
         * @param {VRS.Page|string} nameOrPage
         * @returns {VRS.Page}
         */
        function findPage(nameOrPage)
        {
            /** @type {VRS.Page} */
            var result = nameOrPage;

            if(!(result instanceof VRS.Page)) {
                result = null;
                var length = _Pages.length;
                for(var i = 0;i < length;++i) {
                    var page = _Pages[i];
                    if(page.name === nameOrPage) {
                        result = page;
                        break;
                    }
                }
            }

            return result;
        }

        /**
         * Given a page or the name of a page this returns the index of the associated page within _Pages. Returns -1
         * if no page could be found.
         * @param {VRS.Page|string} nameOrPage
         * @returns {number}
         */
        function findPageIndex(nameOrPage)
        {
            var result = -1;

            var length = _Pages.length;
            for(var i = 0;i < length;++i) {
                var page = _Pages[i];
                if(nameOrPage === page || nameOrPage === page.name) {
                    result = i;
                    break;
                }
            }

            return result;
        }
        //endregion

        //region -- Events subscribed
        /**
         * Called when the window is resized.
         */
        function windowResized()
        {
            if(_VisiblePage && VRS.refreshManager) VRS.refreshManager.refreshTargets(_VisiblePage.container);
        }
        //endregion
    };
    //endregion

    //region Singletons
    VRS.pageManager = new VRS.PageManager();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
