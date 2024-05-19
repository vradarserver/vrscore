namespace Tests
{
    /// <summary>
    /// The base for test classes that need to interact with dependency injection.
    /// </summary>
    public abstract class TestClass
    {
        protected MockServiceProvider _ServiceProvider;

        [TestInitialize]
        public void TestInitialise()
        {
            _ServiceProvider = new();

            Initialise();
        }

        /// <summary>
        /// When overridden by the derivee this gets called once before each test method
        /// begins execution.
        /// </summary>
        protected virtual void Initialise() {;}

        [TestCleanup]
        public void TestCleanup()
        {
            Cleanup();
        }

        /// <summary>
        /// When overridden by the derivee this gets called once after each test method
        /// finishes execution.
        /// </summary>
        protected virtual void Cleanup() {;}
    }
}
