/**
 * @license Copyright © 2014 onwards, Andrew Whewell
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
 * @fileoverview The object that handles the site navigation elements.
 */

(function(VRS, $, undefined)
{
    VRS.WebAdmin = VRS.WebAdmin || {};

    /**
     * The class that manages the creation and injection of the site navigation elements
     * into the DOM for a page.
     * @constructor
     */
    VRS.WebAdmin.SiteNavigation = function()
    {
        var that = this;

        /**
         * The pages that the site navigation knows about.
         * @type    {VRS_WEBADMIN_SITENAVIGATION_PAGE}[]
         * @private
         */
        var _Pages = [
            { pageUrl: 'Index.html', menuTitle: VRS.$$.WA_Title_Main },
            { pageUrl: 'About.html', menuTitle: VRS.$$.WA_Title_About }
        ];

        /**
         * Adds the HTML for the site to the current page's DOM at the element specified.
         * @param {jQuery}  menuElement
         * @param {string}  currentPageUrl
         */
        this.injectIntoPage = function(menuElement, currentPageUrl)
        {
            menuElement.attr('data-role', 'panel');
            menuElement.attr('data-position', 'left');

            var controlGroup = $('<div />')
                .attr('data-role', 'controlgroup')
                .appendTo(menuElement);

            $.each(_Pages, function(/** number */idx, /** VRS_WEBADMIN_SITENAVIGATION_PAGE */ page) {
                var pageElement = $('<a />')
                    .attr('href', page.pageUrl)
                    .attr('data-role', 'button')
                    .text(page.menuTitle)
                    .appendTo(controlGroup);
            });
        }
    };
}(window.VRS = window.VRS || {}, jQuery));