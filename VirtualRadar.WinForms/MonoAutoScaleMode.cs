// Copyright © 2010 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Handles the AutoScaleMode property for forms and user controls under Mono.
    /// </summary>
    /// <remarks>
    /// By default .NET sets the AutoScaleMode for forms and user controls to Font, which is a reasonable thing to do.
    /// However under some Mono distributions this stretches the form - which is fine - without stretching the anchored
    /// controls - which is horrible. This happens during InitializeComponent, which I don't want to mess with, so the
    /// workaround I have is to override the AutoScaleMode and pass it through to the base class version when we're
    /// running under .NET and throw the assignment away when we're running under Mono. This isn't particularly
    /// complicated but to make life a bit easier we have this class, which does all of the grunt work for the form or
    /// user control.
    /// </remarks>
    class MonoAutoScaleMode
    {
        /// <summary>
        /// Gets or sets a value indicating that font scaling is to be disabled throughout the application.
        /// </summary>
        public static bool AlwaysDisableFontScaling;

        /// <summary>
        /// True if we're running under Mono.
        /// </summary>
        private bool _IsMono;

        /// <summary>
        /// The form we're working for.
        /// </summary>
        private Form _Form;

        /// <summary>
        /// The user control we're working for.
        /// </summary>
        private UserControl _UserControl;

        /// <summary>
        /// Gets or sets the AutoScaleMode of the underlying control when running under .NET or AutoScaleMode.None when running under Mono.
        /// </summary>
        public AutoScaleMode AutoScaleMode
        {
            get { return _IsMono ? AutoScaleMode.None : _Form != null ? _Form.AutoScaleMode : _UserControl.AutoScaleMode; }
            set { if(!_IsMono) { if(_Form != null) _Form.AutoScaleMode = value; else if(_UserControl != null) _UserControl.AutoScaleMode = value; } }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        private MonoAutoScaleMode()
        {
            // Unfortunately this MUST be called in the constructor for forms and user controls, which means this will be called at design-time.
            // The factory doesn't have anything in it at design-time, so it'll throw an exception. We need to throw the exception away.
            try {
                _IsMono = AlwaysDisableFontScaling ? true : Factory.ResolveSingleton<IRuntimeEnvironment>().IsMono;
            } catch { }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="form"></param>
        public MonoAutoScaleMode(Form form) : this()
        {
            _Form = form;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="userControl"></param>
        public MonoAutoScaleMode(UserControl userControl) : this()
        {
            _UserControl = userControl;
        }
    }
}
