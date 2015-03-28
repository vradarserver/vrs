using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    public class NameValue
    {
        public int Value { get; set; }

        public string Name { get; set; }

        public NameValue()
        {
        }

        public NameValue(int value, string name)
        {
            Value = value;
            Name = name;
        }
    }
}
