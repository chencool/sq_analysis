using Dxc.Shq.WebApi;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Dxc.Shq.WebApi
{
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);
        }
    }
}