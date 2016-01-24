var Bootstrap;
(function (Bootstrap) {
    var Helper = (function () {
        function Helper() {
        }
        Helper.decorateBootstrapElements = function () {
            Helper.decorateValidationElements();
            Helper.decorateCollapsiblePanels();
            Helper.decorateModals();
        };
        Helper.decorateValidationElements = function () {
            Helper.decorateValidationFieldValidate();
            Helper.decorateValidationIcons();
        };
        Helper.decorateValidationFieldValidate = function () {
            var fieldValidates = $('[data-bsu="field-validate"]');
            $.each(fieldValidates, function () {
                var fieldValidate = $(this);
                var fieldName = Helper.getFieldName(fieldValidate);
                var fieldValidateBinding = VRS.stringUtility.format("css: {{ 'has-feedback': !{0}.IsValid(), 'has-warning': {0}.IsWarning, 'has-error': {0}.IsError }}", fieldName);
                var visibleIfErrorBinding = VRS.stringUtility.format('visible: {0}.IsError', fieldName);
                var visibleIfWarningBinding = VRS.stringUtility.format('visible: {0}.IsWarning', fieldName);
                var messageBinding = VRS.stringUtility.format('visible: !{0}.IsValid(), text: {0}.Message', fieldName);
                fieldValidate.attr('data-bind', fieldValidateBinding);
                var input = $('input,textarea,select,:checkbox', fieldValidate);
                $('<span />')
                    .attr('data-bind', messageBinding)
                    .addClass('help-block')
                    .insertAfter(input);
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
        };
        Helper.decorateValidationIcons = function () {
            var validateIcons = $('[data-bsu="validate-icons"]');
            $.each(validateIcons, function () {
                var validateIcon = $(this);
                var fieldName = Helper.getFieldName(validateIcon);
                var visibleIfErrorBinding = VRS.stringUtility.format('visible: {0}.IsError', fieldName);
                var visibleIfWarningBinding = VRS.stringUtility.format('visible: {0}.IsWarning', fieldName);
                $('<span />')
                    .attr('data-bind', visibleIfErrorBinding)
                    .addClass('glyphicon glyphicon-minus-sign')
                    .appendTo(validateIcon);
                $('<span />')
                    .attr('data-bind', visibleIfWarningBinding)
                    .addClass('glyphicon glyphicon-exclamation-sign')
                    .appendTo(validateIcon);
            });
        };
        Helper.decorateCollapsiblePanels = function () {
            var collapsiblePanels = $('[data-bsu="collapsible-panel"]');
            $.each(collapsiblePanels, function () {
                var panel = $(this);
                var children = panel.children();
                if (children.length !== 2)
                    throw 'The panel should have exactly two children';
                var options = Helper.getOptions(panel);
                var startCollapsed = VRS.arrayHelper.indexOf(options, 'expanded') === -1;
                var heading = $(children[0]);
                var headingId = Helper.applyUniqueId(heading);
                var body = $(children[1]);
                var bodyId = Helper.applyUniqueId(body);
                panel.addClass('panel panel-default').attr('role', 'tablist');
                heading.wrapInner($('<h4 />').append($('<a />')
                    .attr('class', startCollapsed ? 'collapsed' : '')
                    .attr('data-toggle', 'collapse')
                    .attr('data-target', '#' + bodyId)
                    .attr('href', '#' + bodyId)))
                    .addClass('panel-heading')
                    .attr('role', 'tab')
                    .attr('aria-expanded', 'true')
                    .attr('aria-controls', '#' + bodyId);
                body.wrapInner($('<div />').addClass('panel-body'))
                    .addClass('panel-collapse collapse' + (startCollapsed ? '' : ' in'))
                    .attr('role', 'tabpanel')
                    .attr('aria-labelledby', '#' + headingId);
            });
        };
        Helper.decorateModals = function () {
            var modals = $('[data-bsu="modal"]');
            $.each(modals, function () {
                var modal = $(this);
                var modalId = Helper.applyUniqueId(modal);
                var children = modal.children();
                if (children.length < 2 || children.length > 3)
                    throw 'The modal should have two or three children';
                var options = Helper.getOptions(modal);
                var addHeaderCloseButton = VRS.arrayHelper.indexOf(options, 'header-close') !== -1;
                var heading = $(children[0]);
                var body = $(children[1]);
                var footer = children.length === 3 ? $(children[2]) : null;
                var headingTitle = $('<h4 />').addClass('modal-title');
                heading.wrapInner(headingTitle).addClass('modal-header');
                var headingId = Helper.applyUniqueId(headingTitle);
                if (addHeaderCloseButton) {
                    heading.prepend($('<button />')
                        .attr('type', 'button')
                        .attr('data-dismiss', 'modal')
                        .attr('aria-label', 'Close')
                        .addClass('close')
                        .append($('<span />')
                        .attr('aria-hidden', 'true')
                        .html('&times;')));
                }
                body.addClass('modal-body');
                if (footer != null) {
                    footer.addClass('modal-footer');
                }
                modal.addClass('modal fade')
                    .attr('tabindex', '-1')
                    .attr('role', 'dialog')
                    .attr('aria-labelledby', headingId)
                    .attr('aria-hidden', 'true')
                    .wrapInner($('<div />').addClass('modal-content'))
                    .wrapInner($('<div />').addClass('modal-dialog'));
            });
        };
        Helper.getOptions = function (element) {
            var result = [];
            var options = element.data('bsu-options');
            if (options !== undefined && options !== null) {
                $.each(options.split(' '), function (idx, option) {
                    option = option.trim();
                    if (option.length) {
                        result.push(option);
                    }
                });
            }
            return result;
        };
        Helper.getFieldName = function (element) {
            return element.data('bsu-field');
        };
        Helper.applyUniqueId = function (element) {
            var result = element.attr('id');
            if (!result) {
                result = '_bsu_unique_id_' + ++Helper._UniqueId;
                element.attr('id', result);
            }
            return result;
        };
        Helper._UniqueId = 0;
        return Helper;
    })();
    Bootstrap.Helper = Helper;
})(Bootstrap || (Bootstrap = {}));
//# sourceMappingURL=bootstrap-helper.js.map