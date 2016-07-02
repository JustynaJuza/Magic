using Juza.Magic.Models.Enums;
using Juza.Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Juza.Magic.Models.Entities
{
    public class Card
    {
        [NotMapped]
        public IEnumerable<string> TypeNames { get; set; }
        [NotMapped]
        public string ManaCode { get; set; }

        public int Id { get; set; }
        public int MultiverseId { get; set; }
        [Required(ErrorMessage = "The card must have a name.")]
        public string Name { get; set; }
        public string SetId { get; set; }
        public CardSet Set { get; set; }
        public int SetNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateReleased { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }
        [DataType(DataType.ImageUrl)]
        public string ImagePreview { get; set; }
        public string Artist { get; set; }
        public bool IsPermanent { get; set; }
        public bool IsTapped { get; set; }
        public Rarity Rarity { get; set; }
        [Display(Name = "Mana")]
        public int ConvertedManaCost { get; set; }
        public string Description { get; set; }
        public string Flavor { get; set; }
        public virtual IList<CardType> Types { get; set; }
        public virtual IList<CardManaCost> Colors { get; set; }
        public virtual IList<CardAvailableAbility> Abilities { get; set; }

        public Card()
        {
            //Image = 
            IsTapped = false;
            Types = new List<CardType>();
            Colors = new List<CardManaCost>();
            Abilities = new List<CardAvailableAbility>();
        }

        //public Card(string json)
        //{
        //    var serializer = JavaScriptSerializer
        //}

        //public Card(JObject jObject)
        //    : this()
        //{
        //    MultiverseId = jObject.Value<int>("id");
        //    Name = jObject.Value<string>("name");
        //    SetId = jObject.Value<string>("cardSetId");
        //    SetNumber = jObject.Value<int>("setNumber");
        //    DateReleased = jObject.Value<DateTime>("releasedAt");
        //    Artist = jObject.Value<string>("artist");

        //    var rarity = jObject.Value<string>("rarity").Replace(" ", "");
        //    Rarity = Enum.IsDefined(typeof(Rarity), rarity) ? (Rarity)Enum.Parse(typeof(Rarity), rarity, true) : Rarity.Common;

        //    ConvertedManaCost = jObject.Value<int>("convertedManaCost");
        //    Description = jObject.Value<string>("description");
        //    Flavor = jObject.Value<string>("flavor").Replace("â€”", " —").Trim();
        //    Id = Name.ToLower().Replace("\\s+", "_").Replace("[^a-z0-9]*", "");

        //    var types = jObject.Value<string>("type").Replace("Summon", "Creature").Replace("Interrupt", "Instant").Split(' ');
        //    TypeNames = jObject.Property("subType").Value.HasValues ? jObject.Value<string>("subType").Split(' ').Concat(types) : types;
        //    ManaCode = jObject.Value<string>("manaCost");
        //}

        public bool Play()
        {
            throw new NotImplementedException();
        }

        public void UseAbility(CardAbility ability)
        {
            ability.Effect();
        }
    }

    public class CreatureCard : Card
    {
        public bool IsToken { get; set; }
        public int Power { get; set; }
        public int Toughness { get; set; }

        public CreatureCard() { }
        //public CreatureCard(JObject jObject)
        //    : base(jObject)
        //{
        //    Power = jObject.Value<int>("power");
        //    Toughness = jObject.Value<int>("toughness");
        //    IsToken = jObject.Value<bool>("token");
        //}
    }

    public class PlaneswalkerCard : Card
    {
        public int Loyalty { get; set; }

        public PlaneswalkerCard() { }
        //public PlaneswalkerCard(JObject jObject)
        //    : base(jObject)
        //{
        //    Loyalty = jObject.Value<int>("loyalty");
        //}
    }

    public class CardViewModel : ICard, IViewModel<Card>
    {
        public string Id { get { return this.Id; } }
        public string CasterId { get; set; }
        public string Name { get; set; }
        public bool Tapped { get; set; }
        public bool Permanent { get; set; }
        public PlayerCardLocation Location { get; set; }
        public IList<int> CostPerColor { get; set; }
        public virtual IList<ManaColor> Colors { get; set; }
        public virtual IList<CardAbility> Abilities { get; set; }

        public void Tap()
        {
            Tapped = true;
        }
        public void UnTap()
        {
            Tapped = false;
        }
        public bool Play() { return true; }

        public CardViewModel()
        {

        }
    }
}