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
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public int Level { get; set; }

        public long Size { get; set; }

        public ProjectFileViewModel()
        {

        }

        public ProjectFileViewModel(ProjectFilePath path, ShqContext db)
        {
            this.Name = System.IO.Path.GetFileName(path.Path);
            this.Path = path.Path;

            FileInfo fi = new FileInfo(path.FullPath);
            this.Size = fi.Length;
            this.CreatedTime = fi.CreationTime.ToString();
            this.LastModfiedTime = fi.LastWriteTime.ToString();

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

                    this.Level = f.Level;
                }
            }
        }

        public ShqUserRequestViewModel CreatedBy { get; set; }
        public string CreatedTime { get; set; }

        public ShqUserRequestViewModel LastModifiedBy { get; set; }
        public string LastModfiedTime { get; set; }
    }
}