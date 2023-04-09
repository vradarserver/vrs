// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Options;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.Types;

namespace VirtualRadar.Database.SQLite.StandingData
{
    /// <summary>
    /// The default implementation of <see cref="IStandingDataUpdater"/>.
    /// </summary>
    [Obsolete("Do not create instances of this directly. Use dependency injection instead. This is only public so that it can be unit tested")]
    public class StandingDataUpdater : IStandingDataUpdater
    {
        readonly EnvironmentOptions _EnvironmentOptions;
        readonly IHttpClientService _HttpClient;
        readonly IFileSystem _FileSystem;
        readonly IWebAddressManager _WebAddressManager;
        readonly IStandingDataManager _StandingDataManager;

        /// <summary>
        /// The name of the file that describes the dates and state of the other files.
        /// </summary>
        public const string StateFileName = "FlightNumberCoverage.csv";

        /// <summary>
        /// The name of the file that describes the dates and state of the other files.
        /// </summary>
        public const string StateTempName = StateFileName + ".tmp";

        /// <summary>
        /// The filename of the standing data database.
        /// </summary>
        private const string DatabaseFileName = "StandingData.sqb";

        /// <summary>
        /// The temporary filename of the standing data database.
        /// </summary>
        private const string DatabaseTempName = DatabaseFileName + ".tmp";

        /// <summary>
        /// The object that <see cref="Update"/> locks to prevent two threads updating simultaneously.
        /// </summary>
        private static object _UpdateLock = new object();

        /// <summary>
        /// True if <see cref="Update"/> is running on a thread somewhere.
        /// </summary>
        private bool _UpdateRunning;

        /// <summary>
        /// The URL of the file holding the standing data database.
        /// </summary>
        private string _DatabaseUrl { get; }

        /// <summary>
        /// The URL of the file that describes the dates and state of the other files.
        /// </summary>
        private string _StateFileUrl { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="environmentOptions"></param>
        /// <param name="fileSystem"></param>
        /// <param name="httpClient"></param>
        /// <param name="webAddressManager"></param>
        /// <param name="standingDataManager"></param>
        public StandingDataUpdater(
            IOptions<EnvironmentOptions> environmentOptions,
            IFileSystem fileSystem,
            IHttpClientService httpClient,
            IWebAddressManager webAddressManager,
            IStandingDataManager standingDataManager
        )
        {
            _EnvironmentOptions = environmentOptions.Value;
            _FileSystem = fileSystem;
            _HttpClient = httpClient;
            _WebAddressManager = webAddressManager;
            _StandingDataManager = standingDataManager;

            _DatabaseUrl =  _WebAddressManager.RegisterAddress("vrs-sdm-database",    "http://www.virtualradarserver.co.uk/Files/StandingData.sqb.gz");
            _StateFileUrl = _WebAddressManager.RegisterAddress("vrs-sdm-state-file",  "http://www.virtualradarserver.co.uk/Files/FlightNumberCoverage.csv");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool DataIsOld()
        {
            var stateFilename = _FileSystem.Combine(
                _EnvironmentOptions.WorkingFolder,
                StateFileName
            );

            var result = !_FileSystem.FileExists(stateFilename);
            if(!result) {
                var remoteContent = _HttpClient.GetString(_StateFileUrl).SplitIntoLines();
                var localContent = _FileSystem.ReadAllLines(stateFilename);
                result = remoteContent.Count < 2 || localContent.Length < 2;
                if(!result) {
                    result = remoteContent[1] != localContent[1];
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Update()
        {
            if(!_UpdateRunning) {
                lock(_UpdateLock) {
                    _UpdateRunning = true;
                    try {
                        var folder = _EnvironmentOptions.WorkingFolder;
                        var stateFileName = _FileSystem.Combine(folder, StateFileName);
                        var stateTempName = _FileSystem.Combine(folder, StateTempName);
                        var databaseFileName = _FileSystem.Combine(folder, DatabaseFileName);
                        var databaseTempName = _FileSystem.Combine(folder, DatabaseTempName);

                        var stateLines = _HttpClient.GetString(_StateFileUrl).SplitIntoLines();
                        var remoteStateChunks = stateLines[1].Split(new char[] { ',' });
                        var localStateChunks = ReadLocalStateChunks(stateFileName);
                        var updateState = false;

                        var remoteDatabaseChecksum = remoteStateChunks.Length > 7
                            ? remoteStateChunks[7]
                            : "MISSING-REMOTE";
                        var localDatabaseChecksum = localStateChunks.Length > 7
                            ? localStateChunks[7]
                            : "MISSING-LOCAL";

                        if(remoteDatabaseChecksum != localDatabaseChecksum || !_FileSystem.FileExists(databaseFileName)) {
                            updateState = true;
                            DownloadAndDecompressGZipFile(_DatabaseUrl, databaseTempName);

                            _StandingDataManager.Lock(_ => {
                                MoveTemporaryToLive(databaseTempName, databaseFileName);
                            });
                            _StandingDataManager.Load();
                        }

                        if(updateState) {
                            _FileSystem.WriteAllLines(stateTempName, stateLines);
                            MoveTemporaryToLive(stateTempName, stateFileName);
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
            var result = Array.Empty<string>();
            if(_FileSystem.FileExists(stateFileName)) {
                var lines = _FileSystem.ReadAllLines(stateFileName);
                if(lines.Length >= 2) {
                    result = lines[1].Split(new char[] { ',' });
                }
            }

            return result;
        }

        /// <summary>
        /// Downloads a file from a URL and writes it to the file passed across. We know in advance that
        /// the file is a GZIP compressed file and decompress it at the same time as downloading it.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        private void DownloadAndDecompressGZipFile(string url, string fileName)
        {
            using(var remoteStream = _HttpClient.GetStream(url)) {
                using(var gzipStream = new GZipStream(remoteStream, CompressionMode.Decompress)) {
                    using(var fileStream = _FileSystem.OpenFileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                        gzipStream.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Reproduces the behaviour of the original updater's move function.
        /// </summary>
        /// <param name="temporaryFileName"></param>
        /// <param name="liveFileName"></param>
        private void MoveTemporaryToLive(string temporaryFileName, string liveFileName)
        {
            var path = _FileSystem.GetDirectory(liveFileName);
            _FileSystem.CreateDirectoryIfNotExists(path);

            if(_FileSystem.FileExists(liveFileName)) {
                _FileSystem.DeleteFile(liveFileName);
            }

            _FileSystem.MoveFile(temporaryFileName, liveFileName, overwrite: false);
        }
    }
}
