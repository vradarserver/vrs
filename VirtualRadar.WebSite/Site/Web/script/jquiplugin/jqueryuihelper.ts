/**
 * @license Copyright © 2013 onwards, Andrew Whewell
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
 * @fileoverview A TypeScript declaration of jQueryUIHelper, the object that can translate between JQuery elements and plugin classes.
 */

namespace VRS
{
    /**
     * An object that can help when dealing with JQueryUI objects.
     */
    export interface JQueryUIHelper
    {
        getAircraftDetailPlugin?:  (element: JQuery) => AircraftDetailPlugin;
        getAircraftDetailOptions?: (overrides?: AircraftDetailPlugin_Options) => AircraftDetailPlugin_Options;

        getAircraftInfoWindowPlugin?:  (element: JQuery) => AircraftInfoWindowPlugin;
        getAircraftInfoWindowOptions?: (overrides?: AircraftInfoWindowPlugin_Options) => AircraftInfoWindowPlugin_Options;

        getAircraftLinksPlugin?:  (element: JQuery) => AircraftLinksPlugin;
        getAircraftLinksOptions?: (overrides?: AircraftLinksPlugin_Options) => AircraftLinksPlugin_Options;

        getAircraftListPlugin?:  (element: JQuery) => AircraftListPlugin;
        getAircraftListOptions?: (overrides?: AircraftListPlugin_Options) => AircraftListPlugin_Options;

        getAircraftPositionMapPlugin?:  (element: JQuery) => AircraftPositionMapPlugin;
        getAircraftPositionMapOptions?: (overrides?: AircraftPositionMapPlugin_Options) => AircraftPositionMapPlugin_Options;

        getMapNextPageButtonPlugin?:  (element: JQuery) => MapNextPageButton;
        getMapNextPageButtonOptions?: (overrides?: MapNextPageButton_Options) => MapNextPageButton_Options;

        getMapPlugin?:  (element: JQuery) => IMap;
        getMapOptions?: (overrides?: IMapOptions) => IMapOptions;

        getMenuPlugin?:  (jQueryElement: JQuery) => MenuPlugin;
        getMenuOptions?: (overrides?: MenuPlugin_Options) => MenuPlugin_Options;

        getOptionDialogPlugin?:  (element: JQuery) => OptionDialog;
        getOptionDialogOptions?: (overrides?: OptionDialog_Options) => OptionDialog_Options;

        getOptionFieldButtonPlugin?:  (element: JQuery) => OptionFieldButtonPlugin;
        getOptionFieldButtonOptions?: (overrides?: OptionFieldButtonPlugin_Options) => OptionFieldButtonPlugin_Options;

        getOptionFieldCheckBoxPlugin?:  (element: JQuery) => OptionFieldCheckBoxPlugin;
        getOptionFieldCheckBoxOptions?: (overrides?: OptionFieldCheckBoxPlugin_Options) => OptionFieldCheckBoxPlugin_Options;

        getOptionFieldColourPlugin?:  (element: JQuery) => OptionFieldColourPlugin;
        getOptionFieldColourOptions?: (overrides?: OptionFieldColourPlugin_Options) => OptionFieldColourPlugin_Options;

        getOptionFieldComboBoxPlugin?:  (element: JQuery) => OptionFieldComboBoxPlugin;
        getOptionFieldComboBoxOptions?: (overrides?: OptionFieldComboBoxPlugin_Options) => OptionFieldComboBoxPlugin_Options;

        getOptionFieldDatePlugin?:  (jQueryElement: JQuery) => OptionFieldDatePlugin;
        getOptionFieldDateOptions?: (overrides?: OptionFieldDatePlugin_Options) => OptionFieldDatePlugin_Options;

        getOptionFieldLabelPlugin?:  (element: JQuery) => OptionFieldLabelPlugin;
        getOptionFieldLabelOptions?: (overrides?: OptionFieldLabelPlugin_Options) => OptionFieldLabelPlugin_Options;

        getOptionFieldLinkLabelPlugin?:  (element: JQuery) => OptionFieldLinkLabelPlugin;
        getOptionFieldLinkLabelOptions?: (overrides?: OptionFieldLinkLabelPlugin_Options) => OptionFieldLinkLabelPlugin_Options;

        getOptionFieldNumericPlugin?:  (element: JQuery) => OptionFieldNumericPlugin;
        getOptionFieldNumericOptions?: (overrides?: OptionFieldNumeric_Options) => OptionFieldNumeric_Options;

        getOptionFieldOrderedSubsetPlugin?:  (element: JQuery) => OptionFieldOrderedSubsetPlugin;
        getOptionFieldOrderedSubsetOptions?: (overrides?: OptionFieldOrderedSubsetPlugin_Options) => OptionFieldOrderedSubsetPlugin_Options;

        getOptionFieldPaneListPlugin?:  (element: JQuery) => OptionFieldPaneListPlugin;
        getOptionFieldPaneListOptions?: (overrides?: OptionFieldPaneListPlugin_Options) => OptionFieldPaneListPlugin_Options;

        getOptionFieldRadioButtonPlugin?:  (element: JQuery) => OptionFieldRadioButtonPlugin;
        getOptionFieldRadioButtonOptions?: (overrides?: OptionFieldRadioButtonPlugin_Options) => OptionFieldRadioButtonPlugin_Options;

        getOptionFieldTextBoxPlugin?:  (element: JQuery) => OptionFieldTextBoxPlugin;
        getOptionFieldTextBoxOptions?: (overrides?: OptionFieldTextBoxPlugin_Options) => OptionFieldTextBoxPlugin_Options;

        getOptionFormPlugin?:  (element: JQuery) => OptionForm;
        getOptionFormOptions?: (overrides?: OptionForm_Options) => OptionForm_Options;

        getOptionPanePlugin?:  (element: JQuery) => OptionPanePlugin;
        getOptionPaneOptions?: (overrides?: OptionPanePlugin_Options) => OptionPanePlugin_Options;

        getPagePanelPlugin?:  (element: JQuery) => PagePanel;
        getPagePanelOptions?: (overrides?: PagePanel_Options) => PagePanel_Options;

        getReportDetailPlugin?:  (element: JQuery) => ReportDetailPlugin;
        getReportDetailOptions?: (overrides?: ReportDetailPlugin_Options) => ReportDetailPlugin_Options;

        getReportListPlugin?:  (element: JQuery) => ReportListPlugin;
        getReportListOptions?: (overrides?: ReportListPlugin_Options) => ReportListPlugin_Options;

        getReportMapPlugin?:  (element: JQuery) => ReportMapPlugin;
        getReportMapOptions?: (overrides?: ReportMapPlugin_Options) => ReportMapPlugin_Options;

        getReportPagerPlugin?:  (element: JQuery) => ReportPagerPlugin;
        getReportPagerOptions?: (overrides?: ReportPagerPlugin_Options) => ReportPagerPlugin_Options;

        getSelectDialogPlugin?:  (element: JQuery) => SelectDialog;
        getSelectDialogOptions?: (overrides?: SelectDialog_Options) => SelectDialog_Options;

        getSplitterPlugin?: (element: JQuery) => Splitter;
        getSplitterPanePlugin?: (element: JQuery) => SplitterPane;

        getTimeoutMessageBox?: (element: JQuery) => TimeoutMessageBoxPlugin;
    }
}