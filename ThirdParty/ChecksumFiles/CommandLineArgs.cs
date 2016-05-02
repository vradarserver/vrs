// Copyright © 2016 onwards, Andrew Whewell
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
using System.IO;
using System.Collections.Specialized;

namespace ChecksumFiles
{
    /// <summary>
    /// Standard command-line argument handling.
    /// </summary>
    class CommandLineArgs
    {
        #region Properties
        /// <summary>
        /// The command-line arguments broken into name-value pairs.
        /// </summary>
        public NameValueCollection Args { get; private set; }

        /// <summary>
        /// Gets an array of all of the arguments that were not associated with commands.
        /// </summary>
        public string[] UnnamedValues { get { return Args.GetValues("") ?? new string[]{}; } }

        /// <summary>
        /// The default action to take when a bad parameter is encountered.
        /// </summary>
        public Action<string> DefaultBadParameterAction { get; set; }

        /// <summary>
        /// Gets or sets a command value.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[string index]
        {
            get { return Args[index]; }
            set { Args[index] = value; }
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="spaceDelimitedValueKeys"></param>
        /// <param name="unnamedValueKey"></param>
        /// <param name="allowSpaceDelimitedValues"></param>
        public CommandLineArgs(string[] args, string[] spaceDelimitedValueKeys = null, string unnamedValueKey = "", bool allowSpaceDelimitedValues = true)
        {
            Args = ExtractNameValues(args, spaceDelimitedValueKeys, unnamedValueKey, allowSpaceDelimitedValues);
        }
        #endregion

        #region ExtractNameValues
        /// <summary>
        /// Breaks a set of command-line arguments down into name-value pairs.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="spaceDelimitedValueKeys"></param>
        /// <param name="unnamedValueKey"></param>
        /// <param name="allowSpaceDelimitedValues"></param>
        /// <returns></returns>
        public static NameValueCollection ExtractNameValues(string[] args, string[] spaceDelimitedValueKeys = null, string unnamedValueKey = "", bool allowSpaceDelimitedValues = true)
        {
            var result = new NameValueCollection();

            var spaceDelimitedValueNames = new List<string>((spaceDelimitedValueKeys ?? new string[] {}).Select(r => r.ToUpper()));
            for(var i = 0;i < args.Length;++i) {
                var arg = args[i];

                var key = arg.Length > 0 && (arg[0] == '-' || arg[0] == '/') ? arg.Substring(1) : null;
                var valueDelimiterPosn = key == null ? -1 : key.IndexOf(':');
                if(valueDelimiterPosn != -1) key = key.Substring(0, valueDelimiterPosn);
                var value = key == null ? arg : valueDelimiterPosn == -1 ? null : arg.Substring(valueDelimiterPosn + 2);
                if(value == null && allowSpaceDelimitedValues && spaceDelimitedValueNames.Contains(key.ToUpper()) && i + 1 < args.Length) value = args[++i];

                result.Add(key ?? unnamedValueKey, value ?? "");
            }

            return result;
        }
        #endregion

        #region Mandatory***, Optional***
        /// <summary>
        /// Returns a string value for a mandatory argument.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="badArgsAction"></param>
        /// <returns></returns>
        public string MandatoryString(string key, Action<string> badArgsAction = null)
        {
            var result = Args[key];
            if(result == null) HandleBadParameter(badArgsAction, "{0} not supplied", key);

            return result;
        }

        /// <summary>
        /// Returns an optional argument's value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string OptionalString(string key, string defaultValue = null)
        {
            return Args[key] ?? defaultValue;
        }

        /// <summary>
        /// Returns an int value for a mandatory argument.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="badArgsAction"></param>
        /// <returns></returns>
        public int MandatoryInt(string key, Action<string> badArgsAction = null)
        {
            int result;
            var text = MandatoryString(key, badArgsAction);
            if(!int.TryParse(text, out result)) HandleBadParameter(badArgsAction, "Cannot parse {0} value of '{1}'", key, text);

            return result;
        }

        /// <summary>
        /// Returns an int value for an optional argument.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int OptionalInt(string key, int defaultValue = 0)
        {
            var result = defaultValue;
            var text = OptionalString(key);
            if(text != null) result = int.Parse(text);

            return result;
        }

        /// <summary>
        /// Returns a bool value for an optional argument.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool OptionalBool(string key, bool defaultValue = false)
        {
            var result = defaultValue;
            var text = OptionalString(key);
            if(text != null) result = bool.Parse(text);

            return result;
        }

        /// <summary>
        /// Returns a DateTime value for an optional argument.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public DateTime OptionalDateTime(string key, DateTime defaultValue = default(DateTime))
        {
            DateTime result = defaultValue;
            var text = OptionalString(key);
            if(text != null) result = DateTime.Parse(text);

            return result;
        }

        /// <summary>
        /// Returns a decimal value for a mandatory argument.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="badArgsAction"></param>
        /// <returns></returns>
        public decimal MandatoryDecimal(string key, Action<string> badArgsAction = null)
        {
            decimal result;
            var text = MandatoryString(key, badArgsAction);
            if(!decimal.TryParse(text, out result)) HandleBadParameter(badArgsAction, "Cannot parse {0} value of '{1}'", key, text);

            return result;
        }

        /// <summary>
        /// Returns a double value for a mandatory argument.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="badArgsAction"></param>
        /// <returns></returns>
        public double MandatoryDouble(string key, Action<string> badArgsAction = null)
        {
            double result;
            var text = MandatoryString(key, badArgsAction);
            if(!double.TryParse(text, out result)) HandleBadParameter(badArgsAction, "Cannot parse {0} value of '{1}'", key, text);

            return result;
        }

        private void HandleBadParameter(Action<string> badArgsAction, string message)
        {
            if(badArgsAction == null) badArgsAction = DefaultBadParameterAction;
            if(badArgsAction == null) throw new ArgumentException(message);
            else badArgsAction(message);
        }

        private void HandleBadParameter(Action<string> badArgsAction, string format, params object[] args)
        {
            HandleBadParameter(badArgsAction, String.Format(format, args));
        }
        #endregion

        #region HasUnnamedValue, HasKeyWithNoValue
        /// <summary>
        /// Returns true if the unnamed value is present.
        /// </summary>
        /// <param name="unnamedValue"></param>
        /// <param name="unnamedKey"></param>
        /// <returns></returns>
        public bool HasUnnamedValue(string unnamedValue, string unnamedKey = "")
        {
            var unnamedValues = Args.GetValues(unnamedKey) ?? new string[] {};
            return unnamedValues.Any(r => unnamedValue.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns true if the key is present with no value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKeyWithNoValue(string key)
        {
            return Args[key] != null;
        }
        #endregion

        #region ExpandWildcards
        /// <summary>
        /// Returns a collection of expanded filename arguments.
        /// </summary>
        /// <param name="arg">The command-line parameter representing a filespec.</param>
        /// <param name="recursive">True if the expansion should be applied recursively.</param>
        /// <returns></returns>
        public static IEnumerable<string> ExpandWildcards(string arg, bool recursive = false)
        {
            IEnumerable<string> result = null;
    
            if(!String.IsNullOrEmpty(arg)) {
                if(!arg.Contains('*') && !arg.Contains('?')) result = new string[] { arg };
                else {
                    var path = Path.GetDirectoryName(arg);
                    if(String.IsNullOrEmpty(path)) path = Directory.GetCurrentDirectory();

                    var spec = Path.GetFileName(arg);
                    var specExtension = Path.GetExtension(spec);
                    if(specExtension.Contains('*') || specExtension.Contains('?')) specExtension = null;

                    var coarseMatch = Directory.GetFiles(path, spec, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    result = specExtension == null ? coarseMatch : coarseMatch.Where(r => Path.GetExtension(r).Equals(specExtension, StringComparison.OrdinalIgnoreCase));
                }
            }
    
            return result ?? new string[] { };
        }
        #endregion
    }
}
