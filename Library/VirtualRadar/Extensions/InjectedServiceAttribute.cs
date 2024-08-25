namespace VirtualRadar.Extensions
{
    /// <summary>
    /// Marks a property or field on an object that was instantiated outside of DI that
    /// we would like to populate from a DI container. Can also be used to mark methods
    /// that are to be called with injected parameters.
    /// </summary>
    [AttributeUsage(
          AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Method
        | AttributeTargets.Parameter,
        AllowMultiple = false,
        Inherited = true
    )]
    public class InjectedServiceAttribute : Attribute
    {
        /// <summary>
        /// True if the service is not required.
        /// </summary>
        public bool IsOptional { get; set; }
    }
}
