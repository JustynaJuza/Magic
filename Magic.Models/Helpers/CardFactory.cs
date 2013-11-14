using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models.Helpers
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