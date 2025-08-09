// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("planetwarapi", "Planet war API")
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("planetwarapi")
                {
                    Scopes = { "planetwarapi" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // React client
                new Client
                {
                    ClientId = "planetwar",
                    ClientName = "Planet war",
                    ClientUri = "http://localhost:3000",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    
                    RequireClientSecret = false,

                    RedirectUris =
                    {                        
                        "http://localhost:3000/signin-oidc",                        
                    },

                    PostLogoutRedirectUris = { "http://localhost:3000/signout-oidc" },
                    AllowedCorsOrigins = { "http://localhost:3000" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "planetwarapi"
                    },

                    AllowAccessTokensViaBrowser = true
                }
            };
    }
}