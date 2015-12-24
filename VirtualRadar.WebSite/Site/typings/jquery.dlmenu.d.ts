// Type definitions for the modified version of jquery.dlmenu

interface DLMenu_Options
{
    onLevelClick?:  (el: JQuery, name: string) => boolean;
    onLinkClick?:   (el: JQuery, ev: Event) => boolean;
    backLinkText?:  string;
}

interface DLMenu
{
    (options: DLMenu_Options, element: JQuery);
    dispose:    () => void;
    closeMenu:  () => void;
    openMenu:   () => void;
}

interface JQuery
{
    dlmenu() : DLMenu;
    dlmenu(options: DLMenu_Options) : DLMenu;
}