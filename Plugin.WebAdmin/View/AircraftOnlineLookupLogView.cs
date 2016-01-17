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
using VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    class AircraftOnlineLookupLogView : IAircraftOnlineLookupLogView
    {
        private IAircraftOnlineLookupLogPresenter _Presenter;
        private ViewModel _ViewModel;

        public DialogResult ShowView()
        {
            _Presenter = Factory.Singleton.Resolve<IAircraftOnlineLookupLogPresenter>();
            _Presenter.Initialise(this);

            return DialogResult.OK;
        }

        public void Dispose()
        {
            _Presenter.Dispose();
        }

        public void Populate(IEnumerable<AircraftOnlineLookupLogEntry> logEntries)
        {
            _ViewModel = new ViewModel(logEntries);
        }

        [WebAdminMethod]
        public ViewModel GetState()
        {
            return _ViewModel;
        }
    }
}
