//namespace Dxc.Shq.WebApi.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;

//    public class TeamGroupShqUsers
//    {
//        [Column(Order = 0)]
//        [Key,ForeignKey("ShqUser")]
//        public string ShqUserId { get; set; }
//        public ShqUser ShqUser { get; set; }

//        [Column(Order = 1)]
//        [Key,ForeignKey("TeamGroup")]
//        public Guid TeamGroupId { get; set; }
//        public TeamGroup TeamGroup { get; set; } 
//    }
//}