function DateReportCriteria()
{
    var that = this.registerSubclass(this);

    var mOperator = null;
    var mRegistration = null;
    var mIcao = null;
    var mCountry = null;
    var mWtc = null;
    var mSpecies = null;
    var mIsMilitary = false;

    this.getOperator = function() { return mOperator; };
    this.setOperator = function(value) { mOperator = value; };
    this.getRegistration = function() { return mRegistration; };
    this.setRegistration = function(value) { mRegistration = value; if(mRegistration !== null) mRegistration = mRegistration.toUpperCase(); };
    this.getIcao = function() { return mIcao; };
    this.setIcao = function(value) { mIcao = value; if(mIcao !== null) mIcao = mIcao.toUpperCase(); };
    this.getCountry = function() { return mCountry; };
    this.setCountry = function(value) { mCountry = value; };
    this.getWakeTurbulenceCategory = function() { return mWtc; };
    this.setWakeTurbulenceCategory = function(value) { mWtc = value; };
    this.getSpecies = function() { return mSpecies; };
    this.setSpecies = function(value) { mSpecies = value; };
    this.getIsMilitary = function() { return mIsMilitary; };
    this.setIsMilitary = function(value) { mIsMilitary = value; };

    this.subclassIsMultiAircraftReport = function() { return true; };

    this.subclassCopyFromUI = function(form)
    {
        that.setOperator(trim(form.critOperator.value));
        that.setRegistration(trim(form.critRegistration.value));
        that.setIcao(trim(form.critIcao.value));
        that.setCountry(trim(form.critCountry.value));
        that.setWakeTurbulenceCategory(form.critWtc.value);
        that.setSpecies(form.critSpecies.value);
        that.setIsMilitary(form.critIsMilitary.checked);
    };

    this.subclassCopyToUI = function(form)
    {
        form.critOperator.value = mOperator === null ? '' : mOperator;
        form.critRegistration.value = mRegistration === null ? '' : mRegistration;
        form.critIcao.value = mIcao === null ? '' : mIcao;
        form.critCountry.value = mCountry === null ? '' : mCountry;
        form.critWtc.value = mWtc === null ? '' : mWtc;
        form.critSpecies.value = mSpecies === null ? '' : mSpecies;
        form.critIsMilitary.checked = mIsMilitary;
    };

    this.subclassIsValid = function()
    {
        return that.getCallsign() !== null ||
               that.getFromDate() !== null ||
               that.getToDate() !== null ||
               that.getOperator() !== null ||
               that.getRegistration() !== null ||
               that.getIcao() !== null ||
               that.getCountry() !== null
               that.getWtc() !== null ||
               that.getSpecies() !== null ||
               that.getIsMilitary();
    };

    this.subclassToString = function()
    {
        var result = '';
        if(that.getOperator()) result += addCriteria('op-Q', that.getOperator());
        if(that.getRegistration()) result += addCriteria('reg-Q', that.getRegistration());
        if(that.getIcao()) result += addCriteria('icao-Q', that.getIcao());
        if(that.getCountry()) result += addCriteria('cou-Q', that.getCountry());
        if(that.getWakeTurbulenceCategory()) result += addCriteria('wtc-Q', that.getWakeTurbulenceCategory());
        if(that.getSpecies()) result += addCriteria('spc-Q', that.getSpecies());
        if(that.getIsMilitary()) result += addCriteria('mil-Q', '1');

        return result;
    };

    function addCriteria(name, value)
    {
        var result = '';
        if(value !== null && value.length > 0) result = '&' + name + '=' + encodeURIComponent(value);

        return result;
    };

    this.subclassAddUI = function()
    {
        var html = '';

        html += that.addUITextField('critRegistration', 'Registration', 10, true);
        html += that.addUITextField('critIcao', 'ICAO', 10, true);
        html += that.addUITextField('critOperator', 'Operator', 20, false);
        html += that.addUITextField('critCountry', 'Country', 20, false);
        html += that.addUICheckBoxField('critIsMilitary', 'Is military');
        html += that.addUIComboBoxField('critWtc', 'Weight', 1, [
                { value: '', text: '' },
                { value: '0', text: 'None' },
                { value: '1', text: 'Light (to 7 tons)' },
                { value: '2', text: 'Medium (to 135 tons)' },
                { value: '3', text: 'Heavy (over 135 tons)' }
            ]);
        html += that.addUIComboBoxField('critSpecies', 'Species', 1, [
                { value: '', text: '' },
                { value: '0', text: 'None' },
                { value: '1', text: 'Landplane' },
                { value: '2', text: 'Seaplane' },
                { value: '3', text: 'Amphibian' },
                { value: '4', text: 'Helicopter' },
                { value: '5', text: 'Gyrocopter' },
                { value: '6', text: 'Tilt-wing' }
            ]);

        return html;
    };
}

DateReportCriteria.prototype = new ReportCriteria();