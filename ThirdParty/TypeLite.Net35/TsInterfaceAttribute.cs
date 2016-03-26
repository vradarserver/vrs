using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite {
    /// <summary>
    /// Configures an interface to be included in the script model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class TsInterfaceAttribute : Attribute {
        /// <summary>
        /// Gets or sets the name of the interface in the script model. If it isn't set, the name of the CLR interface is used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets name of the module for this interface. If it isn't set, the namespace is used.
        /// </summary>
        public string Module { get; set; }
    }
}
