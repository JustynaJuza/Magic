using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Player_GameStatus : AbstractExtensions
    {
        public string GameId { get; set; }
        public string UserId { get; set; }
        public virtual Game Game { get; set; }
        //public virtual Player Player { get; set; }
        public virtual ApplicationUser User { get; set; }
        public GameStatus? Status { get; set; }
    }
}