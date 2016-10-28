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
 * @fileoverview Static collection of embedded SVGs.
 */

namespace VRS
{
    /**
     * The interface for an embedded SVG.
     */
    export interface EmbeddedSvg
    {
        /**
         * The entire SVG as a single string.
         */
        svg: string;

        /**
         * The paths that should be filled with colours indicating aircraft marker status.
         */
        aircraftMarkerStatusFillPaths?: string[];
    }

    /**
     * Embedded SVGs for aircraft markers.
     */
    // Best viewed with outlining collapsed :)
    export class EmbeddedSvgs
    {
        public static Marker_4TurboProp: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="40"
   height="40"
   viewBox="0 0 40 40"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4194">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="display:inline">
    <path
       style="opacity:1;fill-opacity:1;stroke:#000000;stroke-width:1;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4194)"
       d="m 20.650631,8.756977 c 0.397816,0.288339 0.79714,1.874014 1.364913,3.637949 0.04832,0.250455 0.01064,4.612208 0.01064,4.612208 l -0.0051,0.978996 1.169625,-0.0098 0.01231,-1.107635 c 0,0 0.416489,-1.164358 1.023576,-1.193369 0.5587,-0.0267 0.956925,1.193807 0.956925,1.193807 l 0.03227,1.149227 1.566977,-0.01231 0.02717,-1.308024 c 0,0 0.489726,-0.956647 0.976886,-0.995968 0.469217,-0.03787 1.050745,1.137355 1.050745,1.137355 l 0.01231,1.248147 c 0,0 3.036041,0.121021 4.825345,0.220348 0.97075,0.05389 0.21991,2.637243 0.21991,2.637243 l -9.759807,1.386547 -1.9856,-0.01741 -0.03992,5.101345 -0.07896,0.821075 c 0,0 3.601551,0.68866 3.779698,0.926769 0.362956,0.485123 0.131189,1.797522 0.131189,1.797522 l -4.818133,0.852469 -0.289109,0.847807 -1.181935,0.01741 -0.229233,-0.85458 -4.957408,-0.875416 c 0,0 -0.160791,-1.287643 0.05944,-1.603907 0.253174,-0.363573 3.894789,-1.097875 3.894789,-1.097875 l -0.140511,-0.905135 -0.09936,-5.007086 -1.830666,0.0025 -10.1304265,-1.171737 c 0,0 -0.5627723,-1.709375 0.155809,-2.441516 0.3889468,-0.396287 5.1891915,-0.534515 5.1891915,-0.534515 l -0.02251,-1.14158 c 0,0 0.25659,-1.310977 1.017164,-1.268544 0.508601,0.02838 0.902585,1.184047 0.902585,1.184047 l 0.01996,1.117396 1.61243,0.01486 0.01231,-1.307587 c 0,0 0.469442,-1.084808 0.971348,-1.087676 0.474059,-0.0027 0.915334,1.176398 0.915334,1.176398 l 0.02462,1.156439 1.216752,-0.01231 0.01144,-1.169623 c 0,0 -0.06346,-3.857753 -0.06832,-4.491218 0.0682,-0.530672 0.708376,-2.7498298 1.107635,-3.3628238 0.274885,-0.3875923 0.861553,-0.5371573 1.365709,-0.2403072 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Generic: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="35"
   height="35"
   viewBox="0 0 35 35"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4194">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="display:inline">
    <path
       style="opacity:1;fill-opacity:1;stroke:#000000;stroke-width:1;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4194)"
       d="M 19.598813,8.1191146 19.642857,13.866071 29,19 c 0,0 0.470511,0.475196 0.464286,0.776786 -0.0064,0.309486 -0.508929,0.776785 -0.508929,0.776785 l -9.401786,0.223215 -0.04464,5.267857 3.178571,2.241071 0,1.401786 -4.401786,0 c 0,0 -0.266008,-0.831484 -0.6875,-0.821429 -0.427228,0.01019 -0.642857,0.776786 -0.642857,0.776786 l -4.375,0 0,-1.133928 2.955357,-2.464286 -0.04464,-5.267857 -8.491076,0 c 0,0 -1.0035801,-0.551474 -1.0625,-1.044643 -0.036788,-0.307924 0.491071,-0.790179 0.491071,-0.790179 l 9.017858,-5.165178 -0.01116,-5.7243307 c 0,0 0.434941,-3.2832986 2.108479,-3.2438829 1.790301,0.042166 2.055065,3.3105422 2.055065,3.3105422 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_GroundVehicle: EmbeddedSvg = {
            svg: `
<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="26"
   height="24"
   viewBox="0 0 26 24"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <pattern
       xlink:href="#checkerboard-content"
       id="checkerboard-pattern"
       patternTransform="matrix(2,0,0,2,0.04419417,1.0606602)" />
    <pattern
       id="checkerboard-content"
       patternTransform="translate(0,0) scale(10,10)"
       height="2"
       width="2"
       patternUnits="userSpaceOnUse">
      <rect
         id="top-left"
         x="0"
         y="0"
         width="1"
         height="1"
         style="fill:#363b4e;stroke:none" />
      <rect
         id="top-right" 
         x="1"
         y="0"
         width="1"
         height="1"
         style="fill:#fff422;stroke:none" />
      <rect
         id="bottom-left"
         x="0"
         y="1"
         width="1"
         height="1"
         style="fill:#fff422;stroke:none" />
      <rect
         id="bottom-right"
         x="1"
         y="1"
         width="1"
         height="1"
         style="fill:#363b4e;stroke:none" />
    </pattern>
    <filter
       style="color-interpolation-filters:sRGB"
       id="outline-shadow">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline-group"
     style="display:inline;opacity:1">
    <path
       style="opacity:1;fill:url(#checkerboard-pattern);fill-opacity:1;stroke:#000000;stroke-width:1;stroke-linecap:round;stroke-linejoin:round;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#outline-shadow)"
       d="m 10,3 6,0 2,3 0,15 L 8,21 8,6 10,3"
       id="outline-path" />
  </g>
  <g
     id="windscreen-group"
     style="opacity:1">
    <path
       style="display:inline;opacity:1;fill:#73a6aa;fill-opacity:1;stroke:#000000;stroke-width:1;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1"
       d="m 18,10.625 0,3 c -2,-2 -8,-2 -10,0 l 0,-3 c 2,-3 8,-3 10,0 z"
       id="windscreen-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: null
        };

        public static Marker_Heavy2Jet: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="57"
   height="57"
   viewBox="0 0 57 57"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4253">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="display:inline;opacity:1"
     transform="translate(0,7)">
    <path
       style="fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.8;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4253)"
       d="m 28.887909,4.7710751 c -1.006718,0.03322 -1.811085,2.6846543 -1.925604,4.0090423 -0.152638,1.7652346 -0.142053,2.2097096 -0.142053,2.2097096 l -0.01578,4.766657 -1.246907,0.899667 c 0,0 0.02722,-1.562597 -0.520858,-1.56258 -1.413581,4.4e-5 -2.067655,0.03156 -2.067655,0.03156 0,0 -0.297137,0.66762 -0.331456,0.978586 -0.08937,0.809814 0.12627,1.925602 0.12627,1.925602 l 0.252537,0.457726 -9.296558,6.565991 c 0,0 -0.127815,2.244607 0.07892,2.099222 1.983227,-0.989611 5.180326,-2.293389 9.959471,-3.86699 1.00065,-0.204529 3.062024,-0.110481 3.062024,-0.110481 l -0.01578,5.113896 c 0,0 0.157371,2.156614 0.394592,3.709154 0.06034,0.394931 -4.076998,3.154287 -4.166879,3.661802 -0.136891,0.772961 -0.04735,2.051872 -0.04735,2.051872 l 5.177031,-1.90982 0.678696,1.7362 0.773399,-1.7362 5.161247,1.925604 c 0,0 0.05306,-1.402476 -0.01578,-2.004522 -0.06773,-0.592383 -4.301004,-3.253382 -4.182662,-3.772287 0.143128,-0.627583 0.34083,-2.190054 0.359243,-3.59892 0.03358,-2.569586 0.03535,-5.17678 0.03535,-5.17678 0,0 2.55338,-0.08009 3.125159,0.173619 2.824811,0.987004 5.90885,1.835653 10.03839,3.756504 0.04633,-1.040093 0.04735,-2.083439 0.04735,-2.083439 l -9.291609,-6.460921 0.200232,-0.278686 c 0.04284,-0.593883 0.262522,-1.950867 -0.220967,-3.172509 -0.886701,-0.0176 -2.445863,-0.05566 -2.30441,4e-6 -0.463776,0.366594 -0.457727,1.483661 -0.457727,1.483661 L 30.95558,15.740703 30.9398,11.005613 c -1.55e-4,-0.04627 0.0074,-0.587884 -0.17362,-2.2412766 -0.190209,-1.737455 -0.923414,-4.0247736 -1.878271,-3.9932613 z"
       id="outline-path"
    />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Heavy4Jet: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="60"
   height="60"
   viewBox="0 0 60 60"
   id="svg4148"
   version="1.1">
  <defs
     id="defs4150">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter7068">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="opacity:1">
    <path
       style="fill-rule:evenodd;stroke:#000000;stroke-width:0.8;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;fill-opacity:1;filter:url(#filter7068)"
       d="m 29.99265,10.916982 c 1.665828,-0.0081 2.153163,5.883291 2.153163,5.883291 l 0,5.034155 2.835503,2.62322 0,-1.182723 2.047022,0 c 0,0 0.406231,1.058427 0.121305,2.304794 -0.05573,0.243787 -0.181958,0.727829 -0.181958,0.727829 l 2.850666,2.653547 0.06065,-0.940114 1.925716,0 c 0,0 0.355322,0.951141 0.227447,1.758922 -0.03876,0.244822 -0.212283,0.712666 -0.212283,0.712666 l 0,0.257774 4.981084,4.215347 c 0,0 0.262231,1.05343 0.470052,1.690688 l -0.0834,1.766503 -1.197886,-1.122071 -9.370808,-5.519376 -4.47312,-1.440496 -0.07582,8.036454 c 0,0 -0.171778,2.523324 -0.561034,3.684638 0.977319,1.181928 4.639914,4.457957 4.639914,4.457957 l 0.01517,2.016694 -5.504213,-1.304028 -0.621688,1.122071 -0.667176,-1.152397 -5.44356,1.288866 -1e-6,-1.971206 c 0,0 4.181105,-3.612811 4.533773,-4.457958 -0.323369,-1.33232 -0.470056,-3.669475 -0.470056,-3.669475 l -0.07582,-8.02129 -4.51861,1.45566 -9.461787,5.526957 -1.091744,0.811227 0,-1.576965 0.470057,-1.683106 5.034155,-4.215346 c 0,0 -0.124815,-0.553261 -0.151631,-0.788486 -0.07396,-0.648772 0.04549,-1.910554 0.04549,-1.910554 l 2.107674,-0.01516 0.03033,0.894624 2.835502,-2.653546 c 0,0 -0.189728,-0.845351 -0.242609,-1.258539 -0.106854,-0.834906 0.06065,-1.774085 0.06065,-1.774085 l 2.153163,0 -0.01517,1.197886 2.744524,-2.53224 1e-6,-5.140297 c 0,0 0.598064,-5.785137 2.077363,-5.792312 z"
       id="outline-path"
    />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Helicopter: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="32"
   height="32"
   viewBox="0 0 32 32"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB;"
       id="filter4234">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="opacity:1">
    <path
       style="fill-rule:evenodd;stroke:#000000;stroke-width:0.8;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1;fill-opacity:1;filter:url(#filter4234);stroke-miterlimit:4;stroke-dasharray:none"
       d="M 15.982143,7.3571428 C 14.796622,7.330505 14.464285,9.2544642 14.464285,9.2544642 L 14.399549,10.564732 9.6830358,6.3883928 8.796875,7.0892856 8.7339373,8.000213 14.308032,13.129464 13.428567,13.946429 8.22321,18.622768 9.2254422,19.647321 9.92187,19.53125 l 4.555804,-4.145089 0.540178,4.857143 -1.709821,0.01117 -0.325893,0.595982 0.245536,1.08259 1.966517,0.12054 0.528801,3.550652 0.393074,0.01185 0.341214,-0.05555 0.424411,-3.520342 1.924108,-0.100445 0.279017,-1.109375 -0.339285,-0.571428 -1.700893,-0.0089 0.486607,-4.535714 4.834822,4.287946 0.830357,-1.109375 -0.176335,-0.779018 -5.287946,-4.76116 0.0039,-0.359031 5.992516,-5.1572768 -0.949547,-1.1019943 -0.671875,0.040179 -4.4375,3.8448601 -0.0692,-1.3604851 c 0,0 -0.326391,-1.8727707 -1.618315,-1.9018061 z"
       id="outline-path"
    />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Light1Prop: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="32"
   height="32"
   viewBox="0 0 32 32"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB;"
       id="filter4234">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="display:inline">
    <path
       style="opacity:1;fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.80000001;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4234)"
       d="m 16.05811,10.880087 0.553538,1.058208 c 0,0 0.472498,0.141768 0.501761,0.312501 0.172387,1.005757 0.181678,1.938952 0.181678,1.938952 l 2.508725,0.0065 c 0,0 3.989671,0.154006 5.321226,0.334595 0.45805,0.238645 0.20757,2.635566 0.20757,2.635566 l -5.555712,0.746724 -2.629554,-0.01105 -0.149642,3.325613 c 0,0 0.291598,0.09683 1.873215,0.419844 0.567168,0.777185 -0.0442,1.944543 -0.0442,1.944543 l -2.021883,0.276214 -0.707107,0.01105 -0.143631,-0.331456 -0.15468,0.331457 -0.817589,-0.04419 -1.878253,-0.265164 c 0,0 -0.517698,-1.658851 0.154679,-2.043981 1.279959,-0.212093 1.734162,-0.284581 1.734162,-0.284581 L 14.882384,17.868905 12.241782,17.85786 6.6954133,17.084463 c 0,0 -0.3000078,-2.590865 0.3093594,-2.629554 3.0611123,-0.236503 5.2161573,-0.275984 5.2161573,-0.275984 l 2.54241,0.01127 c 0,0 0.01687,-1.352216 0.24797,-2.022669 0.04321,-0.12536 0.456647,-0.247315 0.456647,-0.247315 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Light2Prop: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="36"
   height="36"
   viewBox="0 0 36 36"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4759">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     transform="translate(0,4)"
     style="display:inline;opacity:1">
    <path
       style="opacity:1;fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.71766599;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4759)"
       d="m 18.006689,8.4882421 c 1.189205,-0.01278 1.421875,3.8906249 1.421875,3.8906249 l 1.013135,0.12366 c 0,0 -0.05631,-0.813085 0.125,-1.179688 0.08267,-0.167159 0.296875,-0.375 0.296875,-0.375 0,0 -0.02055,-0.9672609 0.609375,-0.9843749 0.70008,-0.01902 0.6875,1.0312499 0.6875,1.0312499 0,0 0.362719,0.204431 0.421875,0.421875 0.120511,0.442973 0.113161,1.233265 0.113161,1.233265 0,0 0.871125,0.04908 4.123699,0.07276 0.691673,0.005 0.234375,2.3125 0.234375,2.3125 l -7.67187,1.437503 -0.390625,3.0625 c 0,0 0.394478,0.04681 2.453125,0.515625 0.56669,0.129051 0.0625,1.625 0.0625,1.625 l -3.0625,0.234375 -0.421875,0.546875 -0.421875,-0.578125 -3.09375,-0.28125 c 0,0 -0.508129,-1.428392 0.125,-1.5625 1.719119,-0.364139 2.421875,-0.5 2.421875,-0.5 l -0.421875,-3.109375 -7.59375,-1.390625 c 0,0 -0.5589926,-2.383352 0.1875,-2.375 2.949929,0.033 4.0625,0 4.0625,0 0,0 -0.06182,-0.742045 0.10937,-1.15625 0.08445,-0.204329 0.09525,-0.249939 0.390625,-0.421875 -0.0034,-0.479346 0.04639,-1.044561 0.796875,-0.984375 0.420695,0.03374 0.556724,0.446162 0.5625,0.984375 0.140476,0.117768 0.197962,0.20414 0.238281,0.261719 0.273153,0.03732 0.253907,1.175781 0.253907,1.175781 l 0.992187,-0.1875 c 0,0 0.206727,-3.8311919 1.375005,-3.8437499 z"
       id="outline-path"
       transform="matrix(1.1147247,0,0,1.1147247,-2.057307,-1.6496713)" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Medium2Jet: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="40"
   height="40"
   viewBox="0 0 40 40"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4194">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     transform="translate(0,9.0000004)"
     style="display:inline;opacity:1">
    <path
       style="fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.75;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1;filter:url(#filter4194);stroke-miterlimit:4;stroke-dasharray:none"
       d="m 20.047765,-1.4164221 c -1.077558,0.031588 -1.578202,2.37408345 -1.633331,2.6993142 -0.07206,1.2300474 -0.0472,4.7773739 -0.0472,4.7773739 l -0.673248,0.3459711 -0.01669,-0.3170476 -0.05006,-0.8343336 -1.902283,0 -0.06675,1.6019214 0.166868,0.5506616 c 0,0 -4.742758,2.4104002 -5.9237742,3.0870381 -0.1926247,0.211639 -0.083434,1.835536 -0.083434,1.835536 l 6.4577482,-2.052463 2.102523,4e-6 3e-6,4.889199 0.35042,1.969029 -2.803363,1.735415 0,1.81885 3.287277,-0.967829 0.317047,1.101322 1.051262,4e-6 0.20024,-1.151381 3.354024,0.834334 -0.133493,-1.702042 -2.753304,-1.668669 0.383794,-1.969029 0.03337,-4.922573 1.835536,0 6.975035,2.185956 c 0,0 -0.04908,-1.577846 -0.266986,-1.885596 C 29.177708,9.9695601 24.285221,7.4241329 24.285221,7.4241329 l 0.23361,-0.6674708 -0.05006,-1.4684292 -2.002402,0.01669 c -0.03854,0.6147912 -0.219772,0.4596316 -0.18355,1.0679481 L 21.698797,6.1058836 c -0.0172,-1.6764379 0.07023,-3.1346146 -0.06674,-4.805766 -0.310453,-2.08555889 -1.148455,-2.729316 -1.584292,-2.7165397 z"
       id="outline-path"
       transform="matrix(1.0556793,0,0,1.0556793,-1.0835713,0.79709884)"
    />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Medium2TurboProp: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="40"
   height="40"
   viewBox="0 0 40 40"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4194">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="opacity:1">
    <path
       style="fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.80000001;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4194)"
       d="m 19.995692,11.377686 c 1.208452,7.41e-4 1.618148,3.646088 1.618148,3.646088 l 0.02101,3.288834 1.774524,0 0,-1.103283 c 0,0 0.356381,-1.101545 0.86671,-1.108382 0.55331,-0.0074 0.954067,1.08917 0.954067,1.08917 l 0.03152,1.11379 c 0,0 3.622514,0.580296 5.871919,0.786257 0.403361,0.05692 0.283702,1.922865 0.283702,1.922865 l -6.145113,0.422101 -0.03152,0.682984 c 0,0 -0.233618,1.156408 -0.882317,1.164215 -0.640918,0.0077 -0.938459,-1.071451 -0.938459,-1.071451 l 0.02101,-0.693491 -1.816554,-0.01051 0.01051,2.18555 c 0,0 -0.286892,3.527289 -0.682985,5.00155 1.866094,0.232303 2.668894,0.346746 2.668894,0.346746 0,0 0.324893,1.581611 -0.168119,2.122506 -0.981303,0.106402 -3.089193,0.05254 -3.089193,0.05254 l -0.36776,0.49385 -0.357254,-0.493851 c 0,0 -2.033202,0.09899 -2.994625,-0.105074 -0.529318,-0.455739 -0.25218,-2.101492 -0.25218,-2.101492 0,0 1.637092,-0.212817 2.637372,-0.346745 C 18.568542,26.877385 18.40906,23.67141 18.40906,23.67141 l -1e-6,-2.154028 -1.806045,-0.04023 -1e-6,0.640955 c 0,0 -0.344464,1.195123 -0.945362,1.185231 -0.64942,-0.01069 -0.954067,-1.104778 -0.954067,-1.104778 l 0,-0.693491 -6.0244312,-0.513062 c 0,0 -0.052114,-1.893498 0.1786271,-1.933372 1.6875091,-0.175559 5.8142811,-0.663774 5.8142811,-0.663774 l 0.02101,-1.176835 c 0,0 0.356691,-1.18797 0.941757,-1.19754 0.655185,-0.01072 0.947164,1.180131 0.947164,1.180131 l 0.01051,1.071761 1.82706,0.02972 0.01051,-3.288834 c 0,0 0.278718,-3.63637 1.56562,-3.635578 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_Medium4Jet: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="40"
   height="40"
   viewBox="0 0 40 40"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4194">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="opacity:1">
    <path
       style="opacity:1;fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.80000001;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4194)"
       d="m 19.988351,10.612372 c 1.051903,0.0035 1.708363,4.32217 1.708363,4.32217 l 0.03062,2.229229 0.39403,0.131344 c 0,0 -0.105772,-1.119646 0.299462,-1.407999 0.142017,-0.101055 1.275241,-0.05827 1.422987,-0.02282 0.64881,0.155679 0.381721,1.994613 0.381721,1.994613 l 0.748551,0.276337 c 0,0 0.0061,-1.313751 0.257433,-1.33115 0.146893,-0.01017 1.291977,-0.01931 1.444311,0.01576 0.436313,0.100451 0.320323,1.861622 0.320323,1.861622 0,0 0.707685,0.287105 2.669567,0.893752 0.182518,0.05706 0.105074,1.849313 0.105074,1.849313 l -8.05922,-0.756537 c 0,0 0.192132,1.846843 -0.914149,6.062803 1.185794,0.516268 1.909715,0.811522 3.257311,1.334446 0.441295,0.262437 0.21015,1.723224 0.21015,1.723224 l -3.887759,-0.241672 -0.367761,0.640955 -0.388776,-0.66197 -3.898267,0.399284 c 0,0 -0.19296,-1.200694 0.12609,-1.649671 1.672464,-0.865153 2.703022,-1.101703 3.372894,-1.450028 -0.994897,-3.855896 -0.851104,-6.094326 -0.851104,-6.094326 l -7.93599,1.016674 c 0,0 -0.147883,-0.937903 -0.05689,-1.698912 0.175763,-0.228487 2.904027,-1.286572 2.904027,-1.286572 0,0 -0.188224,-1.451557 0.178319,-1.818099 0.160981,-0.160981 1.496892,-0.153083 1.647122,-0.04458 0.183235,0.132342 0.123228,1.337305 0.123228,1.337305 l 0.799131,-0.279787 c 0,0 -0.260764,-1.576391 0.105819,-1.889849 0.188411,-0.161107 1.304333,-0.139312 1.608697,-0.04893 0.328816,0.09764 0.227247,1.329347 0.227247,1.329347 l 0.357253,-0.136597 0.0066,-2.250633 c 0,0 0.637925,-4.351434 1.653586,-4.348046 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_TypeA340: EmbeddedSvg = {
            svg: `<svg
   xmlns:osb="http://www.openswatchbook.org/uri/2009/osb"
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="60"
   height="60"
   viewBox="0 0 60 60"
   id="svg4148"
   version="1.1">
  <defs
     id="defs4150">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter7068">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="opacity:1">
    <path
       style="opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.80000001;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;fill-opacity:1;filter:url(#filter7068)"
       d="m 31.932528,14.724187 0.176493,10.598575 1.514527,1.092376 0.06738,-1.850163 2.155841,0.0067 c 0,0 0.587052,1.787653 0.04047,3.464441 0.302643,0.21164 3.416828,2.142496 3.416828,2.142496 l 0.0067,-1.914495 2.319403,-6.31e-4 c 0,0 0.402679,2.483944 -0.189313,3.329051 -0.296221,0.422874 4.749289,3.465181 4.749289,3.465181 l 0.378316,0.773135 -0.0077,1.459338 -12.19966,-4.868178 -2.420004,-0.264053 c 0,0 0.209159,6.481416 -0.146426,9.573948 -0.113355,0.985857 -0.452738,2.934025 -0.452738,2.934025 0,0 2.524746,1.429484 4.254817,2.929083 0.603345,0.522971 0.528739,2.250869 0.528739,2.250869 l -5.390081,-1.735692 -0.705755,1.173841 -0.726567,-1.166483 -5.319022,1.552473 c 0,0 -0.213678,-1.282105 0.52138,-1.97998 1.459667,-1.385828 4.367403,-3.042822 4.367403,-3.042822 0,0 -0.433852,-1.447749 -0.543248,-2.999935 -0.222236,-3.153231 -0.150107,-9.481969 -0.150107,-9.481969 l -2.712859,0.371904 -12.028742,4.632187 0.164825,-1.455867 0.593702,-1.091321 c 0,0 4.893565,-2.953794 4.673919,-3.195031 -0.571979,-0.628207 -0.447274,-3.20165 -0.447274,-3.20165 l 2.492739,0.01104 -0.0074,2.121574 c 0,0 4.046367,-2.145936 3.52899,-2.753962 -0.524455,-0.616345 -0.249969,-3.042922 -0.249969,-3.042922 l 2.232474,-0.01776 0.05203,1.995959 1.705367,-1.25289 0.0095,-10.522835 c 0,0 0.511697,-4.159818 1.862519,-4.175214 1.350822,-0.0154 1.883214,4.135657 1.883214,4.135657 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_TypeA380: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="60"
   height="60"
   viewBox="0 0 60 60"
   id="svg4148"
   version="1.1">
  <defs
     id="defs4150">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter7068">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="display:inline;opacity:1"
     transform="translate(0,-10)">
    <path
       id="outline-path"
       style="fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.8;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1;filter:url(#filter7068);stroke-miterlimit:4;stroke-dasharray:none"
       d="m 27.920892,26.803134 -0.103565,4.194379 c -0.601799,1.172792 -1.269164,1.716089 -1.269164,1.716089 0,0 -1.904617,1.666845 -2.128992,1.86299 -0.131093,0.1146 -0.104295,-0.467279 -0.117692,-0.681314 -0.01689,-0.26966 -0.184386,-1.018702 -0.184386,-1.018702 l -2.10279,-0.0052 c 0,0 -0.296395,0.781536 -0.31908,1.034394 -0.05985,0.667137 -0.04082,1.084715 0,1.514322 0.0706,0.577672 0.128384,0.631876 0.309926,1.027855 l -2.943644,2.234871 c 0,0 -0.162562,0.02216 -0.137308,-0.173924 0.04957,-0.384798 0.01262,-0.511832 -0.04316,-0.895778 -0.03608,-0.24833 -0.249773,-0.829085 -0.249773,-0.829085 l -2.096249,0 c 0,0 -0.18315,1.023492 -0.206618,1.251474 -0.05617,0.545703 -0.04167,0.843366 -0.0013,1.216165 0.0368,0.339968 0.360927,1.144241 0.360927,1.144241 l -0.120335,0.08631 -5.628361,4.132347 c -0.3505,0.257338 -0.493143,2.201994 -0.367469,3.026029 0,0 1.843763,-0.738941 2.912352,-1.245382 11.914287,-5.646606 14.228527,-5.763409 14.228527,-5.763409 l 0.116234,1.183433 0.04012,6.684761 0.355518,2.362761 c 0,0 -5.641864,4.77642 -5.989813,5.370487 -0.05263,0.08985 -0.323174,2.449515 -0.0612,2.288706 0.468245,-0.287431 6.70648,-3.420926 7.077021,-3.2146 0.375682,0.209188 0.11,2.016708 0.83507,2.012547 0.72507,-0.0041 0.485279,-1.650646 0.809178,-2.015182 0.281701,-0.317043 6.608777,2.978952 7.077022,3.266383 0.261969,0.160809 0.01732,-2.224748 -0.03531,-2.314597 -0.788099,-0.878871 -5.912139,-5.396378 -5.912139,-5.396378 l 0.226063,-2.285088 0.04532,-6.641101 0.217196,-1.284396 c 0,0 2.345651,0.263267 14.059848,5.667947 1.07375,0.495405 3.098613,1.415676 3.045488,1.17399 -0.178945,-0.814129 0.0085,-2.470492 -0.340816,-2.729423 l -5.806997,-4.30432 0.159292,-0.229249 c 0,0 0.189687,-0.750921 0.226489,-1.090888 0.04036,-0.372799 0.05602,-0.706478 2.66e-4,-1.077286 -0.041,-0.272624 -0.21198,-1.279856 -0.21198,-1.279856 l -2.273316,0.0022 c 0,0 -0.108936,0.854073 -0.145018,1.102402 -0.171437,0.571034 -0.0018,0.928221 -0.0018,0.928221 l -3.078889,-2.383825 c 0.181542,-0.395979 0.218148,-0.496223 0.288746,-1.073894 0.04082,-0.429608 0.08574,-0.880202 0.02589,-1.307574 -0.03521,-0.251419 -0.325743,-1.185411 -0.325743,-1.185411 l -1.955249,0.01282 c 0,0 -0.335143,0.709015 -0.352028,0.978676 -0.0134,0.214035 0.08012,0.20636 -0.02524,0.688024 -0.08063,0.368648 -1.273012,-0.854533 -1.273012,-0.854533 0,0 -1.264972,-0.843315 -2.174863,-2.640906 l -0.05178,-4.19438 c 0.0043,-1.169774 -0.786521,-5.632898 -2.240646,-5.255714 -1.448355,-0.299756 -2.10849,3.890313 -2.140761,5.206551 z"
    />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_TypeGLFx: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="40"
   height="40"
   viewBox="0 0 40 40"
   id="svg2"
   version="1.1">
  <defs
     id="defs4">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter4194">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="opacity:1">
    <path
       style="fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.80000001;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1;filter:url(#filter4194)"
       d="m 19.986416,9.5968723 c 0.88051,-0.00293 1.441399,3.3508827 1.441399,3.3508827 l 0.03715,3.610929 c 0,0 5.840425,3.528556 7.819838,4.667557 0.263633,0.155613 0.302914,1.089991 0.302914,1.089991 l 0.191502,0.763696 c 0,0 0.06333,0.65626 -0.355454,0.961791 -0.254358,0.185569 -0.503584,-0.460654 -0.503584,-0.460654 l -0.44054,-0.09289 -6.007473,-1.506376 c 0,0 0.01608,1.178542 -0.06933,1.766384 -0.08225,0.566113 -0.09637,1.279435 -0.09637,1.279435 l -0.819529,0.0087 -0.324803,0.314296 c 0,0 0.04287,0.994372 0.04352,1.819539 0.829342,0.475132 1.019595,0.732569 2.204605,1.51508 0.65769,0.398013 0.255783,2.119051 0.255783,2.119051 l -3.225886,-0.972916 -0.433793,0.575489 -0.41937,-0.563178 -3.27693,1.004438 c 0,0 -0.2116,-1.258447 0.134794,-2.000738 0.479132,-0.516262 1.623994,-1.040486 2.422583,-1.593166 -0.009,-0.834493 0,-1.892473 0,-1.892473 l -0.333198,-0.29869 -0.795217,-0.0087 c 0,0 -0.0586,-0.89816 -0.115582,-1.344283 -0.127689,-0.999706 -0.09694,-1.677225 -0.09694,-1.677225 l -6.008763,1.473049 -0.35303,0.07698 c 0,0 -0.183058,0.419203 -0.62218,0.335274 -0.419245,-0.114871 -0.247517,-0.846864 -0.247517,-0.846864 L 10.4619,22.440574 c 0.0034,0.0042 -0.0042,-0.78993 0.148906,-1.114922 1.462509,-0.979967 7.960057,-4.780371 7.960057,-4.780371 l 0.04101,-3.575235 c 0,0 0.561263,-3.3704635 1.374543,-3.3731737 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };

