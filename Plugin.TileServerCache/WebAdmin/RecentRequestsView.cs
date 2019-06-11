using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.TileServerCache.WebAdmin
{
    class RecentRequestsView : IRecentRequestsView
    {
        public DialogResult ShowView()
        {
            return DialogResult.OK;
        }

        public void Dispose()
        {
            ;
        }

        [WebAdminMethod]
        public RecentRequestsViewModel GetState()
        {
            var controller = new RecentRequestsController();
            return new RecentRequestsViewModel(
                controller.GetRecentRequestOutcomes()
            );
        }
    }
}
