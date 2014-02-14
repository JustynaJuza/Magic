using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ApplicationUserConnection : AbstractExtensions
    {
        public string Id { get; set; }
        public virtual Game Game { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}