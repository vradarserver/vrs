using Moq;
using VirtualRadar.Interface.Settings;

namespace Test.Framework
{
    /// <summary>
    /// A mock implementation of <see cref="ISharedConfiguration"/>.
    /// </summary>
    public class MockSharedConfiguration : Mock<ISharedConfiguration>
    {
        public Configuration Configuration { get; set; }

        public DateTime ConfigurationLastChangedUtc { get; set; } = DateTime.UtcNow;

        public MockSharedConfiguration(Configuration config = null, bool setupAll = true) : base()
        {
            Configuration = config ?? new();

            if(setupAll) {
                SetupAll();
            }
        }

        public void SetupAll()
        {
            Setup(r => r.Get())
            .Returns(() => Configuration);

            Setup(r => r.GetConfigurationChangedUtc())
            .Returns(() => ConfigurationLastChangedUtc);

            Setup(r => r.AddWeakSubscription(It.IsAny<ISharedConfigurationSubscriber>()))
            .Callback((ISharedConfigurationSubscriber subscriber) => {
                Object.ConfigurationChanged += (object obj, EventArgs e) => {
                    subscriber.SharedConfigurationChanged(Object);
                };
            });
        }

        public void RaiseConfigurationChanged() => Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);
    }
}
