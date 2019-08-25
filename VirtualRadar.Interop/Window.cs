// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using InterfaceFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VirtualRadar.Interface;

namespace VirtualRadar.Interop
{
    /// <summary>
    /// Wraps Window interops.
    /// </summary>
    public static class Window
    {
        /// <summary>
        /// Gets a value indicating that the methods here do nothing.
        /// </summary>
        public static bool IsInert { get; private set; }

        /// <summary>
        /// The pInvoke for SendMessage.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The pInvoke for SendMessage with an HDITEM lParam.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="hditem"></param>
        /// <returns></returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM hditem);

        /// <summary>
        /// Initialises the static fields.
        /// </summary>
        static Window()
        {
            try {
                IsInert = Factory.ResolveSingleton<IRuntimeEnvironment>().IsMono;
            } catch {
                // This can be called inadvertently by the VS designer - it'll always throw an exception
            }
        }

        /// <summary>
        /// Invokes SendMessage.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public static IntPtr CallSendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam)
        {
            return IsInert ? DoNothingIntPtr() : DoSendMessage(hWnd, msg, wParam, lParam);
        }

        private static IntPtr DoSendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam)
        {
            return SendMessage(hWnd, msg, wParam, lParam);
        }

        private static IntPtr DoNothingIntPtr()
        {
            return IntPtr.Zero;
        }

        /// <summary>
        /// Invokes SendMessage with an HDITEM parameter.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="hditem"></param>
        /// <returns></returns>
        public static IntPtr CallSendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM hditem)
        {
            return IsInert ? DoNothingIntPtr() : DoSendMessage(hWnd, msg, wParam, ref hditem);
        }

        private static IntPtr DoSendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM hditem)
        {
            return SendMessage(hWnd, msg, wParam, ref hditem);
        }
    }
}
