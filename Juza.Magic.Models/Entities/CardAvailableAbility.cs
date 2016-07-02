namespace Juza.Magic.Models.Entities
{
    public class CardAvailableAbility
    {
        public int CardId { get; set; }
        public int AbilityId { get; set; }

        public CardAbility Ability { get; set; }
        public Card Card { get; set; }
    }
}