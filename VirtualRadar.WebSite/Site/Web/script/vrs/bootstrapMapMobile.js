var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var BootstrapMapMobile = (function (_super) {
        __extends(BootstrapMapMobile, _super);
        function BootstrapMapMobile() {
            _super.call(this, {
                configPrefix: 'mobile',
                reportUrl: 'mobileReport.html',
                mapSettings: {
                    controlStyle: VRS.MapControlStyle.DropdownMenu,
                    controlPosition: VRS.MapPosition.TopLeft
                },
                suppressTitleUpdate: true,
                settingsPosition: VRS.MapPosition.TopLeft,
                settingsMenuAlignment: VRS.Alignment.Left,
                showOptionsInPage: true
            });
        }
        return BootstrapMapMobile;
    })(VRS.BootstrapMap);
    VRS.BootstrapMapMobile = BootstrapMapMobile;
})(VRS || (VRS = {}));
