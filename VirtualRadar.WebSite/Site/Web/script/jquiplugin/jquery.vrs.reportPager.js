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

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportPagerClass = VRS.globalOptions.reportPagerClass || 'reportPager';                                                                                       // The class to use for the report pager widget container.
    VRS.globalOptions.reportPagerSpinnerPageSize = VRS.globalOptions.reportPagerSpinnerPageSize || 5;                                                                               // The number of pages to skip when paging up and down through the page number spinner.
    VRS.globalOptions.reportPagerAllowPageSizeChange = VRS.globalOptions.reportPagerAllowPageSizeChange !== undefined ? VRS.globalOptions.reportPagerAllowPageSizeChange : true;    // True if the user can change the report page size, false if they cannot.
    VRS.globalOptions.reportPagerAllowShowAllRows = VRS.globalOptions.reportPagerAllowShowAllRows !== undefined ? VRS.globalOptions.reportPagerAllowShowAllRows : true;             // True if the user is allowed to show all rows simultaneously, false if they're not.
    //endregion

    //region ReportPagerState
    VRS.ReportPagerState = function()
    {
        /**
         * The container for the page selection panel.
         * @type {jQuery}
         */
        this.pageSelectionPanelElement = null;

        /**
         * The element for the first page button.
         * @type {jQuery}
         */
        this.firstPageElement = null;

        /**
         * The element for the previous page button.
         * @type {jQuery}
         */
        this.prevPageElement = null;

        /**
         * The element for the next page button.
         * @type {jQuery}
         */
        this.nextPageElement = null;

        /**
         * The element for the last page button.
         * @type {jQuery}
         */
        this.lastPageElement = null;

        /**
         * The element for the page selector control.
         * @type {jQuery}
         */
        this.pageSelectorElement = null;

        /**
         * The element for the display of the total number of pages in the report.
         * @type {jQuery}
         */
        this.countPagesElement = null;

        /**
         * The element that the user can click to fetch the selected page.
         * @type {jQuery}
         */
        this.fetchPageElement = null;

        /**
         * The element that contains all of the page size modification elements.
         * @type {jQuery}
         */
        this.pageSizePanelElement = null;

        /**
         * The element for the control that the user can enter page sizes into.
         * @type {jQuery}
         */
        this.pageSizeSelectElement = null;

        /**
         * True if we are not to react to changes to the page size control.
         * @type {boolean}
         */
        this.suppressPageSizeChangedByUs = false;

        /**
         * The hook result for the report's rows fetched event.
         * @type {Object}
         */
        this.rowsFetchedHookResult = null;

        /**
         * The hook result for the report's page size changed event.
         * @type {Object}
         */
        this.pageSizeChangedHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    /**
     * Returns the report pager widget attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsReportPager}
     */
    VRS.jQueryUIHelper.getReportPagerPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsReportPager'); };

    /**
     * Returns the default options for a report pager widget, with optional overrides.
     * @param {VRS_OPTIONS_REPORTPAGER=} overrides
     * @returns {VRS_OPTIONS_REPORTPAGER}
     */
    VRS.jQueryUIHelper.getReportPagerOptions = function(overrides)
    {
        return $.extend({
            name:                   'default',                                          // The name to save state under.
            report:                 null,                                               // The report whose flights we're going to page.
            allowPageSizeChange:    VRS.globalOptions.reportPagerAllowPageSizeChange,   // True if the user is allowed to change the page size, false if they're not.
            allowShowAllRows:       VRS.globalOptions.reportPagerAllowShowAllRows,      // True if the user is allowed to show all rows simultaneously, false if they're not. Ignored if allowPageSizeChange is false.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsReportPager
    /**
     * A jQuery widget that can display a single aircraft's location on a map for a report.
     * @namespace VRS.vrsReportPager
     */
    $.widget('vrs.vrsReportPager', {
        //region -- options
        /** @type {VRS_OPTIONS_REPORTPAGER} */
        options: VRS.jQueryUIHelper.getReportPagerOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object for the plugin, creating it if it's not already there.
         * @returns {VRS.ReportPagerState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('reportPagerState');
            if(result === undefined) {
                result = new VRS.ReportPagerState();
                this.element.data('reportPagerState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
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
        },

        /**
         * Reverses the UI changes and releases all resources allocated to the widget.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            var options = this.options;

            if(state.rowsFetchedHookResult && options.report) options.report.unhook(state.rowsFetchedHookResult);
            state.rowsFetchedHookResult = null;

            if(state.pageSizeChangedHookResult && options.report) options.report.unhook(state.pageSizeChangedHookResult);
            state.pageSizeChangedHookResult = null;

            this._destroyPageSelectionPanel(state);
            this._destroyPageSizePanel(state);
        },
        //endregion

        //region -- _createPageSelectionPanel, _destroyPageSelectionPanel
        /**
         * Creates the page selection panel.
         * @param {VRS.ReportPagerState}    state
         * @param {jQuery}                  container
         * @private
         */
        _createPageSelectionPanel: function(state, container)
        {
            this._destroyPageSelectionPanel(state);

            state.pageSelectionPanelElement = $('<ul/>')
                .addClass('pageSelection')
                .appendTo(container);

            var self = this;
            var addJumpElement = function(icon, clickHandler, tooltip, container) {
                if(!container) container = $('<li/>').appendTo(state.pageSelectionPanelElement);
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
        },

        /**
         * Tears down the page selection panel.
         * @private
         */
        _destroyPageSelectionPanel: function(state)
        {
            if(state.pageSelectionPanelElement) {
                var destroyJumpElement = function(buttonElement) {
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
        },
        //endregion

        //region -- _createPageSizePanel, _destroyPageSizePanel
        /**
         * Builds up the UI for the page size panel.
         * @param {VRS.ReportPagerState}    state
         * @param {jQuery}                  container
         * @private
         */
        _createPageSizePanel: function(state, container)
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
        },

        /**
         * Destroys the UI added for the page size panel.
         * @param {VRS.ReportPagerState} state
         * @private
         */
        _destroyPageSizePanel: function(state)
        {
            if(state.pageSizePanelElement) {
                if(state.pageSizeSelectElement) state.pageSizeSelectElement.off();
                state.pageSelectionPanelElement = null;

                state.pageSizePanelElement.remove();
                state.pageSizePanelElement = null;
            }
        },
        //endregion

        //region -- _setPageSelectionPanelState, _getPageMetrics
        /**
         * Sets the state of the page selection controls.
         * @param {VRS.ReportPagerState} state
         * @private
         */
        _setPageSelectionPanelState: function(state)
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
        },

        /**
         * Returns an anonymous object that describes the page size
         * @returns {VRS_REPORT_PAGE_METRICS}
         * @private
         */
        _getPageMetrics: function()
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
        },
        //endregion

        //region -- Events subscribed
        /**
         * Called after the report has fetched a new batch of rows.
         * @private
         */
        _rowsFetched: function()
        {
            this._setPageSelectionPanelState(this._getState());
        },

        /**
         * Called when the user clicks the first page button.
         * @private
         */
        _firstPageClicked: function()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onFirstPage) this.options.report.fetchPage(0);
        },

        /**
         * Called when the user clicks the previous page button.
         * @private
         */
        _prevPageClicked: function()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onFirstPage) this.options.report.fetchPage(metrics.currentPage - 1);
        },

        /**
         * Called when the user clicks the next page button.
         * @private
         */
        _nextPageClicked: function()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onLastPage) this.options.report.fetchPage(metrics.currentPage + 1);
        },

        /**
         * Called when the user clicks the last page button.
         * @private
         */
        _lastPageClicked: function()
        {
            var metrics = this._getPageMetrics();
            if(!metrics.onLastPage) this.options.report.fetchPage(metrics.totalPages - 1);
        },

        /**
         * Called when the user clicks the fetch page button.
         * @private
         */
        _fetchPageClicked: function()
        {
            var state = this._getState();
            var metrics = this._getPageMetrics();

            var pageNumber = state.pageSelectorElement ? state.pageSelectorElement.val() : 0;
            if(pageNumber > 0 && pageNumber <= metrics.totalPages) this.options.report.fetchPage(pageNumber - 1);
        },

        /**
         * Called when something (including, but not limited to, this object) changes the page size held by the report.
         * @private
         */
        _pageSizeChanged: function()
        {
            var state = this._getState();

            if(state.pageSizeSelectElement && !state.suppressPageSizeChangedByUs) {
                state.suppressPageSizeChangedByUs = true;
                state.pageSizeSelectElement.val(this.options.report.getPageSize());
                state.suppressPageSizeChangedByUs = false;
            }
        },

        /**
         * Called when the value in the page size control is changed, either by the user or by us.
         * @private
         */
        _pageSizeChangedByUs: function()
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
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));