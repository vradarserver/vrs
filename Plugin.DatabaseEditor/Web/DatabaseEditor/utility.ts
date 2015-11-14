 namespace DatabaseEditor
 {
    export class BootstapUtility
    {
        static formatHorizontalFormInputs(rootElement = $('body'))
        {
            var optionsDataAttr = 'site-form-options';
            var siteFormHelpers = $('[data-' + optionsDataAttr + ']', rootElement);

            // Munge the form fields for horizontal forms
            $.each(siteFormHelpers, function() {
                var parent = $(this);
                var options = BootstapUtility.extractDataObject<ISiteFormOptions>(parent, optionsDataAttr);

                if(options.labelWidth) {
                    $.each($('label, [data-site-form-column="label"]', parent), function() {
                        var labelElement = $(this);
                        if($('input', labelElement).length === 0) {
                            if(BootstapUtility.isInHorizontalForm(labelElement)) {
                                labelElement.addClass(options.labelWidth);
                            }
                        }
                    });
                }

                if(options.fieldWidth) {
                    $.each($('input, textarea, [data-site-form-column="input"]', parent), function() {
                        var inputElement = $(this);
                        if(inputElement.parent('label').length === 0) {
                            if(BootstapUtility.isInHorizontalForm(inputElement, true)) {
                                inputElement.wrap($('<div />').addClass(options.fieldWidth));
                            }
                        }
                    });
                }
            });
        }

        static extractDataObject<T>(element: JQuery, dataAttribute: string) : T
        {
            return <T>element.data(dataAttribute);
        }

        static isInHorizontalForm(element: JQuery, onlyCheckParents = false) : boolean
        {
            var result = onlyCheckParents ? false : element.hasClass('form-horizontal');
            if(!result) result = element.parents('.form-horizontal').length !== 0;

            return result;
        }
    }

    interface ISiteFormOptions
    {
        /**
         * The Bootstrap column width for labels when the fields are horizontal. Does nothing if the fields are not horizontal.
         **/
        labelWidth: string;

        /**
         * The Bootstrap column width for fields when the fields are horizontal. Does nothing if the fields are not horizontal.
         **/
        fieldWidth: string;
    }
 }