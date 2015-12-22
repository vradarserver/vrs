declare interface JQueryStatic
{
    // jquery.d.ts is missing a $() constructor that I'm using
    (object: JQuery, selector: string): JQuery;
} 