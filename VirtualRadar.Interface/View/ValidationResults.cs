using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// Describes the results of the validation of an entire form or a single field.
    /// </summary>
    public class ValidationResults
    {
        /// <summary>
        /// Gets a collection of every error and warning found.
        /// </summary>
        public List<ValidationResult> Results { get; private set; }

        /// <summary>
        /// Gets a value indicating that validation was only run over a subset of all available fields.
        /// </summary>
        public bool IsPartialValidation { get; private set; }

        /// <summary>
        /// Gets a collection of validation results that indicate the records and fields that were
        /// examined in a partial validation. The content of this collection is undefined when
        /// <see cref="IsPartialValidation"/> is false.
        /// </summary>
        /// <remarks>
        /// Only the Record and Field properties are set on the validation results held here.
        /// </remarks>
        public List<ValidationResult> PartialValidationFields { get; private set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Results"/> contains any errors.
        /// </summary>
        public bool HasErrors { get { return Results.Any(r => !r.IsWarning); } }

        /// <summary>
        /// Gets an array of all of the errors from <see cref="Results"/>.
        /// </summary>
        public ValidationResult[] Errors { get { return Results.Where(r => !r.IsWarning).ToArray(); } }

        /// <summary>
        /// Gets a value indicating that <see cref="Results"/> contains at least one warning.
        /// </summary>
        public bool HasWarnings { get { return Results.Any(r => r.IsWarning); } }

        /// <summary>
        /// Gets an array of all of the warnings found in <see cref="Results"/>.
        /// </summary>
        public ValidationResult[] Warnings { get { return Results.Where(r => r.IsWarning).ToArray(); } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="isPartialValidation"></param>
        public ValidationResults(bool isPartialValidation)
        {
            Results = new List<ValidationResult>();
            IsPartialValidation = isPartialValidation;
            PartialValidationFields = new List<ValidationResult>();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}{1} error(s), {2} warning(s)",
                IsPartialValidation ? String.Format("[PARTIAL {0} FIELD(S)] ", PartialValidationFields.Count) : "",
                Errors.Length,
                Warnings.Length
            );
        }
    }
}
