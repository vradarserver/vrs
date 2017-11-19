using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton object that supplies additional configuration for the authentication
    /// middleware.
    /// </summary>
    /// <remarks>
    /// Authentication middleware is expected to automatically reconfigure itself when the application's
    /// configuration changes. The configuration exposed here is for things like paths that are only
    /// accessible to admin users, things like that.
    /// </remarks>
    [Singleton]
    public interface IAuthenticationConfiguration
    {
        /// <summary>
        /// Returns the paths that have been marked as requiring authentication.
        /// </summary>
        /// <returns></returns>
        string[] GetAdministratorPaths();

        /// <summary>
        /// Tells the server that all access to this path must be authenticated and that the user must
        /// be configured as an administrator.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        void AddAdministratorPath(string pathFromRoot);

        /// <summary>
        /// Tells the server that anyone can now access this path.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        void RemoveAdministratorPath(string pathFromRoot);

        /// <summary>
        /// Returns true if the path passed across has been registered as an administrator path.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note that this is not case sensitive.
        /// </remarks>
        bool IsAdministratorPath(string pathAndFile);
    }
}
