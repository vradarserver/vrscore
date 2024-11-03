// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Linq.Expressions;
using System.Reflection;
using VirtualRadar.Reflection;

namespace Test.Framework
{
    /// <summary>
    /// A collection of static methods that cover common aspects of testing objects.
    /// </summary>
    public static class TestUtilities
    {
        /// <summary>
        /// Asserts that a property of an object has a given start value and can be successfuly changed to
        /// another value.
        /// </summary>
        /// <param name="obj">The object under test.</param>
        /// <param name="propertyName">The name of the property to test.</param>
        /// <param name="startValue">The expected starting value of the property.</param>
        /// <param name="newValue">
        /// The value to write to the property - to pass the property must expose this value once it has been
        /// set.
        /// </param>
        /// <param name="testForEquals">
        /// True if the equality test uses <see cref="object.Equals(object)"/>, false if it uses <see
        /// cref="Object.ReferenceEquals"/>. If either the startValue or newValue are value types then this
        /// parameter is ignored, the test is always made using <see cref="object.Equals(object)"/>.
        /// </param>
        public static void TestProperty(
            object obj,
            string propertyName,
            object startValue,
            object newValue,
            bool testForEquals
        )
        {
            Assert.IsNotNull(obj);
            var property = obj.GetType().GetProperty(propertyName);
            Assert.IsNotNull(property, $"{obj.GetType().Name}.{propertyName} is not a public property");

            if(startValue != null && startValue.GetType().IsValueType) {
                testForEquals = true;
            }
            if(newValue != null && newValue.GetType().IsValueType) {
                testForEquals = true;
            }

            var actualStart = property.GetValue(obj, null);
            if(testForEquals) {
                Assert.AreEqual(startValue, actualStart, "{0}.{1} expected [{2}] was [{3}]",
                    obj.GetType().Name,
                    propertyName,
                    startValue == null ? "null" : startValue.ToString(),
                    actualStart == null ? "null" : actualStart.ToString()
                );
            } else {
                Assert.AreSame(startValue, actualStart, "{0}.{1} expected [{2}] was [{3}]",
                    obj.GetType().Name,
                    propertyName,
                    startValue == null ? "null" : startValue.ToString(),
                    actualStart == null ? "null" : actualStart.ToString()
                );
            }

            if(property.CanWrite) {
                property.SetValue(obj, newValue, null);
                var actualNew = property.GetValue(obj, null);
                if(testForEquals) {
                    Assert.AreEqual(newValue, actualNew, "{0}.{1} was set to [{2}] but it returned [{3}] when read",
                        obj.GetType().Name,
                        propertyName,
                        newValue == null ? "null" : newValue.ToString(),
                        actualNew == null ? "null" : actualNew.ToString()
                    );
                } else {
                    Assert.AreSame(newValue, actualNew, "{0}.{1} was set to [{2}] but it returned [{3}] when read",
                        obj.GetType().Name,
                        propertyName,
                        newValue == null ? "null" : newValue.ToString(),
                        actualNew == null ? "null" : actualNew.ToString()
                    );
                }
            }
        }

        /// <summary>
        /// Asserts that the property passed across starts at a given value and can be changed to another
        /// value.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="startValue"></param>
        /// <param name="newValue"></param>
        /// <param name="testForEquals"></param>
        public static void TestProperty<T>(
            T obj,
            Expression<Func<T, object>> propertyExpression,
            object startValue,
            object newValue,
            bool testForEquals
        )
        {
            TestProperty(
                obj,
                ExpressionHelper.PropertyName(propertyExpression),
                startValue,
                newValue,
                testForEquals
            );
        }

        /// <summary>
        /// Asserts that a property of an object has a given start value and can be successfuly changed to
        /// another value.
        /// </summary>
        /// <param name="obj">The object under test.</param>
        /// <param name="propertyName">The name of the property to test.</param>
        /// <param name="startValue">The expected starting value of the property.</param>
        /// <param name="newValue">
        /// The value to write to the property - to pass the property must expose this value once it has been
        /// set.
        /// </param>
        public static void TestProperty(object obj, string propertyName, object startValue, object newValue)
        {
            TestProperty(obj, propertyName, startValue, newValue, false);
        }

        /// <summary>
        /// Asserts that a property of an object has a given start value and can be successfuly changed to
        /// another value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="startValue"></param>
        /// <param name="newValue"></param>
        public static void TestProperty<T>(
            T obj,
            Expression<Func<T, object>> propertyExpression,
            object startValue,
            object newValue
        )
        {
            TestProperty(obj, ExpressionHelper.PropertyName(propertyExpression), startValue, newValue, false);
        }

