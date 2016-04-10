using Juza.Magic.Models.Extensions;

namespace Juza.Magic.Models.Entities
{
    public class CardAvailableAbility : AbstractExtensions
    {
        public int CardId { get; set; }
        public int AbilityId { get; set; }

        public CardAbility Ability { get; set; }
        public Card Card { get; set; }
    }
}