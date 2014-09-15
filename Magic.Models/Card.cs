using System.ComponentModel;
using System.Linq;
using Magic.Models.DataContext;
using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magic.Models
{
    public class Card : AbstractExtensions
    {
        public string Id { get; set; }
        public int MultiverseId { get; set; }
        [Required]
        public string Name { get; set; }
        public string SetId { get; set; }
        public CardSet Set { get; set; }
        public int SetNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateReleased { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }
        public string Artist { get; set; }
        public bool IsPermanent { get; set; }
        public bool IsTapped { get; set; }
        public Rarity Rarity { get; set; }
        public int ConvertedManaCost { get; set; }
        public string Description { get; set; }
        public string Flavor { get; set; }
        public virtual IList<CardType> Types { get; set; }
        public virtual IList<CardManaCost> Colors { get; set; }
        public virtual IList<CardAvailableAbility> Abilities { get; set; }

        public Card()
        {
            IsTapped = false;
            Types = new List<CardType>();
            Colors = new List<CardManaCost>();
            Abilities = new List<CardAvailableAbility>();
        }

        public Card(JObject jCardObject) : this()
        {
            MultiverseId = jCardObject.Value<int>("id");
            Name = jCardObject.Value<string>("name");
            SetId = jCardObject.Value<string>("cardSetId");
            SetNumber = jCardObject.Value<int>("setNumber");
            DateReleased = jCardObject.Value<DateTime>("releasedAt");
            Artist = jCardObject.Value<string>("artist");
            Rarity = (Rarity)Enum.Parse(typeof(Rarity), jCardObject.Value<string>("rarity"), true);
            ConvertedManaCost = jCardObject.Value<int>("convertedManaCost");
            Description = jCardObject.Value<string>("description");
            Flavor = jCardObject.Value<string>("flavor");

            var types = jCardObject.Value<string>("type").Split(' ');
            Types = AssignTypes(jCardObject.Value<string>("subtype").Split(' ').Concat(types));
            Colors = DecodeManaCost(jCardObject.Value<string>("colors").ToCharArray());
            //Types.Add(JsonConvert.DeserializeObject<CardType>(jCardObject.Value<string>("type"), new CardConverter.CardTypeConverter()));
            //foreach (var type in jCardObject.Value<string>("type").Split(' '))
            //{
            //    Types.Add(new CardType()
            //    {
            //        Name = type
            //    });
            //}
            Id = Name.ToLower().Replace(" ", "_").Replace("[^a-z0-9]*", "");
        }

        public IList<CardType> AssignTypes(IEnumerable<string> typeNames)
        {
            var types = new List<CardType>();
            using (var context = new MagicDbContext())
            {
                foreach (var typeName in typeNames)
                {
                    var type = context.CardTypes.FirstOrDefault(t => t.Name == typeName);
                    if (type != null)
                    {
                        types.Add(type);
                    }
                    //else if (CardType.IsSuperType(typeName))
                    //{
                    //    types.Add(new CardSuperType { Name = typeName });
                    //}
                    else if (CardType.IsMainType(typeName))
                    {
                        // Possibly old card with obsolete type, replace with newest value.
                        type = context.CardTypes.FirstOrDefault(t => t.Name == Enum.Parse(typeof(CardMainType),typeName).ToString());
                        types.Add(type ?? new CardMainType { Name = typeName });
                    }
                    else
                    {
                        types.Add(new CardSubType { Name = typeName });
                    }
                }
            }
            return types;
        }

        public IList<CardManaCost> DecodeManaCost(IEnumerable<char> manaCode)
        {
            var mana = new List<CardManaCost>();
            using (var context = new MagicDbContext())
            {
                if (manaCode.Contains('{'))
                {
                }
                else
                {
                    mana.Add(new CardManaCost
                    {
                        Color = context.ManaColors.FirstOrDefault(c => c.Name == "Black"),
                        Cost = manaCode.Count(c => c == 'B')
                    });
                };
            }
            return mana;
        }
    }

    public class CreatureCard : Card
    {
        public bool IsToken { get; set; }
        public int Power { get; set; }
        public int Toughness { get; set; }

        public CreatureCard() { }
        public CreatureCard(JObject jCardObject) : base(jCardObject)
        {
            Power = jCardObject.Value<int>("power");
            Toughness = jCardObject.Value<int>("toughness");
            IsToken = jCardObject.Value<bool>("token");
        }
    }
    
    public class PlaneswalkerCard : Card
    {
        public int Loyalty { get; set; }

        public PlaneswalkerCard() { }
        public PlaneswalkerCard(JObject jCardObject) : base(jCardObject)
        {
            Loyalty = jCardObject.Value<int>("loyalty");
        }
    }

    public class CardViewModel : AbstractExtensions, ICard, IViewModel
    {
        public string Id { get {return this.Id;} }
        [Required(ErrorMessage = "The card must have a name.")]
        public string Name { get; set; }
        public bool Tapped { get; set; }
        public bool Permanent { get; set; }
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