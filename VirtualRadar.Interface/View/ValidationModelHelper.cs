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
        /// Describes the object that is hosting an instance of a validation field.
        /// </summary>
        class ViewModelObject
        {
            /// <summary>
            /// Gets the instance that has a validation property.
            /// </summary>
            public object Instance { get; set; }

            /// <summary>
            /// Gets the property that's been tagged.
            /// </summary>
            public PropertyInfo Property { get; set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="property"></param>
            public ViewModelObject(object instance, PropertyInfo property)
            {
                Instance = instance;
                Property = property;
            }

            public override string ToString()
            {
                return String.Format("{0}.{1}", Instance == null ? "null" : Instance.GetType().Name, Property == null ? "null" : Property.Name);
            }
        }

        /// <summary>
        /// Describes an instance of a field within a view model.
        /// </summary>
        class FieldInstance
        {
            /// <summary>
            /// The validation field.
            /// </summary>
            public ValidationField Field { get; private set; }

            /// <summary>
            /// The non-collection object that the field was seen on. There can only be 0 or 1 of these.
            /// </summary>
            public ViewModelObject SingleInstanceViewModelObject { get; set; }

            /// <summary>
            /// The collection of objects that the field was seen on. There can be many of these.
            /// </summary>
            public List<ViewModelObject> CollectionViewModelObjects { get; private set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="field"></param>
            public FieldInstance(ValidationField field)
            {
                Field = field;
                CollectionViewModelObjects = new List<ViewModelObject>();
            }

            /// <summary>
            /// Sets or adds the appropriate ViewModelObject property.
            /// </summary>
            public void AddViewModelObject(object instance, PropertyInfo property, bool seenOnCollection)
            {
                var viewModelObject = new ViewModelObject(instance, property);

                if(seenOnCollection) {
                    CollectionViewModelObjects.Add(viewModelObject);
                } else {
                    if(SingleInstanceViewModelObject != null) {
                        throw new InvalidOperationException(String.Format("Seen a single instance of {0} on at least two non-collection objects: {1} and {2}",
                            Field,
                            SingleInstanceViewModelObject.Instance.GetType().Name,
                            instance.GetType().Name
                        ));
                    }
                    SingleInstanceViewModelObject = viewModelObject;
                }
            }

            /// <summary>
            /// Returns every view model object that this field has been seen on.
            /// </summary>
            /// <returns></returns>
            public ViewModelObject[] GetAllViewModelObjects()
            {
                var result = new List<ViewModelObject>(CollectionViewModelObjects);
                if(SingleInstanceViewModelObject != null) result.Add(SingleInstanceViewModelObject);

                return result.ToArray();
            }

            /// <summary>
            /// Finds the <see cref="ViewModelObject"/> for the instance passed across.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns></returns>
            public ViewModelObject FindViewModelObject(object instance)
            {
                var result = instance == null ? SingleInstanceViewModelObject : null;
                if(instance != null) {
                    result = CollectionViewModelObjects.FirstOrDefault(r => Object.ReferenceEquals(r.Instance, instance));
                    if(result == null && SingleInstanceViewModelObject != null && Object.ReferenceEquals(instance, SingleInstanceViewModelObject.Instance)) {
                        result = SingleInstanceViewModelObject;
                    }
                }

                return result;
            }

            /// <summary>
            /// Returns the ValidationModelField associated with object. If a non-null view model object is passed
            /// across then it is always used, otherwise the <see cref="ViewModelObject"/> value is used.
            /// </summary>
            /// <param name="viewModelObject"></param>
            /// <returns></returns>
            public ValidationModelField GetField(ViewModelObject viewModelObject)
            {
                ValidationModelField result;

                var obj = viewModelObject ?? SingleInstanceViewModelObject;
                try {
                    result = obj.Property.GetValue(obj.Instance, null) as ValidationModelField;
                    if(result == null) {
                        result = new ValidationModelField();
                        obj.Property.SetValue(obj.Instance, result, null);
                    }
                } catch(Exception ex) {
                    throw new InvalidOperationException(String.Format("Could not set property for validation field {0} on {1} (viewModelObject={2}, {3} singleton instance(s) seen, {4} collection instances seen)",
                        Field,
                        obj == null ? "null" : obj.Property.Name,
                        viewModelObject,
                        SingleInstanceViewModelObject == null ? 0 : 1,
                        CollectionViewModelObjects.Count
                    ), ex);
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
                    var instance = FindViewModelForRecord == null ? null : FindViewModelForRecord(validationResult);
                    ApplyValidationResult(fieldInstances, instance, validationResult);
                }
            } else {
                foreach(var partialValidation in validationResults.PartialValidationFields) {
                    var instance = FindViewModelForRecord == null ? null : FindViewModelForRecord(partialValidation);
                    var validationResult = validationResults.Results.FirstOrDefault(r => Object.ReferenceEquals(partialValidation.Record, r.Record) && partialValidation.Field == r.Field);
                    ApplyValidationResult(fieldInstances, instance, validationResult ?? partialValidation);
                }
            }
        }

        private void ApplyValidationResult(Dictionary<ValidationField, FieldInstance> fieldInstances, object instance, ValidationResult validationResult)
        {
            FieldInstance fieldInstance;
            if(fieldInstances.TryGetValue(validationResult.Field, out fieldInstance)) {
                var viewModelObject = fieldInstance.FindViewModelObject(instance);
                if(viewModelObject != null) {
                    var field = fieldInstance.GetField(viewModelObject);
                    field.SetMessage(validationResult.Message, validationResult.IsWarning);
                }
            }
        }

        /// <summary>
        /// Returns a map of all field types to field instances.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private static Dictionary<ValidationField, FieldInstance> FindAllFields(object instance)
        {
            var result = new Dictionary<ValidationField, FieldInstance>();
            var visitedObjects = new List<Object>();

            FindAllFieldsInObject(instance, result, visitedObjects, isCollectionObject: false);

            return result;
        }

        private static void FindAllFieldsInObject(object instance, Dictionary<ValidationField, FieldInstance> result, List<object> visitedObjects, bool isCollectionObject)
        {
            if(instance != null && !visitedObjects.Any(r => Object.ReferenceEquals(instance, r))) {
                visitedObjects.Add(instance);

                foreach(var property in instance.GetType().GetProperties()) {
                    var attribute = property.GetCustomAttributes(true).OfType<ValidationModelFieldAttribute>().FirstOrDefault();
                    if(attribute != null) {
                        if(property.PropertyType != typeof(ValidationModelField)) {
                            throw new InvalidOperationException(String.Format("Saw ValidationModelField attribute on non-ValidationModelField type {0}.{1}", instance.GetType().Name, property.Name));
                        }
                        FieldInstance fieldInstance = null;
                        if(!result.TryGetValue(attribute.Field, out fieldInstance)) {
                            fieldInstance = new FieldInstance(attribute.Field);
                            result.Add(attribute.Field, fieldInstance);
                        }
                        fieldInstance.AddViewModelObject(instance, property, isCollectionObject);
                    } else {
                        if(property.PropertyType.IsClass && property.PropertyType != typeof(string) && property.CanRead) {
                            var childInstance = property.GetValue(instance, null);
                            if(childInstance != null) {
                                var collectionInstance = childInstance as ICollection;

                                if(collectionInstance == null) {
                                    FindAllFieldsInObject(childInstance, result, visitedObjects, isCollectionObject);
                                } else {
                                    foreach(var collectionItem in collectionInstance) {
                                        FindAllFieldsInObject(collectionItem, result, visitedObjects, isCollectionObject: true);
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
                foreach(var viewModelObject in fieldInstance.GetAllViewModelObjects()) {
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
                foreach(var viewModelObject in fieldInstance.GetAllViewModelObjects()) {
                    fieldInstance.GetField(viewModelObject);
                }
            }
        }
    }
}
