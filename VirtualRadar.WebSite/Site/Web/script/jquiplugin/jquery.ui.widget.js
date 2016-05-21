var JQueryUICustomWidget = (function () {
    function JQueryUICustomWidget() {
        var myPrototype = JQueryUICustomWidget.prototype;
        $.each(myPrototype, function (propertyName, value) {
            delete myPrototype[propertyName];
        });
    }
    JQueryUICustomWidget.prototype._delay = function (callback, milliseconds) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._focusable = function (element) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._getCreateEventData = function () {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._getCreateOptions = function () {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._hide = function (element, option, callback) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._hoverable = function (element) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._off = function (element, eventName) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._on = function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._setOptions = function (options) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._show = function (element, option, callback) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._super = function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._superApply = function (args) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype._trigger = function (triggerType, event, data) {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype.destroy = function () {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype.disable = function () {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype.enable = function () {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype.instance = function () {
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype.option = function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
        throw 'Should not see this';
    };
    JQueryUICustomWidget.prototype.widget = function () {
        throw 'Should not see this';
    };
    return JQueryUICustomWidget;
})();
