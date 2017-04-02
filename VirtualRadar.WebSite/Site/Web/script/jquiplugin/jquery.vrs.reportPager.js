var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportPagerClass = VRS.globalOptions.reportPagerClass || 'reportPager';
    VRS.globalOptions.reportPagerSpinnerPageSize = VRS.globalOptions.reportPagerSpinnerPageSize || 5;
    VRS.globalOptions.reportPagerAllowPageSizeChange = VRS.globalOptions.reportPagerAllowPageSizeChange !== undefined ? VRS.globalOptions.reportPagerAllowPageSizeChange : true;
    VRS.globalOptions.reportPagerAllowShowAllRows = VRS.globalOptions.reportPagerAllowShowAllRows !== undefined ? VRS.globalOptions.reportPagerAllowShowAllRows : true;
    var ReportPagerPlugin_State = (function () {
        function ReportPagerPlugin_State() {
            this.pageSelectionPanelElement = null;
            this.firstPageElement = null;
            this.prevPageElement = null;
            this.nextPageElement = null;
            this.lastPageElement = null;
            this.pageSelectorElement = null;
            this.countPagesElement = null;
            this.fetchPageElement = null;
            this.pageSizePanelElement = null;
            this.pageSizeSelectElement = null;
            this.suppressPageSizeChangedByUs = false;
            this.rowsFetchedHookResult = null;
            this.pageSizeChangedHookResult = null;
        }
        return ReportPagerPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getReportPagerPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsReportPager');
    };
    VRS.jQueryUIHelper.getReportPagerOptions = function (overrides) {
        return $.extend({
            name: 'default',
            report: null,
            allowPageSizeChange: VRS.globalOptions.reportPagerAllowPageSizeChange,
            allowShowAllRows: VRS.globalOptions.reportPagerAllowShowAllRows,
        }, overrides);
    };
    var ReportPagerPlugin = (function (_super) {
        __extends(ReportPagerPlugin, _super);
        function ReportPagerPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getReportPagerOptions();
            return _this;
        }
        ReportPagerPlugin.prototype._getState = function () {
            var result = this.element.data('reportPagerState');
            if (result === undefined) {
                result = new ReportPagerPlugin_State();
                this.element.data('reportPagerState', result);
            }
            return result;
        };
        ReportPagerPlugin.prototype._create = function () {
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
        };
        ReportPagerPlugin.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            if (state.rowsFetchedHookResult && options.report) {
                options.report.unhook(state.rowsFetchedHookResult);
            }
            state.rowsFetchedHookResult = null;
            if (state.pageSizeChangedHookResult && options.report) {
                options.report.unhook(state.pageSizeChangedHookResult);
            }
            state.pageSizeChangedHookResult = null;
            this._destroyPageSelectionPanel(state);
            this._destroyPageSizePanel(state);
        };
        ReportPagerPlugin.prototype._createPageSelectionPanel = function (state, container) {
            this._destroyPageSelectionPanel(state);
            state.pageSelectionPanelElement = $('<ul/>')
                .addClass('pageSelection')
                .appendTo(container);
            var self = this;
            var addJumpElement = function (icon, clickHandler, tooltip, container) {
                if (!container) {
                    container = $('<li/>').appendTo(state.pageSelectionPanelElement);
                }
                return $('<p/>')
                    .appendTo(container)
                    .text(tooltip)
                    .prepend($('<span/>').addClass('vrsIcon vrsIcon-' + icon))
                    .on('click', $.proxy(clickHandler, self));
            };
            state.firstPageElement = addJumpElement('first', this._firstPageClicked, VRS.$$.PageFirst);
            state.prevPageElement = addJumpElement('backward', this._prevPageClicked, VRS.$$.PagePrevious);
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
        };
        ReportPagerPlugin.prototype._destroyPageSelectionPanel = function (state) {
            if (state.pageSelectionPanelElement) {
                var destroyJumpElement = function (buttonElement) {
                    if (buttonElement) {
                        buttonElement.off();
                    }
                    return null;
                };
                state.firstPageElement = destroyJumpElement(state.firstPageElement);
                state.prevPageElement = destroyJumpElement(state.prevPageElement);
                state.nextPageElement = destroyJumpElement(state.nextPageElement);
                state.lastPageElement = destroyJumpElement(state.lastPageElement);
                state.fetchPageElement = destroyJumpElement(state.fetchPageElement);
                if (state.pageSelectorElement) {
                    state.pageSelectorElement.spinner('destroy');
                    state.pageSelectorElement = null;
                }
                state.pageSelectionPanelElement.remove();
                state.pageSelectionPanelElement = null;
            }
        };
        ReportPagerPlugin.prototype._createPageSizePanel = function (state, container) {
            var options = this.options;
            this._destroyPageSizePanel(state);
            if (VRS.globalOptions.reportDefaultStepSize > 0 && options.allowPageSizeChange) {
                state.pageSizePanelElement = $('<ul/>')
                    .addClass('pageSize')
                    .appendTo(container);
                var selectContainer = $('<li/>').appendTo(state.pageSizePanelElement);
                state.pageSizeSelectElement = $('<select/>')
                    .appendTo(selectContainer)
                    .uniqueId();
                for (var i = 1; i <= 10; ++i) {
                    var rows = i * VRS.globalOptions.reportDefaultStepSize;
                    $('<option/>')
                        .attr('value', rows)
                        .text(rows.toString())
                        .appendTo(state.pageSizeSelectElement);
                }
                if (options.allowShowAllRows) {
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
                    .text(VRS.$$.Rows + ':');
            }
        };
        ReportPagerPlugin.prototype._destroyPageSizePanel = function (state) {
            if (state.pageSizePanelElement) {
                if (state.pageSizeSelectElement)
                    state.pageSizeSelectElement.off();
                state.pageSelectionPanelElement = null;
                state.pageSizePanelElement.remove();
                state.pageSizePanelElement = null;
            }
        };
        ReportPagerPlugin.prototype._setPageSelectionPanelState = function (state) {
            var metrics = this._getPageMetrics();
            var setJumpElementDisabled = function (jumpElement, disabled) {
                if (disabled)
                    jumpElement.addClass('disabled');
                else
                    jumpElement.removeClass('disabled');
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
        };
        ReportPagerPlugin.prototype._getPageMetrics = function () {
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
                isPaged: isPaged,
                pageSize: pageSize,
                currentPage: currentPage,
                totalPages: totalPages,
                onFirstPage: !isPaged || currentPage === 0,
                onLastPage: !isPaged || currentPage + 1 >= totalPages,
                pageFirstRow: firstRow,
                pageLastRow: lastRow
            };
        };
        ReportPagerPlugin.prototype._rowsFetched = function () {
            this._setPageSelectionPanelState(this._getState());
        };
        ReportPagerPlugin.prototype._firstPageClicked = function () {
            var metrics = this._getPageMetrics();
            if (!metrics.onFirstPage) {
                this.options.report.fetchPage(0);
            }
        };
        ReportPagerPlugin.prototype._prevPageClicked = function () {
            var metrics = this._getPageMetrics();
            if (!metrics.onFirstPage) {
                this.options.report.fetchPage(metrics.currentPage - 1);
            }
        };
        ReportPagerPlugin.prototype._nextPageClicked = function () {
            var metrics = this._getPageMetrics();
            if (!metrics.onLastPage) {
                this.options.report.fetchPage(metrics.currentPage + 1);
            }
        };
        ReportPagerPlugin.prototype._lastPageClicked = function () {
            var metrics = this._getPageMetrics();
            if (!metrics.onLastPage) {
                this.options.report.fetchPage(metrics.totalPages - 1);
            }
        };
        ReportPagerPlugin.prototype._fetchPageClicked = function () {
            var state = this._getState();
            var metrics = this._getPageMetrics();
            var pageNumber = state.pageSelectorElement ? state.pageSelectorElement.val() : 0;
            if (pageNumber > 0 && pageNumber <= metrics.totalPages) {
                this.options.report.fetchPage(pageNumber - 1);
            }
        };
        ReportPagerPlugin.prototype._pageSizeChanged = function () {
            var state = this._getState();
            if (state.pageSizeSelectElement && !state.suppressPageSizeChangedByUs) {
                state.suppressPageSizeChangedByUs = true;
                state.pageSizeSelectElement.val(this.options.report.getPageSize());
                state.suppressPageSizeChangedByUs = false;
            }
        };
        ReportPagerPlugin.prototype._pageSizeChangedByUs = function () {
            var state = this._getState();
            if (!state.suppressPageSizeChangedByUs) {
                state.suppressPageSizeChangedByUs = true;
                var report = this.options.report;
                var currentPageSize = report.getPageSize();
                var newPageSize = parseInt(state.pageSizeSelectElement.val());
                if (!isNaN(newPageSize) && newPageSize !== currentPageSize) {
                    report.setPageSize(newPageSize);
                    var metrics = this._getPageMetrics();
                    report.fetchPage(metrics.currentPage);
                }
                state.suppressPageSizeChangedByUs = false;
            }
        };
        return ReportPagerPlugin;
    }(JQueryUICustomWidget));
    VRS.ReportPagerPlugin = ReportPagerPlugin;
    $.widget('vrs.vrsReportPager', new ReportPagerPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.reportPager.js.map