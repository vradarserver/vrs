namespace VRS.WebAdmin.About
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View;

    interface Model extends ViewJson.IAboutView_KO
    {
        Environment?:           KnockoutComputed<string>;
        FormattedDescription?:  KnockoutComputed<string>;
    }

    export class PageHandler
    {
        private _Model: Model;

        constructor()
        {
            this.refreshState();
        }

        refreshState()
        {
            $.ajax({
                url: 'About/GetState',
                success: (data: IResponse<ViewJson.IAboutView>) => {
                    this.applyState(data);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            });
        }

        private applyState(state: IResponse<ViewJson.IAboutView>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(state.Response, this._Model);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response, {
                    extend: {
                        "{root}": function(root: Model)
                        {
                            root.Environment = ko.computed(() => root.IsMono() ? VRS.WebAdmin.$$.WA_Value_Mono : VRS.WebAdmin.$$.WA_Value_DotNet);
                            root.FormattedDescription = ko.computed(() => root.Description().replace(/(?:\r\n|\r|\n)/g, '<br />'));
                        }
                    }
                });
                ko.applyBindings(this._Model);
            }
        }
    }
}