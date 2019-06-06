using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using Dxc.Shq.WebApi.ViewModels;
using ElFinder;
using Newtonsoft.Json;

namespace Dxc.Shq.WebApi.Controllers
{
    public class ProjectFilesController : ApiController
    {
        private ShqContext db = new ShqContext();

        /// <summary>
        /// get files
        /// </summary>
        /// <param name="projectId">the project id, the default template id is 1b2cd8ab-6d6c-4a05-931b-e40607bd8b19</param>
        /// <param name="path">the path to seach, the path is started by Root folder</param>
        /// <param name="searchOption">0 for sub folder, 1 for all children folders</param>
        /// <returns></returns>
        [Route("api/ProjectFiles")]
        public async Task<IHttpActionResult> GetProjectFiles(Guid projectId, string path, SearchOption searchOption)
        {
            bool isTemplated = false;
            string fullPath;
            if (string.IsNullOrEmpty(path) == true || path.StartsWith("Root") == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "path must start with Root"));
            }

            var pro = db.Projects.FirstOrDefault(item => item.Id == projectId);
            if (pro != null)
            {
                if (ProjectHelper.HasReadAccess(pro) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }

                fullPath = Path.Combine(ShqConstants.ProjectRootFolder, projectId.ToString()) + "\\" + path;
            }
            else if (db.WorkProjectTemplates.FirstOrDefault(item => item.Id == projectId) != null)
            {
                if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }

