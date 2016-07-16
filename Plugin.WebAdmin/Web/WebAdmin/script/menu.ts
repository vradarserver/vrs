namespace VRS.WebAdmin
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin;

    export class Menu
    {
        private _MenuEntries: ViewJson.IJsonMenuEntry[] = [];
        private _MenuItemsList: JQuery;

        static suppressNavbar = false;
        static suppressSidebar = false;
        static suppressSubmenus = false;

        constructor()
        {
            $(document).ready(() => {
                if(!Menu.suppressNavbar) {
                    this.addTopNavbar();
                }
                if(!Menu.suppressSidebar) {
                    this.addNavSidebar();

                    $('[data-toggle=offcanvas]').click(function() {
                        $('.row-offcanvas').toggleClass('active');
                    });

                    this.fetchMenuEntries();
                }
            });
        }

        private fetchMenuEntries()
        {
            $.ajax({
                url: 'ViewMap.json',
                success: (menuEntries: ViewJson.IJsonMenuEntry[]) => {
                    this._MenuEntries = menuEntries;
                    this.populateMenu();
                },
                error: () => {
                    setTimeout(() => this.fetchMenuEntries, 5000);
                }
            });
        }

        /**
         * Adds the HTML for the top navigation bar.
         */
        private addTopNavbar()
        {
            var toggleSidebarButton: JQuery;
            if(Menu.suppressSidebar) {
                toggleSidebarButton = $('<div />');
            } else {
                toggleSidebarButton = $('<button />')
                    .attr('type', 'button')
                    .attr('class', 'navbar-toggle')
                    .attr('data-toggle', 'offcanvas')
                    .attr('data-target', '.sidebar-nav')
                    .attr('aria-label', 'Menu')
                    .append($('<span />').addClass('icon-bar'))
                    .append($('<span />').addClass('icon-bar'))
                    .append($('<span />').addClass('icon-bar'))
                ;
            }

            var html =
                $('<div />')
                .attr('class', 'navbar navbar-default navbar-fixed-top" role="navigation')
                .append($('<div />').attr('class', 'container-fluid')
                    .append($('<div />').attr('class', 'navbar-header')
                        .append(toggleSidebarButton)
                        .append($('<a />')
                            .attr('class', 'navbar-brand')
                            .attr('href', '#')
                            .text(VRS.WebAdmin.$$.WA_Title_WebAdmin)
                        )
                    )
                );

            $('#page-container').prepend(html);
        }

        /**
         * Adds the HTML for the sidebar.
         */
        private addNavSidebar()
        {
            var sidebar = $('<nav />')
                .attr('id', 'sidebar')
                .attr('role', 'navigation')
                .addClass('col-xs-6 col-sm-3 sidebar-offcanvas hidden-print');
            this._MenuItemsList = $('<ul />').attr('data-spy', 'affix').addClass('nav').appendTo(sidebar);

            $('#content > .row').prepend(sidebar);
        }

        /**
         * Adds the menu items to the list.
         */
        private populateMenu()
        {
            var currentPageUrl = location.pathname.substring(location.pathname.lastIndexOf('/') + 1);

            this._MenuItemsList.empty();
            $.each(this._MenuEntries, (idx, page) => {
                var menuName = page.IsPlugin ? VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Title_PluginOptions, page.Name) : page.Name;
                var isCurrentPage = VRS.stringUtility.equals(currentPageUrl, page.HtmlFileName, true);
                var pageElement = $('<li />').addClass(isCurrentPage ? 'active' : '');
                pageElement.append($('<a />').attr('href', page.HtmlFileName).text(menuName));
                this._MenuItemsList.append(pageElement);

                if(isCurrentPage && !Menu.suppressSubmenus) {
                    this.buildSubmenus(pageElement);
                }
            });
        }

        buildSubmenus(submenuParent: JQuery)
        {
            $.each($('[data-site-jump-submenu]'), (idx, targetElement) => {
                var target = $(targetElement);
                this.buildJumpMenuEntry(submenuParent, target);
            });
        }

        private buildJumpMenuEntry(menuParent: JQuery, target: JQuery)
        {
            var menu = $('ul.nav', menuParent).last();
            if(menu.length === 0) {
                menu = $('<ul />')
                    .addClass('nav')
                    .appendTo(menuParent);
            }

            var title = target.attr('data-site-jump-submenu');
            if(!title) {
                title = this.elementText(target);
            }
            if(!title || !title.length) {
                $.each(target.find('*'), (idx, childElement) => {
                    title = this.elementText($(childElement));
                    return !title || !title.length;
                });
            }

            var targetID = target.attr('id');
            if(!targetID) {
                title = 'NO TARGET FOR ' + title;
            }

            var liTag = $('<li />');
            var aTag = $('<a />')
                .attr('href', '#' + targetID)
                .text(title)
                .appendTo(liTag);

            menu.append(liTag);
        }

        elementText(element: JQuery) : string
        {
            var result: string = null;

            if(element.length === 1) {
                var textNodes = element.contents().filter(function() { return this.nodeType === 3; });
                if(textNodes.length > 0) {
                    result = $.trim(textNodes[0].nodeValue);
                }
            }

            return result;
        }
    }

    export var menu = new Menu();
} 