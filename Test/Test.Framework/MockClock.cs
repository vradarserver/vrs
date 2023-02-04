using Moq;
using VirtualRadar.Interface;

namespace Test.Framework
{
    public class MockClock : Mock<IClock>
    {
        public DateTimeOffset Now { get; set; }

        public MockClock(DateTimeOffset now)
        {
            Now = now;
            base.SetupGet(r => r.Now).Returns(() => Now);
            base.SetupGet(r => r.UtcNow).Returns(() => Now.UtcDateTime);
        }

        public MockClock() : this(DateTimeOffset.Now)
        {
        }
    }
}
