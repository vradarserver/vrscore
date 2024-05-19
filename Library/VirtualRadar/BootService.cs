namespace VirtualRadar
{
    /// <summary>
    /// Boots Virtual Radar Server.
    /// </summary>
    public class BootService
    {
        private readonly IBootable[] _Bootables;

        public BootService(IEnumerable<IBootable> bootables)
        {
            _Bootables = bootables.ToArray();
        }

        /// <summary>
        /// Call after everything has been loaded, after DI has initialised, after
        /// a host has been built etc. Initialises and ties together all of the
        /// different bits of the VRS runtime.
        /// </summary>
        public void Start()
        {
            OnBootStep(BootStep.Initialise);
        }

        private void OnBootStep(BootStep bootStep)
        {
            foreach(var bootable in _Bootables) {
                bootable.OnBootStep(bootStep);
            }
        }
    }
}
