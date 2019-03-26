﻿namespace Dxc.Shq.WebApi.Core
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

        public DbSet<FTANodeType> FTANodeTypes { get; set; }

        public DbSet<FTANodeGateType> FTANodeGateTypes { get; set; }

        public DbSet<FTANode> FTANodes { get; set; }

        public DbSet<FTANodeProperties> FTANodeProperties { get; set; }

        public DbSet<FTANodeGate> FTANodeGates { get; set; }

        public DbSet<FTAEventType> FTAEventTypes { get; set; }

        public DbSet<FTAFailureType> FTAFailureTypes { get; set; }

        public DbSet<FTANodeEventReport> FTANodeEventReports { get; set; }

        public DbSet<FTAEventReport> FTAEventReports { get; set; }

        public DbSet<FTAAnalysisResultById> FTAAnalysisResultByIds { get; set; }
        public DbSet<FTAAnalysisResultByName> FTAAnalysisResultByNames { get; set; }
    }
}