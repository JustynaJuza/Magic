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
        private IEnumerable<string> typeNames;
        private string manaCode;

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

        public Card(JObject jObject)
            : this()
        {
            MultiverseId = jObject.Value<int>("id");
            Name = jObject.Value<string>("name");
            //SetId = jObject.Value<string>("cardSetId");
            SetNumber = jObject.Value<int>("setNumber");
            DateReleased = jObject.Value<DateTime>("releasedAt");
            Artist = jObject.Value<string>("artist");
            Rarity = (Rarity)Enum.Parse(typeof(Rarity), jObject.Value<string>("rarity"), true);
            ConvertedManaCost = jObject.Value<int>("convertedManaCost");
            Description = jObject.Value<string>("description");
            Flavor = jObject.Value<string>("flavor").Replace("â€”", " —").Trim();
            Id = Name.ToLower().Replace(" ", "_").Replace("[^a-z0-9]*", "");

            var types = jObject.Value<string>("type").Replace("Summon", "Creature").Replace("Interrupt", "Instant").Split(' ');
            typeNames = jObject.Value<string>("subType").Split(' ').Concat(types);
            manaCode = jObject.Value<string>("manaCost");
        }

        public void AssignTypes(MagicDbContext context)
        {
            foreach (var typeName in typeNames)
            {
                var type = context.CardTypes.FirstOrDefault(t => t.Name == typeName);
                if (type != null)
                {
                    Types.Add(type);
                }
                //else if (CardType.IsSuperType(typeName))
                //{
                //    types.Add(new CardSuperType { Name = typeName });
                //}
                else if (CardType.IsMainType(typeName))
                {
                    // Possibly old card with obsolete type, replace with newest value.
                    var name = Enum.Parse(typeof(MainType), typeName).ToString();
                    type = context.CardTypes.FirstOrDefault(t => t.Name == name);
                    Types.Add(type ?? new CardMainType { Name = typeName });
                }
                else
                {
                    Types.Add(new CardSubType { Name = typeName });
                }
            }
        }

        public void DecodeManaCost(MagicDbContext context)
        {
            var mana = new List<CardManaCost>
            {
                new CardManaCost
                {
                    ColorId = context.ManaColors.First(c => c.Name == "Black").Id,
                    Cost = manaCode.Count(c => c == 'B')
                },
                new CardManaCost
                {
                    ColorId = context.ManaColors.First(c => c.Name == "Blue").Id,
                    Cost = manaCode.Count(c => c == 'U')
                },
                new CardManaCost
                {
                    ColorId = context.ManaColors.First(c => c.Name == "Green").Id,
                    Cost = manaCode.Count(c => c == 'G')
                },
                new CardManaCost
                {
                    ColorId = context.ManaColors.First(c => c.Name == "Red").Id,
                    Cost = manaCode.Count(c => c == 'R')
                },
                new CardManaCost
                {
                    ColorId = context.ManaColors.First(c => c.Name == "White").Id,
                    Cost = manaCode.Count(c => c == 'W')
                }
            };

            int colorless;
            if (int.TryParse(Regex.Match(manaCode, "[0-9]*").Value, out colorless))
            {
                mana.Add(new CardManaCost
                {
                    ColorId = context.ManaColors.First(c => c.Name == "Colorless").Id,
                    Cost = colorless
                });
            }

            // TODO: Fix this to enable hybrids.
            //var codes = new[] { "B", "U", "G", "R", "W" };
            //foreach (Match hybrid in Regex.Matches(manaCode, @"(./.)"))
            //{
            //    var color = hybrid.Value.Substring(0, 1);
            //    var hybridColor = hybrid.Value.Substring(2, 1);
            //    mana.Add(new HybridManaCost
            //    {
            //        Color = context.ManaColors.FirstOrDefault(c => c.Name == Enum.Parse(typeof(Color), color).ToString()),
            //        HybridColor = context.ManaColors.FirstOrDefault(c => c.Name == Enum.Parse(typeof(Color), color).ToString()),
            //        Cost = 1
            //    });
            //}

            Colors = mana.Where(m => m.Cost > 0).ToList();
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
        public CreatureCard(JObject jObject)
            : base(jObject)
        {
            Power = jObject.Value<int>("power");
            Toughness = jObject.Value<int>("toughness");
            IsToken = jObject.Value<bool>("token");
        }
    }

    public class PlaneswalkerCard : Card
    {
        public int Loyalty { get; set; }

        public PlaneswalkerCard() { }
        public PlaneswalkerCard(JObject jObject)
            : base(jObject)
        {
            Loyalty = jObject.Value<int>("loyalty");
        }
    }

    public class CardViewModel : AbstractExtensions, ICard, IViewModel
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