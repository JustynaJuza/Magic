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
        public virtual IList<CardColor> CardColors { get; set; }
        public IList<int> CardCostPerColor { get; set; }

        [NotMapped]
        public virtual Card target { get; set; }

        public void Effect()
        {

        }
    }
}