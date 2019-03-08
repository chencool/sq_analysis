using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Dxc.Shq.WebApi.Core
{
    using System;
    using System.Data.Entity.Migrations;
    using Models;

    public class Configuration : DbMigrationsConfiguration<ShqContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;

            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
            CodeGenerator = new MySql.Data.Entity.MySqlMigrationCodeGenerator();
        }

        protected override void Seed(ShqContext context)
        {
            string adminRoleId;
            //string pmId;
            //string eid;
            string userRoleId;
            if (!context.Roles.Any())
            {
                adminRoleId = context.Roles.Add(new IdentityRole(ShqConstants.AdministratorRole)).Id;
                //pmId = context.Roles.Add(new IdentityRole("ProjectManager")).Id;
                //eid = context.Roles.Add(new IdentityRole("Engineer")).Id;
                userRoleId = context.Roles.Add(new IdentityRole(ShqConstants.UserRole)).Id;
            }
            else
            {
                adminRoleId = context.Roles.First(c => c.Name == ShqConstants.AdministratorRole).Id;
                //pmId = context.Roles.First(c => c.Name == "ProjectManager").Id;
                //eid = context.Roles.First(c => c.Name == "Engineer").Id;
                userRoleId = context.Roles.First(c => c.Name == "User").Id;
            }

            if(!context.FTANoteTypes.Any())
            {
                context.FTANoteTypes.Add(new FTANoteType() { Id = 1, Description = "ROOT" });
                context.FTANoteTypes.Add(new FTANoteType() { Id = 2, Description = "BRANCH" });
                context.FTANoteTypes.Add(new FTANoteType() { Id = 3, Description = "LEAF" });
            }

            if (!context.FTANoteGateTypes.Any())
            {
                context.FTANoteGateTypes.Add(new FTANoteGateType() { Id = 1, Description = "AND" });
                context.FTANoteGateTypes.Add(new FTANoteGateType() { Id = 2, Description = "OR" });
                context.FTANoteGateTypes.Add(new FTANoteGateType() { Id = 3, Description = "XOR" });
            }

            if (!context.FTAEventTypes.Any())
            {
                context.FTAEventTypes.Add(new FTAEventType() { Id = 1, Name = "单点事件" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 2, Name = "双点事件" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 3, Name = "安全事件" });
            }

            if (!context.FTAFailureTypes.Any())
            {
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 1, Name = "单点故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 2, Name = "残余故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 3, Name = "潜伏故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 4, Name = "可探测双点故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 5, Name = "安全故障" });
            }

            context.SaveChanges();

            if (!context.Users.Any())
            {

                var shqUser = CreateUser(context, "admin@dxc.com", "admin@dxc.com", "123456", adminRoleId, null);
                //CreateUser(context, "YangSong", "YangSong@saicmotor.com", "123456", adminRoleId, shqUser);
                //CreateUser(context, "ZhaoXin", "zhaoxin01@saicmotor.com", "123456", adminRoleId, shqUser);
                //CreateUser(context, "pm01", "pm01@saicmotor.com", "123456", userRoleId, shqUser);
                //CreateUser(context, "pm02", "pm02@saicmotor.com", "123456", userRoleId, shqUser);
                //CreateUser(context, "pm03", "pm03@saicmotor.com", "123456", userRoleId, shqUser);
                //CreateUser(context, "e01", "e01@saicmotor.com", "123456", userRoleId, shqUser);
                //CreateUser(context, "e02", "e02@saicmotor.com", "123456", userRoleId, shqUser);
                //CreateUser(context, "e03", "e03@saicmotor.com", "123456", userRoleId, shqUser);
                CreateUser(context, "testuser01", "testuser01@saicmotor.com", "123456", userRoleId, shqUser);
            }

            context.SaveChanges();
        }

        public ShqUser CreateUser(ShqContext context, string userName, string email, string password, string role, ShqUser CreatedBy)
        {
            var user = context.Users.Add(new IdentityUser(userName) { Email = email, EmailConfirmed = true });
            user.Roles.Add(new IdentityUserRole { RoleId = role });

            var shqUser = new ShqUser() { IdentityUserId = user.Id, IdentityUser = user,RealName= userName, CreatedTime = DateTime.Now, Status = ShqConstants.UserStatusAvailable, EmailAddress = email };
            shqUser.CreatedById = CreatedBy == null ? shqUser.IdentityUserId : CreatedBy.IdentityUserId;
            context.ShqUsers.Add(shqUser);

            context.SaveChanges();

            var store = new ShqUserStore();
            store.SetPasswordHashAsync(user, new ShqUserManager().PasswordHasher.HashPassword(password));

            return shqUser;
        }
    }
}