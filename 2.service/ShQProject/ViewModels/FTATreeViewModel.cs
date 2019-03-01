using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class FTATreeRequestViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        public bool Analysis { get; set; }

        public FTATreeRequestViewModel()
        {
        }
    }

    public class FTATreeViewModel: FTATreeRequestViewModel
    {
       

        public ShqUserRequestViewModel CreatedBy { get; set; }

        public string CreatedTime { get; set; }

        public FTATreeViewModel():base()
        {
        }

        public FTATreeViewModel(FTATree tree, ShqContext db)
        {
            Id = tree.Id;
            Content = tree.Content;
            ProjectId = tree.FTAProject.ProjectId;
            CreatedBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == tree.CreatedById).FirstOrDefault(), db);
            CreatedTime = tree.CreatedTime.ToString();
        }
    }
}