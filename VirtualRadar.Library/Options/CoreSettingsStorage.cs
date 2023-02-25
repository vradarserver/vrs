// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Options;

namespace VirtualRadar.Library.Options
{
    internal class CoreSettingsStorage : ICoreSettingsStorage
    {
        private readonly EnvironmentOptions _EnvironmentOptions;
        private readonly IFileSystem _FileSystem;
        private object _SyncLock = new();

        public string ConfigurationFileName => "core-configuration.json";

        public string ConfigurationFullPath => _FileSystem.Combine(_EnvironmentOptions.WorkingFolder, ConfigurationFileName);

        public CoreSettingsStorage(IOptions<EnvironmentOptions> environmentOptions, IFileSystem fileSystem)
        {
            _EnvironmentOptions = environmentOptions.Value;
            _FileSystem = fileSystem;

            CreateIfMissing();
        }

        public CoreOptions Load()
        {
            CoreOptions result = null;

            if(_FileSystem.FileExists(ConfigurationFullPath)) {
                var jsonText = _FileSystem.ReadAllText(ConfigurationFullPath);
                result = JsonConvert.DeserializeObject<CoreOptions>(jsonText);
            }

            return result ?? new CoreOptions();
        }

        public void Save(CoreOptions options)
        {
            lock(_SyncLock) {
                if(_FileSystem.FileExists(ConfigurationFullPath)) {
                    var currentOptions = Load();
                    if(currentOptions.SaveCounter != options.SaveCounter) {
                        throw new ConflictingUpdateException($"An attempt was made to save configuration options after they had been changed elsewhere. Reapply your edits and try again.");
                    }
                }

                var jsonText = JsonConvert.SerializeObject(options);
                _FileSystem.WriteAllText(ConfigurationFullPath, jsonText);
            }
        }

        public bool CreateIfMissing()
        {
            var result = !_FileSystem.FileExists(ConfigurationFullPath);

            if(result) {
                Save(new());
            }

            return result;
        }
    }
}
