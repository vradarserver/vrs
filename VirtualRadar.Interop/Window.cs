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
        /// Initialises the static fields.
        /// </summary>
        static Window()
        {
            IsInert = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono;
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
    }
}
