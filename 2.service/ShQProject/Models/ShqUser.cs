namespace Dxc.Shq.WebApi.Models
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;

    public class ShqUser: DataBase
    {
        [Key, ForeignKey("IdentityUser")]
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }

        [Required]
        public bool Enabled { get; set; }

        public string RealName { get; set; }

        [Required]
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public string Gender { get; set; }

        public string JobLevel { get; set; }

        public string Department { get; set; }

        public virtual List<ProjectShqUsers> ProjectsAccess { get; set; } = new List<ProjectShqUsers>();
    }
}