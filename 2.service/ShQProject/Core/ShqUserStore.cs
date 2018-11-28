using Microsoft.AspNet.Identity.EntityFramework;

namespace Dxc.Shq.WebApi.Core
{
    public class ShqUserStore : UserStore<IdentityUser>
    {
        public ShqUserStore() : base(new ShqContext())
        {
        }
    }
}