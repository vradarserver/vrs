var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke = VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke === undefined ? '#000' : VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke;
    VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth = VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth === undefined ? 1 : VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth;
    VRS.globalOptions.svgAircraftMarkerNormalFill = VRS.globalOptions.svgAircraftMarkerNormalFill === undefined ? '#FFFFFF' : VRS.globalOptions.svgAircraftMarkerNormalFill;
    VRS.globalOptions.svgAircraftMarkerSelectedFill = VRS.globalOptions.svgAircraftMarkerSelectedFill === undefined ? '#FFFF00' : VRS.globalOptions.svgAircraftMarkerSelectedFill;
    VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml = VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml === undefined ?
        "<filter\n    xmlns=\"http://www.w3.org/2000/svg\"\n    style=\"color-interpolation-filters:sRGB\"\n    id=\"vrs-text-shadow-filter\">\n    <feMorphology\n        in=\"SourceAlpha\"\n        operator=\"dilate\"\n        radius=\"1\"\n        result=\"fat-text\" />\n    <feGaussianBlur\n        in=\"fat-text\"\n        stdDeviation=\"1.5\"\n        result=\"blur\" />\n    <feComposite\n        in=\"SourceGraphic\"\n        in2=\"blur\"\n        operator=\"over\" />\n</filter>" : VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml;
    VRS.globalOptions.svgAircraftMarkerTextStyle = VRS.globalOptions.svgAircraftMarkerTextStyle === undefined ?
        {
            'font-family': 'Roboto, Sans-Serif',
            'font-size': '8pt',
            'font-weight': '700',
            'fill': '#FFFFFF'
        } : VRS.globalOptions.svgAircraftMarkerTextStyle;
    var SvgGenerator = (function () {
        function SvgGenerator() {
            this._DomParser = new DOMParser();
            this._XmlSerialiser = new XMLSerializer();
        }
        SvgGenerator.prototype.serialiseSvg = function (svg) {
            return this._XmlSerialiser.serializeToString(svg);
        };
        SvgGenerator.useSvgGraphics = function () {
            return Modernizr.svg && (VRS.serverConfig ? VRS.serverConfig.get().UseSvgGraphics : false);
        };
        SvgGenerator.prototype.generateAircraftMarker = function (embeddedSvg, fillColour, width, height, rotation, addAltitudeStalk, pinTextLines, pinTextLineHeight, isHighDpi) {
            var result = this.createSvgNode(width, height, this.buildViewBox(0, 0, width, height));
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
                height: markerHeight
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
            var startY = height - (pinTextHeight + 5);
            var pinTextGroup = this.addSvgElement(svg, 'g', {
                id: 'pin-text',
                x: 0,
                y: startY,
                width: width,
                height: pinTextHeight
            });
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
                if (text === null || text === undefined || text === '') {
                    text = ' ';
                }
                var tspan = this.addSvgElement(textElement, 'tspan', {
                    x: centerX,
                    dy: pinTextLineHeight
                });
                tspan.textContent = text;
                if (VRS.globalOptions.svgAircraftMarkerTextStyle) {
                    this.setAttribute(tspan, VRS.globalOptions.svgAircraftMarkerTextStyle);
                }
            }
        };
        SvgGenerator.prototype.buildViewBox = function (x, y, width, height) {
            return '' + x + ' ' + y + ' ' + width + ' ' + height;
        };
        SvgGenerator.prototype.createElement = function (element, namespace) {
            if (namespace === void 0) { namespace = 'http://www.w3.org/2000/svg'; }
            var result = document.createElementNS(namespace, element);
            return result;
        };
        SvgGenerator.prototype.createSvgNode = function (width, height, viewBox, version, namespace) {
            if (version === void 0) { version = '1.1'; }
            if (namespace === void 0) { namespace = 'http://www.w3.org/2000/svg'; }
            var result = this.createElement('svg', namespace);
            this.setAttribute(result, {
                width: width,
                height: height,
                viewBox: viewBox,
                version: version
            });
            return result;
        };
        SvgGenerator.prototype.convertXmlIntoNode = function (xml, namespace) {
            if (namespace === void 0) { namespace = 'http://www.w3.org/2000/svg'; }
            var xmlDoc = this._DomParser.parseFromString(xml, 'image/svg+xml');
            var result = null;
            var length = xmlDoc.childNodes.length;
            for (var i = 0; i < length; ++i) {
                var node = xmlDoc.childNodes[i];
                if (node instanceof Element) {
                    result = node;
                    break;
                }
            }
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
            if (namespace === void 0) { namespace = 'http://www.w3.org/2000/svg'; }
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