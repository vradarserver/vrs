var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.pagePanelClass = VRS.globalOptions.pagePanelClass || 'pagePanel';
    var PagePanel_State = (function () {
        function PagePanel_State() {
            this.previousPageElement = null;
            this.titleElement = null;
            this.titleTextElement = null;
            this.nextPageElement = null;
            this.originalContentParent = null;
            this.headerMenu = null;
            this.headerMenuElement = null;
            this.localeChangedHookResult = null;
        }
        return PagePanel_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getPagePanelPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsPagePanel');
    };
    VRS.jQueryUIHelper.getPagePanelOptions = function (overrides) {
        return $.extend({
            element: null,
            previousPageName: null,
            previousPageLabelKey: null,
            nextPageName: null,
            nextPageLabelKey: null,
            titleLabelKey: null,
            headerMenu: null,
            showFooterGap: false
        }, overrides);
    };
    var PagePanel = (function (_super) {
        __extends(PagePanel, _super);
        function PagePanel() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getPagePanelOptions();
            return _this;
        }
        PagePanel.prototype._getState = function () {
            var result = this.element.data('vrsPageHeaderPanelState');
            if (result === undefined) {
                result = new PagePanel_State();
                this.element.data('vrsPageHeaderPanelState', result);
            }
            return result;
        };
        PagePanel.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            this.element.addClass(VRS.globalOptions.pagePanelClass);
            state.originalContentParent = options.element.parent();
            var headerPanel = $('<header/>')
                .addClass('headerPanel')
                .appendTo(this.element);
            var headerInner = $('<div/>')
                .appendTo(headerPanel);
            state.previousPageElement = $('<p/>')
                .addClass('previous vrsNoHighlight')
                .on('click', $.proxy(this._previousPageClicked, this))
                .appendTo(headerInner);
            state.titleElement = $('<p/>')
                .addClass('title')
                .appendTo(headerInner);
            state.titleTextElement = $('<span/>')
                .appendTo(state.titleElement);
            if (options.headerMenu) {
                state.headerMenuElement = $('<span/>')
                    .prependTo(state.titleElement)
                    .vrsMenu(VRS.jQueryUIHelper.getMenuOptions({
                    menu: options.headerMenu
                }));
                state.headerMenu = VRS.jQueryUIHelper.getMenuPlugin(state.headerMenuElement);
            }
            state.nextPageElement = $('<p/>')
                .addClass('next vrsNoHighlight')
                .on('click', $.proxy(this._nextPageClicked, this))
                .appendTo(headerInner);
            var elementParent = $('<div/>');
            $('<section/>')
                .addClass('pageContent')
                .appendTo(this.element)
                .append($('<div/>')
                .append(elementParent));
            elementParent.append(options.element);
            if (options.showFooterGap)
                $('<div/>').addClass('pageFooterGap').appendTo(elementParent);
            state.localeChangedHookResult = VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            this._updateHeaderText();
        };
        PagePanel.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            if (state.localeChangedHookResult) {
                VRS.globalisation.unhook(state.localeChangedHookResult);
                state.localeChangedHookResult = null;
            }
            if (state.headerMenuElement) {
                state.headerMenu.destroy();
                state.headerMenuElement.remove();
                state.headerMenu = null;
                state.headerMenuElement = null;
            }
            if (state.previousPageElement)
                state.previousPageElement.off();
            if (state.nextPageElement)
                state.nextPageElement.off();
            if (state.originalContentParent) {
                options.element.appendTo(state.originalContentParent);
                state.originalContentParent = null;
            }
        };
        PagePanel.prototype._updateHeaderText = function () {
            var state = this._getState();
            var options = this.options;
            var updateText = function (element, labelKey) {
                if (element) {
                    var text = labelKey ? VRS.globalisation.getText(labelKey) : '';
                    element.text(text);
                }
            };
            updateText(state.previousPageElement, options.previousPageLabelKey);
            updateText(state.titleTextElement, options.titleLabelKey);
            updateText(state.nextPageElement, options.nextPageLabelKey);
        };
        PagePanel.prototype._doPageClicked = function (event, pageName) {
            var result = !(!!pageName);
            if (!result) {
                event.stopPropagation();
                event.preventDefault();
                setTimeout(function () {
                    VRS.pageManager.show(pageName);
                }, 100);
            }
            return result;
        };
        PagePanel.prototype._setOption = function (key, value) {
            this._super(key, value);
            switch (key) {
                case 'previousPageLabelKey':
                case 'nextPageLabelKey':
                case 'titleLabelKey':
                    this._updateHeaderText();
                    break;
            }
        };
        PagePanel.prototype._previousPageClicked = function (event) {
            return this._doPageClicked(event, this.options.previousPageName);
        };
        PagePanel.prototype._nextPageClicked = function (event) {
            return this._doPageClicked(event, this.options.nextPageName);
        };
        PagePanel.prototype._localeChanged = function () {
            this._updateHeaderText();
        };
        return PagePanel;
    }(JQueryUICustomWidget));
    VRS.PagePanel = PagePanel;
    $.widget('vrs.vrsPagePanel', new PagePanel());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.pagePanel.js.map