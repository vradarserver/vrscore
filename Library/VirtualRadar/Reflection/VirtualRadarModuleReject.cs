namespace VirtualRadar.Reflection
{
    /// <summary>
    /// Describes a rejected Virtual Radar Module candidate.
    /// </summary>
    public record VirtualRadarModuleReject(
        string FileName,
        string Reason
    )
    {
    }
}
