// Copyright © 2016 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
