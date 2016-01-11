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
            Helper.decorateCollapsiblePanels();
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
                var children = panel.children();
                if(children.length !== 2) throw 'The panel should have exactly two children';

                var options = Helper.getOptions(panel);
                var startCollapsed = VRS.arrayHelper.indexOf(options, 'expanded') === -1;

                var heading = $(children[0]);
                var headingId = Helper.applyUniqueId(heading);
                var body = $(children[1]);
                var bodyId = Helper.applyUniqueId(body);

                panel.addClass('panel panel-default').attr('role', 'tablist');

                heading.wrapInner(
                    $('<h4 />').append(
                        $('<a />')
                        .attr('class', startCollapsed ? 'collapsed' : '')
                        .attr('data-toggle', 'collapse')
                        .attr('data-target', '#' + bodyId)
                        .attr('href', '#' + bodyId)
                    )
                )
                .addClass('panel-heading')
                .attr('role', 'tab')
                .attr('aria-expanded', 'true')
                .attr('aria-controls', '#' + bodyId);

                body.wrapInner($('<div />').addClass('panel-body'))
                    .addClass('panel-collapse collapse' + (startCollapsed ? '' : ' in'))
                    .attr('role', 'tabpanel')
                    .attr('aria-labelledby', '#' + headingId);
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