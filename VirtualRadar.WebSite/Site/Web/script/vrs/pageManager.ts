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
 * @fileoverview Code that manages multiple full-width pages, of which only one is shown at a time. Intended for use on
 * the mobile site but could be used on desktop sites too.
 */

namespace VRS
{
    /**
     * The settings to pass when creating a new Page object.
     */
    export interface Page_Settings
    {
        /**
         * The unique name of the page.
         */
        name: string;

        /**
         * The top-level element of the page.
         */
        element: JQuery;

        /**
         * A method that is called whenever the page is hidden or made visible. Passed true if the page is about to be made visible and false if it has just been hidden.
         */
        visibleCallback?: (willBecomeVisible: boolean) => void;

        /**
         * A method that is called after the page has been made visible.
         */
        afterVisibleCallback?: () => void;
    }

    /**
     * Describes a page for the page manager.
     */
    export class Page
    {
        // Kept as public fields for backwards compatibility
        name: string;
        element: JQuery;
        visibleCallback: (willBecomeVisible: boolean) => void;
        afterVisibleCallback: () => void;
        container: JQuery = null;
        isVisible = true;
        originalParent: JQuery = null;
        originalCss: Object = null;

        constructor(settings: Page_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(!settings.name) throw 'You must supply a unique name';
            if(!settings.element) throw 'You must supply the top-level element';

            this.name = settings.name;
            this.element = settings.element;
            this.visibleCallback = settings.visibleCallback || function() { };
            this.afterVisibleCallback = settings.afterVisibleCallback || function() { };
        }

        /**
         * Releases any resources held by the page. Moves the page back to its original parent.
         */
        dispose()
        {
            if(this.originalCss)    this.element.css(this.originalCss);
            if(this.originalParent) this.originalParent.append(this.element);
            else                    this.element.remove();
            if(this.container) this.container.remove();
            this.container = null;
            this.originalCss = null;
            this.originalParent = null;
        }
    }

