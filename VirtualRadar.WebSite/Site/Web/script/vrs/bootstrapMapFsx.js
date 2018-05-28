var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var VRS;
(function (VRS) {
    var BootstrapMapFsx = (function (_super) {
        __extends(BootstrapMapFsx, _super);
        function BootstrapMapFsx() {
            var _this = _super.call(this, {
                configPrefix: 'fsx',
                suppressTitleUpdate: true
            }) || this;
            document.title = VRS.$$.FlightSimTitle;
            return _this;
        }
        return BootstrapMapFsx;
    }(VRS.BootstrapMap));
    VRS.BootstrapMapFsx = BootstrapMapFsx;
})(VRS || (VRS = {}));
//# sourceMappingURL=bootstrapMapFsx.js.map