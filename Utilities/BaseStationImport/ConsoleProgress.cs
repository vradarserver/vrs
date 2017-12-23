// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;

namespace BaseStationImport
{
    /// <summary>
    /// Handles showing progress bars on the console.
    /// </summary>
    public class ConsoleProgress
    {
        // Progress state
        private bool _Running;
        private double _LastProgressPercentage;
        private string _LastProgressMessage;
        private DateTime _ProgressStartedUtc;
        private DateTime _LastProgressUpdateUtc;
        private int _ProgressLineLength;

        private int _MaxMessageWidth = 20;
        /// <summary>
        /// Gets or sets the number of characters that progress messages are truncated to.
        /// </summary>
        public int MaxMessageWidth
        {
            get { return _MaxMessageWidth; }
            set { _MaxMessageWidth = _Running ? _MaxMessageWidth : value; }
        }

        private int _PercentPerBlock = 5;
        /// <summary>
        /// Gets or sets the number of percent represented by a single character in the progress bar.
        /// </summary>
        public int PercentPerBlock
        {
            get { return _PercentPerBlock; }
            set { _PercentPerBlock = _Running ? _PercentPerBlock : value; }
        }

        /// <summary>
        /// Gets the number of characters required for the progress bar.
        /// </summary>
        public int BarWidth => 100 / PercentPerBlock;

        private bool? _IsConsoleAttached;
        /// <summary>
        /// Gets a value indicating that a GUI is attached.
        /// </summary>
        public bool IsConsoleAttached
        {
            get {
                if(_IsConsoleAttached == null) {
                    _IsConsoleAttached = !Console.IsOutputRedirected;
                    if(_IsConsoleAttached.Value) {
                        try {
                            if(Console.WindowHeight <= 0) {
                                _IsConsoleAttached = false;
                            }
                        } catch {
                            _IsConsoleAttached = false;
                        }
                    }
                }
                return _IsConsoleAttached.Value;
            }
        }

        /// <summary>
        /// Initialises a progress bar.
        /// </summary>
        public void StartProgress()
        {
            if(!_Running) {
                _Running = true;

                _LastProgressMessage = "";
                _LastProgressPercentage = -1.0;
                _ProgressStartedUtc = DateTime.UtcNow;
                _LastProgressUpdateUtc = default(DateTime);

                if(IsConsoleAttached) {
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Updates a progress bar.
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="maxIterations"></param>
        /// <param name="message"></param>
        public void UpdateProgress(double iteration, double maxIterations, string message = "")
        {
            if(IsConsoleAttached) {
                message = message ?? "";
                var percentage = maxIterations < 0 ? 0 :
                                 maxIterations == 0 ? 100.0 :
                                 (iteration / maxIterations) * 100.0;
                var now = DateTime.UtcNow;

                if(message != _LastProgressMessage ||
                   iteration >= maxIterations ||
                   Math.Abs(percentage - _LastProgressPercentage) > 2.5 ||
                   (now - _LastProgressUpdateUtc).TotalMilliseconds > 500
                ) {
                    _LastProgressMessage = message;
                    _LastProgressPercentage = percentage;
                    _LastProgressUpdateUtc = now;

                    var millisecondsSoFar = (now - _ProgressStartedUtc).TotalMilliseconds;
                    var avgIterationMs = iteration == 0 ? 0 : millisecondsSoFar / iteration;
                    var msToGo = Math.Max(0.0, (maxIterations - iteration) * avgIterationMs);
                    var spanLeft = now.AddMilliseconds(msToGo) - now;
                    var remaining = String.Format("{0:00}:{1:00}:{2:00}", (spanLeft.Days * 24) + spanLeft.Hours, spanLeft.Minutes, spanLeft.Seconds);

                    var blocks = new string('#', ((int)percentage) / PercentPerBlock);
                    var spaces = new string(' ', BarWidth - blocks.Length);
                    var truncatedMessage = message.Length > MaxMessageWidth ? message.Substring(0, MaxMessageWidth) : message;

                    var displayBuffer = new StringBuilder();
                    displayBuffer.Append($" [{blocks}{spaces}]");
                    if(maxIterations >= 0) {
                        displayBuffer.Append($" [{iteration:N0} / {maxIterations:N0}]");
                        displayBuffer.Append($" [{percentage:N1}%] {remaining}");
                        displayBuffer.Append(' ');
                    }
                    displayBuffer.AppendFormat($" {{0,-{MaxMessageWidth}}}", truncatedMessage);

                    RefreshProgressLine(displayBuffer.ToString());
                }
            }
        }

        /// <summary>
        /// Updates the text on the progress bar line and positions the cursor on the line under the progress bar.
        /// </summary>
        /// <param name="text"></param>
        private void RefreshProgressLine(string text)
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(text);
            if(text.Length < _ProgressLineLength) {
                Console.Write(new String(' ', _ProgressLineLength - text.Length));
            }
            _ProgressLineLength = text.Length;

            Console.WriteLine();
        }

        /// <summary>
        /// Performs the final update of a progress bar.
        /// </summary>
        /// <param name="maxIterations"></param>
        public void FinishProgress(double maxIterations)
        {
            if(IsConsoleAttached) {
                UpdateProgress(maxIterations, maxIterations, "");
                _Running = false;
            }
        }
    }
}
