﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ExplorerInfoViewModel
    {

        /// <summary>
        /// the folder or file id is generated by the backend and you could find it by the API "api/ProjectFiles"
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// the path must be in the format "folder1\\\\folder2\\\\.....\\\\folderN" 
        /// please node in this format, it use four \ and it start/end without any \
        /// your could use following JS method to get this format
        /// x.path.split("\\").join("\\\\")   
        /// and you could use API "api/ProjectFiles" to get the folder or file's path
        /// </summary>
        [Required]
        public string TartgetPath { get; set; }

        [Required]
        /// <summary>
        /// project id, the default template project id is always 1b2cd8ab-6d6c-4a05-931b-e40607bd8b19
        /// the default template will be used by all projects and managed by admin only.
        /// All other project files will be copied from this template by its level.
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