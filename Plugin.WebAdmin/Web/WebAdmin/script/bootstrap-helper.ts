namespace Bootstrap
{
    export class Helper
    {
        /**
         * Finds tags decorated with data-bsu attributes and uses those to flesh out
         * the HTML that Bootstrap needs for different elements.
         */
        static decorateBootstrapElements()
        {
            Helper.decorateValidationElements();
            Helper.decorateCollapsiblePanels();
            Helper.decorateModals();
        }

        /**
         * Finds tags decorated with various validation tags and fleshes out the
         * Bootstrap and knockout tags required.
         */
        static decorateValidationElements()
        {
            Helper.decorateValidationFieldValidate();
            Helper.decorateValidationIcons();
        }

        /**
         * Adds validation displays to input elements. Apply the tag in a parent of the input,
         * set data-bsu-field to the full path to the ValidationFieldModel element.
         */
        static decorateValidationFieldValidate()
        {
            var fieldValidates = $('[data-bsu="field-validate"]');
            $.each(fieldValidates, function() {
                var fieldValidate = $(this);
                var fieldName = Helper.getFieldName(fieldValidate);

                var fieldValidateBinding = VRS.stringUtility.format(
                    "css: {{ 'has-feedback': !{0}.IsValid(), 'has-warning': {0}.IsWarning, 'has-error': {0}.IsError }}",
                    fieldName
                );
                var visibleIfErrorBinding = VRS.stringUtility.format(
                    'visible: {0}.IsError',
                    fieldName
                );
                var visibleIfWarningBinding = VRS.stringUtility.format(
                    'visible: {0}.IsWarning',
                    fieldName
                );
                var messageBinding = VRS.stringUtility.format(
                    'visible: !{0}.IsValid(), text: {0}.Message', 
                    fieldName
                );

                fieldValidate.attr('data-bind', fieldValidateBinding);

                var helpBlockControl = $('input,textarea,select,:checkbox', fieldValidate);
                var parentInputGroup = helpBlockControl.parent('.input-group');
                if(parentInputGroup.length > 0) {
                    helpBlockControl = parentInputGroup;
                }
                $('<span />')
                    .attr('data-bind', messageBinding)
                    .addClass('help-block')
                    .insertAfter(helpBlockControl);

                var input = $('input', fieldValidate).filter(function() {
                    return $(this).parent('.input-group').length === 0;     // input groups and input feedback icons do not mix
                });
                $('<span />')
                    .attr('data-bind', visibleIfErrorBinding)
                    .addClass('form-control-feedback glyphicon glyphicon-minus-sign')
                    .attr('aria-hidden', 'true')
                    .insertAfter(input);
                $('<span />')
                    .attr('data-bind', visibleIfWarningBinding)
                    .addClass('form-control-feedback glyphicon glyphicon-exclamation-sign')
                    .attr('aria-hidden', 'true')
                    .insertAfter(input);
            });
        }

        /**
         * Adds span elements to show the warning and error states. Usually associated with wrap-up validation elements.
         */ 
        static decorateValidationIcons()
        {
            var validateIcons = $('[data-bsu="validate-icons"]');
            $.each(validateIcons, function() {
                var validateIcon = $(this);
                var fieldName = Helper.getFieldName(validateIcon);

                var visibleIfErrorBinding = VRS.stringUtility.format(
                    'visible: {0}.IsError',
                    fieldName
                );
                var visibleIfWarningBinding = VRS.stringUtility.format(
                    'visible: {0}.IsWarning',
                    fieldName
                );

                $('<span />')
                    .attr('data-bind', visibleIfErrorBinding)
                    .addClass('glyphicon glyphicon-minus-sign')
                    .appendTo(validateIcon);
                $('<span />')
                    .attr('data-bind', visibleIfWarningBinding)
                    .addClass('glyphicon glyphicon-exclamation-sign')
                    .appendTo(validateIcon);
            });
        }

        /**
         * Finds tags decorated with collapsible-panel data-bsu attributes and fleshes
         * out the HTML required for a collapsible panel.
         */
        static decorateCollapsiblePanels()
        {
            var collapsiblePanels = $('[data-bsu="collapsible-panel"]');
            $.each(collapsiblePanels, function() {
                var panel = $(this);
                var panelId = Helper.applyUniqueId(panel);
                var children = panel.children();
                if(children.length !== 2) throw 'The panel should have exactly two children';

                var options = Helper.getOptions(panel);
                var startCollapsed = VRS.arrayHelper.indexOf(options, 'expanded') === -1;
                var usePanelTitle = VRS.arrayHelper.indexOf(options, 'use-title') !== -1;
                var isInAccordion = panel.parent().hasClass('panel-group') || VRS.arrayHelper.indexOf(options, 'accordion') !== -1;
                var accordion = !isInAccordion ? null : panel.closest('.panel-group');
                if(!accordion) {
                    isInAccordion = false;
                }
                var accordionId = isInAccordion ? Helper.applyUniqueId(accordion) : null;

                var heading = $(children[0]);
                var headingId = Helper.applyUniqueId(heading);
                var body = $(children[1]);
                var bodyId = Helper.applyUniqueId(body);

                panel.addClass('panel panel-default');

                var headerLink = $('<a />')
                    .attr('class', startCollapsed ? 'collapsed' : '')
                    .attr('data-toggle', 'collapse')
                    .attr('role', 'button')
                    .attr('href', '#' + bodyId)
                    .attr('aria-expanded', startCollapsed ? 'false' : 'true')
                    .attr('aria-controls', '#' + bodyId);
                if(!isInAccordion) {
                    headerLink.attr('data-target', '#' + bodyId);
                } else {
                    headerLink.attr('data-parent', '#' + accordionId);
                }
                heading.wrapInner(
                    $('<h4 />').addClass(usePanelTitle ? 'panel-title' : '').append(headerLink)
                )
                .addClass('panel-heading')
                .attr('role', 'tab');

                body.wrapInner($('<div />').addClass('panel-body'))
                    .addClass('panel-collapse collapse' + (startCollapsed ? '' : ' in'))
                    .attr('role', 'tabpanel')
                    .attr('aria-labelledby', '#' + headingId);
            });
        }

        /**
         * Finds tags decorated with modal data-bsu attributes and fleshes out the HTML required
         * for a modal.
         */
        static decorateModals()
        {
            var modals = $('[data-bsu="modal"]');
            $.each(modals, function() {
                var modal = $(this);
                var modalId = Helper.applyUniqueId(modal);
                var children = modal.children();
                if(children.length < 2 || children.length > 3) throw 'The modal should have two or three children';

                var options = Helper.getOptions(modal);
                var addHeaderCloseButton = VRS.arrayHelper.indexOf(options, 'header-close') !== -1;
                var largeModal = VRS.arrayHelper.indexOf(options, 'large') !== -1;
                var smallModal = VRS.arrayHelper.indexOf(options, 'small') !== -1;

                var heading = $(children[0]);
                var body = $(children[1]);
                var footer = children.length === 3 ? $(children[2]) : null;

                var headingTitle = $('<h4 />').addClass('modal-title');
                heading.wrapInner(headingTitle).addClass('modal-header');
                var headingId = Helper.applyUniqueId(headingTitle);

                if(addHeaderCloseButton) {
                    heading.prepend($('<button />')
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

                body.addClass('modal-body');

                if(footer != null) {
                    footer.addClass('modal-footer');
                }

                var modalDialogClass = 'modal-dialog';
                if(largeModal)      modalDialogClass += ' modal-lg';
                else if(smallModal) modalDialogClass += ' modal-sm';

                modal.addClass('modal fade')
                    .attr('tabindex', '-1')
                    .attr('role', 'dialog')
                    .attr('aria-labelledby', headingId)
                    .attr('aria-hidden', 'true')
                    .wrapInner($('<div />').addClass('modal-content'))
                    .wrapInner($('<div />').addClass(modalDialogClass));

                modal.detach();
                $('body').append(modal);
            });
        }

        private static getOptions(element: JQuery) : string[]
        {
            var result: string[] = [];

            var options = element.data('bsu-options');
            if(options !== undefined && options !== null) {
                $.each(options.split(' '), function(idx, option) {
                    option = option.trim();
                    if(option.length) {
                        result.push(option);
                    }
                });
            }

            return result;
        }

        private static getFieldName(element: JQuery) : string
        {
            return element.data('bsu-field');
        }

        private static _UniqueId = 0;
        private static applyUniqueId(element: JQuery) : string
        {
            var result = element.attr('id');
            if(!result) {
                result = '_bsu_unique_id_' + ++Helper._UniqueId;
                element.attr('id', result);
            }

            return result;
        }
    }
}