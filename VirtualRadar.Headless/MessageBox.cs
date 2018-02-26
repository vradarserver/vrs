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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Localisation;

namespace VirtualRadar.Headless
{
    /// <summary>
    /// The default command-line implementation of <see cref="IMessageBox"/>.
    /// </summary>
    class MessageBox : IMessageBox
    {
        /// <summary>
        /// The console wrapper to send messages to.
        /// </summary>
        private static IConsole _Console;

        /// <summary>
        /// The colour to use when highlighting text.
        /// </summary>
        static readonly ConsoleColor HighlightColour = ConsoleColor.White;

        /// <summary>
        /// The static initialiser.
        /// </summary>
        static MessageBox()
        {
            _Console = Factory.ResolveSingleton<IConsole>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        public void Show(string message)
        {
            _Console.WriteLine(message);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void Show(string message, string title)
        {
            var currentColour = Console.ForegroundColor;
            try {
                _Console.ForegroundColor = HighlightColour;
                _Console.WriteLine(title);
            } finally {
                _Console.ForegroundColor = currentColour;
            }

            Show(message);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            Show(message, title);

            var messageBoxButtons = BuildMessageBoxButtonsList(buttons);
            var defaultMessageBoxButton = FindDefaultMessageBoxButton(defaultButton, messageBoxButtons);

            ShowButtonsPrompt(messageBoxButtons, defaultMessageBoxButton);
            var result = WaitForKeypress(messageBoxButtons, defaultMessageBoxButton);

            return result;
        }

        /// <summary>
        /// Returns a list of <see cref="MessageBoxButton"/> objects that correspond with the buttons enum passed across.
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns></returns>
        private static List<MessageBoxButton> BuildMessageBoxButtonsList(MessageBoxButtons buttons)
        {
            var result = new List<MessageBoxButton>();
            switch(buttons) {
                case MessageBoxButtons.OKCancel:
                    result.Add(new MessageBoxButton(DialogResult.OK,     'Y', Strings.OK));
                    result.Add(new MessageBoxButton(DialogResult.Cancel, 'N', Strings.Cancel));
                    break;
                case MessageBoxButtons.YesNo:
                    result.Add(new MessageBoxButton(DialogResult.Yes,    'Y', Strings.Yes));
                    result.Add(new MessageBoxButton(DialogResult.No,     'N', Strings.No));
                    break;
                case MessageBoxButtons.YesNoCancel:
                    result.Add(new MessageBoxButton(DialogResult.Yes,    'Y', Strings.Yes));
                    result.Add(new MessageBoxButton(DialogResult.No,     'N', Strings.No));
                    result.Add(new MessageBoxButton(DialogResult.Cancel, 'C', Strings.Cancel));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Returns the <see cref="MessageBoxButton"/> corresponding to the default button enum passed across.
        /// </summary>
        /// <param name="defaultButton"></param>
        /// <param name="messageBoxButtons"></param>
        /// <returns></returns>
        private static MessageBoxButton FindDefaultMessageBoxButton(MessageBoxDefaultButton defaultButton, List<MessageBoxButton> messageBoxButtons)
        {
            MessageBoxButton result = null;
            switch(defaultButton) {
                case MessageBoxDefaultButton.Button1: result = messageBoxButtons[0]; break;
                case MessageBoxDefaultButton.Button2: result = messageBoxButtons[1]; break;
                case MessageBoxDefaultButton.Button3: result = messageBoxButtons[2]; break;
                default:                              throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Shows a prompt on the console that consists of each button passed across.
        /// </summary>
        /// <param name="messageBoxButtons"></param>
        /// <param name="defaultMessageBoxButton"></param>
        private static void ShowButtonsPrompt(List<MessageBoxButton> messageBoxButtons, MessageBoxButton defaultMessageBoxButton)
        {
            _Console.Write(String.Join("   ", messageBoxButtons.Select(r => r.ToString()).ToArray()));
            _Console.Write("   [");

            bool isFirstChar = true;
            foreach(var messageBoxButton in messageBoxButtons) {
                if(isFirstChar) isFirstChar = false;
                else _Console.Write('/');

                if(messageBoxButton != defaultMessageBoxButton) {
                    _Console.Write(Char.ToLower(messageBoxButton.Shortcut));
                } else {
                    var currentColour = _Console.ForegroundColor;
                    try {
                        _Console.ForegroundColor = HighlightColour;
                        _Console.Write(Char.ToUpper(messageBoxButton.Shortcut));
                    } finally {
                        _Console.ForegroundColor = currentColour;
                    }
                }
            }

            _Console.Write("]: ");
        }

        /// <summary>
        /// Waits for the user to press a key that corresponds to one of the message box buttons and then returns its associated
        /// DialogResult.
        /// </summary>
        /// <param name="messageBoxButtons"></param>
        /// <param name="defaultMessageBoxButton"></param>
        /// <returns></returns>
        private static DialogResult WaitForKeypress(List<MessageBoxButton> messageBoxButtons, MessageBoxButton defaultMessageBoxButton)
        {
            var result = DialogResult.None;

            do {
                var keyChar = _Console.ReadKey(intercept: true);
                if(keyChar.Key == ConsoleKey.Enter) {
                    result = defaultMessageBoxButton.DialogResult;
                } else {
                    var button = messageBoxButtons.FirstOrDefault(r => Char.ToUpper(keyChar.KeyChar) == r.Shortcut);
                    if(button != null) result = button.DialogResult;
                }

                if(result == DialogResult.None) {
                    _Console.Beep();
                }
            } while(result == DialogResult.None);
            _Console.WriteLine();

            return result;
        }
    }
}
