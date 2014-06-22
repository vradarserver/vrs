using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.OptionPage
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public class ValidationFieldAttribute : Attribute
    {
        public ValidationField ValidationField { get; private set; }

        public bool RaisesValueChanged { get; set; }

        public ErrorIconAlignment IconAlignment { get; set; }

        public ValidationFieldAttribute(ValidationField validationField)
        {
            ValidationField = validationField;
            RaisesValueChanged = true;
            IconAlignment = ErrorIconAlignment.MiddleLeft;
        }
    }
}
