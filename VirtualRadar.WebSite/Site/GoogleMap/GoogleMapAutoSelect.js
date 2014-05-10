var AutoSelectField = { nothing: '-', altitude: 'A', distance: 'D' }
var AutoSelectCondition = { lessThan: 'l', lessThanOrEqual: 'L', equal: 'e', greaterThan: 'g', greaterThanOrEqual: 'G' }

function AutoSelectExpression()
{
    var that = this;
    var mField = AutoSelectField.altitude;
    this.getField = function() { return mField; };
    this.setField = function(value) { mField = value; };
    var mCondition = AutoSelectCondition.lessThanOrEqual;
    this.getCondition = function() { return mCondition; };
    this.setCondition = function(value) { mCondition = value; };
    var mValue = 0;
    this.getValue = function() { return mValue; };
    this.setValue = function(value)
    {
        mValue = value;
        if(that.isNumeric()) {
            if(mValue === null) mValue = 0;
            else {
                mValue = Number(mValue);
                if(isNaN(mValue)) mValue = 0;
            }
        }
    }

    this.isNumeric = function() { return true; };

    this.toString = function() { return mField + mCondition + mValue; };
    this.fromString = function(text)
    {
        if(text !== null && text !== undefined && text.length > 2) {
            that.setField(text.charAt(0));
            that.setCondition(text.charAt(1));
            that.setValue(text.substr(2));
        }
    };

    this.pass = function(aircraft)
    {
        var result = false;

        var aircraftValue;
        switch(mField) {
            case AutoSelectField.altitude: aircraftValue = aircraft.ConvertedAlt; break;
            case AutoSelectField.distance: aircraftValue = aircraft.ConvertedDst; break;
        }

        if(aircraftValue !== null && aircraftValue !== undefined && (!that.isNumeric() || !isNaN(aircraftValue))) {
            switch(mCondition) {
                case AutoSelectCondition.lessThan:              result = aircraftValue < mValue; break;
                case AutoSelectCondition.lessThanOrEqual:       result = aircraftValue <= mValue; break;
                case AutoSelectCondition.equal:                 result = aircraftValue == mValue; break;
                case AutoSelectCondition.greaterThan:           result = aircraftValue > mValue; break;
                case AutoSelectCondition.greaterThanOrEqual:    result = aircraftValue >= mValue; break;
            }
        }

        return result;
    }
}

function AutoSelect()
{
    var that = this;
    this.useClosest = true;
    this.expressions = [];

    this.toString = function()
    {
        var expressionStrings = [];
        for(var i = 0;i < that.expressions.length;++i) {
            expressionStrings.push(that.expressions[i].toString());
        }

        return (that.useClosest ? 'C' : 'F') + writeStringArrayToString(expressionStrings);
    };

    this.fromString = function(text)
    {
        that.useClosest = true;
        that.expressions = [];
        if(text !== null && text !== undefined && text.length >= 1) {
            that.useClosest = text.charAt(0) != 'F';
            if(text.length > 1) {
                var expressionStrings = readStringArrayFromString(text.substr(1));
                for(var i = 0;i < expressionStrings.length;++i) {
                    var expression = new AutoSelectExpression();
                    expression.fromString(expressionStrings[i]);
                    that.expressions.push(expression);
                }
            }
        }
    };
};

function AutoSelectState(autoSelect)
{
    var that = this;
    var mAutoSelect = autoSelect;
    var mSelected = null;
    this.getSelected = function() { return mSelected; };

    this.evaluate = function(aircraft)
    {
        if(mSelected === null ||
            (mAutoSelect.useClosest && mSelected.Dst > aircraft.Dst) ||
            (!mAutoSelect.useClosest && mSelected.Dst < aircraft.Dst)) {
            if(expressionsPass(aircraft)) mSelected = aircraft;
        }
    };

    function expressionsPass(aircraft)
    {
        var expressionsCount = mAutoSelect.expressions.length;
        var result = expressionsCount === 0;
        if(!result) {
            for(var i = 0;i < expressionsCount;++i) {
                result = mAutoSelect.expressions[i].pass(aircraft);
                if(!result) break;
            }
        }

        return result;
    };
}