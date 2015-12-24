// Type definitions for the Marker With Label Google Maps utility library.
// Project: http://google-maps-utility-library-v3.googlecode.com/svn/tags/markerwithlabel/

declare interface MarkerWithLabelOptions extends google.maps.MarkerOptions
{
    /**
        * The URL of the cross image to be displayed while dragging a marker. The default value is
        * "http://maps.gstatic.com/intl/en_us/mapfiles/drag_cross_67_16.png".
        */
    crossImage?: string;

    /**
        * The URL of the cursor to be displayed while dragging a marker. The default value is
        * "http://maps.gstatic.com/intl/en_us/mapfiles/closedhand_8_8.cur".
        */
    handCursor?: string;

    /**
        * By default, a label is drawn with its anchor point at (0,0) so that its top left corner is positioned at the anchor point of
        * the associated marker. Use this property to change the anchor point of the label.
        */
    // For example, to center a 50px-wide label beneath a marker, specify a labelAnchor of google.maps.Point(25, 0).
    // (Note: x-values increase to the right and y-values increase to the top.)
    labelAnchor?: google.maps.Point;

    /**
        * The name of the CSS class defining the styles for the label. Note that style values for position, overflow, top, left, zIndex,
        * display, marginLeft, and marginTop are ignored; these styles are for internal use only.
        */
    labelClass?: string;

    /**
        * The content of the label (plain text or an HTML DOM node).
        */
    labelContent?: string | Node;

    /**
        * A flag indicating whether a label that overlaps its associated marker should appear in the background (i.e., in a plane below the marker).
        * The default is false, which causes the label to appear in the foreground.
        */
    labelInBackground?: boolean;

    /**
        * An object literal whose properties define specific CSS style values to be applied to the label. Style values defined here override
        * those that may be defined in the labelClass style sheet. If this property is changed after the label has been created, all previously
        * set styles (except those defined in the style sheet) are removed from the label before the new style values are applied. Note that
        * style values for position, overflow, top, left, zIndex, display, marginLeft, and marginTop are ignored; these styles are for internal
        * use only.
        */
    labelStyle?: Object;

    /**
        * A flag indicating whether the label is to be visible. The default is true. Note that even if labelVisible is true, the label will not
        * be visible unless the associated marker is also visible (i.e., unless the marker's visible property is true).
        */
    labelVisible?: boolean;

    /**
        * A flag indicating whether rendering is to be optimized for the marker. Important: The optimized rendering technique is not supported by
        * MarkerWithLabel, so the value of this parameter is always forced to false.
        */
    optimized?: boolean;

    /**
        * A flag indicating whether the label and marker are to be raised when the marker is dragged. The default is true. If a draggable marker
        * is being created and a version of Google Maps API earlier than V3.3 is being used, this property must be set to false.
        */
    raiseOnDrag?: boolean;
}

declare class MarkerWithLabel extends google.maps.Marker
{
    constructor(opts: MarkerWithLabelOptions);
}
