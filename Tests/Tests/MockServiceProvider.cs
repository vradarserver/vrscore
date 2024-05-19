// Copyright © 2024 onwards, Andrew Whewell
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
    /// A mock service provider for unit tests.
    /// </summary>
    public class MockServiceProvider : IServiceProvider
    {
        /// <summary>
        /// A dictionary of services that this service provider will return.
        /// </summary>
        public Dictionary<Type, object> Services { get; } = [];

        /// <summary>
        /// Strongly typed version of <see cref="GetService(Type)"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>() => (T)GetService(typeof(T));

        /// <inheritdoc />
        public object GetService(Type serviceType) => Services.TryGetValue(serviceType, out var value) ? value : null;

        /// <summary>
        /// True if the service is registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasService<T>() => HasService(typeof(T));

        /// <summary>
        /// True if the service is registered.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasService(Type type) => Services.ContainsKey(type);

        /// <summary>
        /// Adds or overwrites the mapping between a service type and its implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        public MockServiceProvider AddService<T>(T service) => AddService(typeof(T), service);

        /// <summary>
        /// Adds or overwrites the mapping between a service type and its implementation.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementation"></param>
        /// <returns></returns>
        public MockServiceProvider AddService(Type serviceType, object implementation)
        {
            if(HasService(serviceType)) {
                Services[serviceType] = implementation;
            } else {
                Services.Add(serviceType, implementation);
            }

            return this;
        }

        /// <summary>
        /// Creates a mock of the interface type passed across and automatically adds the mock
        /// to the service provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Mock<T> CreateMock<T>()
            where T: class
        {
            var result = MockHelper.CreateMock<T>();
            AddService<T>(result.Object);

            return result;
        }
    }
}
