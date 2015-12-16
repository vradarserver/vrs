var JQueryUICustomWidget = (function () {
    function JQueryUICustomWidget() {
        var myPrototype = JQueryUICustomWidget.prototype;
        $.each(myPrototype, function (propertyName, value) {
            delete myPrototype[propertyName];
        });
    }
    JQueryUICustomWidget.prototype._trigger = function (triggerType, e, d) {
    };
    return JQueryUICustomWidget;
})();
//# sourceMappingURL=jquery.ui.widget.js.map