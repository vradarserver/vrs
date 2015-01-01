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
 * @fileoverview A jQuery UI plugin that adds the HTML for a Twitter Bootstrap modal.
 */

(function(VRS, $, undefined)
{
    $.widget('vrs.bootstrapModal', {
        options: {
            headerCloseButton: true,
            addFooter: true,
            footerCloseButton: true,
            footerCloseText: 'Close',
            bootstrapOptions: undefined
        },

        _create: function()
        {
            var options = this.options;

            var modalDiv = this.element;
            var modalDivChildren = modalDiv.children();
            if(modalDivChildren.length < 2 || modalDivChildren.length > 3) throw 'Modals must have either two or three children';

            var headerDiv = $(modalDivChildren[0]);
            var bodyDiv = $(modalDivChildren[1]);
            var footerDiv = modalDivChildren.length === 3 ? $(modalDivChildren[2]) : null;

            var modalId = modalDiv.attr('id');

            var heading = $('<h4 />')
                .uniqueId()
                .addClass('modal-title');
            headerDiv.wrapInner(heading)
                .addClass('modal-header');
            var headingId = heading.attr('id');
            if(options.headerCloseButton) {
                headerDiv.prepend($('<button />')
                    .attr('type', 'button')
                    .attr('data-dismiss', 'modal')
                    .attr('aria-label', 'Close')
                    .addClass('close')
                    .append($('<span />')
                        .attr('aria-hidden', 'true')
                        .html('&times;')
                    )
                );
            }

            bodyDiv.addClass('modal-body');

            if(footerDiv !== null || options.addFooter) {
                if(footerDiv === null) {
                    footerDiv = $('<div />').appendTo(modalDiv);
                }
                footerDiv.addClass('modal-footer');
                if(options.footerCloseButton) {
                    footerDiv.append($('<button />')
                        .attr('type', 'button')
                        .attr('data-dismiss', 'modal')
                        .addClass('btn btn-default')
                        .text(options.footerCloseText)
                    );
                }
            }

            modalDiv.addClass('modal fade')
                .attr('tabindex', '-1')
                .attr('role', 'dialog')
                .attr('aria-labelledby', headingId)
                .attr('aria-hidden', 'true')
                .wrapInner($('<div />').addClass('modal-content'))
                .wrapInner($('<div />').addClass('modal-dialog'));
        },
    });
}(window.VRS = window.VRS || {}, jQuery));