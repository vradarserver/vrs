namespace VRS.WebAdmin.Log
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View;

    interface Model extends ViewJson.ILogView_KO
    {
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ScrollToEnd = $('#scrollToEnd');
        private _ScrollToTop = $('#scrollToTop');

        constructor()
        {
            this._ScrollToEnd.on('click', function() {
                $('html, body').animate({
                    scrollTop: $(document).height()
                }, 'fast');
                return false;
            }).hide();

            this._ScrollToTop.on('click', function() {
                $('html, body').animate({
                    scrollTop: 0
                }, 'fast');
                return false;
            }).hide();

            this.refreshState();
        }

        refreshState()
        {
            $.ajax({
                url: 'Log/GetState',
                success: (data: IResponse<ViewJson.ILogView>) => {
                    this.applyState(data);
                    this._ScrollToEnd.show();
                    this._ScrollToTop.show();
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            });
        }

        private applyState(state: IResponse<ViewJson.ILogView>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(state.Response, this._Model);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response);
                ko.applyBindings(this._Model);
            }
        }
    }
}
