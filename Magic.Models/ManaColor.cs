using Magic.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{    
    public class ManaColor : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
    }
}