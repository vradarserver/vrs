// Type definitions for Knockout Viewmodel 1.1.3
// Project: http://coderenaissance.github.com/knockout.viewmodel/
// Definitions by: Oisin Grehan <https://github.com/oising>
// Definitions: https://github.com/borisyankov/DefinitelyTyped
//
// CUSTOMISED, DON'T OVERWRITE

// /// <reference path="../knockout/knockout.d.ts" />

// AGW - added options declaration
interface KnockoutViewModelOptions
{
    custom?:        Object;
    append?:        Object;
    exclude?:       Object;
    extend?:        Object;
    arrayChildId?:  Object;
    shared?:        Object;
}

interface KnockoutViewModelStatic {
    toModel(viewmodel: any): any;
    fromModel(model: any, options?: KnockoutViewModelOptions): any;
    updateFromModel(viewmodel: any, model: any);
    
    // INTERNAL flag: enable logging of conversions
    // logs will be written to console
    logging: boolean;
}

// Extend ko global
interface KnockoutStatic {
    viewmodel: KnockoutViewModelStatic;
}