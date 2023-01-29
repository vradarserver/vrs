using Moq;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Library.Mocks
{
    public class Clock : Mock<IClock>
    {
        public DateTimeOffset Now { get; set; }

        public Clock(DateTimeOffset now)
        {
            Now = now;
            base.SetupGet(r => r.Now).Returns(() => Now);
            base.SetupGet(r => r.UtcNow).Returns(() => Now.UtcDateTime);
        }

        public Clock() : this(DateTimeOffset.Now)
        {
        }
    }
}