                fullPath = Path.Combine(ShqConstants.TemplateRootFolder, projectId.ToString()) + "\\" + path;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "projectId is not found"));
            }

            return Ok(new ProjectFolderViewModel(new ProjectFilePath { FullPath = fullPath, Path = path }, searchOption, db));
        }

        /// <summary>
        /// explorer ops
        /// </summary>
        /// <param name="einfo">JSON string by ExplorerInfoViewModel defined</param>
        /// <returns></returns>
        [Route("api/ProjectFiles/Update")]
        [HttpPost]
        public HttpResponseMessage Update(ExplorerInfoViewModel einfo)
        {
            WorkProject wp = null;
            string folder;
            var pro = db.Projects.FirstOrDefault(item => item.Id == einfo.ProjectId);
            Guid? workProjectId;
            Guid worktemplateid = Guid.Empty;
            if (pro != null)
            {
                if (ProjectHelper.HasReadAccess(pro) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }

                folder = ShqConstants.ProjectRootFolder + "\\" + einfo.ProjectId + "\\" + einfo.TartgetPath;
                wp = db.WorkProjects.FirstOrDefault(item => item.ProjectId == einfo.ProjectId);
                workProjectId = wp.Id;
                worktemplateid = wp.WorkProjectTemplateId;
            }
            else if (db.WorkProjectTemplates.FirstOrDefault(item => item.Id == einfo.ProjectId) != null)
            {
                if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }

                workProjectId = null;
                worktemplateid = einfo.ProjectId;
                folder = ShqConstants.TemplateRootFolder + "\\" + worktemplateid + "\\" + einfo.TartgetPath;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "projectId is not found"));
            }

            folder = new DirectoryInfo(Path.Combine(folder, "1b2cd8ab-6d6c-4a05-931b-e40607bd8b19")).Parent.FullName;//to workaround a issue the if path end with \ will fail

            if (CheckIfParentExistInDb(folder) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The parent folder is not found in the db"));
            }

            string name = einfo.Name + "." + einfo.Id.ToString();
            string oldName = einfo.OldName + "." + einfo.Id.ToString();

            switch (einfo.cmd)
            {
                case "createFolder":
                    {
                        if (string.IsNullOrEmpty(einfo.Name) == true)
                        {
                            break;
                        }

                        var fileSystemDriver = new FileSystemDriver();
                        IDriver driver = fileSystemDriver;
                        var root = new Root(new DirectoryInfo(folder))
                        {
                            IsReadOnly = false,
                            Alias = "Root",
                            MaxUploadSizeInMb = 500,
                            LockedFolders = new List<string>()
                        };
                        fileSystemDriver.AddRoot(root);

                        var dbFile = db.ProjectFiles.FirstOrDefault(item => item.WorkProjectId == workProjectId
                        && item.WorkProjectTemplateId == worktemplateid
                        && item.Name == einfo.Name
                        && item.Path == folder + "\\" + einfo.Name + "." + item.Id);

                        if (dbFile != null)
                        {
                            dbFile.Status = (int)ShqConstants.FileStatus.Ready;
                            dbFile.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                            dbFile.LastModfiedTime = DateTime.Now;
                        }
                        else
                        {
                            dbFile = db.ProjectFiles.Add(new ProjectFile
                            {
                                FileId = Guid.NewGuid(),
                                Name = einfo.Name,
                                Level = einfo.Level,
                                IsFolder = true,
                                Path = Path.Combine(folder, einfo.Name),
                                WorkProjectId = workProjectId,
                                WorkProjectTemplateId = worktemplateid,
                                CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                                LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                            });

                            if (wp != null)
                            {
                                wp.ProjectFiles.Add(dbFile);
                            }

                            db.SaveChanges();
                        }

                        name = einfo.Name + "." + dbFile.Id.ToString();
                        string target = root.VolumeId + Helper.EncodePath(new DirectoryInfo(folder).Name);

                        try
                        {
                            driver.MakeDir(target, name);
                        }
                        finally
                        {
                            if (Directory.Exists(Path.Combine(folder, name)))
                            {
                                dbFile.Status = (int)ShqConstants.FileStatus.Ready;
                                dbFile.Path = Path.Combine(folder, name);
                            }
                            else
                            {
                                dbFile.Status = (int)ShqConstants.FileStatus.Deleted;
                                dbFile.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                                dbFile.LastModfiedTime = DateTime.Now;
                            }
                        }
                    }

                    break;
                case "delete":
                    {
                        var fileSystemDriver = new FileSystemDriver();
                        IDriver driver = fileSystemDriver;
                        //folder = Path.Combine(folder, name);
                        var root = new Root(new DirectoryInfo(folder).Parent)
                        {
                            IsReadOnly = false,
                            Alias = "Root",
                            MaxUploadSizeInMb = 500,
                            LockedFolders = new List<string>()
                        };
                        fileSystemDriver.AddRoot(root);
                        string target = root.VolumeId + Helper.EncodePath(@"\" + new DirectoryInfo(folder).Name);

                        driver.Remove(new string[] { target });
                        if (Directory.Exists(folder) == false && File.Exists(folder) == false)
                        {
                            //var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id
                            //&& item.WorkProjectId == projectId
                            //&& item.WorkProjectTemplateId == worktemplateid);
                            //if (f != null)
                            //{
                            //    f.Status = 1;
                            //    f.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                            //    f.LastModfiedTime = DateTime.Now;
                            //}

                            string userId = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                            var fs = db.ProjectFiles.Where(item => item.Path.Contains(folder) == true
                                && item.WorkProjectId == workProjectId
                                && item.WorkProjectTemplateId == worktemplateid);
                            foreach (var f in fs)
                            {
                                f.Status = 1;
                                f.LastModifiedById = userId;
                                f.LastModfiedTime = DateTime.Now;
                            }
                        }

                        break;
                    }
                case "rename":
                    {
                        var fileSystemDriver = new FileSystemDriver();
                        IDriver driver = fileSystemDriver;
                        DirectoryInfo rootPath = new DirectoryInfo(folder).Parent;
                        var root = new Root(rootPath)
                        {
                            IsReadOnly = false,
                            Alias = "Root",
                            MaxUploadSizeInMb = 500,
                            LockedFolders = new List<string>()
                        };
                        fileSystemDriver.AddRoot(root);
                        string target = root.VolumeId + Helper.EncodePath(@"\" + oldName);

                        var dbFile = db.ProjectFiles.FirstOrDefault(item => item.WorkProjectId == workProjectId
                        && item.WorkProjectTemplateId == worktemplateid
                        && item.Name == einfo.Name
                        && item.Path == rootPath.FullName + "\\" + einfo.Name + "." + item.Id
                        && item.Status != (int)ShqConstants.FileStatus.Deleted);
                        if (dbFile != null && (File.Exists(dbFile.Path) || Directory.Exists(dbFile.Path)))
                        {
                            return new HttpResponseMessage(HttpStatusCode.Conflict);
                        }

                        driver.Rename(target, name);
                        var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id
                        && item.WorkProjectId == workProjectId
                        && item.WorkProjectTemplateId == worktemplateid);
                        if (f != null)
                        {
                            f.Name = einfo.Name;
                            f.Path = Path.Combine(Directory.GetParent(folder).FullName, name);
                            f.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                            f.LastModfiedTime = DateTime.Now;
                        }
                        break;
                    }
                case "uploadFile"://https://forums.asp.net/t/2104884.aspx?Uploading+a+file+using+webapi+C+
                    //https://shazwazza.com/post/uploading-files-and-json-data-in-the-same-request-with-angular-js/
                    {
                        var fileSystemDriver = new FileSystemDriver();
                        var root = new Root(new DirectoryInfo(folder))
                        {
                            IsReadOnly = false,
                            Alias = "Root",
                            MaxUploadSizeInMb = 500,
                            LockedFolders = new List<string>()
                        };
                        fileSystemDriver.AddRoot(root);

                        var dbFile = db.ProjectFiles.FirstOrDefault(item => item.WorkProjectId == workProjectId
                        && item.WorkProjectTemplateId == worktemplateid
                        && item.Name == einfo.Name
                        && item.Path == folder + "\\" + einfo.Name + "." + item.Id);

                        if (dbFile == null)
                        {
                            dbFile = db.ProjectFiles.Add(new ProjectFile
                            {
                                FileId = Guid.NewGuid(),
                                Name = einfo.Name,
                                Level = einfo.Level,
                                IsFolder = false,
                                Path = Path.Combine(folder, name),
                                WorkProjectId = workProjectId,
                                WorkProjectTemplateId = worktemplateid,
                                CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                                LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                            });

                            if (wp != null)
                            {
                                wp.ProjectFiles.Add(dbFile);
                            }

                            db.SaveChanges();
                        }
                        else
                        {
                            dbFile.Status = (int)ShqConstants.FileStatus.Ready;
                            dbFile.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                            dbFile.LastModfiedTime = DateTime.Now;
                        }

                        name = einfo.Name + "." + dbFile.Id.ToString();
                        string target = root.VolumeId + Helper.EncodePath(new DirectoryInfo(folder).Name);

                        try
                        {
                            byte[] bytes = Convert.FromBase64String(einfo.FileContent);
                            fileSystemDriver.Upload(target, name, bytes);
                        }
                        finally
                        {

                            if (File.Exists(Path.Combine(folder, name)))
                            {
                                if (File.Exists(Path.Combine(folder, name)))
                                {
                                    dbFile.Path = Path.Combine(folder, name);
                                }
                                else
                                {
                                    db.ProjectFiles.Remove(dbFile);
                                }
                            }
                        }

                        break;
                    }
                case "dowloadFile":
                    {
                        if (File.Exists(folder) == false)
                        {
                            return new HttpResponseMessage(HttpStatusCode.NotFound);
                        }

                        //converting Pdf file into bytes array  
                        var dataBytes = System.IO.File.ReadAllBytes(folder);
                        //adding bytes to memory stream   
                        var stream = new MemoryStream(dataBytes);

                        var result = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(stream.ToArray())
                        };
                        result.Content.Headers.ContentDisposition =
                            new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                            {
                                FileName = einfo.Name
                            };
                        result.Content.Headers.ContentType =
                            new MediaTypeHeaderValue("application/octet-stream");

                        return result;
                    }

                case "newLevel":
                    {
                        var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id && item.WorkProjectId == workProjectId && item.WorkProjectTemplateId == worktemplateid);
                        if (f != null)
                        {
                            f.Level = einfo.Level;
                            f.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                            f.LastModfiedTime = DateTime.Now;
                        }

                        break;
                    }
            }

            db.SaveChanges();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/ProjectFiles/Sync")]
        public async Task<IHttpActionResult> SyncProjectFiles(Guid projectId)
        {
            string path = ShqConstants.ProjectRootFolder + "\\" + projectId;
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            path = path + "\\Root";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            var wp = db.WorkProjects.Include("WorkProjectTemplate").FirstOrDefault(item => item.ProjectId == projectId);
            var sourceFiles = wp.WorkProjectTemplate.ProjectFiles.Where(item => item.Status == (int)ShqConstants.FileStatus.Ready && item.Level <= wp.Level).ToList();
            wp.FilesToCopyNum = sourceFiles.Count();
            wp.FilesCopiedNum = 0;

            string sourceRootFolder = Path.Combine(ShqConstants.TemplateRootFolder, wp.WorkProjectTemplate.Id.ToString()) + "\\Root";
            int start = sourceRootFolder.Split('\\').Length;
            foreach (var file in sourceFiles)
            {
                wp.FilesCopiedNum++;

                string[] folers = file.Path.Split('\\');
                string tempPath = path;
                int to = file.IsFolder == true ? folers.Length : folers.Length - 1;
                for (int i = start; i < to; i++)
                {
                    int id = ShqConstants.GetPathId(folers[i]);
                    var tf = db.ProjectFiles.FirstOrDefault(item => item.Id == id).FileId;

                    var f = db.ProjectFiles.FirstOrDefault(item => item.FileId == tf && item.WorkProjectTemplateId == wp.WorkProjectTemplateId && item.WorkProjectId == wp.Id);
                    if (f == null)
                    {
                        var projectFile = db.ProjectFiles.Add(new ProjectFile
                        {
                            FileId = file.FileId,
                            Name = file.Name,
                            Level = file.Level,
                            IsFolder = file.IsFolder,
                            Path = tempPath,
                            Status = (int)ShqConstants.FileStatus.Copying,
                            WorkProjectTemplateId = wp.WorkProjectTemplateId,
                            WorkProjectId = wp.Id,
                            CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                            LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                        });

                        wp.ProjectFiles.Add(projectFile);

                        db.SaveChanges();
                        tempPath = Path.Combine(tempPath, file.Name + "." + projectFile.Id.ToString());
                        if (Directory.Exists(tempPath) == false)
                        {
                            Directory.CreateDirectory(tempPath);
                            projectFile.Path = tempPath;
                            projectFile.Status = (int)ShqConstants.FileStatus.Ready;
                        }
                    }
                    else
                    {
                        if (f.Status != (int)ShqConstants.FileStatus.Deleted)
                        {
                            tempPath = Path.Combine(tempPath, f.Name + "." + f.Id.ToString());
                            if (Directory.Exists(tempPath) == false)
                            {
                                Directory.CreateDirectory(tempPath);
                                f.Status = (int)ShqConstants.FileStatus.Ready;
                                f.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                                f.LastModfiedTime = DateTime.Now;
                            }
                        }
                    }
                }

                if (file.IsFolder == false)
                {
                    var f = db.ProjectFiles.FirstOrDefault(item => item.FileId == file.FileId && item.WorkProjectTemplateId == wp.WorkProjectTemplateId && item.WorkProjectId == wp.Id);
                    if (f == null)
                    {
                        var projectFile = db.ProjectFiles.Add(new ProjectFile
                        {
                            FileId = file.FileId,
                            Name = file.Name,
                            Level = file.Level,
                            IsFolder = file.IsFolder,
                            Path = tempPath,
                            WorkProjectTemplateId = wp.WorkProjectTemplateId,
                            WorkProjectId = wp.Id,
                            Status = (int)ShqConstants.FileStatus.Copying,
                            CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                            LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                        });

                        wp.ProjectFiles.Add(projectFile);
                        db.SaveChanges();

                        string targetPath = tempPath + "\\" + projectFile.Name + "." + projectFile.Id;

                        if (File.Exists(targetPath) == false)
                        {
                            File.Copy(file.Path, targetPath);
                            projectFile.Status = (int)ShqConstants.FileStatus.Ready;
                            projectFile.Path = targetPath;
                        }
                    }
                    else
                    {
                        if (f.Status != (int)ShqConstants.FileStatus.Deleted)
                        {
                            string targetPath = tempPath + "\\" + f.Name + "." + f.Id;

                            if (File.Exists(targetPath) == false)
                            {
                                File.Copy(file.Path, targetPath);
                                f.Status = (int)ShqConstants.FileStatus.Ready;
                                f.Path = targetPath;
                                f.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                                f.LastModfiedTime = DateTime.Now;
                            }
                        }
                    }
                }
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        private bool CheckIfParentExistInDb(string parentPath)
        {
            if (parentPath.EndsWith("Root") == true)
            {
                return true;
            }

            int id = ShqConstants.GetPathId(parentPath);
            if (id > 0)
            {
                return db.ProjectFiles.FirstOrDefault(item => item.Id == id) != null;
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}