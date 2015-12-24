function IcaoReportCriteria()
{
    var that = this.registerSubclass(this);

    var mIcao = null;

    this.getIcao = function() { return mIcao; };
    this.setIcao = function(value) { mIcao = value; if(mIcao !== null) mIcao = mIcao.toUpperCase(); };

    this.subclassCopyFromUI = function(form)
    {
        that.setIcao(trim(form.critIcao.value));
    };

    this.subclassCopyToUI = function(form)
    {
        form.critIcao.value = mIcao === null ? '' : mIcao;
    };

    this.subclassIsValid = function()
    {
        return mIcao !== null && mIcao.length > 0;
    };

    this.subclassToString = function()
    {
        var result = '';
        if(mIcao) result += '&icao-Q=' + encodeURIComponent(mIcao);
        return result;
    };

    this.subclassAddUI = function()
    {
        return that.addUITextField('critIcao', 'ICAO', 10, true);
    };
}

IcaoReportCriteria.prototype = new ReportCriteria();