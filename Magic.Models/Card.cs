using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
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
        [Required(ErrorMessage = "The card must have a name.")]
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
            //SetId = jCardObject.Value<string>("cardSetId");
            SetNumber = jCardObject.Value<int>("setNumber");
            DateReleased = jCardObject.Value<DateTime>("releasedAt");
            Artist = jCardObject.Value<string>("artist");
            Rarity = (Rarity)Enum.Parse(typeof(Rarity), jCardObject.Value<string>("rarity"), true);
            ConvertedManaCost = jCardObject.Value<int>("convertedManaCost");
            Description = jCardObject.Value<string>("description");
            Flavor = jCardObject.Value<string>("flavor");

            var types = jCardObject.Value<string>("type").Split(' ');
            Types = AssignTypes(jCardObject.Value<string>("subType").Split(' ').Concat(types));
            Colors = DecodeManaCost(jCardObject.Value<string>("manaCost"));
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
                        var name = Enum.Parse(typeof (MainType), typeName).ToString();
                        type = context.CardTypes.FirstOrDefault(t => t.Name == name);
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

        public IList<CardManaCost> DecodeManaCost(string manaCode)
        {
            var mana = new List<CardManaCost>();
            var codes = new[] { "B", "U", "G", "R", "W" };
            using (var context = new MagicDbContext())
            {
                // TODO: Fix this to enable hybrids.
                foreach (Match hybrid in Regex.Matches(manaCode, @"(./.)"))
                {
                    var color = hybrid.Value.Substring(0, 1);
                    var hybridColor = hybrid.Value.Substring(2, 1);
                    mana.Add(new HybridManaCost
                    {
                        Color = context.ManaColors.FirstOrDefault(c => c.Name == Enum.Parse(typeof (Color), color).ToString()),
                        HybridColor = context.ManaColors.FirstOrDefault(c => c.Name == Enum.Parse(typeof (Color), color).ToString()),
                        Cost = 1
                    });
                }

                mana.Add(new CardManaCost
                    {
                        Color = context.ManaColors.FirstOrDefault(c => c.Name == "Black"),
                        Cost = manaCode.Count(c => c == 'B')
                    });
                    mana.Add(new CardManaCost
                    {
                        Color = context.ManaColors.FirstOrDefault(c => c.Name == "Blue"),
                        Cost = manaCode.Count(c => c == 'U')
                    });
                    mana.Add(new CardManaCost
                    {
                        Color = context.ManaColors.FirstOrDefault(c => c.Name == "Green"),
                        Cost = manaCode.Count(c => c == 'G')
                    });
                    mana.Add(new CardManaCost
                    {
                        Color = context.ManaColors.FirstOrDefault(c => c.Name == "Red"),
                        Cost = manaCode.Count(c => c == 'R')
                    });
                    mana.Add(new CardManaCost
                    {
                        Color = context.ManaColors.FirstOrDefault(c => c.Name == "White"),
                        Cost = manaCode.Count(c => c == 'W')
                    });
                    int colorless;
                    if (int.TryParse(Regex.Match(manaCode, "[0-9]*").Value, out colorless))
                    {
                        mana.Add(new CardManaCost
                        {
                            Color = context.ManaColors.FirstOrDefault(c => c.Name == "Colorless"),
                            Cost = colorless
                        });
                    }
                };
            return mana.Where(m => m.Cost > 0).ToList();
        }

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