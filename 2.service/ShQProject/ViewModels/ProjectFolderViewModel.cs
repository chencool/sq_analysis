using Dxc.Shq.WebApi.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ProjectFolderViewModel
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public int Level { get; set; }

        /// <summary>
        /// 0 template
        /// 1 project
        /// </summary>
        public int FolderType { get; set; }

        public ProjectFolderViewModel()
        {

        }

        public ProjectFolderViewModel(string path, int folderType)
        {
            string[] directory, files;
            if (folderType == 0)
            {
                directory = Directory.GetDirectories(System.IO.Path.Combine(ShqConstants.TemplateRootFolder, path));
                files = Directory.GetFiles(System.IO.Path.Combine(ShqConstants.TemplateRootFolder, path));
            }
            else
            {
                directory = Directory.GetDirectories(System.IO.Path.Combine(ShqConstants.ProjectRootFolder, path));
                files = Directory.GetFiles(System.IO.Path.Combine(ShqConstants.ProjectRootFolder, path));
            }

            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
            this.Level = 0;//todo
            this.FolderType = folderType;

            if (directory != null && directory.Length > 0)
            {
                foreach (var d in directory)
                {
                    this.SubFolders.Add(new ProjectFolderViewModel(System.IO.Path.Combine(path, System.IO.Path.GetFileName(d)), folderType));
                }
            }

            if (files != null && files.Length > 0)
            {
                foreach (var f in files)
                {
                    this.Files.Add(new ProjectFileViewModel(System.IO.Path.Combine(path, System.IO.Path.GetFileName(f)), folderType));
                }
            }
        }

        public List<ProjectFolderViewModel> SubFolders = new List<ProjectFolderViewModel>();
        public List<ProjectFileViewModel> Files = new List<ProjectFileViewModel>();

        public ShqUserRequestViewModel CreatedBy { get; set; }
        public string CreatedTime { get; set; }

        public ShqUserRequestViewModel LastModifiedBy { get; set; }
        public string LastModfiedTime { get; set; }
    }
}