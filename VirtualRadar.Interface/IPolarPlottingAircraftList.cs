using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Interface
{
    public interface IPolarPlottingAircraftList : IAircraftList
    {
        /// <summary>
        /// Gets or sets the object that's keeping polar plots for us (if any) - can be null if the listener
        /// does not support polar plots.
        /// </summary>
        IPolarPlotter PolarPlotter { get; set; }
    }
}
