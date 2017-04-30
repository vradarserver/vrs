var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Menu = (function () {
            function Menu() {
                var _this = this;
                this._MenuEntries = [];
                $(document).ready(function () {
                    if (!Menu.suppressNavbar) {
                        _this.addTopNavbar();
                    }
                    if (!Menu.suppressSidebar) {
                        _this.addNavSidebar();
                        $('[data-toggle=offcanvas]').click(function () {
                            $('.row-offcanvas').toggleClass('active');
                        });
                        _this.fetchMenuEntries();
                    }
                });
            }
            Menu.prototype.fetchMenuEntries = function () {
                var _this = this;
                $.ajax({
                    url: 'ViewMap.json',
                    success: function (menuEntries) {
                        _this._MenuEntries = menuEntries;
                        _this.populateMenu();
                    },
                    error: function () {
                        setTimeout(function () { return _this.fetchMenuEntries; }, 5000);
                    }
                });
            };
            Menu.prototype.addTopNavbar = function () {
                var toggleSidebarButton;
                if (Menu.suppressSidebar) {
                    toggleSidebarButton = $('<div />');
                }
                else {
                    toggleSidebarButton = $('<button />')
                        .attr('type', 'button')
                        .attr('class', 'navbar-toggle')
                        .attr('data-toggle', 'offcanvas')
                        .attr('data-target', '.sidebar-nav')
                        .attr('aria-label', 'Menu')
                        .append($('<span />').addClass('icon-bar'))
                        .append($('<span />').addClass('icon-bar'))
                        .append($('<span />').addClass('icon-bar'));
                }
                var html = $('<div />')
                    .attr('class', 'navbar navbar-default navbar-fixed-top" role="navigation')
                    .append($('<div />').attr('class', 'container-fluid')
                    .append($('<div />').attr('class', 'navbar-header')
                    .append(toggleSidebarButton)
                    .append($('<a />')
                    .attr('class', 'navbar-brand')
                    .attr('href', '#')
                    .text(VRS.WebAdmin.$$.WA_Title_WebAdmin))));
                $('#page-container').prepend(html);
            };
            Menu.prototype.addNavSidebar = function () {
                var sidebar = $('<nav />')
                    .attr('id', 'sidebar')
                    .attr('role', 'navigation')
                    .addClass('col-xs-6 col-sm-3 sidebar-offcanvas hidden-print');
                this._MenuItemsList = $('<ul />').attr('data-spy', 'affix').addClass('nav').appendTo(sidebar);
                $('#content > .row').prepend(sidebar);
            };
            Menu.prototype.populateMenu = function () {
                var _this = this;
                var currentPageUrl = location.pathname.substring(location.pathname.lastIndexOf('/') + 1);
                this._MenuItemsList.empty();
                $.each(this._MenuEntries, function (idx, page) {
                    var menuName = page.IsPlugin ? VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Title_PluginOptions, page.Name) : page.Name;
                    var isCurrentPage = VRS.stringUtility.equals(currentPageUrl, page.HtmlFileName, true);
                    var pageElement = $('<li />').addClass(isCurrentPage ? 'active' : '');
                    var linkElement = $('<a />').text(menuName);
                    if (!isCurrentPage) {
                        linkElement.attr('href', page.HtmlFileName);
                    }
                    pageElement.append(linkElement);
                    _this._MenuItemsList.append(pageElement);
                    if (isCurrentPage && !Menu.suppressSubmenus) {
                        _this.buildSubmenus(pageElement);
                    }
                });
            };
            Menu.prototype.buildSubmenus = function (submenuParent) {
                var _this = this;
                $.each($('[data-site-jump-submenu]'), function (idx, targetElement) {
                    var target = $(targetElement);
                    _this.buildJumpMenuEntry(submenuParent, target);
                });
            };
            Menu.prototype.buildJumpMenuEntry = function (menuParent, target) {
                var _this = this;
                var menu = $('ul.nav', menuParent).last();
                if (menu.length === 0) {
                    menu = $('<ul />')
                        .addClass('nav')
                        .appendTo(menuParent);
                }
                var title = target.attr('data-site-jump-submenu');
                if (!title) {
                    title = this.elementText(target);
                }
                if (!title || !title.length) {
                    $.each(target.find('*'), function (idx, childElement) {
                        title = _this.elementText($(childElement));
                        return !title || !title.length;
                    });
                }
                var targetID = target.attr('id');
                if (!targetID) {
                    title = 'NO TARGET FOR ' + title;
                }
                var liTag = $('<li />');
                var aTag = $('<a />')
                    .attr('href', '#' + targetID)
                    .text(title)
                    .appendTo(liTag);
                menu.append(liTag);
            };
            Menu.prototype.elementText = function (element) {
                var result = null;
                if (element.length === 1) {
                    var textNodes = element.contents().filter(function () { return this.nodeType === 3; });
                    if (textNodes.length > 0) {
                        result = $.trim(textNodes[0].nodeValue);
                    }
                }
                return result;
            };
            return Menu;
        }());
        Menu.suppressNavbar = false;
        Menu.suppressSidebar = false;
        Menu.suppressSubmenus = false;
        WebAdmin.Menu = Menu;
        WebAdmin.menu = new Menu();
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=menu.js.map