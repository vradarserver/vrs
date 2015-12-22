var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionForm_State = (function () {
        function OptionForm_State() {
            this.optionPageParent = null;
            this.optionPanes = [];
            this.accordionJQ = null;
            this.tabsJQ = null;
        }
        return OptionForm_State;
    })();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFormPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionForm');
    };
    VRS.jQueryUIHelper.getOptionFormOptions = function (overrides) {
        return $.extend({
            pages: [],
            showInAccordion: false
        }, overrides);
    };
    var OptionForm = (function (_super) {
        __extends(OptionForm, _super);
        function OptionForm() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getOptionFormOptions();
        }
        OptionForm.prototype._getState = function () {
            var result = this.element.data('vrsOptionFormState');
            if (result === undefined) {
                result = new OptionForm_State();
                this.element.data('vrsOptionFormState', result);
            }
            return result;
        };
        OptionForm.prototype._create = function () {
            var _this = this;
            if (VRS.timeoutManager) {
                VRS.timeoutManager.resetTimer();
            }
            var state = this._getState();
            var options = this.options;
            state.optionPageParent = new VRS.OptionPageParent();
            var pages = this._buildValidPages();
            var container = $('<div />')
                .addClass('vrsOptionForm')
                .appendTo(this.element);
            var pagesContainer = $('<div />')
                .uniqueId()
                .appendTo(container);
            var selectPageContainer = !options.showInAccordion ? $('<ul />').appendTo(pagesContainer) : null;
            $.each(pages, function (idx, page) {
                _this._addPage(page, pagesContainer, selectPageContainer);
            });
            if (!this.options.showInAccordion) {
                container.addClass('dialog');
                state.tabsJQ = pagesContainer;
                pagesContainer.tabs();
            }
            else {
                container.addClass('accordion');
                state.accordionJQ = pagesContainer;
                pagesContainer.accordion({
                    heightStyle: 'content',
                    collapsible: true,
                    active: pages.length === 1 ? 0 : false
                });
            }
        };
        OptionForm.prototype._destroy = function () {
            var state = this._getState();
            $.each(state.optionPanes, function (idx, panePlugin) {
                panePlugin.destroy();
            });
            state.optionPanes = [];
            if (state.tabsJQ)
                state.tabsJQ.tabs('destroy');
            if (state.accordionJQ)
                state.accordionJQ.accordion('destroy');
            state.tabsJQ = state.accordionJQ = null;
            this.element.empty();
        };
        OptionForm.prototype._buildValidPages = function () {
            var result = [];
            $.each(this.options.pages.slice(), function (idx, page) {
                var hasGoodPanes = false;
                page.foreachPane(function (pane) {
                    if (pane.getFieldCount()) {
                        hasGoodPanes = true;
                    }
                });
                if (hasGoodPanes) {
                    result.push(page);
                }
            });
            result.sort(function (lhs, rhs) {
                return lhs.getDisplayOrder() - rhs.getDisplayOrder();
            });
            return result;
        };
        OptionForm.prototype._addPage = function (page, pagesContainer, selectPageContainer) {
            var state = this._getState();
            var options = this.options;
            var titleText = VRS.globalisation.getText(page.getTitleKey());
            if (options.showInAccordion) {
                $('<h3/>')
                    .text(titleText)
                    .appendTo(pagesContainer);
            }
            var contentContainer = $('<div/>')
                .uniqueId()
                .addClass('vrsOptionPage')
                .appendTo(pagesContainer);
            if (!options.showInAccordion) {
                var id = contentContainer.attr('id');
                var li = $('<li/>')
                    .appendTo(selectPageContainer), href = $('<a/>')
                    .attr('href', '#' + id)
                    .appendTo(li), span = $('<span/>')
                    .text(titleText)
                    .appendTo(href);
            }
            page.foreachPane(function (pane) {
                if (pane.getFieldCount() > 0) {
                    var paneJQ = $('<div/>')
                        .vrsOptionPane(VRS.jQueryUIHelper.getOptionPaneOptions({
                        optionPane: pane,
                        optionPageParent: state.optionPageParent
                    }))
                        .appendTo(contentContainer);
                    state.optionPanes.push(VRS.jQueryUIHelper.getOptionPanePlugin(paneJQ));
                }
            });
        };
        return OptionForm;
    })(JQueryUICustomWidget);
    VRS.OptionForm = OptionForm;
    $.widget('vrs.vrsOptionForm', new OptionForm());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionForm.js.map