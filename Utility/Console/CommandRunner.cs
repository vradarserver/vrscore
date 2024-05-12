using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace VirtualRadar.Utility.CLIConsole
{
    abstract class CommandRunner
    {
        public abstract bool Run();
    }
}
