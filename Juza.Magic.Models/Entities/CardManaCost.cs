namespace Juza.Magic.Models.Entities
{
    public class CardManaCost
    {
        public int CardId { get; set; }
        public int ColorId { get; set; }

        public Card Card { get; set; }
        public ManaColor Color { get; set; }
        public int Cost { get; set; }

        public bool HasVariableCost { get; set; }

        public bool IsHybrid
        {
            get
            {
                return this is HybridManaCost;
            }
        }
    }

    public class HybridManaCost : CardManaCost
    {
        public int HybridColorId { get; set; }
        public ManaColor HybridColor { get; set; }

        public bool HasColors(int colorId, int hybridColorId)
        {
            return (HybridColorId == hybridColorId && ColorId == colorId) ||
                   (HybridColorId == colorId && ColorId == hybridColorId);
        }
    }
}