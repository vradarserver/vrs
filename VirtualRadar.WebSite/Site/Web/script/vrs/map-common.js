var VRS;
(function (VRS) {
    var MapIcon = (function () {
        function MapIcon(url, size, anchor, origin, scaledSize, labelAnchor) {
            this.url = url;
            this.size = size;
            this.anchor = anchor;
            this.origin = origin;
            this.scaledSize = scaledSize;
            this.labelAnchor = labelAnchor;
        }
        return MapIcon;
    }());
    VRS.MapIcon = MapIcon;
})(VRS || (VRS = {}));
//# sourceMappingURL=map-common.js.map