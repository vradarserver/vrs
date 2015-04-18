// Copyright © 2015 onwards, Andrew Whewell
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Resources;

namespace VirtualRadar.Plugin.CustomContent
{
    /// <summary>
    /// The class that picks up custom resources from the resource images folder and copies them
    /// into the static <see cref="Images"/> object.
    /// </summary>
    class CustomResourceImageManager
    {
        #region Private enum - FileImageType
        /// <summary>
        /// The type of image that can be loaded into <see cref="Images"/> by the manager.
        /// </summary>
        enum FileImageType
        {
            None,

            Bitmap,

            Icon,
        }
        #endregion

        #region Private class - ImageProperty
        /// <summary>
        /// Describes a single customisable image property on <see cref="Images"/>.
        /// </summary>
        class ImageProperty
        {
            /// <summary>
            /// Gets the PropertyInfo for the image on <see cref="Images"/>.
            /// </summary>
            public PropertyInfo PropertyInfo { get; private set; }

            /// <summary>
            /// Gets the PropertyInfo for the property on <see cref="Images"/> that indicates whether <see cref="Images"/>
            /// is holding a customised version of the image.
            /// </summary>
            public PropertyInfo IsCustomPropertyInfo { get; private set; }

            /// <summary>
            /// Gets the property name.
            /// </summary>
            public string Name { get { return PropertyInfo == null ? null : PropertyInfo.Name; } }

            private FileImageType _FileImageType;
            /// <summary>
            /// Gets the <see cref="FileImageType"/> associated with this property.
            /// </summary>
            public FileImageType FileImageType
            {
                get {
                    if(_FileImageType == CustomResourceImageManager.FileImageType.None && PropertyInfo != null) {
                        if(PropertyInfo.PropertyType == typeof(Bitmap))     _FileImageType = CustomResourceImageManager.FileImageType.Bitmap;
                        else if(PropertyInfo.PropertyType == typeof(Icon))  _FileImageType = CustomResourceImageManager.FileImageType.Icon;
                    }
                    return _FileImageType;
                }
            }

            /// <summary>
            /// Gets a value indicating that the image has been customised in <see cref="Images"/>.
            /// </summary>
            public bool HasBeenCustomised
            {
                get { return IsCustomPropertyInfo == null ? false : (bool)IsCustomPropertyInfo.GetValue(null, null); }
            }

            /// <summary>
            /// Gets the Bitmap assigned to the property in <see cref="Images"/> or null if it is either not a bitmap or no custom image has been assigned.
            /// </summary>
            public Bitmap Bitmap
            {
                get { return FileImageType != FileImageType.Bitmap || !HasBeenCustomised ? null : (Bitmap)PropertyInfo.GetValue(null, null); }
            }

            /// <summary>
            /// Gets the Icon assigned to the property in <see cref="Images"/> or null if it is either not an icon or no custom image has been assigned.
            /// </summary>
            public Icon Icon
            {
                get { return FileImageType != FileImageType.Icon || !HasBeenCustomised ? null : (Icon)PropertyInfo.GetValue(null, null); }
            }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="propertyInfo"></param>
            public ImageProperty(PropertyInfo propertyInfo)
            {
                PropertyInfo = propertyInfo;

                var customPropertyInfoName = String.Format("{0}_IsCustom", Name);
                var isCustomPropertyInfo = typeof(Images).GetProperty(customPropertyInfoName, BindingFlags.Public | BindingFlags.Static);
                IsCustomPropertyInfo = isCustomPropertyInfo;
            }

            /// <summary>
            /// Assigns a custom bitmap to the image.
            /// </summary>
            /// <param name="bitmap"></param>
            public void AssignBitmap(Bitmap bitmap)
            {
                if(FileImageType != FileImageType.Bitmap) throw new InvalidOperationException(String.Format("Cannot assign a bitmap to {0}", Name));
                PropertyInfo.SetValue(null, bitmap, null);
            }

