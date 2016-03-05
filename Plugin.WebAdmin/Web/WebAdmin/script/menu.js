var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Menu = (function () {
            function Menu() {
                var _this = this;
                this._MenuEntries = [];
                $(document).ready(function () {
                    _this.addTopNavbar();
                    _this.addNavSidebar();
                    $('[data-toggle=offcanvas]').click(function () {
                        $('.row-offcanvas').toggleClass('active');
                    });
                    _this.fetchMenuEntries();
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
                var html = $('<div />')
                    .attr('class', 'navbar navbar-default navbar-fixed-top" role="navigation')
                    .append($('<div />').attr('class', 'container-fluid')
                    .append($('<div />').attr('class', 'navbar-header')
                    .append($('<button />')
                    .attr('type', 'button')
                    .attr('class', 'navbar-toggle')
                    .attr('data-toggle', 'offcanvas')
                    .attr('data-target', '.sidebar-nav')
                    .attr('aria-label', 'Menu')
                    .append($('<span />').addClass('icon-bar'))
                    .append($('<span />').addClass('icon-bar'))
                    .append($('<span />').addClass('icon-bar')))
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
                this._MenuItemsList = $('<ul />').addClass('nav').appendTo(sidebar);
                $('#content > .row').prepend(sidebar);
            };
            Menu.prototype.populateMenu = function () {
                var _this = this;
                var currentPageUrl = location.pathname.substring(location.pathname.lastIndexOf('/') + 1);
                this._MenuItemsList.empty();
                $.each(this._MenuEntries, function (idx, page) {
                    var isCurrentPage = VRS.stringUtility.equals(currentPageUrl, page.HtmlFileName, true);
                    var pageElement = $('<li />');
                    if (isCurrentPage) {
                        pageElement.text(page.Name).addClass('active');
                    }
                    else {
                        pageElement.append($('<a />').attr('href', page.HtmlFileName).text(page.Name));
                    }
                    _this._MenuItemsList.append(pageElement);
                });
            };
            return Menu;
        })();
        WebAdmin.Menu = Menu;
        WebAdmin.menu = new Menu();
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=menu.js.map