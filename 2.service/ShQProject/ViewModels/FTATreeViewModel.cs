using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class FTATreeViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        public ShqUserViewModel CreateBy { get; set; }

        public string CreateTime { get; set; }

        public FTATreeViewModel()
        {

        }

        public FTATreeViewModel(FTATree tree, ShqContext db)
        {
            Id = tree.Id;
            Content = tree.Content;
            ProjectId = tree.FTAProject.ProjectId;
            CreateBy = new ShqUserViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == tree.CreateById).FirstOrDefault(), db);
            CreateTime = tree.CreateTime.ToString();
        }
    }
}