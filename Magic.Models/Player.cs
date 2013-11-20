using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Player : AbstractExtensions, IViewModel
    {
        private int defaultHP = 20;

        public virtual ApplicationUser User { get; set; }
        public virtual CardDeckViewModel Deck { get; set; }
        public int HPTotal { get; set; }
        public int HPCurrent { get; set; }
        public int CardsInLibraryTotal { get; set; }
        public List<CardViewModel> Library { get; set; }
        public List<CardViewModel> Graveyard { get; set; }
        public List<CardViewModel> Hand { get; set; }
        public List<CardViewModel> Exiled { get; set; }
        public List<CardViewModel> Battlefield { get; set; }

        // Constructor.
        public Player(ApplicationUser user)
        {
            User = user;
            HPTotal = defaultHP;
            HPCurrent = defaultHP;
            Library = new List<CardViewModel>();
            Graveyard = new List<CardViewModel>();
            Exiled = new List<CardViewModel>();
            Battlefield = new List<CardViewModel>();
        }
        // Constructor with deck.
        public Player(ApplicationUser user, CardDeck deck)
        {
            User = user;
            Deck = (CardDeckViewModel) deck.GetViewModel();
            HPTotal = defaultHP;
            HPCurrent = defaultHP;
            Library = new List<CardViewModel>();
            Graveyard = new List<CardViewModel>();
            Exiled = new List<CardViewModel>();
            Battlefield = new List<CardViewModel>();

            SelectDeck((CardDeckViewModel) deck.GetViewModel());
            DrawHand();
        }

        #region DECK MANAGEMENT
        public void SelectDeck(CardDeckViewModel deck)
        {
            Deck = (CardDeckViewModel) deck.GetViewModel();
            foreach (var card in Deck.Cards)
                Library.Add((CardViewModel) card.GetViewModel());
            CardsInLibraryTotal = Library.Count;
        }

        // Fisher-Yates shuffle algorithm.
        public void ShuffleLibrary(Random random)
        {
            var deck = Library.ToArray();
            for (int i = deck.Length - 1; i >= 0; i--)
            {
                // Swap with random element.
                int swapIndex = random.Next(i + 1);
                //yield return deck[swapIndex];
                deck[swapIndex] = deck[i];
            }
            Library = deck.ToList();
        }

        public void DrawHand(int cards = 7)
        {
            // Shuffle library.
            Random random = new Random();
            ShuffleLibrary(random);

            Hand = Library.Take(cards).ToList();
            Library.RemoveRange(0, cards);
        }

        public bool DrawCard(int index = 0)
        {
            if (Library.Count == 0)
            {
                return false;
            }

            Hand.Add(Library.ElementAt(index));
            Library.RemoveAt(index);
            return true;
        }
        #endregion DECK MANAGEMENT

        #region CARD MANAGEMENT
        public bool PlayCard(CardViewModel card)
        {
            if (Hand.Any(c => c.Id == card.Id))
            {
                Hand.Remove(card);
                card.Play();
                return true;
            }
            return false;
        }

        public bool PutCardToGraveyard(CardViewModel card, List<CardViewModel> targetCollection)
        {
            if (targetCollection.Exists(c => c.Id == card.Id))
            {
                targetCollection.Remove(card);
                Graveyard.Add(card);
                return true;
            }
            return false;
        }

        public bool RestoreCardFromGraveyard(CardViewModel card, List<CardViewModel> targetCollection, bool copy = false)
        {
            if (Graveyard.Any(c => c.Id == card.Id))
            {
                targetCollection.Add(card);
                if (!copy)
                {
                    Graveyard.Remove(card);
                }
                return true;
            }
            return false;
        }

        public bool ExileCard(CardViewModel card, List<CardViewModel> targetCollection)
        {
            if (targetCollection.Exists(c => c.Id == card.Id))
            {
                Exiled.Add(card);
                targetCollection.Remove(card);
                return true;
            }
            return false;
        }

        public bool RestoreCardFromExile(CardViewModel card, List<CardViewModel> targetCollection, bool copy = false)
        {
            if (Exiled.Any(c => c.Id == card.Id))
            {
                targetCollection.Add(card);
                if (!copy)
                {
                    Exiled.Remove(card);
                }
                return true;
            }
            return false;
        }
        #endregion CARD MANAGEMENT
    }
}