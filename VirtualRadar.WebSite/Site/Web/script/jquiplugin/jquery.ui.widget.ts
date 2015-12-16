/**
 * Based on an answer by Benjamin in StackOverflow regarding implementing jQuery UI widgets in TypeScript:
 * http://stackoverflow.com/questions/14578651/define-a-custom-jquery-ui-widget-in-typescript
 */
/**
 * @fileoverview The base class for all custom jQuery UI widgets.
 */

/**
 * The options that all widgets have in common.
 */
interface JQueryUICustomWidget_Options
{
    disabled?:  boolean;
    hide?:      boolean;
    show?:      boolean;
}

/**
 * The base class for JQuery UI custom widgets.
 */
class JQueryUICustomWidget
{
    public defaultElement:      string;
    public document:            JQuery;
    public element:             JQuery;
    public namespace:           string;
    public uuid:                string;
    public version:             string;
    public widgetEventPrefix:   string;
    public widgetFullName:      string;
    public widgetName:          string;
    public window:              JQuery;

    constructor()
    {
        // A bit of foulness that all widgets need to call to get rid of the widget factory methods and fields
        // that have been declared on this base. We don't want them here at run-time, they're just standing in
        // for the methods and fields that WidgetFactory exposes for all custom widgets.
        var myPrototype = (<Function>JQueryUICustomWidget).prototype;
        $.each(myPrototype, (propertyName, value) => {
            delete myPrototype[propertyName];
        });
    }

    _trigger(triggerType: string, e?: Event, d?: Object)
    {
    }
}
 