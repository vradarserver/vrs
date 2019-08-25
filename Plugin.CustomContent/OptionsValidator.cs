using InterfaceFactory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.CustomContent
{
    class OptionsValidator
    {
        public ValidationResults Validate(Options options)
        {
            var result = new ValidationResults(isPartialValidation: false);

            if(options.Enabled) {
                ValidateInjectionSettings(options, result);
                ValidateSiteRootFolder(options, result);
                ValidateResourceImagesFolder(options, result);
            }

            return result;
        }

        private static void ValidateInjectionSettings(Options options, ValidationResults result)
        {
            foreach(var injectSettings in options.InjectSettings) {
                ValidateInjectionSettingsFile(result, injectSettings);
                ValidateInjectionSettingsPathAndFile(result, injectSettings);
            }
        }

        private static void ValidateInjectionSettingsFile(ValidationResults result, InjectSettings injectSettings)
        {
            string message = null;

            try {
                if(String.IsNullOrEmpty(injectSettings.File)) {
                    message = CustomContentStrings.FileNameRequired;
                } else if(!File.Exists(injectSettings.File)) {
                    message = String.Format(CustomContentStrings.FileDoesNotExist, injectSettings.File);
                }
            } catch(Exception ex) {
                Factory.Resolve<ILog>().Singleton.WriteLine("Caught exception while checking injection file: {0}", ex.ToString());
                message = String.Format(CustomContentStrings.ErrorCheckingFileName, ex.Message);
            }

            if(message != null) {
                result.Results.Add(new ValidationResult(injectSettings, ValidationField.Name, message));
            }
        }

        private static void ValidateInjectionSettingsPathAndFile(ValidationResults result, InjectSettings injectSettings)
        {
            string message = null;

            if(String.IsNullOrEmpty(injectSettings.PathAndFile)) {
                message = CustomContentStrings.PathAndFileRequired;
            } else if(injectSettings.PathAndFile != "*") {
                if(injectSettings.PathAndFile[0] != '/') {
                    message = CustomContentStrings.PathAndFileMissingRoot;
                } else if(!injectSettings.PathAndFile.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                       && !injectSettings.PathAndFile.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)) {
                    message = CustomContentStrings.PathAndFileMissingExtension;
                }
            }

            if(message != null) {
                result.Results.Add(new ValidationResult(injectSettings, ValidationField.PathAndFile, message));
            }
        }

        private static void ValidateSiteRootFolder(Options options, ValidationResults result)
        {
            if(!String.IsNullOrEmpty(options.SiteRootFolder)) {
                if(ValidateFolderExists(options.SiteRootFolder, ValidationField.SiteRootFolder, result)) {
                    string message = null;

                    try {
                        var currentSiteRoot = Plugin.Singleton.SiteRoot.Folder;
                        var currentFullPath = String.IsNullOrEmpty(currentSiteRoot) ? null : NormalisePath(Path.GetFullPath(currentSiteRoot));
                        var siteRootFullPath = NormalisePath(Path.GetFullPath(options.SiteRootFolder));

                        if(!String.Equals(currentFullPath, siteRootFullPath, StringComparison.OrdinalIgnoreCase)) {
                            if(Plugin.Singleton.WebSite.GetSiteRootFolders().Any(r => String.Equals(NormalisePath(r), siteRootFullPath, StringComparison.OrdinalIgnoreCase))) {
                                message = String.Format(CustomContentStrings.DirectoryAlreadyInUse, siteRootFullPath);
                            }
                        }
                    } catch(Exception ex) {
                        Factory.Resolve<ILog>().Singleton.WriteLine("Caught exception while checking custom content site root folder: {0}", ex.ToString());
                        message = String.Format(CustomContentStrings.ErrorCheckingFolder, ex.Message);
                    }

                    if(message != null) {
                        result.Results.Add(new ValidationResult(ValidationField.SiteRootFolder, message));
                    }
                }
            }
        }

        private static void ValidateResourceImagesFolder(Options options, ValidationResults result)
        {
            ValidateFolderExists(options.ResourceImagesFolder, ValidationField.ResourceImagesFolder, result);
        }

        private static string NormalisePath(string path)
        {
            return String.IsNullOrEmpty(path) ? ""
                    : path[path.Length - 1] != Path.DirectorySeparatorChar ? path + Path.DirectorySeparatorChar
                    : path;
        }

        private static bool ValidateFolderExists(string optionalFolder, ValidationField validationField, ValidationResults result)
        {
            string message = null;

            if(!String.IsNullOrEmpty(optionalFolder)) {

                try {
                    if(!Directory.Exists(optionalFolder)) {
                         message = String.Format(CustomContentStrings.DirectoryDoesNotExist, optionalFolder);
                    }
                } catch(Exception ex) {
                    Factory.Resolve<ILog>().Singleton.WriteLine("Caught exception while checking custom content folder: {0}", ex.ToString());
                    message = String.Format(CustomContentStrings.ErrorCheckingFolder, ex.Message);
                }

                if(!String.IsNullOrEmpty(message)) {
                    result.Results.Add(new ValidationResult(validationField, message));
                }
            }

            return message == null;
        }
    }
}
