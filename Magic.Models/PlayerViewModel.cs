using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class PlayerViewModel
    {
        public int HPTotal { get; set; }
        public int HPCurrent { get; set; }
        public int CardsInLibraryTotal { get; set; }
        public int CardsInLibraryCurrent { get; set; }
        public virtual List<Card> CardsInHand { get; set; }
    }
}