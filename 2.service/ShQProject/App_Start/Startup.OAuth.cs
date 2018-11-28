using System;
using System.Configuration;
using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace Dxc.Shq.WebApi
{
    public partial class Startup
    {
        public void ConfigureOAuth(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["issuer"];
            int timeout = int.Parse( ConfigurationManager.AppSettings["keyTimeOut"]);
            //TextEncodings.Base64Url.Encode(System.Text.ASCIIEncoding.ASCII.GetBytes("^df245qerab546asdfasd6fsadf&*(^df*KeU81oJdlnN%q#F7"))
            var secret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["secret"]);

            app.CreatePerOwinContext(() => new ShqContext());
            app.CreatePerOwinContext(() => new ShqUserManager());

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { "Any" },
                IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                {
                    new SymmetricKeyIssuerSecurityTokenProvider(issuer, secret)
                }
            });

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth2/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(timeout),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat(issuer)
            });
        }
    }
}