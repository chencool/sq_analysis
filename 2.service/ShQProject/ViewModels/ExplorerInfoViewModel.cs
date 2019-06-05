using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ExplorerInfoViewModel
    {
        public int Id { get; set; }

        [Required]
        public string ParentPath { get; set; }

        [Required]
        /// <summary>
        /// project id
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// File or folder name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Old file or folder name
        /// </summary>
        public string OldName { get; set; }

        /// <summary>
        /// file or folder level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// "createFolder"
        /// "delete"
        /// "rename"
        /// "uploadFile"
        /// "dowloadFile"
        /// "newLevel"
        /// </summary>
        [Required]
        public string cmd { get; set; }


        /// <summary>
        /// the file content return by readAsDataURL
        /// </summary>
        public string FileContent { get; set; }
    }
}