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
using VirtualRadar.Interface;
using System.IO;
using VirtualRadar.Interface.Settings;
using InterfaceFactory;
using System.Drawing;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftPictureManager"/>.
    /// </summary>
    sealed class AircraftPictureManager : IAircraftPictureManager
    {
        /// <summary>
        /// The log object that we'll use to record errors.
        /// </summary>
        private ILog _Log;

        /// <summary>
        /// The object that can fetch image dimensions for us.
        /// </summary>
        private IImageDimensionsFetcher _ImageDimensionsFetcher;

        private static readonly IAircraftPictureManager _Singleton = new AircraftPictureManager();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftPictureManager Singleton { get { return _Singleton; } }

        /// <summary>
        /// Creates the <see cref="_ImageDimensionsFetcher"/> if it hasn't already been created.
        /// </summary>
        private void CreateImageDimensionsFetcher()
        {
            if(_ImageDimensionsFetcher == null) {
                _ImageDimensionsFetcher = Factory.Singleton.Resolve<IImageDimensionsFetcher>();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="directoryCache"></param>
        /// <param name="icao24"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public PictureDetail FindPicture(IDirectoryCache directoryCache, string icao24, string registration)
        {
            PictureDetail result = null;

            var fileName = GetImageFileName(directoryCache, icao24, registration);
            if(!String.IsNullOrEmpty(fileName)) {
                var fileInfo = new FileInfo(fileName);
                if(fileInfo != null) {
                    CreateImageDimensionsFetcher();

                    var size = _ImageDimensionsFetcher.ReadDimensions(fileName);
                    result = new PictureDetail() {
                        FileName = fileName,
                        Width = size.Width,
                        Height = size.Height,
                        LastModifiedTime = fileInfo.LastWriteTimeUtc,
                        Length = fileInfo.Length,
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="directoryCache"></param>
        /// <param name="icao24"></param>
        /// <param name="registration"></param>
        /// <param name="existingDetail"></param>
        /// <returns></returns>
        public PictureDetail FindPicture(IDirectoryCache directoryCache, string icao24, string registration, PictureDetail existingDetail)
        {
            var result = existingDetail;

            if(existingDetail == null) result = FindPicture(directoryCache, icao24, registration);
            else {
                var fileName = GetImageFileName(directoryCache, icao24, registration);
                if(String.IsNullOrEmpty(fileName)) result = null;
                else {
                    var fileInfo = new FileInfo(fileName);
                    if(fileInfo == null) result = null;
                    else if(fileInfo.LastWriteTimeUtc != existingDetail.LastModifiedTime || fileInfo.Length != existingDetail.Length) {
                        result = FindPicture(directoryCache, icao24, registration);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="directoryCache"></param>
        /// <param name="icao24"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public Image LoadPicture(IDirectoryCache directoryCache, string icao24, string registration)
        {
            Image result = null;

            var fileName = GetImageFileName(directoryCache, icao24, registration);
            if(!String.IsNullOrEmpty(fileName)) result = LoadImage(fileName);

            return result;
        }

        /// <summary>
        /// Gets the filename of the aircraft's picture, if any.
        /// </summary>
        /// <param name="directoryCache"></param>
        /// <param name="icao24"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        private string GetImageFileName(IDirectoryCache directoryCache, string icao24, string registration)
        {
            string result = null;

            if(!String.IsNullOrEmpty(icao24)) {
                result = SearchForPicture(directoryCache, icao24, "jpg") ??
                         SearchForPicture(directoryCache, icao24, "jpeg") ??
                         SearchForPicture(directoryCache, icao24, "png") ??
                         SearchForPicture(directoryCache, icao24, "gif") ??
                         SearchForPicture(directoryCache, icao24, "bmp");
            }

            if(result == null && !String.IsNullOrEmpty(registration)) {
                var icaoCompliantRegistration = Describe.IcaoCompliantRegistration(registration);
                result = SearchForPicture(directoryCache, icaoCompliantRegistration, "jpg") ??
                         SearchForPicture(directoryCache, icaoCompliantRegistration, "jpeg") ??
                         SearchForPicture(directoryCache, icaoCompliantRegistration, "png") ??
                         SearchForPicture(directoryCache, icaoCompliantRegistration, "gif") ??
                         SearchForPicture(directoryCache, icaoCompliantRegistration, "bmp");
            }

            return result;
        }

        /// <summary>
        /// Loads the image at the filename passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private Image LoadImage(string fileName)
        {
            Image result = null;

            if(!String.IsNullOrEmpty(fileName)) {
                try {
                    result = Image.FromFile(fileName);
                } catch(Exception ex) {
                    if(_Log == null) _Log = Factory.Singleton.Resolve<ILog>().Singleton;
                    _Log.WriteLine("AircraftPictureManager caught an exception while loading {0}: {1}", fileName, ex.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the full path to the file if the file exists or null if it does not.
        /// </summary>
        /// <param name="directoryCache"></param>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        private string SearchForPicture(IDirectoryCache directoryCache, string fileName, string extension)
        {
            return directoryCache.GetFullPath(String.Format("{0}.{1}", fileName, extension));
        }
    }
}