        /// <summary>
        /// Asserts that a bool property of an object has the given start value. Sets the property to the
        /// toggled start value and asserts that the toggled value can then be read back from the property.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="startValue"></param>
        public static void TestProperty(object obj, string propertyName, bool startValue)
        {
            TestProperty(obj, propertyName, startValue, !startValue, true);
        }

        /// <summary>
        /// Asserts that a bool property of an object has the given start value. Sets the property to the
        /// toggled start value and asserts that the toggled value can then be read back from the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="startValue"></param>
        public static void TestProperty<T>(T obj, Expression<Func<T, object>> propertyExpression, bool startValue)
        {
            TestProperty(obj, ExpressionHelper.PropertyName(propertyExpression), startValue, !startValue, true);
        }

        /// <summary>
        /// Assigns a constant dummy value to a property on an object.
        /// </summary>
        /// <param name="obj">The object to assign to.</param>
        /// <param name="property">The property to assign.</param>
        /// <param name="useValue1">
        /// True if the first constant value should be assigned, false if the second (different) constant
        /// value should be assigned.
        /// </param>
        /// <param name="generateValue">
        /// Optional func that generates one of two different values for any non-standard type. Expected to
        /// throw an exception if the type is unrecognised.
        /// </param>
        public static void AssignPropertyValue(
            object obj,
            PropertyInfo property,
            bool useValue1,
            Func<Type, bool, object> generateValue = null
        )
        {
            var type = property.PropertyType;
            if(type == typeof(string))          property.SetValue(obj, useValue1 ? "VALUE1" : "VALUE2", null);

            else if(type == typeof(byte))       property.SetValue(obj, useValue1 ? (byte)1 : (byte)2, null);
            else if(type == typeof(short))      property.SetValue(obj, useValue1 ? (short)1 : (short)2, null);
            else if(type == typeof(int))        property.SetValue(obj, useValue1 ? 1 : 2, null);
            else if(type == typeof(long))       property.SetValue(obj, useValue1 ? 1L : 2L, null);
            else if(type == typeof(float))      property.SetValue(obj, useValue1 ? 1F : 2F, null);
            else if(type == typeof(double))     property.SetValue(obj, useValue1 ? 1.0 : 2.0, null);
            else if(type == typeof(bool))       property.SetValue(obj, useValue1, null);
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

        /// <summary>
        /// Tests that an object whose Equals method tests all of its public properties is behaving correctly.
        /// Only works with objects that have a simple constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedEquals"></param>
        /// <param name="generateValue">
        /// Optional func that generates one of two different values for any non-standard type. Expected to
        /// throw an exception if the type is unrecognised.
        /// </param>
        public static void TestSimpleEquals(
            Type type,
            bool expectedEquals,
            Func<Type, bool, object> generateValue = null
        )
        {
            foreach(var property in type.GetProperties().Where(r => r.CanRead && r.CanWrite)) {
                var instance1 = Activator.CreateInstance(type);
                var instance2 = Activator.CreateInstance(type);

                AssignPropertyValue(instance1, property, true, generateValue);
                AssignPropertyValue(instance2, property, expectedEquals, generateValue);

                Assert.AreEqual(expectedEquals, instance1.Equals(instance2), $"Property that failed: {property.Name}");
            }
        }

        /// <summary>
        /// Tests that an object whose GetHashCode method tests all of its public properties is behaving
        /// correctly. Only works with objects that have a simple constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedEquals"></param>
        /// <param name="generateValue">
        /// Optional func that generates one of two different values for any non-standard type. Expected to
        /// throw an exception if the type is unrecognised.
        /// </param>
        /// <remarks>
        /// This only checks that two objects that would pass equality for <see cref="TestSimpleEquals"/> have
        /// the same hash code.
        /// </remarks>
        public static void TestSimpleGetHashCode(Type type, Func<Type, bool, object> generateValue = null)
        {
            var instance1 = Activator.CreateInstance(type);
            var instance2 = Activator.CreateInstance(type);

            foreach(var property in type.GetProperties().Where(r => r.CanRead && r.CanWrite)) {
                AssignPropertyValue(instance1, property, true, generateValue);
                AssignPropertyValue(instance2, property, true, generateValue);
            }

            Assert.AreEqual(instance1.GetHashCode(), instance2.GetHashCode());
        }
    }
}
