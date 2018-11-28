using System;
using System.Collections.Generic;
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
    }
}