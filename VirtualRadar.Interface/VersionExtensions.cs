// Hoiked from Microsoft's reference source and then tarted up a little bit to get it
// working in .NET 3.5
//
// https://referencesource.microsoft.com/#mscorlib/system/version.cs,0cbe16a765a5ef7e
//
//
// Portions copyright (c) Microsoft Corporation.  All rights reserved.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Version helpers and extensions.
    /// </summary>
    public static class VersionExtensions
    {
        /// <summary>
        /// Tries to parse the input string into a version
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        // Copied from reference source
        public static bool TryParse(string input, out Version result) {
            VersionResult r = new VersionResult();
            r.Init("input", false);
            bool b = TryParseVersion(input, ref r);
            result = r.m_parsedVersion;
            return b;
        }

        // Copied from reference source.
        private static readonly char[] SeparatorsArray = new char[] { '.' };

        // Copied from reference source
        internal enum ParseFailureKind { 
            ArgumentNullException, 
            ArgumentException, 
            ArgumentOutOfRangeException, 
            FormatException 
        }

        // Mostly copied from reference source
        internal struct VersionResult {
            internal Version m_parsedVersion;
            internal ParseFailureKind m_failure;
            internal string m_exceptionArgument;
            internal string m_argumentName;
            internal bool m_canThrow;
 
            internal void Init(string argumentName, bool canThrow) {
                m_canThrow = canThrow;
                m_argumentName = argumentName;
            }
 
            internal void SetFailure(ParseFailureKind failure) {
                SetFailure(failure, String.Empty);
            }
 
            internal void SetFailure(ParseFailureKind failure, string argument) {
                m_failure = failure;
                m_exceptionArgument = argument;
                if (m_canThrow) {
                    throw GetVersionParseException();
                }
            }
 
            internal Exception GetVersionParseException() {
                switch (m_failure) {
                    case ParseFailureKind.ArgumentNullException:
                        return new ArgumentNullException(m_argumentName);
                    case ParseFailureKind.ArgumentException:
                        return new ArgumentException("VersionString");
                    case ParseFailureKind.ArgumentOutOfRangeException:
                        return new ArgumentOutOfRangeException(m_exceptionArgument, "Version out of range");
                    case ParseFailureKind.FormatException:
                        // Regenerate the FormatException as would be thrown by Int32.Parse()
                        try {
                            Int32.Parse(m_exceptionArgument, CultureInfo.InvariantCulture);
                        } catch (FormatException e) {
                            return e;
                        } catch (OverflowException e) {
                            return e;
                        }
                        return new FormatException("Invalid string");
                    default:
                        return new ArgumentException("Version string");
                }
            }
 
        }

        // Copied from reference source
        internal static bool TryParseVersion(string version, ref VersionResult result) {
            int major, minor, build, revision;
 
            if ((Object)version == null) {
                result.SetFailure(ParseFailureKind.ArgumentNullException);
                return false;
            }
 
            String[] parsedComponents = version.Split(SeparatorsArray);
            int parsedComponentsLength = parsedComponents.Length;
            if ((parsedComponentsLength < 2) || (parsedComponentsLength > 4)) {
                result.SetFailure(ParseFailureKind.ArgumentException);
                return false;
            }
 
            if (!TryParseComponent(parsedComponents[0], "version", ref result, out major)) {
                return false;
            }
 
            if (!TryParseComponent(parsedComponents[1], "version", ref result, out minor)) {
                return false;
            }
 
            parsedComponentsLength -= 2;
 
            if (parsedComponentsLength > 0) {
                if (!TryParseComponent(parsedComponents[2], "build", ref result, out build)) {
                    return false;
                }
 
                parsedComponentsLength--;
 
                if (parsedComponentsLength > 0) {
                    if (!TryParseComponent(parsedComponents[3], "revision", ref result, out revision)) {
                        return false;
                    } else {
                        result.m_parsedVersion = new Version(major, minor, build, revision);
                    }
                } else {
                    result.m_parsedVersion = new Version(major, minor, build);
                }
            } else {
                result.m_parsedVersion = new Version(major, minor);
            }
 
            return true;
        }

        // Copied from reference source
        private static bool TryParseComponent(string component, string componentName, ref VersionResult result, out int parsedComponent) {
            if (!Int32.TryParse(component, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedComponent)) {
                result.SetFailure(ParseFailureKind.FormatException, component);
                return false;
            }
 
            if (parsedComponent < 0) {
                result.SetFailure(ParseFailureKind.ArgumentOutOfRangeException, componentName);
                return false;
            }
 
            return true;
        }
    }
}
