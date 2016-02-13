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
        private _ViewId = new ViewId('About');

        constructor()
        {
            this.refreshState();
        }

        refreshState()
        {
            this._ViewId.ajax('GetState', {
                success: (data: IResponse<ViewJson.IAboutView>) => {
                    this.applyState(data);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            }, false);
        }

        private applyState(state: IResponse<ViewJson.IAboutView>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(this._Model, state.Response);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response, {
                    extend: {
                        "{root}": function(root: Model)
                        {
                            root.Environment = ko.computed(() => VRS.stringUtility.format(root.IsMono() ? VRS.Server.$$.EnvironmentMono : VRS.Server.$$.EnvironmentDotNet, root.Is64BitProcess ? '64' : '32'));
                            root.FormattedDescription = ko.computed(() => root.Description().replace(/(?:\r\n|\r|\n)/g, '<br />'));
                        }
                    }
                });
                ko.applyBindings(this._Model);
            }
        }
    }
}