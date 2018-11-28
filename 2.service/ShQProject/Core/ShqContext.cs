namespace Dxc.Shq.WebApi.Core
{
    using System.Data.Entity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;

    public class ShqContext : IdentityDbContext
    {
        public ShqContext()
            : base("ShqContext")
        {
            
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<FTAProject> FTAProjects { get; set; }

        public DbSet<ShqUser> ShqUsers { get; set; }

        public DbSet<ProjectShqUsers> ProjectShqUsers { get; set; }
    }
}