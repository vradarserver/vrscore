// Copyright © 2024 onwards, Andrew Whewell
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

namespace VirtualRadar.Reflection
{
    /// <summary>
    /// Extracts information out of expressions.
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Returns the target of a property expression.
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static PropertyInfo ExtractPropertyInfo(Expression propertyExpression)
        {
            var lambdaExpression = (LambdaExpression)propertyExpression;

            var memberExpression = lambdaExpression.Body.NodeType == ExpressionType.Convert
                ? ((UnaryExpression)lambdaExpression.Body).Operand as MemberExpression
                : lambdaExpression.Body as MemberExpression;

            return memberExpression?.Member as PropertyInfo;
        }

        /// <summary>
        /// Returns the name of the target of a property expression.
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string ExtractNameOfProperty(Expression propertyExpression) => ExtractPropertyInfo(propertyExpression)?.Name;

        /// <summary>
        /// Returns the type of the target of a property expression.
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static Type ExtractTypeOfProperty(Expression propertyExpression) => ExtractPropertyInfo(propertyExpression)?.PropertyType;
    }
}
