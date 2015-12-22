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
 * @fileoverview A jQuery UI widget that displays a tabbed form with an option page in each tab, each page holding a list of panes and each pane holding a list of fields attached to configurable objects.
 */

namespace VRS
{
    /**
     * The state object attached to the option form.
     */
    class OptionForm_State
    {
        /**
         * The instance of a VRS.OptionPageParent that the panes and/or fields can hook and call to raise events across
         * the entire form.
         */
        optionPageParent: OptionPageParent = null;

        /**
         * An array of option panes created for the form.
         */
        optionPanes: OptionPanePlugin[] = [];

        /**
         * The container for the accordion, if in use.
         */
        accordionJQ: JQuery = null;

        /**
         * The container for the tabs, if in use.
         */
        tabsJQ: JQuery = null;
    }

    /**
     * The options that OptionForm supports.
     */
    export interface OptionForm_Options
    {
        /**
         * An array of VRS.OptionPage objects that describe the option pages to display.
         */
        pages: OptionPage[];

        /**
         * If false (default) then pages are shown in tabs, otherwise they're shown in an accordion.
         */
        showInAccordion?: boolean;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFormPlugin = function(jQueryElement: JQuery) : OptionForm
    {
        return jQueryElement.data('vrsVrsOptionForm');
    }
    VRS.jQueryUIHelper.getOptionFormOptions = function(overrides?: OptionForm_Options) : OptionForm_Options
    {
        return $.extend({
            pages:  [],
            showInAccordion: false
        }, overrides);
    }

    /**
     * A widget that can show a collection of OptionPages in tabs or an accordion.
     */
    export class OptionForm extends JQueryUICustomWidget
    {
        options: OptionForm_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFormOptions();
        }

        private _getState() : OptionForm_State
        {
            var result = this.element.data('vrsOptionFormState');
            if(result === undefined) {
                result = new OptionForm_State();
                this.element.data('vrsOptionFormState', result);
            }

            return result;
        }

        _create()
        {
            if(VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            var options = this.options;

            state.optionPageParent = new VRS.OptionPageParent();
            var pages = this._buildValidPages();

            var container =
                $('<div />')
                    .addClass('vrsOptionForm')
                    .appendTo(this.element);
            var pagesContainer =
                $('<div />')
                    .uniqueId()
                    .appendTo(container);
            var selectPageContainer = !options.showInAccordion ? $('<ul />').appendTo(pagesContainer) : null;

            $.each(pages, (idx, page) => {
                this._addPage(page, pagesContainer, selectPageContainer);
            });

            if(!this.options.showInAccordion) {
                container.addClass('dialog');
                state.tabsJQ = pagesContainer;
                pagesContainer.tabs();
            } else {
                container.addClass('accordion');
                state.accordionJQ = pagesContainer;
                pagesContainer.accordion({
                    heightStyle: 'content',
                    collapsible: true,
                    active: pages.length === 1 ? 0 : false
                });
            }
        }

        _destroy()
        {
            var state = this._getState();

            $.each(state.optionPanes, (idx, panePlugin) => {
                panePlugin.destroy();
            });
            state.optionPanes = [];

            if(state.tabsJQ) state.tabsJQ.tabs('destroy');
            if(state.accordionJQ) state.accordionJQ.accordion('destroy');
            state.tabsJQ = state.accordionJQ = null;

            this.element.empty();
        }

        /**
         * Returns a list of valid pages in the correct display order. Pages that are not to be shown to the user are
         * not included.
         */
        private _buildValidPages() : OptionPage[]
        {
            var result: OptionPage[] = [];

            $.each(this.options.pages.slice(), (idx, page) => {
                var hasGoodPanes = false;
                page.foreachPane(function(pane) {
                    if(pane.getFieldCount()) {
                        hasGoodPanes = true;
                    }
                });
                if(hasGoodPanes) {
                    result.push(page);
                }
            });

            result.sort(function(lhs, rhs) {
                return lhs.getDisplayOrder() - rhs.getDisplayOrder()
            });

            return result;
        }

        /**
         * Adds a tab page to the UI.
         */
        private _addPage(page: OptionPage, pagesContainer: JQuery, selectPageContainer: JQuery)
        {
            var state = this._getState();
            var options = this.options;

            var titleText = VRS.globalisation.getText(page.getTitleKey());

            if(options.showInAccordion) {
                $('<h3/>')
                    .text(titleText)
                    .appendTo(pagesContainer);
            }

            var contentContainer = $('<div/>')
                .uniqueId()
                .addClass('vrsOptionPage')
                .appendTo(pagesContainer);

            if(!options.showInAccordion) {
                var id = contentContainer.attr('id');
                var li = $('<li/>')
                    .appendTo(selectPageContainer),
                href = $('<a/>')
                    .attr('href', '#' + id)
                    .appendTo(li),
                span = $('<span/>')
                    .text(titleText)
                    .appendTo(href);
            }

            page.foreachPane(function(pane) {
                if(pane.getFieldCount() > 0) {
                    var paneJQ: JQuery = $('<div/>')
                        .vrsOptionPane(VRS.jQueryUIHelper.getOptionPaneOptions({
                            optionPane: pane,
                            optionPageParent: state.optionPageParent
                        }))
                        .appendTo(contentContainer);
                    state.optionPanes.push(VRS.jQueryUIHelper.getOptionPanePlugin(paneJQ));
                }
            })
        }
    }

    $.widget('vrs.vrsOptionForm', new OptionForm())
}

declare interface JQuery
{
    vrsOptionForm();
    vrsOptionForm(options: VRS.OptionForm_Options);
    vrsOptionForm(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
