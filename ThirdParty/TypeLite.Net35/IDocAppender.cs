using TypeLite.TsModels;

namespace TypeLite {
    /// <summary>
    /// interface of TS document appender.
    /// </summary>
    public interface IDocAppender {
        /// <summary>
        /// Append class document.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="classModel"></param>
        /// <param name="className"></param>
        void AppendClassDoc(ScriptBuilder sb, TsClass classModel, string className);

        /// <summary>
        /// Append property document.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="property"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        void AppendPropertyDoc(ScriptBuilder sb, TsProperty property, string propertyName, string propertyType);

        /// <summary>
        /// Append constant document.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="property"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        void AppendConstantModuleDoc(ScriptBuilder sb, TsProperty property, string propertyName, string propertyType);

        /// <summary>
        /// Append Enum document.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="enumModel"></param>
        /// <param name="enumName"></param>
        void AppendEnumDoc(ScriptBuilder sb, TsEnum enumModel, string enumName);

        /// <summary>
        /// Append Enum value document.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="value"></param>
        void AppendEnumValueDoc(ScriptBuilder sb, TsEnumValue value);
    }

    /// <summary>
    /// Dummy doc appender.
    /// </summary>
    public class NullDocAppender : IDocAppender {
        public void AppendClassDoc(ScriptBuilder sb, TsClass classModel, string className) {
        }

        public void AppendPropertyDoc(ScriptBuilder sb, TsProperty property, string propertyName, string propertyType) {
        }

        public void AppendConstantModuleDoc(ScriptBuilder sb, TsProperty property, string propertyName, string propertyType) {
        }

        public void AppendEnumDoc(ScriptBuilder sb, TsEnum enumModel, string enumName) {
        }

        public void AppendEnumValueDoc(ScriptBuilder sb, TsEnumValue value) {
        }
    }
}
