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
            Guid? projectId;
            Guid worktemplateid = Guid.Empty;
            if (pro != null)
            {
                if (ProjectHelper.HasReadAccess(pro) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }

                folder = ShqConstants.ProjectRootFolder + "\\" + einfo.ProjectId + "\\" + einfo.ParentPath;
                wp = db.WorkProjects.FirstOrDefault(item => item.Id == einfo.ProjectId);
                projectId = einfo.ProjectId;
                worktemplateid = wp.WorkProjectTemplateId;
            }
            else if (db.WorkProjectTemplates.FirstOrDefault(item => item.Id == einfo.ProjectId) != null)
            {
                if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }

                projectId = null;
                worktemplateid = einfo.ProjectId;
                folder = ShqConstants.TemplateRootFolder + "\\" + worktemplateid + "\\" + einfo.ParentPath;
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

                        var dbFile = db.ProjectFiles.FirstOrDefault(item => item.WorkProjectId == projectId && item.WorkProjectTemplateId == worktemplateid && item.Name == einfo.Name);
                        if (dbFile != null)
                        {
                            break;
                        }
                        else
                        {
                            var projectFile = db.ProjectFiles.Add(new ProjectFile
                            {
                                FileId = Guid.NewGuid(),
                                Name = einfo.Name,
                                Level = einfo.Level,
                                IsFolder = true,
                                Path = Path.Combine(folder, einfo.Name),
                                WorkProjectId = projectId,
                                WorkProjectTemplateId = worktemplateid,
                                CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                                LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                            });

                            if (wp != null)
                            {
                                wp.ProjectFiles.Add(projectFile);
                            }

                            db.SaveChanges();

                            name = einfo.Name + "." + projectFile.Id.ToString();
                            string target = root.VolumeId + Helper.EncodePath(new DirectoryInfo(folder).Name);

                            try
                            {
                                driver.MakeDir(target, name);
                            }
                            finally
                            {
                                if (Directory.Exists(Path.Combine(folder, name)))
                                {
                                    projectFile.Path = Path.Combine(folder, name);
                                }
                                else
                                {
                                    db.ProjectFiles.Remove(projectFile);
                                }
                            }
                        }

                        break;
                    }
                case "delete":
                    {
                        var fileSystemDriver = new FileSystemDriver();
                        IDriver driver = fileSystemDriver;
                        folder = Path.Combine(folder, name);
                        var root = new Root(new DirectoryInfo(folder))
                        {
                            IsReadOnly = false,
                            Alias = "Root",
                            MaxUploadSizeInMb = 500,
                            LockedFolders = new List<string>()
                        };
                        fileSystemDriver.AddRoot(root);
                        string target = root.VolumeId + Helper.EncodePath(new DirectoryInfo(folder).Name);

                        driver.Remove(new string[] { target });
                        var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id && item.WorkProjectId == projectId && item.WorkProjectTemplateId == worktemplateid);
                        if (f != null)
                        {
                            f.Status = 1;
                            f.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                            f.LastModfiedTime = DateTime.Now;
                        }
                        break;
                    }
                case "rename":
                    {
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
                        string target = root.VolumeId + Helper.EncodePath(@"\" + oldName);

                        driver.Rename(target, name);
                        var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id && item.WorkProjectId == projectId && item.WorkProjectTemplateId == worktemplateid);
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

                        var dbFile = db.ProjectFiles.FirstOrDefault(item => item.WorkProjectId == projectId && item.WorkProjectTemplateId == worktemplateid && item.Name == einfo.Name);
                        if (dbFile == null)
                        {
                            dbFile = db.ProjectFiles.Add(new ProjectFile
                            {
                                FileId = Guid.NewGuid(),
                                Name = einfo.Name,
                                Level = einfo.Level,
                                IsFolder = false,
                                Path = Path.Combine(folder, name),
                                WorkProjectId = projectId,
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
                        folder = Path.Combine(folder, einfo.Name);
                        if (File.Exists(folder) == false)
                        {
                            return new HttpResponseMessage(HttpStatusCode.NotFound);
                        }

                        var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id);
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
            string path = ShqConstants.ProjectRootFolder + "\\" + "projects\\" + projectId;
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            var wp = db.WorkProjects.Include("WorkProjectTemplate").FirstOrDefault(item => item.Id == projectId);
            var sourceFiles = wp.WorkProjectTemplate.ProjectFiles.Where(item => item.Status != 1 && item.Level <= wp.Level);
            wp.FilesToCopyNum = sourceFiles.Count();
            wp.FilesCopiedNum = 0;
            foreach (var file in sourceFiles)
            {
                string[] folers = file.Path.Split('\\');
                string tempPath = path;
                int to = file.IsFolder == true ? folers.Length : folers.Length - 1;
                for (int i = 1; i < to; i++)
                {
                    wp.FilesCopiedNum++;
                    await db.SaveChangesAsync();
                    tempPath = Path.Combine(path, folers[i]);

                    if (Directory.Exists(tempPath) == false)
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    string name = System.IO.Path.GetFileName(path);
                    name = System.IO.Path.GetFileNameWithoutExtension(name);
                    Guid id = new Guid(System.IO.Path.GetExtension(path).Replace(".", ""));

                    var f = db.ProjectFiles.FirstOrDefault(item => item.FileId == id && item.WorkProjectTemplateId == wp.WorkProjectTemplateId && item.WorkProjectId == wp.Id);
                    if (f == null)
                    {
                        var projectFile = db.ProjectFiles.Add(new ProjectFile
                        {
                            FileId = id,
                            Name = name,
                            Level = file.Level,
                            IsFolder = file.IsFolder,
                            Path = tempPath,
                            WorkProjectTemplateId = wp.WorkProjectTemplateId,
                            WorkProjectId = wp.Id,
                            CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                            LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                        });

                        wp.ProjectFiles.Add(projectFile);
                    }
                }

                if (file.IsFolder == false)
                {
                    wp.FilesCopiedNum++;
                    await db.SaveChangesAsync();

                    File.Copy(file.Path, tempPath);

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
                            CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                            LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                        });

                        wp.ProjectFiles.Add(projectFile);
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