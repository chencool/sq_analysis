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
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Path { get; set; }

        public int Level { get; set; }

        public ProjectFolderViewModel()
        {

        }

        public ProjectFolderViewModel(string path, bool isTemplated, SearchOption searchOption, ShqContext db, int folderLevel = 1)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);

            if (ShqConstants.IsGuidEnded(path))
            {
                this.Name = System.IO.Path.GetFileNameWithoutExtension(this.Name);
                this.Id = new Guid(System.IO.Path.GetExtension(path).Replace(".", ""));

                CreatedBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == db.ProjectFiles.FirstOrDefault(item => item.Id == this.Id).CreatedById).FirstOrDefault(), db);
                CreatedTime = db.ProjectFiles.FirstOrDefault(item => item.Id == this.Id).CreatedTime.ToString();

                LastModifiedBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == db.ProjectFiles.FirstOrDefault(item => item.Id == this.Id).LastModifiedById).FirstOrDefault(), db);
                LastModfiedTime = db.ProjectFiles.FirstOrDefault(item => item.Id == this.Id).LastModfiedTime.ToString();

                this.Level = db.ProjectFiles.FirstOrDefault(item => item.Id == this.Id).Level;
            }

            if ((searchOption == SearchOption.TopDirectoryOnly && folderLevel == 1) || searchOption == SearchOption.AllDirectories)
            {
                string[] directory, files;
                if (isTemplated == true)
                {
                    directory = Directory.GetDirectories(System.IO.Path.Combine(ShqConstants.TemplateRootFolder, path));
                    files = Directory.GetFiles(System.IO.Path.Combine(ShqConstants.TemplateRootFolder, path));
                }
                else
                {
                    directory = Directory.GetDirectories(System.IO.Path.Combine(ShqConstants.ProjectRootFolder, path));
                    files = Directory.GetFiles(System.IO.Path.Combine(ShqConstants.ProjectRootFolder, path));
                }

                if (directory != null && directory.Length > 0)
                {
                    foreach (var d in directory)
                    {
                        this.SubFolders.Add(new ProjectFolderViewModel(System.IO.Path.Combine(path, System.IO.Path.GetFileName(d)), isTemplated, searchOption, db, folderLevel));
                    }
                }

                if (files != null && files.Length > 0)
                {
                    foreach (var f in files)
                    {
                        this.Files.Add(new ProjectFileViewModel(System.IO.Path.Combine(path, System.IO.Path.GetFileName(f)), isTemplated, db));
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