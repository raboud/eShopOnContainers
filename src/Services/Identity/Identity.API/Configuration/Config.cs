using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace HMS.Identity.API.Configuration
{
    public class Config
    {
        // ApiResources define the apis in your system
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("orders", "Orders Service"),
                new ApiResource("basket", "Basket Service"),
                new ApiResource("marketing", "Marketing Service"),
                new ApiResource("locations", "Locations Service"),
				new ApiResource("catalog", "Catalog Service")
			};
        }

        // Identity resources are data like user ID, name, or email address of a user
        // see: http://docs.identityserver.io/en/release/configuration/resources.html
        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(Dictionary<string,string> clientsUrl)
        {
            return new List<Client>
            {
				                // resource owner password grant client
                new Client
				{
					ClientId = "ro.client",
					AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

					ClientSecrets =
					{
						new Secret("secret".Sha256())
					},
					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						"catalog",
						"orders",
						"basket",
						"locations",
						"marketing"
					}
				},

                // JavaScript Client
                new Client
				{
					ClientId = "HMSjs",
					ClientName = "HMS OpenId Client",
					AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
					AllowAccessTokensViaBrowser = true,
					RedirectUris =           { $"http://localhost:4200/home" },
					RequireConsent = false,
					PostLogoutRedirectUris = { $"http://localhost:4200/" },
					AllowedCorsOrigins =     { $"http://localhost:4200" },
					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						"catalog",
						"orders",
						"basket",
						"locations",
						"marketing"
					}
				},

                // JavaScript Client
                new Client
                {
                    ClientId = "js",
                    ClientName = "HMS SPA OpenId Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =           { $"{clientsUrl["Spa"]}/" },
                    RequireConsent = false,
                    PostLogoutRedirectUris = { $"{clientsUrl["Spa"]}/" },
                    AllowedCorsOrigins =     { $"{clientsUrl["Spa"]}" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
						"catalog",
						"orders",
                        "basket",
                        "locations",
                        "marketing"
                    }
                },
                new Client
                {
                    ClientId = "xamarin",
                    ClientName = "HMS Xamarin OpenId Client",
                    AllowedGrantTypes = GrantTypes.Hybrid,                    
                    //Used to retrieve the access token on the back channel.
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    RedirectUris = { clientsUrl["Xamarin"] },
                    RequireConsent = false,
                    RequirePkce = true,
                    PostLogoutRedirectUris = { $"{clientsUrl["Xamarin"]}/Account/Redirecting" },
                    AllowedCorsOrigins = { "http://hmsxamarin" },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
						"catalog",
						"orders",
                        "basket",
                        "locations",
                        "marketing"
                    },
                    //Allow requesting refresh tokens for long lived API access
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri = $"{clientsUrl["Mvc"]}",                             // public uri of the client
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
						"catalog",
						"orders",
                        "basket",
                        "locations",
                        "marketing"
                    },
                },
                new Client
                {
                    ClientId = "mvctest",
                    ClientName = "MVC Client Test",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri = $"{clientsUrl["Mvc"]}",                             // public uri of the client
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    RedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
						"catalog",
						"orders",
                        "basket",
                        "locations",
                        "marketing"
                    },
                },
				new Client
				{
					ClientId = "catalogswaggerui",
					ClientName = "Catalog Swagger UI",
					AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
					AllowAccessTokensViaBrowser = true,

					RedirectUris = { $"{clientsUrl["CatalogApi"]}/swagger/o2c.html" },
					PostLogoutRedirectUris = { $"{clientsUrl["CatalogApi"]}/swagger/" },

					AllowedScopes =
					{
						"catalog"
					}
				},
				new Client
                {
                    ClientId = "locationsswaggerui",
                    ClientName = "Locations Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["LocationsApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["LocationsApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "locations"
                    }
                },
                new Client
                {
                    ClientId = "marketingswaggerui",
                    ClientName = "Marketing Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["MarketingApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["MarketingApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "marketing"
                    }
                },
                new Client
                {
                    ClientId = "basketswaggerui",
                    ClientName = "Basket Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["BasketApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["BasketApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "basket"
                    }
                },
                new Client
                {
                    ClientId = "orderingswaggerui",
                    ClientName = "Ordering Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["OrderingApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["OrderingApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "orders"
                    }
                }
            };
        }
    }
}