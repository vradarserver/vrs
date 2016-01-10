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
 * @fileoverview A jQuery UI plugin that adds the HTML for a Twitter Bootstrap responsive table.
 */

(function(VRS, $, undefined)
{
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * @param {jQuery} jQueryElement
     * @returns {VRS.bootstrapModal}
     */
    VRS.jQueryUIHelper.getBootstrapResponsiveTablePlugin = function(jQueryElement) { return jQueryElement.data('vrsBootstrapResponsiveTable'); };

    /**
     * @namespace VRS.bootstrapModal
     */
    $.widget('vrs.bootstrapResponsiveTable', {
        options: {
            // Create options
            /** @type {boolean} */                              responsive: true,
            /** @type {boolean} */                              striped: true,
            /** @type {boolean} */                              condensed: true,

            // Optional VRS.WebAdmin.RecordCollection handling
            /** @type {VRS.WebAdmin.RecordCollection} */        records: undefined,
            /** @type {number} */                               idCellIndex: undefined,
            /** @type {VRS_WEBADMIN_TABLE_CELL_PROPERTIES[]} */ cellDefs: undefined,
            /** @type {function(jQuery,object)} */              hookNewRow: undefined,
            /** @type {function(jQuery)} */                     unhookOldRow: undefined
        },

        _create: function()
        {
            var options = this.options;

            if(options.responsive) {
                this.element.wrap($('<div />').addClass('table-responsive'));
            }

            this.element.addClass('table');
            if(options.striped) this.element.addClass('table-striped');
            if(options.condensed) this.element.addClass('table-condensed');
        },

        createRowsForRecords: function(recordCollection)
        {
            var tableBody = $('tbody', this.element);
            var records = recordCollection.getCurrentRecords();
            var cellDefs = this.options.cellDefs || [];
            var idCellIndex = this.options.idCellIndex === undefined ? 0 : this.options.idCellIndex;
            var hookNewRow = this.options.hookNewRow || function() {;};
            var unhookOldRow = this.options.unhookOldRow || function() {;};

            this._addOrOverwriteRowsForRecords(records, cellDefs, tableBody, recordCollection, idCellIndex, hookNewRow, unhookOldRow);
            this._removeUnusedRowsForRecords(records, cellDefs, tableBody, unhookOldRow);
        },

        /**
         * Removes rows from the table that are no longer required, unhooking them as they're removed.
         * @param {object[]}                                records
         * @param {VRS_WEBADMIN_TABLE_CELL_PROPERTIES[]}    cellDefs
         * @param {jQuery}                                  tableBody
         * @param {function(jQuery)}                        unhookOldRow
         */
        _removeUnusedRowsForRecords: function(records, cellDefs, tableBody, unhookOldRow)
        {
            var tableRows = tableBody[0].rows;
            var recordCount = records.length;

            for(var rowCount = tableRows.length;rowCount > recordCount;--rowCount) {
                var row = $(tableRows[rowCount - 1]);

                unhookOldRow(row);
                row.remove();
            }
        },

        /**
         * Adds new rows or overwrites existing rows for each record in the record collection.
         * @param {object[]}                                records
         * @param {VRS_WEBADMIN_TABLE_CELL_PROPERTIES[]}    cellDefs
         * @param {jQuery}                                  tableBody
         * @param {VRS.WebAdmin.RecordCollection}           recordCollection
         * @param {number}                                  idCellIndex
         * @param {function(jQuery, object)}                hookNewRow
         * @param {function(jQuery)}                        unhookOldRow
         */
        _addOrOverwriteRowsForRecords: function(records, cellDefs, tableBody, recordCollection, idCellIndex, hookNewRow, unhookOldRow)
        {
            var tableRows = tableBody[0].rows;
            var length = records.length;
            var countCells = cellDefs.length;
            for(var i = 0; i < length; ++i) {
                var record = records[i];

                var tableRowElement = tableRows.length > i ? tableRows[i] : null;
                var firstApplicationOfRecordToRow = false;
                if(!tableRowElement) {
                    var rowJQ = $('<tr />');
                    for(var c = 0;c < countCells;++c) {
                        rowJQ.append($('<td />'));
                    }
                    tableBody.append(rowJQ);
                    tableRowElement = rowJQ[0];

                    firstApplicationOfRecordToRow = true;
                }
                var tableRowJQ = $(tableRowElement);

                var previousRecord = null;
                if(!firstApplicationOfRecordToRow) {
                    var recordID = recordCollection.getRecordId(record);
                    previousRecord = recordCollection.getPreviousRecordById(recordID);

                    var idCell = $(tableRowElement.cells[idCellIndex]);
                    if(idCell.text() !== String(recordID)) {
                        unhookOldRow(tableRowJQ);
                        firstApplicationOfRecordToRow = true;
                    }
                }

                for(var j = 0; j < countCells; ++j) {
                    var cellDef = cellDefs[j];
                    var hasChanged = firstApplicationOfRecordToRow;
                    if(!hasChanged) {
                        if(previousRecord == null) hasChanged = true;
                        else hasChanged = cellDef.hasChanged(record, previousRecord);
                    }

                    if(hasChanged) {
                        var cell = $(tableRowElement.cells[j]);
                        if(cellDef.getText) {
                            var text = cellDef.getText(record);
                            cell.text(text);
                        } else if(cellDef.getHtml) {
                            var html = cellDef.getHtml(record);
                            cell.html(html);
                        } else if(cellDef.addJQuery) {
                            cellDef.addJQuery(cell, record);
                        } else {
                            throw 'You must supply a method to fill the content of a cell';
                        }

                        if(cellDef.getClasses) {
                            var classes = cellDef.getClasses();
                            cell.attr('class', classes);
                        }

                        if(j === idCellIndex) {
                            cell.removeClass('show').addClass('hide');
                        }
                    }
                }

                if(firstApplicationOfRecordToRow) {
                    hookNewRow(tableRowJQ, record);
                }
            }
        },
    });
}(window.VRS = window.VRS || {}, jQuery));