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
 * @fileoverview Static collection of embedded SVG paths.
 */

namespace VRS
{
    /**
     * The interface for an embedded SVG.
     *
     * The SVG generator needs to be able to build the SVGs in memory and then manipulate them as quickly as possible, so
     * to do this it needs to lay down some rules about the SVGs that it can handle. The first rule is that an SVG has to
     * be decomposed into an object that complies with this interface.
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

    export class EmbeddedSvgs
    {
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
         flood-opacity="0.501961"
         flood-color="rgb(0,0,0)"
         result="flood"
         id="feFlood4196" />
      <feComposite
         in="flood"
         in2="SourceGraphic"
         operator="in"
         result="composite1"
         id="feComposite4198" />
      <feGaussianBlur
         in="composite1"
         stdDeviation="1"
         result="blur"
         id="feGaussianBlur4200" />
      <feOffset
         dx="-6.38378e-016"
         dy="-6.38378e-016"
         result="offset"
         id="feOffset4202" />
      <feComposite
         in="SourceGraphic"
         in2="offset"
         operator="over"
         result="composite2"
         id="feComposite4204" />
    </filter>
  </defs>
  <g
     id="outline"
     transform="translate(0,9.0000004)"
     style="display:inline;opacity:1">
    <path
       style="fill-opacity:1;fill-rule:evenodd;stroke:#000000;stroke-width:0.7578059;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1;filter:url(#filter4194);stroke-miterlimit:4;stroke-dasharray:none"
       d="m 20.047765,-1.4164221 c -1.077558,0.031588 -1.578202,2.37408345 -1.633331,2.6993142 -0.07206,1.2300474 -0.0472,4.7773739 -0.0472,4.7773739 l -0.673248,0.3459711 -0.01669,-0.3170476 -0.05006,-0.8343336 -1.902283,0 -0.06675,1.6019214 0.166868,0.5506616 c 0,0 -4.742758,2.4104002 -5.9237742,3.0870381 -0.1926247,0.211639 -0.083434,1.835536 -0.083434,1.835536 l 6.4577482,-2.052463 2.102523,4e-6 3e-6,4.889199 0.35042,1.969029 -2.803363,1.735415 0,1.81885 3.287277,-0.967829 0.317047,1.101322 1.051262,4e-6 0.20024,-1.151381 3.354024,0.834334 -0.133493,-1.702042 -2.753304,-1.668669 0.383794,-1.969029 0.03337,-4.922573 1.835536,0 6.975035,2.185956 c 0,0 -0.04908,-1.577846 -0.266986,-1.885596 C 29.177708,9.9695601 24.285221,7.4241329 24.285221,7.4241329 l 0.23361,-0.6674708 -0.05006,-1.4684292 -2.002402,0.01669 c -0.03854,0.6147912 -0.219772,0.4596316 -0.18355,1.0679481 L 21.698797,6.1058836 c -0.0172,-1.6764379 0.07023,-3.1346146 -0.06674,-4.805766 -0.310453,-2.08555889 -1.148455,-2.729316 -1.584292,-2.7165397 z"
       id="outline-path"
       transform="matrix(1.0556793,0,0,1.0556793,-1.0835713,0.79709884)"
    />
  </g>
</svg>`,
            aircraftMarkerStatusFillPaths: [ 'outline-path' ]
        };
    }
}