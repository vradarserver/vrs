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
 *
 * The CSS for the sticky header and expandable content came from here:
 * http://stackoverflow.com/questions/12605816/sticky-flexible-footers-and-headers-css-working-fine-in-webkit-but-not-in-gecko
 */
/**
 * @fileoverview A jQuery UI widget that wraps an element and displays it with a header panel. Intended for use on the
 * mobile site.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.pagePanelClass = VRS.globalOptions.pagePanelClass || 'pagePanel';     // The class to set on page headers.

    /**
     * The state held by a PagePanel widget.
     */
    class PagePanel_State
    {
        /**
         * The element for the previous page button.
         */
        previousPageElement: JQuery = null;

        /**
         * The element for the title label.
         */
        titleElement: JQuery = null;

        /**
         * The element for the text in the title.
         */
        titleTextElement: JQuery = null;

        /**
         * The element for the next page button
         */
        nextPageElement: JQuery = null;

        /**
         * The original parent of the content that's been moved into the page.
         */
        originalContentParent: JQuery = null;

        /**
         * The VRS.vrsMenu menu shown in the header panel, if any.
         */
        headerMenu: MenuPlugin = null;

        /**
         * The jQuery element holding the header menu.
         */
        headerMenuElement: JQuery = null;

        /**
         * The hook result for the locale changed event.
         */
        localeChangedHookResult: IEventHandle = null;
    }

    /**
     * The options for the PagePanel widget.
     */
    export interface PagePanel_Options
    {
        /**
         * The element that will be moved into the page's container.
         */
        element: JQuery;

        /**
         * The name of the previous page to jump to.
         */
        previousPageName?: string;

        /**
         * The key in VRS.$$ of the label for the previous page.
         */
        previousPageLabelKey?: string;

        /**
         * The name of the next page to jump to.
         */
        nextPageName?: string;

        /**
         * The key in VRS.$$ of the label for the next page.
         */
        nextPageLabelKey?: string;

        /**
         * The key in VRS.$$ of the title for the page.
         */
        titleLabelKey?: string;

        /**
         * The menu to display when the header's menu button is clicked. If omitted then no menu button is shown.
         */
        headerMenu?: Menu;

        /**
         * True if a gap should be shown in the page footer.
         */
        showFooterGap?: boolean;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getPagePanelPlugin = function(jQueryElement: JQuery) : PagePanel
    {
        return jQueryElement.data('vrsVrsPagePanel');
    }
    VRS.jQueryUIHelper.getPagePanelOptions = function(overrides?: PagePanel_Options) : PagePanel_Options
    {
        return $.extend({
            element:                null,
            previousPageName:       null,
            previousPageLabelKey:   null,
            nextPageName:           null,
            nextPageLabelKey:       null,
            titleLabelKey:          null,
            headerMenu:             null,
            showFooterGap:          false
        }, overrides);
    }

    /**
     * A jQuery UI widget that wraps an element and displays it with a header panel. Intended for use on the mobile site.
     */
    export class PagePanel extends JQueryUICustomWidget
    {
        options: PagePanel_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getPagePanelOptions();
        }

        private _getState() : PagePanel_State
        {
            var result = this.element.data('vrsPageHeaderPanelState');
            if(result === undefined) {
                result = new PagePanel_State()
                this.element.data('vrsPageHeaderPanelState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.pagePanelClass);
            state.originalContentParent = options.element.parent();

            // The extra div wrappers are required by the CSS, don't zap them :)
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
            if(options.headerMenu) {
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
                    .append(elementParent)
                );
            elementParent.append(options.element);
            if(options.showFooterGap) $('<div/>').addClass('pageFooterGap').appendTo(elementParent);

            state.localeChangedHookResult = VRS.globalisation.hookLocaleChanged(this._localeChanged, this);

            this._updateHeaderText();
        }

        private _destroy()
        {
            var state = this._getState();
            var options = this.options;

            if(state.localeChangedHookResult) {
                VRS.globalisation.unhook(state.localeChangedHookResult);
                state.localeChangedHookResult = null;
            }

            if(state.headerMenuElement) {
                state.headerMenu.destroy();
                state.headerMenuElement.remove();
                state.headerMenu = null;
                state.headerMenuElement = null;
            }

            if(state.previousPageElement) state.previousPageElement.off();
            if(state.nextPageElement)     state.nextPageElement.off();

            if(state.originalContentParent) {
                options.element.appendTo(state.originalContentParent);
                state.originalContentParent = null;
            }
        }

        /**
         * Redraws the text in the header strip.
         */
        private _updateHeaderText()
        {
            var state = this._getState();
            var options = this.options;

            var updateText = function(element, labelKey) {
                if(element) {
                    var text = labelKey ? VRS.globalisation.getText(labelKey) : '';
                    element.text(text);
                }
            };

            updateText(state.previousPageElement,   options.previousPageLabelKey);
            updateText(state.titleTextElement,      options.titleLabelKey);
            updateText(state.nextPageElement,       options.nextPageLabelKey);
        }

        /**
         * Does the work for the previous/next page clicked handlers.
         */
        private _doPageClicked(event: Event, pageName: string)
        {
            var result = !(!!pageName);
            if(!result) {
                event.stopPropagation();
                event.preventDefault();
                setTimeout(function() {
                    VRS.pageManager.show(pageName);
                }, 100);
            }

            return result;
        }

        /**
         * Called by jQuery when an option is changed.
         */
        _setOption(key: string, value: any)
        {
            this._super(key, value);

            switch(key) {
                case 'previousPageLabelKey':
                case 'nextPageLabelKey':
                case 'titleLabelKey':
                    this._updateHeaderText();
                    break;
            }
        }

        /**
         * Called when the previous page button is clicked.
         */
        private _previousPageClicked(event: Event)
        {
            return this._doPageClicked(event, this.options.previousPageName);
        }

        /**
         * Called when the next page button is clicked.
         */
        private _nextPageClicked(event: Event)
        {
            return this._doPageClicked(event, this.options.nextPageName);
        }

        /**
         * Called when the user changes the locale.
         */
        private _localeChanged()
        {
            this._updateHeaderText();
        }
    }

    $.widget('vrs.vrsPagePanel', new PagePanel());
}

declare interface JQuery
{
    vrsPagePanel();
    vrsPagePanel(options: VRS.PagePanel_Options);
    vrsPagePanel(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
