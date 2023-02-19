using VirtualRadar.Interface.Options;

namespace VirtualRadar
{
    public class CommandLineArgs
    {
        public string WorkingFolder { get; set; } = new EnvironmentOptions().WorkingFolder;
    }
}