    /**
     * The object that manages pages for the mobile versions of the site.
     */
    export class PageManager
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.PageManager'
        });
        private _Events = {
            hidingPage:     'hidingPage',
            hiddenPage:     'hiddenPage',
            showingPage:    'showingPage',
            shownPage:      'shownPage'
        };
        private _Element: JQuery = null;                // The element that the page manager attaches all pages to.
        private _ElementOriginalCss: Object = null;     // The CSS that needs to be applied to the element to return it to the pre-initialised state.
        private _Pages: Page[] = [];                    // The collection of pages that have been added to the manager.
        private _VisiblePage: Page = null;              //  The page that is currently visible.

        /**
         * Releases all of the resources held by the object.
         */
        dispose()
        {
            var length = this._Pages.length;
            for(var i = length - 1;i >= 0;--i) {
                var page = this._Pages[i];
                page.dispose();
            }
            this._Pages = [];
            if(this._Element && this._ElementOriginalCss) {
                this._Element.css(this._ElementOriginalCss);
            }
            this._Element = null;
            this._ElementOriginalCss = null;

            $(window).off('resize.vrspagemanager', this.windowResized);
        }

        /**
         * Called before a page is hidden.
         */
        hookHidingPage(callback: (page: Page) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.hidingPage, callback, forceThis);
        }

        /**
         * Called after a page has been hidden.
         */
        hookHiddenPage(callback: (page: Page) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.hiddenPage, callback, forceThis);
        }

        /**
         * Called before a page is shown.
         */
        hookShowingPage(callback: (page: Page) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.showingPage, callback, forceThis);
        }

        /**
         * Called after a page has been shown.
         */
        hookShownPage(callback: (page: Page) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.shownPage, callback, forceThis);
        }

        /**
         * Raises an event.
         */
        private raiseEvent(event: string, page: Page)
        {
            this._Dispatcher.raise(event, [ page ]);
        }

        /**
         * Unhooks a hooked event.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Initialises the page manager.
         * @param {jQuery} element The element to which all pages will be attached. The CSS for this element gets modified.
         */
        initialise(element: JQuery)
        {
            if(this._Element) throw 'The page manager cannot be initialised more than once';
            if(!element) throw 'You must supply an element for the page manager to use';

            this._Element = element;
            this._ElementOriginalCss = this._Element.css([
                'width',
                'height',
                'position'
            ]);
            this._Element.css({
                width:  '100%',
                height: '100%',
                position: 'relative'
            });

            $(window).on('resize.vrspagemanager', $.proxy(this.windowResized, this));
        }

        /**
         * Creates a page from a jQuery element. If this is the first page that's been added then it remains on screen,
         * otherwise it is hidden.
         */
        addPage(page: Page)
        {
            if(!this._Element) throw 'You must initialise the page manager before calling addPage';
            if(!page) throw 'You must supply a page';
            if(this.findPage(page.name)) throw 'A page already exists with the name ' + page.name;

            this._Pages.push(page);

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
                .appendTo(this._Element)
                .append(page.element);

            page.element.css({
                width:  '100%',
                height: '100%'
            });

            if(VRS.refreshManager) {
                VRS.refreshManager.registerOwner(page.container);
                VRS.refreshManager.rebuildRelationships();
            }

            if(this._Pages.length === 1) this.showPage(page);
            else                         this.hidePage(page);
        }

        /**
         * Removes a page from the manager. The page is either returned to its original parent or, if it had no parent,
         * it is just removed from the window.
         */
        removePage(pageOrName: string | Page)
        {
            var index = this.findPageIndex(pageOrName);
            var page = index === -1 ? null : this._Pages[index];
            if(page) {
                if(VRS.refreshManager) {
                    VRS.refreshManager.unregisterOwner(page.container);
                }

                if(page === this._VisiblePage) {
                    if(index + 1 < this._Pages.length) this.show(this._Pages[index + 1]);
                    else if(index > 0)                 this.show(this._Pages[index - 1]);
                    else                               this._VisiblePage = null;
                }
                this._Pages.splice(index, 1);
                page.dispose();

                if(VRS.refreshManager) {
                    VRS.refreshManager.rebuildRelationships();
                }
            }
        }

        /**
         * Hides or shows a page.
         */
        show(pageOrName: string | Page)
        {
            var page = this.findPage(pageOrName);
            if(!page.isVisible) {
                if(this._VisiblePage) this.hidePage(this._VisiblePage);
                this.showPage(page);
            }
        }

        /**
         * Hides a page from view.
         */
        private hidePage(page: Page)
        {
            if(page.isVisible) {
                page.isVisible = false;
                this.raiseEvent(this._Events.hidingPage, page);
                page.container.hide();
                this.raiseEvent(this._Events.hiddenPage, page);
                page.visibleCallback(false);
            }
            if(page === this._VisiblePage) {
                this._VisiblePage = null;
            }
        }

        /**
         * Brings a page into view.
         */
        private showPage(page: Page)
        {
            if(!page.isVisible) {
                page.isVisible = true;
                page.visibleCallback(true);
                this.raiseEvent(this._Events.showingPage, page);
                page.container.show();
                this.raiseEvent(this._Events.shownPage, page);
                page.afterVisibleCallback();

                if(VRS.refreshManager) {
                    VRS.refreshManager.refreshTargets(page.container);
                }
            }
            this._VisiblePage = page;
        }

        /**
         * If passes the name of a page then this returns the registered VRS.Page with that name, otherwise if passed a
         * page then it simply gets returned. Returns null if no page exists for the name passed across.
         */
        private findPage(nameOrPage: string | Page) : Page
        {
            var result: Page = <Page>nameOrPage;

            if(!(result instanceof VRS.Page)) {
                result = null;
                var length = this._Pages.length;
                for(var i = 0;i < length;++i) {
                    var page = this._Pages[i];
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
         */
        private findPageIndex(nameOrPage: string | Page) : number
        {
            var result = -1;

            var length = this._Pages.length;
            for(var i = 0;i < length;++i) {
                var page = this._Pages[i];
                if(nameOrPage === page || nameOrPage === page.name) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Called when the window is resized.
         */
        private windowResized()
        {
            if(this._VisiblePage && VRS.refreshManager) {
                VRS.refreshManager.refreshTargets(this._VisiblePage.container);
            }
        }
    }

    /*
     * Pre-builts
     */
    export var pageManager = new VRS.PageManager();
}
 