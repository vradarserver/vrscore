﻿// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Test.Framework;
using VirtualRadar.Filtering;

namespace Test.VirtualRadar.Filtering
{
    [TestClass]
    public class FilterString_Tests
    {
        [TestMethod]
        public void FilterString_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var filter = new FilterString();
            TestUtilities.TestProperty(filter, r => r.Condition, FilterCondition.Missing, FilterCondition.Equals);
            TestUtilities.TestProperty(filter, r => r.ReverseCondition, false);
            TestUtilities.TestProperty(filter, r => r.Value, null, "Abc");

            filter = new FilterString("j");
            TestUtilities.TestProperty(filter, r => r.Condition, FilterCondition.Equals, FilterCondition.Contains);
            TestUtilities.TestProperty(filter, r => r.ReverseCondition, false);
            TestUtilities.TestProperty(filter, r => r.Value, "j", "Abc");
        }

        [TestMethod]
        public void FilterString_ToUpper_Converts_Value_To_UpperCase()
        {
            var filter = new FilterString();

            filter.Value = null;
            filter.ToUpperInvariant();
            Assert.IsNull(filter.Value);

            filter.Value = "";
            filter.ToUpperInvariant();
            Assert.AreEqual("", filter.Value);

            filter.Value = "Abc";
            filter.ToUpperInvariant();
            Assert.AreEqual("ABC", filter.Value);
        }

        [TestMethod]
        public void FilterString_Passes_Single_String_Returns_Correct_Results()
        {
            foreach(FilterCondition condition in Enum.GetValues(typeof(FilterCondition))) {
                foreach(var reverseCondition in new bool[] { false, true }) {
                    foreach(var value in new string[] { null, "", "abc", "ABC", }) {
                        foreach(var testValue in new string[] { null, "", "abc", "ABC", "a", "A", "b", "B", "c", "C", "d", "D"}) {
                            var filter = new FilterString() {
                                Condition = condition,
                                ReverseCondition = reverseCondition,
                                Value = value
                            };

                            var result = filter.Passes(testValue);

                            var expectedResult = true;
                            if(condition != FilterCondition.Between && condition != FilterCondition.Missing) {
                                if(String.IsNullOrEmpty(value)) {
                                    if(condition != FilterCondition.Equals) expectedResult = true;
                                    else {
                                        expectedResult = String.IsNullOrEmpty(testValue);
                                        if(reverseCondition) expectedResult = !expectedResult;
                                    }
                                } else {
                                    var ucaseValue = (value ?? "").ToUpperInvariant();
                                    var ucaseTestValue = (testValue ?? "").ToUpperInvariant();
                                    switch(condition) {
                                        case FilterCondition.Contains:      expectedResult = ucaseTestValue.Contains(ucaseValue); break;
                                        case FilterCondition.EndsWith:      expectedResult = ucaseTestValue.EndsWith(ucaseValue); break;
                                        case FilterCondition.Equals:        expectedResult = ucaseTestValue == ucaseValue; break;
                                        case FilterCondition.StartsWith:    expectedResult = ucaseTestValue.StartsWith(ucaseValue); break;
                                        default:                            throw new NotImplementedException();
                                    }
                                    if(reverseCondition) expectedResult = !expectedResult;
                                }
                            }

                            Assert.AreEqual(expectedResult, result, $"{condition}/{reverseCondition}/{value}/{testValue}");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void FilterString_Passes_String_Collection_Returns_Correct_Results()
        {
            foreach(FilterCondition condition in Enum.GetValues(typeof(FilterCondition))) {
                foreach(var reverseCondition in new bool[] { false, true }) {
                    foreach(var value in new string[] { null, "", "abc", "ABC", }) {
                        foreach(var testValue in new string[][] {
                            null,
                            [ null ],
                            [ "" ],
                            [ "abc" ],
                            [ "a", "c" ],
                            [ "ABC" ],
                            [ "A", "C" ],
                            [ null, "A" ],
                            [ "", "A" ],
                            [ "D" ],
                        }) {
                            var filter = new FilterString() {
                                Condition = condition,
                                ReverseCondition = reverseCondition,
                                Value = value
                            };

                            var result = filter.Passes(testValue);

                            var expectedResult = true;
                            if(condition != FilterCondition.Between && condition != FilterCondition.Missing) {
                                if(String.IsNullOrEmpty(value)) {
                                    if(condition != FilterCondition.Equals) expectedResult = true;
                                    else {
                                        expectedResult = testValue == null || testValue.Contains(null) || testValue.Contains("");
                                        if(reverseCondition) expectedResult = !expectedResult;
                                    }
                                } else {
                                    var ucaseValue = (value ?? "").ToUpperInvariant();
                                    var ucaseTestValue = (testValue ?? new string[0]).Select(r => (r ?? "").ToUpperInvariant()).ToArray();
                                    switch(condition) {
                                        case FilterCondition.Contains:      expectedResult = ucaseTestValue.Any(r => r.Contains(ucaseValue)); break;
                                        case FilterCondition.EndsWith:      expectedResult = ucaseTestValue.Any(r => r.EndsWith(ucaseValue)); break;
                                        case FilterCondition.Equals:        expectedResult = ucaseTestValue.Any(r => r == ucaseValue); break;
                                        case FilterCondition.StartsWith:    expectedResult = ucaseTestValue.Any(r => r.StartsWith(ucaseValue)); break;
                                        default:                            throw new NotImplementedException();
                                    }
                                    if(reverseCondition) expectedResult = !expectedResult;
                                }
                            }

                            Assert.AreEqual(expectedResult, result, "{0}/{1}/{2}/{3}",
                                condition,
                                reverseCondition,
                                value == ""
                                    ? "empty string"
                                    : value ?? "null string",
                                testValue == null
                                    ? "null array"
                                    : String.Join(",", testValue.Select(r => r ?? "null"))
                            );
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void FilterString_Equals_Returns_True_When_All_Properties_Match()
        {
            TestUtilities.TestSimpleEquals(typeof(FilterString), true, GenerateValue);
        }

        [TestMethod]
        public void FilterString_Equals_Returns_True_When_Any_Properties_Do_Not_Match()
        {
            TestUtilities.TestSimpleEquals(typeof(FilterString), false, GenerateValue);
        }

        [TestMethod]
        public void FilterString_GetHashCode_Returns_Correct_Value()
        {
            TestUtilities.TestSimpleGetHashCode(typeof(FilterString), GenerateValue);
        }

        private object GenerateValue(Type type, bool useValue1)
        {
            if(type == typeof(FilterCondition)) {
                return useValue1 ? FilterCondition.Contains : FilterCondition.Equals;
            }
            throw new NotImplementedException($"Need to add support for property type {type.Name}");
        }
    }
}
