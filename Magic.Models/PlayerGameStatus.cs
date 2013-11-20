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
        public int GameId { get; set; }
        [Key, Column(Order = 1)]
        public string PlayerId { get; set; }
        public virtual Game Game { get; set; }
        public virtual ApplicationUser Player { get; set; }
        public GameStatus Status { get; set; }
        public virtual CardDeck Deck { get; set; }
        //public virtual Player Player { get; set; }
        //public virtual PlayerState LastSavedState { get; set; }
    }

    [ComplexType]
    public class PlayerState : AbstractExtensions
    {
        public virtual IList<Card> Library { get; set; }
        public virtual IList<Card> Graveyard { get; set; }
        public virtual IList<Card> Exiled { get; set; }
        public virtual IList<Card> Hand { get; set; }
        public virtual IList<Card> Battlefield { get; set; }
        public int CardsPlayed { get; set; }
        public int HPCurrent { get; set; }
        public int HPTotal { get; set; }
    }
}