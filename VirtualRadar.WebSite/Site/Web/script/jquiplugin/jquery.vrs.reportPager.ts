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
 * @fileoverview A jQueryUI plugin that lets the user switch pages in a report.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportPagerClass = VRS.globalOptions.reportPagerClass || 'reportPager';                                                                                       // The class to use for the report pager widget container.
    VRS.globalOptions.reportPagerSpinnerPageSize = VRS.globalOptions.reportPagerSpinnerPageSize || 5;                                                                               // The number of pages to skip when paging up and down through the page number spinner.
    VRS.globalOptions.reportPagerAllowPageSizeChange = VRS.globalOptions.reportPagerAllowPageSizeChange !== undefined ? VRS.globalOptions.reportPagerAllowPageSizeChange : true;    // True if the user can change the report page size, false if they cannot.
    VRS.globalOptions.reportPagerAllowShowAllRows = VRS.globalOptions.reportPagerAllowShowAllRows !== undefined ? VRS.globalOptions.reportPagerAllowShowAllRows : true;             // True if the user is allowed to show all rows simultaneously, false if they're not.

    /**
     * The options for the ReportPagerPlugin.
     */
    export interface ReportPagerPlugin_Options
    {
        /**
         * The name to save state under.
         */
        name?: string;

        /**
         * The report whose flights we're going to page.
         */
        report: Report;

        /**
         * True if the user is allowed to change the page size, false if they're not.
         */
        allowPageSizeChange?: boolean;

        /**
         * True if the user is allowed to show all rows simultaneously, false if they're not. Ignored if allowPageSizeChange is false.
         */
        allowShowAllRows?: boolean;
    }

    /**
     * The state for the ReportPagerPlugin.
     */
    class ReportPagerPlugin_State
    {
        /**
         * The container for the page selection panel.
         */
        pageSelectionPanelElement: JQuery = null;

        /**
         * The element for the first page button.
         */
        firstPageElement: JQuery = null;

        /**
         * The element for the previous page button.
         */
        prevPageElement: JQuery = null;

        /**
         * The element for the next page button.
         */
        nextPageElement: JQuery = null;

        /**
         * The element for the last page button.
         */
        lastPageElement: JQuery = null;

        /**
         * The element for the page selector control.
         */
        pageSelectorElement: JQuery = null;

        /**
         * The element for the display of the total number of pages in the report.
         */
        countPagesElement: JQuery = null;

        /**
         * The element that the user can click to fetch the selected page.
         */
        fetchPageElement: JQuery = null;

        /**
         * The element that contains all of the page size modification elements.
         */
        pageSizePanelElement: JQuery = null;

        /**
         * The element for the control that the user can enter page sizes into.
         */
        pageSizeSelectElement: JQuery = null;

        /**
         * True if we are not to react to changes to the page size control.
         */
        suppressPageSizeChangedByUs: boolean = false;

        /**
         * The hook result for the report's rows fetched event.
         */
        rowsFetchedHookResult: IEventHandle = null;

        /**
         * The hook result for the report's page size changed event.
         */
        pageSizeChangedHookResult: IEventHandle = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getReportPagerPlugin = function(jQueryElement: JQuery) : ReportPagerPlugin
    {
        return jQueryElement.data('vrsVrsReportPager');
    }
    VRS.jQueryUIHelper.getReportPagerOptions = function(overrides?: ReportPagerPlugin_Options) : ReportPagerPlugin_Options
    {
        return $.extend({
            name:                   'default',                                          // The name to save state under.
            report:                 null,                                               // The report whose flights we're going to page.
            allowPageSizeChange:    VRS.globalOptions.reportPagerAllowPageSizeChange,   // True if the user is allowed to change the page size, false if they're not.
            allowShowAllRows:       VRS.globalOptions.reportPagerAllowShowAllRows,      // True if the user is allowed to show all rows simultaneously, false if they're not. Ignored if allowPageSizeChange is false.
        }, overrides);
    }

    /**
     * A jQuery widget that can display a pager strip for the reports.
     */
    export class ReportPagerPlugin extends JQueryUICustomWidget
    {
        options: ReportPagerPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getReportPagerOptions();
        }

        private _getState() : ReportPagerPlugin_State
        {
            var result = this.element.data('reportPagerState');
            if(result === undefined) {
                result = new ReportPagerPlugin_State();
                this.element.data('reportPagerState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.reportPagerClass);
            var controls = $('<div/>')
                .addClass('controls')
                .appendTo(this.element);
            this._createPageSelectionPanel(state, controls);
            this._createPageSizePanel(state, controls);

            state.rowsFetchedHookResult = options.report.hookRowsFetched(this._rowsFetched, this);
            state.pageSizeChangedHookResult = options.report.hookPageSizeChanged(this._pageSizeChanged, this);
        }

        _destroy()
        {
            var state = this._getState();
            var options = this.options;

            if(state.rowsFetchedHookResult && options.report) {
                options.report.unhook(state.rowsFetchedHookResult);
            }
            state.rowsFetchedHookResult = null;

            if(state.pageSizeChangedHookResult && options.report) {
                options.report.unhook(state.pageSizeChangedHookResult);
            }
            state.pageSizeChangedHookResult = null;

            this._destroyPageSelectionPanel(state);
            this._destroyPageSizePanel(state);
        }

        /**
         * Creates the page selection panel.
         */
        private _createPageSelectionPanel(state: ReportPagerPlugin_State, container: JQuery)
        {
            this._destroyPageSelectionPanel(state);

            state.pageSelectionPanelElement = $('<ul/>')
                .addClass('pageSelection')
                .appendTo(container);

            var self = this;
            var addJumpElement = function(icon: string, clickHandler: () => void, tooltip: string, container?: JQuery) {
                if(!container) {
                    container = $('<li/>').appendTo(state.pageSelectionPanelElement);
                }
                return $('<p/>')
                    .appendTo(container)
                    .text(tooltip)
                    .prepend($('<span/>').addClass('vrsIcon vrsIcon-' + icon))
                    .on('click', $.proxy(clickHandler, self));
            };

            state.firstPageElement = addJumpElement('first', this._firstPageClicked, VRS.$$.PageFirst);
            state.prevPageElement =  addJumpElement('backward', this._prevPageClicked, VRS.$$.PagePrevious);

            var pageSelectorContainer = $('<li/>').appendTo(state.pageSelectionPanelElement);
            state.pageSelectorElement = $('<input/>')
                .attr('type', 'text')
                .appendTo(pageSelectorContainer)
                .spinner({
                    page: VRS.globalOptions.reportPagerSpinnerPageSize
                });
            state.countPagesElement = $('<span/>')
                .addClass('countPages')
                .appendTo(pageSelectorContainer);

            var fetchPageContainer = $('<li/>').appendTo(state.pageSelectionPanelElement);
            state.fetchPageElement = addJumpElement('loop', this._fetchPageClicked, VRS.$$.FetchPage, fetchPageContainer);

            state.nextPageElement = addJumpElement('forward', this._nextPageClicked, VRS.$$.PageNext);
            state.lastPageElement = addJumpElement('last', this._lastPageClicked, VRS.$$.PageLast);

            this._setPageSelectionPanelState(state);
        }

        /**
         * Tears down the page selection panel.
         */
        private _destroyPageSelectionPanel(state: ReportPagerPlugin_State)
        {
            if(state.pageSelectionPanelElement) {
                var destroyJumpElement = function(buttonElement: JQuery) {
                    if(buttonElement) {
                        buttonElement.off();
                    }
                    return null;
                };
                state.firstPageElement = destroyJumpElement(state.firstPageElement);
                state.prevPageElement = destroyJumpElement(state.prevPageElement);
                state.nextPageElement = destroyJumpElement(state.nextPageElement);
                state.lastPageElement = destroyJumpElement(state.lastPageElement);
                state.fetchPageElement = destroyJumpElement(state.fetchPageElement);

                if(state.pageSelectorElement) {
                    state.pageSelectorElement.spinner('destroy');
                    state.pageSelectorElement = null;
                }

                state.pageSelectionPanelElement.remove();
                state.pageSelectionPanelElement = null;
            }
        }

        /**
         * Builds up the UI for the page size panel.
         */
        private _createPageSizePanel(state: ReportPagerPlugin_State, container: JQuery)
        {
            var options = this.options;

            this._destroyPageSizePanel(state);

            if(VRS.globalOptions.reportDefaultStepSize > 0 && options.allowPageSizeChange) {
                state.pageSizePanelElement = $('<ul/>')
                    .addClass('pageSize')
                    .appendTo(container);

                var selectContainer = $('<li/>').appendTo(state.pageSizePanelElement);
                state.pageSizeSelectElement = $('<select/>')
                    .appendTo(selectContainer)
                    .uniqueId();
                for(var i = 1;i <= 10;++i) {
                    var rows = i * VRS.globalOptions.reportDefaultStepSize;
                    $('<option/>')
                        .attr('value', rows)
                        .text(rows.toString())
                        .appendTo(state.pageSizeSelectElement);
                }
                if(options.allowShowAllRows) {
                    $('<option/>')
                        .attr('value', -1)
                        .text(VRS.$$.AllRows)
                        .appendTo(state.pageSizeSelectElement);
                }

                state.pageSizeSelectElement.val(options.report.getPageSize());
                state.pageSizeSelectElement.change($.proxy(this._pageSizeChangedByUs, this));

                $('<label/>')
                    .attr('for', state.pageSizeSelectElement.attr('id'))
                    .insertBefore(state.pageSizeSelectElement)
                    .text(VRS.$$.Rows + ':')
            }
        }

        /**
         * Destroys the UI added for the page size panel.
         */
        private _destroyPageSizePanel(state: ReportPagerPlugin_State)
        {
            if(state.pageSizePanelElement) {
                if(state.pageSizeSelectElement) state.pageSizeSelectElement.off();
                state.pageSelectionPanelElement = null;

                state.pageSizePanelElement.remove();
                state.pageSizePanelElement = null;
            }
        }

        /**
         * Sets the state of the page selection controls.
         */
        private _setPageSelectionPanelState(state: ReportPagerPlugin_State)
        {
            var metrics = this._getPageMetrics();

            var setJumpElementDisabled = function(jumpElement, disabled) {
                if(disabled) jumpElement.addClass('disabled');
                else         jumpElement.removeClass('disabled');
            };
            setJumpElementDisabled(state.firstPageElement, !metrics.isPaged || metrics.onFirstPage);
            setJumpElementDisabled(state.prevPageElement, !metrics.isPaged || metrics.onFirstPage);
            setJumpElementDisabled(state.nextPageElement, !metrics.isPaged || metrics.onLastPage);
            setJumpElementDisabled(state.lastPageElement, !metrics.isPaged || metrics.onLastPage);

            state.pageSelectorElement.val(1);
            state.pageSelectorElement.spinner('option', {
                min: 1,
                max: metrics.totalPages
            });
            state.pageSelectorElement.val(metrics.currentPage + 1);
            state.countPagesElement.text(VRS.stringUtility.format(VRS.$$.OfPages, metrics.totalPages));
        }

        /**
         * Returns an anonymous object that describes the page size
         */
        private _getPageMetrics()
        {
            var report = this.options.report;
            var flights = report.getFlights();

            var rowCount = report.getCountRowsAvailable();
            var pageSize = report.getPageSize();
            var firstRow = flights.length > 0 ? flights[0].row : 0;
            var lastRow = flights.length > 0 ? flights[flights.length - 1].row : 0;
            var isPaged = report.isReportPaged();
            var currentPage = isPaged ? Math.max(0, Math.floor(firstRow / pageSize)) : 0;
            var totalPages = isPaged ? Math.max(1, Math.ceil(rowCount / pageSize)) : 1;

            return {
                isPaged:        isPaged,
                pageSize:       pageSize,
                currentPage:    currentPage,
                totalPages:     totalPages,
                onFirstPage:    !isPaged || currentPage === 0,
                onLastPage:     !isPaged || currentPage + 1 >= totalPages,
                pageFirstRow:   firstRow,
                pageLastRow:    lastRow
            };
        }

        /**
         * Called after the report has fetched a new batch of rows.
         */
        private _rowsFetched()
        {
            this._setPageSelectionPanelState(this._getState());
        }

        /**
         * Called when the user clicks the first page button.
         */
        private _firstPageClicked()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onFirstPage) {
                this.options.report.fetchPage(0);
            }
        }

        /**
         * Called when the user clicks the previous page button.
         */
        private _prevPageClicked()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onFirstPage) {
                this.options.report.fetchPage(metrics.currentPage - 1);
            }
        }

        /**
         * Called when the user clicks the next page button.
         */
        private _nextPageClicked()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onLastPage) {
                this.options.report.fetchPage(metrics.currentPage + 1);
            }
        }

        /**
         * Called when the user clicks the last page button.
         */
        private _lastPageClicked()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onLastPage) {
                this.options.report.fetchPage(metrics.totalPages - 1);
            }
        }

        /**
         * Called when the user clicks the fetch page button.
         */
        private _fetchPageClicked()
        {
            var state = this._getState();
            var metrics = this._getPageMetrics();

            var pageNumber = state.pageSelectorElement ? state.pageSelectorElement.val() : 0;
            if(pageNumber > 0 && pageNumber <= metrics.totalPages) {
                this.options.report.fetchPage(pageNumber - 1);
            }
        }

        /**
         * Called when something (including, but not limited to, this object) changes the page size held by the report.
         */
        private _pageSizeChanged()
        {
            var state = this._getState();

            if(state.pageSizeSelectElement && !state.suppressPageSizeChangedByUs) {
                state.suppressPageSizeChangedByUs = true;
                state.pageSizeSelectElement.val(this.options.report.getPageSize());
                state.suppressPageSizeChangedByUs = false;
            }
        }

        /**
         * Called when the value in the page size control is changed, either by the user or by us.
         */
        private _pageSizeChangedByUs()
        {
            var state = this._getState();
            if(!state.suppressPageSizeChangedByUs) {
                state.suppressPageSizeChangedByUs = true;

                var report = this.options.report;
                var currentPageSize = report.getPageSize();
                var newPageSize = parseInt(state.pageSizeSelectElement.val());

                if(!isNaN(newPageSize) && newPageSize !== currentPageSize) {
                    report.setPageSize(newPageSize);
                    var metrics = this._getPageMetrics();
                    report.fetchPage(metrics.currentPage);
                }

                state.suppressPageSizeChangedByUs = false;
            }
        }
    }

    $.widget('vrs.vrsReportPager', new ReportPagerPlugin());
}

declare interface JQuery
{
    vrsReportPager();
    vrsReportPager(options: VRS.ReportPagerPlugin_Options);
    vrsReportPager(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
