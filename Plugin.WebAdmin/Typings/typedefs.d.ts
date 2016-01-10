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