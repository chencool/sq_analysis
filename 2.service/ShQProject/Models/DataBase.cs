namespace Dxc.Shq.WebApi.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class DataBase
    {
        [StringLength(255)]
        public string CreatedById { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;
        
        public string LastModifiedById { get; set; }

        public DateTime LastModfiedTime { get; set; } = DateTime.Now;
    }
}