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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// A class that can help with copying validation results from a presenter to a web form view model.
    /// </summary>
    public class ValidationModelHelper
    {
        /// <summary>
        /// Describes an instance of a field within a view model.
        /// </summary>
        class FieldInstance
        {
            /// <summary>
            /// The attribute that was used to tag the property.
            /// </summary>
            public ValidationModelFieldAttribute Attribute { get; set; }

            /// <summary>
            /// The property that was tagged.
            /// </summary>
            public PropertyInfo Property { get; set; }

            /// <summary>
            /// All of the instances that this field was seen on.
            /// </summary>
            public List<object> ViewModelObjects { get; private set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="property"></param>
            /// <param name="attribute"></param>
            public FieldInstance(PropertyInfo property, ValidationModelFieldAttribute attribute)
            {
                Property = property;
                Attribute = attribute;
                ViewModelObjects = new List<object>();
            }

            /// <summary>
            /// Returns the ValidationModelField associated with object. If a non-null view model object is passed
            /// across then it is always used, otherwise the <see cref="ViewModelObject"/> value is used.
            /// </summary>
            /// <param name="viewModelObject"></param>
            /// <returns></returns>
            public ValidationModelField GetField(object viewModelObject)
            {
                ValidationModelField result;

                try {
                    var obj = viewModelObject ?? ViewModelObjects.Single();
                    result = Property.GetValue(obj, null) as ValidationModelField;
                    if(result == null) {
                        result = new ValidationModelField();
                        Property.SetValue(obj, result, null);
                    }
                } catch(Exception ex) {
                    throw new InvalidOperationException(String.Format("Could not set property for validation field {0} on {1} (viewModelObject={2}, {3} discovered)", Attribute.Field, Property.Name, viewModelObject, ViewModelObjects.Count), ex);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the method that can convert from a <see cref="ValidationResult"/>.Record object to a view model object.
        /// </summary>
        /// <remarks>
        /// This is only required if the presenter returns <see cref="ValidationResult"/>s that use the Record
        /// property.
        /// </remarks>
        public Func<ValidationResult, object> FindViewModelForRecord { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ValidationModelHelper() : this(null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="findViewModelForRecord"></param>
        public ValidationModelHelper(Func<ValidationResult, object> findViewModelForRecord)
        {
            FindViewModelForRecord = findViewModelForRecord;
        }

        /// <summary>
        /// Applies the validation results to the view model.
        /// </summary>
        /// <param name="validationResults"></param>
        /// <param name="viewModelRoot"></param>
        public void ApplyValidationResults(ValidationResults validationResults, object viewModelRoot)
        {
            var fieldInstances = FindAllFields(viewModelRoot);

            if(!validationResults.IsPartialValidation) {
                ClearAllValidationModelFields(fieldInstances);

                foreach(var validationResult in validationResults.Results.Where(r => !String.IsNullOrEmpty(r.Message))) {
                    var viewModelObject = FindViewModelForRecord == null ? null : FindViewModelForRecord(validationResult);
                    ApplyValidationResult(fieldInstances, viewModelObject, validationResult);
                }
            } else {
                foreach(var partialValidation in validationResults.PartialValidationFields) {
                    var viewModelObject = FindViewModelForRecord == null ? null : FindViewModelForRecord(partialValidation);
                    var validationResult = validationResults.Results.FirstOrDefault(r => Object.ReferenceEquals(partialValidation.Record, r.Record) && partialValidation.Field == r.Field);
                    ApplyValidationResult(fieldInstances, viewModelObject, validationResult ?? partialValidation);
                }
            }
        }

        private void ApplyValidationResult(Dictionary<ValidationField, FieldInstance> fieldInstances, object viewModelObject, ValidationResult validationResult)
        {
            FieldInstance fieldInstance;
            if(fieldInstances.TryGetValue(validationResult.Field, out fieldInstance)) {
                var field = fieldInstance.GetField(viewModelObject);
                field.SetMessage(validationResult.Message, validationResult.IsWarning);
            }
        }

        /// <summary>
        /// Returns a map of all field types to field instances.
        /// </summary>
        /// <param name="viewModelRoot"></param>
        /// <returns></returns>
        private static Dictionary<ValidationField, FieldInstance> FindAllFields(object viewModelRoot)
        {
            var result = new Dictionary<ValidationField, FieldInstance>();
            var visitedObjects = new List<Object>();

            FindAllFieldsInObject(viewModelRoot, result, visitedObjects);

            return result;
        }

        private static void FindAllFieldsInObject(object viewModelObj, Dictionary<ValidationField, FieldInstance> result, List<object> visitedObjects)
        {
            if(viewModelObj != null && !visitedObjects.Any(r => Object.ReferenceEquals(viewModelObj, r))) {
                visitedObjects.Add(viewModelObj);

                foreach(var property in viewModelObj.GetType().GetProperties()) {
                    var attribute = property.GetCustomAttributes(true).OfType<ValidationModelFieldAttribute>().FirstOrDefault();
                    if(attribute != null) {
                        if(property.PropertyType != typeof(ValidationModelField)) {
                            throw new InvalidOperationException(String.Format("Saw ValidationModelField attribute on non-ValidationModelField type {0}.{1}", viewModelObj.GetType().Name, property.Name));
                        }
                        FieldInstance fieldInstance = null;
                        if(!result.TryGetValue(attribute.Field, out fieldInstance)) {
                            fieldInstance = new FieldInstance(property, attribute);
                            result.Add(attribute.Field, fieldInstance);
                        }
                        fieldInstance.ViewModelObjects.Add(viewModelObj);
                    } else {
                        if(property.PropertyType.IsClass && property.PropertyType != typeof(string) && property.CanRead) {
                            var child = property.GetValue(viewModelObj, null);
                            if(child != null) {
                                var collection = child as ICollection;

                                if(collection == null) {
                                    FindAllFieldsInObject(child, result, visitedObjects);
                                } else {
                                    foreach(var collectionItem in collection) {
                                        FindAllFieldsInObject(collectionItem, result, visitedObjects);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears all ValidationModelField instances on the view model passed across.
        /// </summary>
        /// <param name="fieldInstances"></param>
        private static void ClearAllValidationModelFields(Dictionary<ValidationField, FieldInstance> fieldInstances)
        {
            foreach(var fieldInstance in fieldInstances.Values) {
                foreach(var viewModelObject in fieldInstance.ViewModelObjects) {
                    var field = fieldInstance.GetField(viewModelObject);
                    field.Clear();
                }
            }
        }

        /// <summary>
        /// Creates empty ValidationModelField objects in the view model passed across.
        /// </summary>
        /// <param name="viewModel"></param>
        public static void CreateEmptyViewModelValidationFields(object viewModel)
        {
            var fieldInstances = FindAllFields(viewModel);
            foreach(var fieldInstance in fieldInstances.Values) {
                foreach(var viewModelObject in fieldInstance.ViewModelObjects) {
                    fieldInstance.GetField(viewModelObject);
                }
            }
        }
    }
}
