using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class PlayerGameStatus : AbstractExtensions
    {
        [Key, Column(Order = 0)]
        public string GameId { get; set; }
        [Key, Column(Order = 1)]
        public string UserId { get; set; }
        public virtual Game Game { get; set; }
        //public virtual Player Player { get; set; }
        public virtual ApplicationUser User { get; set; }
        public GameStatus? Status { get; set; }
    }
}