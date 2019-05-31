using Dxc.Shq.WebApi.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{

    public class ProjectFileViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Path { get; set; }

        public int Level { get; set; }

        public long Size { get; set; }

        public ProjectFileViewModel()
        {

        }

        public ProjectFileViewModel(string path, bool isTemplated, ShqContext db)
        {
            this.Name = System.IO.Path.GetFileName(path);
            this.Path = path;

            string rpath;
            if (isTemplated == true)
            {
                rpath = System.IO.Path.Combine(ShqConstants.TemplateRootFolder, path);
            }
            else
            {
                rpath = System.IO.Path.Combine(ShqConstants.ProjectRootFolder, path);
            }

            FileInfo fi = new FileInfo(rpath);
            this.Size = fi.Length;
            this.CreatedTime = fi.CreationTime.ToString();
            this.LastModfiedTime = fi.LastWriteTime.ToString();

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
        }

        public ShqUserRequestViewModel CreatedBy { get; set; }
        public string CreatedTime { get; set; }

        public ShqUserRequestViewModel LastModifiedBy { get; set; }
        public string LastModfiedTime { get; set; }
    }
}