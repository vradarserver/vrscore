using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Creates a new instance of the type passed across using the default ctor. If the type has no
        /// default ctor, but does have a ctor where every parameter is optional, then that ctor is used
        /// instead, passing all default values.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Throw if the type has no suitable ctor.</exception>
        public static object CreateDefaultInstance(this Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            object result = null;

            foreach(var ctorInfo in type.GetConstructors(bindingFlags)) {
                object[] parameterValues = null;

                var parameters = ctorInfo.GetParameters();
                if(parameters.Length == 0) {
                    parameterValues = Array.Empty<object>();
                } else if(!parameters.Any(p => !p.IsOptional || !p.HasDefaultValue)) {
                    parameterValues = parameters
                        .Select(p => p.DefaultValue)
                        .ToArray();
                }

                if(parameterValues != null) {
                    result = ctorInfo.Invoke(parameterValues);
                    break;
                }
            }

            if(result == null) {
                throw new InvalidOperationException(
                    $"Could not find a default or fully-optional and defaulted ctor for {type.Name}"
                );
            }

            return result;
        }
    }
}
