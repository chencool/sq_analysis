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
        public string Name { get; set; }

        public string Path { get; set; }

        public int Level { get; set; }

        public long Size { get; set; }

        public ProjectFileViewModel()
        {

        }

        public ProjectFileViewModel(string path, int folderType)
        {
            this.Name = System.IO.Path.GetFileName(path);
            this.Path = path;
            this.Level = 0;// todo from name

            string rpath;
            if (folderType == 0)
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
        }

        public ShqUserRequestViewModel CreatedBy { get; set; }
        public string CreatedTime { get; set; }

        public ShqUserRequestViewModel LastModifiedBy { get; set; }
        public string LastModfiedTime { get; set; }
    }
}