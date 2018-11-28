using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ProjectShqUsersViewModel
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        public int Privilege { get; set; }
    }
}