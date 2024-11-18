using VirtualRadar.Configuration;

namespace Tests.Mocks
{
    public class MockSettings<TSettings> : ISettings<TSettings>
    {
        public TSettings Value { get; set; }

        public TSettings LatestValue => Value;

        public MockSettings(TSettings initialValue)
        {
            Value = initialValue;
        }
    }
}
