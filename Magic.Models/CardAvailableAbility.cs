using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardAvailableAbility : AbstractExtensions
    {
        public string CardId { get; set; }
        public int AbilityId { get; set; }

        public CardAbility Ability { get; set; }
        public Card Card { get; set; }
    }
}