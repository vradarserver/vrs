function GoogleMapOutline()
{
    var that = this;
    var mName = "";
    var mTypeNumber = 0;
    var mOutline = null;
    this.getName = function() { return mName; };
    this.getTypeNumber = function() { return mTypeNumber; };

    this.parse = function(outline)
    {
        mName = outline.name;
        mTypeNumber = outline.oType;
        var points = [];
        for(var setNumber = 0;setNumber < outline.sets.length;++setNumber) {
            var set = outline.sets[setNumber];
            var pointList = [];
            for(var pointNumber = 2;pointNumber < set.length;pointNumber += 2) {
                pointList.push(new google.maps.LatLng(set[pointNumber], set[pointNumber+1]));
            }
            if(pointList.length > 0) points.push(pointList);
        }

        mOutline = new google.maps.Polygon({
            clickable: false,
            strokeColor: '#1111ff',
            strokeWeight: 2,
            paths: points,
            fillOpacity: 0
        });
    };

    this.addToMap = function(map)
    {
        mOutline.setMap(map);
    };

    this.removeFromMap = function()
    {
        mOutline.setMap(null);
    };
};