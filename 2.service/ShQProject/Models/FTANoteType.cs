﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTANoteType
    {
        [Key]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}