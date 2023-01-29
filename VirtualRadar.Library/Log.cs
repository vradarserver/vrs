// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.Types;

namespace VirtualRadar.Library
{
    /// <summary>
    /// Default implementation of <see cref="ILog"/> singleton.
    /// </summary>
    public class Log : ILog
    {
        const int MaxAllowableMessagesInFile = 1000;    // Maximum number of messages to keep in the log file
        const int RateLimitMessageMinutes = 5;          // Merge the messages when they appear within this many minutes of a previous instance (rolling)

        // DI services
        private readonly IClock _Clock;
        private readonly IFileSystemProvider _FileSystem;
        private readonly IThreadingEnvironmentProvider _ThreadingEnvironment;

        // Locking
        private readonly object _SyncLock = new();

        // File content - this gets loaded when the first message is written and then appended
        // to. Lines are removed from the start once we go past the limit on allowable number
        // of lines in the file. Some external mechanism will periodically call Flush to write
        // the lines back to disk.
        private readonly LinkedList<LogMessage> _Content = new();
        private long _NextInstanceId;
        private bool _ContentLoaded;
        private bool _ContentNeedsFlushing;

        // Indexed file content - used to find previous instances of messages without looping through
        // hundreds of extant messages. Messages loaded from previous sessions are not indexed.
        private readonly Dictionary<string, List<LogMessage>> _IndexedContent = new();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FileName => Path.Combine(_FileSystem.LogFolder, "VirtualRadarLog.txt");

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="fileSystem"></param>
        /// <param name="threadingEnvironment"></param>
        public Log(IClock clock, IFileSystemProvider fileSystem, IThreadingEnvironmentProvider threadingEnvironment)
        {
            _Clock = clock;
            _FileSystem = fileSystem;
            _ThreadingEnvironment = threadingEnvironment;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteLine(string message)
        {
            if(!String.IsNullOrWhiteSpace(message)) {
                lock(_SyncLock) {
                    ReadLogFileContent();

                    var utcNow = _Clock.UtcNow;
                    var threadId = _ThreadingEnvironment.CurrentThreadId;

                    var extantInstance = FindExtantInstance(message, utcNow);
                    if(extantInstance != null) {
                        extantInstance.AddInstance(utcNow, threadId);
                    } else {
                        AddLast(new LogMessage(_NextInstanceId++, message, utcNow, threadId));
                        while(_Content.Count > MaxAllowableMessagesInFile) {
                            RemoveFirst();
                        }
                    }

                    _ContentNeedsFlushing = true;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task WriteLineAsync(string message) => await Task.Run(() => WriteLine(message));

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Flush()
        {
            lock(_SyncLock) {
                if(_ContentNeedsFlushing) {
                    var lines = _Content
                        .SelectMany(logMessage => logMessage.ToString().SplitIntoLines())
                        .ToArray(); // <-- for debugging

                    _FileSystem.CreateDirectoryIfNotExists(_FileSystem.LogFolder);
                    _FileSystem.WriteAllLines(FileName, lines);

                    _ContentNeedsFlushing = false;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public async Task FlushAsync() => await Task.Run(() => Flush());

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<LogMessage> GetContent()
        {
            lock(_SyncLock) {
                ReadLogFileContent();
                return _Content
                    .Select(r => LogMessage.Clone(r))
                    .ToArray();
            }
        }

        /// <summary>
        /// Loads the log file content if it's not already loaded. Always call from within a lock.
        /// </summary>
        private void ReadLogFileContent()
        {
            if(!_ContentLoaded) {
                if(_FileSystem.FileExists(FileName)) {
                    var currentMessage = new StringBuilder();

                    void addCurrentMessage()
                    {
                        if(currentMessage.Length > 0) {
                            AddLast(new LogMessage(_NextInstanceId++, currentMessage.ToString()));
                            currentMessage.Clear();
                        }
                    }

                    foreach(var line in _FileSystem.ReadAllLines(FileName)) {
                        if(LogMessage.FirstLineRegex.IsMatch(line)) {
                            addCurrentMessage();
                            currentMessage.Append(line);
                        } else {
                            if(currentMessage.Length > 0) {
                                currentMessage.AppendLine();
                            }
                            currentMessage.Append(line);
                        }
                    }

                    addCurrentMessage();
                }
                _ContentLoaded = true;
            }
        }

        private void AddLast(LogMessage message)
        {
            _Content.AddLast(message);

            if(!message.IsFromPreviousSession) {
                if(!_IndexedContent.TryGetValue(message.Text, out var messages)) {
                    messages = new List<LogMessage>();
                    _IndexedContent.Add(message.Text, messages);
                }
                messages.Add(message);
            }
        }

        private void RemoveFirst()
        {
            var first = _Content.First?.Value;
            _Content.RemoveFirst();

            if(first != null && !first.IsFromPreviousSession) {
                if(_IndexedContent.TryGetValue(first.Text, out var messages)) {
                    messages.Remove(first);
                    if(messages.Count == 0) {
                        _IndexedContent.Remove(first.Text);
                    }
                }
            }
        }

        private LogMessage FindExtantInstance(string message, DateTime utcNow)
        {
            LogMessage result = null;

            if(_IndexedContent.TryGetValue(message, out var candidates)) {
                var lastCandidate = candidates
                    .OrderByDescending(r => r.LastLoggedAtUtc)
                    .FirstOrDefault();
                if(lastCandidate?.LastLoggedAtUtc >= utcNow.AddMinutes(-RateLimitMessageMinutes)) {
                    result = lastCandidate;
                }
            }

            return result;
        }
    }
}
