using Juza.Magic.Models.DataContext;

namespace Juza.Magic.Models
{
    public class CardPlayerService
    {
        private readonly IDbContext _context;

        public CardPlayerService(IDbContext context)
        {
            _context = context;
        }


        //public PlayerCard TakeCard()
        //{
        //    return Deck.DrawCard();
        //}

        //#region DECK MANAGEMENT
        //public void SelectDeck(CardDeck deck)
        //{
        //    Deck = deck.ToViewModel<CardDeck, CardDeckViewModel>();
        //    foreach (var card in Deck.Cards)
        //        Library.Add(card.ToViewModel<Card, CardViewModel>());
        //    CardsInLibraryTotal = Library.Count;
        //}

        //// Fisher-Yates shuffle algorithm.
        //public void ShuffleLibrary(Random random)
        //{
        //    var deck = Library.ToArray();
        //    for (int i = deck.Length - 1; i >= 0; i--)
        //    {
        //        // Swap with random element.
        //        int swapIndex = random.Next(i + 1);
        //        //yield return deck[swapIndex];
        //        deck[swapIndex] = deck[i];
        //    }
        //    Library = deck.ToList();
        //}

        //public void DrawHand(int cards = 7)
        //{
        //    // Shuffle library.
        //    Random random = new Random();
        //    ShuffleLibrary(random);

        //    Hand = Library.Take(cards).ToList();
        //    Library.RemoveRange(0, cards);
        //}

        //public bool DrawCard(int index = 0)
        //{
        //    if (Library.Count == 0)
        //    {
        //        return false;
        //    }

        //    Hand.Add(Library.ElementAt(index));
        //    Library.RemoveAt(index);
        //    return true;
        //}
        //#endregion DECK MANAGEMENT

        //#region CARD MANAGEMENT
        //public bool PlayCard(CardViewModel card)
        //{
        //    if (Hand.Any(c => c.Id == card.Id))
        //    {
        //        Hand.Remove(card);
        //        card.Play();
        //        return true;
        //    }
        //    return false;
        //}

        //public bool PutCardToGraveyard(CardViewModel card, List<CardViewModel> targetCollection)
        //{
        //    if (targetCollection.Exists(c => c.Id == card.Id))
        //    {
        //        targetCollection.Remove(card);
        //        Graveyard.Add(card);
        //        return true;
        //    }
        //    return false;
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

        //public bool ExileCard(CardViewModel card, List<CardViewModel> targetCollection)
        //{
        //    if (targetCollection.Exists(c => c.Id == card.Id))
        //    {
        //        Exiled.Add(card);
        //        targetCollection.Remove(card);
        //        return true;
        //    }
        //    return false;
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
        //#endregion CARD MANAGEMENT
    }
}