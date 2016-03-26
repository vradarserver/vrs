using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace TypeLite {

    public static class TsFiles {

        public static Regex regex = new Regex("declare module (\\S+) {(.*?})\r\n}", RegexOptions.Singleline | RegexOptions.CultureInvariant );

        /// <summary>
        /// Splits the current TypeScriptFluent output into several files, one per module
        /// </summary>
        /// <param name="path">The path where the files should be saved.</param>
        /// <returns></returns>
        public static string ToModules(this TypeScriptFluent typeScriptFluent, string path) {
            string typeScript = typeScriptFluent.Generate();
            return ToModules(typeScript, path);
        }

        /// <summary>
        /// Splits template output into several files, one per module
        /// </summary>
        /// <param name="typeScript">The current template output</param>
        /// <param name="path">The path where the files should be saved.</param>
        /// <returns></returns>
        public static string ToModules(string typeScript, string path) {
            StringBuilder result = new StringBuilder();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists) {
                directoryInfo.Create();
            }

            Dictionary<string, string> moduleNames = new Dictionary<string, string>();

            MatchCollection matchCollection = regex.Matches(typeScript);
            foreach (Match match in matchCollection) {
                if (match.Groups.Count == 3) {
                    string moduleName = match.Groups[1].Value;
                    moduleNames.Add(moduleName, moduleName.convertToIdentifier());
                }
            }

            foreach (Match match in matchCollection) {
                if (match.Groups.Count == 3) {
                    string moduleName = match.Groups[1].Value;
                    string moduleContent = match.Groups[2].Value;

                    foreach (KeyValuePair<string, string> namePair in moduleNames) {
                        if (moduleContent.Contains(namePair.Key)){
                            var header = String.Format("import {0} = require(\"{1}\");\r\n", namePair.Value, namePair.Key);
                            moduleContent = moduleContent.Replace(namePair.Key, namePair.Value);
                            moduleContent = header + moduleContent;
                        }
                    }

                    string fileName = String.Format("{0}\\{1}.ts", path, moduleName);
                    moduleContent.SaveToFile(fileName);

                    result.AppendLine(fileName);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Saves a string to a text file
        /// </summary>
        /// <param name="aString">The contents to save to the text file.</param>
        /// <param name="aFileName">The name of the file.</param>
        private static void SaveToFile(this string aString, string aFileName) {
            FileInfo fileInfo = new FileInfo(aFileName);
            if (fileInfo.Exists) {
                fileInfo.Delete();
            }
            using (FileStream stream = new FileStream(fileInfo.FullName, FileMode.Create)) {
                using (StreamWriter sw = new StreamWriter(stream)) {
                    sw.Write(aString);
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Converts a string to a JavaScript Identifier
        /// </summary>
        /// <param name="text">The text that should be converted</param>
        /// <returns></returns>
        private static string convertToIdentifier (this string text){
            string result = text;
            string firstLetter = text[0].ToString().ToLower();
            result = firstLetter + text.Substring(1);
            result = result.Replace(".", "");
            return result;
        }
    }
}
