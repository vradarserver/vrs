var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var TimeoutMessageBoxPlugin_State = (function () {
        function TimeoutMessageBoxPlugin_State() {
            this.SiteTimedOutHookResult = null;
        }
        return TimeoutMessageBoxPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getTimeoutMessageBox = function (jQueryElement) {
        return jQueryElement.data('vrsVrsTimeoutMessageBox');
    };
    var TimeoutMessageBoxPlugin = (function (_super) {
        __extends(TimeoutMessageBoxPlugin, _super);
        function TimeoutMessageBoxPlugin() {
            _super.apply(this, arguments);
            this.options = {
                aircraftListFetcher: null
            };
        }
        TimeoutMessageBoxPlugin.prototype._getState = function () {
            var result = this.element.data('vrsTimeoutMessageBoxState');
            if (result === undefined) {
                result = new TimeoutMessageBoxPlugin_State();
                this.element.data('vrsTimeoutMessageBoxState', result);
            }
            return result;
        };
        TimeoutMessageBoxPlugin.prototype._create = function () {
            if (!this.options.aircraftListFetcher)
                throw 'An aircraft list must be supplied';
            var state = this._getState();
            state.SiteTimedOutHookResult = VRS.timeoutManager.hookSiteTimedOut(this._siteTimedOut, this);
        };
        TimeoutMessageBoxPlugin.prototype._destroy = function () {
            var state = this._getState();
            if (state.SiteTimedOutHookResult) {
                VRS.timeoutManager.unhook(state.SiteTimedOutHookResult);
                state.SiteTimedOutHookResult = null;
            }
        };
        TimeoutMessageBoxPlugin.prototype._siteTimedOut = function () {
            var options = this.options;
            var dialog = $('<div/>')
                .appendTo('body');
            $('<p/>')
                .text(VRS.$$.SiteTimedOut)
                .appendTo(dialog);
            dialog.dialog({
                modal: true,
                title: VRS.$$.TitleSiteTimedOut,
                close: function () {
                    options.aircraftListFetcher.setPaused(false);
                    dialog.dialog('destroy');
                    dialog.remove();
                }
            });
        };
        return TimeoutMessageBoxPlugin;
    }(JQueryUICustomWidget));
    VRS.TimeoutMessageBoxPlugin = TimeoutMessageBoxPlugin;
    $.widget('vrs.vrsTimeoutMessageBox', new TimeoutMessageBoxPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.timeoutMessageBox.js.map