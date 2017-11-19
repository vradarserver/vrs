using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// A static class enumeration of the valid roles for users that authenticate with the site.
    /// </summary>
    /// <remarks>
    /// Sorry about the use of const, it's necessary if I want to use these in attributes.
    /// </remarks>
    public static class Roles
    {
        /// <summary>
        /// All authenticated users (including administrators) have this role.
        /// </summary>
        public const string User = "User";

        /// <summary>
        /// All administrator users have this role.
        /// </summary>
        public const string Admin = "Admin";
    }
}
