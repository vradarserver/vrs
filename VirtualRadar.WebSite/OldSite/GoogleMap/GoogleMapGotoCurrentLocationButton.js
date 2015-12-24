function GoogleMapGotoCurrentLocationButton(events)
{
    var that = this;
    var mEvents = events;
    var mContainer;

    this.getVisible = function() { return mContainer.style.display === 'block'; };
    this.setVisible = function(value) { mContainer.style.display = (value ? 'block' : 'none'); };

    this.hide = function() { mContainer.style.display = 'none'; }
    this.show = function() { mContainer.style.display = 'block'; }

    this.addToMap = function(map)
    {
        mContainer = createElement('div', 'gotoCLocButton');
        var inner = createElement('div', 'gotoCLocButtonInner', mContainer);
        var icon = createElement('div', 'gotoCLocButtonIcon', inner);

        that.setVisible(false);

        icon.innerHTML = '<img src="Images/GotoCurrentLocation.png" width="20px" height="20px">';

        google.maps.event.addDomListener(icon, 'click', onButtonClicked);
        google.maps.event.addDomListener(icon, 'doubleclick', onButtonClicked);

        map.controls[google.maps.ControlPosition.TOP_LEFT].push(mContainer);
    };

    function onButtonClicked()
    {
        mEvents.raise(EventId.gotoCurrentLocationClicked, this, null);
    }
}