using Dxc.Shq.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Core
{
    public class ShqConstants
    {
        public const string AdministratorRole = "Administrator";
        public const string UserRole = "User";

        public const int NoProjectAccess = 0;
        public const int AllowProjectRead = 1;
        public const int AllowProjectUpdate = 2;

        public const int UserStatusException = 0;
        public const int UserStatusAvailable = 1;
        public const int UserStatusDisable = 2;

        public const int FTANodeTypeRoot= 1;
        public const int FTANodeTypeBrand= 2;
        public const int FTANodeTypeLeaf= 3;

        public const int FTANodeGateTypeAnd = 1;
        public const int FTANodeGateTypeOr = 2;
        public const int FTANodeGateTypeXor = 3;

        public static string TemplateRootFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["TemplateRootFolder"];
            }
        }

        public static string ProjectRootFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["ProjectRootFolder"];
            }
        }
    }
}