function ReportPageControl(cookieSuffix)
{
    var that = this;
    var mCookieSuffix = cookieSuffix;
    var mTotalRows = 0;
    var mFirstRow = 0;
    var mRowsPerPage = 25;
    var mMaxElements = 5;
    var mOptionsPrefix = 'pageControlOptions';
    var mOptionsFormPrefix = 'pageControlOptionsForm';
    var mOptionsShowHidePrefix = 'pageControlToggleOptions';
    var mShowAllRows = false;

    loadCookies();

    this.getTotalRows = function() { return mTotalRows; };
    this.setTotalRows = function(value) { mTotalRows = value; };
    this.getFirstRow = function() { return mFirst; };
    this.setFirstRow = function(value) { mFirstRow = value; };
    this.getRowsPerPage = function() { return mRowsPerPage; };
    this.setRowsPerPage = function(value) { mRowsPerPage = value; };

    function loadCookies()
    {
        var cookies = readCookieValues();

        // Get the obsolete cookie out first, just in case
        var rowsPerPage = extractCookieValue(cookies, 'repRowsPerPage' + mCookieSuffix);

        // Get the real cookie out
        var nameValues = new nameValueCollection();
        nameValues.fromString(extractCookieValue(cookies, 'reportPageControl'));
        if(nameValues.getLength() > 0) rowsPerPage = nameValues.getValue('rows' + mCookieSuffix);

        if(rowsPerPage !== null) mRowsPerPage = Number(rowsPerPage);
    };

    function saveCookies()
    {
        // remove obsolete cookies
        eraseCookie('repRowsPerPage' + mCookieSuffix);

        var cookieValues = readCookieValues();

        var nameValues = new nameValueCollection();
        nameValues.fromString(extractCookieValue(cookieValues, 'reportPageControl'));
        nameValues.setValue('rows' + mCookieSuffix, mRowsPerPage);

        writeCookie('reportPageControl', nameValues.toString());
    };

    this.addUI = function()
    {
        that.refreshUI();
    };

    function getCurrentPage()                           { return Math.floor(mFirstRow / mRowsPerPage); };
    function getTotalPages()                            { return Math.ceil(mTotalRows / mRowsPerPage); };
    function isFirstPageEnabled(currentPage)            { return currentPage > 0; };
    function isPreviousPageEnabled(currentPage)         { return isFirstPageEnabled(currentPage); };
    function isNextPageEnabled(currentPage, totalPages) { return currentPage + 1 < totalPages; };
    function isLastPageEnabled(currentPage, totalPages) { return isNextPageEnabled(currentPage, totalPages); };

    this.refreshUI = function()
    {
        var bigSeparator = ' :: ';
        var littleSeparator = ' ';

        var currentPage = getCurrentPage();
        var totalPages = getTotalPages();

        for(var elementNum = 1;elementNum <= mMaxElements;++elementNum) {
            var element = document.getElementById('repPageControl' + elementNum.toString());
            if(element === undefined || element === null) break;

            var html = '';
            if(!mShowAllRows && mTotalRows > 0) {
                html += '<div class="pageControl">';
                html += '<div class="pageControlInner">';

                // Add control to show and hide the extra options
                var toggleId = mOptionsShowHidePrefix + elementNum.toString();
                html += '<div class="pageControlShowHide"><a ref="#" onclick="togglePageControlOptions()"><img id="' + toggleId + '" src="Images/OpenSlider.png" width="16px" height="16px" /></a></div>';

                // Add first and previous links
                html += addLinkOrText('goFirstPage', '', 'First', isFirstPageEnabled(currentPage));
                html += bigSeparator;
                html += addLinkOrText('goPreviousPage', '', 'Prev', isPreviousPageEnabled(currentPage));
                html += bigSeparator;

                // Work out a range of pages around the current one and add links for them
                var firstPage = Math.max(0, currentPage - 3);
                var lastPage = Math.min(totalPages, firstPage + 7);
                for(var pageNum = firstPage;pageNum < lastPage;++pageNum) {
                    if(pageNum !== firstPage) html += littleSeparator;
                    html += addLinkOrText('goPage', pageNum.toString(), (pageNum + 1).toString(), pageNum !== currentPage);
                }

                // Add next and last links
                html += bigSeparator;
                html += addLinkOrText('goNextPage', '', 'Next', isNextPageEnabled(currentPage, totalPages));
                html += bigSeparator;
                html += addLinkOrText('goLastPage', '', 'Last', isLastPageEnabled(currentPage, totalPages));
                html += '</div>'; // pageControlInner

                // Add the options bit
                var optionsName = mOptionsPrefix + elementNum.toString();
                var formName = mOptionsFormPrefix + elementNum.toString();
                html += '<div class="pageControlOptions" id="' + optionsName + '">';
                html += '<form name="' + formName + '" id="' + formName + '" onsubmit="return jumpToPage()" >';
                html += '<table>';

                html += '<tr><td><label for="rowsPerPageInput">Rows per page:</label></td>';
                html += '<td><input name="rowsPerPageInput" type="text" size="3" value="' + mRowsPerPage + '" onchange="saveRowsPerPage(' + elementNum.toString() + ')" />';
                html += '</td></tr>'

                html += '<td></td><td><a href="#" onclick="showAllRows()">Show all</a></td></tr>';

                html += '<tr><td><label for="jumpToPageInput">Jump to page:</label></td>';
                html += '<td><input name="jumpToPageInput" type="text" size="4" />';
                html += '&nbsp;<input type="submit" name="doJump" value="Go" /></td></tr>';

                html += '</table></form>';
                html += '</div>'; // pageControlOptions

                html += '</div>'; // pageControl
            }

            element.innerHTML = html;
        }
    };

    function addLinkOrText(linkFunctionName, linkFunctionParams, linkDescription, enabled)
    {
        var result = '';

        if(!enabled) result += linkDescription;
        else result += '<a href="#" onclick="' + linkFunctionName + '(' + linkFunctionParams + ')">' + linkDescription + '</a>';

        return result;
    };

    this.togglePageControlOptions = function()
    {
        for(var elementNum = 1;elementNum <= mMaxElements;++elementNum) {
            var divId = mOptionsPrefix + elementNum.toString();
            var element = document.getElementById(divId);
            if(element === undefined || element === null) break;
            var showElement = element.style.display != 'block'
            element.style.display = showElement ? 'block' : 'none';

            var toggleId = mOptionsShowHidePrefix + elementNum.toString();
            var toggleImg = document.getElementById(toggleId);
            if(showElement) toggleImg.setAttribute('src', 'Images/CloseSlider.png');
            else            toggleImg.setAttribute('src', 'Images/OpenSlider.png');
        }
    };

    this.showAllRows = function()       { mShowAllRows = true; goPage(0); }
    this.goFirstPage = function()       { that.goPage(0); };
    this.goLastPage = function()        { that.goPage(getTotalPages() - 1); };
    this.goNextPage = function()        { that.goPage(getCurrentPage() + 1); };
    this.goPreviousPage = function()    { that.goPage(getCurrentPage() - 1); };

    this.goPage = function(pageNumber)
    {
        pageNumber = Math.min(pageNumber, getTotalPages() - 1);
        pageNumber = Math.max(0, pageNumber);
        mFirstRow = pageNumber * mRowsPerPage;
        raiseRunReport();
    };

    this.jumpToPage = function()
    {
        var currentPage = getCurrentPage();

        for(var elementNum = 1;elementNum < mMaxElements;++elementNum) {
            var element = document.getElementById('repPageControl' + elementNum.toString());
            if(element === undefined || element === null) break;

            var formName = mOptionsFormPrefix + elementNum.toString();
            var form = document.getElementById(formName);
            var pageNumberText = form.jumpToPageInput.value;
            if(pageNumberText.length > 0) {
                var pageNumber = parseInt(pageNumberText, 10);
                if(isNaN(pageNumber)) pageNumber = 1;
                pageNumber -= 1;
                pageNumber = Math.min(pageNumber, getTotalPages() - 1);
                pageNumber = Math.max(0, pageNumber);

                that.goPage(pageNumber);
            }
        }

        return false;
    };

    this.saveRowsPerPage = function(elementNum)
    {
        var element = document.getElementById('repPageControl' + elementNum.toString());

        var formName = mOptionsFormPrefix + elementNum.toString();
        var form = document.getElementById(formName);
        var rowsPerPageText = form.rowsPerPageInput.value;
        if(rowsPerPageText.length > 0) {
            var rowsPerPage = parseInt(rowsPerPageText, 10);
            if(isNaN(rowsPerPage)) rowsPerPage = 1;
            if(rowsPerPage < 1) rowsPerPage = 1;
            mRowsPerPage = rowsPerPage;
            saveCookies();
            that.goPage(getCurrentPage());
        }
    };

    function raiseRunReport()
    {
        _Events.raise(EventId.runReport, that, null);
    };

    this.addToCriteria = function()
    {
        var result = '';
        result += '&fromRow=' + (mShowAllRows ? -1 : mFirstRow).toString();
        result += '&toRow=' + (mShowAllRows ? -1 : (mFirstRow + (mRowsPerPage - 1))).toString();

        return result;
    };
}