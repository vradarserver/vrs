using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TypeLite.TsModels;

namespace TypeLite.Net4 {
    /// <summary>
    /// ts document appender
    /// </summary>
    public class DocAppender : IDocAppender {
        /// <summary>
        /// xml doc provider cache
        /// </summary>
        protected Dictionary<string, XmlDocumentationProvider> _providers;

        public DocAppender() {
            _providers = new Dictionary<string, XmlDocumentationProvider>();
        }

        public void AppendClassDoc(ScriptBuilder sb, TsClass classModel, string className) {
            AppendModelDoc(sb, classModel.Type);
        }

        public void AppendPropertyDoc(ScriptBuilder sb, TsProperty property, string propertyName, string propertyType) {
            AppendMemberDoc(sb, property.MemberInfo);
        }

        public void AppendConstantModuleDoc(ScriptBuilder sb, TsProperty property, string propertyName, string propertyType) {
            AppendMemberDoc(sb, property.MemberInfo);
        }

        public void AppendEnumDoc(ScriptBuilder sb, TsEnum enumModel, string enumName) {
            AppendModelDoc(sb, enumModel.Type);
        }

        public void AppendEnumValueDoc(ScriptBuilder sb, TsEnumValue value) {
            AppendMemberDoc(sb, value.Field);
        }

        private XmlDocumentationProvider GetXmlDocProvider(Assembly assembly) {
            var xmlPath = FindXmlDocPath(assembly);

            System.Diagnostics.Debug.Print("GetXmlDocProvider {0}", xmlPath);
            if (xmlPath == null || File.Exists(xmlPath) == false) {
                System.Diagnostics.Debug.Print("GetXmlDocProvider not found");
                return null;
            }

            var key = xmlPath.ToLower();
            XmlDocumentationProvider provider;
            if (_providers.TryGetValue(key, out provider) == false) {
                provider = new XmlDocumentationProvider(xmlPath);
                _providers[key] = provider;
            }

            return provider;
        }

        private string FindXmlDocPath(Assembly assembly) {
            string asmPath = Uri.UnescapeDataString((new UriBuilder(assembly.CodeBase).Path));
            string xmlPath;

            // find same place
            xmlPath = Path.ChangeExtension(asmPath, ".xml");
            if (File.Exists(xmlPath)) {
                return xmlPath;
            }

            // find from Reference Assemblies
            // ex. C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1
            var baseDir = Path.Combine(
                String.Format("{0} (x86)", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)),
                @"Reference Assemblies\Microsoft\Framework\.NETFramework");

            var dirInfo = new DirectoryInfo(baseDir);
            if (dirInfo.Exists) {
                // find v4.* etc. directory
                var pattern = assembly.ImageRuntimeVersion.Substring(0, 2) + ".*";
                var verDirs = dirInfo.GetDirectories(pattern)
                    .OrderByDescending(dir => dir.Name)
                    .ToArray();

                // find xml in version directory
                var xmlName = Path.GetFileNameWithoutExtension(asmPath) + ".xml";
                foreach (var verDir in verDirs) {
                    xmlPath = Path.Combine(verDir.FullName, xmlName);
                    if (File.Exists(xmlPath)) {
                        return xmlPath;
                    }
                }
            }

            // nothing
            return null;
        }

        private void AppendModelDoc(ScriptBuilder sb, Type type) {
            var provider = GetXmlDocProvider(type.Assembly);
            if (provider == null) {
                return;
            }

            var doc = provider.GetDocumentation(type);
            if (string.IsNullOrEmpty(doc)) {
                return;
            }

            sb.AppendLine();
            sb.AppendFormatIndented("/**");
            sb.AppendLine();
            sb.AppendFormatIndented(" * {0}", doc);
            sb.AppendLine();
            sb.AppendFormatIndented(" */");
            sb.AppendLine();
        }

        private void AppendMemberDoc(ScriptBuilder sb, MemberInfo member) {
            var provider = GetXmlDocProvider(member.DeclaringType.Assembly);
            if (provider == null) {
                return;
            }

            var doc = provider.GetDocumentation(member);
            if (string.IsNullOrEmpty(doc)) {
                return;
            }

            sb.AppendLine();
            sb.AppendFormatIndented("/**");
            sb.AppendLine();
            sb.AppendFormatIndented(" * {0}", doc);
            sb.AppendLine();
            sb.AppendFormatIndented(" */");
            sb.AppendLine();
        }
    }
}
