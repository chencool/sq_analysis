using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ShqUserRequestViewModel
    {
        public ShqUserRequestViewModel()
        {
        }

        public ShqUserRequestViewModel(ShqUser shqUser, ShqContext db)
        {
            if (shqUser == null)
            {
                return;
            }

            Status = shqUser.Status;
            //LoginName = shqUser.IdentityUser.UserName;
            RealName = shqUser.RealName;
            Description = shqUser.Description;
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
        public int Status { get; set; }

        public string Description { get; set; }

        //[Required]
        //public string LoginName { get; set; }

        public string RealName { get; set; }

        [Required]
        private string emailAddress;
        public string EmailAddress
        {
            get
            {
                return emailAddress;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == false)
                {
                    emailAddress = value.Trim().ToLower();
                }
            }
        }

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
                Status = this.Status,
                RealName = this.RealName,
                Description = this.Description,
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