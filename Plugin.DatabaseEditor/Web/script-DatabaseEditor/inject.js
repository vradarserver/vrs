// Only proceed if the web page has link handlers loaded
if(VRS && VRS.LinkRenderHandler && VRS.linkRenderHandlers) {
    // Add our own entry to the enumeration of link sites
    VRS.LinkSite['DatabaseEditorPlugin'] = 'databaseEditorPlugin';

    // Add a link to the aircraft detail panel (and any other panel that shows default aircraft links)
    VRS.linkRenderHandlers.push(
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.DatabaseEditorPlugin,
            displayOrder:       9000,
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return true; },
            hasChanged:         function(/** VRS.Aircraft */ aircraft) { return false; },
            title:              function(/** VRS.Aircraft */ aircraft) { return 'Database Editor'; },
            buildUrl:           function(/** VRS.Aircraft */ aircraft) { return 'DatabaseEditor/index.html?icao=' + encodeURIComponent(aircraft.formatIcao()); },
            target:             'databaseEditor'
        })
    );
}