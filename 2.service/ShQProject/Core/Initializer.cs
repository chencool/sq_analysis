namespace Dxc.Shq.WebApi.Core
{
    using Dxc.Shq.WebApi.Models;
    using System.Data.Entity;

    public class Initializer : MigrateDatabaseToLatestVersion<ShqContext, Configuration>
    {
    }


    public class DropAndCreateInitializer : DropCreateDatabaseIfModelChanges<ShqContext>
    {
        protected override void Seed(ShqContext context)
        {
            base.Seed(context);
        }
    }
}