function ReportCriteria()
{
    var that = this;
    this.registerSubclass = function(value) { that = value; return value; }

    var mFromDate = null;
    var mToDate = null;
    var mCallsign = null;
    var mIsEmergency = null;

    var mSortField1 = 'date';
    var mSortField1Direction = 'desc';
    var mSortField2 = '';
    var mSortField2Direction = 'asc';

    this.getFromDate = function() { return mFromDate; };
    this.setFromDate = function(value) { mFromDate = value !== '' ? value : null; };
    this.getToDate = function() { return mToDate; };
    this.setToDate = function(value) { mToDate = value !== '' ? value : null; }
    this.getCallsign = function() { return mCallsign; };
    this.setCallsign = function(value) { mCallsign = value !== '' ? value : null; }
    this.getIsEmergency = function() { return mIsEmergency; };
    this.setIsEmergency = function(value) { mIsEmergency = value; };

    this.getSortField1 = function() { return mSortField1; };
    this.setSortField1 = function(value) { mSortField1 = value; }
    this.getSortField1Direction = function() { return mSortField1Direction; };
    this.setSortField1Direction = function(value) { mSortField1Direction = value; }
    this.getSortField2 = function() { return mSortField2; };
    this.setSortField2 = function(value) { mSortField2 = value; }
    this.getSortField2Direction = function() { return mSortField2Direction; };
    this.setSortField2Direction = function(value) { mSortField2Direction = value; }

    this.copyFromUI = function()
    {
        var form = document.criteriaForm;

        that.subclassCopyFromUI(form);
        that.setFromDate(trim(form.critFromDate.value));
        that.setToDate(trim(form.critToDate.value));
        that.setCallsign(trim(form.critCallsign.value));
        that.setIsEmergency(form.critIsEmergency.checked);
        that.setSortField1(form.sortField1.value);
        that.setSortField1Direction(form.sortField1Direction.value);
        that.setSortField2(form.sortField2.value);
        that.setSortField2Direction(form.sortField2Direction.value);
    };

    this.copyToUI = function()
    {
        var form = document.criteriaForm;

        that.subclassCopyToUI(form);
        form.critFromDate.value = mFromDate === null ? '' : mFromDate;
        form.critToDate.value = mToDate === null ? '' : mToDate;
        form.critCallsign.value = mCallsign === null ? '' : mCallsign;
        form.critIsEmergency.checked = mIsEmergency == true;
        form.sortField1.value = mSortField1;
        form.sortField1Direction.value = mSortField1Direction;
        form.sortField2.value = mSortField2;
        form.sortField2Direction.value = mSortField2Direction;
    };

    this.isValid = function()
    {
        return that.subclassIsValid();
    };

    this.isMultiAircraftReport = function()
    {
        var result = false;
        if(that.subclassIsMultiAircraftReport !== undefined) result = that.subclassIsMultiAircraftReport();

        return result;
    };

    this.toString = function()
    {
        var result = that.subclassToString();
        if(mFromDate) result += '&date-L=' + encodeURIComponent(mFromDate);
        if(mToDate) result += '&date-U=' + encodeURIComponent(mToDate);
        if(mCallsign) result += '&call-Q=' + encodeURIComponent(mCallsign);
        if(mIsEmergency) result += '&emg-Q=1';
        result += '&sort1=' + mSortField1;
        result += '&sort1dir=' + mSortField1Direction;
        result += '&sort2=' + mSortField2;
        result += '&sort2dir=' + mSortField2Direction;
        result += '&tzoffset=' + encodeURIComponent(new Date().getTimezoneOffset().toString());

        return result;
    };

    this.setRowCount = function(value)
    {
        var html = '';
        if(value !== null) {
            html = value.toString() + ' row';
            if(value !== 1) html += 's';
            html += ' match';
        }
        document.getElementById('criteriaRowCount').innerHTML = html;
    };

    this.addUI = function()
    {
        var html = "";

        html += '<table class="toggleCriteria">';
        html += '<tr>';
        html += '<td class="rowCount" id="criteriaRowCount"></td>';
        html += '<td class="showHideCriteria"><a href="#" id="toggleCriteria" onclick="showHideCriteria()" >Show criteria</a></td>';
        html += '</tr></table>';
        html += '<div id="criteriaPanel">';

        html += '<form name="criteriaForm" id="criteriaForm" onsubmit="return runReportClicked()">';
        html += '<table>';

        html += that.subclassAddUI();

        html += that.addUITextField('critFromDate', 'From date', 10, false);
        html += that.addUITextField('critToDate', 'To date', 10, false);
        html += that.addUITextField('critCallsign', 'Callsign', 10, true);
        html += that.addUICheckBoxField('critIsEmergency', 'Had emergency');

        html += addSortFields('1', 'Sort by:');
        html += addSortFields('2', 'then by:');

        html += '<tr><td></td><td>';
        html += '<input type="submit" name="critRunReport" value="Run Report" />';
        html += '</td></tr>';
        html += '</table>';
        html += '</form>';

        html += '</div>'; // criteriaPanel

        document.getElementById('criteria').innerHTML = html;
    };

    this.addUITextField = function(fieldName, description, length, upperCase)
    {
        var html = ''
        html += '<tr><td>';
        html += '<label for="' + fieldName + '">' + description + ':</label>';
        html += '</td><td>';
        html += '<input name="' + fieldName + '" type="text" size="' + length + '" ';
        if(upperCase) html += 'class="critUppercase" ';
        html += '/>';
        html += '</td></tr>';

        return html;
    };

    this.addUICheckBoxField = function(fieldName, description)
    {
        var html = '';
        html += '<tr><td>';
        html += '<label for="' + fieldName + '">' + description + ':</label>';
        html += '</td><td>';
        html += '<input name="' + fieldName + '" type="checkbox" />';
        html += '</td></tr>';

        return html;
    };

    this.addUIComboBoxField = function(fieldName, description, size, items)
    {
        var html = '';
        html += '<tr><td>';
        html += '<label for="' + fieldName + '">' + description + ':</label>';
        html += '</td><td>';
        html += '<select name="' + fieldName + '"';
        if(size !== undefined) html += ' size="' + size + '"';
        html += '>';
        for(var i = 0;i < items.length;++i) {
            html += '<option value="' + items[i].value + '">' + items[i].text + '</option>';
        }
        html += '</select>';
        html += '</td></tr>';

        return html;
    };

    function addSortFields(fieldSuffix, labelText)
    {
        var result = '';

        result += '<tr><td>';
        result += '<label for="sortField' + fieldSuffix + '">' + labelText + '</label>';
        result += '</td><td>';
        result += '<select name="sortField' + fieldSuffix + '" size="1">';
        result += '  <option value="">None</option>';
        if(that.isMultiAircraftReport()) {
            result += '    <option value="model">Aircraft Model</option>';
            result += '    <option value="type">Aircraft Type</option>';
        }
        result += '  <option value="date">Date</option>';
        result += '  <option value="callsign">Callsign</option>';
        if(that.isMultiAircraftReport()) {
            result += '    <option value="icao">ICAO</option>';
            result += '    <option value="country">Mode-S Country</option>';
            result += '    <option value="operator">Operator</option>';
            result += '    <option value="reg">Registration</option>';
        }
        result += '</select>';
        result += '&nbsp;';
        result += '<select name="sortField' + fieldSuffix + 'Direction" size="1">';
        result += '  <option value="asc">Ascending</option>';
        result += '  <option value="desc">Descending</option>';
        result += '</select>';
        result += '</td></tr>';

        return result;
    };

    this.showHideCriteria = function()
    {
        var toggleText = document.getElementById('toggleCriteria');
        var panel = document.getElementById('criteriaPanel');

        var showPanel = panel.style.display != 'block';
        toggleText.innerHTML = showPanel ? 'Hide criteria' : 'Show criteria';
        panel.style.display = showPanel ? 'block' : 'none';
    };
}