using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite {
    /// <summary>
    /// Creates outptut script in memory 
    /// </summary>
    public class ScriptBuilder {
        private StringBuilder _internalBuilder;

        /// <summary>
        /// Gets or sets string for the single indentation level.
        /// </summary>
        public string IndentationString { get; set; }

        /// <summary>
        /// Get current number of indentation levels.
        /// </summary>
        public int IndentationLevels { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ScriptBuilder class
        /// </summary>
        public ScriptBuilder() : this("    ") {
        }

        /// <summary>
        /// Initializes a new instace of the ScriptBuilder class with the specific characters for indentation.
        /// </summary>
        /// <param name="indentation">The characters for indentation.</param>
        public ScriptBuilder(string indentation) {
            _internalBuilder = new StringBuilder();
            this.IndentationString = indentation;
        }

        /// <summary>
        /// Inceases indentation by one level.
        /// </summary>
        /// <returns>An IndentationLevel object that represents a scope of the indentation.</returns>
        public IndentationLevelScope IncreaseIndentation() {
            this.IndentationLevels += 1;
            return new IndentationLevelScope(this);
        }

        /// <summary>
        /// Decreases indentation by one level.
        /// </summary>
        internal void DecreaseIndentation(IndentationLevelScope indentationScope) {
            if (indentationScope == null) {
                throw new ArgumentNullException();
            }

            if (this.IndentationLevels <= 0) {
                throw new InvalidOperationException("Indentation level is already set to zero.");
            }

            this.IndentationLevels -= 1;
        }

        /// <summary>
        /// Appends the specified string to this instance
        /// </summary>
        /// <param name="value">the string to append</param>
        public void Append(string value) {
            _internalBuilder.Append(value);
        }

        /// <summary>
        /// Appends default line delimeter.
        /// </summary>
        public void AppendLine() {
            _internalBuilder.AppendLine();
        }

        /// <summary>
        /// Appends the specific string ot this instace of the ScriptBuilder followed by the default new line delimiter.
        /// </summary>
        /// <param name="value">the string to append</param>
        public void AppendLine(string value) {
            _internalBuilder.AppendLine(value);
        }

        /// <summary>
        /// Appends a string returned by processing a composite format string. 
        /// </summary>
        /// <param name="format">A composite format string</param>
        /// <param name="args">An array of objects to format</param>
        public void AppendFormat(string format, params object[] args) {
            _internalBuilder.AppendFormat(format, args);
        }

        /// <summary>
        /// Appends indentation.
        /// </summary>
        public void AppendIndentation() {
            for(var i = 0;i < IndentationLevels;++i) {
                _internalBuilder.Append(IndentationString);
            }
        }

        /// <summary>
        /// Appends an indented string to the current instance of script builder.
        /// </summary>
        /// <param name="value">The string to append.</param>
        public void AppendIndented(string value) {
            this.AppendIndentation();
            this.Append(value);
        }
        
        /// <summary>
        /// Appends a indented string returned by processing a composite format string. 
        /// </summary>
        /// <param name="format">A composite format string</param>
        /// <param name="args">An array of objects to format</param>
        public void AppendFormatIndented(string format, params object[] args) {
            this.AppendIndentation();
            this.AppendFormat(format, args);
        }

        /// <summary>
        /// Appends an indented string followed by the default new line delimiter.
        /// </summary>
        /// <param name="value">The string to append</param>
        public void AppendLineIndented(string value) {
            this.AppendIndentation();
            this.AppendLine(value);
        }

        /// <summary>
        /// Converts value of this builder to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return _internalBuilder.ToString();
        }



    }
}
