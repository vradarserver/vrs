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
    protected defaultElement:      string;
    protected document:            JQuery;
    protected element:             JQuery;
    protected namespace:           string;
    protected uuid:                string;
    protected version:             string;
    protected widgetEventPrefix:   string;
    protected widgetFullName:      string;
    protected widgetName:          string;
    protected window:              JQuery;

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

    protected _delay(callback: Function, milliseconds?: number) : number
    {
        throw 'Should not see this';
    }

    protected _focusable(element: JQuery) : JQuery
    {
        throw 'Should not see this';
    }

    protected _getCreateEventData() : Object
    {
        throw 'Should not see this';
    }

    protected _getCreateOptions() : Object
    {
        throw 'Should not see this';
    }

    protected _hide(element: JQuery, option: Object, callback?: () => void) : JQuery
    {
        throw 'Should not see this';
    }

    protected _hoverable(element: JQuery) : JQuery
    {
        throw 'Should not see this';
    }

    protected _off(element: JQuery, eventName: string) : JQuery
    {
        throw 'Should not see this';
    }

    protected _on(...args: any[]) : JQuery
    {
        throw 'Should not see this';
    }

    protected _setOption(key: string, value: Object) : JQuery
    {
        throw 'Should not see this';
    }

    protected _setOptions<T>(options: T) : JQuery
    {
        throw 'Should not see this';
    }

    protected _show(element: JQuery, option: Object, callback?: () => void) : JQuery
    {
        throw 'Should not see this';
    }

    protected _super(...args: any[]) : JQuery
    {
        throw 'Should not see this';
    }

    protected _superApply(args: any[]) : JQuery
    {
        throw 'Should not see this';
    }

    protected _trigger(triggerType: string, event?: Event, data?: Object) : boolean
    {
        throw 'Should not see this';
    }

    destroy() : JQuery
    {
        throw 'Should not see this';
    }

    disable() : JQuery
    {
        throw 'Should not see this';
    }

    enable() : JQuery
    {
        throw 'Should not see this';
    }

    instance() : Object
    {
        throw 'Should not see this';
    }

    option(...args: any[]) : Object
    {
        throw 'Should not see this';
    }

    widget() : JQuery
    {
        throw 'Should not see this';
    }
}
 