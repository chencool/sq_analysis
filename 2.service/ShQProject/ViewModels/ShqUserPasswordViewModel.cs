using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ShqUserPasswordViewModel
    {
        [Required]
        public string EmailAddress { get; set; }
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}