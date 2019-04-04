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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// The default implementation of <see cref="IImageServerConfiguration"/>.
    /// </summary>
    class ImageServerConfiguration : IImageServerConfiguration
    {
        /// <summary>
        /// Describes a folder whose existence has been checked.
        /// </summary>
        class CheckedFolder
        {
            public string Folder;
            public bool Exists;
        }

        /// <summary>
        /// Protects writes to variables.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The object that returns settings objects for us.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        private CheckedFolder _CheckedOperatorFolder;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string OperatorFolder
        {
            get {
                var result = _SharedConfiguration.Get().BaseStationSettings.OperatorFlagsFolder;
                var checkedFolder = _CheckedOperatorFolder;

                if(checkedFolder == null || checkedFolder.Folder != result) {
                    checkedFolder = new CheckedFolder() {
                        Folder = result,
                    };
                    if(!String.IsNullOrEmpty(checkedFolder.Folder)) {
                        checkedFolder.Exists = Factory.Resolve<IFileSystemProvider>().DirectoryExists(checkedFolder.Folder);
                    }
                    lock(_SyncLock) {
                        _CheckedOperatorFolder = checkedFolder;
                    }
                }

                return checkedFolder.Exists ? result : null;
            }
        }

        private CheckedFolder _CheckedSilhouettesFolder;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string SilhouettesFolder
        {
            get {
                var result = _SharedConfiguration.Get().BaseStationSettings.SilhouettesFolder;
                var checkedFolder = _CheckedSilhouettesFolder;

                if(checkedFolder == null || checkedFolder.Folder != result) {
                    checkedFolder = new CheckedFolder() {
                        Folder = result,
                    };
                    if(!String.IsNullOrEmpty(checkedFolder.Folder)) {
                        checkedFolder.Exists = Factory.Resolve<IFileSystemProvider>().DirectoryExists(checkedFolder.Folder);
                    }
                    lock(_SyncLock) {
                        _CheckedSilhouettesFolder = checkedFolder;
                    }
                }

                return checkedFolder.Exists ? result : null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IImageFileManager ImageFileManager { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ImageServerConfiguration()
        {
            _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
            ImageFileManager = Factory.ResolveSingleton<IImageFileManager>();
        }
    }
}
