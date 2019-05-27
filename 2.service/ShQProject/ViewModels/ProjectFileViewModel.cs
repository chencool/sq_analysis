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
            this.Name = ProjectFileViewModel.GetExc(System.IO.Path.GetFileName(path), 4);
            this.Path = path;
            this.Level = int.Parse(ProjectFileViewModel.GetExc(System.IO.Path.GetFileName(path), 1));

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

        private static string GetExc(string name, int index)
        {
            string ext = System.IO.Path.GetExtension(name);

            if (index == 1)
            {
                return ext.Replace(".", "");
            }

            string temp = System.IO.Path.GetFileNameWithoutExtension(name);
            if (index == 2)
            {

                return GetExc(temp, 1); ;
            }

            temp = System.IO.Path.GetFileNameWithoutExtension(temp);
            if (index == 3)
            {
                return GetExc(temp, 1); ;
            }

            temp = System.IO.Path.GetFileNameWithoutExtension(temp);
            if (index == 4)
            {
                return temp;
            }

            return ShqConstants.DefaultFileLevel.ToString();
        }
    }
}