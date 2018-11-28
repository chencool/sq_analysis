//namespace Dxc.Shq.WebApi.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;

//    public class TeamGroup: DataBase
//    {
//        [Key]
//        public Guid ID { get; set; }

//        [Required]
//        public string Name { get; set; }

//        public string Description { get; set; }

//        public virtual List<Project> Projects { get; set; } = new List<Project>();

//        public virtual List<TeamGroupShqUsers> TeamGroupShqUsers { get; set; } = new List<TeamGroupShqUsers>();
//    }
//}