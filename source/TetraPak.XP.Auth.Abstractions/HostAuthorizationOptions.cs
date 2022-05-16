using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP.Auth.Abstractions
{
    public sealed class HostAuthorizationOptions
    {
        
        /// <summary>
        ///   Gets or sets which grant types to be supported by the mobile application.
        /// </summary>
        public IEnumerable<GrantType> GrantTypes { get; set; }

        /// <summary>
        ///   Gets or sets a value specifying whether the mobile application's should cache acquired grants
        ///   (and hence "silent" grant acquisition).
        /// </summary>
        public bool IsGrantCacheSupported { get; set; }

        /// <summary>
        ///   Gets or sets a value specifying whether the mobile application should support fetching
        ///   user information based on acquired grants.
        /// </summary>
        public bool IsUserInformationSupported { get; set; }

        /// <summary>
        ///   
        /// </summary>
        /// <param name="grantTypes"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HostAuthorizationOptions(params GrantType[] grantTypes)
        {
            if (!grantTypes.Any())
                throw new ArgumentNullException(nameof(grantTypes));
                
            GrantTypes = grantTypes;
        }
    }
}