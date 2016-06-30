var DatabaseEditor;
(function (DatabaseEditor) {
    var BootstapUtility = (function () {
        function BootstapUtility() {
        }
        BootstapUtility.formatHorizontalFormInputs = function (rootElement) {
            if (rootElement === void 0) { rootElement = $('body'); }
            var optionsDataAttr = 'site-form-options';
            var siteFormHelpers = $('[data-' + optionsDataAttr + ']', rootElement);
            // Munge the form fields for horizontal forms
            $.each(siteFormHelpers, function () {
                var parent = $(this);
                var options = BootstapUtility.extractDataObject(parent, optionsDataAttr);
                if (options.labelWidth) {
                    $.each($('label, [data-site-form-column="label"]', parent), function () {
                        var labelElement = $(this);
                        if ($('input', labelElement).length === 0) {
                            if (BootstapUtility.isInHorizontalForm(labelElement)) {
                                labelElement.addClass(options.labelWidth);
                            }
                        }
                    });
                }
                if (options.fieldWidth) {
                    $.each($('input, textarea, [data-site-form-column="input"]', parent), function () {
                        var inputElement = $(this);
                        if (inputElement.parent('label').length === 0) {
                            if (BootstapUtility.isInHorizontalForm(inputElement, true)) {
                                inputElement.wrap($('<div />').addClass(options.fieldWidth));
                            }
                        }
                    });
                }
            });
        };
        BootstapUtility.extractDataObject = function (element, dataAttribute) {
            return element.data(dataAttribute);
        };
        BootstapUtility.isInHorizontalForm = function (element, onlyCheckParents) {
            if (onlyCheckParents === void 0) { onlyCheckParents = false; }
            var result = onlyCheckParents ? false : element.hasClass('form-horizontal');
            if (!result)
                result = element.parents('.form-horizontal').length !== 0;
            return result;
        };
        return BootstapUtility;
    }());
    DatabaseEditor.BootstapUtility = BootstapUtility;
})(DatabaseEditor || (DatabaseEditor = {}));
//# sourceMappingURL=utility.js.map