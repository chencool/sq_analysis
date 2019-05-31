using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ExplorerInfoViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string ParentPath { get; set; }

        public string ProjectId { get; set; }

        [Required]
        public string Name { get; set; }

        public string OldName { get; set; }

        [Required]
        public int Level { get; set; }

        /// <summary>
        /// "createFolder"
        /// "deleteFolder"
        /// "uploadFile"
        /// "dowloadFile"
        /// "deleteFile"
        /// "newLevel"
        /// </summary>
        [Required]
        public string cmd { get; set; }
    }
}