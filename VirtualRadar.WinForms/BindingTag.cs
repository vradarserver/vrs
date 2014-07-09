using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Carries a <see cref="System.Windows.Forms.Binding"/> and a tag to associate with it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindingTag<T>
    {
        /// <summary>
        /// Gets the binding that the tag is associated with.
        /// </summary>
        public System.Windows.Forms.Binding Binding { get; private set; }

        /// <summary>
        /// Gets or sets the tag associated with the binding.
        /// </summary>
        public T Tag { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="binding"></param>
        public BindingTag(System.Windows.Forms.Binding binding) : this(binding, default(T))
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="tag"></param>
        public BindingTag(System.Windows.Forms.Binding binding, T tag)
        {
            Binding = binding;
            Tag = tag;
        }
    }
}
