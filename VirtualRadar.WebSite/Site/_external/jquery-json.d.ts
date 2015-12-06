interface JQueryStatic
{
    toJSON(obj: any) : string;

    evalJSON(json: string) : any;

    secureEvalJSON(json: string) : any;

    quoteString(str: string) : string;
}
