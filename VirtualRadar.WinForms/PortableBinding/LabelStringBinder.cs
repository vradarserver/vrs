using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// Binds a label to a string property.
    /// </summary>
    public class LabelStringBinder<TModel> : ValueBinder<TModel, Label, string>
        where TModel: class, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="getModelValue"></param>
        /// <param name="setModelValue"></param>
        public LabelStringBinder(TModel model, Label control, Expression<Func<TModel, string>> getModelValue, Action<TModel, string> setModelValue)
            : base(model, control, getModelValue, setModelValue,
                r => (r.Text ?? "").Trim(),
                (ctrl, val) => ctrl.Text = (val ?? "").Trim())
        {
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoHookControlPropertyChanged(EventHandler eventHandler)
        {
            Control.TextChanged += eventHandler;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoUnhookControlPropertyChanged(EventHandler eventHandler)
        {
            Control.TextChanged -= eventHandler;
        }
    }
}
