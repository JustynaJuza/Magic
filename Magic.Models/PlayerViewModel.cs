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
    public class PlayerViewModel : AbstractExtensions, IViewModel
    {
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

        public PlayerViewModel (ApplicationUser user, CardDeck deck)
        {
            Id = user.Id;
            Username = user.UserName;
            Title = user.Title;
            Image = user.Image;
            Deck = (CardDeckViewModel) deck.getViewModel();
            HPTotal = 20;
            HPCurrent = 20;
            Library = Deck.Cards;
            CardsInLibraryTotal = Library.Count;

            // Shuffle library.
            Random random = new Random();
            ShuffleLibrary(random);

            CardsInHand = Library.Take(7).ToList();
            Library.RemoveRange(0, 7);
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
    }
}