using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Moq;
using VirtualRadar.Interface.Settings;

namespace Test.Framework
{
    public class MockSharedConfiguration : Mock<ISharedConfiguration>
    {
        public Configuration Configuration { get; set; }

        public List<ISharedConfigurationSubscriber> Subscribers { get; } = new List<ISharedConfigurationSubscriber>();

        private void SetupSharedConfiguration()
        {
            Setup(r => r.Get())
                .Returns(() => Configuration);

            Setup(r => r.AddWeakSubscription(It.IsAny<ISharedConfigurationSubscriber>()))
                .Callback((ISharedConfigurationSubscriber subscriber) => {
                    Subscribers.Add(subscriber);
                });
        }

        public void RaiseConfigurationChanged()
        {
            Raise(r => r.ConfigurationChanged += null);

            foreach(var subscriber in Subscribers) {
                subscriber.SharedConfigurationChanged(this.Object);
            }
        }

        public static MockSharedConfiguration TestInitialise(Configuration configuration)
        {
            var result = new MockSharedConfiguration() {
                DefaultValue =  DefaultValue.Mock,
                Configuration = configuration,
            };
            result.SetupAllProperties();
            result.SetupSharedConfiguration();

            Factory.RegisterInstance<ISharedConfiguration>(result.Object);

            return result;
        }
    }
}