        public static Marker_TypeGLID: EmbeddedSvg = {
            svg: `<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:xlink="http://www.w3.org/1999/xlink"
   width="60"
   height="60"
   viewBox="0 0 60 60"
   id="svg4148"
   version="1.1">
  <defs
     id="defs4150">
    <filter
       style="color-interpolation-filters:sRGB"
       id="filter7068">
      <feFlood
         flood-opacity="1"
         flood-color="rgb(0,0,0)"
         result="flood" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite" />
      <feGaussianBlur
         in="composite"
         stdDeviation="1"
         result="blur" />
      <feComposite
         in="SourceGraphic"
         in2="blur"
         operator="over" />
    </filter>
  </defs>
  <g
     id="outline"
     style="opacity:1">
    <path
       style="fill-rule:evenodd;stroke:#000000;stroke-width:0.8;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1;stroke-miterlimit:4;stroke-dasharray:none;fill-opacity:1;filter:url(#filter7068)"
       d="m 29.9372,24.528445 c 1.012944,-0.121264 1.199315,3.414951 1.141624,4.202154 6.874164,0.276046 19.421537,0.647527 19.421537,0.647527 l 0.04289,1.608292 -8.749107,0.450322 -10.957828,0.08577 -0.04289,0.450322 -0.150106,4.996426 2.873481,0.450322 0.02144,1.586848 -3.045032,0.02144 -0.493209,0.922087 -0.493209,-0.900643 -3.130808,-0.150109 0.04289,-1.543959 2.894924,-0.385991 -0.128663,-4.932095 -0.06433,-0.557541 -10.571837,-0.08578 -8.8348822,-0.49321 0,-1.586848 19.1493922,-0.600428 c -0.06122,-1.421153 0.372868,-4.27891 1.073726,-4.184917 z"
       id="outline-path" />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };
    }
}