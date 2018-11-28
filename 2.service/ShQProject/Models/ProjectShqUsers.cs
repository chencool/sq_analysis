namespace Dxc.Shq.WebApi.Models
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;

    public class ProjectShqUsers : DataBase
    {
        [Column(Order = 0)]
        [Key,ForeignKey("Project")]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        [Column(Order = 1)]
        [Key, ForeignKey("ShqUser")]
        public string ShqUserId { get; set; }
        public ShqUser ShqUser { get; set; }

        public int Privilege { get; set; }
    }
}