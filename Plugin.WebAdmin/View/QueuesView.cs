using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Plugin.WebAdmin.View.Queues;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    class QueuesView : IBackgroundThreadQueuesView
    {
        private IBackgroundThreadQueuesPresenter _Presenter;
        private ViewModel _ViewModel;

        public DialogResult ShowView()
        {
            _Presenter = Factory.Singleton.Resolve<IBackgroundThreadQueuesPresenter>();
            _Presenter.Initialise(this);

            return DialogResult.OK;
        }

        public void Dispose()
        {
            _Presenter.Dispose();
        }

        public void RefreshDisplay(IQueue[] queues)
        {
            _ViewModel = new ViewModel(queues);
        }

        [WebAdminMethod]
        public ViewModel GetState()
        {
            return _ViewModel;
        }
    }
}
