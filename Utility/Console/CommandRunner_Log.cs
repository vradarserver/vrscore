namespace VirtualRadar.Utility.CLIConsole
{
    class CommandRunner_Log(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        HeaderService _Header,
        ILog _Log
        #pragma warning restore IDE1006
    ) : CommandRunner
    {
        public async override Task<bool> Run()
        {
            await _Header.OutputCopyright();
            await _Header.OutputTitle("Log");

            var log = _Log.ReadLines();
            foreach(var line in log) {
                await WriteLine(line);
            }

            return true;
        }
    }
}
