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
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual List<CardColor> CardColors { get; set; }
        public List<int> CardCostPerColor { get; set; }

        [NotMapped]
        public virtual Card target { get; set; }

        public void Effect()
        {

        }
    }
}