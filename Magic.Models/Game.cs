using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Game : AbstractExtensions
    {
        public int Id { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateEnded { get; set; }
        public virtual List<PlayerGameStatus> Players { get; set; }
    }

    [ComplexType]
    public class PlayerGameStatus : AbstractExtensions
    {
        public virtual ApplicationUser Player { get; set; }
        public GameStatus Status { get; set; }
        public virtual CardDeck Deck { get; set; }
        //public virtual GameStatistics Statistics { get; set; }
    }

    [ComplexType]
    public class GameStatistics : AbstractExtensions
    {
        public int CardsInLibrary { get; set; }
        public int CardsInGraveyard { get; set; }
        public int CardsExiled { get; set; }
        public int CardsInHand { get; set; }
        public int CardsPlayed { get; set; }
        public int HP { get; set; }
    }
}