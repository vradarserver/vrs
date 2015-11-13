declare namespace VRS
{
    export interface StringUtility
    {
        contains(text: string, hasText: string, ignoreCase?: boolean) : boolean;
        
        equals(lhs: string, rhs: string, ignoreCase?: boolean) : boolean;
        
        filter(text: string, allowCharacter: (string) => boolean) : string;
        
        filterReplace(text: string, replaceCharacter: (string) => string) : string;
        
        htmlEscape(text: string) : string;
        
        format(text: string, ...args: any[]) : string;
        
        formatNumber(value: number, format: string) : string;
        
        indexNotOf(text: string, character: string) : number;
        
        isUpperCase(text: string) : boolean;
        
        repeatedSequence(sequence: string, count: number) : string;
        
        padLeft(text: string, ch: string, length: number) : string;
        
        padRight(text: string, ch: string, length: number) : string;
        
        startsWith(text: string, withText: string, ignoreCase?: boolean) : boolean;
        
        endsWith(text: string, withText: string, ignoreCase?: boolean) : boolean;
    }

    export var stringUtility: VRS.StringUtility;
}
