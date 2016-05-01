using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.CustomContent.WebAdmin
{
    class OptionsView : IOptionsView
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
        public ViewModel GetState()
        {
            var options = OptionsStorage.Load(Plugin.Singleton);
            var viewModel = new ViewModel(options);
            Validate(options, viewModel);

            return viewModel;
        }

        [WebAdminMethod(DeferExecution = true)]
        public SaveOutcomeModel Save(ViewModel viewModel)
        {
            var outcome = "";

            var options = new Options();
            viewModel.CopyToSettings(options);

            if(!Validate(options, viewModel)) {
                outcome = "FailedValidation";
            } else {
                try {
                    OptionsStorage.Save(Plugin.Singleton, options);
                    outcome = "Saved";
                } catch(ConflictingUpdateException) {
                    outcome = "ConflictingUpdate";
                }
                options = OptionsStorage.Load(Plugin.Singleton);
                viewModel.RefreshFromSettings(options);
                Validate(options, viewModel);
            }

            return new SaveOutcomeModel(outcome, viewModel);
        }

        private bool Validate(Options options, ViewModel viewModel)
        {
            var validator = new OptionsValidator();
            var validationResults = validator.Validate(options);

            var validationHelper = new ValidationModelHelper(r => {
                object model = viewModel;
                var injectSettings = r.Record as InjectSettings;
                if(injectSettings != null) {
                    model = viewModel.InjectSettings.FirstOrDefault(i => String.Equals(i.File, injectSettings.File));
                }
                return model;
            });
            validationHelper.ApplyValidationResults(validationResults, viewModel);

            return !validationResults.HasErrors;
        }
    }
}
