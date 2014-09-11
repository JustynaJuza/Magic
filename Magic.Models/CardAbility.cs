using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardAbility : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<CardAvailableAbility> Cards { get; set; }

        [NotMapped]
        public virtual Card Target { get; set; }

        public void Effect()
        {

        }
    }
}