            /// <summary>
            /// Assigns a custom icon to the image.
            /// </summary>
            /// <param name="icon"></param>
            public void AssignIcon(Icon icon)
            {
                if(FileImageType != FileImageType.Icon) throw new InvalidOperationException(String.Format("Cannot assign an icon to {0}", Name));
                PropertyInfo.SetValue(null, icon, null);
            }
        }
        #endregion

        #region Private class - UnusedImage
        /// <summary>
        /// A class that holds an unused image and the time that it became unused.
        /// </summary>
        class UnusedImage
        {
            /// <summary>
            /// Gets the unused image.
            /// </summary>
            public IDisposable DisposableImage { get; private set; }

            /// <summary>
            /// Gets the time that the image became unused.
            /// </summary>
            public DateTime TimestampUtc { get; private set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="unusedImage"></param>
            public UnusedImage(IDisposable disposableImage)
            {
                DisposableImage = disposableImage;
                TimestampUtc = DateTime.Now;
            }
        }
        #endregion

        #region Static fields
        /// <summary>
        /// The mininum number of minutes that the class will wait before it disposes of an unused image.
        /// </summary>
        public static readonly int MinutesBeforeImageDisposal = 1;
        #endregion

        #region Fields
        /// <summary>
        /// Locks access to the object while operations are ongoing.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// A map of normalised filenames to classes that represent each customisable image on <see cref="Images"/>.
        /// </summary>
        private Dictionary<string, ImageProperty> _ImageProperties = new Dictionary<string, ImageProperty>();

        /// <summary>
        /// A map of property names to bitmap objects that have been loaded into <see cref="Images"/>.
        /// </summary>
        private Dictionary<string, Bitmap> _CustomBitmaps = new Dictionary<string,Bitmap>();

        /// <summary>
        /// A map of property names to icon objects that have been loaded into <see cref="Images"/>.
        /// </summary>
        private Dictionary<string, Icon> _CustomIcons = new Dictionary<string,Icon>();

        /// <summary>
        /// A map of filenames to FileInfo objects, one for every custom image that has been loaded.
        /// </summary>
        private Dictionary<string, FileInfo> _FileInfos = new Dictionary<string,FileInfo>();

