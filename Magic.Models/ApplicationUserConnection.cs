﻿using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ApplicationUserConnection : AbstractExtensions
    {
        public string Id { get; set; }
        public virtual ApplicationUser User { get; set; }
        //public virtual IList<ChatRoom> ChatRooms { get; set; }
    }

    public class ApplicationUserGameConnection : ApplicationUserConnection
    {
        public virtual Game Game { get; set; }
    }
}