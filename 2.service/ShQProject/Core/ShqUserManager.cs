using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Dxc.Shq.WebApi.Core
{
    public class ShqUserManager : UserManager<IdentityUser>
    {
        public ShqUserManager() : base(new ShqUserStore())
        {
        }
    }
}