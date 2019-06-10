using Dxc.Shq.WebApi.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ProjectFilePath
    {
        public string Path { get; set; }

        public string FullPath { get; set; }
    }

    public class ProjectFolderViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Path { get; set; }

        public int Level { get; set; }

        public int Privilege { get; set; }

        public ProjectFolderViewModel()
        {

        }

        public ProjectFolderViewModel(ProjectFilePath path, SearchOption searchOption, ShqContext db, int folderLevel = 1)
        {
            this.Path = path.Path;
            this.Name = System.IO.Path.GetFileName(path.Path);

            Id = ShqConstants.GetPathId(path.Path);
            if (Id > 0)
            {

                this.Name = System.IO.Path.GetFileNameWithoutExtension(this.Name);
                var f = db.ProjectFiles.FirstOrDefault(item => item.Id == Id);
                if (f == null)
                {
                    this.Name = "";
                }
                else
                {
                    CreatedBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == db.ProjectFiles.FirstOrDefault(item => item.Id == Id).CreatedById).FirstOrDefault(), db);
                    CreatedTime = f.CreatedTime.ToString();

                    LastModifiedBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == db.ProjectFiles.FirstOrDefault(item => item.Id == Id).LastModifiedById).FirstOrDefault(), db);
                    LastModfiedTime = f.LastModfiedTime.ToString();

                    this.Privilege = f.Privilege;
                    this.Level = f.Level;
                }
            }

            if (this.Name == "")
            {
                return;
            }

            if ((searchOption == SearchOption.TopDirectoryOnly && folderLevel == 1) || searchOption == SearchOption.AllDirectories)
            {
                string[] directory, files;
                directory = Directory.GetDirectories(path.FullPath);
                files = Directory.GetFiles(path.FullPath);

                if (directory != null && directory.Length > 0)
                {
                    foreach (var d in directory)
                    {
                        var pf = new ProjectFolderViewModel(new ProjectFilePath { FullPath = d, Path = System.IO.Path.Combine(path.Path, System.IO.Path.GetFileName(d)) }, searchOption, db, folderLevel);
                        if(pf.Name != "")
                        {
                            this.SubFolders.Add(pf);
                        }
                    }
                }

                if (files != null && files.Length > 0)
                {
                    foreach (var f in files)
                    {
                        var ff = new ProjectFileViewModel(new ProjectFilePath { FullPath = f, Path = System.IO.Path.Combine(path.Path, System.IO.Path.GetFileName(f)) }, db);
                        if(ff.Name != "")
                        {
                            this.Files.Add(ff);
                        }
                    }
                }

                folderLevel--;
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