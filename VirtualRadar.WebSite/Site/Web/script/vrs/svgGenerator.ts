/**
 * @license Copyright © 2016 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview Handles the generation of SVG aircraft map markers.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke = VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke === undefined ? '#000' : VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke;
    VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth = VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth === undefined ? 1 : VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth;
    VRS.globalOptions.svgAircraftMarkerNormalFill = VRS.globalOptions.svgAircraftMarkerNormalFill === undefined ? '#FFFFFF' : VRS.globalOptions.svgAircraftMarkerNormalFill;
    VRS.globalOptions.svgAircraftMarkerSelectedFill = VRS.globalOptions.svgAircraftMarkerSelectedFill === undefined ? '#FFFF00' : VRS.globalOptions.svgAircraftMarkerSelectedFill;
    VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml = VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml === undefined ?
`<filter
    xmlns="http://www.w3.org/2000/svg"
    style="color-interpolation-filters:sRGB"
    id="vrs-text-shadow-filter">
    <feMorphology
        in="SourceAlpha"
        operator="dilate"
        radius="1"
        result="fat-text" />
    <feGaussianBlur
        in="fat-text"
        stdDeviation="1.5"
        result="blur" />
    <feComposite
        in="SourceGraphic"
        in2="blur"
        operator="over" />
</filter>` : VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml;
    VRS.globalOptions.svgAircraftMarkerTextStyle = VRS.globalOptions.svgAircraftMarkerTextStyle === undefined ?
    {
        'font-family': 'Roboto, Sans-Serif',
        'font-size': '8pt',
        'font-weight': '700',
        'fill': '#FFFFFF'
    } : VRS.globalOptions.svgAircraftMarkerTextStyle;

    /**
     * A class that can generate SVG elements and strings.
     */
    export class SvgGenerator
    {
        private _DomParser = new DOMParser();
        private _XmlSerialiser = new XMLSerializer();

        /**
         * Serialises an SVG element into a string.
         * @param svg
         */
        public serialiseSvg(svg: Element) : string
        {
            return this._XmlSerialiser.serializeToString(svg);
        }

        /**
         * Returns true if SVG graphics should be used, false if they should not.
         */
        public static useSvgGraphics() : boolean
        {
            return Modernizr.svg && (VRS.serverConfig ? VRS.serverConfig.get().UseSvgGraphics : false);
        }

        /**
         * Generates a disconnected DOM element for an aircraft marker's SVG.
         * @param embeddedSvg
         * @param fillColour
         * @param width
         * @param height
         * @param rotation
         * @param addAltitudeStalk
         * @param pinTextLines
         * @param pinTextLineHeight
         * @param isHighDpi
         */
        public generateAircraftMarker(embeddedSvg: EmbeddedSvg, fillColour: string, width: number, height: number, rotation: number, addAltitudeStalk: boolean, pinTextLines: string[], pinTextLineHeight: number, isHighDpi: boolean) : Element
        {
            var result = this.createSvgNode(width, height, this.buildViewBox(0, 0, width, height));

            var marker = this.convertXmlIntoNode(embeddedSvg.svg);
            var markerWidth = Number(marker.getAttribute('width'));
            var markerHeight = Number(marker.getAttribute('height'));

            if(addAltitudeStalk) {
                this.addAltitudeStalk(result, width, height, markerHeight);
            }

            this.addMarker(result, marker, embeddedSvg, width, height, markerWidth, markerHeight, fillColour, rotation);

            if(pinTextLines && pinTextLines.length > 0) {
                this.addPinText(result, width, height, markerHeight, pinTextLines, pinTextLineHeight);
            }

            return result;
        }

        private addAltitudeStalk(svg: Element, width: number, height: number, markerHeight: number)
        {
            var x = (width / 2) - (VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth / 2);

            this.addSvgElement(svg, 'line', {
                x1: x, y1: markerHeight / 2,
                x2: x, y2: height - 2,
                'stroke':       VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke,
                'stroke-width': VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth
            });
            this.addSvgElement(svg, 'line', {
                x1: x - 2, y1: height - 4,
                x2: x + 3, y2: height,
                'stroke':       VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke,
                'stroke-width': VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth
            });
            this.addSvgElement(svg, 'line', {
                x1: x - 3, y1: height,
                x2: x + 2, y2: height - 4,
                'stroke':       VRS.globalOptions.svgAircraftMarkerAltitudeLineStroke,
                'stroke-width': VRS.globalOptions.svgAircraftMarkerAltitudeLineWidth
            });
        }

        private addMarker(svg: Element, marker: Element, embeddedSvg: EmbeddedSvg, width: number, height: number, markerWidth: number, markerHeight: number, fillColour: string, rotation: number)
        {
            var offsetX = (width / 2) - (markerWidth / 2);
            var centerMarkerX = offsetX + (markerWidth / 2);

            var markerGroup = this.addSvgElement(svg, 'g', {
                id: 'marker-group',
                x: offsetX,
                y: 0,
                width: markerWidth,
                height: markerHeight
            });
            if(rotation > 0) {
                this.setAttribute(markerGroup, {
                    transform: 'rotate(' + rotation % 360 + ',' + centerMarkerX + ',' + markerHeight / 2 + ')'
                });
            }

            this.setAttribute(marker, {
                x: offsetX,
                y: 0
            });

            if(fillColour && embeddedSvg.aircraftMarkerStatusFillPaths && embeddedSvg.aircraftMarkerStatusFillPaths.length) {
                var length = embeddedSvg.aircraftMarkerStatusFillPaths.length;
                for(var i = 0;i < length;++i) {
                    var pathID = embeddedSvg.aircraftMarkerStatusFillPaths[i];
                    var element = marker.querySelector('#' + pathID);
                    if(element) {
                        element.setAttribute('fill', fillColour);
                    }
                }
            }

            markerGroup.appendChild(marker);
        }

        private addPinText(svg: Element, width: number, height: number, markerHeight: number, pinTextLines: string[], pinTextLineHeight: number)
        {
            var countLines = pinTextLines.length;

            var filterElementID: string = null;
            if(VRS.globalOptions.svgAircraftMarkerTextShadowFilterXml) {
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

            var textElement = this.addSvgElement(pinTextGroup, 'text', { 
                x: centerX,
                y: startY,
                'text-anchor': 'middle'
            });
            if(filterElementID) {
                textElement.setAttribute('filter', 'url(#' + filterElementID + ')');
            }

            for(var i = 0;i < countLines;++i) {
                var text = pinTextLines[i];
                if(text === null) {
                    text = '';
                }

                var tspan = this.addSvgElement(textElement, 'tspan', {
                    x: centerX,
                    dy: pinTextLineHeight
                });
                tspan.textContent = text;

                if(VRS.globalOptions.svgAircraftMarkerTextStyle) {
                    this.setAttribute(tspan, VRS.globalOptions.svgAircraftMarkerTextStyle);
                }
            }
        }

        private buildViewBox(x: number, y: number, width: number, height: number) : string
        {
            return '' + x + ' ' + y + ' ' + width + ' ' + height;
        }

        private createElement(element: string, namespace: string = 'http://www.w3.org/2000/svg') : any
        {
            var result = document.createElementNS(namespace, element);
            return result;
        }

        private createSvgNode(width: number, height: number, viewBox: string, version: string = '1.1', namespace: string = 'http://www.w3.org/2000/svg') : SVGSVGElement
        {
            var result = this.createElement('svg', namespace);
            this.setAttribute(result, {
                width: width,
                height: height,
                viewBox: viewBox,
                version: version
            });

            return result;
        }

        private convertXmlIntoNode(xml: string, namespace: string = 'http://www.w3.org/2000/svg') : Element
        {
            var xmlDoc = this._DomParser.parseFromString(xml, 'image/svg+xml');

            var result: Element = null;
            var length = xmlDoc.childNodes.length;
            for(var i = 0;i < length;++i) {
                var node = xmlDoc.childNodes[i];
                if(node instanceof Element) {
                    result = <Element>node;
                    break;
                }
            }

            return result;
        }

        private setAttribute(node: Element, attributes: Object) : Element
        {
            for(var property in attributes) {
                if(attributes.hasOwnProperty(property)) {
                    node.setAttribute(property, String(attributes[property]));
                }
            }

            return node;
        }

        private addSvgElement(parent: Element, element: string, attributes: Object = null, namespace: string = 'http://www.w3.org/2000/svg') : Element
        {
            var result = this.createElement(element, namespace);
            if(attributes) {
                this.setAttribute(result, attributes);
            }
            parent.appendChild(result);

            return result;
        }
    }
}