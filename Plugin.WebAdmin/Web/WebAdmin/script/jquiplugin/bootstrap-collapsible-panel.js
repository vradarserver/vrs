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
 * @fileoverview A jQuery UI plugin that adds the HTML for a Twitter Bootstrap collapsible panel.
 */

(function(VRS, $, undefined)
{
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * @param {jQuery} jQueryElement
     * @returns {VRS.bootstrapCollapsiblePanel}
     */
    VRS.jQueryUIHelper.getBootstrapCollapsiblePanelPlugin = function(jQueryElement) { return jQueryElement.data('vrsBootstrapCollapsiblePanel'); };

    /**
     * @namespace VRS.bootstrapCollapsiblePanel
     */
    $.widget('vrs.bootstrapCollapsiblePanel', {
        options: {
            startCollapsed: false
        },

        _create: function()
        {
            var panelDiv = this.element;
            var children = panelDiv.children();
            if(children.length !== 2) throw 'The panel div should have exactly two children';

            var headingDiv = $(children[0]);
            var bodyDiv = $(children[1]);

            var headingId = headingDiv.attr('id');
            var bodyId = bodyDiv.attr('id');

            panelDiv.addClass('panel panel-default')
                .attr('role', 'tablist');

            headingDiv.wrapInner(
                $('<h4 />').append(
                    $('<a />')
                        .attr('class', this.options.startCollapsed ? 'collapsed' : '')
                        .attr('data-toggle', 'collapse')
                        .attr('data-target', '#' + bodyId)
                        .attr('href', '#' + bodyId)
                )
            )
                .addClass('panel-heading')
                .attr('role', 'tab')
                .attr('aria-expanded', 'true')
                .attr('aria-controls', '#' + bodyId);

            bodyDiv.wrapInner($('<div />').addClass('panel-body'))
                .addClass('panel-collapse collapse' + (this.options.startCollapsed ? '' : ' in'))
                .attr('role', 'tabpanel')
                .attr('aria-labelledby', '#' + headingId);
        },
    });
}(window.VRS = window.VRS || {}, jQuery));