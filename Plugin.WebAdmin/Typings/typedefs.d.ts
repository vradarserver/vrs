// Need this - the WebAdmin ts files won't pick it up automatically:
/// <reference path="translations-webadmin.d.ts" />

declare module VRS.WebAdmin
{
    interface IVoidResponse
    {
        Exception: string;
    }

    interface IResponse<T> extends IVoidResponse
    {
        Response: T;
    }
} 