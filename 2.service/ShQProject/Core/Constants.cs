using Dxc.Shq.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Core
{
    public class FTANodeGateTypeConstant
    {
        public FTANoteGateType FTANoteGateType { get; set; }

        public string Key;
    }

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
    }
}