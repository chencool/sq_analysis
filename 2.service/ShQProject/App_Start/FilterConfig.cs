using System.Web.Http;

namespace Dxc.Shq.WebApi
{
    public class FilterConfig
    {
        public static void Configure(HttpConfiguration config)
        {
            config.Filters.Add(new AuthorizeAttribute());
        }
    }
}