using Juza.Magic.Models.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.Entities
{
    public class PlayerCardDeck
    {
        public int DeckId { get; set; }
        public int UserId { get; set; }
        public string GameId { get; set; }
        //public int CardsTotal { get; set; }
        public int CardsPlayed { get; set; }

        public virtual Player Player { get; set; }
        public virtual CardDeck Deck { get; set; }
        public virtual IList<PlayerCard> Cards { get; set; }

        public PlayerCardDeck()
        {
            CardsPlayed = 0;
            Cards = new List<PlayerCard>();
        }

        #region DECK MANAGEMENT

        //public void Shuffle(Random rng)
        //{
        //    for (var i = Cards.Count - 1; i >= 0; i--)
        //    {
        //        var swapIndex = rng.Next(i + 1);
        //        Cards.ElementAt(i).Index = swapIndex;
        //        Cards.ElementAt(swapIndex).Index = i;
        //    }

        //    Cards = Cards.OrderBy(c => c.Index).ToList();
        //}

        //public void DrawHand(int cards = 7)
        //{
        //    using (var context = new MagicDbContext())
        //    {
        //        Shuffle(new Random());

        //        for (var i = 0; i < cards; i++)
        //        {
        //            Cards.ElementAt(i).Location = PlayerCardLocation.Hand;
        //        }
        //        context.InsertOrUpdate(this);
        //    }
        //}

        //public PlayerCard DrawCard()
        //{
        //    return Cards.FirstOrDefault(c => c.Location == PlayerCardLocation.Library);
        //}

        //public PlayerCard GetCard(int index)
        //{
        //    var card = Cards.ElementAt(index);
        //    return card.Location == PlayerCardLocation.Library ? card : null;
        //}

        //public bool PlayCard(int index)
        //{
        //    return Cards.ElementAt(index).Card.Play();
        //}

        //public void PutCardToGraveyard(int index)
        //{
        //    using (var context = new MagicDbContext())
        //    {
        //        Cards.ElementAt(index).Location = PlayerCardLocation.Graveyard;
        //        context.InsertOrUpdate(this);
        //    }
        //}

        //public void AddCard(string cardId, PlayerCardLocation location)
        //{
        //    using (var context = new MagicDbContext())
        //    {
        //        var card = context.Read<Card>().Find(cardId);
        //        Cards.Add(new PlayerCard
        //        {
        //            Card = card,
        //            GameId = GameId,
        //            UserId = UserId,
        //            Index = Cards.Count + 1,
        //            Location = location,
        //        });
        //        context.InsertOrUpdate(this);
        //    }
        //}

        //public bool RestoreCardFromGraveyard(CardViewModel card, List<CardViewModel> targetCollection, bool copy = false)
        //{
        //    if (Graveyard.Any(c => c.Id == card.Id))
        //    {
        //        targetCollection.Add(card);
        //        if (!copy)
        //        {
        //            Graveyard.Remove(card);
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        //public void ExileCard(int index)
        //{
        //    using (var context = new MagicDbContext())
        //    {
        //        Cards.ElementAt(index).Location = PlayerCardLocation.Exiled;
        //        context.InsertOrUpdate(this);
        //    }
        //}

        //public bool RestoreCardFromExile(CardViewModel card, List<CardViewModel> targetCollection, bool copy = false)
        //{
        //    if (Exiled.Any(c => c.Id == card.Id))
        //    {
        //        targetCollection.Add(card);
        //        if (!copy)
        //        {
        //            Exiled.Remove(card);
        //        }
        //        return true;
        //    }
        //    return false;
        //}
        #endregion DECK MANAGEMENT
    }
}