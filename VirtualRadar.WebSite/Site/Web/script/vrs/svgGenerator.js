var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke = VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke === undefined ? '#000' : VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke;
    VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth = VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth === undefined ? 1 : VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth;
    VRS.globalOptions.svgAircraftMarkerNormalFill = VRS.globalOptions.svgAircraftMarkerNormalFill === undefined ? '#FFFFFF' : VRS.globalOptions.svgAircraftMarkerNormalFill;
    VRS.globalOptions.svgAircraftMarkerSelectedFill = VRS.globalOptions.svgAircraftMarkerSelectedFill === undefined ? '#FFFF00' : VRS.globalOptions.svgAircraftMarkerSelectedFill;
    VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml = VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml === undefined ?
        "<filter\n    style=\"color-interpolation-filters:sRGB\"\n    id=\"vrs-text-shadow-filter\">\n    <feFlood\n        flood-opacity=\"1\"\n        flood-color=\"rgb(0,0,0)\"\n        result=\"flood\" />\n    <feComposite\n        in=\"flood\"\n        in2=\"SourceGraphic\"\n        operator=\"in\"\n        result=\"composite1\" />\n    <feGaussianBlur\n        in=\"composite1\"\n        stdDeviation=\"1\"\n        result=\"blur\" />\n    <feOffset\n         in=\"blur\"\n         dx=\"-0.5\"\n         dy=\"-0.5\"\n         result=\"offset\" />\n    <feComposite\n        in=\"SourceGraphic\"\n        in2=\"offset\"\n        operator=\"over\"\n        result=\"composite2\" />\n</filter>" : VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml;
    VRS.globalOptions.svgAircraftMarkerStyle = VRS.globalOptions.svgAircraftMarkerStyle === undefined ?
        {
            'font-family': 'Roboto',
            'font-size': '8.5pt',
            'font-weight': 700,
            'fill': '#FFFFFF',
            'stroke': '#000',
            'stroke-width': 1,
            'stroke-opacity': 0.25
        } : VRS.globalOptions.svgAircraftMarkerStyle;
    var SvgGenerator = (function () {
        function SvgGenerator() {
        }
        SvgGenerator.prototype.generateAircraftMarker = function (embeddedSvg, fillColour, width, height, rotation, addAltitudeStalk, pinTextLines, pinTextLineHeight) {
            var result = this.createSvgNode(width, height);
            var marker = this.convertXmlIntoNode(embeddedSvg.svg);
            var markerWidth = Number(marker.getAttribute('width'));
            var markerHeight = Number(marker.getAttribute('height'));
            if (addAltitudeStalk) {
                this.addAltitudeStalk(result, width, height, markerHeight);
            }
            this.addMarker(result, marker, embeddedSvg, width, height, markerWidth, markerHeight, fillColour, rotation);
            if (pinTextLines && pinTextLines.length > 0) {
                this.addPinText(result, width, height, markerHeight, pinTextLines, pinTextLineHeight);
            }
            return result;
        };
        SvgGenerator.prototype.addAltitudeStalk = function (svg, width, height, markerHeight) {
            var x = (width / 2) - (VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth / 2);
            this.addSvgElement(svg, 'line', {
                x1: x, y1: markerHeight / 2,
                x2: x, y2: height - 2,
                'stroke': VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke,
                'stroke-width': VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth
            });
            this.addSvgElement(svg, 'line', {
                x1: x - 2, y1: height - 4,
                x2: x + 3, y2: height,
                'stroke': VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke,
                'stroke-width': VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth
            });
            this.addSvgElement(svg, 'line', {
                x1: x - 3, y1: height,
                x2: x + 2, y2: height - 4,
                'stroke': VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke,
                'stroke-width': VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth
            });
        };
        SvgGenerator.prototype.addMarker = function (svg, marker, embeddedSvg, width, height, markerWidth, markerHeight, fillColour, rotation) {
            var offsetX = (width / 2) - (markerWidth / 2);
            var centerMarkerX = offsetX + (markerWidth / 2);
            var markerGroup = this.addSvgElement(svg, 'g', {
                id: 'marker-group',
                x: offsetX,
                y: 0,
                width: markerWidth,
                height: markerHeight,
                viewBox: '' + offsetX + ' 0 ' + markerWidth + ' ' + markerHeight
            });
            if (rotation > 0) {
                this.setAttribute(markerGroup, {
                    transform: 'rotate(' + rotation % 360 + ',' + centerMarkerX + ',' + markerHeight / 2 + ')'
                });
            }
            this.setAttribute(marker, {
                x: offsetX,
                y: 0
            });
            if (fillColour && embeddedSvg.aircraftMarkerStatusFillPaths && embeddedSvg.aircraftMarkerStatusFillPaths.length) {
                var length = embeddedSvg.aircraftMarkerStatusFillPaths.length;
                for (var i = 0; i < length; ++i) {
                    var pathID = embeddedSvg.aircraftMarkerStatusFillPaths[i];
                    var element = marker.querySelector('#' + pathID);
                    if (element) {
                        element.setAttribute('fill', fillColour);
                    }
                }
            }
            markerGroup.appendChild(marker);
        };
        SvgGenerator.prototype.addPinText = function (svg, width, height, markerHeight, pinTextLines, pinTextLineHeight) {
            var countLines = pinTextLines.length;
            var filterElementID = null;
            if (VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml) {
                var filterElement = this.convertXmlIntoNode(VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml);
                svg.appendChild(filterElement);
                filterElementID = filterElement.getAttribute('id');
            }
            var centerX = width / 2;
            var pinTextHeight = pinTextLineHeight * countLines;
            var startY = height - (pinTextHeight + 7);
            var pinTextGroup = this.addSvgElement(svg, 'g', {
                id: 'pin-text',
                x: 0,
                y: startY,
                width: width,
                height: pinTextHeight
            });
            if (VRS.globalOptions.svgAircraftMarkerStyle) {
                this.setAttribute(pinTextGroup, VRS.globalOptions.svgAircraftMarkerStyle);
            }
            var textElement = this.addSvgElement(pinTextGroup, 'text', {
                x: centerX,
                y: startY,
                'text-anchor': 'middle'
            });
            if (filterElementID) {
                textElement.setAttribute('filter', 'url(#' + filterElementID + ')');
            }
            for (var i = 0; i < countLines; ++i) {
                var text = pinTextLines[i];
                if (text === null) {
                    text = '';
                }
                var tspan = this.addSvgElement(textElement, 'tspan', {
                    x: centerX,
                    dy: pinTextLineHeight
                });
                tspan.textContent = text;
            }
        };
        SvgGenerator.prototype.createElement = function (element, namespace) {
            if (namespace === void 0) { namespace = 'http://www.w3.org/2000/svg'; }
            var result = document.createElementNS(namespace, element);
            return result;
        };
        SvgGenerator.prototype.createSvgNode = function (width, height, viewBox, version, namespace) {
            if (version === void 0) { version = '1.1'; }
            if (namespace === void 0) { namespace = 'http://www.w3.org/2000/svg'; }
            if (!viewBox) {
                viewBox = '0 0 ' + width + ' ' + height;
            }
            var result = this.createElement('svg', namespace);
            this.setAttribute(result, {
                width: width,
                height: height,
                viewBox: viewBox,
                xmlns: namespace,
                version: version
            });
            return result;
        };
        SvgGenerator.prototype.convertXmlIntoNode = function (xml, namespace) {
            if (namespace === void 0) { namespace = 'http://www.w3.org/2000/svg'; }
            var dummySvgNode = this.createElement('svg', namespace);
            dummySvgNode.innerHTML = xml;
            var result = dummySvgNode.firstElementChild;
            return result;
        };
        SvgGenerator.prototype.setAttribute = function (node, attributes) {
            for (var property in attributes) {
                if (attributes.hasOwnProperty(property)) {
                    node.setAttribute(property, String(attributes[property]));
                }
            }
            return node;
        };
        SvgGenerator.prototype.addSvgElement = function (parent, element, attributes, namespace) {
            if (attributes === void 0) { attributes = null; }
            if (namespace === void 0) { namespace = null; }
            var result = this.createElement(element, namespace);
            if (attributes) {
                this.setAttribute(result, attributes);
            }
            parent.appendChild(result);
            return result;
        };
        return SvgGenerator;
    }());
    VRS.SvgGenerator = SvgGenerator;
})(VRS || (VRS = {}));
//# sourceMappingURL=svgGenerator.js.map