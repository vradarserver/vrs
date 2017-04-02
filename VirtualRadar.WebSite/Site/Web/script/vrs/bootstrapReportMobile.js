var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var BootstrapReportMobile = (function (_super) {
        __extends(BootstrapReportMobile, _super);
        function BootstrapReportMobile() {
            return _super.call(this, {
                configPrefix: 'mobile-report',
                showOptionsInPage: true
            }) || this;
        }
        return BootstrapReportMobile;
    }(VRS.BootstrapReport));
    VRS.BootstrapReportMobile = BootstrapReportMobile;
})(VRS || (VRS = {}));
//# sourceMappingURL=bootstrapReportMobile.js.map