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
        /// <param name="path">the path to seach, "templates\.." mean tempalte folder, "projects\{projectid}\..." mean project folders</param>
        /// <param name="searchOption">0 for sub folder, 1 for all children folders</param>
        /// <returns></returns>
        [Route("api/ProjectFiles")]
        public async Task<IHttpActionResult> GetProjectFiles(string path, SearchOption searchOption)
        {
            bool isTemplated = false;

            if (path.StartsWith("projects"))
            {
                Guid projectId = new Guid(path.Split('\\')[1]);

                Project pro = await db.Projects.FindAsync(projectId);
                if (pro == null)
                {
                    return NotFound();
                }

                if (ProjectHelper.HasReadAccess(pro) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }
            }
            else if (path.StartsWith("templates"))
            {
                if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
                }

                isTemplated = true;
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "path in error format"));
            }

            return Ok(new ProjectFolderViewModel(path, isTemplated, searchOption, db));
        }

        /// <summary>
        /// explorer ops
        /// </summary>
        /// <param name="explorerInfo">JSON string by ExplorerInfoViewModel defined</param>
        /// <returns></returns>
        [Route("api/ProjectFiles/Update")]
        [HttpPost]
        public HttpResponseMessage Update(string explorerInfo)
        {
            ExplorerInfoViewModel einfo = JsonConvert.DeserializeObject<ExplorerInfoViewModel>(explorerInfo);
            Guid projectId;
            string folder;
            if (Guid.TryParse(einfo.ProjectId, out projectId) == true)
            {
                folder = Path.Combine(ShqConstants.ProjectRootFolder, einfo.ParentPath);
            }
            else
            {
                folder = Path.Combine(ShqConstants.TemplateRootFolder, einfo.ParentPath);
            }
            folder = new DirectoryInfo(Path.Combine(folder, "1b2cd8ab-6d6c-4a05-931b-e40607bd8b19")).Parent.FullName;//to workaround a issue the if path end with \ will fail

            string name = einfo.Name + "." + einfo.Id.ToString();
            string oldName = einfo.OldName + "." + einfo.Id.ToString();

            switch (einfo.cmd)
            {
                case "createFolder":
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
                        string target = root.VolumeId + Helper.EncodePath(new DirectoryInfo(folder).Name);

                        driver.MakeDir(target, name);
                        db.ProjectFiles.Add(new ProjectFile
                        {
                            Id = einfo.Id,
                            Name = einfo.Name,
                            Level = einfo.Level,
                            IsFolder = true,
                            Path = Path.Combine(folder, name),
                            CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId,
                            LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                        });
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
                        var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id);
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
                        var f = db.ProjectFiles.FirstOrDefault(item => item.Id == einfo.Id);
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

                        var wrapper = new HttpRequestWrapper(HttpContext.Current.Request);
                        driver.Upload(target, wrapper.Files);
                        break;
                    }
                case "dowloadFile":
                    {
                        folder = Path.Combine(folder, einfo.Name);
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
            }

            db.SaveChanges();

            return new HttpResponseMessage(HttpStatusCode.OK);
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