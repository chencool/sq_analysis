using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using Dxc.Shq.WebApi.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Dxc.Shq.WebApi.Controllers
{
    public class ShqUsersController : ApiController
    {
        private ShqContext db = new ShqContext();

        [HttpGet]
        [Route("api/ShqUsers/All")]
        public IQueryable<ShqUserRespondViewModel> GetShqUses()
        {
            var list = db.ShqUsers.ToList();
            List<ShqUserRespondViewModel> toList = new List<ShqUserRespondViewModel>();
            foreach (var item in list)
            {
                toList.Add(new ShqUserRespondViewModel(item, db));
            }

            return toList.AsQueryable();
        }

        [HttpGet]
        [Route("api/ShqUsers")]
        [ResponseType(typeof(ShqUserRespondViewModel))]
        public async Task<IHttpActionResult> GetShqUserByStatus(int status)
        {
            ShqUser shqUser = await db.ShqUsers.Where(item => item.Status == status).Include("IdentityUser").FirstOrDefaultAsync();
            if (shqUser == null)
            {
                return NotFound();
            }

            return Ok(new ShqUserRespondViewModel(shqUser, db));
        }


        [HttpGet]
        [Route("api/ShqUsers")]
        [ResponseType(typeof(ShqUserRespondViewModel))]
        public async Task<IHttpActionResult> GetShqUserByEmail(string email)
        {
            ShqUser shqUser = await db.ShqUsers.Where(item => item.IdentityUser.Email == email).Include("IdentityUser").FirstOrDefaultAsync();
            if (shqUser == null)
            {
                return NotFound();
            }

            return Ok(new ShqUserRespondViewModel(shqUser, db));
        }

        [Authorize(Roles = ShqConstants.AdministratorRole)]
        [HttpPost]
        [Route("api/ShqUsers/Add")]
        [ResponseType(typeof(ShqUserRespondViewModel))]
        public async Task<IHttpActionResult> Add(ShqUserRequestViewModel shqUserView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(shqUserView.Password) == true)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Password is invalide"));
            }

            if (shqUserView.Roles == null || shqUserView.Roles.Count < 1)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "User role not specified"));
            }

            if (db.Users.Where(u => u.UserName.ToLower() == shqUserView.EmailAddress.ToLower()).FirstOrDefault() != null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Name is duiplicated"));
            }

            var user = db.Users.Add(new IdentityUser(shqUserView.EmailAddress) { Email = shqUserView.EmailAddress, EmailConfirmed = true });
            foreach (var role in shqUserView.Roles)
            {
                string roleid = db.Roles.First(c => c.Name == role).Id;
                user.Roles.Add(new IdentityUserRole { RoleId = roleid });
            }

            var shqUser = shqUserView.ToShqUser();
            shqUser.IdentityUserId = user.Id;
            shqUser.IdentityUser = user;
            shqUser.CreatedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
            shqUser.LastModifiedById = shqUser.CreatedById;
            db.ShqUsers.Add(shqUser);

            var store = new ShqUserStore();
            await store.SetPasswordHashAsync(user, new ShqUserManager().PasswordHasher.HashPassword(shqUserView.Password));

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ShqUserExists(shqUser.IdentityUserId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new ShqUserRespondViewModel(shqUser, db));
        }

        [HttpPut]
        [Route("api/ShqUsers/Update")]
        [ResponseType(typeof(ShqUserRespondViewModel))]
        public async Task<IHttpActionResult> Update(ShqUserRequestViewModel shqUserView)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            if (HttpContext.Current.User.Identity.Name != shqUserView.EmailAddress
                && HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            ShqUser shqUser = await db.ShqUsers.Where(item => item.IdentityUser.Email == shqUserView.EmailAddress).Include("IdentityUser").FirstOrDefaultAsync();
            if (shqUser == null)
            {
                return NotFound();
            }
            else
            {
                shqUser.Address = shqUserView.Address;
                shqUser.Description = shqUserView.Description;
                //shqUser.EmailAddress = shqUserView.EmailAddress;
                shqUser.Status = shqUserView.Status;
                shqUser.PhoneNumber = shqUserView.PhoneNumber;
                shqUser.RealName = shqUserView.RealName;
                shqUser.Gender = shqUserView.Gender;
                shqUser.JobLevel = shqUserView.JobLevel;
                shqUser.Department = shqUserView.Department;
                shqUser.IdentityUser.Email = shqUserView.EmailAddress;
                shqUser.IdentityUser.PhoneNumber = shqUserView.PhoneNumber;

                shqUser.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                shqUser.LastModfiedTime = DateTime.Now;
                //var store = new ShqUserStore();
                //await store.SetPasswordHashAsync(shqUser.IdentityUser, new ShqUserManager().PasswordHasher.HashPassword(shqUserView.Password));

                await db.SaveChangesAsync();
            }

            return Ok(new ShqUserRespondViewModel(shqUser, db));
        }

        [HttpPut]
        [Route("api/ShqUsers/ChangePassword")]
        [ResponseType(typeof(ShqUserRespondViewModel))]
        public async Task<IHttpActionResult> ChangePassword(ShqUserPasswordViewModel password)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            if (HttpContext.Current.User.Identity.Name != password.EmailAddress
                && HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            ShqUser shqUser = await db.ShqUsers.Where(item => item.IdentityUser.Email == password.EmailAddress).Include("IdentityUser").FirstOrDefaultAsync();
            if (shqUser == null)
            {
                return NotFound();
            }
            else
            {
                var userManager = new ShqUserManager();
                IdentityResult result = null;
                if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
                {
                    result = await userManager.ChangePasswordAsync(shqUser.IdentityUser.Id, password.OldPassword, password.NewPassword);

                }
                else
                {
                    await userManager.RemovePasswordAsync(shqUser.IdentityUser.Id);
                    result = await userManager.AddPasswordAsync(shqUser.IdentityUser.Id, password.NewPassword);
                }

                if (result.Succeeded == true)
                {

                    shqUser.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                    shqUser.LastModfiedTime = DateTime.Now;
                    await db.SaveChangesAsync();

                    return Ok(new ShqUserRespondViewModel(shqUser, db));
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Join(",", result.Errors.ToArray())));
                }
            }
        }


        [Authorize(Roles = ShqConstants.AdministratorRole)]
        [HttpPut]
        [Route("api/ShqUsers/Disable")]
        [ResponseType(typeof(ShqUserRespondViewModel))]
        public async Task<IHttpActionResult> DisableShqUser(string email)
        {
            ShqUser shqUser = await db.ShqUsers.FirstOrDefaultAsync(item => item.EmailAddress == email);
            if (shqUser == null)
            {
                return NotFound();
            }

            shqUser.Status = ShqConstants.UserStatusDisable;
            shqUser.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
            shqUser.LastModfiedTime = DateTime.Now;
            //db.ShqUsers.Remove(shqUser);
            await db.SaveChangesAsync();

            return Ok(new ShqUserRespondViewModel(shqUser, db));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ShqUserExists(string id)
        {
            return db.ShqUsers.Count(e => e.IdentityUserId == id) > 0;
        }
    }
}