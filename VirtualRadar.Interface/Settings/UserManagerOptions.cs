namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Options that can be passed to the 
    /// </summary>
    public class UserManagerOptions
    {
        /// <summary>
        /// If true then database diagnostics are shown in the debug console. This
        /// might not be observed by all implementations of <see cref="IUserManager"/>.
        /// </summary>
        public bool ShowDatabaseDiagnosticsInDebugConsole { get; set; }
    }
}
