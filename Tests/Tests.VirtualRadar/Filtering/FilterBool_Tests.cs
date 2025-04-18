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
    public class FilterBool_Tests
    {
        [TestMethod]
        public void FilterBool_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var filter = new FilterBool();
            TestUtilities.TestProperty(filter, r => r.Condition, FilterCondition.Missing, FilterCondition.Equals);
            TestUtilities.TestProperty(filter, r => r.ReverseCondition, false);
            TestUtilities.TestProperty(filter, r => r.Value, false);

            filter = new FilterBool(true);
            TestUtilities.TestProperty(filter, r => r.Condition, FilterCondition.Equals, FilterCondition.Between);
            TestUtilities.TestProperty(filter, r => r.ReverseCondition, false);
            TestUtilities.TestProperty(filter, r => r.Value, true);
        }

        [TestMethod]
        public void FilterBool_Passes_Returns_Correct_Results()
        {
            foreach(FilterCondition condition in Enum.GetValues(typeof(FilterCondition))) {
                foreach(var reverseCondition in new bool[] { false, true }) {
                    foreach(var value in new bool[] { false, true }) {
                        foreach(var testValue in new bool?[] { null, false, true }) {
                            var filter = new FilterBool() {
                                Condition = condition,
                                ReverseCondition = reverseCondition,
                                Value = value
                            };

                            var result = filter.Passes(testValue);

                            var expectedResult = true;
                            if(condition == FilterCondition.Equals) {
                                expectedResult = testValue != null && value == testValue;
                                if(reverseCondition) {
                                    expectedResult = !expectedResult;
                                }
                            }

                            Assert.AreEqual(expectedResult, result, "{0}/{1}/{2}/{3}",
                                condition,
                                reverseCondition,
                                value,
                                testValue
                            );
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void FilterBool_Equals_Returns_True_When_All_Properties_Match()
        {
            TestUtilities.TestSimpleEquals(typeof(FilterBool), true, GenerateValue);
        }

        [TestMethod]
        public void FilterBool_Equals_Returns_True_When_Any_Properties_Do_Not_Match()
        {
            TestUtilities.TestSimpleEquals(typeof(FilterBool), false, GenerateValue);
        }

        [TestMethod]
        public void FilterBool_GetHashCode_Returns_Correct_Value()
        {
            TestUtilities.TestSimpleGetHashCode(typeof(FilterBool), GenerateValue);
        }

        private object GenerateValue(Type type, bool useValue1)
        {
            if(type == typeof(FilterCondition)) {
                return useValue1 ? FilterCondition.Equals : FilterCondition.Missing;
            }
            throw new NotImplementedException($"Need to add support for property type {type.Name}");
        }
    }
}
