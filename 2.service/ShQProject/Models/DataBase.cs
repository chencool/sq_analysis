namespace Dxc.Shq.WebApi.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class DataBase
    {
        public string CreateById { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
        
        public string LastModifiedById { get; set; }

        public DateTime LastModfiedDate { get; set; } = DateTime.Now;
    }
}