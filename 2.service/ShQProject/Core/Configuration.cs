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

            if(!context.FTANodeTypes.Any())
            {
                context.FTANodeTypes.Add(new FTANodeType() { Id = 1, Description = "ROOT" });
                context.FTANodeTypes.Add(new FTANodeType() { Id = 2, Description = "BRANCH" });
                context.FTANodeTypes.Add(new FTANodeType() { Id = 3, Description = "LEAF" });
            }

            if (!context.FTANodeGateTypes.Any())
            {
                context.FTANodeGateTypes.Add(new FTANodeGateType() { Id = 1, Description = "AND" });
                context.FTANodeGateTypes.Add(new FTANodeGateType() { Id = 2, Description = "OR" });
                context.FTANodeGateTypes.Add(new FTANodeGateType() { Id = 3, Description = "XOR" });
            }

            if (!context.FTAEventTypes.Any())
            {
                context.FTAEventTypes.Add(new FTAEventType() { Id = 1, EventName = "单点事件" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 2, EventName = "双点事件" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 3, EventName = "安全事件" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 4, EventName = "节点失效概率q",Description="存放各个节点的失效概率，可能由用户输入，或者计算获得" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 5, EventName = "顶事件" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 6, EventName = "底事件" });
                context.FTAEventTypes.Add(new FTAEventType() { Id = 7, EventName = "最小割集" });
            }

            if (!context.FTAFailureTypes.Any())
            {
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 1, FailureTypeName = "单点故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 2, FailureTypeName = "残余故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 3, FailureTypeName = "潜伏故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 4, FailureTypeName = "可探测双点故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 5, FailureTypeName = "安全故障" });
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 6, FailureTypeName = "单点故障度量SPFM",Description= "存放根节点的单点故障度量SPFM"});
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 7, FailureTypeName = "潜伏故障度量LFM", Description = "存放根节点的潜伏故障度量LFM"});
                context.FTAFailureTypes.Add(new FTAFailureType() { Id = 8, FailureTypeName = "PMHF", Description = "存放根节点的PMHF" });
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