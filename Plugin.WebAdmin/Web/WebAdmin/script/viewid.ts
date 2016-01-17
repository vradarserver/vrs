namespace VRS.WebAdmin
{
    export class ViewId
    {
        private _HeartbeatTimerId: number = null;
        private _LostContact = false;

        private _Id: string;
        get Id() : string
        {
            return this._Id;
        }

        private _ViewName: string;
        get ViewName() : string
        {
            return this._ViewName;
        }

        constructor(viewName: string, viewId: string)
        {
            this._ViewName = viewName;
            this._Id = viewId;

            this.sendHeartbeat();
        }

        private setHeartbeatTimer()
        {
            this._HeartbeatTimerId = setTimeout(() => {
                this.sendHeartbeat();
            }, 10000);
        }

        private sendHeartbeat()
        {
            this.ajax({
                url: this._ViewName + '/BrowserHeartbeat',
                success: () => {
                    this.setHeartbeatTimer();
                },
                error: () => {
                    this._LostContact = true;

                    var modalBackdrop = $('<div />')
                        .addClass('modal-alert')
                        .appendTo($('body'));
                    $('<div />')
                        .addClass('alert alert-danger text-center')
                        .text(VRS.WebAdmin.$$.WA_Lost_Contact)
                        .appendTo(modalBackdrop);
                }
            });
        }

        ajax(settings: JQueryAjaxSettings) : JQueryXHR
        {
            if(!this._LostContact) {
                var data = settings.data || {};
                data.__ViewId = this._Id;

                settings.data = data;

                return $.ajax(settings);
            }
        }
    }
} 