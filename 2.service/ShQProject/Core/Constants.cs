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

        public static Guid DefaultWorkProjectTemplateId = new Guid("1b2cd8ab-6d6c-4a05-931b-e40607bd8b19");

        public const int DefaultFileLevel = 0;

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

        public static int GetPathId(string path)
        {
            string id = System.IO.Path.GetExtension(path).Replace(".", "");
            int pathId;
            if(int.TryParse(id,out pathId) ==true)
            {
                return pathId;
            }

            return 0;
        }
    }
}