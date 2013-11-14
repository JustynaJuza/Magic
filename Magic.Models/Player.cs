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

        public string Id { get; private set; }
        public string Username { get; private set; }
        public string Title { get; private set; }
        public string Image { get; private set; }
        public virtual CardDeckViewModel Deck { get; set; }
        public int HPTotal { get; set; }
        public int HPCurrent { get; set; }
        public int CardsInLibraryTotal { get; set; }
        public List<CardViewModel> Library { get; set; }
        public List<CardViewModel> CardsInHand { get; set; }

        // Constructor.
        public Player(ApplicationUser user)
        {
            Id = user.Id;
            Username = user.UserName;
            Title = user.Title;
            Image = user.Image;
            HPTotal = defaultHP;
            HPCurrent = defaultHP;
            Library = new List<CardViewModel>();
        }
        // Constructor.
        public Player(ApplicationUser user, CardDeck deck)
        {
            Id = user.Id;
            Username = user.UserName;
            Title = user.Title;
            Image = user.Image;
            Deck = (CardDeckViewModel) deck.GetViewModel();
            HPTotal = defaultHP;
            HPCurrent = defaultHP;
            Library = new List<CardViewModel>();

            SelectDeck((CardDeckViewModel) deck.GetViewModel());
            DrawHand();
        }

        #region HELPERS
        public void SelectDeck(CardDeckViewModel deck)
        {
            Deck = (CardDeckViewModel) deck.GetViewModel();
            foreach (var card in Deck.Cards)
                Library.Add((CardViewModel) card.GetViewModel());
            CardsInLibraryTotal = Library.Count;
        }

        public void DrawHand(int cards = 7)
        {
            // Shuffle library.
            Random random = new Random();
            ShuffleLibrary(random);

            CardsInHand = Library.Take(cards).ToList();
            Library.RemoveRange(0, cards);
        }

        public bool DrawCard(int index = 0)
        {
            if (Library.Count == 0)
            {
                return false;
            }

            this.CardsInHand.Add(Library.ElementAt(index));
            Library.RemoveAt(index);
            return true;
        }

        public bool PlayCard(CardViewModel card)
        {
            this.CardsInHand.Remove(card);
            card.Play();
            return true;
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
        #endregion HELPERS
    }
}