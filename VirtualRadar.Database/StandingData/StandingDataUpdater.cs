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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.StandingData
{
    /// <summary>
    /// The default implementation of <see cref="IStandingDataUpdater"/>.
    /// </summary>
    class StandingDataUpdater : IStandingDataUpdater
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of <see cref="IStandingDataUpdaterProvider"/>.
        /// </summary>
        class DefaultProvider : IStandingDataUpdaterProvider
        {
            public bool FileExists(string fileName) { return File.Exists(fileName); }

            public string[] ReadLines(string fileName) { return File.ReadAllLines(fileName); }

            public void WriteLines(string fileName, string[] lines)
            {
                string path = Path.GetDirectoryName(fileName);
                if(!String.IsNullOrEmpty(path) && !Directory.Exists(path)) Directory.CreateDirectory(path);
                File.WriteAllLines(fileName, lines);
            }

            public string[] DownloadLines(string url)
            {
                string[] result = new string[] {};
                using(MemoryStream stringStream = new MemoryStream()) {
                    DownloadToStream(url, stringStream, false);
                    string text = Encoding.UTF8.GetString(stringStream.GetBuffer(), 0, (int)stringStream.Length).Replace("\r\n", "\n");
                    result = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }

                return result;
            }

            public void DownloadAndDecompressFile(string url, string fileName)
            {
                string path = Path.GetDirectoryName(fileName);
                if(!String.IsNullOrEmpty(path) && !Directory.Exists(path)) Directory.CreateDirectory(path);

                using(var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    DownloadToStream(url, fileStream, true);
                }
            }

            private void DownloadToStream(string url, Stream stream, bool urlIsGzipFile)
            {
                byte[] buffer = new byte[1024];
                try {
                    WebRequest webRequest = WebRequest.Create(url);
                    using(WebResponse webResponse = WebRequestHelper.GetResponse(webRequest)) {
                        using(Stream remoteStream = WebRequestHelper.GetResponseStream(webResponse)) {
                            var readStream = !urlIsGzipFile ? remoteStream : new GZipStream(remoteStream, CompressionMode.Decompress);
                            try {
                                int bytesRead = 0;
                                do {
                                    bytesRead = readStream.Read(buffer, 0, buffer.Length);
                                    if(bytesRead > 0) stream.Write(buffer, 0, bytesRead);
                                } while(bytesRead > 0);
                                stream.Flush();
                            } finally {
                                if(urlIsGzipFile && readStream != null) readStream.Dispose();
                            }
                        }
                    }
                } catch(Exception ex) {
                    // The standard exception is not very helpful - in particular it doesn't report the URL that we're downloading
                    throw new InvalidOperationException(String.Format("Could not download {0}: {1}", url, ex.Message), ex);
                }
            }

            public void MoveFile(string temporaryFileName, string liveFileName)
            {
                var path = Path.GetDirectoryName(liveFileName);
                if(!String.IsNullOrEmpty(path) && !Directory.Exists(path)) Directory.CreateDirectory(path);

                if(File.Exists(liveFileName)) File.Delete(liveFileName);
                File.Move(temporaryFileName, liveFileName);
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that <see cref="Update"/> locks to prevent two threads updating simultaneously.
        /// </summary>
        private static object _UpdateLock = new object();

        /// <summary>
        /// True if <see cref="Update"/> is running on a thread somewhere.
        /// </summary>
        private bool _UpdateRunning;

        /// <summary>
        /// The filename of the standing data database.
        /// </summary>
        private const string DatabaseFileName = "StandingData.sqb";

        /// <summary>
        /// The temporary filename of the standing data database.
        /// </summary>
        private const string DatabaseTempName = DatabaseFileName + ".tmp";

        /// <summary>
        /// The URL of the file holding the standing data database.
        /// </summary>
        private const string DatabaseUrl = "http://www.virtualradarserver.co.uk/Files/StandingData.sqb.gz";

        /// <summary>
        /// The name of the file that describes the dates and state of the other files.
        /// </summary>
        public const string StateFileName = "FlightNumberCoverage.csv";

        /// <summary>
        /// The name of the file that describes the dates and state of the other files.
        /// </summary>
        public const string StateTempName = StateFileName + ".tmp";

        /// <summary>
        /// The URL of the file that describes the dates and state of the other files.
        /// </summary>
        private const string StateFileUrl = "http://www.virtualradarserver.co.uk/Files/FlightNumberCoverage.csv";
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataUpdaterProvider Provider { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public StandingDataUpdater()
        {
            Provider = new DefaultProvider();
        }
        #endregion

        #region DataIsOld
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool DataIsOld()
        {
            var folder = Factory.Resolve<IConfigurationStorage>().Singleton.Folder;
            var stateFilename = Path.Combine(folder, StateFileName);

            bool result = !Provider.FileExists(stateFilename);
            if(!result) {
                string[] remoteContent = Provider.DownloadLines(StateFileUrl);
                string[] localContent = Provider.ReadLines(stateFilename);
                result = remoteContent.Length < 2 || localContent.Length < 2;
                if(!result) result = remoteContent[1] != localContent[1];
            }

            return result;
        }
        #endregion

        #region Update
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Update()
        {
            if(!_UpdateRunning) {
                lock(_UpdateLock) {
                    _UpdateRunning = true;
                    try {
                        var folder = Factory.Resolve<IConfigurationStorage>().Singleton.Folder;
                        var stateFileName = Path.Combine(folder, StateFileName);
                        var stateTempName = Path.Combine(folder, StateTempName);
                        var databaseFileName = Path.Combine(folder, DatabaseFileName);
                        var databaseTempName = Path.Combine(folder, DatabaseTempName);

                        string[] stateLines = Provider.DownloadLines(StateFileUrl);
                        string[] remoteStateChunks = stateLines[1].Split(new char[] { ',' });
                        string[] localStateChunks = ReadLocalStateChunks(stateFileName);
                        var updateState = false;

                        string remoteDatabaseChecksum = remoteStateChunks.Length > 7 ? remoteStateChunks[7] : "MISSING-REMOTE";
                        string localDatabaseChecksum = localStateChunks.Length > 7 ? localStateChunks[7] : "MISSING-LOCAL";
                        if(remoteDatabaseChecksum != localDatabaseChecksum || !Provider.FileExists(databaseFileName)) {
                            updateState = true;
                            Provider.DownloadAndDecompressFile(DatabaseUrl, databaseTempName);

                            var standingDataManager = Factory.Resolve<IStandingDataManager>().Singleton;
                            lock(standingDataManager.Lock) {
                                Provider.MoveFile(databaseTempName, databaseFileName);
                            }
                            standingDataManager.Load();
                        }

                        if(updateState) {
                            Provider.WriteLines(stateTempName, stateLines);
                            Provider.MoveFile(stateTempName, stateFileName);
                        }
                    } finally {
                        _UpdateRunning = false;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the state information out of the state file.
        /// </summary>
        /// <param name="stateFileName"></param>
        /// <returns></returns>
        private string[] ReadLocalStateChunks(string stateFileName)
        {
            string[] result = new string[0];
            if(Provider.FileExists(stateFileName)) {
                string[] lines = Provider.ReadLines(stateFileName);
                if(lines.Length >= 2) result = lines[1].Split(new char[] { ',' });
            }

            return result;
        }
        #endregion
    }
}
