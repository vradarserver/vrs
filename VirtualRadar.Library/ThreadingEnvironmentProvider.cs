using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    class ThreadingEnvironmentProvider : IThreadingEnvironmentProvider
    {
        public int CurrentThreadId => Environment.CurrentManagedThreadId;
    }
}
