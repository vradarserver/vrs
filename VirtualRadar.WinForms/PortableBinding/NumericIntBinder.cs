// Copyright © 2014 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// Binds an intger property on a model to a numeric up-down control.
    /// </summary>
    public class NumericIntBinder<TModel> : ValueBinder<TModel, NumericUpDown, int>
        where TModel: class, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="getModelValue"></param>
        /// <param name="setModelValue"></param>
        public NumericIntBinder(TModel model, NumericUpDown control, Expression<Func<TModel, int>> getModelValue, Action<TModel, int> setModelValue)
            : base(model, control, getModelValue, setModelValue,
                r => (int)r.Value,
                (ctrl, val) => ctrl.Value = (decimal)val)
        {
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoHookControlPropertyChanged(EventHandler eventHandler)
        {
            Control.ValueChanged += eventHandler;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoUnhookControlPropertyChanged(EventHandler eventHandler)
        {
            Control.ValueChanged -= eventHandler;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialiseControl()
        {
            var isMono = Factory.Resolve<IRuntimeEnvironment>().Singleton.IsMono;
            if(isMono) {
                Control.TextAlign = HorizontalAlignment.Left;
            }
            base.DoInitialiseControl();
        }
    }
}
