using Magic.Models.Interfaces;

namespace Magic.Models.Extensions
{
    public static class CardFactory
    {
        public static IViewModel CreateCardViewModel(MainType selectedType)
        {
            switch (selectedType)
            {
                case MainType.Creature:
                    return new CreatureCard().GetViewModel();
                case MainType.Instant:
                    return new CreatureCard().GetViewModel();
                default: 
                    return new Card().GetViewModel();
            }
        }
    }
}