        /// <summary>
        /// A list of unused images and the times that they became unused.
        /// </summary>
        private List<UnusedImage> _UnusedImages = new List<UnusedImage>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the folder that holds the custom images.
        /// </summary>
        public string ResourceImagesFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="LoadCustomImages"/> will not perform any work if <see cref="Enabled"/> is false.
        /// </remarks>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the last time that a <see cref="LoadCustomImage"/> call was started or completed.
        /// </summary>
        public DateTime LastLoadCustomImagesUtc { get; private set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CustomResourceImageManager()
        {
            BuildImagePropertiesCache();
        }

        /// <summary>
        /// Fills the <see cref="_ImageProperties"/> collection.
        /// </summary>
        private void BuildImagePropertiesCache()
        {
            _ImageProperties.Clear();
            var propertyInfos = typeof(Images).GetProperties(BindingFlags.Public | BindingFlags.Static)
                                              .Where(r => r.PropertyType == typeof(Bitmap) || r.PropertyType == typeof(Icon))
                                              .ToArray();
            foreach(var propertyInfo in propertyInfos) {
                var imageProperty = new ImageProperty(propertyInfo);
                var propertyName = NormalisePropertyName(imageProperty.Name);
                _ImageProperties.Add(propertyName, imageProperty);
            }
        }
        #endregion

        #region LoadCustomImages
        /// <summary>
        /// Synchronises the image files in <see cref="ResourceImagesFolder"/> with the custom images in <see cref="Images"/>.
        /// </summary>
        /// <param name="blockThread"></param>
        public void LoadCustomImages(bool blockThread)
        {
            LastLoadCustomImagesUtc = DateTime.Now;
            if(blockThread) DoLoadCustomImagesOnBackgroundThread(null);
            else            ThreadPool.QueueUserWorkItem(DoLoadCustomImagesOnBackgroundThread);
        }

        /// <summary>
        /// Performs the work of loading the images on a background thread.
        /// </summary>
        /// <param name="unusedState"></param>
        private void DoLoadCustomImagesOnBackgroundThread(object unusedState)
        {
            try {
                try {
                    var folder = ResourceImagesFolder;
                    if(Enabled && !String.IsNullOrEmpty(folder) && Directory.Exists(folder)) {
                        lock(_SyncLock) {
                            LoadNewOrModifiedImageFiles(folder);
                            RemoveDeletedImageFiles(folder);
                        }
                    }

                    LastLoadCustomImagesUtc = DateTime.UtcNow;
                } catch(Exception ex) {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught unhandled exception while loading resource images in CustomContent plugin: {0}", ex);
                }
            } catch {
                // Never let any exceptions bubble out of this
            }
        }

        /// <summary>
        /// Fetches the FileInfos for every file in <see cref="ResourceImagesFolder"/> that corresponds to an image property. If
        /// the file has not been loaded before, or the FileInfo indicates that it has changed, then it is loaded and assigned
        /// to <see cref="Images"/>.
        /// </summary>
        /// <param name="folder"></param>
        private void LoadNewOrModifiedImageFiles(string folder)
        {
            foreach(var fileName in Directory.GetFiles(folder)) {
                try {
                    var normalisedFileName = NormaliseFileName(fileName);
                    var imageProperty = ImagePropertyForNormalisedFileName(normalisedFileName);
                    if(imageProperty != null && CanOverwriteImagesVersion(imageProperty)) {
                        var fileInfo = new FileInfo(fileName);
                        FileInfo existingFileInfo;
                        _FileInfos.TryGetValue(normalisedFileName, out existingFileInfo);
                        if(!CompareFileInfos(fileInfo, existingFileInfo)) {
                            if(existingFileInfo == null) _FileInfos[normalisedFileName] = fileInfo;
                            else                         _FileInfos.Add(normalisedFileName, fileInfo);

                            switch(imageProperty.FileImageType) {
                                case FileImageType.Bitmap:
                                    MarkExistingBitmapAsUnused(imageProperty);
                                    var bitmap = (Bitmap)Bitmap.FromFile(fileName);
                                    imageProperty.AssignBitmap(bitmap);
                                    _CustomBitmaps.Add(imageProperty.Name, bitmap);
                                    break;
                                case FileImageType.Icon:
                                    MarkExistingIconAsUnused(imageProperty);
                                    var icon = new Icon(fileName);
                                    imageProperty.AssignIcon(icon);
                                    _CustomIcons.Add(imageProperty.Name, icon);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                } catch(Exception ex) {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught exception while loading {0} in custom content resource image manager: {1}", fileName, ex);
                }
            }
        }

        /// <summary>
        /// Removes customised images for files that were previously loaded but no longer exist.
        /// </summary>
        /// <param name="folder"></param>
        private void RemoveDeletedImageFiles(string folder)
        {
            var unusedFileInfos = _FileInfos.Values.Where(r => !File.Exists(r.FullName)).ToArray();
            foreach(var unusedFileInfo in unusedFileInfos) {
                UnloadImageForFileInfo(unusedFileInfo);
            }
        }
        #endregion

        #region UnloadCustomImages
        /// <summary>
        /// Removes all customised images from <see cref="Images"/>. Note that if <see cref="Enabled"/> is true then
        /// a subsequent call to <see cref="LoadCustomImages"/> will reload them.
        /// </summary>
        /// <remarks>
        /// Unlike the <see cref="LoadCustomImages"/> and <see cref="DisposeOfUnusedImages"/> this method blocks until
        /// the operation has completed. If there is a long-running background operation then it may block for some time.
        /// </remarks>
        public void UnloadCustomImages()
        {
            lock(_SyncLock) {
                Enabled = false;

                var fileInfos = _FileInfos.Values.ToArray();
                foreach(var fileInfo in fileInfos) {
                    UnloadImageForFileInfo(fileInfo);
                }
            }
        }

        /// <summary>
        /// Removes all traces of an image for the FileInfo passed across.
        /// </summary>
        /// <param name="fileInfo"></param>
        private void UnloadImageForFileInfo(FileInfo fileInfo)
        {
            var normalisedFileName = NormaliseFileName(fileInfo.FullName);
            _FileInfos.Remove(normalisedFileName);

            var imageProperty = ImagePropertyForNormalisedFileName(normalisedFileName);
            if(imageProperty != null) {
                var canRemoveFromImages = CanOverwriteImagesVersion(imageProperty);
                switch(imageProperty.FileImageType) {
                    case FileImageType.Bitmap:
                        MarkExistingBitmapAsUnused(imageProperty);
                        if(canRemoveFromImages) imageProperty.AssignBitmap(null);
                        break;
                    case FileImageType.Icon:
                        MarkExistingIconAsUnused(imageProperty);
                        if(canRemoveFromImages) imageProperty.AssignIcon(null);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        #region UnusedImage handling - DisposeOfUnusedImages, MarkExistingBitmapAsUnused, MarkExistingIconAsUnused
        /// <summary>
        /// Disposes of unused bitmaps that have been unused for a period of time - enough time for
        /// consumers of <see cref="Images"/> to have finished using it.
        /// </summary>
        public void DisposeOfUnusedImages()
        {
            ThreadPool.QueueUserWorkItem(DoDisposeOfUnusedImagesOnBackgroundThread);
        }

        /// <summary>
        /// Does the work for <see cref="DisposeOfUnusedImages"/> on a background thread.
        /// </summary>
        /// <param name="unusedState"></param>
        /// <remarks>
        /// This should not take a long time. However the lock can be held by functions that are doing long-running
        /// file operations, so we don't want to block the calling thread while we wait on it.
        /// </remarks>
        private void DoDisposeOfUnusedImagesOnBackgroundThread(object unusedState)
        {
            try {
                lock(_SyncLock) {
                    try {
                        var threshold = DateTime.UtcNow.AddMinutes(-MinutesBeforeImageDisposal);
                        var unusedImages = _UnusedImages.Where(r => r.TimestampUtc <= threshold).ToArray();
                        foreach(var unusedImage in unusedImages) {
                            unusedImage.DisposableImage.Dispose();
                            _UnusedImages.Remove(unusedImage);
                        }
                    } catch(Exception ex) {
                        var log = Factory.Singleton.Resolve<ILog>().Singleton;
                        log.WriteLine("Caught exception while disposing of unused images in custom resource image manager: {0}", ex);
                    }
                }
            } catch {
                // Let no exception bubble up out of this.
            }
        }

        /// <summary>
        /// Removes the bitmap that we currently hold for <paramref name="imageProperty"/> (if any) from the custom dictionary
        /// and adds it to the unused image list.
        /// </summary>
        /// <param name="imageProperty"></param>
        private void MarkExistingBitmapAsUnused(ImageProperty imageProperty)
        {
            Bitmap existingBitmap;
            if(_CustomBitmaps.TryGetValue(imageProperty.Name, out existingBitmap)) {
                _CustomBitmaps.Remove(imageProperty.Name);
                _UnusedImages.Add(new UnusedImage(existingBitmap));
            }
        }

        /// <summary>
        /// Removes the icon that we currently hold for <paramref name="imageProperty"/> (if any) from the custom dictionary and 
        /// adds it to the unused image list.
        /// </summary>
        /// <param name="imageProperty"></param>
        private void MarkExistingIconAsUnused(ImageProperty imageProperty)
        {
            Icon existingIcon;
            if(_CustomIcons.TryGetValue(imageProperty.Name, out existingIcon)) {
                _CustomIcons.Remove(imageProperty.Name);
                _UnusedImages.Add(new UnusedImage(existingIcon));
            }
        }
        #endregion

        #region Normalisation - NormaliseFileName, NormalisePropertyName, NormalisePropertyNameFromFileName
        /// <summary>
        /// Returns a normalised version of the filename passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string NormaliseFileName(string fileName)
        {
            return (fileName ?? "").ToLowerInvariant();
        }

        /// <summary>
        /// Returns a normalised version of the property name passed across.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private string NormalisePropertyName(string propertyName)
        {
            return (propertyName ?? "").ToLowerInvariant();
        }

        /// <summary>
        /// Returns a normalised property name derived from a file name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string NormalisePropertyNameFromFileName(string fileName)
        {
            return NormalisePropertyName(Path.GetFileNameWithoutExtension(fileName ?? ""));
        }
        #endregion

        #region ImageProperty handling - ImagePropertyForNormalisedFileName, CanOverwriteImagesVersion
        /// <summary>
        /// Returns the <see cref="ImageProperty"/> associated with a filename.
        /// </summary>
        /// <param name="normalisedFileName"></param>
        /// <returns></returns>
        private ImageProperty ImagePropertyForNormalisedFileName(string normalisedFileName)
        {
            ImageProperty result = null;

            var normalisedPropertyName = NormalisePropertyNameFromFileName(normalisedFileName);
            if(!String.IsNullOrEmpty(normalisedPropertyName)) {
                if(_ImageProperties.TryGetValue(normalisedPropertyName, out result)) {
                    var fileImageType = GetFileImageTypeForNormalisedFileName(normalisedFileName);
                    if(fileImageType != result.FileImageType) {
                        result = null;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retrurns true if the Images version of the property has not been customised, or contains one of our images. Returns false
        /// if the image has been customised and we don't own the image that has been used. All other plugins have priority over us.
        /// </summary>
        /// <param name="imageProperty"></param>
        /// <returns></returns>
        private bool CanOverwriteImagesVersion(ImageProperty imageProperty)
        {
            var result = true;

            if(imageProperty.HasBeenCustomised) {
                object theirImage = null;
                object ourImage = null;

                switch(imageProperty.FileImageType) {
                    case FileImageType.Bitmap:
                        theirImage = imageProperty.Bitmap;
                        ourImage = _CustomBitmaps.ContainsKey(imageProperty.Name) ? _CustomBitmaps[imageProperty.Name] : null;
                        break;
                    case FileImageType.Icon:
                        theirImage = imageProperty.Icon;
                        ourImage = _CustomIcons.ContainsKey(imageProperty.Name) ? _CustomIcons[imageProperty.Name] : null;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                result = Object.ReferenceEquals(theirImage, ourImage);
            }

            return result;
        }
        #endregion

        #region File handling - CompareFileInfos, GetFileImageTypeForNormalisedFileName
        /// <summary>
        /// Returns true if <paramref name="fileInfo"/> represents the same file state as in <paramref name="existingFileInfo"/>.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="existingFileInfo"></param>
        /// <returns></returns>
        private bool CompareFileInfos(FileInfo fileInfo, FileInfo existingFileInfo)
        {
            var result = existingFileInfo != null;
            if(result) {
                result = fileInfo.CreationTimeUtc == existingFileInfo.CreationTimeUtc &&
                         fileInfo.LastWriteTimeUtc == existingFileInfo.LastWriteTimeUtc &&
                         fileInfo.Length == existingFileInfo.Length;
            }

            return result;
        }

        /// <summary>
        /// Returns the <see cref="FileImageType"/> for a normalised filename.
        /// </summary>
        /// <param name="normalisedFileName"></param>
        /// <returns></returns>
        private FileImageType GetFileImageTypeForNormalisedFileName(string normalisedFileName)
        {
            var result = FileImageType.None;

            switch(Path.GetExtension(normalisedFileName)) {
                case ".bmp":
                case ".gif":
                case ".jpg":
                case ".jpeg":
                case ".png":
                    result = FileImageType.Bitmap;
                    break;
                case ".ico":
                    result = FileImageType.Icon;
                    break;
            }

            return result;
        }
        #endregion
    }
}
