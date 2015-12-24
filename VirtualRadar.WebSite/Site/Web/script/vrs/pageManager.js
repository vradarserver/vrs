var VRS;
(function (VRS) {
    var Page = (function () {
        function Page(settings) {
            this.container = null;
            this.isVisible = true;
            this.originalParent = null;
            this.originalCss = null;
            if (!settings)
                throw 'You must supply a settings object';
            if (!settings.name)
                throw 'You must supply a unique name';
            if (!settings.element)
                throw 'You must supply the top-level element';
            this.name = settings.name;
            this.element = settings.element;
            this.visibleCallback = settings.visibleCallback || function () { };
            this.afterVisibleCallback = settings.afterVisibleCallback || function () { };
        }
        Page.prototype.dispose = function () {
            if (this.originalCss)
                this.element.css(this.originalCss);
            if (this.originalParent)
                this.originalParent.append(this.element);
            else
                this.element.remove();
            if (this.container)
                this.container.remove();
            this.container = null;
            this.originalCss = null;
            this.originalParent = null;
        };
        return Page;
    })();
    VRS.Page = Page;
    var PageManager = (function () {
        function PageManager() {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.PageManager'
            });
            this._Events = {
                hidingPage: 'hidingPage',
                hiddenPage: 'hiddenPage',
                showingPage: 'showingPage',
                shownPage: 'shownPage'
            };
            this._Element = null;
            this._ElementOriginalCss = null;
            this._Pages = [];
            this._VisiblePage = null;
        }
        PageManager.prototype.dispose = function () {
            var length = this._Pages.length;
            for (var i = length - 1; i >= 0; --i) {
                var page = this._Pages[i];
                page.dispose();
            }
            this._Pages = [];
            if (this._Element && this._ElementOriginalCss) {
                this._Element.css(this._ElementOriginalCss);
            }
            this._Element = null;
            this._ElementOriginalCss = null;
            $(window).off('resize.vrspagemanager', this.windowResized);
        };
        PageManager.prototype.hookHidingPage = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.hidingPage, callback, forceThis);
        };
        PageManager.prototype.hookHiddenPage = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.hiddenPage, callback, forceThis);
        };
        PageManager.prototype.hookShowingPage = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.showingPage, callback, forceThis);
        };
        PageManager.prototype.hookShownPage = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.shownPage, callback, forceThis);
        };
        PageManager.prototype.raiseEvent = function (event, page) {
            this._Dispatcher.raise(event, [page]);
        };
        PageManager.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        PageManager.prototype.initialise = function (element) {
            if (this._Element)
                throw 'The page manager cannot be initialised more than once';
            if (!element)
                throw 'You must supply an element for the page manager to use';
            this._Element = element;
            this._ElementOriginalCss = this._Element.css([
                'width',
                'height',
                'position'
            ]);
            this._Element.css({
                width: '100%',
                height: '100%',
                position: 'relative'
            });
            $(window).on('resize.vrspagemanager', $.proxy(this.windowResized, this));
        };
        PageManager.prototype.addPage = function (page) {
            if (!this._Element)
                throw 'You must initialise the page manager before calling addPage';
            if (!page)
                throw 'You must supply a page';
            if (this.findPage(page.name))
                throw 'A page already exists with the name ' + page.name;
            this._Pages.push(page);
            page.originalParent = page.element.parent();
            page.originalCss = page.element.css([
                'width',
                'height'
            ]);
            page.container = $('<div/>')
                .css({
                width: '100%',
                height: '100%'
            })
                .appendTo(this._Element)
                .append(page.element);
            page.element.css({
                width: '100%',
                height: '100%'
            });
            if (VRS.refreshManager) {
                VRS.refreshManager.registerOwner(page.container);
                VRS.refreshManager.rebuildRelationships();
            }
            if (this._Pages.length === 1)
                this.showPage(page);
            else
                this.hidePage(page);
        };
        PageManager.prototype.removePage = function (pageOrName) {
            var index = this.findPageIndex(pageOrName);
            var page = index === -1 ? null : this._Pages[index];
            if (page) {
                if (VRS.refreshManager) {
                    VRS.refreshManager.unregisterOwner(page.container);
                }
                if (page === this._VisiblePage) {
                    if (index + 1 < this._Pages.length)
                        this.show(this._Pages[index + 1]);
                    else if (index > 0)
                        this.show(this._Pages[index - 1]);
                    else
                        this._VisiblePage = null;
                }
                this._Pages.splice(index, 1);
                page.dispose();
                if (VRS.refreshManager) {
                    VRS.refreshManager.rebuildRelationships();
                }
            }
        };
        PageManager.prototype.show = function (pageOrName) {
            var page = this.findPage(pageOrName);
            if (!page.isVisible) {
                if (this._VisiblePage)
                    this.hidePage(this._VisiblePage);
                this.showPage(page);
            }
        };
        PageManager.prototype.hidePage = function (page) {
            if (page.isVisible) {
                page.isVisible = false;
                this.raiseEvent(this._Events.hidingPage, page);
                page.container.hide();
                this.raiseEvent(this._Events.hiddenPage, page);
                page.visibleCallback(false);
            }
            if (page === this._VisiblePage) {
                this._VisiblePage = null;
            }
        };
        PageManager.prototype.showPage = function (page) {
            if (!page.isVisible) {
                page.isVisible = true;
                page.visibleCallback(true);
                this.raiseEvent(this._Events.showingPage, page);
                page.container.show();
                this.raiseEvent(this._Events.shownPage, page);
                page.afterVisibleCallback();
                if (VRS.refreshManager) {
                    VRS.refreshManager.refreshTargets(page.container);
                }
            }
            this._VisiblePage = page;
        };
        PageManager.prototype.findPage = function (nameOrPage) {
            var result = nameOrPage;
            if (!(result instanceof VRS.Page)) {
                result = null;
                var length = this._Pages.length;
                for (var i = 0; i < length; ++i) {
                    var page = this._Pages[i];
                    if (page.name === nameOrPage) {
                        result = page;
                        break;
                    }
                }
            }
            return result;
        };
        PageManager.prototype.findPageIndex = function (nameOrPage) {
            var result = -1;
            var length = this._Pages.length;
            for (var i = 0; i < length; ++i) {
                var page = this._Pages[i];
                if (nameOrPage === page || nameOrPage === page.name) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        PageManager.prototype.windowResized = function () {
            if (this._VisiblePage && VRS.refreshManager) {
                VRS.refreshManager.refreshTargets(this._VisiblePage.container);
            }
        };
        return PageManager;
    })();
    VRS.PageManager = PageManager;
    VRS.pageManager = new VRS.PageManager();
})(VRS || (VRS = {}));
//# sourceMappingURL=pageManager.js.map