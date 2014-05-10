var GoogleMapOptionsUIObjects = [];

function googleMapOptionsUISelectTab(idx, tabIndex) { GoogleMapOptionsUIObjects[idx].selectTab(tabIndex); }
function googleMapOptionsToggleUI(idx)              { GoogleMapOptionsUIObjects[idx].toggleDisplay(); }
function googleMapOptionsUIChanged(idx, tabName)    { GoogleMapOptionsUIObjects[idx].changedHandler(tabName); }

function GoogleMapOptionsUI(events, options)
{
    var that = this;
    var mGlobalIndex = GoogleMapOptionsUIObjects.length;
    GoogleMapOptionsUIObjects.push(that);

    var mTabPages = [];
    var mSelectedTab = null;
    var mOptions = options;

    var mEvents = events;
    mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);

    this.createTabPage = function(name, displayOrder, headerText, html, copyToForm, copyFromForm, save)
    {
        mTabPages.push( { 
            name: name,
            displayOrder: displayOrder,
            headerText: headerText,
            html: html,
            copyOptionsToForm: copyToForm,
            copyFormToOptions: copyFromForm,
            save: save
        } );
    };

    function tabPageHeaderId(tabPage) { return 'optionsTabHeader' + tabPage.name; }
    function tabPageBodyId(tabPage)   { return 'optionsTabBody' + tabPage.name; }
    function findTabPage(tabName)
    {
        var result = null;
        for(var i = 0;i < mTabPages.length;++i) {
            if(mTabPages[i].name === tabName) {
                result = mTabPages[i];
                break;
            }
        }

        return result;
    };

    this.addToPage = function()
    {
        mEvents.raise(EventId.optionsUICreateTabPage, this, null);
        mTabPages.sort(function(lhs, rhs) { return lhs.displayOrder - rhs.displayOrder; });

        document.getElementById('options').innerHTML = buildHtml();
        if(mTabPages.length > 0) that.selectTab(0);
    };

    this.toggleDisplay = function()
    {
        var optionsPane = document.getElementById('optionsPane');
        var showPane = optionsPane.style.display !== 'block';
        optionsPane.style.display = showPane ? 'block' : 'none';
        document.getElementById('optionsToggle').innerHTML = (showPane ? 'Hide' : 'Show') + ' options';
    }

    function buildHtml()
    {
        var html = '';

        html += '<div id="optionsSwitch">';

        if(_MapMode !== MapMode.flightSim) {
            if(mOptions.canRunReports()) {
                var forceFrame = getPageParameterValue(null, 'forceFrame');
                forceFrame = forceFrame ? '?forceFrame=' + encodeURIComponent(forceFrame) : '';
                html += '<a href="DateReport.htm' + forceFrame + '"' + target('dateReport') + '>Date Report</a> :: ';
                html += '<a href="RegReport.htm' + forceFrame + '"' + target('regReport') + '>Registration Report</a> :: ';
                html += '<a href="IcaoReport.htm' + forceFrame + '"' + target('icaoReport') + '>ICAO Report</a><br />';
            }
        }

        html += '<a id="optionsToggle" href="#" onClick="googleMapOptionsToggleUI(' + mGlobalIndex + ')">Show options</a></div>';

        html += '<div id="optionsPane">';
        html += '<form name="optionsForm" class="optionsForm">';

        html += '<div id="optionsTabHeaders">';
        html += '<div class="optionsTabHeaderLeft">&nbsp;</div>';
        for(var i = 0;i < mTabPages.length;++i) {
            var tabPage = mTabPages[i];
            html += '<div id="' + tabPageHeaderId(tabPage) + '" class="optionsTabHeader" onclick="googleMapOptionsUISelectTab(' + mGlobalIndex + ',' + i + ')">' + tabPage.headerText + '</div>';
        }
        html += '<div class="optionsTabHeaderBookend">&nbsp;</div>';
        html += '<div class="optionsTabHeaderRight">&nbsp;</div>';
        html += '</div>';

        html += '<div id="optionsTabBodies">';
        html += '<div id="optionsTabHelp"><a href="http://www.virtualradarserver.co.uk/OnlineHelp/DesktopGoogleMapOptions.aspx" target="help">Options help</a></div>';
        for(i = 0;i < mTabPages.length;++i) {
            tabPage = mTabPages[i];
            html += '<div id="' + tabPageBodyId(tabPage) + '" class="optionsTabBody">' + tabPage.html + '</div>';
        }
        html += '</div>';

        html += "</form></div>";

        return html;
    };

    this.getForm = function() { return document.optionsForm; };

    this.htmlEol = function() { return '<br/>'; };
    this.htmlHeading = function(text) { return '<p class="optionsHeading">' + text + '</p>'; }
    this.htmlLabel = function(tabName, forName, text) { return '<label for="' + forName + '">' + text + '</label>'; };
    this.htmlHidden = function(elementName, condition) { return '<' + elementName + (condition ? ' class="optionsHidden"' : '') + '>'; };
    this.htmlTextBox = function(tabName, name, size, upperCase)
    {
        var result = '<input type="text" name="' + name + '" onChange="googleMapOptionsUIChanged(' + mGlobalIndex + ',\'' + tabName + '\')"';
        if(size) result += ' size="' + size + '"';
        if(upperCase) result += ' style="text-transform:uppercase;"';
        result += '>';
        return result;
    };
    this.htmlCheckBox = function(tabName, name, label, clickFunction)
    {
        if(clickFunction === undefined) clickFunction = 'googleMapOptionsUIChanged(' + mGlobalIndex + ',\'' + tabName + '\')';
        var result = '<input type="checkbox" name="' + name + '" onClick="' + clickFunction + '">';
        if(label !== undefined) result += that.htmlLabel(tabName, name, label);
        return result;
    };
    this.htmlRadioButton = function(tabName, groupName, radioValue, label)
    {
        var result = '<input type="radio" name="' + groupName + '" value="' + radioValue + '" onClick="googleMapOptionsUIChanged(' + mGlobalIndex + ',\'' + tabName + '\')">';
        if(label !== undefined) result += label;
        return result;
    };
    this.htmlSelect = function(tabName, name, items, size)
    {
        var result = '<select name="' + name + '" onchange="googleMapOptionsUIChanged(' + mGlobalIndex + ',\'' + tabName + '\')"';
        if(size !== undefined) result += ' size="' + size + '">';
        for(var i = 0;i < items.length;++i) {
            result += '<option value="' + items[i].value + '">' + items[i].text + '</option>';
        }
        result += '</select>';
        return result;
    };
    this.htmlColumns = function(rows)
    {
        var result = '<table class="optionsTable">';
        for(var rowNumber = 0;rowNumber < rows.length;++rowNumber) {
            var columns = rows[rowNumber];
            result += '<tr class="optionsRow">';
            for(var columnNumber = 0;columnNumber < columns.length;++columnNumber) {
                result += '<td class="optionsCell">' + columns[columnNumber] + '</td>';
            }
            result += '</tr>';
        }
        result += '</table>';
        return result;
    }

    this.selectTab = function(tabIndex)
    {
        if(mSelectedTab !== null) setTabSelected(mSelectedTab, false);
        mSelectedTab = mTabPages[tabIndex];
        setTabSelected(mSelectedTab, true);
    };

    function setTabSelected(tabPage, select)
    {
        var header = tabPageHeaderId(tabPage);
        var body = tabPageBodyId(tabPage);
        setClass(document.getElementById(header), select ? "optionsTabHeaderSelected" : "optionsTabHeader");
        setClass(document.getElementById(body), select ? "optionsTabBodySelected" : "optionsTabBody");
    };

    this.copyOptionsToForm = function(tabName)
    {
        var form = that.getForm();
        if(tabName !== undefined) {
            var tabPage = findTabPage(tabName);
            if(tabPage !== null) tabPage.copyOptionsToForm(form, this, tabPage);
        } else {
            for(var i = 0;i < mTabPages.length;++i) {
                tabPage = mTabPages[i];
                tabPage.copyOptionsToForm(form, this, tabPage);
            }
        }
    };

    this.copyFormToOptions = function(tabName)
    {
        var form = that.getForm();
        if(tabName !== undefined) {
            var tabPage = findTabPage(tabName);
            if(tabPage !== null) tabPage.copyFormToOptions(form, this, tabPage);
        } else {
            for(var i = 0;i < mTabPages.length;++i) {
                tabPage = mTabPages[i];
                tabPage.copyFormToOptions(form, this, tabPage);
            }
        }
    };

    this.raiseChanged = function(tabName)
    {
        that.changedHandler(tabName);
    };

    this.changedHandler = function(tabName)
    {
        mEvents.raise(EventId.resetTimeOut, null, null);
        that.copyFormToOptions();
        mEvents.raise(EventId.optionsChanged, this, tabName);
    };

    function optionsChangedHandler(sender, args)
    {
        var tabName = args;
        var saved = false;
        if(tabName !== null && tabName !== undefined) {
            var tabPage = findTabPage(tabName);
            if(tabPage !== null) {
                tabPage.save(this, tabPage);
                saved = true;
            }
        }

        if(!saved) {
            for(var i = 0;i < mTabPages.length;++i) {
                tabPage = mTabPages[i];
                tabPage.save(this, tabPage);
            }
        }
    };
}