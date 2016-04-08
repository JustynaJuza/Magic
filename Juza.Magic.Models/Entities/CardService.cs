using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Enums;

namespace Juza.Magic.Models.Entities
{
    public interface ICardService
    {
        void AssignTypes(Card card);
        void DecodeManaCost(Card card);
    }

    public class CardService : ICardService
    {
        private readonly IDbContext _context;

        public CardService(IDbContext context)
        {
            _context = context;
        }

        public void AssignTypes(Card card)
        {
            foreach (var typeName in card.TypeNames)
            {
                var type = _context.Query<CardType>().FirstOrDefault(t => t.Name == typeName);
                if (type != null)
                {
                    card.Types.Add(type);
                }
                    //else if (CardType.IsSuperType(typeName))
                    //{
                    //    types.Add(new CardSuperType { Name = typeName });
                    //}
                else if (CardType.IsMainType(typeName))
                {
                    // Possibly old card with obsolete type, replace with newest value.
                    var name = Enum.Parse(typeof(MainType), typeName).ToString();
                    type = _context.Query<CardType>().FirstOrDefault(t => t.Name == name);
                    card.Types.Add(type ?? new CardMainType { Name = typeName });
                }
                else
                {
                    card.Types.Add(new CardSubType { Name = typeName });
                }
            }
        }

        public void DecodeManaCost(Card card)
        {
            var manaCost = new List<CardManaCost>();

            // Check for initial colorless mana cost.
            var colorlessCost = Regex.Match(card.ManaCode, "^[0-9]+").Value;
            if (!string.IsNullOrWhiteSpace(colorlessCost))
            {
                manaCost.Add(new CardManaCost
                {
                    ColorId = 1,
                    Cost = int.Parse(colorlessCost)
                });
            }

            // Parse rest of the mana code.
            for (var i = colorlessCost.Length; i < card.ManaCode.Length; i++)
            {
                var character = card.ManaCode[i].ToString();
                if (character == "X")
                {
                    i += 1;
                    character = card.ManaCode[i].ToString();

                    var colorId = Enum.IsDefined(typeof(Color), character)
                        ? (int)Enum.Parse(typeof(Color), character)
                        : 0;

                    if (colorId == 0)
                    {
                        throw new InvalidCastException(
                            "The Color enum for mana colors does not define a color with the alias " + character);
                    }

                    manaCost.Add(new CardManaCost
                    {
                        ColorId = colorId,
                        HasVariableCost = true
                    });
                }
                else if (character != "{")
                {
                    var colorId = Enum.IsDefined(typeof(Color), character)
                        ? (int)Enum.Parse(typeof(Color), character)
                        : 0;

                    if (colorId == 0)
                    {
                        throw new InvalidCastException(
                            "The Color enum for mana colors does not define a color with the alias " + character);
                    }

                    var existingManaColor = manaCost.FirstOrDefault(c => c.ColorId == colorId && !c.IsHybrid);
                    if (existingManaColor != null)
                    {
                        existingManaColor.Cost += 1;
                    }
                    else
                    {
                        manaCost.Add(new CardManaCost
                        {
                            ColorId = colorId,
                            Cost = 1
                        });
                    }
                }
                else
                {
                    character = card.ManaCode[i + 1].ToString();
                    var hybridCharacter = card.ManaCode[i + 3].ToString();
                    i += 4;

                    var colorId = Enum.IsDefined(typeof(Color), character)
                        ? (int)Enum.Parse(typeof(Color), character)
                        : 0;

                    var hybridColorId = Enum.IsDefined(typeof(Color), hybridCharacter)
                        ? (int)Enum.Parse(typeof(Color), hybridCharacter)
                        : 0;

                    if (colorId == 0 || hybridColorId == 0)
                    {
                        throw new InvalidCastException(
                            "The Color enum for mana colors does not define a color with the alias " + character);
                    }

                    var existingManaColor =
                        manaCost.FirstOrDefault(
                            c => c.IsHybrid && ((HybridManaCost)c).HasColors(colorId, hybridColorId));
                    if (existingManaColor != null)
                    {
                        existingManaColor.Cost += 1;
                    }
                    else
                    {
                        manaCost.Add(new HybridManaCost()
                        {
                            ColorId = colorId,
                            HybridColorId = hybridColorId,
                            Cost = 1
                        });
                    }
                }
            }

            //var mana = new List<CardManaCost>
            //{
            //    new CardManaCost
            //    {
            //        ColorId = _context.ManaColors.First(c => c.Name == "Black").Id,
            //        Cost = manaCode.Count(c => c == 'B')
            //    },
            //    new CardManaCost
            //    {
            //        ColorId = _context.ManaColors.First(c => c.Name == "Blue").Id,
            //        Cost = manaCode.Count(c => c == 'U')
            //    },
            //    new CardManaCost
            //    {
            //        ColorId = _context.ManaColors.First(c => c.Name == "Green").Id,
            //        Cost = manaCode.Count(c => c == 'G')
            //    },
            //    new CardManaCost
            //    {
            //        ColorId = _context.ManaColors.First(c => c.Name == "Red").Id,
            //        Cost = manaCode.Count(c => c == 'R')
            //    },
            //    new CardManaCost
            //    {
            //        ColorId = _context.ManaColors.First(c => c.Name == "White").Id,
            //        Cost = manaCode.Count(c => c == 'W')
            //    }
            //};

            //int colorless;
            //if (int.TryParse(Regex.Match(manaCode, "[0-9]*").Value, out colorless))
            //{
            //    mana.Add(new CardManaCost
            //    {
            //        ColorId = _context.ManaColors.First(c => c.Name == "Colorless").Id,
            //        Cost = colorless
            //    });
            //}

            card.Colors = manaCost;

            //Colors = mana.Where(m => m.Cost > 0).ToList();
        }
    }
}