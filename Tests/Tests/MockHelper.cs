// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Moq;

namespace Tests
{
    /// <summary>
    /// Mock helpers.
    /// </summary>
    public static class MockHelper
    {
        /// <summary>
        /// Creates a Moq stub for an object but does not register it with the class factory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strict"></param>
        /// <returns></returns>
        public static Mock<T> CreateMock<T>(bool strict = false)
            where T: class
        {
            var mockBehaviour = strict ? MockBehavior.Strict : MockBehavior.Loose;

            return new Mock<T>(mockBehaviour) {
                DefaultValue = DefaultValue.Mock
            }
            .SetupAllProperties();
        }

        /// <summary>
        /// Creates the Mock type passed across.
        /// </summary>
        /// <param name="mockType"></param>
        /// <returns></returns>
        public static Mock CreateMock(Type mockType)
        {
            var result = (Mock)Activator.CreateInstance(mockType);
            result.DefaultValue = DefaultValue.Mock;

            return result;
        }
    }
}
