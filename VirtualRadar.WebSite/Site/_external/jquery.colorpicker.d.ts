// Declarations for the jQuery colour picker widget

interface ColourPicker_Configuration
{
    id?: string;
    ico?: string;
    title?: string;
    inputBG?: boolean;
    speed?: number;
    openTxt?: string;
    colours?: string[];
}

interface ColourPicker
{
    (config: ColourPicker_Configuration);
}

interface JQuery
{
    colourPicker() : ColourPicker;
    colourPicker(options: ColourPicker_Configuration) : ColourPicker;
}