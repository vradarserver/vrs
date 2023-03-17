using System.Reflection;

namespace Test.Framework
{
    /// <summary>
    /// Methods that generate test data.
    /// </summary>
    public static class TestDataGenerator
    {
        /// <summary>
        /// Assigns a constant dummy value to a property on an object. Ported from V2/3's TestUtilities.
        /// </summary>
        /// <param name="obj">The object to assign to.</param>
        /// <param name="property">The property to assign.</param>
        /// <param name="useValue1">True if the first constant value should be assigned, false if the second (different) constant value should be assigned.</param>
        /// <param name="generateValue">Optional func that generates one of two different values for any non-standard type. Expected to throw an exception if the type is unrecognised.</param>
        public static void AssignPropertyValue(object obj, PropertyInfo property, bool useValue1, Func<Type, bool, object> generateValue = null)
        {
            var type = property.PropertyType;
            if(type == typeof(string))          property.SetValue(obj, useValue1 ? "VALUE1" : "VALUE2", null);

            else if(type == typeof(byte))       property.SetValue(obj, useValue1 ? (byte)1 : (byte)2, null);
            else if(type == typeof(short))      property.SetValue(obj, useValue1 ? (short)1 : (short)2, null);
            else if(type == typeof(int))        property.SetValue(obj, useValue1 ? 1 : 2, null);
            else if(type == typeof(long))       property.SetValue(obj, useValue1 ? 1L : 2L, null);
            else if(type == typeof(float))      property.SetValue(obj, useValue1 ? 1F : 2F, null);
            else if(type == typeof(double))     property.SetValue(obj, useValue1 ? 1.0 : 2.0, null);
            else if(type == typeof(bool))       property.SetValue(obj, useValue1 ? true : false, null);
            else if(type == typeof(DateTime))   property.SetValue(obj, useValue1 ? new DateTime(2014, 1, 25) : new DateTime(1920, 8, 17), null);

            else if(type == typeof(byte?))      property.SetValue(obj, useValue1 ? (byte?)null : (byte)2, null);
            else if(type == typeof(short?))     property.SetValue(obj, useValue1 ? (short?)null : (short)2, null);
            else if(type == typeof(int?))       property.SetValue(obj, useValue1 ? (int?)null : 2, null);
            else if(type == typeof(long?))      property.SetValue(obj, useValue1 ? (long?)null : 2L, null);
            else if(type == typeof(bool?))      property.SetValue(obj, useValue1 ? (bool?)null : false, null);
            else if(type == typeof(float?))     property.SetValue(obj, useValue1 ? (float?)null : 2F, null);
            else if(type == typeof(double?))    property.SetValue(obj, useValue1 ? (double?)null : 2.0, null);
            else if(type == typeof(DateTime?))  property.SetValue(obj, useValue1 ? (DateTime?)null : new DateTime(1920, 8, 17), null);

            else {
                if(generateValue == null) {
                    throw new NotImplementedException($"Need to add support for property type {type.Name}");
                } else {
                    property.SetValue(obj, generateValue(type, useValue1));
                }
            }
        }
    }
}
