using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ShqUserViewModel
    {
        public ShqUserViewModel()
        {
        }

        public ShqUserViewModel(ShqUser shqUser, ShqContext db)
        {
            if (shqUser == null)
            {
                return;
            }

            Enabled = shqUser.Enabled;
            //LoginName = shqUser.IdentityUser.UserName;
            RealName = shqUser.RealName;
            EmailAddress = shqUser.EmailAddress;
            PhoneNumber = shqUser.PhoneNumber;
            Address = shqUser.Address;
            Gender = shqUser.Gender;
            JobLevel = shqUser.JobLevel;
            Department = shqUser.Department;
            shqUser.IdentityUser = db.Users.Find(shqUser.IdentityUserId);
            foreach (var r in shqUser.IdentityUser.Roles)
            {
                Roles.Add(db.Roles.Find(r.RoleId).Name);
            }
        }

        [Required]
        public bool Enabled { get; set; }

        //[Required]
        //public string LoginName { get; set; }

        public string RealName { get; set; }

        [Required]
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public string Password { get; set; }

        public string Gender { get; set; }

        public string JobLevel { get; set; }

        public string Department { get; set; }

        [Required]
        public List<string> Roles { get; set; } = new List<string>();

        public ShqUser ToShqUser()
        {
            return new ShqUser
            {
                Enabled = this.Enabled,
                RealName = this.RealName,
                EmailAddress = this.EmailAddress,
                PhoneNumber = this.PhoneNumber,
                Address = this.Address,
                Gender = this.Gender,
                JobLevel = this.JobLevel,
                Department = this.Department,
            };
        }
    }
}