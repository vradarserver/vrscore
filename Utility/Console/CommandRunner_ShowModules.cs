namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_ShowModules(
        #pragma warning disable CS9113 // VS2022 handle name rules for primary ctors in classes properly
        IModuleInformationService _ModuleInfo
        #pragma warning restore CS9113
    ) : CommandRunner
    {
        public async override Task<bool> Run()
        {
            var loadedModules = _ModuleInfo.LoadedModules;
            var rejectedModules = _ModuleInfo.RejectedModules;

            await WriteLine("Loaded Modules");
            await WriteLine("--------------");
            for(var idx = 0;idx < loadedModules.Length;++idx) {
                var module = loadedModules[idx];
                if(idx != 0) {
                    await WriteLine();
                }
                await WriteLine($"{module.Manifest.ModuleName}");
                await WriteLine($"    Filename: {module.FileName}");
                await WriteLine($"    Priority: {module.ModuleInstance.Priority}");
                await WriteLine($"    Versions: {module.Manifest.MinimumSupportedVirtualRadarVersion} to {module.Manifest.MaximumSupportedVirtualRadarVersion}");
            }

            await WriteLine();
            await WriteLine("Rejected Modules");
            await WriteLine("----------------");
            if(rejectedModules.Length == 0) {
                await WriteLine("None");
            }
            for(var idx = 0;idx < rejectedModules.Length;++idx) {
                var reject = rejectedModules[idx];
                if(idx != 0) {
                    await WriteLine();
                }
                await WriteLine($"Filename: {reject.FileName}");
                await WriteLine($"Reason:   {reject.Reason}");
            }

            return true;
        }
    }
}
